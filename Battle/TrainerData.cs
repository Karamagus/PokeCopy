using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu (menuName = "Character/Create New TrainerData")]
public class TrainerData : ScriptableObject
{
    [SerializeField] string Name;
    [SerializeField] PokemonParty party;//
    [SerializeField] Sprite battleSrpite;
    [SerializeField] List<Pokemon> pokemons = new List<Pokemon>();
    public PokemonTeam team { get; private set; }

    public string _Name { get => Name; }
    public PokemonParty Party { get => party;}
    public Sprite BattleSrpite { get => battleSrpite;  }
    public List<Pokemon> Pokemons { get => pokemons; }

    [SerializeField] bool battleLost = false;

    public void Awake()
    {
       // pokemons = party.GetFullParty();
       /*
        
        foreach (var p in pokemons)
        {
            p.Init();
        }
        */
    }

    public void SetData(string name, PokemonParty party, Sprite sprite, PokemonTeam team)
    {
        Name = name;
        this.party = party;
        battleSrpite = sprite;
        pokemons = party.GetFullParty();
        this.team = team;
        /*
        foreach (var p in this.team.)
        {
            p.Init();
        }
        */
    }

    public Pokemon GetHealthy()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddToParty(Pokemon p)
    {
        if (pokemons.Count >= 6)
            return;

        pokemons.Add(p);

    }
    public void RemoveFromParty(int index)
    {
        if (pokemons.Count <= 1)
            return;

        pokemons.RemoveAt(index);
    }

    public Pokemon Retrive(int index)
    {
        return (index >= pokemons.Count) ? null : pokemons[index];
    }

    public void OnEndOfBattle(bool result)
    {
        battleLost = result;
    }

    public bool GetBattleResult()
    {
        return battleLost;
    }
}
