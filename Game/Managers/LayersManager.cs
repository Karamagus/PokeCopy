using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayersManager : MonoBehaviour
{
    [SerializeField] LayerMask unwalkable;
    [SerializeField] LayerMask interactable;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fOVLayer;
    [SerializeField] LayerMask portalLayer;

    public static LayersManager i { get; set; }
    public LayerMask Unwalkable { get => unwalkable;  }
    public LayerMask Interactable { get => interactable; }
    public LayerMask GrassLayer { get => grassLayer;  }
    public LayerMask PlayerLayer { get => playerLayer;  }
    public LayerMask FOVLayer { get => fOVLayer;}
    public LayerMask PortalLayer { get => portalLayer;}

    public LayerMask TriggerableLayers
    {
        get => portalLayer | grassLayer | fOVLayer;
    }

    public void Awake()
    {
        i = this;
    }



}
