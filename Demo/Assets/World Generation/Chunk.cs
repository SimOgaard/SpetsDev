using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class Chunk : MonoBehaviour
{
    public bool isLoading;
    public bool isLoaded;
    private float chunkDisableDistance;
    private Transform playerTransform;
    private Enemies enemies;
    public GroundMesh groundMesh;

    public void Initialize(float chunkDisableDistance, Transform playerTransform)
    {
        isLoading = false;
        isLoaded = false;
        gameObject.layer = Layer.gameWorld;
        this.chunkDisableDistance = chunkDisableDistance;
        this.playerTransform = playerTransform;
    }

    public IEnumerator LoadChunk(NoiseLayerSettings noiseLayerSettings, NativeArray<Noise.NoiseLayer> noiseLayersNativeArray, WorldGenerationManager.ChunkDetails chunkDetails/*, int[] triangles*/)
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        // initilize variables
        isLoading = true;

        // create ground
        GameObject groundGameObject = new GameObject("Ground");
        groundGameObject.transform.parent = transform;
        groundGameObject.transform.localPosition = Vector3.zero;
        groundGameObject.layer = Layer.gameWorld;
        groundMesh = groundGameObject.AddComponent<GroundMesh>();
        yield return groundMesh.CreateGround(wait, chunkDetails.unitSize, chunkDetails.resolution, noiseLayersNativeArray, noiseLayerSettings.materialStatic.material, noiseLayerSettings.randomFoliage);

        // initilizes all parrent objects of prefabs
        GameObject landMarksGameObject;
        WorldGenerationManager.InitNewChild(out landMarksGameObject, transform, SpawnInstruction.PlacableGameObjectsParrent.landMarks);
        GameObject rocksGameObject;
        WorldGenerationManager.InitNewChild(out rocksGameObject, transform, SpawnInstruction.PlacableGameObjectsParrent.rocks);
        GameObject treesGameObject;
        WorldGenerationManager.InitNewChild(out treesGameObject, transform, SpawnInstruction.PlacableGameObjectsParrent.trees);
        GameObject chestsGameObject;
        WorldGenerationManager.InitNewChild(out chestsGameObject, transform, SpawnInstruction.PlacableGameObjectsParrent.chests);
        GameObject enemiesGameObject;
        WorldGenerationManager.InitNewChild(out enemiesGameObject, transform, SpawnInstruction.PlacableGameObjectsParrent.enemies);
        enemies = enemiesGameObject.AddComponent<Enemies>();

        // spawns prefabss
        SpawnPrefabs spawnPrefabs = gameObject.AddComponent<SpawnPrefabs>();
        yield return StartCoroutine(spawnPrefabs.Spawn(wait, noiseLayerSettings.spawnPrefabs, noiseLayerSettings.objectDensity, chunkDetails.offset));

        PlaceInWorld.SetRecursiveToGameWorld(gameObject);

        groundGameObject.layer = Layer.gameWorldStatic;
        isLoading = false;
        isLoaded = true;
        WorldGenerationManager.chunksInLoading.Remove(this);
    }
   
    public float DistToPlayer()
    {
        return (transform.position - playerTransform.position).magnitude;
    }

    private void Update()
    {
        if (DistToPlayer() > chunkDisableDistance / PixelPerfectCameraRotation.zoom && isLoaded)
        {
            enemies.MoveParrent();
            gameObject.SetActive(false);
        }
    }
}
