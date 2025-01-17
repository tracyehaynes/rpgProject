﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.UI.DamageText
{
    public class DamageText : MonoBehaviour
    {
        [SerializeField] TMP_Text damageText = null;

        public void DestroyText ()
        {
            Destroy (gameObject);
        }

        public void SetValue (float amount)
        {
            damageText.text = string.Format ("{0:0}", amount);
        }

    }
}