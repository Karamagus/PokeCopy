using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pokeCopy;

public class Move
{
    public MovesBase Base { get; set; }

    public int Pp { get; set; }

    public bool isCharged = true;

    public int LastUsedTurn {get; set;}
    public int Consecutives { get; private set; }
    public int TurnsBfrHit { get; set; }

    public Move(MovesBase pBase)
    {
        Base = pBase;
        Pp = pBase.MaxPP;
    }

    public Move(MoveSaveData saveData)
    {
        Base = MoveDB.GetDataByName(saveData.name);
        Pp = saveData.pp;
    }



    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData()
        {
            name = Base.name,
            pp = this.Pp

        };

        return saveData;
    }





    public void consecutiveCount(int turn)
    {
        if(turn - LastUsedTurn > 1)
        {
            Consecutives = 0;
            return;
        }

        Consecutives++;

    }


    public void IncreasePP(int amount)
    {
        Pp = Mathf.Clamp(Pp + amount, 0, Base.MaxPP);
    }
}

[System.Serializable]
public class MoveSaveData
{
    public string name;
    public int pp; 
}
