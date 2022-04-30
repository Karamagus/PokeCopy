using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace pokeCopy
{
    public class Portal : MonoBehaviour, ITriggerable, IPortal
    {

        [SerializeField] int sceneIdx = -1;
        [SerializeField] PortalLink link;
        [SerializeField] Transform spawn;
        [SerializeField] Animator door;
        PlayerMovement player;

        Fader fader;

        public Transform Spawn => spawn;

        public PortalLink Link => link;


        public void Start()
        {
            fader = FindObjectOfType<Fader>();
        }

        public void OnContact(PlayerMovement player)
        {
            this.player = player;
            StartCoroutine(SwitchScene());

        }


        public IEnumerator OnLook(PlayerMovement player)
        {
            if (door != null)
            {
                
                door.Play("WoodDoor_Opening");
                yield return new WaitForSeconds(door.GetCurrentAnimatorStateInfo(0).length/256);
                //yield return null;
            }

        }

        IEnumerator SwitchScene()
        {
            DontDestroyOnLoad(gameObject);


            GameManager.Instance.PauseGame(true);
            yield return fader.FadeIn(.5f);

            yield return SceneManager.LoadSceneAsync(sceneIdx);

            var destination = FindObjectsOfType<Portal>().First(x => x != this && Link == x.Link);
            player.SnapPositionToGrid(destination.Spawn.position);

            yield return fader.FadeOut(.5f);
            GameManager.Instance.PauseGame(false);
            Destroy(gameObject);
        }


    }


    public enum PortalLink
    {
        A, B, C, D, E,
    }


}
