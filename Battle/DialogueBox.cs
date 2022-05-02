using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using System;


namespace pokeCopy
{

    public class DialogueBox : MonoBehaviour //Refator needed. Better integration with battle system ekm menus. Redesign Architecture
    {

        /* Needs Clean Up of cancel action. Use A Delegate instead of if steatments 
         * and configure in a way that there would be the nedd to contantly keep 
         * track o current and previus open windows
         */

        [SerializeField] Text dialogueText;
        [SerializeField] EventSystem buttonController;

        [SerializeField] GameObject ActionMenu;
        [SerializeField] GameObject MovesMenu;
        [SerializeField] GameObject ChoiceButtons;
        [SerializeField] Text moveTypeText;
        [SerializeField] Text ppText;
        [SerializeField] GameObject currentActiveMenu;
        GameObject previousActive;
        GameObject previousSelection;
        [SerializeField] InputSystemUIInputModule input;
        //[SerializeField] PartyMenu partySelection;

        [SerializeField] Button[] moveButtuns;//can be by gameobject too
        public Dictionary<Button, Move> ButtonMove { get; private set; }//decouple this from the dialog system so it limits its funtions to dialogue only

        const float DEFAULT_READ_TIME = 1.25f;
        const float DEFAULT_TYPE_DELAY = 0.0175f;

        public MenuMode menuMode;//revise
        public SubMenu subMenu;

        Action OnCancelInput;


        private void Awake()
        {
            buttonController = EventSystem.current;
            input = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        }

        public void SetText(string dialog)
        {
            dialogueText.text = dialog;
        }

        public IEnumerator TypeDialougueText_CR(string dialogue, float readTimeInSeconds = DEFAULT_READ_TIME, bool persistAfterRead = false, bool keepPrev = false, float typingDelayInSeconds = DEFAULT_TYPE_DELAY)
        {
            if (!keepPrev)
                dialogueText.text = "";

            foreach (char letter in dialogue.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingDelayInSeconds);

            }
            yield return new WaitForSeconds(readTimeInSeconds);

            if (!persistAfterRead)
                SetText("");
        }

        public IEnumerator TypeDialougueText_CR(string dialogue, float readTimeInSeconds, float typingDelayInSeconds)
        {
            yield return (TypeDialougueText_CR(dialogue, readTimeInSeconds, false, false, typingDelayInSeconds));
        }

        public IEnumerator TypeDialougueText_CR(string dialogue, bool persistAfterRead, bool keepPrev)
        {
            yield return (TypeDialougueText_CR(dialogue, DEFAULT_READ_TIME, persistAfterRead, keepPrev, DEFAULT_TYPE_DELAY));

        }


        public void SetActionsMenu()
        {
            ActionMenu.SetActive(true);
            menuMode = MenuMode.Actions;
            currentActiveMenu = ActionMenu;

            if (ActionMenu.activeSelf)
                buttonController.SetSelectedGameObject(ActionMenu.GetComponentInChildren<Button>().gameObject);

        }

        public void SetMovesMenu()
        {
            menuMode = MenuMode.Moves;
            SetCurrentActiveMenu(MovesMenu);
            UpdateMoveData();
        }


        public void UpdateMoveData()
        {
            var b = buttonController.currentSelectedGameObject.GetComponent<Button>();

            if (ButtonMove.ContainsKey(b))
            {
                Move m = ButtonMove[b];
                ppText.text = $"PP {m.Pp}/{m.Base.MaxPP}";
                if (m.Pp <= 0)
                    ppText.color = Color.red;
                else
                    ppText.color = Color.HSVToRGB(0, 0, .20f);
                moveTypeText.text = m.Base.Type.ToString();
            }
        }

        void Update()
        {

            //if (input == null)
            //  input = EventSystem.current.GetComponent<InputSystemUIInputModule>();

            if (menuMode == MenuMode.Moves && input.move.ToInputAction().ReadValue<Vector2>() != Vector2.zero)
                UpdateMoveData();

            if (menuMode != MenuMode.None && input.cancel.action.triggered)
                CancelButtonHandler();
        }

        public void CancelButtonHandler()
        {
            if (menuMode == MenuMode.Party || menuMode == MenuMode.Confirmation || menuMode == MenuMode.Forget_Move || menuMode == MenuMode.Bag)
            {
                if (previousActive != null)
                    currentActiveMenu = previousActive;

                OnCancelInput?.Invoke();
                return;

            }
            ReturnToActionMenu();
        }

        public void ReturnToActionMenu()
        {

            if (currentActiveMenu == ActionMenu || !currentActiveMenu)// && menuMode == MenuMode.Actions)
                return;

            currentActiveMenu.SetActive(false);
            SetActionsMenu();

        }


        public void SetMoveButtons(List<Move> moves)
        {
            ButtonMove = new Dictionary<Button, Move>();

            Text bText;
            int i = 0;
            foreach (var b in moveButtuns)
            {
                bText = b.GetComponentInChildren<Text>();
                if (i < moves.Count)
                {
                    b.enabled = true;
                    bText.text = moves[i].Base.name;
                    ButtonMove.Add(b, moves[i]);
                    b.interactable = true;
                    b.GetComponent<Image>().color = Color.white;
                }
                else
                {

                    b.GetComponent<Image>().color = b.colors.disabledColor;
                    b.enabled = false;

                    bText.text = " - ";
                }
                i++;
            }
        }

        //Refactor
        public void SetCurrentActiveMenu(GameObject menu, bool keepPrevious = false, MenuMode mode = MenuMode.None, Action onReturn = null)
        {
            menu.SetActive(true);
            if (mode != MenuMode.None)
                menuMode = mode;

            if (currentActiveMenu != null)
            {
                if (keepPrevious)
                {
                    previousActive = currentActiveMenu;
                    previousSelection = EventSystem.current.currentSelectedGameObject;
                }
                else
                    currentActiveMenu.SetActive(false);
            }

            currentActiveMenu = menu;
            if (onReturn != null)
                OnCancelInput = onReturn;

            buttonController.SetSelectedGameObject(currentActiveMenu.GetComponentInChildren<Button>().gameObject);
        }

        public void SetCurrentBattleScreen(pokeCopy.IGameScreen screen, Action onSelection = null, Action onReturn = null, MenuMode mode = MenuMode.None, bool keepPrevious = false)
        {

            if (currentActiveMenu != null)
            {
                if (keepPrevious)
                {
                    previousActive = currentActiveMenu;
                    previousSelection = EventSystem.current.currentSelectedGameObject;
                }
                else
                    currentActiveMenu.SetActive(false);
            }

            screen.Open(onSelection, onReturn);
            menuMode = mode;

            currentActiveMenu = screen.GetGameObject();
            OnCancelInput = screen.CancelButton;
        }


        public void CloseMenus()
        {
            currentActiveMenu.SetActive(false);
            if (previousActive != null)
                previousActive.SetActive(false);
            previousActive = null;
            currentActiveMenu = null;
            OnCancelInput = null;
            subMenu = SubMenu.none;
            menuMode = MenuMode.None;//to test
        }



        public Move GetMove(Button moveButton)
        {
            return ButtonMove[moveButton];

        }


        public void SetCurrentByMode(MenuMode mode)
        {

        }

        public void UpdateMoveDataByIndex()
        {

        }


    }


    public enum MenuMode
    {
        None,
        Actions,
        Moves,
        Bag,
        Party,
        Sub_Party,
        Sub_Bag,
        Confirmation,
        Forget_Move,
        Busy

    }

    public enum SubMenu
    {
        none,
        Sub_Party,
        Sub_Bag,
        Confirmation,

    }
}
