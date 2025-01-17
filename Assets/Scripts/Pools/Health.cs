using System;
using System.Collections;
using GameDevTV.Saving;
using GameDevTV.Utils;
using RPG.Core;
using RPG.Stats;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Pools
{
    public class Health : MonoBehaviour, ISaveable
    {
        [Range (1, 100)]
        [SerializeField] float levelUpHealthPercent = 90f;
        [SerializeField] TakeDamageEvent takeDamage;        
        public UnityEvent onDeath;

        [System.Serializable]
        public class TakeDamageEvent : UnityEvent<float> { }

        [SerializeField] Condition condition;
        [SerializeField] string[] deathAnimationParams;

        LazyValue<float> healthPoints;

        bool wasDeadLastFrame = false;
        float experienceReward = 0;

        void Awake ()
        {
            healthPoints = new LazyValue<float> (GetInitialhealth);
        }

        private float GetInitialhealth ()
        {
            return GetComponent<BaseStats> ().GetStat (Stat.Health);
        }

        private void Start ()
        {
            healthPoints.ForceInit ();
        }

        private void OnEnable ()
        {
            GetComponent<BaseStats> ().onLevelUp += RegenerateHealth;
        }

        private void OnDisable ()
        {
            GetComponent<BaseStats> ().onLevelUp -= RegenerateHealth;
        }

        public bool IsDead ()
        {
            return healthPoints.value <= 0;
        }

        public void TakeDamage (GameObject instigator, float damage)
        {
            healthPoints.value = Mathf.Max (healthPoints.value - damage, 0);

            if (IsDead())
            {
                onDeath.Invoke(); // Invokes Loot Drop and Death SFX that is set in the inspector               
                AwardExperience (instigator);                
            }
            else
            {
                takeDamage.Invoke(damage);
            }
            UpdateState ();            
        }

        

        public void Heal (float healthToRestore, bool isOverTime, bool isSmooth, float duration, float tickSpeed) 
        {
            if (isOverTime && isSmooth)
            {
                //Smooth Heal Over Time
                StartCoroutine(HealOverTime(isSmooth, healthToRestore, duration, tickSpeed));
            }
            if (isOverTime && !isSmooth)
            {
                // Ticking Heal Over Time
                StartCoroutine(HealOverTime(isSmooth, healthToRestore, duration, tickSpeed));
            }
            if (!isOverTime && !isSmooth)
            {
                //Immediate Heal
                healthPoints.value = Mathf.Min (healthPoints.value + healthToRestore, GetMaxHealthPoints ());
            }
            
            UpdateState ();
        }

        //Maybe configure this differently so the editor configuration makes more sense
        IEnumerator HealOverTime(bool isSmooth, float healthToRestore, float duration, float tickSpeed) //
        {
            float totalHealedVal = Mathf.Min(healthPoints.value + healthToRestore, GetMaxHealthPoints()); 
            float tickHealValue  = healthToRestore / duration;

            if (isSmooth)
            {
                float smoothTickSpeed = tickSpeed * Time.deltaTime;
                float preHealHealthPoints = healthPoints.value;
                float smoothDuration = duration * 40;
                float smoothTickHealValue = healthToRestore / smoothDuration;

                for (int i = 0; i < smoothDuration; i++)
                {
                    float smoothTick = healthPoints.value + smoothTickHealValue;
                    
                    if (healthPoints.value >= totalHealedVal)
                    {                    
                        yield break;                        
                    }
                    
                    

                    //This catched a case where i rises above the smooth duration... Should never be called
                    if (i >= smoothDuration && healthPoints.value < totalHealedVal)
                    {
                        RemainderHeal(i, duration, totalHealedVal);
                        yield return new WaitForEndOfFrame();
                    }
                    
                    healthPoints.value = Mathf.Lerp(healthPoints.value, smoothTick, smoothTickSpeed); 

                    smoothTick += smoothTickHealValue;

                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                for (int i = 0; i < duration; i++)
                {
                    float tick = Mathf.Min(1, tickSpeed);
                    healthPoints.value += tickHealValue;
                    if (i >= duration && healthPoints.value < totalHealedVal)
                    {
                        RemainderHeal(i, duration, totalHealedVal);
                    }
                    // Debug.Log("HOT is healing " + tickHealValue + " per " + tick + " for " + duration + " seconds");
                    yield return new WaitForSeconds(tick);
                }
            }    
        }

        private void RemainderHeal(int i, float duration, float totalHealedVal)
        {
            if (i >= duration && healthPoints.value < totalHealedVal)
                    {
                        float remainder = totalHealedVal - healthPoints.value;
                        healthPoints.value += remainder;
                        Debug.Log("Error: Healed with Remainder... Remainder amount was " + remainder);
                    }
        }

        public float GetPercentage ()
        {
            return 100 * GetFraction ();
        }

        public float GetFraction ()
        {
            return healthPoints.value / GetComponent<BaseStats> ().GetStat (Stat.Health);
        }

        public float GetHealthPoints ()
        {
            return healthPoints.value;
        }

        public float GetMaxHealthPoints ()
        {
            return GetComponent<BaseStats> ().GetStat (Stat.Health);
        }

        public GameObject GetInstigator (GameObject instigator)
        {
            return instigator;
        }



        private void UpdateState ()
        {     
            Animator animator = GetComponent<Animator> ();
            var player = GameObject.FindGameObjectWithTag("Player");

            if (!wasDeadLastFrame && IsDead())
            {
                RandomDeathAnimation();
                GetComponent<ActionScheduler>().CancelCurrentAction();
                if (this.gameObject != player)
                {
                    Collider collider = GetComponent<Collider> ();
                    Rigidbody rigidbody = GetComponent<Rigidbody> ();
                    Destroy (collider);
                    Destroy (rigidbody);
                }
            }

            if (wasDeadLastFrame && !IsDead())
            {
                animator.Rebind();
            }
            
            wasDeadLastFrame = IsDead();
        }

        private void RandomDeathAnimation()
        {
            Animator animator = GetComponent<Animator> ();
            int death = 0;
            if (deathAnimationParams.Length >= 1)
            {
                death = UnityEngine.Random.Range(0, deathAnimationParams.Length - 1);
                animator.SetTrigger(deathAnimationParams[death]);
            }
            else
            {
                animator.SetTrigger("die");
            }
        }

        private void AwardExperience (GameObject instigator)
        {
            Experience experience = instigator.GetComponent<Experience> ();
            if (experience == null)
            {
                Debug.Log("Error: Experience is null");
                return;
            }
            BaseStats baseStats = instigator.GetComponent<BaseStats>();
            if (baseStats.GetLevel() >= baseStats.GetMaxLevel())
            {
                Debug.Log("already at max level");
                return;
            }
            experience.GainExperience (GetComponent<BaseStats> ().GetStat (Stat.ExperienceReward));
        }

        private void RegenerateHealth ()
        {
            float newHealthPoints = GetComponent<BaseStats> ().GetStat (Stat.Health) * (levelUpHealthPercent / 100);
            healthPoints.value = Mathf.Max (healthPoints.value, newHealthPoints);
        }

        public object CaptureState ()
        {
            return healthPoints.value;
        }

        public void RestoreState (object state)
        {
            healthPoints.value = (float) state;
            
            UpdateState();
        }

    }
}