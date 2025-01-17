using System.Collections.Generic;
using GameDevTV.Inventories;
using GameDevTV.Saving;
using GameDevTV.Utils;
using RPG.Pools;
using RPG.Core;
using RPG.Movement;
using RPG.Stats;
using UnityEngine;
using System;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction
    {

        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] WeaponConfig defaultWeapon = null;
        [SerializeField] float autoAttackRange = 4f;
        [SerializeField] bool doesCancelActions = true;

        Health target;

        Equipment equipment;
        float timeSinceLastAttack = Mathf.Infinity;
        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;

        private void Awake ()
        {

            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon> (SetupDefaultWeapon);
            equipment = GetComponent<Equipment> ();
            if (equipment)
            {
                equipment.equipmentUpdated += UpdateWeapon;
            }
        }

        private void UpdateWeapon ()
        {
            var weapon = equipment.GetItemInSlot (EquipLocation.Weapon) as WeaponConfig;

            if (weapon == null)
            {
                EquipWeapon (defaultWeapon);
            }
            else
            {
                EquipWeapon (weapon);
            }
        }

        private Weapon SetupDefaultWeapon ()
        {
            return AttachWeapon (defaultWeapon);
        }

        private void Start ()
        {
            currentWeapon.ForceInit ();
        }

        private void Update ()
        {
            timeSinceLastAttack += Time.deltaTime;

            if (target == null) return;
            //if (target.IsDead ()) return; 
            
            if (target.IsDead())
            {
                target = FindNewTargetInRange();
                
                if (target == null) 
                {
                    //Debug.Log("Target is Null");
                    return; 
                }
                //Debug.Log("New Target found. It's " + target);
            }

            if (!GetIsInRange (target.transform))
            {
                GetComponent<Mover> ().MoveTo (target.transform.position, 1f);
            }
            else
            {
                GetComponent<Mover> ().Cancel ();
                AttackBehaviour ();
            }

        }

        public void EquipWeapon (WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            currentWeapon.value = AttachWeapon (weapon);
        }

        private Weapon AttachWeapon (WeaponConfig weapon)
        {
            Animator animator = GetComponent<Animator> ();
            return weapon.Spawn (rightHandTransform, leftHandTransform, animator);
        }

        public Health GetTarget ()
        {
            return target;
        }

        public Transform GetHandTransform(bool isRightHand)
        {
            if (isRightHand)
            {
                return rightHandTransform;
            }
            else
            {
                return leftHandTransform;
            }
        }

        private void AttackBehaviour ()
        {
            Animator animator = GetComponent<Animator> ();
            transform.LookAt (target.transform);
            if (timeSinceLastAttack > timeBetweenAttacks) // if attack cooldown is up
            {
                currentWeaponConfig.GetWeaponOverrideController (animator);
                TriggerAttack (); //triggers hit event
                timeSinceLastAttack = 0;
            }
        }

        public void DialogueAttack()
        {
            target = FindNewTargetInRange();                
            if (target == null) 
            {
                return; 
            }
        }

        private Health FindNewTargetInRange()
        {
            Health bestEnemy = null;
            float bestDistance = Mathf.Infinity;
            foreach (var enemy in FindAllTargetsInRange())
            {
                float enemyDistance = Vector3.Distance(transform.position, enemy.transform.position);
                if (enemyDistance < bestDistance)
                {
                    bestEnemy = enemy;
                    bestDistance = enemyDistance;
                }
            }
            return bestEnemy;
        }

        private IEnumerable<Health> FindAllTargetsInRange()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, autoAttackRange, Vector3.up);
            //Debug.Log("RayCast in FindAllTargets hit this many: " + hits.Length);
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            foreach (var hit in hits)
            {
                Health health = hit.transform.GetComponent<Health>();
                if (health == null) continue;                
                if (!CanAttack(health.gameObject)) continue;
                yield return health;                
            }
        }

        private void TriggerAttack ()
        {
            if (this != GameObject.FindGameObjectWithTag("Player"))
            {
                Debug.Log("Triggering attack from " + this.name);
            }
            
            GetComponent<Animator> ().ResetTrigger ("stopAttack");
            GetComponent<Animator> ().SetTrigger ("attack"); //triggers animation event
        }

        // Animation Event
        void Hit ()
        {
            if (target == null) { return; }
            
            float damage = GetComponent<BaseStats> ().GetStat (Stat.Damage);
            BaseStats targetBaseStats = target.GetComponent<BaseStats>();
            if (targetBaseStats != null)
            {
                float defense = targetBaseStats.GetStat (Stat.Defense);
                damage /= 1 + defense / damage;
            }

            if (currentWeapon.value != null)
            {
                currentWeapon.value.OnHit ();
            }

            if (currentWeaponConfig.HasProjectile ())
            {
                currentWeaponConfig.LaunchProjectile (rightHandTransform, leftHandTransform, target, gameObject, damage);
            }

            else
            {
                target.TakeDamage (gameObject, damage);
            }
        }

        void Shoot ()
        {
            Hit ();
        }

        private bool GetIsInRange (Transform targetTransform)
        {
            if (Vector3.Distance (transform.position, targetTransform.position) > currentWeaponConfig.GetWeaponRange ())
            {
                Debug.Log("Target is not in range: " + targetTransform.name);
                Debug.Log("The distance is " + Vector3.Distance (transform.position, targetTransform.position));
            }
            return Vector3.Distance (transform.position, targetTransform.position) < currentWeaponConfig.GetWeaponRange ();
        }

        public bool CanAttack (GameObject combatTarget)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (combatTarget == null) { return false; }
            if (!combatTarget.GetComponent<Fighter>().enabled) {return false;}
            if (this.gameObject == player && combatTarget == player) {return false;}
            if (combatTarget == gameObject) {return false;}

            if (!GetComponent<Mover> ().CanMoveTo (combatTarget.transform.position) && !GetIsInRange (combatTarget.transform)) { return false; }
            Health targetToTest = combatTarget.GetComponent<Health> ();
            return targetToTest != null && !targetToTest.IsDead ();
        }

        public void Attack (GameObject combatTarget)
        {
            GetComponent<ActionScheduler> ().StartAction (this);
            target = combatTarget.GetComponent<Health> ();
        }

        public void Cancel ()
        {
            StopAttack ();
            target = null;
            GetComponent<Mover> ().Cancel ();
        }

    public bool GetDoesCancel()
    {
        return doesCancelActions;
    }

        private void StopAttack ()
        {
            GetComponent<Animator> ().ResetTrigger ("attack");
            GetComponent<Animator> ().SetTrigger ("stopAttack");
        }

    }
}