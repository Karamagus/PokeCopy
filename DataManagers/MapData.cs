using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu (fileName = "New MapData", menuName = "MapData/ Create new MapData")]
public class MapData : ScriptableObject
{
    [SerializeField] List<Pokemon> wildPokemons;
    [SerializeField] List<MapData> mapConections;
    [SerializeField] Scene scenes;


    public List<Pokemon> WildPokemons { get => wildPokemons; }


}
