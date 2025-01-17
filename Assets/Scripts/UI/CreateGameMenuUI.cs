using System;
using GameDevTV.Saving;
using GameDevTV.Utils;
using RPG.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPG.UI
{
    public class CreateGameMenuUI : MonoBehaviour
    {
        [SerializeField] Button createNewButton = null;
        [SerializeField] Button backButton = null;
        [SerializeField] UISwitcher switcher = null;

        [SerializeField] GameObject creationError = null;

        [SerializeField] TextMeshProUGUI inputText = null;
        [SerializeField] TMP_InputField newGameNameField = null;

        LazyValue<SavingWrapper> savingWrapper;

        private void Awake() 
        {
            savingWrapper = new LazyValue<SavingWrapper>(GetSavingWrapper);
        }

        public void CreateNewGame()
        {
            if (newGameNameField != null && !String.IsNullOrEmpty(newGameNameField.text) )
            {                
                savingWrapper.value.CreateNewGame(newGameNameField.text);                
            }
            else
            {
                switcher.SwitchTo(creationError);
            }
        }

        private SavingWrapper GetSavingWrapper()
        {
            return FindObjectOfType<SavingWrapper>();
        }
    }
}