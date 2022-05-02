using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using pokeCopy;

public delegate void OnBattleEndHandler();
public delegate void OnStateChangeHandler();

public class GameManager : MonoBehaviour
{
    //static GameManager instance;

    public static GameManager Instance;
    [SerializeField] GameState state;
    GameState prevState;

    [SerializeField] PlayerMovement player;
    [SerializeField] Camera playerCam;
    [SerializeField] SceneData battleSceneData;
    [SerializeField] BattleData battleData;
    [SerializeField] GameObject battleSystem;
    [SerializeField] MenuController menuController;
    [SerializeField] bool isBattleAScene;
    [SerializeField] Fader fader;

    public GameState State { get => state; }
    public PlayerMovement Player { get => player; }

    public event OnStateChangeHandler OnStateChange;
    public event OnBattleEndHandler OnBattleEnd;
    public SceneData BattleSceneData { get => battleSceneData; set => battleSceneData = value; }
    public BattleData BattleData { get => battleData; }
    public PlayerInputActions Inputs { get => Player.InputActions; }
    public bool IsBattleAScene { get => isBattleAScene; }

    public EventSystem eventSystem;

    public UIColors uiColors;

    public SceneDetails CrrtScene { get; private set; }
    public SceneDetails PrvScene { get; private set; }


    public void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        // DontDestroyOnLoad(gameObject);
        uiColors.Init();

        menuController = GetComponent<MenuController>();

        Debug.Log("GM woke");
        //battleSceneData.ClearData();
        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
        QuestDB.Init();

        menuController.Init();
    }


    // Start is called before the first frame update
    void Start()
    {

        DialogueCacheManager.I.OnShowDialogue += () =>
        {
            SetState(GameState.Dialogue);
        };


        DialogueCacheManager.I.OnCloseDialogue += () =>
        {
            if (state == GameState.Dialogue)
            {
                SetState(GameState.freeRoam);
            }
        };


        EvolutionSceneUI.I.OnStartEvolution += () => SetState(GameState.Evolution);
        EvolutionSceneUI.I.OnEndEvolution += ()
            =>
        {

            StartCoroutine(FadeOutFromEvolution_CR());

        };



    }


    public void Update()
    {

        if (state == GameState.freeRoam)
        {
            if (player.InputActions.PlayerDigital.OpenMenu.triggered && !player.IsMoving)
            {
                menuController.OpenMenu(eventSystem);
                SetState(GameState.Menu);
            }

        }
        else if (state == GameState.Menu)
        {

            if (player.InputActions.UI.Cancel.triggered)
            {
                menuController.OnBack?.Invoke();
            }
            if (Instance.Inputs.UI.Navigate.triggered)
                menuController.UpdateSelection();

        }
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Paused;
        }
        else
        {
            state = prevState;
        }
    }

    public void StartWildBattle()
    {
        //pass data for
        SetState(GameState.Battle);
        battleData.SetPlayer(player);

        battleData.IsTrainerBattle = false;
        //eventSystem.gameObject.SetActive(false);

        battleSystem.SetActive(true);
    }

    public void StartTrainerBattle(TrainerController trainer)
    {
        battleData.SetPlayer(player);

        battleData.SetTrainer(trainer);
        battleData.IsTrainerBattle = true;
        //eventSystem.gameObject.SetActive(false);

        SetState(GameState.Battle);
        battleSystem.SetActive(true);

    }


    public void OnTrainerView()
    {
        player.InputActions.PlayerDigital.Move.Disable();
        state = GameState.CutScene;

    }

    public void EndOfBattle()
    {
        //battleData.ClearData();
        //StartCoroutine(FadeFromBattle());
        //player.gameObject.SetActive(true);

        StartCoroutine(FadeOutFromBattle_CR());


    }


    public IEnumerator FadeOutFromBattle_CR()
    {
        battleSystem.SetActive(false);
        SetState(GameState.freeRoam);
        //eventSystem.gameObject.SetActive(true);
        yield return null;
        StartCoroutine(fader.FadeOut_CR(.2f));
        var party = player.GetComponent<PokemonParty>();
        yield return party.CheckForEvolutions_CR();

    }

    public IEnumerator FadeOutFromEvolution_CR()
    {
        yield return fader.FadeIn_CR(.5f);
        EvolutionSceneUI.I.EvolutionUI.SetActive(false);
        SetState(GameState.freeRoam);
        yield return fader.FadeOut_CR(.5f);
    }

    public IEnumerator FadeInToFreeRoam_CR()
    {

        yield return fader.FadeIn_CR(.5f);

    }

    //da
    void StartBattleScene()
    {
        battleSceneData.GetPosition(player.transform.position);
        battleSceneData.PassDirection(player.GetDirection());
        state = GameState.Battle;
        battleSceneData.LoadParty(player.GetComponent<PokemonParty>());
        eventSystem.gameObject.SetActive(false);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
        player.gameObject.SetActive(false);
        // SceneManager.LoadScene(1);

    }

    public void StartTrainerBattleScene(TrainerData info, TrainerController trainer = null)
    {
        battleSceneData.GetPosition(player.transform.position);
        battleSceneData.PassDirection(player.GetDirection());
        state = GameState.Battle;
        //battleSceneData.GetTrainerData(trainer);
        battleSceneData.GetTrainerData(info);
        eventSystem.gameObject.SetActive(false);

        battleSceneData.ReceiveBattleData(player.GetComponent<PokemonParty>(), info.Party);
        //SceneManager.LoadScene(1);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
        player.gameObject.SetActive(false);
    }



    // Update is called once per frame
    public void EndBattleScene()
    {
        state = GameState.freeRoam;
        //playerCam.gameObject.SetActive(true);
        battleSceneData.ClearData();
        OnBattleEnd?.Invoke();
        //eventSystem.gameObject.SetActive(true);
        player.gameObject.SetActive(true);
        SceneManager.UnloadSceneAsync(1);

    }

    //not implemented yet. Dependes of separation on the object level, each taking care of themself in each situation
    public void SetState(GameState newState)
    {
        OnStateChange?.Invoke();// Use when invetoryUI and partyMenu are one instanc used in all situations, eg.
        if (newState == GameState.Battle)
        {
            player.InputActions.PlayerDigital.Disable();
            player.InputActions.UI.Enable();

            //player.gameObject.SetActive(false);
            playerCam.gameObject.SetActive(false);
            //player.GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }

        if (newState == GameState.freeRoam)
        {
            switch (state)
            {
                case GameState.freeRoam:

                    break;
                case GameState.Battle:
                    battleData.ClearData();
                    eventSystem.gameObject.SetActive(true);
                    player.InputActions.PlayerDigital.Enable();
                    player.InputActions.UI.Disable();

                    playerCam.gameObject.SetActive(true);

                    break;
                case GameState.Evolution:
                case GameState.Menu:
                    player.InputActions.UI.Disable();
                    player.InputActions.PlayerDigital.Enable();

                    break;
                case GameState.MainMenu:
                    break;
                case GameState.Dialogue:
                    player.InputActions.PlayerDigital.Move.Enable();

                    break;
                case GameState.CutScene:
                    break;
                case GameState.Paused:

                    break;
                default:
                    break;
            }
            //player.gameObject.SetActive(true);
        }

        if (newState == GameState.Dialogue)//irrelevant maybe
        {
            player.InputActions.PlayerDigital.Move.Disable();
            //state = GameState.Dialogue;

        }

        if (newState == GameState.Menu)
        {
            player.InputActions.UI.Enable();
            player.InputActions.PlayerDigital.Disable();
        }

        if (newState == GameState.Evolution)
        {
            player.InputActions.UI.Enable();
            player.InputActions.PlayerDigital.Disable();

        }

        state = newState;

    }

    public void SetCurrentScene(SceneDetails scene)
    {
        PrvScene = CrrtScene;
        CrrtScene = scene;
    }


    public void SaveButton()
    {
        SavingSystem.i.Save("save");
    }

    public void LoadButton()
    {
        SavingSystem.i.Load("save");
    }




    public void OnApplicationQuit()
    {
        Instance = null;
    }
}


public enum GameState
{
    freeRoam,
    Battle,
    Menu,
    MainMenu,
    Dialogue,
    CutScene,
    Paused,
    Evolution,

}