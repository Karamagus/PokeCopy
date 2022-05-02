using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace pokeCopy
{

    public class BattleHud : MonoBehaviour
    {
        Pokemon poke;

        [SerializeField] Text nameText;
        [SerializeField] Text LvlText;
        [SerializeField] Image conditionImg;
        [SerializeField] Text conditionText;
        [Space]
        [SerializeField] Text HPText;
        //public UIColors colors;

        [SerializeField] Slider HPSlider;
        Image hpFill;
        [Space]
        [SerializeField] Slider XPSlider;


        int hpData;
        bool isUpdating;

        public void SetHUD(Unit unit, bool hidden = false)//TODO: change Unit parameter to Pokemon parameter
        {
            if (unit.CrrPokemon != null)
            {
                unit.CrrPokemon.OnHPChange -= UpdateHP;
                unit.CrrPokemon.OnStatusChanged -= SetStatusData;

            }


            if (!hidden)
                gameObject.SetActive(true);
            poke = unit.CrrPokemon;
            nameText.text = unit.CrrPokemon.NickName;
            UpdateLevel();
            //LvlText.text = "Lvl " + unit.CrrPokemon.Level;
            HPSlider.maxValue = unit.CrrPokemon.MaxHP;
            HPSlider.value = unit.CrrPokemon.HP;
            SetHP(poke.HP);

            if (HPText)
            {
                HPText.text = $"{unit.CrrPokemon.HP}/{unit.CrrPokemon.MaxHP}";
            }

            SetXp();
            SetUpXPValue();

            hpFill = HPSlider.fillRect.GetComponent<Image>();
            unit.CrrPokemon.OnHPChange += UpdateHP;
            unit.CrrPokemon.OnStatusChanged += SetStatusData;
            //unit.CrrPokemon.OnChangesInHP.AddListener(SetHP(unit.CrrPokemon.HP));
            HPColorUpdate();
            SetStatusData();
        }

        public void Hide() => gameObject.SetActive(false);


        void SetStatusData()
        {

            if (poke.Status == null)
            {
                conditionImg.gameObject.SetActive(false);
                conditionText.text = "";
            }
            else
            {
                conditionImg.gameObject.SetActive(true);

                //conditionImg.color = GameManager.Instance.uiColors.statusColors[poke.Status.Id];
                conditionImg.color = UIColors.statusColors[poke.Status.Id];
                conditionText.text = poke.Status.Id.ToString().ToUpper();
            }
        }

        void SetHP(int hp) => hpData = hp;

        void SetXp()
        {
            if (XPSlider == null)
                return;

            XPSlider.minValue = poke.Base.GetExpForLevel(poke.Level);
            XPSlider.maxValue = poke.Base.GetExpForLevel(poke.Level + 1);


        }

        void SetUpXPValue()
        {
            if (XPSlider != null)
                XPSlider.value = poke.Exp;
        }

        public void UpdateLevel()
        {
            LvlText.text = "Lvl " + poke.Level;
            if (HPText)
            {
                HPText.text = $"{poke.HP}/{poke.MaxHP}";
            }
            HPSlider.maxValue = poke.MaxHP;
            HPSlider.value = poke.HP;


        }

        public IEnumerator UpdateXp_CR(bool reset = false)
        {
            if (XPSlider == null) yield break;

            if (reset)
                SetXp();
            //float diff = 
            while (XPSlider.value < poke.Exp && XPSlider.value != XPSlider.maxValue)
            {
                XPSlider.value = Mathf.MoveTowards(XPSlider.value, target: poke.Exp, 1f * poke.Level);
                // Mathf.Lerp(XPSlider.value, poke.Exp, 1f);
                yield return new WaitForFixedUpdate();
                if (XPSlider.value > XPSlider.maxValue)//check for errors
                    XPSlider.value = poke.Exp;

            }

        }

        public void UpdateHP()
        {
            StartCoroutine(UpdateHP_CR());
        }


        public IEnumerator UpdateHP_CR()
        {
            isUpdating = true;
            // float rate =  ((HPSlider.value - hpData);
            // float rate = 50* (HPSlider.value - hpData)/(HPSlider.maxValue + hpData);
            float rate = (HPSlider.maxValue + (HPSlider.value - poke.HP) + 1);


            while (Mathf.Abs(HPSlider.value - poke.HP) > Mathf.Epsilon)//check for errors
            {
                //HPSlider.value -= Mathf.Clamp((rate/2 )* 0.008f, -0.95f, 0.95f);
                HPSlider.value = Mathf.MoveTowards(HPSlider.value, target: poke.HP, Mathf.Abs(rate * Time.deltaTime));
                if (HPText)
                    HPText.text = $"{Mathf.Round(HPSlider.value)}/{HPSlider.maxValue}";
                HPColorUpdate();
                yield return null;
            }
            HPSlider.value = poke.HP;

            isUpdating = false;
            //configure later so the value is checked on update of value

        }


        void HPColorUpdate()
        {
            if (HPSlider.value <= HPSlider.maxValue * 0.25f)
                hpFill.color = UIColors.hpStateColor[HpState.danger];
            else if (HPSlider.value <= HPSlider.maxValue * 0.5f)
                hpFill.color = UIColors.hpStateColor[HpState.caution];
            else
                hpFill.color = UIColors.hpStateColor[HpState.healthy];

        }


        public IEnumerator WaitHpUpdat_CR()
        {
            yield return new WaitUntil(() => isUpdating == false);
            yield return new WaitForSeconds(0.25f);
        }

        public void OnDisable()
        {
            poke.OnHPChange -= UpdateHP;
            poke.OnStatusChanged -= SetStatusData;

        }

        public void OnDestroy()
        {
            poke.OnHPChange -= UpdateHP;
            poke.OnStatusChanged -= SetStatusData;
        }

    }
}
