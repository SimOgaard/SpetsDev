using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WorldGenerationManager : MonoBehaviour
{
    public static Chunk[,] chunks = new Chunk[200, 200];
    public static List<Chunk> chunks_in_loading = new List<Chunk>();

    [SerializeField] private NoiseLayerSettings noise_layer_settings;
    private Noise.NoiseLayer[] noise_layers;

    public static Vector2 static_chunk_size;

    [System.Serializable]
    public struct ChunkDetails
    {
        [Min(0.01f)] public Vector2 unit_size;
        [Min(2)] public Vector2Int resolution;
        public float chunk_disable_distance;
        [HideInInspector] public float chunk_disable_distance_squared;
        public float chunk_enable_distance;
        [HideInInspector] public float chunk_enable_distance_squared;
        public int chunk_load_dist;
        [Range(0.01f, 1f)] public float chunk_load_speed;
        public int max_chunk_load_at_a_time;
        [HideInInspector] public Vector2 offset;
    }
    [SerializeField] private ChunkDetails chunk_details;

    [System.Serializable]
    public struct WaterDetails
    {
        public Material water_material;
        public NoiseLayerSettings.Curve water_curve_color;
        public NoiseLayerSettings.Curve water_curve_alpha;
        public float level;
        public float bobing_frequency;
        public float bobing_amplitude;
    }
    [SerializeField] private WaterDetails water_details;

    public Chunk InstanciateChunkGameObject(Vector2 chunk_coord)
    {
        GameObject chunk_game_object = new GameObject("chunk " + chunk_coord);
        chunk_game_object.transform.parent = transform;
        chunk_game_object.transform.localPosition = new Vector3(chunk_coord.x, 0f, chunk_coord.y);
        Chunk chunk_object = chunk_game_object.AddComponent<Chunk>();
        return chunk_object;
    }

    public void StartLoadingChunk(Chunk chunk)
    {
        StartCoroutine(chunk.LoadChunk(noise_layer_settings, noise_layers, chunk_details, player_transform));
    }

    public void InstaLoadChunk(Chunk chunk)
    {
        var load = chunk.LoadChunk(noise_layer_settings, noise_layers, chunk_details, player_transform, true);
        while (load.MoveNext()) { }
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

        chunk_details.offset = chunk_details.unit_size * chunk_details.resolution;
        static_chunk_size = chunk_details.offset;
        chunk_details.chunk_disable_distance_squared = chunk_details.chunk_disable_distance * chunk_details.chunk_disable_distance;
        chunk_details.chunk_enable_distance_squared = chunk_details.chunk_enable_distance * chunk_details.chunk_enable_distance;
        player_transform = GameObject.Find("Player").transform;

        Water water = new GameObject().AddComponent<Water>();
        water.Init(water_details.water_material, water_details.water_curve_color, water_details.water_curve_alpha, 350f, 350f, water_details.level, transform);

        //LoadNearestChunk(Vector3.zero/*, true*/);
        StartCoroutine(LoadProgressively());

        Application.targetFrameRate = -1;
    }

    private IEnumerator LoadProgressively()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        while (true)
        {
            for (int ring = 0; ring < chunk_details.chunk_load_dist; ring++)
            {
                int x_start = -ring;
                int z_start = -ring;

                for (int x = 0; x <= 2 * ring; x++)
                {
                    for (int z = 0; z <= 2 * ring; z++)
                    {
                        yield return wait;
                        LoadNearestChunk(player_transform.localPosition + new Vector3((float)(x_start + x) * chunk_details.offset.x, 0f, (float)(z_start + z) * chunk_details.offset.y));
                    }
                }
            }
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif

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

    public static Vector2Int ReturnNearestChunkIndex(Vector3 position)
    {
        Vector2 position_2d = new Vector2(position.x, position.z);
        Vector2Int nearest_chunk = new Vector2Int(
            Mathf.RoundToInt(position_2d.x / static_chunk_size.x),
            Mathf.RoundToInt(position_2d.y / static_chunk_size.y)
        );
        return nearest_chunk;
    }

    public static Vector2 ReturnNearestChunkCoord(Vector3 position)
    {
        Vector2Int nearest_chunk = ReturnNearestChunkIndex(position);
        Vector2 chunk_coord = nearest_chunk * static_chunk_size;
        return chunk_coord;
    }

    public static Chunk ReturnNearestChunk(Vector3 position)
    {
        Vector2Int chunk_index_offset = new Vector2Int(100, 100);
        Vector2Int nearest_chunk = ReturnNearestChunkIndex(position);
        Vector2Int nearest_chunk_index = nearest_chunk + chunk_index_offset;
        return chunks[nearest_chunk_index.x, nearest_chunk_index.y];
    }

    public Chunk LoadNearestChunk(Vector3 position, bool insta_load = false)
    {
        Vector2Int chunk_index_offset = new Vector2Int(100, 100);
        Vector2Int nearest_chunk = ReturnNearestChunkIndex(position);
        Vector2Int nearest_chunk_index = nearest_chunk + chunk_index_offset;
        Chunk chunk = chunks[nearest_chunk_index.x, nearest_chunk_index.y];

        if (chunk == null)
        {
            if (chunks_in_loading.Count < chunk_details.max_chunk_load_at_a_time)
            {
                // Create chunk
                chunk = InstanciateChunkGameObject(ReturnNearestChunkCoord(position));
                chunks_in_loading.Add(chunk);
                chunks[nearest_chunk_index.x, nearest_chunk_index.y] = chunk;
                //Debug.Log("started loading: " + chunk.name);
                if (insta_load)
                {
                    InstaLoadChunk(chunk);
                }
                else
                {
                    StartLoadingChunk(chunk);
                }
            }
        }
        else if (!chunk.gameObject.activeSelf && (chunk.transform.position - player_transform.position).sqrMagnitude < chunk_details.chunk_enable_distance_squared)
        {
            // enable chunk
            chunk.gameObject.SetActive(true);
        }

        return chunk;
    }
}
