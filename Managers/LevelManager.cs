using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace pokeCopy
{

    public static class LevelManager
    {
        /*
        #region Singleton
        public static LevelManager instance;

        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        #endregion
        */
        public static int currentLevel;
        public static float[] position = new float[3];

        public static SceneData priviousScene;
        static Scene CurrentWorldLocation;

    }
}
