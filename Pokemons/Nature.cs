using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class Nature
    {
        [SerializeField] string nature;
        [SerializeField] NatureID id;
        [SerializeField] Stat incresedStat;
        [SerializeField] Stat decreasedStat;



        public Nature(NatureID name)
        {
            id = name;
            nature = id.ToString();

            switch (name)
            {
                case NatureID.Hardy:
                    SetStats(Stat.Attack, Stat.Attack);
                    break;
                case NatureID.Lonely:
                    SetStats(Stat.Attack, Stat.Defense);
                    break;
                case NatureID.Adamant:
                    SetStats(Stat.Attack, Stat.SpecialAtk);
                    break;
                case NatureID.Naughty:
                    SetStats(Stat.Attack, Stat.SpecialDef);
                    break;
                case NatureID.Brave:
                    SetStats(Stat.Attack, Stat.Speed);
                    break;
                case NatureID.Bold:
                    SetStats(Stat.Defense, Stat.Attack);
                    break;
                case NatureID.Docile:
                    SetStats(Stat.Defense, Stat.Defense);
                    break;
                case NatureID.Impish:
                    SetStats(Stat.Defense, Stat.SpecialAtk);
                    break;
                case NatureID.Lax:
                    SetStats(Stat.Defense, Stat.SpecialDef);
                    break;
                case NatureID.Relaxed:
                    SetStats(Stat.Defense, Stat.Speed);
                    break;
                case NatureID.Modest:
                    SetStats(Stat.SpecialAtk, Stat.Attack);
                    break;
                case NatureID.Mild:
                    SetStats(Stat.SpecialAtk, Stat.Defense);
                    break;
                case NatureID.Bashful:
                    SetStats(Stat.SpecialAtk, Stat.SpecialAtk);
                    break;
                case NatureID.Rash:
                    SetStats(Stat.SpecialAtk, Stat.SpecialDef);
                    break;
                case NatureID.Quiet:
                    SetStats(Stat.SpecialAtk, Stat.Speed);
                    break;
                case NatureID.Calm:
                    SetStats(Stat.SpecialDef, Stat.Attack);
                    break;
                case NatureID.Gentle:
                    SetStats(Stat.SpecialDef, Stat.Defense);
                    break;
                case NatureID.Careful:
                    SetStats(Stat.SpecialDef, Stat.SpecialAtk);
                    break;
                case NatureID.Quirky:
                    SetStats(Stat.SpecialDef, Stat.SpecialDef);
                    break;
                case NatureID.Sassy:
                    SetStats(Stat.SpecialAtk, Stat.Speed);
                    break;
                case NatureID.Timid:
                    SetStats(Stat.Speed, Stat.Attack);
                    break;
                case NatureID.Hasty:
                    SetStats(Stat.Speed, Stat.Defense);
                    break;
                case NatureID.Jolly:
                    SetStats(Stat.Speed, Stat.SpecialAtk);
                    break;
                case NatureID.Naive:
                    SetStats(Stat.Speed, Stat.SpecialDef);
                    break;
                case NatureID.Serious:
                    SetStats(Stat.Speed, Stat.Speed);
                    break;
                default:
                    break;
            }
        }



        public Nature(NatureID name, Stat increased = Stat.Attack, Stat decreased = Stat.Attack)
        {
            id = name;
            nature = id.ToString();
            incresedStat = increased;
            decreasedStat = decreased;

        }

        void SetStats(Stat increase, Stat decrease)
        {
            incresedStat = increase;
            decreasedStat = decrease;
        }

        public float GetNatureMod(Stat stat)
        {
            var mod = 1f;
            if (stat == incresedStat)
                mod += .1f;
            if (stat == decreasedStat)
                mod -= .1f;

            return mod;
        }

    }
}
