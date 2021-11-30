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

    private JobHandle jobHandle;
    private MeshData meshData;
    private bool is_deallocated = false;
    private void OnDestroy()
    {
        if (!is_deallocated)
        {
            if (!jobHandle.IsCompleted)
            {
                jobHandle.Complete();
            }

            meshData.Dispose();
        }
    }


    // move all mesh creating to another thread that is passed in (since createing new thread is expensive)
    // while (thread is not complete)
    // {
    //      yield return wait;
    // }
    public IEnumerator LoadChunk(NoiseLayerSettings noise_layer_settings, NativeArray<Noise.NoiseLayer> noise_layers_native_array, WorldGenerationManager.ChunkDetails chunk_details/*, int[] triangles*/)
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        // initilize variables
        is_loading = true;

        // initilizes ground
        /*
        CoroutineWithData create_mesh_coroutine = new CoroutineWithData(this, CreateMesh.CreateMeshByNoise(wait, noise_layers, chunk_details.unit_size, chunk_details.resolution, transform.localPosition));
        yield return create_mesh_coroutine.coroutine;
        Mesh ground_mesh = create_mesh_coroutine.result as Mesh;
        Transform ground_transform = CreateGround(ground_mesh, noise_layer_settings.material_grass, noise_layer_settings.curve_grass);
        */

        int quadCount = chunk_details.resolution.x * chunk_details.resolution.y;
        int vertexCount = quadCount * 4;
        int triangleCount = quadCount * 6;

        meshData = new MeshData();
        meshData.vertices = new NativeArray<Vector3>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.triangles = new NativeArray<int>(triangleCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.normals = new NativeArray<Vector3>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        meshData.uv = new NativeArray<Vector2>(vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        CreateMesh.CreatePlaneJob job = new CreateMesh.CreatePlaneJob
        {
            meshData = meshData,
            planeSize = chunk_details.resolution,
            quadSize = chunk_details.unit_size,
            quadCount = quadCount,
            vertexCount = vertexCount,
            triangleCount = triangleCount
        };

        jobHandle = job.Schedule();
        while (!jobHandle.IsCompleted)
        {
            yield return wait;
        }
        jobHandle.Complete();

        
        CreateMesh.CreateMeshByNoiseJob noiseJob = new CreateMesh.CreateMeshByNoiseJob
        {
            vertices = meshData.vertices,
            offset = transform.position,
            noise_layers = noise_layers_native_array
        };

        jobHandle = noiseJob.Schedule();
        while (!jobHandle.IsCompleted)
        {
            yield return wait;
        }
        jobHandle.Complete();
        

        CreateMesh.NormalizeJob normalizeJob = new CreateMesh.NormalizeJob
        {
            quadCount = quadCount,
            meshData = meshData
        };

        jobHandle = normalizeJob.Schedule(jobHandle);
        while (!jobHandle.IsCompleted)
        {
            yield return wait;
        }
        jobHandle.Complete();

        Mesh ground_mesh = new Mesh();
        ground_mesh.vertices = meshData.vertices.ToArray();
        ground_mesh.triangles = meshData.triangles.ToArray();
        ground_mesh.normals = meshData.normals.ToArray();
        ground_mesh.uv = meshData.uv.ToArray();

        is_deallocated = true;

        Transform ground_transform = CreateGround(ground_mesh, noise_layer_settings.material_grass, noise_layer_settings.curve_grass);

        /*
        Task mesh_creator_task = new Task(CreateMesh.CreateMeshByNoiseOnThread);
        mesh_creator_thread.Start();

        while (mesh_creator_thread.IsAlive)
        {
            yield return wait;
        }
        */

        MeshDataList mesh_data_foliage = new MeshDataList();
        mesh_data_foliage.vertices = new NativeList<Vector3>(Allocator.Persistent);
        mesh_data_foliage.triangles = new NativeList<int>(Allocator.Persistent);
        mesh_data_foliage.normals = new NativeList<Vector3>(Allocator.Persistent);
        mesh_data_foliage.uv = new NativeList<Vector2>(Allocator.Persistent);

        // initilizes flowers
        for (int i = 0; i < noise_layer_settings.random_foliage.Length; i++)
        {
            // Flower testing
            NoiseLayerSettings.Foliage foliage_settings = noise_layer_settings.random_foliage[i];
            if (!foliage_settings.enabled)
            {
                continue;
            }

            yield return wait;

            CreateMesh.DropMeshVerticesJob dropVerticesJob = new CreateMesh.DropMeshVerticesJob
            {
                mesh_data_list = mesh_data_foliage,
                original_mesh_data = meshData,
                noise_layer = new Noise.NoiseLayer(foliage_settings.noise_layer),
                keep_range_noise = foliage_settings.keep_range_noise,
                keep_range_random_noise = foliage_settings.keep_range_random_noise,
                keep_range_random = foliage_settings.keep_range_random,
                offset = Vector3.up * 0.1f,
                transform_offset = transform.localPosition
            };

            jobHandle = dropVerticesJob.Schedule();
            while (!jobHandle.IsCompleted)
            {
                yield return wait;
            }
            jobHandle.Complete();

            Mesh foliage_mesh = new Mesh();
            foliage_mesh.vertices = mesh_data_foliage.vertices.ToArray();
            foliage_mesh.triangles = mesh_data_foliage.triangles.ToArray();
            foliage_mesh.normals = mesh_data_foliage.normals.ToArray();
            foliage_mesh.uv = mesh_data_foliage.uv.ToArray();

            mesh_data_foliage.Dispose();

            yield return wait;

            /*
            CoroutineWithData create_foliage_mesh_coroutine = new CoroutineWithData(this, CreateMesh.DropMeshVertices(wait, ground_mesh, foliage_settings.noise_layer, foliage_settings.keep_range_noise, foliage_settings.keep_range_random_noise, foliage_settings.keep_range_random, Vector3.up * 0.1f, transform.localPosition));
            yield return create_foliage_mesh_coroutine.coroutine;
            Mesh foliage_mesh = create_foliage_mesh_coroutine.result as Mesh;
            */
            CurveCreator.AddCurveTexture(ref foliage_settings.material, foliage_settings.curve);
            GameObject foliage_game_object = CreateRandomFoliage(foliage_mesh, foliage_settings.material, foliage_settings.name, ground_transform);
        }
        meshData.Dispose();

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

        //JoinMeshes join_meshes = rocks_game_object.AddComponent<JoinMeshes>();
        //join_meshes.SetCollider();

        ground_game_object.layer = Layer.game_world_static;
        is_loading = false;
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
