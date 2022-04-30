using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class QuestObserver : MonoBehaviour
    {
        [SerializeField] List<QuestBase> questsToCheck = new List<QuestBase>();
        [SerializeField] ObjectQuestReaction onStart;
        [SerializeField] ObjectQuestReaction onComplete;

        QuestLogManager questLog;


        private void Start()
        {
            questLog = QuestLogManager.GetQuestManager();
            questLog.OnUpdate += UpdateObjectOnQuestChange;


            UpdateObjectOnQuestChange();
        }

        private void OnDestroy()
        {
            questLog.OnUpdate -= UpdateObjectOnQuestChange;
        }

        public void UpdateObjectOnQuestChange()
        {
            bool started = true;
            foreach (QuestBase quest in questsToCheck)
                started &= questLog.HasStarted(quest.name);


            if (onStart != ObjectQuestReaction.None && started)
            {
                foreach (Transform child in transform)
                {
                    if (onStart == ObjectQuestReaction.Enable)
                    {
                        child.gameObject.SetActive(true);

                        var savable = GetComponent<SavableEntity>();
                        if (savable != null)
                            SavingSystem.i.RestoreEntity(savable);

                    }
                    else if (onStart == ObjectQuestReaction.Disable)
                        child.gameObject.SetActive(false);

                }
            }

            bool completed = true;
            foreach (QuestBase quest in questsToCheck)
                completed &= questLog.HasCompleted(quest.name);

            if (onComplete != ObjectQuestReaction.None && completed)
            {
                foreach (Transform child in transform)
                {
                    if (onComplete == ObjectQuestReaction.Enable)
                    {
                        child.gameObject.SetActive(true);

                        var savable = GetComponent<SavableEntity>();
                        if (savable != null)
                            SavingSystem.i.RestoreEntity(savable);

                    }
                    else if (onComplete == ObjectQuestReaction.Disable)
                        child.gameObject.SetActive(false);

                }
            }

        }


    }



    public enum ObjectQuestReaction
    {
        None, Enable, Disable, change
    }
}
