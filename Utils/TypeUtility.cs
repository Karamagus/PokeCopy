using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Reflection;


public class TypeUtility
{
    public static Type GetTypeByName(string name)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.FullName.Contains("UnityEngine.WSA"))
                    continue;


                if (type.Name == name)
                    return type;
            }
        }

        return null;
    }



}


namespace pokeCopy
{

}
