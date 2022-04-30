using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;




[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MovesBase : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] string moveName;
    [TextArea]
    [SerializeField] string description;
    
    [SerializeField] MoveCategory category;
    [SerializeField] PokemonType type;
    [SerializeField] int maxPP;
    
    [SerializeField] float power;
    [SerializeField] float accuracy;

    [SerializeField] Target target;
    [Space]
    [SerializeField] bool makesContact;
    [Space]
    [SerializeField] MoveEffects moveEffects;
    [SerializeField] List<SecundaryEffects> secundaries;

    [Header("Move Proprities")]
    [SerializeField] bool alwaysHits;
    //[Range (-7,5)] 
    [SerializeField] int priority;
    [Space]
    [SerializeField] bool hasChargingTurn;
    [Space]
    [SerializeField] HitEffectsID effectWhenHit;


    [SerializeField] bool notAffectedByProtect;
    [Space]
    [Range(0,2)]
    [SerializeField] int increasedCritRatio;

    [Space]
    [Header ("Animations")]
    [SerializeField] AnimationClip opntAnimation;
    [SerializeField] AnimationClip userAnimation;
    [SerializeField] AnimationClip effectAnimation;

    public void OnValidate()
    {
        Priority = priority;
    }

    public void Awake()
    {
        moveName = name;
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize() => OnValidate();

    public string MoveName { get => moveName;}
    public int MaxPP { get { return maxPP; }  }
    public string Description { get => description; }
    public PokemonType Type { get => type;}
    public float Power { get => power;  }
    public float Accuracy { get => accuracy; }
    public MoveCategory Category { get => category;}
    public bool MakesContact { get => makesContact; }

    public int Priority { get => priority; private set => priority = Mathf.Clamp(value, -7, 5); }
    public bool AlwaysHits { get => alwaysHits;  }
    public MoveEffects Effects { get => moveEffects; }
    public Target Target { get => target;  }
    public bool NotAffectedByProtect { get => notAffectedByProtect; }
    public List<SecundaryEffects> Secundaries { get => secundaries;}
    public int IncreasedCritRatio { get => increasedCritRatio; }
    public HitEffectsID EffectWhenHit { get => effectWhenHit;  }
    public bool HasChargingTurn { get => hasChargingTurn; }
    public AnimationClip UserAnimation { get => userAnimation; }
    public AnimationClip OpntAnimation { get => opntAnimation;  }
    public AnimationClip EffectAnimation { get => effectAnimation;  }
}

public enum MoveCategory
{
    Physical,
    Special,
    Status
}

public enum Target
{
    Foe,//One Enemy
    Self,
    NotIMPL_Ally,//One next ally
    NotIMPL_Adj,//one ally and two enemies
    NotIMPL_Others,//Everyone but self
    NotIMPL_Opponents,//All foes
    NotIMPL_TeamAndSelf,//Allies and Self
    NotIMPL_Mates,//Only allies
    NotIMPL_Everyone,
}


[System.Serializable]
public class MoveEffects
{
    [SerializeField] ConditionID condition;
    [SerializeField] List<StatModsStage> statMods;



    public List<StatModsStage> StatMods { get { return statMods; } }

    public ConditionID Condition { get => condition; }

}


[System.Serializable]
public class SecundaryEffects : MoveEffects
{
    [Range (0,101)]
    [SerializeField] int statusChance;
    [SerializeField] Target effectTarget;
    [SerializeField] int antecipation;//remove and use a move propety called hasChargin turn



    public int StatusChance { get => statusChance; }
    public Target Target { get => effectTarget; }
    public int Antecipation { get => antecipation;}

}


[System.Serializable]
public class StatModsStage
{
    public Stat stat;
    [Range(-6,+6)]
    public int stage;
}

public enum Effect
{
    AtkUp,
    AtkDwn,
}

public static class ProtectiveSuccessTest
{
    const int outOf = 65536;

    const int rate = 65535;

    const int cap = 65535 / 4;

    static readonly float[] chanceTable = new float[] {rate/outOf, 32767 /outOf, 16383 /outOf, 8191 /outOf};

    public static bool HasSuccess(int consecUse)
    {
        int turnRate =Mathf.FloorToInt( rate / Mathf.Pow(2, consecUse));
        return (Mathf.Max( turnRate, rate/8  )) >= Random.Range(1, outOf);
    }

    public static bool SuccessFromTable(int turn)
    {
        return Random.value >= chanceTable[Mathf.Clamp(turn -1, 0, 3)];
    }

}

public static class RecoilTable//CONSIDER USING A DICTIONARY INSTEAD OF ARRAY
{

    static float[] recoilMod = new float[] { 0.25f, 0.33f, 0.5f };

    public static float GetRecoil(RecoilDmg mod)
    {
        return recoilMod[Mathf.Max((int)mod, 2)];
    }
}
public enum RecoilDmg
{
    halfOfDmg,  thirdOfDmg, quaterOfDmg,  quaterOfMaxHP
}