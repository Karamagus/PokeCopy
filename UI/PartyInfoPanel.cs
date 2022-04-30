using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyInfoPanel : MonoBehaviour
{
    [SerializeField] Image pkmnImage;
    [SerializeField] Image type1;
    [SerializeField] Image type2;
    [SerializeField] Text[] moveName;
    [SerializeField] Text[] ppText;
    [SerializeField] Image[] moveType;
    [SerializeField] Slider XpBar;
    [SerializeField] Text xPtext;
    [SerializeField] GameObject noSelectionPanel;
    //Change color of move button to mactch type

    // Update is called once per frame
    public void UpdateInfoPanel(Pokemon pkmn)
    {
        if (pkmn != null)
        {

            if (noSelectionPanel)
                noSelectionPanel.SetActive(false);

            pkmnImage.sprite = pkmn.Base.Front;
            for (int i = 0; i < moveName.Length; i++)
            {
                if (pkmn == null || i >= pkmn.MoveSet.Count)
                {
                    moveName[i].text = "-";
                    ppText[i].text = "-/-";
                    ppText[i].rectTransform.localPosition = new Vector3(ppText[i].rectTransform.localPosition.x, -3.814697e-06f);
                    moveType[i].gameObject.SetActive(false);
                    continue;
                }
                var pkmnMove = pkmn.MoveSet[i];

                moveName[i].text = pkmnMove.Base.name;
                ppText[i].text = $"{pkmnMove.Pp}/{pkmnMove.Base.MaxPP}";
                ppText[i].rectTransform.localPosition = new Vector3(ppText[i].rectTransform.localPosition.x, 12.2f);
                moveType[i].gameObject.SetActive(true);
                moveType[i].sprite = UIColors.typeIcons[pkmnMove.Base.Type];



            }
            type1.sprite = UIColors.typeIcons[pkmn.Base.Type1];


            type2.sprite = UIColors.typeIcons[pkmn.Base.Type2];
            if (type2.sprite == null)
            {
                type1.rectTransform.localPosition = new Vector3(-275f, type1.rectTransform.localPosition.y);
                type2.gameObject.SetActive(false);
            }
            else
            {
                type1.rectTransform.localPosition = new Vector3(-323.4f, type1.rectTransform.localPosition.y);

                type2.gameObject.SetActive(true);
            }

            if (XpBar != null && xPtext != null)
            {
                int crntLvlXP = pkmn.Base.GetExpForLevel(pkmn.Level);
                int nxtLvlXP = pkmn.Base.GetExpForLevel(pkmn.Level + 1);
                //Debug.Log($" crrt {crntLvlXP}");
                //Debug.Log($" nxt {nxtLvlXP}");


                XpBar.value = (pkmn.Exp - crntLvlXP) / (float)(nxtLvlXP - crntLvlXP);
                //Debug.Log(XpBar.value);
                xPtext.text = $"{pkmn.Exp}/{nxtLvlXP}";
            }

        }
        else
        {
            if (noSelectionPanel)
                noSelectionPanel.SetActive(true);

            return;
        }

    }





}
