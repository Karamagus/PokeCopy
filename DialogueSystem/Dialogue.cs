using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{

    [System.Serializable]
    public class Dialogue
    {
        [TextArea(3, 15)]
        public string[] senteces;

    }
}

