using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

[ExecuteInEditMode]
public class WorldGenerationManager : MonoBehaviour
{
    public static Chunk[,] chunks = new Chunk[200, 200];
    public static List<Chunk> chunksInLoading;

    [SerializeField] private NoiseLayerSettings noiseLayerSettings;
    private Noise.NoiseLayer[] noiseLayers;

    [System.Serializable]
    public struct ChunkDetails
    {
        [Min(0.01f)] public Vector2 chunkSize;
        [Min(1)] public Vector2Int quadAmount;

        public int maxChunkLoadDist;
        public int chunkLoadDist;
        public float chunkEnableDistance;
        public float chunkDisableDistance;

        [Range(0.01f, 1f)] public float chunkLoadSpeed;
        public int maxChunkLoadAtATimeInit;
        public int maxChunkLoadAtATime;
    }
    [SerializeField] private ChunkDetails chunkDetails;
    public static ChunkDetails chunkDetailsStatic;

    private void OnValidate()
    {
        Debug.Log("OnValidate");

        // uppdate all material properties
        AddCurveToAllMaterials();

        // make a static version of chunk details
        chunkDetailsStatic = chunkDetails;

        // update triangle size margin for mesh manipulation
        ColliderMeshManipulation.triangleSizeMargin = Mathf.Max(chunkDetails.chunkSize.x / (float)chunkDetails.quadAmount.x, chunkDetails.chunkSize.y / (float)chunkDetails.quadAmount.y);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif
        // update static chunk ground
        Chunk.groundMeshConst.Update(noiseLayerSettings);
    }

    private void OnDestroy()
    {
        Chunk.groundMeshConst.Destroy();
    }

    public Chunk InstanciateChunkGameObject(Vector2 chunkCoord)
    {
        GameObject chunkGameObject = new GameObject("chunk " + chunkCoord);
        chunkGameObject.transform.parent = transform;
        chunkGameObject.transform.localPosition = new Vector3(chunkCoord.x, 0f, chunkCoord.y);
        Chunk chunkObject = chunkGameObject.AddComponent<Chunk>();
        return chunkObject;
    }

    public void StartLoadingChunk(Chunk chunk)
    {
        if (!chunk.isLoading)
        {
            StartCoroutine(chunk.LoadChunk(noiseLayerSettings));
        }
    }

    public void InstaLoadChunk(Chunk chunk)
    {
        var load = chunk.LoadChunk(noiseLayerSettings);
        while (load.MoveNext()) { }
    }

    private void AddCurveToAllMaterials()
    {
        CurveCreator.AddCurveTextures(ref noiseLayerSettings.materialStatic);
        CurveCreator.AddCurveTextures(ref noiseLayerSettings.materialLeaf);
        CurveCreator.AddCurveTextures(ref noiseLayerSettings.materialWood);
        CurveCreator.AddCurveTextures(ref noiseLayerSettings.materialStone);
        CurveCreator.AddCurveTextures(ref noiseLayerSettings.materialGolem);
        CurveCreator.AddCurveTextures(ref noiseLayerSettings.materialGolemShoulder);

        CurveCreator.AddCurveTextures(ref noiseLayerSettings.water.material);

        CurveCreator.AddCurveTextures(ref noiseLayerSettings.materialSun);
        CurveCreator.AddCurveTextures(ref noiseLayerSettings.materialMoon);

        foreach (NoiseLayerSettings.Foliage foliage in noiseLayerSettings.randomFoliage)
        {
            CurveCreator.AddCurveTextures(ref foliage.material);
        }

        // set materials to be globally accesable
        Global.Materials.stoneMaterial = noiseLayerSettings.materialStone.material;
        Global.Materials.waterMaterial = noiseLayerSettings.water.material.material;
    }

    private void Awake()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        chunksInLoading = new List<Chunk>();
        noiseLayers = Noise.CreateNoiseLayers(noiseLayerSettings);

#if UNITY_EDITOR
        LoadNoiseTextures();
#endif

        OnValidate();

        Water water = new GameObject().AddComponent<Water>();
        water.Init(noiseLayerSettings.water, 350f, 350f, transform);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif

        LoadNearest(chunkDetails.maxChunkLoadAtATimeInit);
        StartCoroutine(LoadProgressively());

        Application.targetFrameRate = -1;
    }

    private void LoadNearest(int maxChunkLoadAtATime)
    {
        int loadDist = Mathf.Min(Mathf.RoundToInt(chunkDetails.chunkLoadDist / PixelPerfectCameraRotation.zoom), chunkDetails.maxChunkLoadDist);

        for (int x = -loadDist; x <= loadDist; x++)
        {
            for (int z = -loadDist; z <= loadDist; z++)
            {
                LoadNearestChunk(Global.playerTransform.localPosition + new Vector3((float)x * chunkDetails.chunkSize.x, 0f, (float)z * chunkDetails.chunkSize.y));
            }
        }

        chunksInLoading.Sort(delegate (Chunk c1, Chunk c2) { return c1.DistToPlayer().CompareTo(c2.DistToPlayer()); });

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
            LoadNearest(chunkDetails.maxChunkLoadAtATime);
            yield return wait;
        }
    }

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

    #region InitGameObjects
    public static GameObject InitNewChild(Transform parrent, SpawnInstruction.PlacableGameObjectsParrent name)
    {
        return InitNewChild(parrent, SpawnInstruction.GetHierarchyName(name));
    }

    public static GameObject InitNewChild(Transform parrent, string name)
    {
        GameObject gameObject;
        InitNewChild(out gameObject, parrent, name);
        return gameObject;
    }

    public static void InitNewChild(out GameObject child, Transform parrent, SpawnInstruction.PlacableGameObjectsParrent name)
    {
        InitNewChild(out child, parrent, SpawnInstruction.GetHierarchyName(name));
    }

    public static void InitNewChild(out GameObject child, Transform parrent, string name)
    {
        child = new GameObject(name);
        child.transform.parent = parrent;
        child.transform.localPosition = Vector3.zero;
    }

    public static GameObject InitNewChild(Transform parrent, SpawnInstruction.PlacableGameObjectsParrent name, params System.Type[] components)
    {
        return InitNewChild(parrent, SpawnInstruction.GetHierarchyName(name), components);
    }

    public static GameObject InitNewChild(Transform parrent, string name, params System.Type[] components)
    {
        GameObject gameObject;
        InitNewChild(out gameObject, parrent, name, components);
        return gameObject;
    }

    public static void InitNewChild(out GameObject child, Transform parrent, SpawnInstruction.PlacableGameObjectsParrent name, params System.Type[] components)
    {
        InitNewChild(out child, parrent, SpawnInstruction.GetHierarchyName(name), components);
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
            Mathf.RoundToInt(position_2d.x / chunkDetailsStatic.chunkSize.x) + 100,
            Mathf.RoundToInt(position_2d.y / chunkDetailsStatic.chunkSize.y) + 100
        );

        return nearestChunk;
    }

    public static Vector2 ReturnNearestChunkCoord(Vector3 position)
    {
        Vector2 position_2d = new Vector2(position.x, position.z);
        Vector2Int nearestChunk = new Vector2Int(
            Mathf.RoundToInt(position_2d.x / chunkDetailsStatic.chunkSize.x),
            Mathf.RoundToInt(position_2d.y / chunkDetailsStatic.chunkSize.y)
        );
        Vector2 chunkCoord = nearestChunk * chunkDetailsStatic.chunkSize;
        return chunkCoord;
    }

    public static Chunk ReturnNearestChunk(Vector3 position)
    {
        Vector2Int nearestChunk = ReturnNearestChunkIndex(position);
        Vector2Int nearestChunkIndex = nearestChunk;
        return chunks[nearestChunkIndex.x, nearestChunkIndex.y];
    }

    public Chunk LoadNearestChunk(Vector3 position)
    {
        Vector2Int nearestChunk = ReturnNearestChunkIndex(position);
        Vector2Int nearestChunkIndex = nearestChunk;
        
        Chunk chunk = chunks[nearestChunkIndex.x, nearestChunkIndex.y];

        if (chunk == null)
        {
            chunk = InstanciateChunkGameObject(ReturnNearestChunkCoord(position));
            chunks[nearestChunkIndex.x, nearestChunkIndex.y] = chunk;
            chunksInLoading.Add(chunk);
        }
        else if (!chunk.gameObject.activeSelf && (chunk.transform.position - Global.playerTransform.position).magnitude < chunkDetails.chunkEnableDistance / PixelPerfectCameraRotation.zoom)
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
