using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ColossalPlains : MonoBehaviour
{
    /*
    private GameObject meshGameObject;
    private GameObject landMarksGameObject;
    private GameObject rocksGameObject;
    private GameObject treesGameObject;

    private NoiseLayerSettings noiseLayerSettings;
    private CreateMesh createMesh;
    private Texture2D[] noiseTextures;
    private SpawnPrefabs spawnPrefabs;

    private float chunkUnloadDistanceSquared;
    private Transform playerTransform;

    public void DestroyAll()
    {
        DestroyImmediate(meshGameObject);
        DestroyImmediate(landMarksGameObject);
        DestroyImmediate(rocksGameObject);
        DestroyImmediate(treesGameObject);
        DestroyImmediate(gameObject);
    }

    public NoiseLayerSettings GetNoiseSettings()
    {
        return noiseLayerSettings;
    }

    public void LoadInNoiseSettings()
    {
        noiseLayerSettings = (NoiseLayerSettings)AssetDatabase.LoadAssetAtPath("Assets/World Generation/Colossal Plains/ColossalPlains.asset", typeof(NoiseLayerSettings));
    }

    public Texture2D[] GetNoiseTextures()
    {
        return noiseTextures;
    }

    public void UpdateWorld()
    {
        Mesh groundMesh = createMesh.CreateMeshByNoise(GetNoiseSettings());
        createMesh.UpdateGrass(groundMesh);
        noiseTextures = createMesh.GetNoiseTextures();
        //spawnPrefabs.Spawn(noiseLayerSettings.spawnPrefabs, noiseLayerSettings.objectDensity, noiseLayerSettings.unitSize * noiseLayerSettings.resolution);
        CurveCreator.AddCurveTexture(ref noiseLayerSettings.materialLeaf, noiseLayerSettings.curveLeaf);
        CurveCreator.AddCurveTexture(ref noiseLayerSettings.materialWood, noiseLayerSettings.curveWood);
    }

    public void Init(Vector2 unitSize, Vector2Int resolution, Vector2 offset, float chunkUnloadDistanceSquared, Transform playerTransform, float chunkLoadSpeed)
    {
        gameObject.layer = Layer.gameWorld;
        gameObject.isStatic = true;

        LoadInNoiseSettings();
        noiseLayerSettings.unitSize = unitSize;
        noiseLayerSettings.resolution = resolution;
        noiseLayerSettings.offsett = offset;
        this.chunkUnloadDistanceSquared = chunkUnloadDistanceSquared;
        this.playerTransform = playerTransform;

        CurveCreator.AddCurveTexture(ref noiseLayerSettings.materialLeaf, noiseLayerSettings.curveLeaf);
        CurveCreator.AddCurveTexture(ref noiseLayerSettings.materialWood, noiseLayerSettings.curveWood);
        WorldGenerationManager.InitNewChild(out meshGameObject, transform, SpawnInstruction.PlacableGameObjectsParrent.groundMesh);
        meshGameObject.tag = "Flammable";
        createMesh = meshGameObject.AddComponent<CreateMesh>();
        spawnPrefabs = gameObject.AddComponent<SpawnPrefabs>();

        WorldGenerationManager.InitNewChild(out landMarksGameObject, transform, SpawnInstruction.PlacableGameObjectsParrent.landMarks);
        WorldGenerationManager.InitNewChild(out rocksGameObject, transform, SpawnInstruction.PlacableGameObjectsParrent.rocks);
        WorldGenerationManager.InitNewChild(out treesGameObject, transform, SpawnInstruction.PlacableGameObjectsParrent.trees);
        GameObject enemiesGameObject;
        WorldGenerationManager.InitNewChild(out enemiesGameObject, transform, SpawnInstruction.PlacableGameObjectsParrent.enemies, typeof(Enemies));
        GameObject interactGameObject;
        WorldGenerationManager.InitNewChild(out interactGameObject, transform, SpawnInstruction.PlacableGameObjectsParrent.interact, typeof(Interact));

        Mesh groundMesh = createMesh.CreateMeshByNoise(GetNoiseSettings());
        createMesh.CreateGrass(groundMesh);

        for (int i = 0; i < noiseLayerSettings.randomFoliage.Length; i++)
        {
            // Flower testing
            NoiseLayerSettings.Foliage foliageSettings = noiseLayerSettings.randomFoliage[i];
            if (!foliageSettings.enabled)
            {
                continue;
            }

            Mesh foliageMesh = createMesh.DropMeshVertices(groundMesh, foliageSettings.noiseLayer, foliageSettings.keepRangeNoise, foliageSettings.keepRangeRandomNoise, foliageSettings.keepRangeRandom, Vector3.up * 0.1f);
            CurveCreator.AddCurveTexture(ref foliageSettings.material, foliageSettings.curve);
            GameObject foliageGameObject = createMesh.CreateRandomFoliage(foliageMesh, foliageSettings.material, foliageSettings.name);
            foliageGameObject.transform.parent = meshGameObject.transform;
            foliageGameObject.transform.position += Vector3.up * 0.1f;
        }

        noiseTextures = createMesh.GetNoiseTextures();
        spawnPrefabs.Spawn(noiseLayerSettings.spawnPrefabs, noiseLayerSettings.objectDensity, noiseLayerSettings.unitSize * noiseLayerSettings.resolution, offset, chunkLoadSpeed);
    }

    private void Start()
    {
        JoinMeshes joinMeshes = rocksGameObject.AddComponent<JoinMeshes>();
        joinMeshes.SetCollider();
    }

    private void Update()
    {
        if ((transform.position - playerTransform.position).sqrMagnitude > chunkUnloadDistanceSquared)
        {
            gameObject.SetActive(false);
        }
    }
    */
}
