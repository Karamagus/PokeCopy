using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pokeCopy;
[RequireComponent(typeof(PokemonParty))]
public class TrainerController : MonoBehaviour, IInteractable, ISavable, INamable
{
    [SerializeField] private TrainerClass trainerClass;
    [SerializeField] private string trainerName;
    [SerializeField] private Sprite trainerChar;

    public string Name 
    { 
        get => trainerClass.ToString().Replace("Jr_", "Jr. ").Replace("_n_", " & ").Replace('_',' ') + " " + trainerName;
    }


    public Sprite TrainerChar { get => trainerChar; }
    [SerializeField] TrainerData data;
    [SerializeField] GameObject exclamation;
    [SerializeField] CharMovement movement;
    [SerializeField] NPCMono behave;
    [SerializeField] Direction startingDirection;
    Vector2[] startingPos;
    [SerializeField] Dialogue dialogue;
    [SerializeField] Dialogue defeatDialogue;
    [SerializeField] GameObject fov;
    [SerializeField] PokemonTeam team;
    [SerializeField] PokemonParty pokeParty;

    public PokemonParty PokeParty { get => pokeParty; }

    Vector3 rot;

    [SerializeField] bool battleLost = false;

    public void Awake()
    {
        startingPos = new Vector2[4] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };//use dictionary?
        rot = new Vector3(0, 0, 90);
        team.Init();
        //No Need to do this more than unce
        data.SetData(Name, GetComponent<PokemonParty>(), trainerChar, team);
        //battleLost = data.GetBattleResult();
    }
    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<CharMovement>();
        movement.animator.SetFloat("Horizontal", Mathf.Clamp(startingPos[(int)startingDirection].x, -1, 1));
        movement.animator.SetFloat("Vertical", Mathf.Clamp(startingPos[(int)startingDirection].y, -1, 1));
        pokeParty = GetComponent<PokemonParty>();
        /*
        if (battleLost)
            BattleResultHandler();
        */
        //data.Awake();
        AdjustFOV();
    }

    // Update is called once per frame
    void Update()
    {

        if (movement.IsMoving)
            AdjustFOV();
    }

    void AdjustFOV()
    {
        //if instead: use vector2.angle(
        //rot.z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;
        //var fovPos = new Vector2(transform.position.x + dir.x, transform.position.y + dir.y);
        var dir = new Vector3(movement.animator.GetFloat("Horizontal"), movement.animator.GetFloat("Vertical"));
        var fovPos = new Vector2(transform.position.x, transform.position.y + 0.3f);

        rot.z = Vector2.SignedAngle(Vector2.down, dir);
        fov.transform.SetPositionAndRotation(fovPos, Quaternion.Euler(rot));

    }

    public void BattleResultHandler()
    {

        battleLost = data.GetBattleResult();
        if (battleLost)
            fov.SetActive(false);
        GameManager.Instance.OnBattleEnd -= BattleResultHandler;
    }


    public void GetBattleResult(bool result)
    {
        battleLost = result;
        if (battleLost)
            fov.SetActive(false);
    }

    public IEnumerator TriggerBattle(PlayerMovement player = null)//passing player allows for referencing without the need for a raycast
    {
        //behave.enabled = false;
        if (battleLost)
            yield break;

        exclamation.SetActive(true);
        yield return new WaitForSeconds(.5f);
        exclamation.SetActive(false);

        if (player != null)
        {
            var foePos = player.transform.position;
            var diff = foePos - transform.position;

            var moveVec = diff - diff.normalized;
            moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

            yield return movement.Move(moveVec);
            yield return DialogueCacheManager.I.ShowDialogue(dialogue);

            GameManager.Instance.StartTrainerBattle(this);


        }
        //yield return new WaitWhile(() => GameManager.Instance.State == GameState.Battle);
        //Debug.Log("UPdate trainer");
        //battleLost = data.GetBattleResult();

    }

    /*
    public IEnumerator TriggerBattleOld(PlayerMovement player = null)//passing player allows for referencing without the need for a raycast
    {
        //behave.enabled = false;
        if (battleLost)
            yield break;

        exclamation.SetActive(true);
        yield return new WaitForSeconds(.5f);
        exclamation.SetActive(false);

        if (player != null)
        {
            var foePos = player.transform.position;
            var diff = foePos - transform.position;

            var moveVec = diff - diff.normalized;
            moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

            yield return movement.Move(moveVec);
            DialogueCacheManager.I.StartDialogue(dialogue, () =>
            {
                if (GameManager.Instance.IsBattleAScene)
                {
                    GameManager.Instance.OnBattleEnd += BattleResultHandler;
                    GameManager.Instance.StartTrainerBattleScene(data);

                }
                else
                {
                    GameManager.Instance.StartTrainerBattle(this);
                }
            }
            );


        }
        /*
        else
        {
            var dir = new Vector3(movement.animator.GetFloat("Horizontal"), movement.animator.GetFloat("Vertical"));

            var detected = Physics2D.Raycast(transform.position + dir, dir, 5, LayersManager.i.PlayerLayer);
            if (detected)
            {
                var diff = (detected.transform.position - transform.position);
                Debug.Log(diff);
                var movVec = diff - diff.normalized;
                Debug.Log(movVec);
                movVec = new Vector2(Mathf.Round(movVec.x), Mathf.Round(movVec.y));
                yield return movement.Move(movVec);
                DialogueCacheManager.I.StartDialogue(dialogue, () =>
                {
                    if (GameManager.Instance.IsBattleAScene)
                    {
                        GameManager.Instance.OnBattleEnd += BattleResultHandler;
                        GameManager.Instance.StartTrainerBattleScene(data);

                    }
                    else
                    {
                        GameManager.Instance.StartTrainerBattle(this);
                    }
                });

            }
        }
        //Why this does not working?
        yield return new WaitUntil(() => GameManager.Instance.State == GameState.freeRoam);
        Debug.Log("UPdate trainer");
        battleLost = data.GetBattleResult();

    }
        */

    public IEnumerator Interact(Transform initiator) //reeavaluete
    {


        movement.LookTowrds(initiator.position);
        if (!battleLost)
        {
            yield return DialogueCacheManager.I.ShowDialogue(dialogue);
            /*
            DialogueCacheManager.I.StartDialogue(dialogue, () =>
            {
                GameManager.Instance.StartTrainerBattle(this);
            }
            );
            */
            GameManager.Instance.StartTrainerBattle(this);
        }
        else
        {
            //DialogueCacheManager.I.StartDialogue(defeatDialogue);
            yield return DialogueCacheManager.I.ShowDialogue(defeatDialogue);
        }


        yield return null;
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;

        if (battleLost)
            fov.SetActive(false);
    }

    /*
    public void InteractOld(Transform initiator = null)
    {

        Debug.LogWarning("Interact");

        movement.LookTowrds(initiator.position);
        Debug.LogWarning("moveTowrds");
        if (!battleLost)
        {

            DialogueCacheManager.I.StartDialogue(dialogue, () =>
            {
                if (GameManager.Instance.IsBattleAScene)
                {
                    GameManager.Instance.OnBattleEnd += BattleResultHandler;
                    GameManager.Instance.StartTrainerBattleScene(data);

                }
                else
                {
                    GameManager.Instance.StartTrainerBattle(this);
                }
            });
        }
        else
        {
            Debug.LogWarning("Start dialogue");
            DialogueCacheManager.I.StartDialogue(defeatDialogue);
        }

    }

    */

}


public enum Direction
{
    up, down, left, right,
}

public enum TrainerClass
{
    Artist,
    Belle_n_Pa,
    Cameraman,
    Clown,
    Commander,
    Cowgirl,
    Cyclist,
    Double_Team,
    Galactic_Grunt,
    Galactic_Boss,
    Idol,
    Jogger,
    PokéKid,
    Rancher,
    Reporter,
    Socialite,
    Veteran,
    Waiter,
    Waitress,
    Beauty,
    Biker,
    Bird_Keeper,
    Blackbelt,
    Boss,
    Bug_Catcher,
    Burglar,
    Champion,
    Channeler,
    Cool_Trainer,
    Cue_Ball,
    Elite_Four,
    Engineer,
    Fisherman,
    Gambler,
    Gentleman,
    Hiker,
    Jr_TrainerM,
    Jr_TrainerF,
    Juggler,
    Lass,
    Leader,
    PokéManiac,
    Psychic,
    Rival,
    Rocker,
    Rocket,
    Sailor,
    Scientist,
    Super_Nerd,
    Swimmer,
    Tamer,
    Youngster,

}





