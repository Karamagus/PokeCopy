using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{

    public class MapDataHandler : MonoBehaviour
    {

        [SerializeField] MapData data;
        [SerializeField] List<Pokemon> WildPokemons;


        public Pokemon RandomEncounterWildPokemon()
        {
            var wildPoke = data.WildPokemons[Random.Range(0, data.WildPokemons.Count)];
            wildPoke.Init();
            wildPoke.RecoverFromStatus();

            var wildCopy = new Pokemon(wildPoke.Base, wildPoke.Level);
            return wildCopy;
        }

        public void LoadWildPokemon()
        {
            // GameManager.Instance.BattleSceneData.LoadWilPkmn(RandomEncounterWildPokemon());
            GameManager.Instance.BattleData.SetWildPkmn(RandomEncounterWildPokemon());

        }


    }
}
