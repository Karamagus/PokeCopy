using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace pokeCopy
{
    public class Inventory : MonoBehaviour, ISavable //   TODO: Add implementation to addition to itens
    {

        [SerializeField] List<ItemStack<MedicineItem>> medicine;
        [SerializeField] List<ItemStack<PokeBallItem>> pokeBalls;
        [SerializeField] List<ItemStack<TmAndHMItem>> tm_and_hm;

        public Action<int, int> OnTabUpdate;
        public Action<int, int> OnStackUpdate;

        // public List<ItemStack<MedicineItem>> Medicine { get { return allItens.Where(i =>  i.GetType() == typeof(ItemStack<MedicineItem>)).Cast<ItemStack<MedicineItem>>().ToList(); } }
        public List<List<ItemStack>> AllItens { get => allItens; }
        public List<List<object>> PocketRef => pocketRef;

        [SerializeField] List<List<ItemStack>> allItens;
        List<List<object>> pocketRef;
        //List<IList<Item>> allItens;
        // List<IList> allItens;

        Dictionary<Type, List<ItemStack>> categoryType;

        public static List<string> itemCategory = new List<string>() { "MEDICINE", "POKEBALLS", "TMs & HMs" };
        public static List<Type> itemTypes = new List<Type> { typeof(MedicineItem), typeof(PokeBallItem), typeof(TmAndHMItem) };

        public void Awake()
        {
            categoryType = new Dictionary<Type, List<ItemStack>>();
            allItens = new List<List<ItemStack>>() {
               medicine.ConvertAll( new Converter<ItemStack<MedicineItem>, ItemStack>(m => (ItemStack)m) ),
               pokeBalls.ConvertAll( new Converter<ItemStack<PokeBallItem>, ItemStack>(p => (ItemStack)p) ),
               tm_and_hm.ConvertAll( new Converter<ItemStack<TmAndHMItem>, ItemStack>(p => (ItemStack)p) )
            };



            /*
            categoryType = new Dictionary<Type, List<ItemStack>>()
            {
                { typeof(MedicineItem), allItens[0]},
                { typeof(PokeBallItem), allItens[1]},
                { typeof(TmAndHMItem), allItens[2]},
            }

            pocketRef = new List<List<object>>() {
                (medicine as IEnumerable<object>).Cast<object>().ToList(),
                (pokeBalls as IEnumerable<object>).Cast<object>().ToList(),
                (tm_and_hm as IEnumerable<object>).Cast<object>().ToList()
            };
            //allItens = new List<IList<Item>>() { medicine, pokeBalls };
            */
        }

        private void Start()
        {
            if (itemTypes.Count < allItens.Count)
                throw new IndexOutOfRangeException($"There are {allItens.Count} pockets but only { itemTypes.Count} Item Types are defined. \n Please check the lists sizes");


            for (int i = 0; i < allItens.Count; i++)
                categoryType.Add(itemTypes[i], allItens[i]);

        }

        public Item UseItem(int pocket, int itemIndex, Pokemon selectedPokemon)
        {
            var stack = allItens[pocket][itemIndex];
            var item = stack.Item;
            if (item.IsUsed(selectedPokemon))
            {
                ConsumeFromStack(stack, pocket);
                return item;

            }
            return null;
        }

        public Item UseItem(int pocket, int itemIndex, Pokemon selectedPokemon, int moveIndex = -1)
        {
            var stack = allItens[pocket][itemIndex];
            var item = stack.Item;
            if (item.IsUsed(selectedPokemon, moveIndex))
            {

                ConsumeFromStack(stack, pocket);
                return item;

            }
            return null;
        }




        public Item UseItem(ItemStack stack, Pokemon selectedPokemon, int moveIndex = -1)
        {
            var item = stack.Item;
            var pocket = -1;
            foreach (var p in allItens)
            {
                if (p.Contains(stack))
                    pocket = allItens.IndexOf(p);
            }

            if (item.IsUsed(selectedPokemon, moveIndex) && pocket > -1)
            {
                ConsumeFromStack(stack, pocket);

                return item;
            }
            return null;
        }

        void ConsumeFromStack(ItemStack itemStack, int pocket)
        {
            if (itemStack.IsAllConsumed())
            {
                allItens[pocket].Remove(itemStack);
                OnTabUpdate?.Invoke(pocket, 0);
                return;
            }
            OnStackUpdate?.Invoke(pocket, allItens[pocket].IndexOf(itemStack));

        }

        public Item GetItemFromCategory(int itemIndex, int pocketIndex)
        {
            var currentSlot = AllItens[pocketIndex];
            return currentSlot[itemIndex].Item;
        }

        public void AddItem(Item item)
        {
            //check which item pocket
            //check if there is a stack for the item
            //  if yes increase amount on stack on inventory and return true
            //  else add a new itemStack of the item and return true
            //update inventory UI
        }

        public bool AddItemStack(ItemStack item)
        {
            var p = categoryType[item.Item.GetType()];


            //Debug.Log($"{p == null}");


            var stack = p.FirstOrDefault(i => i.Item == item.Item);



            if (stack != null)
            {
                stack.AddedToStack(item.Amount);
                // Debug.Log(allItens.IndexOf(p));
                // Debug.Log(p.IndexOf(stack));
                OnStackUpdate?.Invoke(allItens.IndexOf(p), p.IndexOf(stack));
                return true;
            }

            p.Add(item);
            OnTabUpdate?.Invoke(allItens.IndexOf(p), p.IndexOf(item));
            return true;


        }

        public void RemoveItemFromStack(ItemStack item)
        {
            var p = categoryType[item.Item.GetType()];
            var stack = p.FirstOrDefault(i => i.Item == item.Item);

            if (stack != null)
            {
                if (stack.IsAllConsumed(item.Amount))
                {

                    p.Remove(stack);
                    OnTabUpdate?.Invoke(allItens.IndexOf(p), p.IndexOf(item));
                    return;
                }
                OnStackUpdate?.Invoke(allItens.IndexOf(p), p.IndexOf(stack));
            }

        }

        public bool HasItem(ItemStack item)
        {
            var p = categoryType[item.Item.GetType()];
            var stack = p.FirstOrDefault(i => i.Item == item.Item);

            if (stack != null) 
                return stack.Amount >= item.Amount;

            return false;
        }
        /*
        public List<ItemStack<T>> GetItemStacksByCategory<T>(int index) where T : Item
        {

            return (List<ItemStack<T>>)allItens[index];
        }
        */
        public List<ItemStack> GetItemStacksByCategory<T>(List<ItemStack<T>> itemStack) where T : Item
        {
            List<ItemStack> list = new List<ItemStack>();
            foreach (var i in itemStack)
            {
                list.Add((ItemStack)i);
            }

            return list;
        }

        public object GetObjectByCategory(int index)
        {
            return allItens[index];

        }

        public static Inventory GetPlayerInventory(PlayerMovement player = null)
        {
            if (player != null)
                return player.GetComponent<Inventory>();

            return GameManager.Instance.Player.GetComponent<Inventory>();
        }




        public object CaptureState()
        {
            var saved = new InventorySavaData()
            {
                medicine = allItens[0].Select(i => i.GetSaveData()).ToList(),
                pokeballs = allItens[1].Select(i => i.GetSaveData()).ToList(),
                tmNhm = allItens[2].Select(i => i.GetSaveData()).ToList(),
            };

            return saved;
        }

        public void RestoreState(object state)
        {
            var saveData = state as InventorySavaData;
            //allItens.Clear();
            allItens = new List<List<ItemStack>>() {
                saveData.medicine.Select(i => new ItemStack(i)).ToList(),
                saveData.pokeballs.Select(i => new ItemStack(i)).ToList(),
                saveData.tmNhm.Select(i => new ItemStack(i)).ToList(),
            };


            categoryType = new Dictionary<Type, List<ItemStack>>();

            if (itemTypes.Count < allItens.Count)
                throw new IndexOutOfRangeException($"There are {allItens.Count} pockets but only { itemTypes.Count} Item Types are defined. \n Please check the lists sizes");

            for (int i = 0; i < allItens.Count; i++)
                categoryType.Add(itemTypes[i], allItens[i]);
            for (int i = 0; i < allItens.Count; i++)
                OnTabUpdate?.Invoke(i, 0);

        }
    }


    [System.Serializable]
    public class ItemStack<T> : ISerializationCallbackReceiver where T : Item
    {

        public ItemStack(int amount, T i)
        {
            item = i;
            this.amount = amount;
        }

        public static explicit operator ItemStack(ItemStack<T> i)
        {
            var item = i.item;
            return new ItemStack(item, i.amount);
        }

        [SerializeField] T item;
        [SerializeField] int amount;
        public T Item => item;
        public int Amount { get => amount; private set => amount = Mathf.Clamp(value, 1, 99); }

        void OnValidate()
        {
            Amount = amount;
        }


        public void OnBeforeSerialize()
        {
            OnValidate();

        }
        public void OnAfterDeserialize() { }

    }

    [Serializable]
    public class ItemStack : ISerializationCallbackReceiver
    {
        public ItemStack(Item item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }

        public ItemStack CreateInstance<T>(ItemStack<T> i) where T : Item
        {
            var it = i.Item;
            return new ItemStack(it, i.Amount);
        }

        public ItemStack<T> CreateTypedStack<T>() where T : Item
        {
            if (!(item is T))
            {
                return null;

            }
            var stack = new ItemStack<T>(amount, (T)item);
            return stack;

        }

        [SerializeField] Item item;
        [SerializeField] int amount;
        const int MAX_STACK = 999;

        public ItemStack(ItemSaveData saveData)
        {
            item = ItemDB.GetDataByName(saveData.name);
            amount = saveData.amount;
        }
        public ItemSaveData GetSaveData()
        {
            var saveData = new ItemSaveData()
            {
                amount = this.amount,
                name = item.name,
            };

            return saveData;
        }

        public Item Item => item;
        public int Amount { get => amount; private set => amount = Mathf.Clamp(value, 1, MAX_STACK); }




        public bool IsAllConsumed(int amountConsumed = 1)
        {
            var consumption = amount - Mathf.Abs(amountConsumed);
            if (consumption == 0)
                return true;

            amount = (consumption > 0) ? consumption : amount;
            return false;
        }

        public bool AddedToStack(int amountAdded)
        {
            if (amountAdded + Amount <= MAX_STACK && amountAdded > 0)
            {
                Amount += amountAdded;
                return true;

            }

            return false;
        }


        public bool ChangeAmount(int amountChanged)
        {
            if (amountChanged < 0)
                return IsAllConsumed(amountChanged);

            return AddedToStack(amountChanged);

        }
        void OnValidate()
        {
            Amount = amount;
        }


        public void OnBeforeSerialize()
        {
            OnValidate();

        }
        public void OnAfterDeserialize() { }

    }


    [Serializable]
    public class ItemSaveData
    {
        public string name;
        public int amount;
    }

    [Serializable]
    public class InventorySavaData
    {
        public List<ItemSaveData> medicine;
        public List<ItemSaveData> pokeballs;
        public List<ItemSaveData> tmNhm;
    }

}


