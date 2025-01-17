﻿using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Utils;
using RPG.Core;
using UnityEditor;
using UnityEngine;

namespace RPG.Dialogue
{
    public class DialogueNode : ScriptableObject
    {
        [SerializeField]
        bool isPlayer = false;
        [SerializeField]
        string text;
        [SerializeField]
        List<string> children = new List<string> ();
        [SerializeField]
        Rect rect = new Rect (0, 0, 200, 100);
        [SerializeField] string onEnterAction;
        [SerializeField] string onExitAction;
        [SerializeField] Condition condition;

        public bool IsPlayerSpeaking ()
        {
            return isPlayer;
        }

        public Rect GetRect ()

        {
            return rect;
        }

        public string GetText ()
        {
            return text;
        }

        public List<string> GetChildren ()
        {
            return children;
        }

        public string GetOnEnterAction ()
        {
            return onEnterAction;
        }

        public string GetOnExitAction ()
        {
            return onExitAction;
        }

        public bool CheckCondition (IEnumerable<IPredicateEvaluator> evaluators)
        {
            bool conditionToCheck = condition.Check(evaluators);

            if (conditionToCheck == true)
            {
                return conditionToCheck;
            }

            return false;
        }

#if UNITY_EDITOR
        public void SetPosition (Vector2 newPosition)
        {
            Undo.RecordObject (this, "Move Dialogue Node");
            rect.position = newPosition;
            EditorUtility.SetDirty (this);
        }

        public void SetIsPlayerSpeaking (bool newIsPlayerSpeaking)
        {
            Undo.RecordObject (this, "Change Dialogue Speaker");
            isPlayer = newIsPlayerSpeaking;
            EditorUtility.SetDirty (this);
        }

        public void SetText (string newText)
        {
            if (newText != text)
            {
                Undo.RecordObject (this, "Update Dialogue Text");
                text = newText;
                EditorUtility.SetDirty (this);
            }
        }

        public void AddChild (string childID)
        {
            Undo.RecordObject (this, "Add Dialogue Link");
            children.Add (childID);
            EditorUtility.SetDirty (this);
        }

        public void RemoveChild (string childID)
        {
            Undo.RecordObject (this, "Remove Dialogue Link");
            children.Remove (childID);
            EditorUtility.SetDirty (this);
        }
        #endif
    }
}