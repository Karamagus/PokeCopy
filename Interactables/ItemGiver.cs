using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class ItemGiver : MonoBehaviour, ISavable
    {
        [SerializeField] ItemStack stack;
        [SerializeField] Dialogue dialogue;


        bool gotItem = false;



        public IEnumerator GiveItem(PlayerMovement player)
        {
            yield return DialogueCacheManager.I.ShowDialogue(dialogue);

            player.GetComponent<Inventory>().AddItemStack(stack);


            gotItem = true;

            var text = $"{player.Name} got {stack.Amount} {stack.Item.Name}";
            if (stack.Amount > 1)
                text += "s";
            text += ".";

            yield return DialogueCacheManager.I.ShowText(text);
        }




        public bool CanGive()
        {
            return stack != null && stack.Item != null && !gotItem;
        }

        public object CaptureState()
        {
            return gotItem;
        }

        public void RestoreState(object state)
        {
            gotItem = (bool)state;
        }
    }
}
