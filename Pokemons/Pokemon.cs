using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine.Events;
using pokeCopy;

namespace pokeCopy
{

    [System.Serializable]
    public class Pokemon : ISerializationCallbackReceiver
    {

        [SerializeField] PokemonBase _base;
        [SerializeField] int level;

        [SerializeField] private string nickName;
        Gender gender;

        public PokemonBase Base { get => _base; }
        public string Species { get; private set; }
        public string NickName
        {
            get => (nickName.Length > 0 && !nickName.StartsWith(" ")) ? nickName : Species;
            set => nickName = value ?? "";
        }


        public int Level { get => level; private set => level = Mathf.Clamp(value, 1, 100); }

        public int Exp { get; set; }


        bool didLeveledUp;

        [SerializeField][ReadOnly] int hp;
        public int HP { get { return hp; } private set { hp = Mathf.Max(value, 0); } }

        public List<Move> MoveSet { get; set; }

        public Dictionary<Stat, int> Stats { get; private set; }

        public Dictionary<Stat, float> StatMods { get; private set; }
        public Queue<string> StatusChangeQuote { get; private set; }

        public float spDmgMod { get; set; }
        public float phDmgMod { get; set; }


        public Condition Status { get; private set; }

        public int StatusTimer { get; set; }

        public List<VolCondition> Volat { get; private set; }
        public List<int> VolatTimers { get; private set; }


        public delegate void HpChanges();
        public delegate void StatusChanged();

        public HpChanges OnHPChange;
        public StatusChanged OnStatusChanged;

        [SerializeField] bool hasFixedNature;
        [SerializeField] NatureID nature;

        public void OnValidate() => Level = level;


        public Pokemon(PokemonBase pBase, int pLevel, string nick = null)
        {
            _base = pBase;
            Level = pLevel;
            NickName = nick;
            Init();
        }


        public Pokemon(PokemonSaveData saveData)
        {
            _base = PokemonDB.GetDataByName(saveData.species);
            Species = Base.name.Substring(Base.name.IndexOfAny(new char[] { '_', ' ' }) + 1);
            nature = saveData.nature;
            gender = saveData.gender;
            level = saveData.level;
            Exp = saveData.xp;
            NickName = saveData.nickName;
            CalculateStats();
            hp = saveData.hp;

            if (saveData.status != null)
                Status = ConditionsDB.Conditions[saveData.status.Value];
            else
                Status = null;

            MoveSet = saveData.moves.Select(m => new Move(m)).ToList();

            StatusChangeQuote = new Queue<string>();
            Volat = new List<VolCondition>();
            VolatTimers = new List<int>();
            RemoveAllVolat();


            SetStatsMods();

        }

        public PokemonSaveData GetSaveData()
        {
            var saveData = new PokemonSaveData()
            {
                species = Base.name,
                nickName = this.nickName,
                gender = this.gender,
                hp = HP,
                level = Level,
                xp = Exp,
                status = Status?.Id,
                nature = this.nature,
                moves = MoveSet.Select(m => m.GetSaveData()).ToList()

            };


            return saveData;
        }

        public void Init(bool randomNature = true)
        {

            //Species = _Base.name.Remove(0, _Base.name.IndexOfAny(new char[] { '_', ' ' })+1);
            Species = Base.name.Substring(Base.name.IndexOfAny(new char[] { '_', ' ' }) + 1);
            if (randomNature && !hasFixedNature)
                nature = (NatureID)Random.Range(0, 24);
            //nature = (NatureID)(Mathf.Abs( GetHashCode()) % 24);

            gender = GenderGenerator.GetPokemondGender(_base);

            StatusChangeQuote = new Queue<string>();

            Status = null;
            Volat = new List<VolCondition>();
            VolatTimers = new List<int>();
            RemoveAllVolat();


            MoveSet = new List<Move>();
            foreach (var move in Base.Leanrnables)
            {
                if (MoveSet.Count >= PokemonBase.MAX_MOVES_COUNT || move.BaseMove == null)
                    break;

                if (move.Level <= Level)
                    MoveSet.Add(new Move(move.BaseMove));

            }


            Exp = Base.GetExpForLevel(level);


            CalculateStats();
            HP = MaxHP;
            SetStatsMods();

            spDmgMod = 1f;
            phDmgMod = 1f;
        }

        public bool HasLeveledUp()
        {
            if (Exp < Base.GetExpForLevel(level + 1) || level >= 100)
                return false;

            ++level;
            didLeveledUp = true;
            RecaulculateStatsAndHP();
            return true;

        }

        public ByLevelMoves GetLearnableMoveAtLevel()
        {
            return Base.Leanrnables.Where(x => x.Level == level).FirstOrDefault();
        }

        public void LearnMove(MovesBase moveToLearn)
        {
            if (MoveSet.Count >= PokemonBase.MAX_MOVES_COUNT)
                return;
            MoveSet.Add(new Move(moveToLearn));
        }




        void CalculateStats()
        {
            var natureMods = PokeNature.GetNatureModfier(nature);

            //Add nature modfier
            Stats = new Dictionary<Stat, int>();
            Stats.Add(Stat.Attack, Mathf.FloorToInt((((Base.Attack * Level) / 100f) + 5) * natureMods[(int)Stat.Attack]));
            Stats.Add(Stat.Defense, Mathf.FloorToInt((((Base.Defense * Level) / 100f) + 5) * natureMods[(int)Stat.Defense]));
            Stats.Add(Stat.SpecialAtk, Mathf.FloorToInt((((Base.SpAttack * Level) / 100f) + 5) * natureMods[(int)Stat.SpecialAtk]));
            Stats.Add(Stat.SpecialDef, Mathf.FloorToInt((((Base.SpDefense * Level) / 100f) + 5) * natureMods[(int)Stat.SpecialDef]));
            Stats.Add(Stat.Speed, Mathf.FloorToInt((((Base.Speed * Level) / 100f) + 5) * natureMods[(int)Stat.Speed]));

            //var oldMaxHP = MaxHP;
            MaxHP = Mathf.FloorToInt(((Base.MaxHP * Level) / 100f) + Level + 10);
            //TakeHeal(MaxHP - oldMaxHP);
        }

        void RecaulculateStatsAndHP()
        {
            var oldHp = MaxHP;
            CalculateStats();
            TakeHeal(MaxHP - oldHp);
        }


        void SetStatsMods()
        {
            StatMods = new Dictionary<Stat, float>()
        {
            {Stat.Attack, 1f },
            {Stat.Defense, 1f },
            {Stat.SpecialAtk, 1f },
            {Stat.SpecialDef, 1f },
            {Stat.Speed, 1f },
            {Stat.Accuracy, 1f },
            {Stat.Evasion, 1f },
            {Stat.Critical, 1f },
        };
        }


        public bool HasMove(MovesBase moveCheck)
        {
            return MoveSet.Count(m => m.Base == moveCheck) > 0;
        }

        public EvolutionCriteria CanEvolveInto()
        {

            return (didLeveledUp)? Base.Evolutions.FirstOrDefault(e => e.RequiredLevel <= Level): null ;
        }

        public void Evolve(EvolutionCriteria evolution)
        {
            if (!Base.Evolutions.Contains(evolution))
                return;

            _base = evolution.Evolution;

            RecaulculateStatsAndHP();
            Species = Base.name.Substring(Base.name.IndexOfAny(new char[] { '_', ' ' }) + 1);
            didLeveledUp = false;
            /*
            var oldHp = MaxHP;
            CalculateStats();
            TakeHeal(MaxHP - oldHp);
            */
        }

        public int GetBaseStat(Stat stat)
        {

            if (stat == Stat.Speed && Status?.Id == ConditionID.par)
                return Mathf.FloorToInt(Stats[stat] * 0.75f);

            return Mathf.FloorToInt(Stats[stat] * StatMods[stat]);

            /* Using boosts on pokemon intead of battleUnit
            int stage = StatsBoost[stat];

            if (stage > 0)
                return Mathf.FloorToInt(statVal * BoostTable.GetBoostValue(stage));

            return Mathf.FloorToInt(statVal * BoostTable.GetBoostValue(-stage));
            */
        }

        public int Attack
        {
            get { return Stats[Stat.Attack]; }
        }
        public int Defense
        {
            get { return Stats[Stat.Defense]; }
        }
        public int SpAttack
        {
            get { return Stats[Stat.SpecialAtk]; }
        }
        public int SpDefense
        {
            get { return Stats[Stat.SpecialDef]; }
        }
        public int Speed
        {
            get { return Stats[Stat.Speed]; }
        }
        public int MaxHP { get; private set; }
        public Gender Gender => gender;

        public bool DoesLevelUp { get => didLeveledUp; set => didLeveledUp = value; }

        //public Nature Nature { get => nature; set => nature = value; }

        public bool TakeMoveDamage(Move move, Pokemon attacker)
        {

            float eff = Effectiveness.GetAllEffectivenesses(move.Base.Type, Base.Type1, Base.Type2);
            float aff = Effectiveness.GetAllAffinities(move.Base.Type, attacker.Base.Type1, attacker.Base.Type2);
            float modifiers = Random.Range(.85f, 1f) * eff * aff;

            float atk = (move.Base.Category == MoveCategory.Special) ? attacker.GetBaseStat(Stat.SpecialAtk) : attacker.GetBaseStat(Stat.Attack);
            float def = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;


            float l = (2 * Level + 10) / 250;
            float bdmg = l * move.Base.Power * (atk / def) + 2;
            int damage = Mathf.FloorToInt(bdmg * modifiers);


            HP -= damage;
            OnHPChange?.Invoke();

            if (HP <= 0)
            {
                HP = 0;
                return true;
            }
            return false;
        }

        public bool TakeConfusionDmg()
        {

            float burn = (Status?.Id == ConditionID.brn) ? .5f : 1f;
            float l = (2 * Level + 10) / 250f;

            float bdmg = l * 40 * (GetBaseStat(Stat.Attack) / GetBaseStat(Stat.Defense)) + 2;
            int damage = Mathf.FloorToInt(bdmg * Random.Range(0.85f, 1f) * burn);

            TakeDamage(damage);

            return true;
        }

        public bool TakeDamage(int damage)
        {
            HP = Mathf.Max(HP - damage, 0);

            OnHPChange?.Invoke();

            if (HP <= 0)
                return true;

            return false;
        }

        public void TakeHeal(int healAmount)
        {
            HP = Mathf.Min((HP + healAmount), MaxHP);

            OnHPChange?.Invoke();
        }

        public void FullRecovery()
        {
            TakeHeal(MaxHP);
            Status = null;
            OnStatusChanged?.Invoke();
            RemoveAllVolat();
        }

        public void RestorePPByMove(int ppAmount, Move move)
        {
            if (MoveSet.Contains(move))
            {
                move.Pp += ppAmount;
            }
        }

        public void RestorePPAllMoves(int ppAmount)
        {
            foreach (var m in MoveSet)
            {
                m.Pp += ppAmount;
            }
        }

        public void ApplyCondition(ConditionID id)
        {
            if (ConditionsDB.Conditions.ContainsKey(id))
                ReceiveStatus(id);

            //if (ConditionsDB.VolatConditions.ContainsKey(id))
            //ReceiveVolatile(id)
        }

        public void ReceiveStatus(ConditionID status)
        {
            if (Status != null && status != ConditionID.fnt)
            {
                StatusChangeQuote.Enqueue($"{NickName} is not affected.");

                return;
            }



            PokemonType resistence = ConditionsDB.Conditions[status].TypeResited;
            if (resistence != PokemonType.None && (resistence == Base.Type1 || resistence == Base.Type2))
            {
                StatusChangeQuote.Enqueue($"{NickName} {ConditionsDB.Conditions[status].ResistenceMassage}");
                return;
            }

            Status = ConditionsDB.Conditions[status];
            Status?.OnStart?.Invoke(this);
            StatusChangeQuote.Enqueue($"{NickName} {Status.StartMessage}");
            if (status != ConditionID.fnt)
                OnStatusChanged?.Invoke();

        }

        public void RecoverFromStatus()
        {
            Status?.OnRemoveCondition?.Invoke(this);
            Status = null;
            OnStatusChanged?.Invoke();

        }

        public void ReceiveVolatile(ConditionID volCond, Unit self = null, Unit leecher = null)
        {

            if (Volat.Contains(ConditionsDB.VolatConditions[volCond]))
            {
                if (ConditionsDB.VolatConditions[volCond].FailledMassage != null)
                    StatusChangeQuote.Enqueue(ConditionsDB.VolatConditions[volCond].FailledMassage);
                return;
            }

            var resistence = ConditionsDB.VolatConditions[volCond].TypeResited;
            if (resistence != PokemonType.None && (resistence == Base.Type1 || resistence == Base.Type2))
            {
                StatusChangeQuote.Enqueue($"{NickName} {ConditionsDB.VolatConditions[volCond].ResistenceMassage}");
                return;
            }

            Volat.Add(ConditionsDB.VolatConditions[volCond]);
            VolatTimers.Add(Volat[Volat.Count - 1].Timer);

            Volat[Volat.Count - 1]?.OnPlantSeed?.Invoke(self, leecher);
            Volat[Volat.Count - 1]?.OnStart?.Invoke(this);
            if (Volat[Volat.Count - 1].StartMessage != "")
                StatusChangeQuote.Enqueue($"{NickName} {Volat[Volat.Count - 1].StartMessage}");

        }

        public void RecoverFromVolat(ConditionID volCond)
        {
            VolatTimers.RemoveAt(Volat.IndexOf(ConditionsDB.VolatConditions[volCond]));
            Volat.Remove(ConditionsDB.VolatConditions[volCond]);
        }

        public void RemoveAllVolat()
        {
            VolatTimers.Clear();
            Volat.Clear();
        }

        public bool BeforeTurnEffects()
        {
            bool canPeformMove = true;

            if (Volat.Contains(ConditionsDB.VolatConditions[ConditionID.flinch]))//change this
            {
                if (!Volat[Volat.IndexOf(ConditionsDB.VolatConditions[ConditionID.flinch])].OnBeforeMove(this))
                {
                    canPeformMove = false;

                }
            }

            if (Volat != null && Volat.Count > 0)
            {
                for (int i = 0; i < Volat.Count; i++)
                {
                    var prvsCount = Volat.Count;
                    if (Volat[i].OnBeforeMove != null)
                        if (!Volat[i].OnBeforeMove(this) && canPeformMove)
                        {
                            canPeformMove = false;
                        }

                    if (prvsCount > Volat.Count)
                        i--;

                }
            }

            if (Status?.OnBeforeMove != null)
            {
                if (!Status.OnBeforeMove(this) && canPeformMove)
                    canPeformMove = false;
            }

            return canPeformMove;
        }

        public void AfterTurnEffects()
        {
            Status?.OnAfterMove?.Invoke(this);

            if (Volat != null && Volat.Count > 0)
            {
                for (int i = 0; i < Volat.Count; i++)
                {
                    var prvsCount = Volat.Count;

                    Volat[i]?.OnAfterMove?.Invoke(this);
                    if (prvsCount > Volat.Count)
                        i--;

                }
            }
        }


        public int? OnEndOfRoundEffects()
        {
            int? residual = null;
            if (Volat != null && Volat.Count > 0)
            {
                for (int i = 0; i < Volat.Count; i++)
                {
                    var prvsCount = Volat.Count;

                    residual = Volat[i]?.OnAfterAllTurns?.Invoke(this);
                    if (prvsCount > Volat.Count)
                        i--;

                }

            }
            return residual;
        }

        public Move GetRandomMove()
        {
            var moveWithPP = MoveSet.Where(x => x.Pp > 0).ToList();

            int m = Random.Range(0, MoveSet.Count);
            return moveWithPP[m];
        }

        public void EndOfBattle() { }

        public void OnBeforeSerialize() => this.OnValidate();

        public void OnAfterDeserialize() { }




    }

    [System.Serializable]
    public class PokemonSaveData
    {
        public string species;
        public string nickName;
        public Gender gender;
        public int hp;
        public int level;
        public int xp;
        public ConditionID? status;
        public NatureID nature;
        public List<MoveSaveData> moves;
    }



}

