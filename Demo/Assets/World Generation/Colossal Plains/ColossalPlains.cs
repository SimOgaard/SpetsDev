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

    private float chunk_unload_distance_squared;
    private Transform player_transform;

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

    public void Init(Vector2 unit_size, Vector2Int resolution, Vector2 offset, float chunk_unload_distance_squared, Transform player_transform, float chunk_load_speed)
    {
        gameObject.layer = Layer.game_world;
        gameObject.isStatic = true;

        LoadInNoiseSettings();
        noise_layer_settings.unit_size = unit_size;
        noise_layer_settings.resolution = resolution;
        noise_layer_settings.offsett = offset;
        this.chunk_unload_distance_squared = chunk_unload_distance_squared;
        this.player_transform = player_transform;

        CurveCreator.AddCurveTexture(ref noise_layer_settings.material_leaf, noise_layer_settings.curve_leaf);
        CurveCreator.AddCurveTexture(ref noise_layer_settings.material_wood, noise_layer_settings.curve_wood);
        WorldGenerationManager.InitNewChild(out mesh_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.ground_mesh);
        mesh_game_object.tag = "Flammable";
        create_mesh = mesh_game_object.AddComponent<CreateMesh>();
        spawn_prefabs = gameObject.AddComponent<SpawnPrefabs>();

        WorldGenerationManager.InitNewChild(out land_marks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.land_marks);
        WorldGenerationManager.InitNewChild(out rocks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.rocks);
        WorldGenerationManager.InitNewChild(out trees_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.trees);
        GameObject enemies_game_object;
        WorldGenerationManager.InitNewChild(out enemies_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.enemies, typeof(Enemies));
        GameObject interact_game_object;
        WorldGenerationManager.InitNewChild(out interact_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.interact, typeof(Interact));

        Mesh ground_mesh = create_mesh.CreateMeshByNoise(GetNoiseSettings());
        create_mesh.CreateGrass(ground_mesh);

        for (int i = 0; i < noise_layer_settings.random_foliage.Length; i++)
        {
            // Flower testing
            NoiseLayerSettings.Foliage foliage_settings = noise_layer_settings.random_foliage[i];
            if (!foliage_settings.enabled)
            {
                continue;
            }

            Mesh foliage_mesh = create_mesh.DropMeshVertices(ground_mesh, foliage_settings.noise_layer, foliage_settings.keep_range_noise, foliage_settings.keep_range_random_noise, foliage_settings.keep_range_random, Vector3.up * 0.1f);
            CurveCreator.AddCurveTexture(ref foliage_settings.material, foliage_settings.curve);
            GameObject foliage_game_object = create_mesh.CreateRandomFoliage(foliage_mesh, foliage_settings.material, foliage_settings.name);
            foliage_game_object.transform.parent = mesh_game_object.transform;
            foliage_game_object.transform.position += Vector3.up * 0.1f;
        }

        noise_textures = create_mesh.GetNoiseTextures();
        spawn_prefabs.Spawn(noise_layer_settings.spawn_prefabs, noise_layer_settings.object_density, noise_layer_settings.unit_size * noise_layer_settings.resolution, offset, chunk_load_speed);
    }

    private void Start()
    {
        JoinMeshes join_meshes = rocks_game_object.AddComponent<JoinMeshes>();
        join_meshes.SetCollider();
    }

    private void Update()
    {
        if ((transform.position - player_transform.position).sqrMagnitude > chunk_unload_distance_squared)
        {
            gameObject.SetActive(false);
        }
    }
}
