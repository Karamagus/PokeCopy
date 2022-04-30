using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class TrainerBattleTrigger : MonoBehaviour, ITriggerable
    {
        public void OnContact(PlayerMovement player)
        {
            GameManager.Instance.OnTrainerView();
            StartCoroutine(GetComponentInParent<TrainerController>().TriggerBattle(player));
        }
    }
}
