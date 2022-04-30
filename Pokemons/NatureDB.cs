using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class NatureDB
    {

        static Dictionary<NatureID, Nature> Natures;

        public static void Init()
        {
            Natures = new Dictionary<NatureID, Nature>();

            for(int i = 0; i < (int)NatureID.Serious +1; i++)
            {
                var n = (NatureID)i;
                Natures.Add(n, new Nature(n));
            }

        }

        public static Nature GetNatureByID(NatureID id)
        {
            return Natures[id];
        }



    }
}


public enum NatureID : byte
{
 /*row = Incre, col = Decr*/

             /*-Atk      -Def        -S.Atk      -S.Def     -Speed */
 /* +Atk*/   Hardy,      Lonely,     Adamant,    Naughty,   Brave,
 /* +Def*/   Bold,       Docile,     Impish,     Lax,       Relaxed,
 /* +SAt*/   Modest,     Mild,       Bashful,    Rash,      Quiet,
 /* +SDf*/   Calm,       Gentle,     Careful,    Quirky,    Sassy,
 /* +Spd*/   Timid,      Hasty,      Jolly,      Naive,     Serious,
    
}