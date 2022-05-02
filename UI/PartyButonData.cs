using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace pokeCopy
{

    public class PartyButonData : MonoBehaviour
    {
        public Pokemon Pokemon { get; set; }

        [SerializeField] GameObject data;
        [SerializeField] GameObject empty;
        // [SerializeField] UIColors colors;

        //Consider reuse battleHud code here so part of the data can be dealt altomaticly as the pokemon has its data changed

        [SerializeField] Text lvlText;
        [SerializeField] Text NameText;
        [SerializeField] Image chibiImage;
        [SerializeField] Text HpText;
        [SerializeField] Slider HpSlider;
        [SerializeField] Image statusImage;
        [SerializeField] Text statusText;
        [SerializeField] Text[] moveTexts;
        [SerializeField] Text[] movePP;
        [SerializeField] Image type1Icon;
        [SerializeField] Image type2Icon;
        [SerializeField] Text ableText;
        public Button Button { get; private set; }

        [SerializeField] BattleHud hud;

        public void Start()
        {
            Button = GetComponent<Button>();


        }

        public void SetUpPkmnInfo(Pokemon pkmn)
        {
            Pokemon = pkmn;
            Button = GetComponent<Button>();
            hud = GetComponent<BattleHud>();

            if (Pokemon == null)
            {
                if (data != null)
                {
                    data.SetActive(false);
                    empty.SetActive(true);
                    gameObject.GetComponent<Button>().interactable = false;
                    return;

                }
                gameObject.SetActive(false);
                return;
            }

            gameObject.GetComponent<Button>().interactable = true;
            if (data != null)
            {

                data.SetActive(true);
                empty.SetActive(false);
            }

            //TO DO: Reuse BattleHUD script to update data here
            pkmn.OnHPChange += UpdateHPValue;

            lvlText.text = $"Lvl {Pokemon.Level}";
            NameText.text = Pokemon.NickName;

            chibiImage.sprite = (Pokemon.Base.Chibi) ? Pokemon.Base.Chibi : Pokemon.Base.Front;

            HpText.text = $"{Pokemon.HP}/{Pokemon.MaxHP}";
            HpSlider.maxValue = Pokemon.MaxHP;
            HpSlider.value = Pokemon.HP;

            if (moveTexts.Length > 0 && movePP.Length > 0)
            {
                for (int i = 0; i < moveTexts.Length; i++)
                {
                    if (i >= Pokemon.MoveSet.Count)
                    {
                        moveTexts[i].text = "-";
                        movePP[i].text = "-/-";
                        continue;
                    }
                    moveTexts[i].text = Pokemon.MoveSet[i].Base.name;
                    movePP[i].text = $"{Pokemon.MoveSet[i].Pp}/{Pokemon.MoveSet[i].Base.MaxPP}";
                }
            }


            if (type1Icon != null)
            {
                type1Icon.sprite = UIColors.typeIcons[pkmn.Base.Type1];
            }
            if (type2Icon != null)
            {

                type2Icon.sprite = UIColors.typeIcons[pkmn.Base.Type2];
                if (type2Icon.sprite == null)
                    type2Icon.gameObject.SetActive(false);
                else
                    type2Icon.gameObject.SetActive(true);
            }


            UpdateStatusCondition();

            UpdateHPColors();

        }

        public void SetLeanrableMove(bool isLearnable)
        {
            statusImage.gameObject.SetActive(false);
            ableText.gameObject.SetActive(true);
            ableText.text = (isLearnable) ? "ABLE!" : "UNABLE";

        }

        void UpdateHPValue()
        {
            HpText.text = $"{Pokemon.HP}/{Pokemon.MaxHP}";
            HpSlider.maxValue = Pokemon.MaxHP;
            HpSlider.value = Pokemon.HP;

            UpdateHPColors();

        }

        void UpdateHPColors()
        {
            var hpFill = HpSlider.fillRect.GetComponent<Image>();

            if (HpSlider.value <= HpSlider.maxValue * 0.25f)
                hpFill.color = UIColors.hpStateColor[HpState.danger];
            else if (HpSlider.value <= HpSlider.maxValue * 0.5f)
                hpFill.color = UIColors.hpStateColor[HpState.caution];
            else
                hpFill.color = UIColors.hpStateColor[HpState.healthy];

            if (Pokemon.HP <= 0)
                Button.colors = UIColors.partyMenuFaintColors;
            else
                Button.colors = UIColors.partyMenuNormalColors;

        }

        void UpdateStatusCondition()
        {
            var pkmn = Pokemon;
            if (pkmn.Status == null)
            {
                statusImage.gameObject.SetActive(false);
                statusText.text = "";

            }
            else
            {
                statusImage.gameObject.SetActive(true);

                statusImage.color = UIColors.statusColors[pkmn.Status.Id];

                statusText.text = pkmn.Status.Id.ToString().ToUpper();
            }



            if (ableText)
                ableText.gameObject.SetActive(false);

        }


    }
}

