using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Linq;

[ExecuteInEditMode]
public class WorldGenerationManager : MonoBehaviour
{
    public static Chunk[,] chunks;
    public static List<Chunk> chunksInLoading;
    public static Transform worldGenerationManagerTransform;

    public WorldGenerationSettings worldGenerationSettings;
    [HideInInspector] public bool foldout;

    [ContextMenu("Regenerate")]
    public void Regenerate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        // Re-run this like it was game start
        Awake();
        Start();

        // Wait for game to not be paused and then place player on chunk
        StartCoroutine(PlacePlayer(Global.playerTransform.position));

        Debug.Log("Regenerated world");
    }

    public static IEnumerator PlacePlayer(Vector3 position)
    {
        // place player at wanted position
        Global.playerTransform.position = position;
        yield return new WaitForEndOfFrame();

        // wait for ground to be on the same place as player
        RaycastHit hit = new RaycastHit();
        while (true)
        {
            RaycastHit[] hits = Physics.SphereCastAll(position + Vector3.up * 10_000f, 5f, Vector3.down, float.MaxValue).OrderBy(h => h.distance).ToArray();
            bool didHit = false;

            for (int i = 0; i < hits.Length; i++)
            {
                if (Layer.IsInLayer(Layer.gameWorldStatic, hits[i].transform.gameObject.layer))
                {
                    hit = hits[i];
                    didHit = true;
                    break;
                }
            }

            if (didHit)
            {
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        // raycast position downwards to place player on world to not clip
        Global.playerTransform.position = hit.point + Vector3.up * 10f;
    }

    private void OnDestroy()
    {
        Ground.GPUData.Destroy();
        worldGenerationSettings.Destroy();
    }

    public static Chunk InstanciateChunkGameObject(Vector2 chunkCoord)
    {
        GameObject chunkGameObject = new GameObject("chunk " + chunkCoord);
        chunkGameObject.transform.parent = worldGenerationManagerTransform;
        chunkGameObject.transform.localPosition = new Vector3(chunkCoord.x, 0f, chunkCoord.y);
        Chunk chunkObject = chunkGameObject.AddComponent<Chunk>();
        return chunkObject;
    }

    public void StartLoadingChunk(Chunk chunk)
    {
        if (!chunk.isLoading)
        {
            StartCoroutine(chunk.LoadChunk(worldGenerationSettings));
        }
    }

    public void InstaLoadChunk(Chunk chunk)
    {
        var load = chunk.LoadChunk(worldGenerationSettings);
        while (load.MoveNext()) { }
    }

    private void Awake()
    {
        // destroy all child gameobjects
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        worldGenerationManagerTransform = transform;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif

        // set global properties
        Global.PreLoad();

        // add update renders
        worldGenerationSettings.AddUpdateRenders();

        // update all assets
        worldGenerationSettings.Update();

        // create chunk lists
        chunksInLoading = new List<Chunk>();
        chunks = new Chunk[200, 200];

        /*
#if UNITY_EDITOR
        LoadNoiseTextures();
#endif
        */

        Water water = new GameObject().AddComponent<Water>();
        water.Init(worldGenerationSettings.water, 350f, 350f, transform);

        //LoadNearest(worldGenerationSettings.chunkSettings.maxChunkLoadAtATimeInit);
        StartCoroutine(LoadProgressively());

        Application.targetFrameRate = -1;
    }

    private void Start()
    {
        // set global properties
        Global.PostLoad();
    }

    private void LoadNearest(int maxChunkLoadAtATime)
    {
        void sunflower(int n, float alpha)
        {
            float radius(int k, int n, int b)
            {
                if (k > n - b)
                {
                    return ChunkSettings.chunkLoadDistance; // put on the boundary
                }
                else
                {
                    return ChunkSettings.chunkLoadDistance * (Mathf.Sqrt(k - 1 / 2) / Mathf.Sqrt(n - (b + 1) / 2)); // apply square root
                }
            }

            int b = Mathf.RoundToInt(alpha * Mathf.Sqrt(n)); // number of boundary points
            float phi = (Mathf.Sqrt(5) + 1) / 2; // golden ratio
            for (int k = 1; k <= n; k++)
            {
                float r = radius(k, n, b);
                float theta = 2 * Mathf.PI * k / Mathf.Pow(phi, 2);

                LoadNearestChunk(MainCamera.CameraRayHitPlane() + new Vector3(r * Mathf.Cos(theta), 0f, r * Mathf.Sin(theta)));
            }
        }

        // start loading in sunflower orientation
        sunflower(worldGenerationSettings.chunk.chunkLoadPrecision, 0f);

        // sort the chunks that are supposed to be loading
        chunksInLoading.Sort(delegate (Chunk c1, Chunk c2) { return c1.DistToPlayer().CompareTo(c2.DistToPlayer()); });

        // start loading the closests chunks
        for (int i = 0; i < maxChunkLoadAtATime && i < chunksInLoading.Count; i++)
        {
            StartLoadingChunk(chunksInLoading[i]);
        }
    }

    private IEnumerator LoadProgressively()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        while (true)
        {
            LoadNearest(worldGenerationSettings.chunk.maxChunkLoadAtATime);
            yield return wait;
        }
    }

    /*
#if UNITY_EDITOR
    [HideInInspector] public bool foldout = true;
    private Texture2D[] noiseTextures;
    private void LoadNoiseTextures()
    {
        noiseTextures = new Texture2D[noiseLayers.Length];
        for (int i = 0; i < noiseTextures.Length; i++)
        {
            noiseTextures[i] = noiseLayers[i].GetNoiseTexture(Vector2Int.one * 100);
        }
    }
    public Texture2D[] GetNoiseTextures()
    {
        return noiseTextures;
    }
    public NoiseLayerSettings GetNoiseSettings()
    {
        return noiseLayerSettings;
    }
    public void UpdateWorld()
    {
        LoadNoiseTextures();
    }
#endif
    */

    #region InitGameObjects
    public static GameObject InitNewChild(Transform parrent, InstantiateInstruction.PlacableGameObjectsParrent name)
    {
        return InitNewChild(parrent, InstantiateInstruction.GetHierarchyName(name));
    }

    public static GameObject InitNewChild(Transform parrent, string name)
    {
        GameObject gameObject;
        InitNewChild(out gameObject, parrent, name);
        return gameObject;
    }

    public static void InitNewChild(out GameObject child, Transform parrent, InstantiateInstruction.PlacableGameObjectsParrent name)
    {
        InitNewChild(out child, parrent, InstantiateInstruction.GetHierarchyName(name));
    }

    public static void InitNewChild(out GameObject child, Transform parrent, string name)
    {
        child = new GameObject(name);
        child.transform.parent = parrent;
        child.transform.localPosition = Vector3.zero;
    }

    public static GameObject InitNewChild(Transform parrent, InstantiateInstruction.PlacableGameObjectsParrent name, params System.Type[] components)
    {
        return InitNewChild(parrent, InstantiateInstruction.GetHierarchyName(name), components);
    }

    public static GameObject InitNewChild(Transform parrent, string name, params System.Type[] components)
    {
        GameObject gameObject;
        InitNewChild(out gameObject, parrent, name, components);
        return gameObject;
    }

    public static void InitNewChild(out GameObject child, Transform parrent, InstantiateInstruction.PlacableGameObjectsParrent name, params System.Type[] components)
    {
        InitNewChild(out child, parrent, InstantiateInstruction.GetHierarchyName(name), components);
    }

    public static void InitNewChild(out GameObject child, Transform parrent, string name, params System.Type[] components)
    {
        child = new GameObject(name);
        child.transform.parent = parrent;
        child.transform.localPosition = Vector3.zero;

        for (int i = 0; i < components.Length; i++)
        {
            child.AddComponent(components[i]);
        }
    }
    #endregion

    #region ChunkFunctions
    public static Vector2Int ReturnNearestChunkIndex(Vector3 position)
    {
        Vector2 position_2d = new Vector2(position.x, position.z);
        Vector2Int nearestChunk = new Vector2Int(
            Mathf.RoundToInt(position_2d.x / ChunkSettings.meshSize.x) + 100,
            Mathf.RoundToInt(position_2d.y / ChunkSettings.meshSize.y) + 100
        );

        return nearestChunk;
    }

    public static Vector2 ReturnNearestChunkCoord(Vector3 position)
    {
        Vector2 position_2d = new Vector2(position.x, position.z);
        Vector2Int nearestChunk = new Vector2Int(
            Mathf.RoundToInt(position_2d.x / ChunkSettings.meshSize.x),
            Mathf.RoundToInt(position_2d.y / ChunkSettings.meshSize.y)
        );
        Vector2 chunkCoord = nearestChunk * ChunkSettings.meshSize;
        return chunkCoord;
    }

    public static Chunk ReturnNearestChunk(Vector3 position)
    {
        Vector2Int nearestChunkIndex = ReturnNearestChunkIndex(position);
        return chunks[nearestChunkIndex.x, nearestChunkIndex.y];
    }

    public static Chunk LoadNearestChunk(Vector3 position)
    {
        Vector2Int nearestChunkIndex = ReturnNearestChunkIndex(position);
        
        Chunk chunk = chunks[nearestChunkIndex.x, nearestChunkIndex.y];

        if (chunk == null)
        {
            chunk = InstanciateChunkGameObject(ReturnNearestChunkCoord(position));
            chunks[nearestChunkIndex.x, nearestChunkIndex.y] = chunk;
            chunksInLoading.Add(chunk);
        }
        else if (!chunk.gameObject.activeSelf && (chunk.transform.position - MainCamera.CameraRayHitPlane()).magnitude < ChunkSettings.chunkEnableDistance)
        {
            // enable chunk
            chunk.gameObject.SetActive(true);
        }

        return chunk;
    }

    public static Chunk[] ReturnAllCunksInBounds(Bounds bounds)
    {
        // get nearest chunk for min max coords
        Vector2Int maxIndex = ReturnNearestChunkIndex(bounds.max);
        Vector2Int minIndex = ReturnNearestChunkIndex(bounds.min);

        // create list of length:
        Vector2Int diffIndex = maxIndex - minIndex + Vector2Int.one;
        int length = diffIndex.x * diffIndex.y;
        Chunk[] chunksInBounds = new Chunk[length];

        // populate list with every chunk between maxIndex and minIndex
        int i = 0;
        for (int x = minIndex.x; x <= maxIndex.x; x++)
        {
            for (int z = minIndex.y; z <= maxIndex.y; z++)
            {
                chunksInBounds[i] = chunks[x, z];
                i++;
            }
        }

        return chunksInBounds;
    }
    #endregion
}
