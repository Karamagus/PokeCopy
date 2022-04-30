using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using System;

namespace pokeCopy
{
    public class ForgetMenuSubMenu : MonoBehaviour, IGameScreen
    {
        //reference to moves and respective buttons.
        //reference to new move and button
        //reference to move data ( move description, power accuracy and category, type, pp, name)
        [SerializeField] List<ForgetMoveButtonData> moveButtons;
        [SerializeField] MoveInfo infoPanel;
        [SerializeField] Button forget_cancelButton;
        [SerializeField] Text pokeNick;
        [SerializeField] Image pokeImage;
        [SerializeField] Image Type1;
        [SerializeField] Image Type2;

        [SerializeField][ReadOnly] ForgetMoveButtonData crrtSellectedMove;
        List<MovesBase> movesData = new List<MovesBase>();
        InputSystemUIInputModule input;


        int selectedIndex;
        Action OnSelected;
        Action<int> OnIndexSelected;
        //Func<bool> OnCancel;
        Action OnCancel;

        public int SelectedIndex => selectedIndex;

        public void SetForgetMenu(Pokemon poke, MovesBase move, Action<int> onSelection = null)
        {
            pokeNick.text = poke.NickName;
            var pokeChibi = (poke.Base.Chibi != null) ? poke.Base.Chibi : poke.Base.Front;

            pokeImage.sprite = pokeChibi;
            Type1.sprite = (poke.Base.Type1 != PokemonType.None) ? UIColors.typeIcons[poke.Base.Type1] : UIColors.typeIcons[poke.Base.Type2];
            if (poke.Base.Type2 != PokemonType.None && poke.Base.Type1 != PokemonType.None)
                Type2.sprite = UIColors.typeIcons[poke.Base.Type2];
            else
                Type2.gameObject.SetActive(false);

            if (onSelection != null)
                OnIndexSelected = onSelection;

            SetMoveButtons(poke.MoveSet.Select(x => x.Base).ToList(), move);
            UIUpdate();

        }



        public void SetMoveButtons(List<MovesBase> moveSet, MovesBase newMove)
        {
            for (int i = 0; i < moveSet.Count; i++)
            {
                moveButtons[i].MoveName.text = moveSet[i].MoveName;
                moveButtons[i].MovePP.text = $"PP {moveSet[i].MaxPP}";
                moveButtons[i].MoveType.sprite = UIColors.typeIcons[moveSet[i].Type];

                movesData.Add(moveSet[i]);
            }

            var newMoveData = moveButtons[moveSet.Count];

            newMoveData.MoveName.text = newMove.MoveName;
            newMoveData.MovePP.text = $"PP {newMove.MaxPP}";
            newMoveData.MoveType.sprite = UIColors.typeIcons[newMove.Type];
            movesData.Add(newMove);
        }


        // Start is called before the first frame update
        public void OnEnable()
        {
            input = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            EventSystem.current.SetSelectedGameObject(GetComponentInChildren<Button>().gameObject);
            EventSystem.current.currentSelectedGameObject.GetComponent<Button>().OnSelect(null);
        }

        // Update is called once per frame
        void Update()
        {
            if (gameObject.activeSelf != false && input.move.ToInputAction().ReadValue<Vector2>() != Vector2.zero)
                UIUpdate();
        }

        public void UIUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject.TryGetComponent(out crrtSellectedMove))
                infoPanel.UpdateMoveInfo(movesData[moveButtons.IndexOf(crrtSellectedMove)]);
        }

        public void SelectMove()
        {
            selectedIndex = moveButtons.IndexOf(crrtSellectedMove);
            OnSelected?.Invoke();
        }

        public void Open(Action onSelection = null, Action onCancel = null)
        {
            gameObject.SetActive(true);
            if (onSelection != null)
                OnSelected = onSelection;
            if (onCancel != null)
                OnCancel = onCancel;
        }

        public void SetButtons(Action onSelection, Action onCancel )
        {
            if (onSelection != null)
                OnSelected = onSelection;
            if (onCancel != null)
                OnCancel = onCancel;

        }

        public void CancelButton()
        {
            OnCancel?.Invoke();
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}
