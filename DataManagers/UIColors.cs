using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace pokeCopy
{
    [CreateAssetMenu(fileName = "UIColors", menuName = "")]
    public class UIColors : ScriptableObject//make Static?
    {
        [Header("Status colors")]
        public Color psnColor;
        public Color brnColor;
        public Color parColor;
        public Color frzColor;
        public Color slpColor;
        public Color fntColor;
        [Space]
        [Header("Type Icons")]
        public Sprite none;
        public Sprite normal;
        public Sprite fighting;
        public Sprite flying;
        public Sprite poison;
        public Sprite ground;
        public Sprite rock;
        public Sprite bug;
        public Sprite ghost;
        public Sprite steel;
        public Sprite fire;
        public Sprite water;
        public Sprite grass;
        public Sprite electric;
        public Sprite psychic;
        public Sprite ice;
        public Sprite dragon;
        public Sprite dark;
        public Sprite fairy;
        [Space]
        [Header("Health bar colors")]
        [SerializeField] Color healthyHPColor;
        [SerializeField] Color warningHPColor;
        [SerializeField] Color dangerHPColor;
        [Space]
        [Header("Moves Categories")]
        [SerializeField] Sprite phisical;
        [SerializeField] Sprite special;
        [SerializeField] Sprite status;
        [Header("Party Button Colors")]
        [Space]
        [SerializeField] ColorBlock cb;
        [Header("Party Faint Colors")]
        [SerializeField] ColorBlock fntCb;
        [Header("Party Switch Colors")]
        [SerializeField] ColorBlock swtchCb;


        public static Dictionary<ConditionID, Color> statusColors = new Dictionary<ConditionID, Color>();
        public static Dictionary<PokemonType, Sprite> typeIcons = new Dictionary<PokemonType, Sprite>();
        public static Dictionary<HpState, Color> hpStateColor = new Dictionary<HpState, Color>();
        public static Dictionary<MoveCategory, Sprite> moveCategoryIcon = new Dictionary<MoveCategory, Sprite>();
        public static ColorBlock partyMenuNormalColors;
        public static ColorBlock partyMenuFaintColors;
        public static ColorBlock partyMenuSwitchColors;

        public Color HealthyHPColor { get => healthyHPColor; }
        public Color WarningHPColor { get => warningHPColor; }
        public Color DangerHPColor { get => dangerHPColor; }

        public static UIColors i;


        public void Awake()
        {
            if (i == null)
                i = this;
        }

        public void Init()
        {
            statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.par, parColor },
            {ConditionID.slp, slpColor },
            {ConditionID.frz, frzColor },
            {ConditionID.fnt, fntColor }
        };

            typeIcons = new Dictionary<PokemonType, Sprite>()
        {
            {PokemonType.None, null },
            {PokemonType.Normal, normal},
            {PokemonType.Fighting, fighting },
            {PokemonType.Flying, flying },
            {PokemonType.Poison, poison },
            {PokemonType.Ground, ground },
            {PokemonType.Rock, rock },
            {PokemonType.Bug, bug },
            {PokemonType.Ghost, ghost },
            {PokemonType.Steel, steel },
            {PokemonType.Fire, fire},
            {PokemonType.Water, water },
            {PokemonType.Grass, grass},
            {PokemonType.Electric, electric},
            {PokemonType.Psychic, psychic },
            {PokemonType.Ice, ice },
            {PokemonType.Dragon, dragon },
            {PokemonType.Dark, dark},
            {PokemonType.Fairy, fairy },

        };

            hpStateColor = new Dictionary<HpState, Color>()
        {
            { HpState.healthy, HealthyHPColor },
            { HpState.caution, WarningHPColor },
            { HpState.danger, DangerHPColor }
        };

            moveCategoryIcon = new Dictionary<MoveCategory, Sprite>()
        {
            {MoveCategory.Physical, phisical },
            {MoveCategory.Special, special },
            {MoveCategory.Status, status },
        };

            partyMenuNormalColors = cb;
            partyMenuFaintColors = fntCb;
            partyMenuSwitchColors = swtchCb;

        }
    }


    public enum HpState
    {
        healthy, caution, danger
    }

}
