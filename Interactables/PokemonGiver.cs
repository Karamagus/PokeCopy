using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class PokemonGiver : MonoBehaviour, ISavable
    {
        [SerializeField] Pokemon givenPokemon;
        [SerializeField] Dialogue dialogue;


        bool gotPokemon = false;



        public IEnumerator GivePokemon(PlayerMovement player)
        {
            yield return DialogueCacheManager.I.ShowDialogue(dialogue);

            givenPokemon.Init();
            player.GetComponent<PokemonParty>().AddToParty(givenPokemon);

            gotPokemon = true;

            var text = $"{player.Name} received {givenPokemon.Species}.";

            yield return DialogueCacheManager.I.ShowText(text);
        }




        public bool CanGive()
        {
            return givenPokemon != null && givenPokemon.Base != null && !gotPokemon ;
        }

        public object CaptureState()
        {
            return gotPokemon;
        }

        public void RestoreState(object state)
        {
            gotPokemon = (bool)state;
        }
    }

}

