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
        CurveCreator.AddCurveTexture(ref noise_layer_settings.material_leaf, noise_layer_settings.light_curve_leaf);
        CurveCreator.AddCurveTexture(ref noise_layer_settings.material_wood, noise_layer_settings.light_curve_wood);
    }

    public void Init()
    {
        gameObject.layer = Layer.game_world;
        gameObject.isStatic = true;

        LoadInNoiseSettings();
        CurveCreator.AddCurveTexture(ref noise_layer_settings.material_leaf, noise_layer_settings.light_curve_leaf);
        CurveCreator.AddCurveTexture(ref noise_layer_settings.material_wood, noise_layer_settings.light_curve_wood);
        WorldGenerationManager.InitNewChild(out mesh_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.ground_mesh);
        mesh_game_object.tag = "Flammable";
        create_mesh = mesh_game_object.AddComponent<CreateMesh>();
        spawn_prefabs = gameObject.AddComponent<SpawnPrefabs>();

        WorldGenerationManager.InitNewChild(out land_marks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.land_marks);
        WorldGenerationManager.InitNewChild(out rocks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.rocks);
        WorldGenerationManager.InitNewChild(out trees_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.trees);

        Mesh ground_mesh = create_mesh.CreateMeshByNoise(GetNoiseSettings());
        create_mesh.CreateGrass(ground_mesh);

        // Flower testing
        Mesh flower_mesh = create_mesh.DropMeshToPoints(ground_mesh, noise_layer_settings.flower_noise_layer, noise_layer_settings.flower_keep_range);
        GameObject flower_game_object = new GameObject("flower_test");
        flower_game_object.transform.parent = mesh_game_object.transform;
        MeshFilter flower_mesh_filter = flower_game_object.AddComponent<MeshFilter>();
        MeshRenderer flower_mesh_renderer = flower_game_object.AddComponent<MeshRenderer>();
        flower_mesh_filter.mesh = flower_mesh;
        flower_mesh_renderer.material = noise_layer_settings.material_leaf;

        Water water = new GameObject().AddComponent<Water>();
        water.Init(noise_layer_settings.material_water, 1000, 1000, 7, mesh_game_object.transform);

        noise_textures = create_mesh.GetNoiseTextures();
        spawn_prefabs.Spawn(noise_layer_settings.spawn_prefabs, noise_layer_settings.object_density, noise_layer_settings.unit_size * noise_layer_settings.resolution);
    }

    private void Start()
    {
        JoinMeshes join_meshes = rocks_game_object.AddComponent<JoinMeshes>();
        join_meshes.SetCollider();
    }

    /*
    private void Update()
    {
        Water.water_level += Time.deltaTime;
    }
    */
}
