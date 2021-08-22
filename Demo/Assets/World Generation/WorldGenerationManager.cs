using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WorldGenerationManager : MonoBehaviour
{
    public enum Map { ColossalPlain };
    public Map map;

    public WorldGeneration world_generation = null;

    public interface WorldGeneration
    {
        void DestroyAll();
        NoiseLayerSettings GetNoiseSettings();
        Texture2D[] GetNoiseTextures();
        void UpdateWorld();
    }

    public void CreateWorld()
    {
        if (transform.childCount != 0)
        {
            GameObject world_object = transform.GetChild(0).gameObject;
            DestroyImmediate(world_object);
        }

        switch (map)
        {
            case Map.ColossalPlain:
                GameObject colossal_plains_game_object = new GameObject("colossal_plains");
                world_generation = colossal_plains_game_object.AddComponent<ColossalPlains>();
                colossal_plains_game_object.transform.parent = transform;
                break;
        }
    }

    private void Start()
    {
        CreateWorld();
    }

    [HideInInspector] public bool foldout = true;

    public static void InitNewChild(out GameObject child, Transform parrent, SpawnInstruction.PlacableGameObjectsParrent name)
    {
        child = new GameObject(name.ToString());
        child.layer = 12;
        child.isStatic = true;
        child.transform.parent = parrent;
    }

    public static void InitNewChild(out GameObject child, Transform parrent, SpawnInstruction.PlacableGameObjectsParrent name, params System.Type[] components)
    {
        child = new GameObject(name.ToString());
        child.layer = 12;
        child.isStatic = true;
        child.transform.parent = parrent;

        for (int i = 0; i < components.Length; i++)
        {
            child.AddComponent(components[i]);
        }
    }
}
