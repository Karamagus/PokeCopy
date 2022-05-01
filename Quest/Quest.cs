using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace pokeCopy
{
    [Serializable]
    public class Quest
    {
        public QuestBase Base { get; private set; }
        public QuestState State { get; private set; }

        public Quest(QuestBase qBase)
        {
            Base = qBase;
        }

        public Quest(QuestSaveData saveData)
        {
            Base = QuestDB.GetDataByName(saveData.name);
            State = saveData.state;
        }

        public QuestSaveData GetSaveData()
        {
            var saveData = new QuestSaveData()
            {
                name = Base.name,
                state = State,
            };

            return saveData;
        }

        

        public IEnumerator StartQuest_CR()
        {
            State = QuestState.Started;

            yield return DialogueCacheManager.I.ShowDialogue_CR(Base.StartDialogue);

            var questManager = QuestLogManager.GetQuestManager();
            questManager.AddQuest(this);

        }

        public IEnumerator CompleteQuest_CR(Transform player)
        {
            State = QuestState.Finished;

            yield return DialogueCacheManager.I.ShowDialogue_CR(Base.CompletedDialogue);

            var invetory = Inventory.GetPlayerInventory();
            if (Base.RequiredItem.Count > 0)
            {
                foreach (var item in Base.RequiredItem)
                {
                    if (item == null) continue;
                    invetory.RemoveItemFromStack(item);
                }

            }

            if (Base.RewardItem.Count > 0)
            {
                foreach (var item in Base.RewardItem)
                {
                    if (item == null) continue;
                    invetory.AddItemStack(item);


                    string playerName = player.GetComponent<PlayerMovement>().Name;
                    string article = (item.Amount == 1) ? "a " : "";
                    string plural = (item.Amount > 1) ? "s" : "";
                    yield return DialogueCacheManager.I.ShowText_CR($"{playerName} received {article}{item.Item.name}{plural}.");
                }
            }

            var questManager = QuestLogManager.GetQuestManager();
            questManager.AddQuest(this);
        }

        public bool CanBeCompleted()
        {
            var invetory = Inventory.GetPlayerInventory();
            var hasItem = true;
            if (Base.RequiredItem.Count > 0)
            {
                foreach (var item in Base.RequiredItem)
                {
                    if (item == null || item.Item == null) continue;
                    hasItem &= invetory.HasItem(item);
                }
            }
            return hasItem;
        }



    }

    [System.Serializable]
    public class QuestSaveData
    {
        public string name; 
        public QuestState state;

    }




}

public enum QuestState
{
    None, Started, Finished
}
