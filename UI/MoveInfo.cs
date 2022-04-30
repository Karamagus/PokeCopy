using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace pokeCopy
{
    public class MoveInfo : MonoBehaviour
    {
        [SerializeField] Text moveName;
        [SerializeField] Text mDescription;
        [SerializeField] Text mPowerValue;
        [SerializeField] Text mAccuracyValue;
        [SerializeField] Text mCategoryName;
        [SerializeField] Image mCategorySymbol;

        public void UpdateMoveInfo(MovesBase move)
        {
            moveName.text = move.MoveName;
            mDescription.text = move.Description;

            mPowerValue.text = (move.Power != 0) ? move.Power.ToString() : "-";

            mAccuracyValue.text = (move.Accuracy != 0 || move.AlwaysHits)? move.Accuracy.ToString() : "-";
            mCategoryName.text = move.Category.ToString();

            mCategorySymbol.sprite = UIColors.moveCategoryIcon[move.Category];
        }


    }
}
