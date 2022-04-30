using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace pokeCopy
{
    public class ItemSlotUI : MonoBehaviour
    {
        [SerializeField] Text nameText;
        [SerializeField] Text amountText;

        Item item;
        [SerializeField] RectTransform rectTransf;

        public Item Item => item;
        public float Height => rectTransf.rect.height;

        public void Awake()
        {
            rectTransf = GetComponent<RectTransform>();
        }

        public void SetData<T>(ItemStack<T> itemStack) where T : Item
        {
            // Debug.Log(itemStack);
            item = itemStack.Item;
            nameText.text = Item.name;
            amountText.text = $"X {itemStack.Amount}";
        }
        public void SetData(object itemStack)
        {
            dynamic stack = Convert.ChangeType(itemStack, itemStack.GetType());
            //Debug.Log(itemStack);
            item = stack.Item;
            nameText.text = Item.name;
            amountText.text = $"X {stack.Amount}";


        }

    }
}
