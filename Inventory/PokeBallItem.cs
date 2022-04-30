using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    [CreateAssetMenu(fileName = "New PokeBall", menuName = "Item / Create new PokeBall")]

    public class PokeBallItem : Item
    {
        [Header("Catch propeties")]
        [SerializeField] PokeBallType type; 
        [SerializeField] float catchRateMod;

        public static float GetCatchRateMod( PokeBallType type, Pokemon poke = null)
        {
            return type switch
            {
                PokeBallType.Great_Ball => 1.5f,
                PokeBallType.Ultra_Ball => 2f,
                PokeBallType.Master_Ball => 255f,
                PokeBallType.Fast_Ball => (poke != null && poke.Speed > 100)? 4f:1f,
                _ => 1f,
            };

        }

        public float GetCatchRateMod( Pokemon poke = null)
        {
            return GetCatchRateMod(this.type, poke);
        }


        public static void AdditionalEffect(Pokemon poke) { Debug.LogError("To be implemented"); }


        public override bool IsUsed(Pokemon target, int moveIndex = -1)
        {
            if(GameManager.Instance.State == GameState.Battle)
                return true;

            return false;
        }

        public override bool CanUseOutsideBattle => false;

    }

    public enum PokeBallType
    {
        pokeBall, Great_Ball, Ultra_Ball, Master_Ball, Fast_Ball

    }

}
