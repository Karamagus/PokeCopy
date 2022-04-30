using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "SceneData/New SceneData")]
public class SceneData : ScriptableObject
{
    //TO BE CLEANED
    bool loaded;
    public bool Loaded { get { return loaded; } private set { } }
    Vector3 playerPosition;
    Vector2 direction;
    Transform trannsformData;
    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPkmn;

    public Sprite PlayerChar { get; private set; }
    public Sprite TrainerChar { get; private set; }

    TrainerController trainer;
    TrainerData trainerData;

    public void ClearData()
    {
        playerParty = null;
        trainerData = null;
        TrainerChar = null;
    }

    public void GetPosition(Vector3 position)
    {
        loaded = true;
        playerPosition = position;
    }

    public void ReceivePositionData(Transform transform)
    {
        trannsformData = transform;
    }

    public void PassDirection(Vector2 d)
    {
        direction = d;
    }

    public void LoadParty(PokemonParty party)
    {
        playerParty = party;
    }

    public void LoadWilPkmn(Pokemon wild)
    {
        wildPkmn = wild;
    }

    public Vector3 ReturnPosition()
    {
        if (loaded)
        {
            loaded = false;
            return playerPosition;

        }
        else
            return Vector3.zero;

    }

    public Vector2 RetriveDirection()
    {
        return direction;
    }

    public PokemonParty RetrivePlayerParty()
    {
        return playerParty ? playerParty: null;
    }

    public PokemonParty RetriveTrainerParty()
    {
        return trainerParty ? trainerParty : null;
    }
    public void ReceiveBattleData(PokemonParty party, Pokemon wild)
    {
        playerParty = party;
        wildPkmn = wild;

    }

    public void ReceiveBattleData(PokemonParty party, PokemonParty trainer)
    {
        playerParty = party;
        trainerParty = trainer;
    }

    public Pokemon RetriveWildPkmn()
    {
        return wildPkmn;
    }

    public void SetPlayerCharSprite(Sprite playerchar)
    {
        PlayerChar = playerchar;
    }

    public void SetTrainerSprite(Sprite trainerSprite)
    {
        TrainerChar = trainerSprite;
    }

    public void GetTrainerData(TrainerController trainer)
    {
        this.trainer = trainer;
    }

    public void GetTrainerData(TrainerData trainer)
    {
        this.trainerData = trainer;
    }

    public TrainerController RetriveTrainer()
    {
        return trainer;
    }
    public TrainerData RetriveTrainerData()
    {
        return trainerData;
    }
}
