using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WorldGenerationManager : MonoBehaviour
{
    private Transform[,] chunks = new Transform[200, 200];

    [SerializeField] private NoiseLayerSettings noise_layer_settings;
    private Noise.NoiseLayer[] noise_layers;

    [System.Serializable]
    public struct ChunkDetails
    {
        [Min(0.01f)] public Vector2 unit_size;
        [Min(2)] public Vector2Int resolution;
        public float chunk_unload_distance;
        [HideInInspector] public float chunk_unload_distance_squared;
        [HideInInspector] public Vector2 chunk_size;
        public float chunk_preload_factor;
        [Range(0.01f, 1f)] public float chunk_load_speed;
    }
    [SerializeField] private ChunkDetails chunk_details;

    [System.Serializable]
    public struct WaterDetails
    {
        public Material water_material;
        public float level;
        public float bobing_frequency;
        public float bobing_amplitude;
    }
    [SerializeField] private WaterDetails water_details;

    public Chunk InstanciateChunkGameObject(Vector3 coord)
    {
        Vector2 coord_2d = ReturnNearestChunkCoord(coord);
        Debug.Log("created new chunk game object for chunk " + coord_2d);
        GameObject chunk_game_object = new GameObject("chunk " + coord_2d);
        chunk_game_object.transform.parent = transform;
        chunk_game_object.transform.position = new Vector3(coord_2d.x, 0f, coord_2d.y);
        return chunk_game_object.AddComponent<Chunk>();
    }

    public void CreateChunk(Chunk chunk)
    {
        chunk.InitChunk(noise_layer_settings, noise_layers, chunk_details, player_transform);
    }

    private Transform player_transform;
    private void Awake()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        noise_layers = Noise.CreateNoiseLayers(noise_layer_settings);
        LoadNoiseTextures();

        chunk_details.chunk_size = chunk_details.unit_size * chunk_details.resolution;
        chunk_details.chunk_unload_distance_squared = chunk_details.chunk_unload_distance * chunk_details.chunk_unload_distance;
        player_transform = GameObject.Find("Player").transform;

        Water water = new GameObject().AddComponent<Water>();
        water.Init(water_details.water_material, 100000, 100000, water_details.level, transform);

        LoadNearestChunk(Vector3.zero);
    }

    private void Update()
    {
        Vector2 offset = chunk_details.chunk_size * chunk_details.chunk_preload_factor;
        
        LoadNearestChunk(player_transform.position + new Vector3(offset.x, 0f, offset.y));
        LoadNearestChunk(player_transform.position + new Vector3(-offset.x, 0f, offset.y));
        LoadNearestChunk(player_transform.position + new Vector3(offset.x, 0f, -offset.y));
        LoadNearestChunk(player_transform.position + new Vector3(-offset.x, 0f, -offset.y));

        LoadNearestChunk(player_transform.position + new Vector3(offset.x, 0f, 0f));
        LoadNearestChunk(player_transform.position + new Vector3(-offset.x, 0f, 0f));
        LoadNearestChunk(player_transform.position + new Vector3(0f, 0f, offset.y));
        LoadNearestChunk(player_transform.position + new Vector3(0f, 0f, -offset.y));

        float water_level_wave = water_details.bobing_amplitude * Mathf.Sin(Time.time * water_details.bobing_frequency);
        Water.water_level = water_details.level + water_level_wave;
    }

    [HideInInspector] public bool foldout = true;
    private Texture2D[] noise_textures;
    private void LoadNoiseTextures()
    {
        noise_textures = new Texture2D[noise_layers.Length];
        for (int i = 0; i < noise_textures.Length; i++)
        {
            noise_textures[i] = noise_layers[i].GetNoiseTexture(Vector2Int.one * 100);
        }
    }
    public Texture2D[] GetNoiseTextures()
    {
        return noise_textures;
    }
    public NoiseLayerSettings GetNoiseSettings()
    {
        return noise_layer_settings;
    }
    public void UpdateWorld()
    {
        LoadNoiseTextures();
    }

    public static void InitNewChild(out GameObject child, Transform parrent, SpawnInstruction.PlacableGameObjectsParrent name)
    {
        child = new GameObject(SpawnInstruction.GetHierarchyName(name));
        child.transform.parent = parrent;
    }

    public static void InitNewChild(out GameObject child, Transform parrent, SpawnInstruction.PlacableGameObjectsParrent name, params System.Type[] components)
    {
        child = new GameObject(SpawnInstruction.GetHierarchyName(name));
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
            Mathf.RoundToInt(position_2d.x / chunk_details.chunk_size.x),
            Mathf.RoundToInt(position_2d.y / chunk_details.chunk_size.y)
        );
        return nearest_chunk;
    }

    public Vector2 ReturnNearestChunkCoord(Vector3 position)
    {
        Vector2Int nearest_chunk = ReturnNearestChunkIndex(position);
        Vector2 chunk_coord = nearest_chunk * chunk_details.chunk_size;
        return chunk_coord;
    }

    public Transform ReturnNearestChunk(Vector3 position)
    {
        Vector2Int chunk_index_offset = new Vector2Int(100, 100);
        Vector2Int nearest_chunk = ReturnNearestChunkIndex(position);
        Vector2Int nearest_chunk_index = nearest_chunk + chunk_index_offset;
        return chunks[nearest_chunk_index.x, nearest_chunk_index.y];
    }

    public Transform LoadNearestChunk(Vector3 position)
    {
        Vector2Int chunk_index_offset = new Vector2Int(100, 100);
        Vector2Int nearest_chunk = ReturnNearestChunkIndex(position);
        Vector2Int nearest_chunk_index = nearest_chunk + chunk_index_offset;
        Transform chunk = chunks[nearest_chunk_index.x, nearest_chunk_index.y];

        if (chunk == null)
        {
            // Create chunk
            Chunk chunk_class = InstanciateChunkGameObject(position);
            chunk = chunk_class.transform;
            chunks[nearest_chunk_index.x, nearest_chunk_index.y] = chunk;
            CreateChunk(chunk_class);
        }
        else if (!chunk.gameObject.activeSelf)
        {
            // enable chunk
            chunk.gameObject.SetActive(true);
        }

        return chunk;
    }
}
