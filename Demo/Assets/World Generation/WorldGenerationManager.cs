using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WorldGenerationManager : MonoBehaviour
{
    private Transform[,] chunks = new Transform[200, 200];
    [SerializeField] [Min(0.01f)] private Vector2 unit_size;
    [SerializeField] [Min(2)] private Vector2Int resolution;
    [SerializeField] private float chunk_unload_distance;
    private float chunk_unload_distance_squared;
    private Vector2 chunk_size;
    private float chunk_preload = 1f;
    [SerializeField] [Range(0.01f,1f)] private float chunk_load_speed = 1f;

    [Header("Water")]
    public Material water_material;
    public float level;
    public float bobing_frequency;
    public float bobing_amplitude;

    public enum Map { ColossalPlain };
    public Map map;

    public WorldGeneration world_generation = null;

    public interface WorldGeneration
    {
        void Init(Vector2 unit_size, Vector2Int resolution, Vector2 offset, float chunk_unload_distance_squared, Transform player_transform, float chunk_load_speed);
        void DestroyAll();
        NoiseLayerSettings GetNoiseSettings();
        Texture2D[] GetNoiseTextures();
        void UpdateWorld();
    }

    public Transform CreateWorld(Vector3 coord)
    {
        Vector2 coord_2d = ReturnNearestChunkCoord(coord);

        switch (map)
        {
            case Map.ColossalPlain:
                GameObject colossal_plains_game_object = new GameObject("colossal_plains");
                colossal_plains_game_object.transform.position = new Vector3(coord_2d.x, 0f, coord_2d.y);
                world_generation = colossal_plains_game_object.AddComponent<ColossalPlains>();
                colossal_plains_game_object.transform.parent = transform;
                world_generation.Init(unit_size, resolution, coord_2d, chunk_unload_distance_squared, player_transform, chunk_load_speed);
                return colossal_plains_game_object.transform;
        }
        return null;
    }

    private Transform player_transform;
    private void Awake()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        chunk_size = unit_size * resolution;
        chunk_unload_distance_squared = chunk_unload_distance * chunk_unload_distance;
        player_transform = GameObject.Find("Player").transform;

        Water water = new GameObject().AddComponent<Water>();
        water.Init(water_material, 100000, 100000, level, transform);

        chunks[100, 100] = CreateWorld(Vector3.zero);
    }

    private void Update()
    {
        Vector2 offset = chunk_size * chunk_preload;

        ReturnNearestChunk(player_transform.position + new Vector3(offset.x, 0f, offset.y));
        ReturnNearestChunk(player_transform.position + new Vector3(-offset.x, 0f, offset.y));
        ReturnNearestChunk(player_transform.position + new Vector3(offset.x, 0f, -offset.y));
        ReturnNearestChunk(player_transform.position + new Vector3(-offset.x, 0f, -offset.y));

        ReturnNearestChunk(player_transform.position + new Vector3(offset.x, 0f, 0f));
        ReturnNearestChunk(player_transform.position + new Vector3(-offset.x, 0f, 0f));
        ReturnNearestChunk(player_transform.position + new Vector3(0f, 0f, offset.y));
        ReturnNearestChunk(player_transform.position + new Vector3(0f, 0f, -offset.y));

        float water_level_wave = bobing_amplitude * Mathf.Sin(Time.time * bobing_frequency);
        Water.water_level = level + water_level_wave;
    }

    [HideInInspector] public bool foldout = true;

    public static void InitNewChild(out GameObject child, Transform parrent, SpawnInstruction.PlacableGameObjectsParrent name)
    {
        child = new GameObject(SpawnInstruction.GetHierarchyName(name));
        child.layer = Layer.game_world;
        child.isStatic = true;
        child.transform.parent = parrent;
    }

    public static void InitNewChild(out GameObject child, Transform parrent, SpawnInstruction.PlacableGameObjectsParrent name, params System.Type[] components)
    {
        child = new GameObject(SpawnInstruction.GetHierarchyName(name));
        child.layer = Layer.game_world;
        child.isStatic = true;
        child.transform.parent = parrent;

        for (int i = 0; i < components.Length; i++)
        {
            child.AddComponent(components[i]);
        }
    }

    public Vector2Int ReturnNearestChunkIndex(Vector3 position)
    {
        Vector2 position_2d = new Vector2(position.x, position.z);
        Vector2Int nearest_chunk = new Vector2Int(
            Mathf.RoundToInt(position_2d.x / chunk_size.x),
            Mathf.RoundToInt(position_2d.y / chunk_size.y)
        );
        return nearest_chunk;
    }

    public Vector2 ReturnNearestChunkCoord(Vector3 position)
    {
        Vector2Int nearest_chunk = ReturnNearestChunkIndex(position);
        Vector2 chunk_coord = nearest_chunk * chunk_size;
        return chunk_coord;
    }

    public Transform ReturnNearestChunk(Vector3 position)
    {
        Vector2Int chunk_index_offset = new Vector2Int(100, 100);
        Vector2Int nearest_chunk = ReturnNearestChunkIndex(position);
        Vector2Int nearest_chunk_index = nearest_chunk + chunk_index_offset;

        Transform chunk = chunks[nearest_chunk_index.x, nearest_chunk_index.y];
        if (chunk == null)
        {
            // Create chunk
            chunk = CreateWorld(position);
            chunks[nearest_chunk_index.x, nearest_chunk_index.y] = chunk;
        }
        else if (!chunk.gameObject.activeSelf)
        {
            // enable chunk
            chunk.gameObject.SetActive(true);
        }

        return chunk;
    }
}
