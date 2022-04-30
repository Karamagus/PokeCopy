using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PokemonTeam
{
    [SerializeField] List<Pokemon> party = new List<Pokemon>();


    public void Init()
    {
        foreach (var p in party)
        {
            p.Init();
        }
    }

    public void AddToParty(Pokemon p)
    {
        if (party.Count >= 6)
            return;

        party.Add(p);

    }
    public void RemoveFromParty(int index)
    {
        if (party.Count <= 1)
            return;

        party.RemoveAt(index);
    }

    public Pokemon Retrive(int index)
    {
        return (index >= party.Count) ? null : party[index];
    }


    public Pokemon GetHealthy()
    {
        return party.Where(x => x.HP > 0).FirstOrDefault();
    }

    public int GetSize()
    {
        return party.Count;
    }

    public List<Pokemon> GetFullParty()
    {
        return party;
    }


}
