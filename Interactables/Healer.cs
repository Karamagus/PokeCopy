using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class Healer : MonoBehaviour
    {
        [SerializeField] Dialogue confirmedDialogue;
        [SerializeField] Dialogue denialDialogue;

        public IEnumerator Heal(Transform player, Dialogue dialogue)
        {
            yield return DialogueCacheManager.I.ShowDialogue_CR(dialogue, new List<string>() { "YES", "NO" });

            if (DialogueCacheManager.I.Selection == 0)
            {
                yield return Fader.I.FadeIn_CR(0.5f);
                var party = player.GetComponent<PokemonParty>();
                party.Party.ForEach(p => p.FullRecovery());
                //maybe Update

                yield return Fader.I.FadeOut_CR(.5f);

                yield return DialogueCacheManager.I.ShowDialogue_CR(confirmedDialogue);

            }
            else if (DialogueCacheManager.I.Selection == 1)
            {
                yield return DialogueCacheManager.I.ShowDialogue_CR(denialDialogue);

            }

        }


    }
}
