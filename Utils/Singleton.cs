using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

    private static bool ShuttingDown = false;
    private static object Lock = new object();

    private static T Instance;
    // Start is called before the first frame update

    public static T I
    {
        get
        {
            if (ShuttingDown)
            {
                Debug.LogWarning("[Singleton] instance '" + typeof(T) + "' already destroyed. Returning null.");
                return null;
            }
            lock (Lock)
            {
                if (Instance == null)
                {
                    Instance = (T)FindObjectOfType(typeof(T));

                    if (Instance == null)
                    {
                        var singletonObject = new GameObject();
                        Instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton";

                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return Instance;
            }
        }
    }

    private void OnApplicationQuit()
    {
        ShuttingDown = true;
    }

    private void OnDestroy()
    {
        ShuttingDown = true;
    }

}
