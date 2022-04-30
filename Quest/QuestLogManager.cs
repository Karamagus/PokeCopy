using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace pokeCopy
{
    public class QuestLogManager : MonoBehaviour, ISavable
    {
        List<Quest> questLog = new List<Quest>();


        public event System.Action OnUpdate;

        public void AddQuest(Quest quest)
        {
            if (!questLog.Contains(quest))
                questLog.Add(quest);

            OnUpdate?.Invoke();
        }

        public static QuestLogManager GetQuestManager(PlayerMovement player = null)
        {
            if (player == null)
                return GameManager.Instance.Player.GetComponent<QuestLogManager>();

            return player.GetComponent<QuestLogManager>();
        }

        public bool HasStarted(string questName)
        {
            var questStatus = questLog.FirstOrDefault(q => q.Base.name == questName)?.State;
            return questStatus == QuestState.Started;
        }
        public bool HasCompleted(string questName)
        {
            var questStatus = questLog.FirstOrDefault(q => q.Base.name == questName)?.State;
            return questStatus == QuestState.Finished;
        }

        public object CaptureState()
        {
            return questLog.Select(q => q.GetSaveData()).ToList();
        }

        public void RestoreState(object state)
        {
            var saveData = state as List<QuestSaveData>;

            if (saveData != null)
            {
                questLog = saveData.Select(q => new Quest(q)).ToList();

                OnUpdate?.Invoke();
            }

        }
    }




}
