using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerationManager : MonoBehaviour
{
    public enum Map { ColossalPlain };
    public Map map;

    public WorldGeneration world_generation;

    public interface WorldGeneration
    {

    }

    private void CreateWorld(Map map)
    {
        switch (map)
        {
            case Map.ColossalPlain:
                GameObject colossal_plains_game_object = new GameObject("colossal_plains", typeof(ColossalPlains));
                world_generation = colossal_plains_game_object.GetComponent<ColossalPlains>();
                colossal_plains_game_object.transform.parent = transform;
                break;
        }
    }

    private void Start()
    {
        CreateWorld(map);
    }
}
