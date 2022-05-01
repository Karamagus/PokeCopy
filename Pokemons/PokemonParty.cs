using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using pokeCopy;

public class PokemonParty : MonoBehaviour
{

    [SerializeField] List<Pokemon> party = new List<Pokemon>();
    string trainerName;

    public static int MAX_PARTY_SIZE = 6;

    public List<Pokemon> Party { get => party; set => party = value; }
    public string TrainerName { get => trainerName; set => trainerName = value; }

    public void Start()
    {
        foreach (var p in Party) 
            p.Init();

        trainerName = GetComponent<INamable>().Name;
    }

    public void AddToParty(Pokemon p)
    {
        if (Party.Count >= 6)
        {
            //TO DO: Add to PC after implementing it
        }
            

        Party.Add(p);

    }
    public void RemoveFromParty(int index)
    {
        if (Party.Count <= 1)
            return;

        Party.RemoveAt(index);
    }

    public void SwapPosition(int pokemon1, int pokemon2)
    {
        var mB = Mathf.Max(pokemon1, pokemon2);
        var mS = Mathf.Min(pokemon1, pokemon2);

        var p1 = party[mS];
        party.Insert(mB,p1);
        var p2 = party[mB + 1];
        party.Insert(mS, p2);
        party.RemoveAt(mS + 1);
        party.RemoveAt(mB+ 1);


    }

    public Pokemon Retrive(int index)
    {
        return (index >= Party.Count) ? null : Party[index];
    }


    public Pokemon GetHealthy()
    {
        return Party.Where(x => x.HP > 0).FirstOrDefault();
    }


    public IEnumerator CheckForEvolutions_CR()
    {

        foreach (var pokemon in party)
        {
            var evolution = pokemon.CanEvolveInto();
            if (evolution != null)
            {
                yield return EvolutionSceneUI.I.StartEvolution_CR(pokemon, evolution);
            }

        }
    }
    public int GetSize() => Party.Count;


    public List<Pokemon> GetFullParty()
    {
        return Party;
    }
}


