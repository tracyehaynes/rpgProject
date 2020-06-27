﻿// using System.Collections;
// using System.Collections.Generic;
using GameDevTV.Core.UI.Dragging;
using GameDevTV.UI.Inventories;
using RPG.Stats;
using UnityEngine;

namespace RPG.UI
{
    public class SkillTreeUI : MonoBehaviour
    {
        // CONFIG DATA
        [SerializeField] ActionSkillSlotUI actionSkillPrefab = null;

        // CACHE
        ActionSkill actionSkill;
        SkillTree skillTree;
        InventoryItemIcon skillIcon;

        void Awake ()
        {

            //skillTree.skillTreeUpdated += Redraw; // TODO fix chase Condition
        }
        private void Start ()
        {
            skillTree = GameObject.FindGameObjectWithTag("Player").GetComponent<SkillTree>().GetSkillTree();
            //Redraw ();
        }

        // private void Redraw ()
        // {
        //     // This is Destroying all Action Skill Slots at Runtime
        //     foreach (Transform child in transform)
        //     {
        //         Destroy (child.gameObject);
        //     }

        //     // This is Redrawing all available skills at Runtime
        //     // Will Later set this to total Class skill size most likely

        //     for (int i = 0; i < skillTree.GetTreeSize (); i++)
        //     {
        //         var skillUI = Instantiate (actionSkillPrefab, transform);
        //         //skillUI.Setup (skillTree, i);
        //     }
        //}
    }
}