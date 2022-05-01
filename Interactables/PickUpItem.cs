using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class PickUpItem : MonoBehaviour, IInteractable, ISavable
    {
        [SerializeField] ItemStack item;
        public bool WasPickedUp { get; set; } = false;


        public IEnumerator Interact_CR(Transform initiator = null)
        {
           // Debug.Log($"Interacted with {this.name}.");

            yield return null;
            if (!WasPickedUp)
            {
                WasPickedUp = true;
                initiator.GetComponent<Inventory>().AddItemStack(item);
                

                var plural = (item.Amount > 1) ? "s" : "";
                var playerName = initiator.GetComponent<PlayerMovement>().Name;

                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<BoxCollider2D>().enabled = false;

                yield return DialogueCacheManager.I.ShowText_CR($"{playerName} found {item.Amount} {item.Item.Name}{plural}");



                //gameObject.SetActive(false);

            }
            //add item to its respective invetory slot

        }
        public object CaptureState()
        {
            return WasPickedUp;
        }

        public void RestoreState(object state)
        {
            WasPickedUp = (bool)state;

            if (WasPickedUp)
            {
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<BoxCollider2D>().enabled = false;

                // gameObject.SetActive(false);
            }

        }
    }
}
