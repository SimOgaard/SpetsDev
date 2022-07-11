using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines how all plain chunks are constructed
/// </summary>
[CreateAssetMenu(menuName = "WorldGeneration/Chunk", order = 1)]
[System.Serializable]
public class ChunkSettings : ScriptableObject
{
    /// <summary>
    /// The final size of the chunk
    /// </summary>
    [Min(0.01f)] public Vector2 _chunkSize = Vector2.one * 100f;
    public static Vector2 chunkSize;

    /// <summary>
    /// The resolution of the chunk mesh, how many quads make up the chunk in x, z
    /// </summary>
    [Min(1)] public Vector2Int quadAmount = Vector2Int.one * 100;

    /// <summary>
    /// The distance from camera origin a chunk should start to load in
    /// </summary>
    public float _chunkLoadDist = 350f;
    private static float chunkLoadDistPrivate;
    public static float chunkLoadDist
    {
        get { return chunkLoadDistPrivate + PixelPerfect.cameraMaxRadius; }
        set { chunkLoadDistPrivate = value; }
    }
    /// <summary>
    /// The distance from camera origin a chunks game objects should be disabled
    /// </summary>
    public float _chunkDisableDistance = 300f;
    private static float chunkDisableDistancePrivate;
    public static float chunkDisableDistance
    {
        get { return chunkDisableDistancePrivate + PixelPerfect.cameraMaxRadius; }
        set { chunkDisableDistancePrivate = value; }
    }
    /// <summary>
    /// The distance from camera origin a chunks game objects should be enabled
    /// </summary>
    public float _chunkEnableDistance = 250f;
    private static float chunkEnableDistancePrivate;
    public static float chunkEnableDistance
    {
        get { return chunkEnableDistancePrivate + PixelPerfect.cameraMaxRadius; }
        set { chunkEnableDistancePrivate = value; }
    }

    /// <summary>
    /// How fast prefabs should be spawned into the chunk when it gets loaded
    /// </summary>
    [Range(0.01f, 1f)] public float prefabSpawnSpeed;
    /// <summary>
    /// How many chunks at scene load time should be initilized in parrarell
    /// </summary>
    public int maxChunkLoadAtATimeInit;
    /// <summary>
    /// Limits how many chunks can be initilized in parrarell during gameplay
    /// </summary>
    public int maxChunkLoadAtATime;
    /// <summary>
    /// How many uniformed points inside circle around player should be tried to load chunks
    /// </summary>
    public int chunkLoadPrecision = 50;

    /// <summary>
    /// Updates static ground mesh const to adher to new chunk settings
    /// </summary>
    [ContextMenu("Update", false, -1000)]
    public void Update()
    {
        chunkSize = _chunkSize;

        chunkLoadDist = _chunkLoadDist;
        chunkDisableDistance = _chunkDisableDistance;
        chunkEnableDistance = _chunkEnableDistance;

        // update triangle size margin for mesh manipulation
        ColliderMeshManipulation.triangleSizeMargin = Mathf.Max(chunkSize.x / quadAmount.x, chunkSize.y / quadAmount.y);
    }
}
