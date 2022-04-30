using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class RandomBattleTrigger : MonoBehaviour, ITriggerable
    {
        [SerializeField] [ReadOnly] MapDataHandler mapHandler;

        public void Awake()
        {
            mapHandler = GetComponentInParent<MapDataHandler>();
        }

        public void OnContact(PlayerMovement player)
        {
            if (Random.Range(1, 101) <= 10)
            {
                mapHandler.LoadWildPokemon();
                GameManager.Instance.StartWildBattle();
            }
        }
    }
}
