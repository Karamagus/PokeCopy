using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class Condition
    {
        public ConditionID Id { get; set; }

        public string Name { get; set; }
        public Color UIColor { get; set; }

        public string Description { get; set; }

        public string StartMessage { get; set; }

        public string FailledMassage { get; set; }
        public AnimationClip EffectAnimation { get; set; }

        public Unit Leecher { get; set; }

        public PokemonType TypeResited { get; set; }

        public string ResistenceMassage { get; set; }

        public Action<Pokemon, int> OnHit { get; set; }
        public Action<Unit, Unit> OnPlantSeed { get; set; }
        public Action<Pokemon> OnStart { get; set; }
        public Action<Unit> SetUnit { get; set; }

        public Func<Pokemon, bool> OnBeforeMove { get; set; }

        public Func<MoveCategory, float> GetOnDamageMod { get; set; }

        //ondamage function?

        public Action<Pokemon> OnAfterMove { get; set; }

        public Func<Pokemon, int> OnAfterAllTurns { get; set; }

        public Action<Pokemon> OnRemoveCondition { get; set; }
    }

    [Serializable]
    public class VolCondition : Condition
    {
        public int Timer { get; set; }
    }


    public class HitEffects
    {
        public HitEffectsID Id { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public Action<Pokemon, int> OnHit { get; set; }

    }

}
