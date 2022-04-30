using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


using UnityEngine;


namespace pokeCopy
{



    public class Item : ScriptableObject
    {
        [SerializeField] int cost;
        [SerializeField] int value;
        [SerializeField] string itemName;
        [SerializeField] Sprite icon;

        [TextArea(3, 6)]
        [SerializeField] string description;
        public string Name => itemName;
        public string Description => description;
        public Sprite Icon => icon;

        /*
        [Serializable]
        public class OnUseEvent : UnityEvent { };

        [FormerlySerializedAs("Use")]
        [SerializeField]
        private OnUseEvent m_Use = new OnUseEvent();

        public OnUseEvent onUse
        { get { return m_Use; } set { m_Use = value; } }

        */
        public virtual bool IsUsed(Pokemon target, int moveIndex = -1)
        {
            return false;
        }

        public virtual bool TryToUse(Pokemon target, Move move = null)
        {
            return false;
        }

        public virtual bool CanUseInBattle => true;
        public virtual bool CanUseOutsideBattle => true;

        public virtual bool IsReusable => false;

    }
}
