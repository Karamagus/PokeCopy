using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace pokeCopy
{
    public class EvolutionSceneManager : MonoBehaviour
    {
        [SerializeField] GameObject evolutionUI;
        [SerializeField] Image pokemonSprite;

        public static EvolutionSceneManager I { get; private set; }

        private void Awake()
        {
            I = this;
        }

        bool doEvolution;
        public event System.Action OnStartEvolution;
        public event System.Action OnEndEvolution;

        public IEnumerator StartEvolution(Pokemon pokemon, EvolutionCriteria evolution)
        {
            OnStartEvolution?.Invoke();
            evolutionUI.SetActive(true);
            pokemonSprite.sprite = pokemon.Base.Front;
            yield return DialogueCacheManager.I.ShowText($"{pokemon.NickName} is evolving.",false, false);

            yield return CaptureCancelComand(3);

            if (doEvolution)
            {
                pokemon.Evolve(evolution);
                pokemonSprite.sprite = pokemon.Base.Front;
                yield return DialogueCacheManager.I.ShowText($"{pokemon.NickName} evolved to {pokemon.Species}.");

            }
            else
            {
                yield return DialogueCacheManager.I.ShowText($"Huh, {pokemon.NickName} stoped evolving!");

            }


            evolutionUI.SetActive(false);
            OnEndEvolution?.Invoke();
            //cancel corroutine

        }

        public IEnumerator CaptureCancelComand(float time)
        {
            
            while (time > 0)
            {
                doEvolution = !(GameManager.Instance.Inputs.UI.Cancel.ReadValue<float>() > 0);
                
                Debug.Log(time);
                time -= Time.deltaTime;
                yield return null;
            }
            yield return null;
        }




    }
}
