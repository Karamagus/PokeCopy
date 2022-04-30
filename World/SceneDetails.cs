using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace pokeCopy
{
    public class SceneDetails : MonoBehaviour
    {

        [SerializeField] List<SceneDetails> connectedScenes;
        [SerializeField] SceneDetails extScene;
        bool IsLoaded;
        bool IsConected;

        public SceneDetails ExtScene => extScene;

        List<SavableEntity> Savables;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                GameManager.Instance.SetCurrentScene(this);
                LoadScene();
                if (connectedScenes.Count > 0)
                {
                    foreach (var scene in connectedScenes)
                        scene.LoadScene();

                }
                
                if (GameManager.Instance.PrvScene != null)
                {
                    var prevLoadedScene = GameManager.Instance.PrvScene.connectedScenes;
                    foreach (var scene in prevLoadedScene)
                    {
                        if (!connectedScenes.Contains(scene) && scene != this)
                            scene.UnloadScene();
                    }
                }
                

                if (extScene)
                    extScene.LoadScene();

            }
        }
        /*
        public void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && !IsConected)
            {
                StartCoroutine(AsyncExitTriger());
            }
        }
        */
        IEnumerator AsyncExitTriger()
        {
            yield return new WaitForEndOfFrame();
            if (GameManager.Instance.CrrtScene.connectedScenes.Count > 0)
            {

                foreach (var scene in connectedScenes)
                {
                    if (!GameManager.Instance.CrrtScene.connectedScenes.Contains(scene) && scene != this)
                        scene.UnloadScene();

                }

                if (!GameManager.Instance.CrrtScene.connectedScenes.Contains(this))
                    UnloadScene();
            }

        }

        public void LoadScene()
        {
            if (!IsLoaded)
            {
                var op = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
                IsLoaded = true;


                op.completed += (AsyncOperation operation) =>
                {
                    Savables = GetSavableEntities();
                    SavingSystem.i.RestoreEntityStates(Savables);
                };


            }
        }

        public void UnloadScene()
        {
            if (IsLoaded)
            {
                SavingSystem.i.CaptureEntityStates(Savables);

                SceneManager.UnloadSceneAsync(name);
                IsLoaded = false;
            }
        }

        public void SetConection(bool hasConection) => IsConected = hasConection;

        public void SetAllConections(bool hasConcection = false)
        {
            foreach (var scene in connectedScenes)
                SetConection(hasConcection);
        }

        List<SavableEntity> GetSavableEntities()
        {
            var crrtScene = SceneManager.GetSceneByName(gameObject.name);
            var savables = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == crrtScene).ToList();
            return savables;
        }



    }
}
