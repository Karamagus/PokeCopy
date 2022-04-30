using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    [CreateAssetMenu(menuName = "Quest/Create new Quest")]
    public class QuestBase : ScriptableObject
    {
        [SerializeField] string Name;
        [TextArea(3,6)]
        [SerializeField] string Description;

        [SerializeField] Dialogue startDialogue;
        [SerializeField] Dialogue inProgressDialogue;
        [SerializeField] Dialogue completedDialogue;


        [SerializeField] List<ItemStack> requiredItem;
        [SerializeField] List<ItemStack> rewardItem;

        public Dialogue StartDialogue  => startDialogue; 
        public Dialogue InProgressDialogue  => inProgressDialogue?.senteces?.Length > 0 ? inProgressDialogue: startDialogue; 
        public Dialogue CompletedDialogue  => completedDialogue; 
        public List<ItemStack> RequiredItem => requiredItem; 
        public List<ItemStack> RewardItem  => rewardItem; 
        public string Description1  => Description;

    }
}
