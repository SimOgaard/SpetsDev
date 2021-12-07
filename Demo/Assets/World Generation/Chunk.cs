using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class Chunk : MonoBehaviour
{
    public bool is_loading;
    public bool is_loaded;
    private float chunk_disable_distance;
    private Transform player_transform;
    private Enemies enemies;

    public void Initialize(float chunk_disable_distance, Transform player_transform)
    {
        is_loading = false;
        is_loaded = false;
        gameObject.layer = Layer.game_world;
        this.chunk_disable_distance = chunk_disable_distance;
        this.player_transform = player_transform;
    }

    public IEnumerator LoadChunk(NoiseLayerSettings noise_layer_settings, NativeArray<Noise.NoiseLayer> noise_layers_native_array, WorldGenerationManager.ChunkDetails chunk_details/*, int[] triangles*/)
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        // initilize variables
        is_loading = true;

        // create ground
        GameObject ground_game_object = new GameObject("Ground");
        ground_game_object.transform.parent = transform;
        ground_game_object.transform.localPosition = Vector3.zero;
        ground_game_object.layer = Layer.game_world;
        GroundMesh ground_mesh = ground_game_object.AddComponent<GroundMesh>();
        yield return ground_mesh.CreateGround(wait, chunk_details.unit_size, chunk_details.resolution, noise_layers_native_array, noise_layer_settings.material_static.material, noise_layer_settings.random_foliage);

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
        yield return StartCoroutine(spawn_prefabs.Spawn(wait, noise_layer_settings.spawn_prefabs, noise_layer_settings.object_density, chunk_details.offset));

        PlaceInWorld.SetRecursiveToGameWorld(gameObject);

        ground_game_object.layer = Layer.game_world_static;
        is_loading = false;
        is_loaded = true;
        WorldGenerationManager.chunks_in_loading.Remove(this);
    }
   
    public float DistToPlayer()
    {
        return (transform.position - player_transform.position).magnitude;
    }

    private void Update()
    {
        if (DistToPlayer() > chunk_disable_distance / PixelPerfectCameraRotation.zoom && is_loaded)
        {
            enemies.MoveParrent();
            gameObject.SetActive(false);
        }
    }
}
