using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class MainLoader : MonoBehaviour
    {
        [SerializeField] GameObject mainObjects;
        [SerializeField] Transform playerSpawn;


        private void Awake()
        {
            var existing = FindObjectsOfType<CoreObjects>();
            if (existing.Length == 0)
                Instantiate(mainObjects, (playerSpawn != null) ? playerSpawn.position : transform.position, Quaternion.identity) ;
        }

    }
}
