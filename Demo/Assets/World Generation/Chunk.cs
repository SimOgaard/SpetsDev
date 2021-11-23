using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Chunk : MonoBehaviour
{
    public bool is_loaded = false;
    private float chunk_disable_distance;
    private Transform player_transform;
    private Enemies enemies;

    public IEnumerator LoadChunk(NoiseLayerSettings noise_layer_settings, Noise.NoiseLayer[] noise_layers, WorldGenerationManager.ChunkDetails chunk_details, Transform player_transform, bool insta_load = false)
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        // initilize variables
        gameObject.layer = Layer.game_world;
        this.chunk_disable_distance = chunk_details.chunk_disable_distance;
        this.player_transform = player_transform;

        // initilizes ground
        CoroutineWithData create_mesh_coroutine = new CoroutineWithData(this, CreateMesh.CreateMeshByNoise(wait, noise_layers, chunk_details.unit_size, chunk_details.resolution, transform.localPosition));
        yield return create_mesh_coroutine.coroutine;
        Mesh ground_mesh = create_mesh_coroutine.result as Mesh;
        Transform ground_transform = CreateGround(ground_mesh, noise_layer_settings.material_grass, noise_layer_settings.curve_grass);

        // initilizes flowers
        for (int i = 0; i < noise_layer_settings.random_foliage.Length; i++)
        {
            // Flower testing
            NoiseLayerSettings.Foliage foliage_settings = noise_layer_settings.random_foliage[i];
            if (!foliage_settings.enabled)
            {
                continue;
            }

            CoroutineWithData create_foliage_mesh_coroutine = new CoroutineWithData(this, CreateMesh.DropMeshVertices(wait, ground_mesh, foliage_settings.noise_layer, foliage_settings.keep_range_noise, foliage_settings.keep_range_random_noise, foliage_settings.keep_range_random, Vector3.up * 0.1f, transform.localPosition));
            yield return create_foliage_mesh_coroutine.coroutine;
            Mesh foliage_mesh = create_foliage_mesh_coroutine.result as Mesh;
            CurveCreator.AddCurveTexture(ref foliage_settings.material, foliage_settings.curve);
            yield return wait;
            GameObject foliage_game_object = CreateRandomFoliage(foliage_mesh, foliage_settings.material, foliage_settings.name, ground_transform);
        }

        // initilizes all parrent objects of prefabs
        GameObject land_marks_game_object;
        WorldGenerationManager.InitNewChild(out land_marks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.land_marks);
        GameObject rocks_game_object;
        WorldGenerationManager.InitNewChild(out rocks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.rocks);
        GameObject trees_game_object;
        WorldGenerationManager.InitNewChild(out trees_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.trees);
        GameObject chests_game_object;
        WorldGenerationManager.InitNewChild(out chests_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.chests);
        GameObject enemies_game_object;
        WorldGenerationManager.InitNewChild(out enemies_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.enemies);
        enemies = enemies_game_object.AddComponent<Enemies>();

        // spawns prefabss
        SpawnPrefabs spawn_prefabs = gameObject.AddComponent<SpawnPrefabs>();
        yield return StartCoroutine(spawn_prefabs.Spawn(wait, noise_layer_settings.spawn_prefabs, noise_layer_settings.object_density, chunk_details.offset, chunk_details.chunk_load_speed));

        PlaceInWorld.SetRecursiveToGameWorld(gameObject);

        //JoinMeshes join_meshes = rocks_game_object.AddComponent<JoinMeshes>();
        //join_meshes.SetCollider();

        ground_game_object.layer = Layer.game_world_static;
        is_loaded = true;
        WorldGenerationManager.chunks_in_loading.Remove(this);
        //Debug.Log("done loading: " + transform.name);
    }
    public void ReloadChunk()
    {

    }

    private GameObject ground_game_object;
    private MeshFilter ground_mesh_filter;
    private MeshRenderer ground_mesh_renderer;
    private MeshCollider ground_mesh_collider;
    private Transform CreateGround(Mesh mesh, Material ground_material, NoiseLayerSettings.Curve ground_curve)
    {
        WorldGenerationManager.InitNewChild(out ground_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.ground_mesh);

        ground_game_object.transform.localPosition = Vector3.zero;

        ground_mesh_filter = ground_game_object.AddComponent<MeshFilter>();
        ground_mesh_renderer = ground_game_object.AddComponent<MeshRenderer>();
        ground_mesh_collider = ground_game_object.AddComponent<MeshCollider>();
        ground_mesh_collider.sharedMesh = mesh;

        ground_mesh_filter.mesh = mesh;
        CurveCreator.AddCurveTexture(ref ground_material, ground_curve);
        ground_mesh_renderer.material = ground_material;

        ground_game_object.tag = "Flammable";
        ground_game_object.layer = Layer.game_world;
        return ground_game_object.transform;
    }

    private void UpdateGrass(Mesh mesh, Material ground_material, NoiseLayerSettings.Curve ground_curve)
    {
        ground_mesh_filter.mesh = mesh;
        ground_mesh_collider.sharedMesh = mesh;
        CurveCreator.AddCurveTexture(ref ground_material, ground_curve);
        ground_mesh_renderer.material = ground_material;
    }

    private GameObject CreateRandomFoliage(Mesh mesh, Material material, string name, Transform parrent)
    {
        GameObject foliage_game_object = new GameObject(name);

        foliage_game_object.transform.parent = parrent;
        foliage_game_object.transform.localPosition = Vector3.up * 0.1f;
        foliage_game_object.layer = Layer.game_world;

        foliage_game_object.AddComponent<MeshFilter>().mesh = mesh;
        foliage_game_object.AddComponent<MeshRenderer>().material = material;

        return foliage_game_object;
    }

    private void Update()
    {
        if ((transform.position - player_transform.position).magnitude > chunk_disable_distance / PixelPerfectCameraRotation.zoom && is_loaded)
        {
            enemies.MoveParrent();
            gameObject.SetActive(false);
        }
    }
}
