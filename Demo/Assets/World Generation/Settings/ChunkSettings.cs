using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines how all plain chunks are constructed
/// </summary>
[CreateAssetMenu()]
[System.Serializable]
public class ChunkSettings : ScriptableObject
{
    /// <summary>
    /// The final size of the chunk
    /// </summary>
    [Min(0.01f)] public Vector2 chunkSize = Vector2.one * 100f;
    /// <summary>
    /// The resolution of the chunk mesh, how many quads make up the chunk in x, z
    /// </summary>
    [Min(1)] public Vector2Int quadAmount = Vector2Int.one * 100;

    /// <summary>
    /// The distance from camera origin a chunk should start to load in
    /// </summary>
    public float chunkLoadDist = 300f;
    /// <summary>
    /// The distance from camera origin a chunks game objects should be disabled
    /// </summary>
    public float chunkDisableDistance = 250f;
    /// <summary>
    /// The distance from camera origin a chunks game objects should be enabled
    /// </summary>
    public float chunkEnableDistance = 200f;

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
    public void Update()
    {
        Chunk.groundMeshConst.Update(this);

        // update triangle size margin for mesh manipulation
        ColliderMeshManipulation.triangleSizeMargin = Mathf.Max(chunkSize.x / quadAmount.x, chunkSize.y / quadAmount.y);
    }
}
