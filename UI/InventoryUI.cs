using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using System.Linq;

namespace pokeCopy
{
    public class InventoryUI : MonoBehaviour, IGameScreen
    {
        [SerializeField] Text pocketName;
        [SerializeField] GameObject ItemListBase;
        [SerializeField] ItemSlotButtonData itemSlotUi;
        [SerializeField] Text description;
        [SerializeField] Image icon;
        [Header("Submenus")]
        [SerializeField] GameObject selectedUseMenu;
        [SerializeField] GameObject selectedMenu;
        [SerializeField] PartyScreenUI partyMenuSelection;
        [SerializeField] Text cancelCloseButtonText;
        [SerializeField] ForgetMenuSubMenuUI forgetMoveUI;
        ColorBlock normalButtonColor;
        ColorBlock selectedButtonColors;

        PlayerInputActions inputs;
        InputSystemUIInputModule input;

        int currentTabIndex;
        [SerializeField] bool isTabFixed;
        List<int> selectionIndexesByTab = new List<int>();
        ItemStack selectedStack;
        Move selectedMove;

        private Action onSelection;
        private Action onCancelButton;

        Inventory inventory;
        [SerializeField] List<GameObject> tabs = new List<GameObject>(Inventory.itemCategory.Count);
        [SerializeField] List<GameObject> tabIcons;

        ScrollRect scrollView;
        const int itemsInViewport = 8;
        RectTransform itemListRect;
        float itemListSpacing;
        float topPadding;


        public Action OnCancelButton { get => onCancelButton; set => onCancelButton = value; }
        public PartyScreenUI PartyMenuSelection => partyMenuSelection;

        public Inventory Inventory => inventory;


        public void Awake()
        {
            inventory = Inventory.GetPlayerInventory();

            itemListRect = ItemListBase.GetComponent<RectTransform>();
            itemListSpacing = ItemListBase.GetComponent<VerticalLayoutGroup>().spacing;
            topPadding = itemListRect.localPosition.y;
            scrollView = GetComponentInChildren<ScrollRect>();

        }

        public void Start()
        {

            normalButtonColor = itemSlotUi.GetComponent<Button>().colors;
            selectedButtonColors = normalButtonColor;
            selectedButtonColors.normalColor = normalButtonColor.selectedColor;
            StartCoroutine(SetUpItemTabs_CR());
            inventory.OnTabUpdate += UpdateTabOnUse;
            inventory.OnStackUpdate += UpdateStackOnUse;
            //UpdateItemTabs();


        }

        public void OnEnable()
        {
            inputs = GameManager.Instance.Inputs;
            input = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (tabIcons.Count > 0 && tabIcons.Count > currentTabIndex && tabIcons[currentTabIndex])
                tabIcons[currentTabIndex].SetActive(true);
            pocketName.text = Inventory.itemCategory[currentTabIndex];
        }

        public void Open(Action onSelectionAction, Action onCancelAction, int fixedTabIndex = -1)
        {
            SetButtons(onSelectionAction, onCancelAction);
            //onSelection = onSelectionAction;
            //onCancelButton = onCancelAction;
            gameObject.SetActive(true);

            if (fixedTabIndex > -1 && fixedTabIndex < inventory.AllItens.Count)
            {
                isTabFixed = true;
            }
            else
                isTabFixed = false;

            if (selectionIndexesByTab.Count > 0)
            {
                for (int i = 0; i < inventory.AllItens.Count; i++)
                    UpdateTabOnUse(i);


            }
            if (isTabFixed)
            {
                tabs[currentTabIndex].SetActive(false);
                StartCoroutine(ChangeToTab_CR(fixedTabIndex));
            }
            HandleUpdate();
        }
        public void FixTab(int tab)
        {
            isTabFixed = true;
            currentTabIndex = tab;

        }

        public void SetButtons(Action onSelectionAction, Action onCancelAction)
        {
            if (onSelectionAction != null)
                onSelection = onSelectionAction;
            if (onCancelAction != null)
                onCancelButton = onCancelAction;

        }

        public void Open(Action onSelectionAction, Action onCancelAction)
        {
            Open(onSelectionAction, onCancelAction, -1);
        }


        public void Update()
        {
            //Debug.Log($"party menu: {!partyMenuSelection.gameObject.activeSelf}, {!selectedMenu || !selectedMenu.activeSelf}");
            //

            //if (input.move.action.ReadValue<>
            //  Debug.Log($"{inputs.UI.Navigate.ReadValue<Vector2>()}");

            // Debug.Log($"{(!partyMenuSelection.gameObject.activeSelf && (!selectedMenu || !selectedMenu.activeSelf)) && inputs.UI.Navigate.ReadValue<Vector2>() != Vector2.zero}");

            //clean this up
            if (!partyMenuSelection.gameObject.activeSelf && (selectedUseMenu == null || !selectedUseMenu.activeSelf) && inputs.UI.Navigate.ReadValue<Vector2>() != Vector2.zero)
                HandleUpdate();



        }

        void HandleUpdate()
        {
            //Debug.Log("Update inventroty UI");

            var nav = inputs.UI.Navigate.ReadValue<Vector2>();

            var slot = EventSystem.current.currentSelectedGameObject.GetComponent<ItemSlotButtonData>();

            //if ( nav.y != 0)

            if (nav.x != 0 && inputs.UI.Navigate.triggered)
            {
                MoveToTab(nav.x);

            }
            UpdateDescription(slot);
            HandleScrolling(slot);
        }

        public void HandleScrolling(ItemSlotButtonData slot = null)
        {


            if (slot != null || EventSystem.current.currentSelectedGameObject.TryGetComponent(out slot))
            {
                var selection = slot.transform.GetSiblingIndex();
                float scrollPos = Mathf.Clamp(selection - ((itemsInViewport / 2) - 1), 0, selection) * (itemSlotUi.Height + itemListSpacing);
                itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);
                if (selectionIndexesByTab.Count > 0)
                    selectionIndexesByTab[currentTabIndex] = selection;
            }
        }

        public void UpdateDescription(ItemSlotButtonData slot = null)
        {
            if ((selectedUseMenu && selectedUseMenu.activeSelf) || (selectedMenu && selectedMenu.activeSelf))
                return;

            if (slot == null)
                slot = EventSystem.current.currentSelectedGameObject.GetComponent<ItemSlotButtonData>();
            if (slot != null && slot.Item != null)
            {
                description.gameObject.SetActive(true);
                icon.gameObject.SetActive(true);

                var item = slot.Item;
                description.text = item.Description;
                icon.sprite = item.Icon;
                return;
            }

            description.gameObject.SetActive(false);
            icon.gameObject.SetActive(false);

        }

        public IEnumerator WriteOnDdescription_CR(string text, float typingDelayInSeconds = 0.01f, float readTimeInSeconds = 1f)
        {
            inputs.UI.Disable();
            description.text = "";
            foreach (char letter in text.ToCharArray())
            {
                description.text += letter;
                yield return new WaitForSeconds(typingDelayInSeconds);

            }
            yield return new WaitForSeconds(readTimeInSeconds);
            description.text = selectedStack.Item.Description;
            inputs.UI.Enable();
            CancelButton();
            //UpdateDescription();

        }

        void MoveToTab(float dir)//refactor
        {
            if (isTabFixed)
                return;

            tabs[currentTabIndex].SetActive(false);
            if (tabIcons.Count > currentTabIndex && tabIcons[currentTabIndex])
                tabIcons[currentTabIndex].SetActive(false);

            currentTabIndex = (int)Mathf.Repeat(currentTabIndex + (int)dir, inventory.AllItens.Count);

            tabs[currentTabIndex].SetActive(true);
            tabIcons[currentTabIndex].SetActive(true);

            if (pocketName)
                pocketName.text = Inventory.itemCategory[currentTabIndex];
            var selection = tabs[currentTabIndex].GetComponentInChildren<ItemSlotButtonData>();
            if (selection)
                EventSystem.current.SetSelectedGameObject(tabs[currentTabIndex].transform.GetChild(selectionIndexesByTab[currentTabIndex]).gameObject);
            else
                EventSystem.current.SetSelectedGameObject(GetComponentInChildren<Button>().gameObject);

            itemListRect = tabs[currentTabIndex].GetComponent<RectTransform>();
            scrollView.content = itemListRect;
        }

        public IEnumerator ChangeToTab_CR(int tab)//refactor
        {
            if (!isTabFixed)
                yield break;

            yield return new WaitUntil(() => (selectionIndexesByTab.Count > 0 && selectionIndexesByTab.Count == tabs.Count));

            tabs[currentTabIndex].SetActive(false);
            if (tabIcons.Count > currentTabIndex && tabIcons[currentTabIndex])
                tabIcons[currentTabIndex].SetActive(false);

            currentTabIndex = tab;

            tabs[currentTabIndex].SetActive(true);
            if (tabIcons.Count > 0)
                tabIcons[currentTabIndex].SetActive(true);

            if (pocketName)
                pocketName.text = Inventory.itemCategory[currentTabIndex];
            var selection = tabs[currentTabIndex].GetComponentInChildren<ItemSlotButtonData>();
            if (selection)
                EventSystem.current.SetSelectedGameObject(tabs[currentTabIndex].transform.GetChild(selectionIndexesByTab[currentTabIndex]).gameObject);
            else
                EventSystem.current.SetSelectedGameObject(GetComponentInChildren<Button>().gameObject);

            itemListRect = tabs[currentTabIndex].GetComponent<RectTransform>();
            scrollView.content = itemListRect;
            HandleUpdate();

        }

        public IEnumerator SetUpItemTabs_CR()
        {

            for (int i = 0; i < inventory.AllItens.Count; i++)
            {
                if (i > tabs.Count - 1)
                {
                    var g = Instantiate(ItemListBase, ItemListBase.transform.parent);
                    tabs.Add(g);
                }

                SetUpItemList(inventory.AllItens[i]);

                if (tabs[i].transform.GetSiblingIndex() != currentTabIndex && tabs[i].activeSelf)
                    tabs[i].SetActive(false);
            }

            for (int i = 0; i < tabIcons.Count; i++)
            {
                if (i >= inventory.AllItens.Count)
                    tabIcons[i].transform.parent.gameObject.SetActive(false);
            }
            yield return null;
            /*
            for (int i = 0; i < Inventory.itemCategory.Count; i++)
                UpdateItemList(tabs[i] );
            for (int i = 0; i < tabs.Count; i++)
            {
                dynamic list = Convert.ChangeType(inventory.GetObjectByCategory(i), Inventory.itemTypes[i]);
                UpdateItemList(tabs[i], list);
            }

            for (int i = 0; i < tabs.Count; i++)
            {
               // dynamic list = Convert.ChangeType(inventory.GetObjectByCategory(i), Inventory.itemTypes[i]);
                UpdateItemList(tabs[i], Inventory.itemTypes[i]);
            }
            UpdateItemList(tabs[0], inventory.Medicine);
            UpdateItemList(tabs[1], inventory.AllItens[1]);

            yield return new WaitUntil(() => GetComponentInChildren<Button>().gameObject != null);
            */
            EventSystem.current.SetSelectedGameObject(GetComponentInChildren<Button>().gameObject);

            UpdateDescription();
            HandleScrolling();
        }

        void SetUpItemList<T>(GameObject itemList, List<ItemStack<T>> inventory) where T : Item
        {
            foreach (Transform child in itemList.transform)
                Destroy(child.gameObject);

            foreach (var slot in inventory)
            {
                var slotObj = Instantiate(itemSlotUi, itemList.transform);

                slotObj.SetData(slot);
                slotObj.GetComponent<Button>().onClick.AddListener(() => Debug.Log("Click on " + slotObj.name, slotObj.gameObject));
            }

            if (!tabs.Contains(itemList))
                tabs.Add(itemList);

        }

        void SetUpItemList(GameObject itemList)
        {
            var inv = this.inventory.AllItens[tabs.IndexOf(itemList)];
            dynamic inventory = Convert.ChangeType(inv, inv.GetType());
            foreach (Transform child in itemList.transform)
                Destroy(child.gameObject);

            foreach (var slot in inventory)
            {
                //var slotConv = Convert.ChangeType(slot, slot.GetType());
                var slotObj = Instantiate(itemSlotUi, itemList.transform);
                slotObj.SetData(slot);
                slotObj.GetComponent<Button>().onClick.AddListener(() => Debug.Log("Click on " + slotObj.name, slotObj.gameObject));
            }

            if (!tabs.Contains(itemList))
                tabs.Add(itemList);

        }

        void SetUpItemList(GameObject itemList, Type type)
        {

            var inv = this.inventory.AllItens[tabs.IndexOf(itemList)];
            dynamic inventory = Convert.ChangeType(inv, type);
            foreach (Transform child in itemList.transform)
                Destroy(child.gameObject);

            foreach (var slot in inventory)
            {
                var slotConv = Convert.ChangeType(slot, slot.GetType());
                var slotObj = Instantiate(itemSlotUi, itemList.transform);
                slotObj.SetData(slot);
                slotObj.GetComponent<Button>().onClick.AddListener(() => Debug.Log("Click on " + slotObj.name, slotObj.gameObject));
            }

            if (!tabs.Contains(itemList))
                tabs.Add(itemList);

        }

        void SetUpItemList(GameObject itemList, List<ItemStack> inventory)
        {
            foreach (Transform child in itemList.transform)
                Destroy(child.gameObject);

            foreach (var slot in inventory)
            {
                var slotObj = Instantiate(itemSlotUi, itemList.transform);
                slotObj.SetData(slot);
                slotObj.GetComponent<Button>().onClick.AddListener(() => Debug.Log("Click on " + slotObj.name, slotObj.gameObject));
            }

            if (!tabs.Contains(itemList))
                tabs.Add(itemList);

        }

        void SetUpItemList(List<ItemStack> inventory)
        {
            var itemList = tabs[this.inventory.AllItens.IndexOf(inventory)];
            foreach (Transform child in itemList.transform)
                Destroy(child.gameObject);

            foreach (var slot in inventory)
            {
                var slotObj = Instantiate(itemSlotUi, itemList.transform);
                slotObj.SetData(slot);
                slotObj.GetComponent<Button>().onClick.AddListener(SelectItemButton);
            }

            selectionIndexesByTab.Add(0);
            /*
            if (!tabs.Contains(itemList))
                tabs.Add(itemList);
            */
        }

        void SetUpItemList<T>(List<ItemStack<T>> list) where T : Item
        {

        }


        void UpdateTabOnUse(int tabIndex, int itemStackIndex = 0)// make a version that uses preloaded gameobjects and prefabs, activating and deactivating as needed and loading from the index changed
        {
            int slotsCount = tabs[tabIndex].transform.childCount;
            var invetorySlots = inventory.AllItens[tabIndex];
            int diff = inventory.AllItens[tabIndex].Count - slotsCount;
            itemStackIndex = (itemStackIndex >= invetorySlots.Count || itemStackIndex < 0) ? 0 : itemStackIndex;

            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    var newSlot = Instantiate(itemSlotUi, tabs[tabIndex].transform);
                    newSlot.GetComponent<Button>().onClick.AddListener(SelectItemButton);
                    slotsCount++;
                }
            }

            if (diff < 0)
            {
                //var diffAbs = Mathf.Abs( diff);
                for (int i = diff; i < 0; i++)
                {
                    var oldSlot = (tabs[tabIndex].transform.GetChild(i - diff + inventory.AllItens[tabIndex].Count).gameObject);
                    Destroy(oldSlot);
                    slotsCount--;
                }
            }

            for (int i = itemStackIndex; i < slotsCount; i++)
                tabs[tabIndex].transform.GetChild(i).GetComponent<ItemSlotButtonData>().SetData(inventory.AllItens[tabIndex][i]);

            if (selectionIndexesByTab[tabIndex] >= slotsCount)
            {
                selectionIndexesByTab[tabIndex]--;
            }

            Debug.Log("TABS Updated");

        }


        void UpdateStackOnUse(int tabIndex, int slotIndex)
        {
            tabs[tabIndex].transform.GetChild(slotIndex).GetComponent<ItemSlotButtonData>().SetData(inventory.AllItens[tabIndex][slotIndex]);
            Debug.Log("SLOT Updated");

        }

        public void CrrntItemListEnabaler(bool isActive)
        {
            tabs[currentTabIndex].SetActive(isActive);
            if (cancelCloseButtonText)
            {
                if (isActive)
                    cancelCloseButtonText.text = "CLOSE BAG";
                else

                    cancelCloseButtonText.text = "CANCEL";
            }


        }

        public void SelectItemButton()//Some Adjustments: make less especific
        {
            var sel = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            sel.colors = selectedButtonColors;
            selectionIndexesByTab[currentTabIndex] = sel.transform.GetSiblingIndex();

            selectedStack = inventory.AllItens[currentTabIndex][selectionIndexesByTab[currentTabIndex]];

            if (onSelection != null)
            {
                onSelection?.Invoke();
                return;
            }

            if (selectedStack.Item.CanUseOutsideBattle)
            {
                selectedUseMenu.SetActive(true);
                EventSystem.current.SetSelectedGameObject(selectedUseMenu.GetComponentInChildren<Button>().gameObject);
                return;
            }

            selectedMenu.SetActive(true);
            EventSystem.current.SetSelectedGameObject(selectedMenu.GetComponentInChildren<Button>().gameObject);



        }


        public void UseButton()//Button use on confirm menu
        {
            if (!selectedStack.Item.CanUseOutsideBattle)
            {
                StartCoroutine(TryToUseSelected_OnMenu_CR());
                return;
            }


            partyMenuSelection.Open(
                () =>
                {
                    StartCoroutine(UseItem_OnMenu_CR(selectedStack, partyMenuSelection.SelectedPokemon));
                    EventSystem.current.SetSelectedGameObject(null);

                },
                null
                /*
                () =>
                {
                    tabs[currentTabIndex].SetActive(true);
                    if (cancelCloseButtonText)
                        cancelCloseButtonText.text = "CLOSE BAG";
                    UnselectItem();
                    partyMenuSelection.gameObject.SetActive(false);
                    //return false;

                }

                //CancelButton
                */
                );


            if (selectedStack.Item is TmAndHMItem)
            {
                var tmHmItem = selectedStack.Item as TmAndHMItem;
                partyMenuSelection.SetAble(tmHmItem.Move);

            }

            tabs[currentTabIndex].SetActive(false);
            if (cancelCloseButtonText)
                cancelCloseButtonText.text = "CANCEL";

            selectedUseMenu.SetActive(false);
        }



        IEnumerator UseItem_CR(ItemStack stack, Pokemon selectedPkmn)
        {
            if (GameManager.Instance.State == GameState.Menu)
            {
                if (stack.Item.CanUseOutsideBattle)
                {
                    yield return UseItem_OnMenu_CR(stack, selectedPkmn);
                    yield break;
                }

                yield return DialogueCacheManager.I.ShowText_CR($"Can't use {stack.Item.name} outside Battle.");
            }

            if (GameManager.Instance.State == GameState.Battle)
            {
                if (stack.Item.CanUseInBattle)
                {
                    yield return UseItem_OnBattle();
                    yield break;
                }

                yield return DialogueCacheManager.I.ShowText_CR($"Can't use {stack.Item.name} on Battle.");

            }


            yield return null;
        }

        IEnumerator UseItem_OnMenu_CR(ItemStack stack, Pokemon selectedPokemon)
        {

            if (stack.Item is TmAndHMItem)
            {
                yield return UseTMItens_CR();
                yield break;
            }

            if (stack.Item is MedicineItem med)
            {
                if (med.PpRestoration == RestorePP.one_move || med.PpRestoration == RestorePP.full_one_move)
                {
                    //open move selection ui
                    //get move index
                }
            }

            var usedItem = inventory.UseItem(stack, selectedPokemon);
            EventSystem.current.SetSelectedGameObject(null);


            if (usedItem && usedItem is MedicineItem)
            {

                yield return DialogueCacheManager.I.ShowText_CR($"Player used {usedItem.name} on {selectedPokemon.NickName}.");

            }
            else
                yield return DialogueCacheManager.I.ShowText_CR($"It won't have any effect!");

            CancelButton();

        }

        public Item UseItem_OnBattle()
        {
            if (selectedStack.Item.CanUseInBattle)
                return inventory.UseItem(selectedStack, partyMenuSelection.SelectedPokemon);
            return null;
        }

        public IEnumerator TryToUseSelected_OnMenu_CR()
        {
            if (!selectedStack.Item.CanUseOutsideBattle)
            {
                EventSystem.current.SetSelectedGameObject(null);
                selectedUseMenu.SetActive(false);

                yield return DialogueCacheManager.I.ShowText_CR($"Can't use {selectedStack.Item.Name} outside battle.");
                UnselectItem();

                yield break;

            }



        }

        public bool CheckIfPkball()
        {
            if (selectedStack.Item is PokeBallItem)
            {
                gameObject.SetActive(false);

                return true;
            }
            return false;
        }


        public IEnumerator UseTMItens_CR()
        {
            var tmItem = selectedStack.Item as TmAndHMItem;
            if (tmItem == null)
                yield break;

            var pokemon = partyMenuSelection.SelectedPokemon;

            if (!pokemon.Base.TM_n_HM_Leanrnables.Contains(tmItem.Move))
            {

                yield return DialogueCacheManager.I.ShowText_CR($"{pokemon.NickName} can't learn this move");

                partyMenuSelection.ResetPressedButtonColors();
                EventSystem.current.SetSelectedGameObject(partyMenuSelection.transform.GetChild(partyMenuSelection.GetSelectedPartyIndex()).gameObject);
                yield break;
            }


            if (pokemon.HasMove(tmItem.Move))
            {
                yield return DialogueCacheManager.I.ShowText_CR($"{pokemon.NickName} already knowns {tmItem.Move.name}.");
                CancelButton();
                yield break;


            }

            if (pokemon.MoveSet.Count < PokemonBase.MAX_MOVES_COUNT)
            {
                inventory.UseItem(selectedStack, pokemon);
                yield return DialogueCacheManager.I.ShowText_CR($"{pokemon.NickName} learned {tmItem.Move.name}.");

                CancelButton();
            }

            else
            {

                yield return DialogueCacheManager.I.ShowText_CR($"{pokemon.NickName} is trying to learn {tmItem.Move.MoveName}");
                yield return DialogueCacheManager.I.ShowText_CR($"But it cannot learn more than {PokemonBase.MAX_MOVES_COUNT} moves");
                Debug.Log($"{selectedStack.Item.name}");
                var tmIndex = selectionIndexesByTab[currentTabIndex];
                forgetMoveUI.Open(
                    () =>
                    {
                        if (DialogueCacheManager.I.IsShowing)
                            return;

                        StartCoroutine(MoveToForgetChoice_CR(pokemon, tmIndex, tmItem));




                    },
                        CancelButton
                    );

                forgetMoveUI.SetForgetMenu(pokemon, tmItem.Move);
                yield break;

            }


        }


        public IEnumerator UnusableWarnning_CR()
        {
            if (!selectedStack.Item.CanUseInBattle)
            {
                yield return WriteOnDdescription_CR($"Can't Use {selectedStack.Item.name} on Battle.");
                yield break;

            }
            yield return WriteOnDdescription_CR($"Can't Use {selectedStack.Item.name} on {partyMenuSelection.SelectedPokemon.NickName}.");
        }

        IEnumerator MoveToForgetChoice_CR(Pokemon pokemon, int tmIndex, TmAndHMItem tmItem)// rename
        {
            //not using the return value of UseItem() as check because Item can be a HM and therefore not consumed upon use
            inventory.UseItem(currentTabIndex, tmIndex, pokemon, forgetMoveUI.SelectedIndex);
            if (pokemon.HasMove(tmItem.Move))
            {
                yield return DialogueCacheManager.I.ShowText_CR($"{pokemon.NickName} learned {tmItem.Move.name}.");
                CancelButton();
                yield break;
            }
            yield return DialogueCacheManager.I.ShowText_CR($"{pokemon.NickName} did not learn {tmItem.Move.name}.");

            CancelButton();
        }

        IEnumerator MoveToForgetClose_CR(Pokemon pokemon, TmAndHMItem tmItem)
        {
            yield return DialogueCacheManager.I.ShowText_CR($"{pokemon.NickName} did not learn {tmItem.Move.name}.");
            CancelButton();
        }

        public void CancelButton()
        {
            //return on dialoge open?
            if (DialogueCacheManager.I.IsShowing)
                return;

            if (selectedUseMenu && selectedUseMenu.activeSelf)
            {
                selectedUseMenu.SetActive(false);
                UnselectItem();

                return;
            }
            if (selectedMenu && selectedMenu.activeSelf)
            {
                selectedMenu.SetActive(false);
                UnselectItem();

                return;
            }



            if (partyMenuSelection && partyMenuSelection.gameObject.activeSelf)
            {
                //partyMenuSelection.CancelButton();//some future problems? Maybe
                partyMenuSelection.gameObject.SetActive(false);
                tabs[currentTabIndex].SetActive(true);
                if (cancelCloseButtonText)
                    cancelCloseButtonText.text = "CLOSE BAG";

                if (forgetMoveUI && forgetMoveUI.gameObject.activeSelf)
                {
                    forgetMoveUI.gameObject.SetActive(false);
                    tabs[currentTabIndex].SetActive(true);
                }

                UnselectItem();

                return;
            }

            if (forgetMoveUI && forgetMoveUI.gameObject.activeSelf)
            {
                forgetMoveUI.gameObject.SetActive(false);
                tabs[currentTabIndex].SetActive(true);


                UnselectItem();
                return;


            }

            OnCancelButton?.Invoke();
            /*
            if ((bool)OnCancelButton?.Invoke())
                return;

            gameObject.SetActive(false);
            */
        }

        void UnselectItem()
        {


            var button = GetComponentInChildren<Button>();

            if (tabs[currentTabIndex].transform.childCount > 0)
            {
                button = tabs[currentTabIndex].transform.GetChild(selectionIndexesByTab[currentTabIndex]).GetComponent<Button>();
                button.colors = normalButtonColor;
            }
            EventSystem.current.SetSelectedGameObject(button.gameObject);

            selectedStack = null;
            UpdateDescription();

        }




        public bool TryClosePartyMenuSelec()
        {
            if (!partyMenuSelection || !partyMenuSelection.gameObject.activeSelf)
                return false;

            partyMenuSelection.gameObject.SetActive(false);
            tabs[currentTabIndex].SetActive(true);
            if (cancelCloseButtonText)
                cancelCloseButtonText.text = "CLOSE BAG";
            UnselectItem();
            return true;

        }


        public GameObject GetLastSelection()//refactor
        {

            if (inventory.AllItens[currentTabIndex].Count > 0 && tabs.Count > 0 && selectionIndexesByTab.Count > 0)
                return tabs[currentTabIndex].transform.GetChild(selectionIndexesByTab[currentTabIndex]).gameObject;

            return GetComponentInChildren<Button>().gameObject;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}

public enum InvetoryUIState
{
    Menu, Battle, Party_Use
}