using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class NPCMovement : MonoBehaviour, IInteractable, ISavable
    {
        [Header("Quests")]
        [SerializeField] QuestBase questToStart;
        [SerializeField] QuestBase questToComplete;
        [Space]
        [SerializeField] Dialogue dialogue;
        [SerializeField] List<Vector2> routePoints;
        [SerializeField] float patternsInterval = 2f;


        Quest activeQuest;

        NPCState state;
        bool isInteracting;
        float idleTimer;
        int routeIndx;

        CharMovement movement;
        ItemGiver giver;
        PokemonGiver pokemonGiver;

        private void Awake()
        {
            movement = GetComponentInChildren<CharMovement>();
            giver = GetComponentInChildren<ItemGiver>();
            pokemonGiver = GetComponentInChildren<PokemonGiver>();
            AdjustRoute();

        }

        public void Start()
        {
        }

        void AdjustRoute()
        {
            for (int i = 0; i < routePoints.Count; i++)
            {
                if (routePoints[i].x != 0 & routePoints[i].y != 0)
                {
                    routePoints.Insert(i + 1, new Vector2(0, routePoints[i].y));
                    routePoints[i] = new Vector2(routePoints[i].x, 0);
                }
            }

        }

        public IEnumerator Interact_CR(Transform initiator)
        {
            if (state == NPCState.idle && GameManager.Instance.State == GameState.freeRoam)
            {

                if (questToComplete != null)
                {
                    var quest = new Quest(questToComplete);
                    yield return quest.CompleteQuest_CR(initiator);
                    questToComplete = null;

                    Debug.Log($"{quest.Base.name} completed.");

                }

                //isInteracting = true;
                state = NPCState.dialogue;
                movement.LookTowrds(initiator.position);

                if (giver != null && giver.CanGive())
                {
                    yield return giver.GiveItem_CR(initiator.GetComponent<PlayerMovement>());
                }
                else if (pokemonGiver != null && pokemonGiver.CanGive())
                {
                    yield return pokemonGiver.GivePokemon_CR(initiator.GetComponent<PlayerMovement>());
                }
                else if (questToStart != null)
                {
                    activeQuest = new Quest(questToStart);
                    yield return activeQuest.StartQuest_CR();
                    questToStart = null;

                    if (activeQuest.CanBeCompleted())
                    {
                        yield return activeQuest.CompleteQuest_CR(initiator);
                        activeQuest = null;
                    }

                }
                else if (activeQuest != null)
                {
                    if (activeQuest.CanBeCompleted())
                    {
                        yield return activeQuest.CompleteQuest_CR(initiator);
                        activeQuest = null;
                    }
                    else
                    {
                        yield return DialogueCacheManager.I.ShowDialogue_CR(activeQuest.Base.InProgressDialogue);
                    }

                }
                else
                {

                    yield return DialogueCacheManager.I.ShowDialogue_CR(dialogue);
                }





                //DialogueCacheManager.I.OnCloseDialogue += ClearInteraction;
                idleTimer = 0f;
                state = NPCState.idle;


            }
            //StartCoroutine(movement.Move(new Vector2(0, 1)));
        }

        void ClearInteraction()
        {
            // isInteracting = false;
            //  Debug.Log("CLEAR INTERACTION");
            //DialogueCacheManager.I.OnCloseDialogue -= ClearInteraction;
        }

        public void Update()
        {
            if (isInteracting || state == NPCState.interacting)
                return;


            if (state == NPCState.idle)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer > patternsInterval)
                {
                    idleTimer = 0f;
                    if (routePoints.Count > 0)
                        StartCoroutine(WalkRoute_CR());

                }
            }

        }

        IEnumerator WalkRoute_CR()
        {
            state = NPCState.walking;

            var oldPos = transform.position;

            yield return movement.Move(routePoints[routeIndx]);

            if (transform.position != oldPos)
                routeIndx = (routeIndx + 1) % routePoints.Count;

            state = NPCState.idle;
        }

        public object CaptureState()
        {
            var saveData = new NPCQuestSavaData();
            saveData.activeQuest = activeQuest?.GetSaveData();

            saveData.toStartQuest = (questToStart != null) ? questToStart.name : null;

            saveData.toFinishQuest = (questToComplete != null) ? questToComplete.name : null;

            return saveData;

        }

        public void RestoreState(object state)
        {
            var saveData = state as NPCQuestSavaData;
            if (saveData != null)
            {
                activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;

                questToStart = (saveData.toStartQuest != null || saveData.toStartQuest.Length > 0) ? QuestDB.GetDataByName(saveData.toStartQuest) : null;
                questToComplete = (saveData.toFinishQuest != null || saveData.toFinishQuest.Length > 0) ? QuestDB.GetDataByName(saveData.toFinishQuest) : null;

            }

        }
    }


    public class NPCQuestSavaData
    {
        public QuestSaveData activeQuest;

        public string toStartQuest;
        public string toFinishQuest;
    }


    public enum NPCState
    {
        idle, walking, interacting, dialogue
    }

}
