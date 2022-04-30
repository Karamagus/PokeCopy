using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace pokeCopy
{
    public class ForgetMoveButtonData : MonoBehaviour
    {
        [SerializeField] Text moveName;

        [SerializeField] Text movePP;

        [SerializeField] Image moveType;
        [SerializeField] Button mButton;


        public Text MoveName => moveName; 
        public Text MovePP => movePP;  
        public Image MoveType  => moveType;

        public Button MButton => mButton;
    }
}
