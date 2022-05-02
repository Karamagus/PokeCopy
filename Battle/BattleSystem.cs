using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.Animations;
using pokeCopy;
using System.Linq;
using System;


namespace pokeCopy
{
    public enum BattleState { START, DECISION_PHASE, TURN_RESOLUTION, SWITCHING, FOE_SWITCHING, CATCHING, FORGET_MOVE, END }
    public enum BattleAction { Moves, Switch, UseItem, Run };

    public class BattleSystem : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] SceneData battlesSceneData;
        [SerializeField] BattleData battlesData;

        bool isTrainerBattle = false;
        bool isScene = false;

        PokemonParty playerParty;
        PokemonParty trainerParty;
        PokemonTeam trainerTeam;
        Pokemon wildPokemon;

        TrainerController trainer;
        TrainerData trainerData;

        public GameObject playerPrefab;
        public GameObject enemyPrefab;

        [Header("Units Game Objects")]
        [SerializeField] GameObject pokeBall;
        [SerializeField] GameObject playerObject;
        [SerializeField] GameObject enemyObject;
        [SerializeField] Transform playerBattleStation;
        [SerializeField] Transform enemyBattleStation;
        [SerializeField] SpriteRenderer playerChar;
        [SerializeField] SpriteRenderer trainerChar;
        [SerializeField] SpriteRenderer background;

        Unit playerUnit;
        Unit enemyUnit;

        [Header("HUDs")]
        [SerializeField] BattleHud playerHUD;
        [SerializeField] BattleHud enemyHUD;

        [Header("SubSystems and Windows")]
        [SerializeField] PartyScreenUI partyMenu;
        [SerializeField] InventoryUI inventory;
        [SerializeField] GameObject choiceDoSwitchMenu;
        [SerializeField] GameObject partyBattleSelection;
        [SerializeField] GameObject inventoryPocketSelectionMenu;
        [SerializeField] ForgetMenuSubMenuUI forgetMoveUI;
        public DialogueBox dialogueBox;
        [SerializeField] BattleAnimationsController animations;

        [Space]
        [Header("State Info")]
        [SerializeField][ReadOnly] BattleState state;
        [SerializeField][ReadOnly] BattleState prevState;

        [SerializeField][ReadOnly] int fleeAtempts;
        [SerializeField][ReadOnly] int crntTurn;

        public BattleState State
        {
            get => state;
            set
            {
                prevState = state;
                state = value;
            }
        }

        private void Awake()
        {
        }

        public void Start()
        {
            isScene = GameManager.Instance.IsBattleAScene;
            if (battlesData == null)
                battlesData = GameManager.Instance.BattleData;

            partyMenu.SetBattleSelectionMenu(partyBattleSelection);
            partyMenu.SetButtons(OnSelectFromParty, CancelButtonPartyMenu);
            // forgetMoveUI.SetButtons();
        }

        public void OnEnable()
        {
            state = BattleState.START;
            enemyHUD.Hide();
            playerHUD.Hide();
            isTrainerBattle = battlesData.IsTrainerBattle;

            if (isScene)
            {
                playerParty = battlesSceneData.RetrivePlayerParty();
                wildPokemon = battlesSceneData.RetriveWildPkmn();

                trainerData = battlesSceneData.RetriveTrainerData();
                playerChar.sprite = battlesSceneData.PlayerChar;
                //dialogueBox = GetComponentInChildren<DialogueBox>();
                //Debug.Log(trainerData == null);
                // Debug.Log(trainerTeam);
                if (trainerData != null)
                {
                    trainerParty = trainerData.Party;
                    trainerTeam = trainerData.team;
                }
            }
            else
            {
                playerChar.sprite = battlesData.GetPlayer().BattleSprite;
                playerParty = battlesData.GetPlayerParty();

                trainer = battlesData.GetTrainer();
                if (trainer)
                    trainerChar.sprite = trainer.TrainerChar;

                trainerParty = battlesData.GetTrainerParty();

                wildPokemon = battlesData.GetWildPkmn();

            }
            StartCoroutine(StartBattle_CR());

        }

        public void Update()
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
                Application.Quit();

        }

        void ChangeState(BattleState newState)
        {
            prevState = state;
            state = newState;
        }

        public IEnumerator StartBattle_CR()
        {
            playerChar.gameObject.SetActive(true);
            dialogueBox.SetText("");
            fleeAtempts = 0;
            crntTurn = 0;

            if (isTrainerBattle)
            {
                trainerChar.gameObject.SetActive(true);
                //  trainerChar.sprite = trainerData.BattleSrpite;
                trainerChar.sprite = trainer.TrainerChar;

                yield return dialogueBox.TypeDialougueText_CR($"{trainer.Name} wants to battle.");

            }

            StartCoroutine(SetUpBattle_CR());
        }


        IEnumerator SetUpBattle_CR()
        {
            trainerChar.gameObject.SetActive(false);

            //Set enemy Unit
            var enemyPoke = (isTrainerBattle) ? trainerParty.GetHealthy() : wildPokemon;
            enemyObject.SetActive(true);
            enemyUnit = enemyObject.GetComponent<Unit>();
            enemyUnit.SetUpUnit(enemyPoke, enemyHUD);

            //Change start pokemon sprite animation so if traner get out of pokeball aka same animation as playerUnity on Battle_EnemyBegin

            //yield return new WaitForSeconds(0.5f);

            if (isTrainerBattle)
            {
                enemyUnit.SetAnimatorControl(animations.TrainerOverride);
                yield return dialogueBox.TypeDialougueText_CR($"{trainer.Name} send out a {enemyPoke.Species}.");
                yield return new WaitForSeconds(.5f);
                enemyUnit.unitName = "The foe's " + enemyUnit.unitName;
            }
            else
            {
                enemyUnit.SetAnimatorControl(animations.WildOverride);
                yield return dialogueBox.TypeDialougueText_CR("A wild " + enemyUnit.unitName + " approaches!");
                yield return new WaitForSeconds(.5f);
                enemyUnit.unitName = "The wild " + enemyUnit.unitName;

            }

            //Set PlayerUnit
            playerChar.gameObject.SetActive(false);
            playerObject.SetActive(true);
            playerUnit = playerObject.GetComponent<Unit>();
            playerUnit.SetUpUnit(playerParty.GetHealthy(), playerHUD);
            playerUnit.CrrPokemon.DoesLevelUp = false;

            yield return new WaitForSeconds(1.5f);

            //Change Phase and set move Buttons
            ChangeState(BattleState.DECISION_PHASE);

            dialogueBox.SetMoveButtons(playerUnit.CrrPokemon.MoveSet);

            DecisionPhase();

        }


        IEnumerator SwitchPokemonUnit_CR(Pokemon substitute)
        {
            if (playerUnit.CrrPokemon.HP > 0)
            {
                yield return dialogueBox.TypeDialougueText_CR($"Come back, {playerUnit.name}.");

                yield return animations.SwitchPokemon_CR(playerUnit);
            }
            playerHUD.Hide();
            playerUnit.CrrPokemon.RemoveAllVolat();

            yield return new WaitForSeconds(.75f);

            //Set New Unit
            playerUnit.SetUpUnit(substitute, playerHUD);
            yield return dialogueBox.TypeDialougueText_CR($"Go {playerUnit.name}!!!");

            dialogueBox.SetMoveButtons(playerUnit.CrrPokemon.MoveSet);


            yield return new WaitForSeconds(playerUnit.animator.GetCurrentAnimatorStateInfo(0).length);


            yield return new WaitForSeconds(.75f);

            partyMenu.SelectedPokemon = null;

            ChangeState(BattleState.TURN_RESOLUTION);//maybe change?
        }


        void DecisionPhase()
        {

            if (playerUnit.MoveOfChoice?.isCharged == false)
            {
                playerUnit.MoveOfChoice.isCharged = true;
                StartCoroutine(TurnResolution_CR(BattleAction.Moves));
                return;
            }


            dialogueBox.SetText("Choose an action.");
            dialogueBox.SetActionsMenu();
        }


        IEnumerator TurnResolution_CR(BattleAction playerAction)
        {
            ChangeState(BattleState.TURN_RESOLUTION);
            crntTurn++;


            //enemy decision

            enemyUnit.SetMoveToUse(playerUnit);
            //Allow to enmey to run away if wild
            //enemyUnit.MoveOfChoice = enemyUnit.CrrPokemon.GetRandomMove();

            if (playerAction == BattleAction.Moves)
            {
                //Array or list to define order of multple units
                bool doesPlayerGoesFirst = IsPlayerFirst(playerUnit, enemyUnit);

                var firstUnit = (doesPlayerGoesFirst) ? playerUnit : enemyUnit;
                var secondUnit = (doesPlayerGoesFirst) ? enemyUnit : playerUnit;

                yield return UseMove_CR(firstUnit, secondUnit, firstUnit.MoveOfChoice);
                yield return AfterMove_CR(firstUnit);
                if (state == BattleState.END) yield break;

                if (secondUnit.CrrPokemon.HP > 0 && secondUnit.MoveOfChoice != null)
                {
                    yield return UseMove_CR(secondUnit, firstUnit, secondUnit.MoveOfChoice);
                }
                yield return AfterMove_CR(secondUnit);
                if (state == BattleState.END) yield break;
            }
            else
            {
                if (playerAction == BattleAction.Switch)
                    yield return SwitchPokemonUnit_CR(partyMenu.SelectedPokemon);

                else if (playerAction == BattleAction.Run)
                {
                    yield return TryToEscape_CR(playerUnit, enemyUnit);
                    if (state == BattleState.END) yield break;
                }
                else if (playerAction == BattleAction.UseItem)
                {


                    //
                    yield return new WaitForSeconds(0.5f);
                    //play animation
                    /*
                    var itemUsed = inventory.UseItem_OnBattle();

                    if (itemUsed is PokeBallItem)
                        yield return ThrowPokeBall_CR(itemUsed as PokeBallItem);
                    else
                    {
                        yield return playerHUD.WaitHpUpdate();
                        yield return dialogueBox.TypeDialougueText_CR($"Used {itemUsed.name} on {inventory.PartyMenuSelection.SelectedPokemon.NickName}.");
                    }
                    //
                    */

                    //yield return ThrowPokeBall_CR();
                    if (state == BattleState.END) yield break;

                }

                yield return UseMove_CR(enemyUnit, playerUnit, enemyUnit.MoveOfChoice);
                yield return AfterMove_CR(enemyUnit);
                if (state == BattleState.END) yield break;

            }



            yield return OnEndOfRound_CR();

            if (state != BattleState.END)
            {
                ChangeState(BattleState.DECISION_PHASE);
                DecisionPhase();
            }
        }

        bool IsPlayerFirst(Unit player, Unit enemy)
        {
            int player_P = player.MoveOfChoice.Base.Priority;
            int enemy_P = enemy.MoveOfChoice.Base.Priority;

            int speedDiff = player.CrrPokemon.Speed - enemy.CrrPokemon.Speed;

            if (player_P > enemy_P)
                return true;

            if (player_P == enemy_P)
            {
                if (speedDiff > 0)
                    return true;
                if (speedDiff == 0)
                    return (UnityEngine.Random.value > 0.5f);
            }

            return false;
        }

        IEnumerator UseMove_CR(Unit user, Unit target, Move move)
        {
            dialogueBox.SetText("");

            bool canMove = user.CrrPokemon.BeforeTurnEffects();
            //play before effect animation
            if (!canMove)
            {
                yield return user.BHud.WaitHpUpdat_CR();
                yield return ShowEffectsChanges_CR(user);
                // method returning a bool 
                if (user.CrrPokemon.HP <= 0)
                {
                    //
                    //faint animation
                    //

                    yield return Fainted_CR(user);

                }
                //

                yield break;
            }
            yield return ShowEffectsChanges_CR(user);

            //Check codition to execute
            // when selecting


            if (move.TurnsBfrHit <= 0)
                move.Pp--;


            //deal with charge moves here
            //TO DO: Check if use custom used move message for

            yield return dialogueBox.TypeDialougueText_CR($" {user.unitName} used {move.Base.name}", 1f);

            if (MoveHitCheck(move, user, target))
            {
                int dmg = 0;
                if (move.Base.Category == MoveCategory.Status)
                {
                    yield return animations.AnimateMoveUsage_CR(user, target, move);
                    yield return animations.EffectAnimation_CR(target);
                    yield return RunMoveEffects_CR(move.Base.Effects, user, target, move.Base.Target);

                }
                else
                {
                    //&& move.Base.Power != 0
                    if (move.isCharged)// use charge turn move propety instead
                    {

                        //play animation coordenated by animation events
                        //Check for the overheads on assinging animaitons at runtime
                        /*
                        user.animator.Play("Battle_EnemyAttack");
                        target.impact.Play();

                        yield return new WaitForSeconds(user.animator.GetCurrentAnimatorStateInfo(0).length);
                        //
                        */

                        yield return animations.AnimateMoveUsage_CR(user, target, move);
                        DamageDetails details = target.TakeMoveDamage(move, user);

                        target.animator.Play("BattleEnemy_Damaged2");

                        yield return new WaitForSeconds(target.animator.GetCurrentAnimatorStateInfo(0).length);

                        yield return target.BHud.WaitHpUpdat_CR();


                        dmg = details.Damage;
                        yield return ShowMoveDetails_CR(details);

                    }
                    else
                    {
                        //play charging animation?
                        yield return dialogueBox.TypeDialougueText_CR($"{user.name} is charging a attack.");//make constumized for each move
                    }
                }

                var hitEffect = move.Base.EffectWhenHit;
                if (hitEffect != HitEffectsID.none)
                {
                    //TO DO: set this effect on takedamage method, without the need of capturing damage without method
                    ConditionsDB.HitEffects[hitEffect].OnHit?.Invoke(user.CrrPokemon, dmg);

                    //Animations

                    yield return user.BHud.WaitHpUpdat_CR();
                    var affectd = (hitEffect == HitEffectsID.leech_life) ? target : user;
                    yield return dialogueBox.TypeDialougueText_CR($"{affectd.unitName}{ConditionsDB.HitEffects[hitEffect].Message}");

                }
                if (target.CrrPokemon.HP <= 0)
                {
                    yield return Fainted_CR(target);
                    yield break;

                }


                if (user.CrrPokemon.HP <= 0)
                {
                    yield return Fainted_CR(user);
                    yield break;
                }


                if (move.Base.Secundaries != null && move.Base.Secundaries.Count > 0)
                {
                    foreach (var secondary in move.Base.Secundaries) //(int i = 0; i < move.Base.Secundaries.Count; i++)
                    {
                        if (secondary.Condition == ConditionID.leech_seed)
                            continue;

                        if (UnityEngine.Random.Range(1, 101) <= secondary.StatusChance)
                        {
                            var doesHappenOnChargingTurn = ((move.isCharged && secondary.Target == Target.Foe) || (!move.isCharged && secondary.Target == Target.Self));
                            if (!move.Base.HasChargingTurn || doesHappenOnChargingTurn)
                                yield return RunMoveEffects_CR(secondary, user, target, secondary.Target);


                            //play animations here? only one execution but need to check if there is no more then two diferente changes, like one up stat, one doen stat, one status change
                        }
                    }
                }

            }
            else
                yield return dialogueBox.TypeDialougueText_CR($"{user.unitName}'s attack missed!");

            yield return new WaitForSeconds(.75f);
            move.LastUsedTurn = crntTurn;// change so charge moves count as last used after the charged turn

        }



        IEnumerator OnMove(HitEffectsID effect, Unit user, int dmg)
        {
            yield return null;
        }


        IEnumerator AfterMove_CR(Unit user)
        {
            if (state == BattleState.END) yield break;
            yield return new WaitUntil(() => state == BattleState.TURN_RESOLUTION);

            user.CrrPokemon.AfterTurnEffects();

            //animation

            yield return user.BHud.WaitHpUpdat_CR();
            yield return ShowEffectsChanges_CR(user);
            //refactor
            if (user.CrrPokemon.HP <= 0)
            {
                yield return Fainted_CR(user);
            }
        }

        IEnumerator RunMoveEffects_CR(MoveEffects effects, Unit user, Unit enemy, Target effectTarget)
        {

            var target = (effectTarget == Target.Self) ? user : enemy;

            if (effects.StatMods != null)
                target.ApplyStatStage(effects.StatMods);

            if (effects.Condition != ConditionID.none)
                target.ApplyCondition(effects.Condition, user);


            //play animations here animation



            yield return ShowEffectsChanges_CR(user);
            yield return ShowEffectsChanges_CR(enemy);

        }

        IEnumerator TryToEscape_CR(Unit escapee, Unit persuer)
        {
            if (isTrainerBattle)
            {
                yield return dialogueBox.TypeDialougueText_CR("Can't escape!");

                yield break;
            }

            if (OddsOfEscape(escapee.GetStat(Stat.Speed), persuer.GetStat(Stat.Speed)))
            {
                if (escapee == playerUnit)
                    yield return dialogueBox.TypeDialougueText_CR("You got away safely!", 2.25f, persistAfterRead: true);
                else
                    yield return dialogueBox.TypeDialougueText_CR($"{escapee.CrrPokemon.NickName} ran away!", 2.25f, persistAfterRead: true);

                ChangeState(BattleState.END);//

                yield return new WaitForSeconds(1f);
                yield return GameManager.Instance.FadeInToFreeRoam_CR();

                OnEndBattle(false);
                yield break;
            }

            yield return dialogueBox.TypeDialougueText_CR("You couldn't get away!", 2.25f);


        }

        IEnumerator OnEndOfRound_CR()
        {

            //animation
            yield return playerUnit.LeechSeedEffect_CR();
            yield return ShowEffectsChanges_CR(playerUnit);

            if (playerUnit.CrrPokemon.HP <= 0)
            {
                yield return Fainted_CR(playerUnit);
            }
            if (state == BattleState.END) yield break;

            //animation
            yield return enemyUnit.LeechSeedEffect_CR();
            yield return ShowEffectsChanges_CR(enemyUnit);

            if (enemyUnit.CrrPokemon.HP <= 0)
            {
                yield return Fainted_CR(enemyUnit);
            }

            if (state == BattleState.END) yield break;
            yield return null;
        }


        IEnumerator ShowEffectsChanges_CR(Unit poke, float readTimeInSec = 1.5f)
        {
            while (poke.CrrPokemon.StatusChangeQuote.Count > 0)
            {
                var message = poke.CrrPokemon.StatusChangeQuote.Dequeue();
                yield return dialogueBox.TypeDialougueText_CR(message, readTimeInSec);
            }
        }

        bool MoveHitCheck(Move move, Unit user, Unit target)
        {
            if (move.Base.AlwaysHits)
                return true;

            //other hit conditions


            float moveAccu = move.Base.Accuracy;

            int accu = user.StatStages[Stat.Accuracy];
            int evas = target.StatStages[Stat.Evasion];

            int precision = Mathf.Clamp(accu - evas, -6, +6);

            int hit = Mathf.FloorToInt(moveAccu * BoostTable.GetPrecision(precision));

            return (UnityEngine.Random.Range(1, 101) <= hit);
        }


        bool OddsOfEscape(int escapee, int persuer)
        {
            ++fleeAtempts;
            if (escapee < persuer)
            {
                int oddsBase = ((escapee * 128) / persuer) + 30 * fleeAtempts;

                return (oddsBase % 256) > UnityEngine.Random.Range(0, 255);
            }

            return true;
        }

        IEnumerator ShowMoveDetails_CR(DamageDetails details)//show recoil and lifeLeech here too
        {
            if (details.Critical > 1f)
                yield return dialogueBox.TypeDialougueText_CR("A critical hit!", 1.75f);

            if (details.Effectiveness > 1f)
                yield return dialogueBox.TypeDialougueText_CR("It's super effective!");

            else if (details.Effectiveness < 1f)
                yield return dialogueBox.TypeDialougueText_CR("It's not very effective.");

            else if (details.Effectiveness <= 0f)
                yield return dialogueBox.TypeDialougueText_CR("It doesn't affect...");

        }

        IEnumerator Fainted_CR(Unit fainted)
        {

            fainted.ApplyCondition(ConditionID.fnt);
            // yield return dialogueBox.TypeDialougueText_CR($"{fainted.CrrPokemon.NickName} has fainted.", 2.5f);
            yield return animations.FaintAnimation_CR(fainted);
            fainted.BHud.gameObject.SetActive(false);
            yield return ShowEffectsChanges_CR(fainted, 2);

            if (!fainted.IsPlayerOrAlly)
            {
                int exp = fainted.Species.ExpYield;
                int enemyLvl = fainted.CrrPokemon.Level;
                float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;
                Pokemon crrtPkmn = playerUnit.CrrPokemon;


                int expGain = Mathf.FloorToInt((trainerBonus * exp * enemyLvl) / 7);
                if (crrtPkmn.Level < 100)
                {
                    playerUnit.CrrPokemon.Exp += expGain;
                    yield return dialogueBox.TypeDialougueText_CR($"{playerUnit.CrrPokemon.NickName} gained {expGain} Exp. Points!");
                    yield return playerUnit.BHud.UpdateXp_CR();//redundent?
                }
                //playerUnit.CrrPokemon.HasLeveledUp1 = false;
                while (playerUnit.CrrPokemon.HasLeveledUp())
                {
                    playerUnit.BHud.UpdateLevel();
                    yield return dialogueBox.TypeDialougueText_CR($"{playerUnit.CrrPokemon.NickName} grew to Lv. {playerUnit.CrrPokemon.Level}!");

                    //try to learn new move
                    var newMove = playerUnit.CrrPokemon.GetLearnableMoveAtLevel();
                    if (newMove != null)
                    {
                        if (playerUnit.CrrPokemon.MoveSet.Count < PokemonBase.MAX_MOVES_COUNT)
                        {
                            playerUnit.CrrPokemon.LearnMove(newMove.BaseMove);
                            yield return dialogueBox.TypeDialougueText_CR($"{playerUnit.CrrPokemon.NickName} learned  {newMove.BaseMove.name}!");
                            dialogueBox.SetMoveButtons(playerUnit.CrrPokemon.MoveSet);

                        }
                        else
                        {

                            yield return dialogueBox.TypeDialougueText_CR($"{playerUnit.CrrPokemon.NickName} is trying to learn {newMove.BaseMove.MoveName}");
                            yield return dialogueBox.TypeDialougueText_CR($"But it cannot learn more than {PokemonBase.MAX_MOVES_COUNT} moves");
                            yield return SelectMoveToForget_CR(playerUnit.CrrPokemon, newMove.BaseMove);

                            yield return new WaitWhile(() => state == BattleState.FORGET_MOVE);
                        }
                    }


                    yield return playerUnit.BHud.UpdateXp_CR(true);//redundent?

                }

            }

            yield return CheckBattleResults_CR(fainted);
            yield break;


        }

        IEnumerator SelectMoveToForget_CR(Pokemon poke, MovesBase newMove)
        {
            ChangeState(BattleState.FORGET_MOVE);
            yield return dialogueBox.TypeDialougueText_CR($"Choose a move to forget", readTimeInSeconds: 2.5f);
            // dialogueBox.SetCurrentActiveMenu(forgetMoveUI.gameObject, mode: MenuMode.Forget_Move);
            dialogueBox.SetCurrentBattleScreen(forgetMoveUI,
                () =>
                {
                    dialogueBox.CloseMenus();

                    StartCoroutine(ForgetMove_CR(forgetMoveUI.SelectedIndex, newMove));

                },
                () =>
                {
                    dialogueBox.CloseMenus();
                    StartCoroutine(ForgetMove_CR(PokemonBase.MAX_MOVES_COUNT, newMove));
                //return false;
            },
                MenuMode.Forget_Move);

            /*
            Action<int> OnMoveSelection = (index) =>
            {

                dialogueBox.CloseMenus();

                StartCoroutine(ForgetMove(index, newMove));
            };
            */

            forgetMoveUI.SetForgetMenu(poke, newMove);

        }

        IEnumerator ForgetMove_CR(int index, MovesBase newMove)
        {
            //dialogueBox.CloseMenus();
            if (index == PokemonBase.MAX_MOVES_COUNT)
            {
                yield return dialogueBox.TypeDialougueText_CR($"{playerUnit.unitName} did not learn {newMove.MoveName}");
            }
            else
            {

                var selectedMove = playerUnit.CrrPokemon.MoveSet[index].Base;
                yield return dialogueBox.TypeDialougueText_CR($"{playerUnit.unitName} forgot {selectedMove.MoveName} and learned {newMove.MoveName}");

                playerUnit.CrrPokemon.MoveSet[index] = new Move(newMove);
            }
            ChangeState(BattleState.TURN_RESOLUTION);



            yield return null;
        }


        IEnumerator CheckBattleResults_CR(Unit loser)//separate logic from display of results
        {
            bool hasWon;
            if (loser == playerUnit)
            {
                var substitute = playerParty.GetHealthy();
                if (substitute != null)
                {
                    ChangeState(BattleState.SWITCHING);
                    //partyMenu.SetButtons(OnSelectFromParty, CancelButtonPartyMenu);
                    //dialogueBox.SetCurrentActiveMenu(partyMenu.gameObject, true, MenuMode.Party);
                    dialogueBox.SetCurrentBattleScreen(partyMenu, mode: MenuMode.Party, keepPrevious: true);


                    yield return new WaitUntil(() => state != BattleState.SWITCHING);
                    yield break;

                }

                //yield return dialogueBox.TypeDialougueText_CR($"You Were defeated.", 1.75f);
                hasWon = false;

            }
            else
            {
                if (isTrainerBattle)
                {
                    //var nextPoke = trainerTeam.GetHealthy();
                    var nextPoke = trainerParty.GetHealthy();
                    if (nextPoke != null)
                    {
                        yield return WhenTrainerSwitching_CR(nextPoke);
                        yield return SwitchOpponentPokemon_CR(nextPoke);
                        yield break;
                    }

                }

                // yield return dialogueBox.TypeDialougueText_CR($"Congratulations!!!",persistAfterRead: true);
                hasWon = true;
            }

            yield return new WaitForSeconds(1f);

            ChangeState(BattleState.END);
            yield return ShowBattleResult_CR(hasWon);
            yield return GameManager.Instance.FadeInToFreeRoam_CR();

            OnEndBattle(hasWon);

        }



        IEnumerator ShowBattleResult_CR(bool hasWon = false)//for implementation
        {
            if (isTrainerBattle)
            {
                if (hasWon)
                    yield return dialogueBox.TypeDialougueText_CR($"Congratulations!!! You defeated {trainer.Name}!!", persistAfterRead: true);

                else
                    yield return dialogueBox.TypeDialougueText_CR($"You Were defeated by {trainer.Name}.", 1.75f);

            }

        }


        void OnEndBattle(bool result = false)
        {
            ChangeState(BattleState.END);
            //put results here?
            playerUnit.CrrPokemon.RemoveAllVolat();
            playerUnit.RemoveSeeds();
            enemyUnit.RemoveSeeds();
            enemyUnit.ResetSprite();
            if (trainerData != null)
                trainerData.OnEndOfBattle(result);

            battlesData.Won = result;
            if (trainer != null)
                trainer.GetBattleResult(result);

            StopAllCoroutines();//does this prevent all stacked corputines to continue?
                                //Clear Battle data
                                //GameManager.Instance.EndBattleScene();

            playerObject.SetActive(false);
            enemyObject.SetActive(false);
            GameManager.Instance.EndOfBattle();

        }

        IEnumerator SwitchOpponentPokemon_CR(Pokemon next)
        {
            yield return new WaitUntil(() => state != BattleState.SWITCHING);

            enemyUnit.SetUpUnit(next);
            yield return dialogueBox.TypeDialougueText_CR($"{trainer.Name} sent out a {next.NickName}.");
            enemyUnit.unitName = "The foe's " + enemyUnit.unitName;

            ChangeState(BattleState.TURN_RESOLUTION);
        }




        IEnumerator WhenTrainerSwitching_CR(Pokemon trainerNew)
        {
            yield return dialogueBox.TypeDialougueText_CR($"{trainer.Name} is about to send {trainerNew.NickName}. Do you want to change pokemon?");

            //dialogueBox.SetQuestionToSwitch();
            dialogueBox.SetCurrentActiveMenu(choiceDoSwitchMenu, mode: MenuMode.Confirmation, onReturn: ContinueBattleFromTrainerLost);
            ChangeState(BattleState.SWITCHING);

            //make call for trainer switch

        }

        //Buttons Fuctionalities
        public void OnMoveButton()
        {
            if ((state != BattleState.DECISION_PHASE))
                return;

            dialogueBox.SetMovesMenu();

        }


        public void OnPartyButton()
        {
            if (state != BattleState.DECISION_PHASE && state != BattleState.SWITCHING)
                return;

            //partyMenu.SetSelectionAction(OnSelectFromParty);
            // partyMenu.SetButtons(OnSelectFromParty, CancelButtonPartyMenu);
            // dialogueBox.SetCurrentBattleScreen(partyMenu, OnSelectFromParty, CancelButtonPartyMenu, MenuMode.Party);
            dialogueBox.SetCurrentBattleScreen(partyMenu, mode: MenuMode.Party);
            //dialogueBox.SetCurrentActiveMenu(partyMenu.gameObject, mode: MenuMode.Party);
        }

        //OnBagSelection

        public void OnBagSelection()
        {
            if (state != BattleState.DECISION_PHASE)
                return;


            dialogueBox.SetCurrentActiveMenu(inventoryPocketSelectionMenu, mode: MenuMode.Bag,
                onReturn:
                () =>
                {
                    if (inventory && inventory.gameObject.activeSelf)
                    {
                        inventory.CancelButton();
                        return;
                    }

                    dialogueBox.ReturnToActionMenu();
                }
                );


            //dialogueBox.CloseMenus();
            //StartCoroutine(TurnResolution_CR(BattleAction.UseItem));

            //open bag menu without the need for deactivate actionsmenu
            //maybe use another scene
        }


        public void OnRunButton()
        {
            if (state != BattleState.DECISION_PHASE)
                return;

            dialogueBox.CloseMenus();
            StartCoroutine(TurnResolution_CR(BattleAction.Run));
        }






        /// <summary>
        /// Buttons of menus. 
        /// </summary>


        public void ContinueBattleFromTrainerLost()
        {

            dialogueBox.CloseMenus();
            ChangeState(BattleState.FOE_SWITCHING);
        }


        public void ChooseMoveByButton(Button mButton)
        {
            if (state != BattleState.DECISION_PHASE)
                return;

            var movesRel = dialogueBox.ButtonMove;
            if (movesRel.ContainsKey(mButton))
            {
                if (movesRel[mButton].Pp >= 1)
                {
                    playerUnit.MoveOfChoice = movesRel[mButton];
                    if (playerUnit.MoveOfChoice.Base.HasChargingTurn)
                        playerUnit.MoveOfChoice.isCharged = false;
                    animations.SetMoveAnimation(playerUnit.MoveOfChoice, playerUnit);
                    dialogueBox.CloseMenus();
                    StartCoroutine(TurnResolution_CR(BattleAction.Moves));
                }
            }
        }


        public void ChooseMoveByIndex()
        {
            if (state != BattleState.DECISION_PHASE)
                return;

            var move = playerUnit.CrrPokemon.MoveSet[EventSystem.current.currentSelectedGameObject.transform.GetSiblingIndex()];

            if (move.Pp >= 1)
            {
                playerUnit.MoveOfChoice = move;
                if (playerUnit.MoveOfChoice.Base.HasChargingTurn)
                    playerUnit.MoveOfChoice.isCharged = false;
                animations.SetMoveAnimation(playerUnit.MoveOfChoice, playerUnit);
                dialogueBox.CloseMenus();
                StartCoroutine(TurnResolution_CR(BattleAction.Moves));

            }
        }

        public void OnSelectFromParty()
        {
            if (partyMenu.SelectedPokemon.HP <= 0 || partyMenu.SelectedPokemon == null || playerUnit.CrrPokemon == partyMenu.SelectedPokemon)
                return;

            //dialogueBox.subMenu = SubMenu.Confirmation;
            if (partyMenu.BattleConfirmMenu != null)
            {
                dialogueBox.SetCurrentActiveMenu(partyMenu.BattleConfirmMenu, true);
                return;
            }
            dialogueBox.SetCurrentActiveMenu(partyMenu.ConfirmMenu, true);

        }

        public void SwitchButton()
        {
            if (state != BattleState.SWITCHING && state != BattleState.DECISION_PHASE && partyMenu.SelectedPokemon == null)
                return;

            if (partyMenu.SelectedPokemon.HP <= 0)
                return;

            dialogueBox.CloseMenus();
            if (state == BattleState.SWITCHING)
            {
                StartCoroutine(SwitchPokemonUnit_CR(partyMenu.SelectedPokemon));//change to decision phase

                return;
            }
            StartCoroutine(TurnResolution_CR(BattleAction.Switch));
        }

        public void CancelButtonPartyMenu()
        {
            if (playerUnit.CrrPokemon.HP <= 0)
                return;

            if (state == BattleState.DECISION_PHASE)
            {

                dialogueBox.ReturnToActionMenu();
                return;
            }

            ContinueBattleFromTrainerLost();
            // return false;
        }

        public void BagSelectionButton()
        {
            int tabSelected = EventSystem.current.currentSelectedGameObject.transform.GetSiblingIndex();
            //Debug.Log(tabSelected);
            inventoryPocketSelectionMenu.SetActive(false);
            inventory.Open(
                () =>
                {

                    if (inventory.CheckIfPkball())
                    {
                        StartCoroutine(UseItem_CR());
                        return;
                    }

                    inventory.CrrntItemListEnabaler(false);
                    inventory.PartyMenuSelection.Open(
                        () =>
                        {
                        //check if pp restore
                        StartCoroutine(UseItem_CR());
                        }
                        );
                }
                ,
                () =>
                {
                    inventory.gameObject.SetActive(false);


                    inventoryPocketSelectionMenu.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(inventoryPocketSelectionMenu.GetComponentInChildren<Button>().gameObject);
                    return;

                //inventoryPocketSelectionMenu.SetActive(true);
                //EventSystem.current.SetSelectedGameObject(inventoryPocketSelectionMenu.GetComponentInChildren<Button>().gameObject);
                //return;
            },
                tabSelected);

        }



        IEnumerator UseItem_CR()
        {


            var itemUsed = inventory.UseItem_OnBattle();

            if (!itemUsed)
            {

                yield return inventory.UnusableWarnning_CR();
                yield break;
            }
            dialogueBox.CloseMenus();

            //play animations for med itens etc
            if (inventory.CheckIfPkball())
            {
                //inventory.gameObject.SetActive(false);
                yield return ThrowPokeBall_CR(itemUsed as PokeBallItem);

            }
            else
            {
                inventory.PartyMenuSelection.gameObject.SetActive(false);
                inventory.CrrntItemListEnabaler(true);
                inventory.gameObject.SetActive(false);
                //play Animation
                yield return playerHUD.WaitHpUpdat_CR();
                yield return dialogueBox.TypeDialougueText_CR($"Used {itemUsed.name} on {inventory.PartyMenuSelection.SelectedPokemon.NickName}.");
            }


            StartCoroutine(TurnResolution_CR(BattleAction.UseItem));

        }


        IEnumerator ThrowPokeBall_CR(PokeBallItem pkball)// link to use pkball
        {

            if (isTrainerBattle)
            {
                yield return dialogueBox.TypeDialougueText_CR("You can't steal another trainer Pokemon!");
                ChangeState(BattleState.TURN_RESOLUTION);
                yield break;
            }


            ChangeState(BattleState.CATCHING);
            yield return dialogueBox.TypeDialougueText_CR($"{playerParty.TrainerName} used a {pkball.Name.ToUpper()}.");
            var ball = Instantiate(pokeBall, transform);
            var ballAni = ball.GetComponent<Animator>();
            yield return null;
            yield return new WaitForSeconds(ballAni.GetCurrentAnimatorStateInfo(0).length);
            //var ballAni = ;
            //Debug.Log(ballAni.runtimeAnimatorController.name);


            yield return animations.PlayThrowAnimation_CR(ballAni, enemyUnit);
            int shakeCount = TryToCatch(enemyUnit.CrrPokemon, pkball);


            Debug.Log("Shake count: " + shakeCount);
            for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
            {
                Debug.Log("Cycle " + i);
                yield return new WaitForSeconds(.5f);
                yield return animations.PlayShakeAnimation_CR(ballAni);
            }

            if (shakeCount == 4)
            {
                yield return animations.PlayCatchSuccess_CR(ballAni);
                yield return dialogueBox.TypeDialougueText_CR($"{enemyUnit.CrrPokemon.Species} was caught!");
                //add pokemmon to party or PC
                if (playerParty.GetSize() < 6)
                {
                    playerParty.AddToParty(enemyUnit.CrrPokemon);
                    yield return dialogueBox.TypeDialougueText_CR($"{enemyUnit.CrrPokemon.Species} has been added to the party.");

                }
                //else adds to PC

                Destroy(ball);
                yield return new WaitForSeconds(1f);
                yield return GameManager.Instance.FadeInToFreeRoam_CR();
                OnEndBattle(true);

            }
            else
            {
                yield return new WaitForSeconds(1f);
                yield return animations.PlayCatchFail_CR(ballAni, enemyUnit);

                if (shakeCount == 0)
                    yield return dialogueBox.TypeDialougueText_CR($"Oh, no! The Pokémon broke free!");
                else if (shakeCount == 1)
                    yield return dialogueBox.TypeDialougueText_CR($"Aww! It appeared to be caught!");
                else if (shakeCount == 2)
                    yield return dialogueBox.TypeDialougueText_CR($"Aargh! Almost had it!");
                else
                    yield return dialogueBox.TypeDialougueText_CR($"Shoot! It was so close, too!");

                Destroy(ball);
                ChangeState(BattleState.TURN_RESOLUTION);
            }

        }


        int TryToCatch(Pokemon pke, PokeBallItem ball)
        {
            float a = (3 * pke.MaxHP - 2 * pke.HP) * pke.Base.CatchRate * ball.GetCatchRateMod(pke) * ConditionsDB.GetCatchRateMod(pke.Status) / (3 * pke.MaxHP);

            if (a >= byte.MaxValue)
                return 4;

            float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

            int shakeCount = 0;
            while (shakeCount < 4)
            {
                if (UnityEngine.Random.Range(0, 65535) >= b)
                    break;

                ++shakeCount;
            }
            return shakeCount;
        }



    }

}
