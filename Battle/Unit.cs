using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public PokemonBase Species { get; private set; }
    public int lvl;
    [SerializeField] Pokemon unitPokemon;
    public Pokemon CrrPokemon { get { return unitPokemon; } }

    public Animator animator;
    //[SerializeField] new Animation animation;
    [SerializeField] AnimationClip aniclip;
    public ParticleSystem impact;
    public AnimatorOverrideController aniControl;

    [SerializeField] SpriteRenderer pImage;
    [SerializeField] SpriteMask mask;
    [SerializeField] BattleHud hud;
    [SerializeField] bool isPlayerOrAlly;
    public BattleHud BHud { get; set; }

    public string unitName;
    public int unitLevel;

    // public int damage;


    public Move MoveOfChoice { get; set; }
    public Move LastUsed { get; set; }


    public Dictionary<Stat, int> StatStages { get; set; }


    // public Queue<string> StatusChange { get; private set; } = new Queue<string>();

    PokemonType effctvType1;
    PokemonType effctvType2;

    public Unit Leecher { get; private set; }
    public bool IsPlayerOrAlly { get => isPlayerOrAlly; set => isPlayerOrAlly = value; }

    public void SetUpUnit(Pokemon p, BattleHud hud = null)
    {
        unitPokemon = p;
        Species = unitPokemon.Base;//

        unitName = ((unitPokemon.NickName.Length > 0) && !(unitPokemon.NickName.StartsWith(" ")) || (unitPokemon.NickName == null)) ? unitPokemon.NickName : unitPokemon.Species;
        name = unitName;

        effctvType1 = Species.Type1;
        effctvType2 = Species.Type2;

        SetBoosts();
        unitPokemon.RemoveAllVolat();
        Leecher = null;
        SetSprite();
        foreach (var m in unitPokemon.MoveSet)
            m.LastUsedTurn = 0;
        pImage.transform.localScale = Vector3.one;
        pImage.color = Color.white; 

        MoveOfChoice = null;
        if (hud != null && BHud == null)
            BHud = hud;
        BHud.SetHUD(this);
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        aniControl = (AnimatorOverrideController)animator.runtimeAnimatorController;
        animator.Play("Battle_EnemyBegin");

    }

    public void SetAnimatorControl(AnimatorOverrideController newControl)
    {
        animator.runtimeAnimatorController = newControl;
        aniControl = (AnimatorOverrideController)animator.runtimeAnimatorController;
    }

    void SetBoosts()
    {
        StatStages = new Dictionary<Stat, int>
        {
            { Stat.Attack, 0},
            { Stat.Defense, 0},
            { Stat.SpecialAtk, 0},
            { Stat.SpecialDef, 0},
            { Stat.Speed, 0},
            { Stat.Evasion, 0},
            { Stat.Accuracy, 0},
            { Stat.Critical, 0},
        };
    }

    public void ApplyStatStage(List<StatModsStage> changes)
    {
        List<Stat> highest = new List<Stat>();
        List<Stat> lowest = new List<Stat>();

        string quote = $"{unitName}'s ";
        int nameEndIndex = quote.Length;

        foreach (var st in changes)
        {
            var stat = st.stat;

            if (Mathf.Abs(StatStages[stat]) < 6)
                continue;

            if (StatStages[stat] > 0)
            {
                highest.Add(stat);
                continue;
            }
            lowest.Add(stat);
        }

        foreach (var st in changes)
        {
            if (st.stage == 0) continue;
            if (quote.Length > nameEndIndex)
                quote = quote.Remove(nameEndIndex);

            Stat stat = st.stat;
            var stage = StatStages[stat];
            var change = st.stage;

            if (highest.IndexOf(stat) > 0)
                continue;

            if (lowest.IndexOf(stat) > 0)
                continue;

            if (Mathf.Abs(stage) >= 6)
            {
                quote += "won't go ";

                if (highest.Contains(stat))
                    quote += "higher.";

                if (lowest.Contains(stat))
                    quote += "lower.";

                if (highest.Count > 1 || lowest.Count > 1)
                {
                    quote = quote.Insert(nameEndIndex, "stats ");
                    Debug.Log(quote.LastIndexOf("won't go "));
                    quote = quote.Insert(quote.LastIndexOf(" ") + 1, "any ");
                }
                else
                    quote = quote.Insert(nameEndIndex, ((lowest.Contains(stat)) ? $"{lowest[lowest.IndexOf(stat)]} " : $"{highest[highest.IndexOf(stat)]} "));

                CrrPokemon.StatusChangeQuote.Enqueue(quote);
                continue;
            }

            quote += $"{stat} ";

            StatStages[stat] = Mathf.Clamp(StatStages[stat] + change, -6, +6);
            // details.Add(new EffectDetails(StatStages[stat], stage));

            if (change > 1)
                quote += "sharply ";
            if (change > 0)
                quote += "rose!";

            if (change < -1)
                quote += "harshly ";
            if (change < 0)
                quote += "fell!";


            //play animation on battle sistem intead of here? won't be hidden but would nedd a return value from this function
            if (change > 0)
                animator.Play("Battle_EnemyUpStat");
            if (change < 0)
                animator.Play("Battle_EnemyDownStat");

            CrrPokemon.StatusChangeQuote.Enqueue(quote);
        }
    }

    public void ApplyCondition(ConditionID status, Unit leecher = null)
    {
        if (ConditionsDB.Conditions.ContainsKey(status))
            CrrPokemon.ReceiveStatus(status);

        if (ConditionsDB.VolatConditions.ContainsKey(status)) 
            CrrPokemon.ReceiveVolatile(status, this, leecher);
        //CrrPokemon.StatusChange.Enqueue($"{CrrPokemon._name} {CrrPokemon.Status.StartMessage}");
    }

    public bool SetLeecher(Unit root)
    {
        if (Leecher == null)
        {
            Leecher = root;
            return true;
        }
        return false;
    }

    //Is there a way to use conditions instead of the unit

    public void RemoveSeeds()
    {
        Leecher = null;
    }

    public IEnumerator LeechSeedEffect_CR()
    {
        var leechedHp = CrrPokemon.OnEndOfRoundEffects();
        if (leechedHp == null || Leecher == null)
            yield break;


        yield return BHud.WaitHpUpdat_CR();
        Leecher.CrrPokemon.TakeHeal(leechedHp.Value);
        yield return Leecher.BHud.WaitHpUpdat_CR();

    }

    void SetSprite()
    {
        if (!IsPlayerOrAlly)
        {
            pImage.sprite = Species.Front;
            mask.sprite = pImage.sprite;
            return;
        }
        pImage.gameObject.transform.localScale = Vector3.one;
        pImage.sprite = Species.Back;
        mask.sprite = pImage.sprite;

    }

    public int GetStat(Stat stat)
    {
        int statVal = CrrPokemon.GetBaseStat(stat);

        int stage = StatStages[stat];


        if (stage >= 0)
            return Mathf.FloorToInt(statVal * BoostTable.GetBoostValue(stage));

        return Mathf.FloorToInt(statVal / BoostTable.GetBoostValue(-stage));

    }

    int GetCritStat(Stat stat, float crit)
    {
        if (crit > 1f)
        {
            if ((stat == Stat.Attack || stat == Stat.SpecialAtk) && StatStages[stat] < 0)
                return CrrPokemon.GetBaseStat(stat);

            if ((stat == Stat.Defense || stat == Stat.SpecialDef) && StatStages[stat] > 0)
                return CrrPokemon.GetBaseStat(stat);
        }

        return GetStat(stat);
    }


    public DamageDetails TakeMoveDamage(Move move, Unit atker)
    {
        PokemonBase self = Species;
        PokemonBase atkerBase = atker.Species;

        int incrCrit = move.Base.IncreasedCritRatio;
        MoveCategory category = move.Base.Category;
        PokemonType moveType = move.Base.Type;

        //DAMAGE CACULUS

        float crit = ((BoostTable.GetCritBoost(StatStages[Stat.Critical] + incrCrit) * 100) > (Random.value * 100f)) ? 2f : 1f;

        int atk = (category == MoveCategory.Special) ? atker.GetCritStat(Stat.SpecialAtk, crit) : atker.GetCritStat(Stat.Attack, crit); ;
        int def = (category == MoveCategory.Special) ? GetCritStat(Stat.SpecialDef, crit) : GetCritStat(Stat.Defense, crit); 


        float eff = Effectiveness.GetAllEffectivenesses(moveType, self.Type1, self.Type2);
        float aff = Effectiveness.GetAllAffinities(moveType, atkerBase.Type1, atkerBase.Type2);
        // float burn = (atker.CrrPokemon.Status?.Id == ConditionID.brn && category == MoveCategory.Physical) ? .5f : 1f;

        float modifiers = Random.Range(0.85f, 1f) * eff * aff * crit;// * burn;

        float others = OtherDmgMods(atker, category);//


        //check calculus
        var lvlFactor = (2* atker.CrrPokemon.Level + 10) / 250f;
        float baseDmg = lvlFactor * move.Base.Power * (atk / def) + 2;

        int damage = Mathf.FloorToInt(baseDmg * modifiers * others);


        var damageDetails = new DamageDetails()
        {
            Effectiveness = eff,
            Critical = crit,
            Fainted = CrrPokemon.TakeDamage(damage),
            Damage = damage,
        };


        //OTHER EFFECTS

        //put in a separete function that deals with all changes in status caused by offensive moves
        if (CrrPokemon.Status?.Id == ConditionID.frz && moveType == PokemonType.Fire)
            CrrPokemon.RecoverFromStatus();


        //do recoil here using atker?

        return damageDetails;
    }

    float OtherDmgMods(Unit atker, MoveCategory category)
    {
        var mod = 1f;
        mod *= atker.CrrPokemon.Status?.GetOnDamageMod?.Invoke(category) ?? 1f;

        return mod;
    }

    public DamageDetails TakeFixedDamage(int damage)
    {
        var details = new DamageDetails()
        {
            Effectiveness = 1f,
            Critical = 1f,
            Fainted = CrrPokemon.TakeDamage(damage)
        };

        return details;
    }

    //Use a delegate to handle diferent behaviours.
    //Make a DB for diferent Behaves? Use Nature of a Pokemon to influence the liklyhood of use more offenssive attacks or more status moves etc.
    //Nature influence tatics that inlfuence the MoveOfChoice
    //use lambdas?
    public void SetMoveToUse(Unit opponent = null, UnitTatics trainnerTatic = UnitTatics.Random)
    {

        //get crrtPokemon nature if wild, else, use providaded trainer tatic, overriding nature behaviour.
        // opponet is used to addapt depending on the enemies actions, specialy last usedmove

        if (MoveOfChoice?.isCharged == false)
        {
            MoveOfChoice.isCharged = true;
            return;
        }

        MoveOfChoice = CrrPokemon.GetRandomMove();
        if (MoveOfChoice?.Base.HasChargingTurn == true)
            MoveOfChoice.isCharged = false;


        aniControl["Battle_EnemyAttack"] = MoveOfChoice.Base.OpntAnimation;

    }

    public void PlayImpact(object targetO)
    {
        var target = (Unit)targetO;
        target.animator.Play("BattlePlayer_Impact");
        target.impact.Play();
    }

    public void ResetSprite()
    {
        pImage.transform.localScale = Vector3.one;
        pImage.color = Color.white;

    }

}

public enum UnitTatics
{
    Random, Aggressive, Defensive, Suport,
}
