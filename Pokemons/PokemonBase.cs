using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace pokeCopy
{

    [CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]
    public class PokemonBase : ScriptableObject
    {

        [TextArea]
        [SerializeField] string description;
        [SerializeField] byte catchRate = 255;
        [SerializeField] int xpYield;
        [SerializeField] Species_Gender_Threshold genderDistribution;
        [SerializeField] LevelingRate growth;
        [Header("Sprites")]
        [SerializeField] Sprite front;
        [SerializeField] Sprite back;
        [SerializeField] Sprite chibi;
        [Header("Types")]
        [SerializeField] PokemonType type1;
        [SerializeField] PokemonType type2;
        [Header("Base Stats")]
        [SerializeField] int maxHP;
        [SerializeField] int attack;
        [SerializeField] int defense;
        [SerializeField] int spAttack;
        [SerializeField] int spDefense;
        [SerializeField] int speed;
        [Space]
        [SerializeField] List<ByLevelMoves> leanrnables;
        [SerializeField] List<MovesBase> TM_n_HM;
        [SerializeField] List<EvolutionCriteria> evolutions;



        public string Description { get => description; }

        public Sprite Front { get => front; }
        public Sprite Back { get => back; }
        public Sprite Chibi => chibi;
        public PokemonType Type1 { get => type1; }
        public PokemonType Type2 { get => type2; }



        public int MaxHP { get => maxHP; }
        public int Attack { get { return attack; } }
        public int Defense { get => defense; }
        public int SpAttack { get => spAttack; }
        public int SpDefense { get => spDefense; }
        public int Speed { get => speed; }
        public List<ByLevelMoves> Leanrnables { get => leanrnables; }

        public static int MAX_MOVES_COUNT => 4;

        public byte CatchRate => catchRate;

        public int ExpYield => xpYield;

        public LevelingRate Growth => growth;

        public List<MovesBase> TM_n_HM_Leanrnables => TM_n_HM;

        public Species_Gender_Threshold GenderThreshold => genderDistribution;

        public List<EvolutionCriteria> Evolutions => evolutions;

        public int GetExpForLevel(int level)
        {
            return ExpTable.GetXPFromLevel(level, growth);
        }
    }

    [System.Serializable]
    public class ByLevelMoves : ISerializationCallbackReceiver
    {
        [SerializeField] MovesBase baseMove;
        [SerializeField] int level;

        public MovesBase BaseMove { get => baseMove; }
        public int Level { get => level; private set => level = Mathf.Clamp(value, 1, 100); }

        public void OnAfterDeserialize() { }

        public void OnBeforeSerialize() => OnValidate();

        public void OnValidate()
        {
            Level = level;
        }


    }

    public enum PokemonType
    {
        None,
        Normal,
        Fighting,
        Flying,
        Poison,
        Ground,
        Rock,
        Bug,
        Ghost,
        Steel,
        Fire,
        Water,
        Grass,
        Electric,
        Psychic,
        Ice,
        Dragon,
        Dark,
        Fairy,
    }


    public enum Stat
    {
        Attack,
        Defense,
        SpecialAtk,
        SpecialDef,
        Speed,
        Evasion,
        Accuracy,
        Critical,
    }



    public struct DamageDetails
    {
        public bool Fainted { get; set; }
        public float Critical { get; set; }
        public float Effectiveness { get; set; }

        public int Damage { get; set; }

        public int OnUserSideEffect { get; set; }


    }


    public class Effectiveness
    {
        static readonly float[][] chart =
        {
        // row = atk, col =def     NOR  FGT  FLG  POS  GRD  RCK  BUG  GHT  STL  FIR  WTR  GRS  ELC  PSY  ICE  DRG  DRK  FAR
		/* NOR */   new float[] {  1f,  1f,  1f,  1f,  1f, .5f,  1f,  0f, .5f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f }, 
		/* FGT */   new float[] {  2f,  1f, .5f, .5f,  1f,  2f, .5f,  0f,  2f,  1f,  1f,  1f,  1f, .5f,  2f,  1f,  2f, .5f }, 
		/* FLG*/    new float[] {  1f,  2f,  1f,  1f,  1f, .5f,  2f,  1f, .5f,  1f,  1f,  2f, .5f,  1f,  1f,  1f,  1f,  1f }, 
		/* POS */   new float[] {  1f,  1f,  1f, .5f, .5f, .5f,  1f, .5f,  0f,  1f,  1f,  2f,  1f,  1f,  1f,  1f,  1f,  2f }, 
		/* GRD */   new float[] {  1f,  1f,  0f,  2f,  1f,  2f, .5f,  1f,  2f,  2f,  1f, .5f,  2f,  1f,  1f,  1f,  1f,  1f }, 
		/* RCK*/    new float[] {  1f, .5f,  2f,  1f, .5f,  1f,  2f,  1f, .5f,  2f,  1f,  1f,  1f,  1f,  2f,  1f,  1f,  1f }, 
		/* BUG */   new float[] {  1f, .5f, .5f, .5f,  1f,  1f,  1f, .5f, .5f, .5f,  1f,  2f,  1f,  2f,  1f,  1f,  2f, .5f }, 
		/* GHT */   new float[] {  0f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f, .5f,  1f }, 
		/* STL */   new float[] {  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f, .5f, .5f, .5f,  1f, .5f,  1f,  2f,  1f,  1f,  2f }, 
		/* FIR */   new float[] {  1f,  1f,  1f,  1f,  1f, .5f,  2f,  1f,  2f, .5f, .5f,  2f,  1f,  1f,  2f, .5f,  1f,  1f }, 
		/* WTR */   new float[] {  1f,  1f,  1f,  1f,  2f,  2f,  1f,  1f,  1f,  2f, .5f, .5f,  1f,  1f,  1f, .5f,  1f,  1f }, 
		/* GRS */   new float[] {  1f,  1f, .5f, .5f,  2f,  2f, .5f,  1f, .5f, .5f,  2f, .5f,  1f,  1f,  1f, .5f,  1f,  1f }, 
		/* ELE */   new float[] {  1f,  1f,  2f,  1f,  0f,  1f,  1f,  1f,  1f,  1f,  2f, .5f, .5f,  1f,  1f, .5f,  1f,  1f }, 
		/* PSY */   new float[] {  1f,  2f,  1f,  2f,  1f,  1f,  1f,  1f, .5f,  1f,  1f,  1f,  1f, .5f,  1f,  1f,  0f,  1f }, 
		/* ICE */   new float[] {  1f,  1f,  2f,  1f,  2f,  1f,  1f,  1f, .5f, .5f, .5f,  2f,  1f,  1f, .5f,  2f,  1f,  1f }, 
		/* DRG */   new float[] {  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f, .5f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  0f }, 
		/* DRK */   new float[] {  1f, .5f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f, .5f, .5f }, 
		/* FAR */   new float[] {  1f,  2f,  1f, .5f,  1f,  1f,  1f,  1f, .5f, .5f,  1f,  1f,  1f,  1f,  1f,  2f,  2f,  1f },

    };
        public static float GetEffectivenes(PokemonType attackType, PokemonType defenseType)
        {
            if (attackType == PokemonType.None || defenseType == PokemonType.None)
                return 1;

            //int row = (int)attackType - 1;
            //int col = (int)defenseType - 1;

            return chart[((int)attackType) - 1][(int)defenseType - 1];
        }

        public static float GetAllEffectivenesses(PokemonType attackType, PokemonType defense1, PokemonType defense2)
        {
            float t1 = GetEffectivenes(attackType, defense1);
            float t2 = GetEffectivenes(attackType, defense2);

            return (defense1 != defense2) ? t1 * t2 : t1;
        }

        public static float GetAffinityMod(PokemonType move, PokemonType pokemonType)
        {
            return (move == pokemonType) ? 1.5f : 1f;
        }

        public static float GetAllAffinities(PokemonType move, PokemonType type1, PokemonType type2)
        {
            float t1 = GetAffinityMod(move, type1);
            float t2 = GetAffinityMod(move, type2);

            return Mathf.Max(t1, t2);
        }
    }



    public static class BoostTable
    {
        static readonly float[] values = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        static readonly float[] hitValues = new float[] { .33f, .36f, .43f, .5f, .6f, .75f, 1f, 1.33f, 1.66f, 2f, 2.5f, 2.66f, 3f };

        static readonly float[] critBonus = new float[] { 0.0625f, 0.125f, 0.25f, 0.333f, 0.5f, 1f };

        public static float GetBoostValue(int stage)
        {
            return values[stage];
        }

        public static float GetPrecision(int stage)
        {
            return hitValues[stage + 6];
        }

        public static float GetCritBoost(int stage)
        {
            return critBonus[Mathf.Clamp(stage, 0, 4)];
        }

    }

    public static class ExpTable
    {

        static readonly int[][] expTable =
        {
        /* 1 */ new int[] { 0, 0, 0, 0, 0, 0 },
        /* 2 */ new int[] { 15, 6, 8, 9, 10, 4 },
        /* 3 */ new int[] { 52, 21, 27, 57, 33, 13 },
        /* 4 */ new int[] { 122, 51, 64, 96, 80, 32 },
        /* 5 */ new int[] { 237, 100, 125, 135, 156, 65 },
        /* 6 */ new int[] { 406, 172, 216, 179, 270, 112 },
        /* 7 */ new int[] { 637, 274, 343, 236, 428, 178 },
        /* 8 */ new int[] { 942, 409, 512, 314, 640, 276, },
        /* 9 */ new int[] { 1326, 583, 729, 419, 911, 393, },
        /* 10 */ new int[] { 1800, 800, 1000, 560, 1250, 540, },
        /* 11 */ new int[] { 2369, 1064, 1331, 742, 1663, 745, },
        /* 12 */ new int[] { 3041, 1382, 1728, 973, 2160, 967, },
        /* 13 */ new int[] { 3822, 1757, 2197, 1261, 2746, 1230 },
        /* 14 */ new int[] { 4719, 2195, 2744, 1612, 3430, 1591 },
        /* 15 */ new int[] { 5737, 2700, 3375, 2035, 4218, 1957 },
        /* 16 */ new int[] { 6881, 3276, 4096, 2535, 5120, 2457 },
        /* 17 */ new int[] { 8155, 3930, 4913, 3120, 6141, 3046 },
        /* 18 */ new int[] { 9564, 4665, 5832, 3798, 7290, 3732 },
        /* 19 */ new int[] { 11111, 5487, 6859, 4575, 8573, 4526 },
        /* 20 */ new int[] { 12800, 6400, 8000, 5460, 10000, 5440 },
        /* 21 */ new int[] { 14632, 7408, 9261, 6458, 11576, 6482 },
        /* 22 */new int[] { 16610, 8518, 10648, 7577, 13310, 7666 },
        /* 23 */new int[] { 18737, 9733, 12167, 8825, 15208, 9003 },
        /* 24 */new int[] { 21012, 11059, 13824, 10208, 17280, 10506 },
        /* 25 */new int[] { 23437, 12500, 15625, 11735, 19531, 12187 },
        /* 26 */new int[] { 26012, 14060, 17576, 13411, 21970, 14060 },
        /* 27 */new int[] { 28737, 15746, 19683, 15244, 24603, 16140 },
        /* 28 */new int[] { 31610, 17561, 21952, 17242, 27440, 18439 },
        /* 29 */new int[] { 34632, 19511, 24389, 19411, 30486, 20974 },
        /* 30 */new int[] { 37800, 21600, 27000, 21760, 33750, 23760 },
        /* 31 */new int[] { 41111, 23832, 29791, 24294, 37238, 26811 },
        /* 32 */new int[] { 44564, 26214, 32768, 27021, 40960, 30146 },
        /* 33 */new int[] { 48155, 28749, 35937, 29949, 44921, 33780 },
        /* 34 */new int[] { 51881, 31443, 39304, 33084, 49130, 37731 },
        /* 35 */new int[] { 55737, 34300, 42875, 36435, 53593, 42017 },
        /* 36 */new int[] { 59719, 37324, 46656, 40007, 58320, 46656 },
        /* 37 */new int[] { 63822, 40522, 50653, 43808, 63316, 50653 },
        /* 38 */new int[] { 68041, 43897, 54872, 47846, 68590, 55969 },
        /* 39 */new int[] { 72369, 47455, 59319, 52127, 74148, 60505 },
        /* 40 */new int[] { 76800, 51200, 64000, 56660, 80000, 66560 },
        /* 41 */new int[] { 81326, 55136, 68921, 61450, 86151, 71677 },
        /* 42 */new int[] { 85942, 59270, 74088, 66505, 92610, 78533 },
        /* 43 */new int[] { 90637, 63605, 79507, 71833, 99383, 84277 },
        /* 44 */new int[] { 95406, 68147, 85184, 77440, 106480, 91998 },
        /* 45 */new int[] { 100237, 72900, 91125, 83335, 113906, 98415 },
        /* 46 */new int[] { 105122, 77868, 97336, 89523, 121670, 107069 },
        /* 47 */new int[] { 110052, 83058, 103823, 96012, 129778, 114205 },
        /* 48 */new int[] { 115015, 88473, 110592, 102810, 138240, 123863 },
        /* 49 */new int[] { 120001, 94119, 117649, 109923, 147061, 131766 },
        /* 50 */new int[] { 125000, 100000, 125000, 117360, 156250, 142500 },
        /* 51 */new int[] { 131324, 106120, 132651, 125126, 165813, 151222 },
        /* 52 */new int[] { 137795, 112486, 140608, 133229, 175760, 163105 },
        /* 53 */new int[] { 144410, 119101, 148877, 141677, 186096, 172697 },
        /* 54 */new int[] { 151165, 125971, 157464, 150476, 196830, 185807 },
        /* 55 */new int[] { 158056, 133100, 166375, 159635, 207968, 196322 },
        /* 56 */new int[] { 165079, 140492, 175616, 169159, 219520, 210739 },
        /* 57 */new int[] { 172229, 148154, 185193, 179056, 231491, 222231 },
        /* 58 */new int[] { 179503, 156089, 195112, 189334, 243890, 238036 },
        /* 59 */new int[] { 186894, 164303, 205379, 199999, 256723, 250562 },
        /* 60 */new int[] { 194400, 172800, 216000, 211060, 270000, 267840 },
        /* 61 */new int[] { 202013, 181584, 226981, 222522, 283726, 281456 },
        /* 62 */new int[] { 209728, 190662, 238328, 234393, 297910, 300293 },
        /* 63 */new int[] { 217540, 200037, 250047, 246681, 312558, 315059 },
        /* 64 */new int[] { 225443, 209715, 262144, 259392, 327680, 335544 },
        /* 65 */new int[] { 233431, 219700, 274625, 272535, 343281, 351520 },
        /* 66 */new int[] { 241496, 229996, 287496, 286115, 359370, 373744 },
        /* 67 */new int[] { 249633, 240610, 300763, 300140, 375953, 390991 },
        /* 68 */new int[] { 257834, 251545, 314432, 314618, 393040, 415050 },
        /* 69 */new int[] { 267406, 262807, 328509, 329555, 410636, 433631 },
        /* 70 */new int[] { 276458, 274400, 343000, 344960, 428750, 459620 },
        /* 71 */new int[] { 286328, 286328, 357911, 360838, 447388, 479600 },
        /* 72 */new int[] { 296358, 298598, 373248, 377197, 466560, 507617 },
        /* 73 */new int[] { 305767, 311213, 389017, 394045, 486271, 529063 },
        /* 74 */new int[] { 316074, 324179, 405224, 411388, 506530, 559209 },
        /* 75 */new int[] { 326531, 337500, 421875, 429235, 527343, 582187 },
        /* 76 */new int[] { 336255, 351180, 438976, 447591, 548720, 614566 },
        /* 77 */new int[] { 346965, 365226, 456533, 466464, 570666, 639146 },
        /* 78 */new int[] { 357812, 379641, 474552, 485862, 593190, 673863 },
        /* 79 */new int[] { 367807, 394431, 493039, 505791, 616298, 700115 },
        /* 80 */new int[] { 378880, 409600, 512000, 526260, 640000, 737280 },
        /* 81 */new int[] { 390077, 425152, 531441, 547274, 664301, 765275 },
        /* 82 */new int[] { 400293, 441094, 551368, 568841, 689210, 804997 },
        /* 83 */new int[] { 411686, 457429, 571787, 590969, 714733, 834809 },
        /* 84 */new int[] { 423190, 474163, 592704, 613664, 740880, 877201 },
        /* 85 */new int[] { 433572, 491300, 614125, 636935, 767656, 908905 },
        /* 86 */new int[] { 445239, 508844, 636056, 660787, 795070, 954084 },
        /* 87 */new int[] { 457001, 526802, 658503, 685228, 823128, 987754 },
        /* 88 */new int[] { 467489, 545177, 681472, 710266, 851840, 1035837 },
        /* 89 */new int[] { 479378, 563975, 704969, 735907, 881211, 1071552 },
        /* 90 */new int[] { 491346, 583200, 729000, 762160, 911250, 1122660 },
        /* 91 */new int[] { 501878, 602856, 753571, 789030, 941963, 1160499 },
        /* 92 */new int[] { 513934, 622950, 778688, 816525, 973360, 1214753 },
        /* 93 */new int[] { 526049, 643485, 804357, 844653, 1005446, 1254796 },
        /* 94 */new int[] { 536557, 664467, 830584, 873420, 1038230, 1312322 },
        /* 95 */new int[] { 548720, 685900, 857375, 902835, 1071718, 1354652 },
        /* 96 */new int[] { 560922, 707788, 884736, 932903, 1105920, 1415577 },
        /* 97 */new int[] { 571333, 730138, 912673, 963632, 1140841, 1460276 },
        /* 98 */new int[] { 583539, 752953, 941192, 995030, 1176490, 1524731 },
        /* 99 */new int[] { 591882, 776239, 970299, 1027103, 1212873, 1571884 },
        /* 100 */new int[] { 600000, 800000, 1000000, 1059860, 1250000, 1640000 },
    };

        public static int GetSize()
        {
            return expTable.Length;
        }
        public static int GetNumGrowthTypes()
        {
            int i = Random.Range(0, 99);
            Debug.Log(i);
            return expTable[i].Length;
        }

        public static int GetXPFromLevel(int level, LevelingRate growth)
        {
            if (level > 100)
                return -1;

            return expTable[level - 1][(int)growth];
        }
    }

    [System.Serializable]
    public class EvolutionCriteria
    {
        [SerializeField] PokemonBase evolution;
        [SerializeField] int requiredLevel;

        public PokemonBase Evolution => evolution;
        public int RequiredLevel => requiredLevel;
    }

    public static class PokeNature
    {
        static readonly float[][] natureMods =
        {
        /* Hardy    */    new float[] {1f,  1f, 1f, 1f, 1f },
        /* Lonely   */    new float[] {1.1f, .9f, 1f, 1f, 1f },
        /* Adamant  */    new float[] {1.1f, 1f, .9f, 1f, 1f },
        /* Naughty  */    new float[] {1.1f, 1f, 1f, .9f, 1f },
        /* Brave    */    new float[] {1.1f, 1f, 1f, 1f, .9f },
        /* Bold     */    new float[] {.9f, 1.1f, 1f, 1f, 1f },
        /* Docile   */    new float[] {1f, 1f, 1f, 1f, 1f },
        /* Impish   */    new float[] {1f, 1.1f, .9f, 1f, 1f },
        /* Lax      */    new float[] {1f, 1.1f, 1f, .9f, 1f },
        /* Relaxed  */    new float[] {1f, 1.1f, 1f, 1f, .9f },
        /* Modest   */    new float[] {.9f, 1f, 1.1f, 1f, 1f },
        /* Mild     */    new float[] {1f, .9f, 1.1f, 1f, 1f },
        /* Bashful  */    new float[] {1f, 1f, 1f, 1f, 1f },
        /* Rash     */    new float[] {1f, 1f, 1.1f, .9f, 1f },
        /* Quiet    */    new float[] {1f, 1f, 1.1f, 1f, .9f },
        /* Calm     */    new float[] {.9f, 1f, 1f, 1.1f, 1f },
        /* Gentle   */    new float[] {1f, .9f, 1f, 1.1f, 1f },
        /* Careful  */    new float[] {1f, 1f, .9f, 1.1f, 1f },
        /* Quirky   */    new float[] {1f, 1f, 1f, 1f, 1f },
        /* Sassy    */    new float[] {1f, 1f, 1f, 1.1f, .9f },
        /* Timid    */    new float[] {.9f, 1f, 1f, 1f, 1.1f },
        /* Hasty    */    new float[] {1f, .9f, 1f, 1f, 1.1f },
        /* Jolly    */    new float[] {1f, 1f, .9f, 1f, 1.1f },
        /* Naive    */    new float[] {1f, 1f, 1f, .9f, 1.1f },
        /* Serious  */    new float[] {1f, 1f, 1f, 1f, 1f },

    };

        public static float[] GetNatureModfier(NatureID nature)
        {
            return natureMods[(int)nature];
        }

    }

    public enum LevelingRate
    {
        erratic, fast, mid_fast, mid_slow, slow, fluctuant,
    }


    public static class GenderGenerator
    {
        public static Gender GetPokemondGender(PokemonBase species)
        {
            if (species.GenderThreshold == Species_Gender_Threshold.NoGender)
                return Gender.Genderless;

            if ((byte)Random.Range(1, 252) >= (byte)species.GenderThreshold)
                return Gender.Male;

            return Gender.Female;
        }
    }

    public enum Species_Gender_Threshold : byte
    {
        F1_M1 = 127,
        M = 0,
        M7_F1 = 31,
        M3_F1 = 63,
        F3_M1 = 191,
        F7_M1 = 225,
        F = 254,
        NoGender
    }

    public enum Gender
    {
        Male, Female, Genderless
    }
}

