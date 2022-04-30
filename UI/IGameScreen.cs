using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public interface IGameScreen
    {

        public void Open(Action onSelection, Action onCancel);
        //public void Open(Action onSelection, Func<bool> onCancel);
        //public void OpenAsMain(Action onSelection, Action onCancel);
        //public void OpenAsSub(Action onSelection);
        public void CancelButton();
        public GameObject GetGameObject();


    }
}
