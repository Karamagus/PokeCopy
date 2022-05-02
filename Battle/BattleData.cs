using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{

    [CreateAssetMenu(menuName = "BattleData")]
    public class BattleData : ScriptableObject
    {
        PlayerMovement player;
        TrainerController trainer;
        PokemonParty playerParty;
        PokemonParty trainerParty;
        Pokemon wildPokemon;
        [SerializeField] AnimatorOverrideController wildOverride;
        [SerializeField] AnimatorOverrideController trainerOverride;

        public bool Won { get; set; }

        public bool IsTrainerBattle { get; set; }

        public void ClearData()
        {
            player = null;
            trainer = null;
            trainerParty = null;
            playerParty = null;
            wildPokemon = null;
            IsTrainerBattle = false;
            Won = false;
        }

        public void SetTrainer(TrainerController trainer)
        {
            this.trainer = trainer;
            trainerParty = trainer.PokeParty;
            //use get component?
        }

        public void SetPlayer(PlayerMovement player)
        {
            this.player = player;
            playerParty = player.GetComponent<PokemonParty>();
        }

        public PokemonParty GetPlayerParty()
        {
            return playerParty;
        }

        public PokemonParty GetTrainerParty()
        {
            return trainerParty;
        }

        public PlayerMovement GetPlayer()
        {
            return player;
        }

        public TrainerController GetTrainer()
        {
            return trainer;
        }

        public void SetWildPkmn(Pokemon wild)
        {
            wildPokemon = wild;

        }

        public Pokemon GetWildPkmn()
        {


            return wildPokemon;
        }



    }
}
