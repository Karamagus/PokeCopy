using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using System.Linq;
using pokeCopy;

public class PartyScreenUI : MonoBehaviour, IGameScreen
{
    [SerializeField] SceneData BattleSceneData;
    [SerializeField] BattleData battleData;

    PokemonParty playerParty;
    [SerializeField] Text intructs;

    [SerializeField] GameObject confirmMenu;
    [SerializeField] GameObject battleConfirmMenu;// how to do without referencing dependency injection
    [SerializeField] GameObject inventoryUI;


    [SerializeField] PartyInfoPanelUI infoPanel;
    [SerializeField] EventSystem controller;
    [SerializeField] Image type1;
    [SerializeField] Image type2;

    [SerializeField] PartyButonData[] team;

    public Pokemon SelectedPokemon { get; set; }
    public GameObject ConfirmMenu => confirmMenu;

    public GameObject BattleConfirmMenu => battleConfirmMenu;

    public GameObject InventoryUI => inventoryUI;

    public PartyButonData[] Team  => team; 

    bool reorderingParty;
    int switchIndex;

    bool isTmHmSelection;

    InputSystemUIInputModule input;

    System.Action OnSelectPokemon;
    // System.Func<bool> OnCancelButton;
    System.Action OnCancelButton;

    public void Awake()
    {
        //team = new PartyButonData[6];
        team = GetComponentsInChildren<PartyButonData>(true);
        if (controller == null)
            controller = EventSystem.current;

    }



    public void OnEnable()//refactor and update only on changes
    {
        // team = GetComponentsInChildren<PartyButonData>(true);
        //playerParty = BattleSceneData.RetrivePlayerParty();
        //playerParty = battleData.GetPlayerParty();

        //playerParty = BattleSceneData.RetrivePlayerParty();

        if (playerParty == null)
        {
            if (battleData)
                playerParty = battleData.GetPlayerParty();
            if (playerParty == null && GameManager.Instance.Player)
                SetPartyData(GameManager.Instance.Player.GetComponent<PokemonParty>());
        }


        for (int i = 0; i < team.Length; i++)
        {
            team[i].SetUpPkmnInfo(playerParty.Retrive(i));
        }
        SelectedPokemon = null;

        if (input == null)
            input = EventSystem.current.GetComponent<InputSystemUIInputModule>();



        controller.SetSelectedGameObject(team[0].gameObject);
        if (infoPanel != null)
        {
            infoPanel.UpdateInfoPanel(controller.currentSelectedGameObject.GetComponent<PartyButonData>().Pokemon);
        }
        controller.currentSelectedGameObject.GetComponent<Button>().OnSelect(null);
    }

    public void SetPartyData(PokemonParty party)
    {
        playerParty = party;
    }


    public void SetButtons(System.Action pokemonSelectionAction, System.Action cancelButtonAction, GameObject battleSelectionMenu = null)
    {
        //Debug.Log("Seted Buttons on party menu");
        //Set a function to open sub menu if pokemonSelection is Null
        if (pokemonSelectionAction != null)
            OnSelectPokemon = pokemonSelectionAction;

        if (OnSelectPokemon == null)
            OnSelectPokemon = OpenSelectionMenu;

        //Debug.Log(OnSelectPokemon?.Method);

        if (cancelButtonAction != null && OnCancelButton == null)
            OnCancelButton = cancelButtonAction;

        //Debug.Log(OnCancelButton?.Method);
        if (battleSelectionMenu != null)
            SetBattleSelectionMenu(battleSelectionMenu);
    }

    public void SetBattleSelectionMenu(GameObject battleSelectionMenu) => battleConfirmMenu = battleSelectionMenu;



    public void Open(System.Action pokemonSelectionAction = null, System.Action cancelButtonAction = null)
    {
        if (pokemonSelectionAction != null || cancelButtonAction != null)
            SetButtons(pokemonSelectionAction, cancelButtonAction);
        gameObject.SetActive(true);
    }


    public void SetSelectionAction(System.Action onSelection)
    {
        OnSelectPokemon = onSelection;
    }

    public void SelectPkmn()//on button using inspector RENAME: SelectPkmnButton
    {
        var selection = EventSystem.current.currentSelectedGameObject.GetComponent<PartyButonData>();
        SelectedPokemon = selection.Pokemon;

        SetPressedButtonColors(selection.Button);

        //contextual function
        OnSelectPokemon?.Invoke();
        switchIndex = GetSelectedPartyIndex();

    }

    public void SetAble(MovesBase move)
    {
        for (int i = 0; i < team.Count(); i++)
        {
            var poke = team[i].Pokemon;
            if (poke != null)
                team[i].SetLeanrableMove(poke.Base.TM_n_HM_Leanrnables.Contains(move));// && poke.HasMove(move));
        }
    }

    void SetPressedButtonColors(Button button)
    {
        if (reorderingParty)
            return;
        ColorBlock cb = button.colors;
        cb.normalColor = cb.pressedColor;
        button.colors = cb;


        //change normal color when button is pressed


    }


    // Update is called once per frame
    void Update()
    {
        HandleUpdate();

    }

    public void HandleUpdate()
    {
        //control summary screen here via HandleUpdate function on summarry screen class

        if (input.move.action.triggered && infoPanel != null && (!confirmMenu.activeSelf))//input.move.ToInputAction().ReadValue<Vector2>() != Vector2.zero)
        {
            if (battleConfirmMenu && battleConfirmMenu.activeSelf)
                return;

            var crrntSele = controller.currentSelectedGameObject.GetComponent<PartyButonData>();
            if (crrntSele != null)
                infoPanel.UpdateInfoPanel(crrntSele.Pokemon);
            else
                infoPanel.UpdateInfoPanel(null);

        }

    }


    public void OpenSelectionMenu()
    {
        confirmMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(confirmMenu.GetComponentInChildren<Button>().gameObject);
    }



    public void SummaryButton()
    {

    }


    public void SwitchButton()
    {
        if (GameManager.Instance.State != GameState.Menu)
            return;

        ColorBlock cb = team[switchIndex].Button.colors;
        reorderingParty = true;
        confirmMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(team[switchIndex].gameObject);
        cb.normalColor = cb.pressedColor;
        team[switchIndex].Button.colors = cb;
        OnSelectPokemon = MovePokemon;


    }

    public void MovePokemon()
    {
        if (GetSelectedPartyIndex() != switchIndex)
        {
            var selIndex = GetSelectedPartyIndex();
            playerParty.SwapPosition(switchIndex, GetSelectedPartyIndex());
            for (int i = 0; i < team.Length; i++)
            {
                if (i == switchIndex || i == selIndex)
                    team[i].SetUpPkmnInfo(playerParty.Retrive(i));
            }

            if (infoPanel != null)
                infoPanel.UpdateInfoPanel(controller.currentSelectedGameObject.GetComponent<PartyButonData>().Pokemon);
        }
        team[switchIndex].Button.colors = (team[switchIndex].Pokemon.HP > 0) ? UIColors.partyMenuNormalColors : UIColors.partyMenuFaintColors;
        reorderingParty = false;
        OnSelectPokemon = OpenSelectionMenu;

    }



    public void CancelButton()
    {
        if (GameManager.Instance.State == GameState.Menu)
        {
            if (reorderingParty)
            {
                reorderingParty = false;
                OnSelectPokemon = OpenSelectionMenu;
                ResetPressedButtonColors();
                return;
            }

            if (confirmMenu && confirmMenu.activeSelf)
            {
                confirmMenu.SetActive(false);
                EventSystem.current.SetSelectedGameObject(team[switchIndex].gameObject);
                ResetPressedButtonColors();
                return;
            }

            if (inventoryUI && inventoryUI.activeSelf)
            {
                inventoryUI.SetActive(false);
                EventSystem.current.SetSelectedGameObject(team[switchIndex].gameObject);
                ResetPressedButtonColors();
                return;

            }

        }

        if (GameManager.Instance.State == GameState.Battle)
        {
            if (battleConfirmMenu && battleConfirmMenu.activeSelf)
            {

                battleConfirmMenu.SetActive(false);
                EventSystem.current.SetSelectedGameObject(team[switchIndex].gameObject);
                ResetPressedButtonColors();
                return;

            }
        }


        OnCancelButton?.Invoke();
        /*
        if ((bool)OnCancelButton?.Invoke())
            return;

        //gameObject.SetActive(false);
        */

    }



    public bool TryCloseSubmenu(GameObject subMenu)
    {
        if (!subMenu.activeSelf)
            return false;

        subMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(team[switchIndex].gameObject);
        ResetPressedButtonColors();
        return true;
    }

    public void ResetPressedButtonColors()
    {
        team[switchIndex].Button.colors = (team[switchIndex].Pokemon.HP > 0) ? UIColors.partyMenuNormalColors : UIColors.partyMenuFaintColors;

    }
    

    public int GetSelectedPartyIndex()
    {
        return playerParty.Party.IndexOf(SelectedPokemon);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}

public enum PartyMenuState
{
    Menu, Battle_Decision, Battle_Fainted, Battle_Optional, Inventory_Selection
}
