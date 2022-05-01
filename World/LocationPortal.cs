using System.Collections;
using System.Linq;
using UnityEngine;

namespace pokeCopy
{
    public class LocationPortal : MonoBehaviour, ITriggerable, IPortal
    {
        [SerializeField] SceneDetails sceneData;
        
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
            sceneData = GameObject.Find(gameObject.scene.name).GetComponent<SceneDetails>();
            sceneData.SetConection(false);
            
        }

        public void OnContact(PlayerMovement player)
        {
            this.player = player;
            sceneData.SetConection(true);
            if (sceneData.ExtScene != null)
            {
                sceneData.ExtScene.SetConection(false);
            }
            StartCoroutine(SwitchScene_CR());

        }


        public IEnumerator OnLook_CR(PlayerMovement player)
        {
            if (door != null)
            {

                door.Play("WoodDoor_Opening");
                yield return new WaitForSeconds(door.GetCurrentAnimatorStateInfo(0).length / 256);
                //yield return null;
            }

        }

        IEnumerator SwitchScene_CR()
        {


            GameManager.Instance.PauseGame(true);
            yield return fader.FadeIn_CR(.5f);
            
            
              
            
            var destination = FindObjectsOfType<LocationPortal>().First(x => x != this && Link == x.Link);
            player.SnapPositionToGrid(destination.Spawn.position);

            yield return fader.FadeOut_CR(.5f);
            if (door)
                door.Play("WoodDoor_Closed");

            sceneData.SetConection(false);

            GameManager.Instance.PauseGame(false);
        }


    }

}
