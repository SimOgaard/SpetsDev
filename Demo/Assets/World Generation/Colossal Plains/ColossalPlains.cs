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
        create_mesh.UpdateGround(ground_mesh);
        create_mesh.UpdateGrass(ground_mesh);
        noise_textures = create_mesh.GetNoiseTextures();
        spawn_prefabs.Spawn(noise_layer_settings.spawn_prefabs);
    }

    private void Awake()
    {
        gameObject.layer = 12;
        gameObject.isStatic = true;

        LoadInNoiseSettings();
        WorldGenerationManager.InitNewChild(out mesh_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.ground_mesh);
        mesh_game_object.tag = "Flammable";
        create_mesh = mesh_game_object.AddComponent<CreateMesh>();
        spawn_prefabs = gameObject.AddComponent<SpawnPrefabs>();

        WorldGenerationManager.InitNewChild(out land_marks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.land_marks);
        WorldGenerationManager.InitNewChild(out rocks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.rocks);
        WorldGenerationManager.InitNewChild(out trees_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.trees);

        Mesh ground_mesh = create_mesh.CreateMeshByNoise(GetNoiseSettings());
        create_mesh.CreateGround(ground_mesh, Quaternion.Euler(0f, 45f, 0f));
        create_mesh.CreateGrass(ground_mesh);

        Water water = new GameObject().AddComponent<Water>();
        water.Init(noise_layer_settings.water_material, 1000, 1000, 7, mesh_game_object.transform);

        noise_textures = create_mesh.GetNoiseTextures();
        spawn_prefabs.Spawn(noise_layer_settings.spawn_prefabs);
    }

    private void Start()
    {
        JoinMeshes join_meshes = rocks_game_object.AddComponent<JoinMeshes>();
        join_meshes.SetMaterial(new Material(Shader.Find("Custom/Stone Shader")));
    }

    /*
    private void Update()
    {
        Water.water_level += Time.deltaTime;
    }
    */
}
