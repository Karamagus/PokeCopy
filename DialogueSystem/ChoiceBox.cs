using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace pokeCopy
{
    public class ChoiceBox : MonoBehaviour
    {

        [SerializeField] List<Button> choicesButtons = new List<Button>();
        List<Text> choicesTexts = new List<Text>();

        int selectedIndex = -1;
        bool selected = false;

        System.Action OnSelection;

        public int SelectedIndex => selectedIndex;

        private void Awake()
        {
            if (choicesButtons.Count > 0)
            {
                foreach (var choice in choicesButtons)
                {
                    choicesTexts.Add(choice.GetComponentInChildren<Text>());
                    choice.gameObject.SetActive(false);
                }
            }
        }


        public IEnumerator SetChoices(List<string> choicesText, List<UnityAction> actions = null)
        {
            gameObject.SetActive(true);
            selected = false;
            selectedIndex = -1;
            for (int i = 0; i < choicesButtons.Count; i++)
            {
                if (i >= choicesText.Count)
                {
                    choicesButtons[i].gameObject.SetActive(false);
                    continue;
                }

                choicesTexts[i].text = choicesText[i];
                choicesButtons[i].gameObject.SetActive(true);



                if (actions != null)
                    choicesButtons[i].onClick.AddListener(actions[i]);

            }
            EventSystem.current.SetSelectedGameObject(choicesButtons[0].gameObject);
            yield return new WaitUntil(() => selected);

        }

        public void SelectChoiceButton()
        {
            selectedIndex = choicesButtons.IndexOf(EventSystem.current.currentSelectedGameObject.GetComponent<Button>());

            OnSelection?.Invoke();
            selected = true;
            gameObject.SetActive(false);
        }



    }
}
