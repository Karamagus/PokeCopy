using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MenuTracker", menuName = "MenuTracker/ Create MenuTracker")]
public class MenuTracker : ScriptableObject
{
    static GameObject currentMenu;
    GameObject previousMenu;
    
    public void SetCurrentMenu(GameObject current)
    {
        currentMenu = current;
    }

    public GameObject GetCurrentMenu()
    {
        return currentMenu;
    }

    public void SetPreviousMenu(GameObject previous)
    {
        previousMenu = previous;
    }

    public GameObject GetPreviousMenu()
    {
        return previousMenu;
    }


}
