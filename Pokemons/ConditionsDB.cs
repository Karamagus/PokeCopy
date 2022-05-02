using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace pokeCopy
{
    public class ConditionsDB
    {

        public static void Init()
        {
            foreach (var kvp in Conditions)
            {
                var conditionId = kvp.Key;
                var condition = kvp.Value;

                condition.Id = conditionId;
            }

            foreach (var kvp in VolatConditions)
            {
                var condId = kvp.Key;
                var condition = kvp.Value;

                condition.Id = condId;
            }
            foreach (var kvp in HitEffects)
            {
                var condId = kvp.Key;
                var hitEffect = kvp.Value;

                hitEffect.Id = condId;
            }

        }

        //refactor
        public static Dictionary<ConditionID, Condition> Conditions { get; set; } =
            new Dictionary<ConditionID, Condition>()
            {
            {
                ConditionID.psn,
                new Condition()
                {
                    Name = "Poison",

                    StartMessage = "has been poisoned.",
                    OnAfterMove = (Pokemon pokemon) =>
                    {
                        pokemon.TakeDamage(pokemon.MaxHP/8);
                        pokemon.StatusChangeQuote.Enqueue($"{pokemon.NickName} is hurt by poison");
                    }

                }

            },
            {
                ConditionID.brn,
                new Condition()
                {
                    Name = "Burn",
                    StartMessage = "got burned.",
                    TypeResited = PokemonType.Fire,
                    ResistenceMassage = "can't be burned.",
                    OnAfterMove = (Pokemon poke) =>
                    {
                        poke.TakeDamage(poke.MaxHP/16);
                        poke.StatusChangeQuote.Enqueue($"{poke.NickName} is hurt by its burn.");

                    },
                    GetOnDamageMod = (MoveCategory category) =>
                    {

                       return (category == MoveCategory.Physical)? .5f: 1f;
                    }

                }
            },
             {
                ConditionID.par,
                new Condition()
                {
                    Name = "Parilyzed",
                    StartMessage = "has been paralyzed.",
                    TypeResited = PokemonType.Electric,
                    ResistenceMassage = " resisted the paralysis.",
                    OnBeforeMove = (Pokemon poke) =>
                    {

                        if (UnityEngine.Random.Range(1,5) == 1)
                        {
                            poke.StatusChangeQuote.Enqueue($"{poke.NickName} is paralized and can't move.");
                            return false;
                        }


                        return true;
                    }
                }
            },
            {
                ConditionID.frz,
                new Condition()
                {
                    Name = "Freeze",
                    StartMessage = "has been frozen.",
                    OnBeforeMove = (Pokemon poke) =>
                    {

                        if (UnityEngine.Random.Range(1,6) == 1)
                        {
                            poke.RecoverFromStatus();
                            poke.StatusChangeQuote.Enqueue($"{poke.NickName} is not frozen anymore.");
                            return true;
                        }


                        return false;
                    }
                }
            },
            {
                ConditionID.slp,
                new Condition()
                {
                    Name = "Asleep",
                    StartMessage = "has fallen asleep.",
                    OnStart = (Pokemon poke) =>
                    {
                        poke.StatusTimer = UnityEngine.Random.Range(1,6);
                        Debug.Log($"Will be asleep for {poke.StatusTimer} moves");


                    },
                    OnBeforeMove = (Pokemon poke) =>
                    {
                        if (poke.StatusTimer <= 0)
                        {
                            poke.RecoverFromStatus();
                            poke.StatusChangeQuote.Enqueue($"{poke.NickName} woke up.");
                            return true;
                        }



                        poke.StatusTimer--;
                        poke.StatusChangeQuote.Enqueue($"{poke.NickName} is sleeping.");
                        return false;
                    }
                }

            },
            {
                ConditionID.fnt,
                new Condition
                {
                    Name = "Fainted",
                    StartMessage = "has fainted.",
                    OnStart = (Pokemon p) =>
                    {
                        p.RemoveAllVolat();
                    }
                }




            }



            };

        public static Dictionary<ConditionID, VolCondition> VolatConditions { get; set; } =
        new Dictionary<ConditionID, VolCondition>()
        {
            {
                ConditionID.confusion,
                new VolCondition()
                {
                    Name = "Confusion",
                    StartMessage = "has been confused.",
                    Timer = UnityEngine.Random.Range(1,6),
                    OnStart = (Pokemon poke) =>
                    {
                       // poke.VolatTimers.Add(UnityEngine.Random.Range(1,6));
                       // poke.StatusTimer = UnityEngine.Random.Range(1,6);
                        Debug.Log($"Will be confused for {poke.VolatTimers[poke.VolatTimers.Count-1]} moves");


                    },

                    OnBeforeMove = (Pokemon poke) =>
                    {
                        if (poke.VolatTimers[poke.Volat.IndexOf(VolatConditions[ConditionID.confusion])] <= 0)
                        {
                            poke.RecoverFromVolat(ConditionID.confusion);
                            poke.StatusChangeQuote.Enqueue($"{poke.NickName} kicked out confusion!");
                            return true;
                        }
                        poke.VolatTimers[poke.Volat.IndexOf(VolatConditions[ConditionID.confusion])]--;

                        if (UnityEngine.Random.Range(1,3) == 1)
                            return true;

                        poke.StatusChangeQuote.Enqueue($"{poke.NickName} is confused.");
                        poke.TakeConfusionDmg();
                        poke.StatusChangeQuote.Enqueue("It hurts itself due to confusion");

                        return false;
                    }
                }
            },
            {
                ConditionID.flinch,
                new VolCondition()
                {
                    Name = "Flinch",
                    StartMessage = "",
                    Timer = 0,
                    OnStart = (Pokemon poke) =>
                    {
                       // poke.VolatTimers.Add(0);
                       // poke.StatusTimer = UnityEngine.Random.Range(1,6);


                    },

                    OnBeforeMove = (Pokemon poke) =>
                    {

                        poke.StatusChangeQuote.Enqueue($"{poke.NickName} flinched and couldn't move!");
                        if (poke.VolatTimers[poke.Volat.IndexOf(VolatConditions[ConditionID.confusion])] <= 0)
                            poke.RecoverFromVolat(ConditionID.flinch);


                        return false;


                    },
                    OnAfterMove = (Pokemon poke) =>
                    {
                         poke.RecoverFromVolat(ConditionID.flinch);

                    }
                }
            },




            //Some refactoring and redesing needed so the effect heppans here and not on the Unit class or battlesystem class
            {
                ConditionID.leech_seed,//tests needed
                new VolCondition()
                {
                    Name = "Leech Seed",
                    StartMessage = "was seeded.",
                    FailledMassage = "But it failed.",
                    TypeResited = PokemonType.Grass,
                    ResistenceMassage = "is not affected by.",
                    OnPlantSeed = (Unit root, Unit leecher) =>
                    {
                        root.SetLeecher(leecher);
                    },
                    OnAfterAllTurns = (Pokemon poke) =>
                    {
                        var dmg = Mathf.Max( poke.MaxHP /8, 1);
                        poke.TakeDamage(dmg);
                        poke.StatusChangeQuote.Enqueue($"{poke.NickName}'s health is sapped by Leech Seed.");
                        return dmg;//leecher.CrrPokemon.ReciveHeal(dmg);

                    }


                }
            },
            {
                ConditionID.speedX2,
                new VolCondition()
                {
                    Name = "Double Speed",
                    StartMessage = "",
                    Timer = 4,
                    OnStart = (Pokemon poke) =>
                    {
                        poke.StatusChangeQuote.Enqueue($"The tailwind blew from behind your team.");
                        poke.StatMods[Stat.Speed] *= 2f;
                        
                       // poke.StatusTimer = UnityEngine.Random.Range(1,6);


                    },
                    OnAfterMove = (Pokemon poke) =>
                    {
                        if (poke.VolatTimers[poke.Volat.IndexOf(VolatConditions[ConditionID.speedX2])] <= 0)
                        {
                            poke.StatMods[Stat.Speed] *= 0.5f;
                            poke.RecoverFromVolat(ConditionID.speedX2);
                            return;
                        }
                        poke.VolatTimers[poke.Volat.IndexOf(VolatConditions[ConditionID.speedX2])]--;
                    }
                }
            },
            {
                ConditionID.leechLife,//tests needed
                new VolCondition()
                {
                    Name = "Leech Life",
                    StartMessage = "",
                    FailledMassage = "But it failed.",
                    OnHit = (Pokemon poke, int heal) =>
                    {
                        poke.TakeHeal(heal);
                    },
                    OnAfterMove = (Pokemon poke) =>
                    {

                        poke.RecoverFromVolat(ConditionID.leechLife);

                    }

                }
            },




        };

        public static Dictionary<HitEffectsID, HitEffects> HitEffects { get; set; } =
            new Dictionary<HitEffectsID, HitEffects>()
            {
            {
                  HitEffectsID.leech_life,
                  new HitEffects()
                  {
                      Message = "had its energy drained.",
                      OnHit = (Pokemon poke, int dmg) =>
                      {
                          dmg = Mathf.FloorToInt(dmg * .5f);
                          poke.TakeHeal(Mathf.Max(dmg, 1));
                      }
                  }

            },
            {
                HitEffectsID.recoilQuarter,
                new HitEffects()
                {
                    Message = "is hit with recoil",
                    OnHit = (Pokemon poke, int dmg) =>
                    {
                        poke.TakeDamage(Mathf.Max( Mathf.FloorToInt( dmg *.25f),1));
                       // poke.StatusChange.Enqueue($"{poke._name} is hit with recoil.");
                    }
                }
            }






            };



        public static float GetCatchRateMod(Condition condition)
        {
            if (condition == null)
                return 1f;

            if (condition.Id == ConditionID.frz || condition.Id == ConditionID.slp)
                return 2f;
            else if (condition.Id == ConditionID.brn || condition.Id == ConditionID.psn || condition.Id == ConditionID.par)
                return 1.5f;

            return 1f;
        }


    }

    [Serializable]
    public enum ConditionID
    {
        none, psn, brn, par, frz, slp, fnt, confusion, flinch, leech_seed, speedX2, foresight, leechLife,
    }


    public enum HitEffectsID
    {
        none, leech_life, recoilQuarter, recoilThird, recoilHalf, recoilSelf, drainHalfDmg,
    }

}
