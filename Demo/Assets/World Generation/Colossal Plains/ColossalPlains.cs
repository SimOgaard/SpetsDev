using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ColossalPlains : MonoBehaviour, WorldGenerationManager.WorldGeneration
{
    private GameObject mesh_game_object;
    private GameObject land_marks_game_object;
    private GameObject rocks_game_object;
    private GameObject trees_game_object;

    private NoiseLayerSettings noise_layer_settings;
    private CreateMesh create_mesh;
    private Texture2D[] noise_textures;
    private SpawnPrefabs spawn_prefabs;

    public void DestroyAll()
    {
        DestroyImmediate(mesh_game_object);
        DestroyImmediate(land_marks_game_object);
        DestroyImmediate(rocks_game_object);
        DestroyImmediate(trees_game_object);
        DestroyImmediate(gameObject);
    }

    public NoiseLayerSettings GetNoiseSettings()
    {
        return noise_layer_settings;
    }

    public void LoadInNoiseSettings()
    {
        noise_layer_settings = (NoiseLayerSettings)AssetDatabase.LoadAssetAtPath("Assets/World Generation/Colossal Plains/ColossalPlains.asset", typeof(NoiseLayerSettings));
    }

    public Texture2D[] GetNoiseTextures()
    {
        return noise_textures;
    }

    public void UpdateWorld()
    {
        Mesh ground_mesh = create_mesh.CreateMeshByNoise(GetNoiseSettings());
        create_mesh.UpdateGrass(ground_mesh);
        noise_textures = create_mesh.GetNoiseTextures();
        //spawn_prefabs.Spawn(noise_layer_settings.spawn_prefabs, noise_layer_settings.object_density, noise_layer_settings.unit_size * noise_layer_settings.resolution);
        CurveCreator.AddCurveTexture(ref noise_layer_settings.material_leaf, noise_layer_settings.curve_leaf);
        CurveCreator.AddCurveTexture(ref noise_layer_settings.material_wood, noise_layer_settings.curve_wood);
    }

    public void Init()
    {
        gameObject.layer = Layer.game_world;
        gameObject.isStatic = true;

        LoadInNoiseSettings();
        CurveCreator.AddCurveTexture(ref noise_layer_settings.material_leaf, noise_layer_settings.curve_leaf);
        CurveCreator.AddCurveTexture(ref noise_layer_settings.material_wood, noise_layer_settings.curve_wood);
        WorldGenerationManager.InitNewChild(out mesh_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.ground_mesh);
        mesh_game_object.tag = "Flammable";
        create_mesh = mesh_game_object.AddComponent<CreateMesh>();
        spawn_prefabs = gameObject.AddComponent<SpawnPrefabs>();

        WorldGenerationManager.InitNewChild(out land_marks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.land_marks);
        WorldGenerationManager.InitNewChild(out rocks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.rocks);
        WorldGenerationManager.InitNewChild(out trees_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.trees);

        Mesh ground_mesh = create_mesh.CreateMeshByNoise(GetNoiseSettings());
        create_mesh.CreateGrass(ground_mesh);

        for (int i = 0; i < noise_layer_settings.random_foliage.Length; i++)
        {
            // Flower testing
            NoiseLayerSettings.Foliage foliage_settings = noise_layer_settings.random_foliage[i];
            Mesh foliage_mesh = create_mesh.DropMeshVertices(ground_mesh, foliage_settings.noise_layer, foliage_settings.keep_range_noise, foliage_settings.keep_range_random_noise, foliage_settings.keep_range_random, Vector3.zero);
            CurveCreator.AddCurveTexture(ref foliage_settings.material, foliage_settings.curve);
            GameObject foliage_game_object = create_mesh.CreateRandomFoliage(foliage_mesh, foliage_settings.material, foliage_settings.name);
            foliage_game_object.transform.parent = mesh_game_object.transform;
            foliage_game_object.transform.position += Vector3.up * 0.1f;
        }

        Water water = new GameObject().AddComponent<Water>();
        water.Init(noise_layer_settings.water.material, 1000, 1000, noise_layer_settings.water.level, mesh_game_object.transform);

        noise_textures = create_mesh.GetNoiseTextures();
        spawn_prefabs.Spawn(noise_layer_settings.spawn_prefabs, noise_layer_settings.object_density, noise_layer_settings.unit_size * noise_layer_settings.resolution);
    }

    private void Start()
    {
        JoinMeshes join_meshes = rocks_game_object.AddComponent<JoinMeshes>();
        join_meshes.SetCollider();
    }

    
    private void Update()
    {
        float water_level_wave = noise_layer_settings.water.bobing_amplitude * Mathf.Sin(Time.time * noise_layer_settings.water.bobing_frequency);
        Water.water_level = noise_layer_settings.water.level + water_level_wave;
    }
}
