using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Text;
using System.Linq;

namespace pokeCopy
{

    public class DialogueCacheManager : MonoBehaviour //Refactor needed
    {
        static DialogueCacheManager i;
        [SerializeField] GameObject TextBox;
        [SerializeField] Text dialogueText;

        public System.Action OnShowDialogue;
        public System.Action OnCloseDialogue;


        bool isShowing;
        public static DialogueCacheManager I { get { return i; } }

        public bool IsShowing { get => isShowing; }

        //singleton pattern
        public void Awake()
        {
            if (i != null && i != this)
                Destroy(this);
            else
                i = this;


            //DontDestroyOnLoad(this);
        }

        // Start is called before the first frame update


        public IEnumerator ShowText_CR(string text, bool waitInput = true, bool autoclose = true)
        {

            if (GameManager.Instance.State == GameState.freeRoam)
                OnShowDialogue?.Invoke();

            GameManager.Instance.Inputs.UI.Navigate.Disable();
            isShowing = true;
            TextBox.SetActive(true);
            yield return TypeSentence_CR(text);

            if (waitInput)
            {
                yield return new WaitUntil(() => GameManager.Instance.Inputs.UI.Submit.triggered || GameManager.Instance.Inputs.PlayerDigital.Interact.triggered);
            }

            if (!autoclose)
                yield break;


            dialogueText.text = "";
            isShowing = false;
            TextBox.SetActive(false);
            GameManager.Instance.Inputs.UI.Navigate.Enable();

            if (GameManager.Instance.State == GameState.Dialogue)
                OnCloseDialogue?.Invoke();

        }

        //Starts dialogue. It also continues dialogue when interacting. 

        public IEnumerator ShowDialogue_CR(Dialogue dialogue)
        {
            if (dialogue == null || !(dialogue.senteces.Count() > 0))
                yield break;
            yield return new WaitForEndOfFrame();
            OnShowDialogue?.Invoke();
            isShowing = true;
            TextBox.SetActive(true);

            foreach (var line in dialogue.senteces)
            {
                //yield return TypeSentence_CR(line);
                yield return TypeSentence_CR(line);
                yield return new WaitUntil(() => GameManager.Instance.Inputs.UI.Submit.triggered || GameManager.Instance.Inputs.PlayerDigital.Interact.triggered);

            }
            TextBox.SetActive(false);
            isShowing = false;


            OnCloseDialogue?.Invoke();
        }

        IEnumerator TypeSentence_CR(string sentence)
        {
            dialogueText.text = "";
            foreach (char c in sentence.ToCharArray())
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.015f);

            }

        }

        IEnumerator TypeSentenceReplacing_CR(string sentence)
        {
            dialogueText.text = new StringBuilder(sentence.Length).Insert(0, " ", sentence.Length).ToString();
            int i = 0;
            foreach (char c in sentence.ToString())
            {

                i++;
                yield return new WaitForSeconds(0.015f);
            }

        }

        /*
        public void StartDialogue(Dialogue dialogue, System.Action onFinished = null)
        {

            if (!TextBox.activeSelf)//for contingece, in case this function starts by other means
            {
                OnShowDialogue?.Invoke();//using this to avoid reference GameManager
                dialogueText.text = "";
                sentences.Clear();
                foreach (var s in dialogue.senteces)
                {
                    sentences.Enqueue(s);
                }
                OnFinished += onFinished;
                TextBox.SetActive(true);
                isFirstLine = true;
                NextSentence();
            }
            //NextSentence();
            //Old Dialogue when using only interac() to initiate next line. the isInteracting on NPCMono was determened by the end of this function
            //Can be clean up so the iteraction state of the npc does not depend of this function anymore.
            // return TextBox.activeSelf;
        }

        public void NextSentence()
        {

            if (sentences.Count == 0)
            {
                StartCoroutine(EndOfDialogue_CR());
                return;
            }
            string crntSente = sentences.Dequeue();


            StopAllCoroutines();//interupts any sentece being typed, avoiding mixed typing of different lines. Alternative to waiting until finishing one line
            //For the case of wait til finished typing, isFirstLine can be replaced for is typing, avoiding any update on input and preventing going to the next line before the previous is completed
            StartCoroutine(TypeSentence_CR(crntSente));
        }
        private IEnumerator EndOfDialogue_CR()
        {
            yield return null;
            TextBox.SetActive(false);
            dialogueText.text = "";
            OnFinished?.Invoke();
            OnFinished = null;

            OnCloseDialogue?.Invoke();
        }
        */



    }
}
