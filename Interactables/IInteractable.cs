using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public interface IInteractable
    {
        public IEnumerator Interact_CR(Transform initiator = null);
    }

}
