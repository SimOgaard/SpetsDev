using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

[ExecuteInEditMode]
public class WorldGenerationManager : MonoBehaviour
{
    public static Chunk[,] chunks = new Chunk[200, 200];
    public static List<Chunk> chunks_in_loading;

    [SerializeField] private NoiseLayerSettings noise_layer_settings;
    private Noise.NoiseLayer[] noise_layers;
    private NativeArray<Noise.NoiseLayer> noise_layers_native_array;
    private void OnDestroy()
    {
        if (noise_layers_native_array.IsCreated)
        {
            noise_layers_native_array.Dispose();
        }
    }

    public static Vector2 static_chunk_size;

    [System.Serializable]
    public struct ChunkDetails
    {
        [Min(0.01f)] public Vector2 unit_size;
        [Min(2)] public Vector2Int resolution;
        public float chunk_disable_distance;
        public float chunk_enable_distance;
        public int chunk_load_dist;
        public int max_chunk_load_dist;
        [Range(0.01f, 1f)] public float chunk_load_speed;
        public int max_chunk_load_at_a_time_init;
        public int max_chunk_load_at_a_time;
        [HideInInspector] public Vector2 offset;
    }
    [SerializeField] private ChunkDetails chunk_details;

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
        if (!chunk.is_loading)
        {
            StartCoroutine(chunk.LoadChunk(noise_layer_settings, noise_layers_native_array, chunk_details));
        }
    }

    public void InstaLoadChunk(Chunk chunk)
    {
        var load = chunk.LoadChunk(noise_layer_settings, noise_layers_native_array, chunk_details);
        while (load.MoveNext()) { }
    }

    private void AddCurveToAllMaterials()
    {
        CurveCreator.AddCurveTextures(ref noise_layer_settings.material_static);
        CurveCreator.AddCurveTextures(ref noise_layer_settings.material_leaf);
        CurveCreator.AddCurveTextures(ref noise_layer_settings.material_wood);
        CurveCreator.AddCurveTextures(ref noise_layer_settings.material_stone);
        CurveCreator.AddCurveTextures(ref noise_layer_settings.material_golem);
        CurveCreator.AddCurveTextures(ref noise_layer_settings.material_golem_shoulder);

        CurveCreator.AddCurveTextures(ref noise_layer_settings.water.material);

        CurveCreator.AddCurveTextures(ref noise_layer_settings.material_sun);
        CurveCreator.AddCurveTextures(ref noise_layer_settings.material_moon);

        foreach (NoiseLayerSettings.Foliage foliage in noise_layer_settings.random_foliage)
        {
            CurveCreator.AddCurveTextures(ref foliage.material);
        }

        // set materials to be globally accesable
        Global.Materials.stone_material = noise_layer_settings.material_stone.material;
        Global.Materials.water_material = noise_layer_settings.water.material.material;
    }

    private void Awake()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        chunks_in_loading = new List<Chunk>();

        AddCurveToAllMaterials();

        noise_layers = Noise.CreateNoiseLayers(noise_layer_settings);
#if UNITY_EDITOR
        LoadNoiseTextures();
#endif

        ColliderMeshManipulation.triangle_size_margin = Mathf.Max(chunk_details.unit_size.x, chunk_details.unit_size.y);
        chunk_details.offset = chunk_details.unit_size * chunk_details.resolution;
        static_chunk_size = chunk_details.offset;

        Water water = new GameObject().AddComponent<Water>();
        water.Init(noise_layer_settings.water, 350f, 350f, transform);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif

        noise_layers_native_array = new NativeArray<Noise.NoiseLayer>(noise_layers, Allocator.Persistent);

        //LoadNearestChunk(Vector3.zero/*, true*/);
        LoadNearest(chunk_details.max_chunk_load_at_a_time_init);
        StartCoroutine(LoadProgressively());

        Application.targetFrameRate = -1;
    }

    private void LoadNearest(int max_chunk_load_at_a_time)
    {
        int load_dist = Mathf.Min(Mathf.RoundToInt(chunk_details.chunk_load_dist / PixelPerfectCameraRotation.zoom), chunk_details.max_chunk_load_dist);

        for (int x = -load_dist; x <= load_dist; x++)
        {
            for (int z = -load_dist; z <= load_dist; z++)
            {
                LoadNearestChunk(Global.player_transform.localPosition + new Vector3((float)x * chunk_details.offset.x, 0f, (float)z * chunk_details.offset.y));
            }
        }

        chunks_in_loading.Sort(delegate (Chunk c1, Chunk c2) { return c1.DistToPlayer().CompareTo(c2.DistToPlayer()); });

        for (int i = 0; i < max_chunk_load_at_a_time && i < chunks_in_loading.Count; i++)
        {
            StartLoadingChunk(chunks_in_loading[i]);
        }
    }

    private IEnumerator LoadProgressively()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        while (true)
        {
            LoadNearest(chunk_details.max_chunk_load_at_a_time);
            yield return wait;
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        AddCurveToAllMaterials();
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
#endif
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
            Mathf.RoundToInt(position_2d.x / static_chunk_size.x) + 100,
            Mathf.RoundToInt(position_2d.y / static_chunk_size.y) + 100
        );
        return nearest_chunk;
    }

    public static Vector2 ReturnNearestChunkCoord(Vector3 position)
    {
        Vector2 position_2d = new Vector2(position.x, position.z);
        Vector2Int nearest_chunk = new Vector2Int(
            Mathf.RoundToInt(position_2d.x / static_chunk_size.x),
            Mathf.RoundToInt(position_2d.y / static_chunk_size.y)
        );
        Vector2 chunk_coord = nearest_chunk * static_chunk_size;
        return chunk_coord;
    }

    public static Chunk ReturnNearestChunk(Vector3 position)
    {
        Vector2Int nearest_chunk = ReturnNearestChunkIndex(position);
        Vector2Int nearest_chunk_index = nearest_chunk;
        return chunks[nearest_chunk_index.x, nearest_chunk_index.y];
    }

    public Chunk LoadNearestChunk(Vector3 position)
    {
        Vector2Int nearest_chunk = ReturnNearestChunkIndex(position);
        Vector2Int nearest_chunk_index = nearest_chunk;
        Chunk chunk = chunks[nearest_chunk_index.x, nearest_chunk_index.y];

        if (chunk == null)
        {
            chunk = InstanciateChunkGameObject(ReturnNearestChunkCoord(position));
            chunks[nearest_chunk_index.x, nearest_chunk_index.y] = chunk;
            chunk.Initialize(chunk_details.chunk_disable_distance, Global.player_transform);
            chunks_in_loading.Add(chunk);
        }
        else if (!chunk.gameObject.activeSelf && (chunk.transform.position - Global.player_transform.position).magnitude < chunk_details.chunk_enable_distance / PixelPerfectCameraRotation.zoom)
        {
            // enable chunk
            chunk.gameObject.SetActive(true);
        }

        return chunk;
    }

    public static Chunk[] ReturnAllCunksInBounds(Bounds bounds)
    {
        // get nearest chunk for min max coords
        Vector2Int max_index = ReturnNearestChunkIndex(bounds.max);
        Vector2Int min_index = ReturnNearestChunkIndex(bounds.min);

        // create list of length:
        Vector2Int diff_index = max_index - min_index + Vector2Int.one;
        int length = diff_index.x * diff_index.y;
        Chunk[] chunks_in_bounds = new Chunk[length];

        // populate list with every chunk between max_index and min_index
        int i = 0;
        for (int x = min_index.x; x <= max_index.x; x++)
        {
            for (int z = min_index.y; z <= max_index.y; z++)
            {
                chunks_in_bounds[i] = chunks[x, z];
                i++;
            }
        }

        return chunks_in_bounds;
    }
}
