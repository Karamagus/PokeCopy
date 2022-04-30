using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class CoreObjects : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
