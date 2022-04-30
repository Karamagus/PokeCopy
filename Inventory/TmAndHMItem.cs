using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace pokeCopy
{
    [CreateAssetMenu(menuName = "Item/Create new TM or HM")]
    public class TmAndHMItem : Item
    {
        [SerializeField] MovesBase move;
        [SerializeField] bool isHM;

        public MovesBase Move => move;

        public override bool IsUsed(Pokemon target, int moveIndex = -1)
        {

            if (target.MoveSet.Count < PokemonBase.MAX_MOVES_COUNT)
                target.LearnMove(this.move);


            if (moveIndex < PokemonBase.MAX_MOVES_COUNT && moveIndex > -1)
            {
                target.MoveSet[moveIndex] = new Move(this.move);
            }

            var result = target.HasMove(Move) && !isHM;
            return result;
        }

        public override bool TryToUse(Pokemon target, Move move = null)
        {



            return target.Base.TM_n_HM_Leanrnables.Contains(this.move);
        }


        public override bool CanUseInBattle => false;

        public override bool IsReusable => isHM;

        public bool IsHM => isHM;

    }
}
