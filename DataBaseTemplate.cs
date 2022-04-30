using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pokeCopy
{
    public class DataBaseTemplate<T> where T : ScriptableObject 
    {
        static Dictionary<string, T> data;

        public static void Init()
        {
            data = new Dictionary<string, T>();

            var dataArray = Resources.LoadAll<T>("");
            foreach (var d in dataArray)
            {
                if (data.ContainsKey(d.name))
                {
                    Debug.LogError($"There is two {typeof(T)} with the name {d.name}");
                    continue;
                }

                data[d.name] = d;
            }
        }

        public static T GetDataByName(string name)
        {
            if (!data.ContainsKey(name))
            {
                Debug.LogError($"{typeof(T)} data with name {name} not found in the Database.");
                return null;
            }

            return data[name];
        }


    }
}
