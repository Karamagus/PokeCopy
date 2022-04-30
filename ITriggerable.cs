using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public interface ITriggerable
    {
        public void OnContact(PlayerMovement player);

    }
}
