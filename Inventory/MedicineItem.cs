using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    [CreateAssetMenu(fileName = "New Medicine", menuName = "Item / Create new Medicine")]
    public class MedicineItem : Item
    {
        [Header("HP")]
        [SerializeField] RestoreHPMode hpRestore;
        [SerializeField] int hpAmount;
        [SerializeField] bool restoreFullHealth;
        [SerializeField] bool restoreHalf;

        [Header("PP")]
        [SerializeField] RestorePP ppRestoration;
        [SerializeField] int ppAmount;
        [SerializeField] bool restoreFullPP;

        [Header("Status conditions")]
        [SerializeField] ConditionID cureStatus;
        [SerializeField] bool recoverFromAllStatus;

        [Header("Revive")]
        [SerializeField] ReviveEffect revive;

        public RestorePP PpRestoration => ppRestoration;

        public override bool IsUsed(Pokemon target, int moveIndex)
        {

            if (hpRestore != RestoreHPMode.none && target.HP > 0 && target.HP < target.MaxHP)
            {
                int heal = 0;
                switch (hpRestore)
                {
                    case RestoreHPMode.fixed_value:
                        heal = hpAmount;
                        break;
                    case RestoreHPMode.full_health:
                        heal = target.MaxHP;
                        break;
                    case RestoreHPMode.half_health:
                        heal = target.MaxHP / 2;
                        break;
                    case RestoreHPMode.percentege:
                        heal = target.MaxHP * (hpAmount / 100);
                        break;
                }
                target.TakeHeal(heal);
                return true;
            }

            if (ppRestoration != RestorePP.none)
            {
                switch (ppRestoration)
                {
                    case RestorePP.one_move:
                        target.MoveSet[moveIndex].IncreasePP(ppAmount);
                        break;
                    case RestorePP.all_moves:
                        target.MoveSet.ForEach(m => m.IncreasePP(ppAmount)); 
                        break;
                    case RestorePP.full_one_move:
                        target.MoveSet[moveIndex].IncreasePP(target.MoveSet[moveIndex].Base.MaxPP);
                        break;
                    case RestorePP.full_all_moves:
                        target.MoveSet.ForEach(m => m.IncreasePP(m.Base.MaxPP)); 
                        break;
                    default:
                        break;
                }


                return true;
            }

            if (recoverFromAllStatus && (target.Status != null || target.Volat.Count > 0))
            {
                target.RecoverFromStatus();
                target.RemoveAllVolat();
                return true;
            }

            if (cureStatus != ConditionID.none && target.Status != null)
            {
                if (target.Status.Id == cureStatus)
                {
                    target.RecoverFromStatus();
                    return true;

                }
            }

            if (revive != ReviveEffect.none && target.HP == 0 && target.Status.Id == ConditionID.fnt)
            {
                target.RecoverFromStatus();
                int recovery = 0;

                switch (revive)
                {
                    case ReviveEffect.n_halfHP_restored:
                        recovery = Mathf.FloorToInt(target.MaxHP * .5f);
                        break;
                    case ReviveEffect.n_fullHP_restored:
                        recovery = target.MaxHP;
                        break;
                }
                target.TakeHeal(recovery);
                return true;
            }

            return false;
        }

        public override bool TryToUse(Pokemon target, Move move = null)
        {

            if (hpRestore != RestoreHPMode.none && target.HP > 0 && target.HP < target.MaxHP)
                return true;


            if (ppRestoration != RestorePP.none)
                return true;


            if (recoverFromAllStatus && (target.Status != null || target.Volat.Count > 0))
                return true;

            if (cureStatus != ConditionID.none && target.Status != null)
                return true;


            if (revive != ReviveEffect.none && target.HP == 0 && target.Status.Id == ConditionID.fnt)
                return true;

            return false;

        }
    }


}

public enum RestoreHPMode
{
    none, fixed_value, full_health, half_health, percentege
}

public enum RestorePP
{
    none, one_move, all_moves, full_one_move, full_all_moves
}

public enum ReviveEffect
{
    none, n_halfHP_restored, n_fullHP_restored,
}