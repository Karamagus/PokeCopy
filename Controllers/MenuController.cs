using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace pokeCopy
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] GameObject menu;
        [SerializeField] PartyScreenUI PartyMenu;
        [SerializeField] InventoryUI bag;

        EventSystem eventSystem;

        PlayerMovement player;

        Button[] buttons;

        MenuScreen menuState;
        int slect;


        public Action OnBack { get; private set; }
        public PartyScreenUI PartyMenu1 { get => PartyMenu; }

        public void Init(EventSystem control = null)
        {
            player = GameManager.Instance.Player;
            SetControls(control);
            PartyMenu.SetPartyData(player.GetComponent<PokemonParty>());
            buttons = GetComponentsInChildren<Button>();
            PartyMenu.SetButtons(null, () =>
            {
                ReturnToMenu();
                PartyMenu.gameObject.SetActive(false);

            });
            bag.SetButtons(null, () =>
            {
                bag.gameObject.SetActive(false);
                ReturnToMenu();

            });
        }

        void SetControls(EventSystem control)
        {
            if (eventSystem == null && control != null)
                eventSystem = control;
            else
                eventSystem = EventSystem.current;
        }



        public void OpenMenu(EventSystem eventsSystem = null)
        {
            menu.SetActive(true);

            if (eventsSystem && !eventSystem)
                eventsSystem.SetSelectedGameObject(menu.GetComponentInChildren<Button>().gameObject);
            else
                eventSystem.SetSelectedGameObject(menu.GetComponentInChildren<Button>().gameObject);

            OnBack = CloseMenu;
        }

        void DisableButtons()
        {

            foreach (var b in buttons)
                b.interactable = false;
        }

        void EnableButtons()
        {

            foreach (var b in buttons)
                b.interactable = true;
        }

        public void CloseMenu(EventSystem eventsSystem = null)
        {
            menu.SetActive(false);
            if (eventsSystem)
                eventsSystem.SetSelectedGameObject(null);

            GameManager.Instance.SetState(GameState.freeRoam);

        }

        public void CloseMenu()
        {
            menu.SetActive(false);
            if (eventSystem)
                eventSystem.SetSelectedGameObject(null);
            GameManager.Instance.SetState(GameState.freeRoam);
            slect = 0;

            OnBack = null;
        }



        public void HandleUpdate()
        {
            if (GameManager.Instance.Inputs.UI.Cancel.phase == UnityEngine.InputSystem.InputActionPhase.Canceled)
            {

                CloseMenu(GameManager.Instance.eventSystem);
            }
            if (GameManager.Instance.Inputs.UI.Navigate.triggered)
                slect = eventSystem.currentSelectedGameObject.transform.GetSiblingIndex();
        }

        public void UpdateSelection()
        {
            if (menu.activeSelf)
                slect = eventSystem.currentSelectedGameObject.transform.GetSiblingIndex();

        }


        void ReturnToMenu()
        {
            menu.SetActive(true);

            if (eventSystem)
                eventSystem.SetSelectedGameObject(menu.transform.GetChild(slect).gameObject);

            OnBack = CloseMenu;

        }

        public void PartyButton()
        {
            /*
            PartyMenu.Open(PartyMenu.OpenSelectionMenu, () =>
            {
                ReturnToMenu();
                PartyMenu.gameObject.SetActive(false);
               // return false;
            });
            */
            PartyMenu.Open();

            //PartyMenu.gameObject.SetActive(true);
            menu.SetActive(false);
            // if (eventSystem)
            EventSystem.current.SetSelectedGameObject(PartyMenu.GetComponentInChildren<Button>().gameObject);

            OnBack = PartyMenu.CancelButton;
        }




        public void BagButton()
        {

            bag.Open(null, () =>
            {
                bag.gameObject.SetActive(false);
                ReturnToMenu();
                //return false;

            });

            eventSystem.SetSelectedGameObject(bag.GetLastSelection());
            bag.UpdateDescription();
            bag.HandleScrolling();

            menu.SetActive(false);
            OnBack = bag.CancelButton;
        }

        public void TrainerButton()
        {

        }

        public void SaveButton()
        {

            //show message of saving or override of save file using dialogueCacher
            //check if file already exists
            //ask if wants to overrride if it is the case
            SavingSystem.i.Save("save");
            //show save massage

        }

        public void LoadButton()
        {
            SavingSystem.i.Load("save");
        }


    }
}

public enum MenuScreen { Menu, PartyScreen, Inventory, Profile, Pokedex }
