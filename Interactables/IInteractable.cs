using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable 
{
    public IEnumerator Interact(Transform initiator = null);
}
