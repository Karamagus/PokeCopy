using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace pokeCopy
{
    public class Fader : MonoBehaviour
    {
        Image image;

        public void Awake()
        {
            image = GetComponent<Image>();
        }



        public IEnumerator FadeIn(float timeInSeconds)
        {
            var colorNew = image.color;
            while(image.color.a < 1)
            {
                colorNew.a += (Time.deltaTime/timeInSeconds);
                image.color = colorNew;

                yield return null;
            }
        }


        public IEnumerator FadeOut(float timeInSeconds)
        {
            var colorNew = image.color;
            while (image.color.a > 0)
            {
                colorNew.a -= (Time.deltaTime / timeInSeconds);
                image.color = colorNew;

                yield return null;
            }

        }
    }
}
