using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

[ExecuteInEditMode]
public class Chunk : MonoBehaviour
{
    private float chunk_unload_distance_squared;
    private Transform player_transform;

    public async void InitChunk(NoiseLayerSettings noise_layer_settings, Noise.NoiseLayer[] noise_layers, WorldGenerationManager.ChunkDetails chunk_details, Transform player_transform)
    {
        Debug.Log(this == null);
        Debug.Log("start init chunk");
        // initilize variables
        gameObject.layer = Layer.game_world;
        gameObject.isStatic = true;
        this.chunk_unload_distance_squared = chunk_details.chunk_unload_distance_squared;
        this.player_transform = player_transform;

        // initilizes ground
        // Run in second thread
        Debug.Log(this == null);
        Vector3 mesh_position = transform.position;
        (Vector3[] vertices, int[] triangles) mesh_data = await Task.Run(() =>
        {
            return CreateMesh.CreateMeshByNoise(noise_layers, chunk_details.unit_size, chunk_details.resolution, mesh_position);
        });

        // run in main thread
        Debug.Log(this == null);
        Mesh ground_mesh = new Mesh() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt16 };
        ground_mesh.vertices = mesh_data.vertices;
        ground_mesh.triangles = mesh_data.triangles;
        ground_mesh.Optimize();
        ground_mesh.RecalculateNormals();
        ground_mesh.RecalculateTangents();
        ground_mesh.RecalculateBounds();
        Debug.Log(this == null);
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

            System.Random random = new System.Random();
            mesh_position = transform.position;
            (Vector3[] vertices, int[] triangles) mesh_data_flowers = await Task.Run(() =>
            {
                return CreateMesh.DropMeshVertices(mesh_data.vertices, mesh_data.triangles, random, foliage_settings.noise_layer, foliage_settings.keep_range_noise, foliage_settings.keep_range_random_noise, foliage_settings.keep_range_random, Vector3.up * 0.1f, mesh_position);
            });

            Mesh flower_mesh = new Mesh();
            flower_mesh.vertices = mesh_data_flowers.vertices;
            flower_mesh.triangles = mesh_data_flowers.triangles;
            flower_mesh.Optimize();
            flower_mesh.RecalculateNormals();
            flower_mesh.RecalculateTangents();
            flower_mesh.RecalculateBounds();

            CurveCreator.AddCurveTexture(ref foliage_settings.material, foliage_settings.curve);
            GameObject foliage_game_object = CreateRandomFoliage(flower_mesh, foliage_settings.material, foliage_settings.name, ground_transform);
        }

        // initilizes all parrent objects of prefabs
        GameObject land_marks_game_object;
        WorldGenerationManager.InitNewChild(out land_marks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.land_marks);
        GameObject rocks_game_object;
        WorldGenerationManager.InitNewChild(out rocks_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.rocks);
        GameObject trees_game_object;
        WorldGenerationManager.InitNewChild(out trees_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.trees);
        GameObject enemies_game_object;
        WorldGenerationManager.InitNewChild(out enemies_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.enemies, typeof(Enemies));
        GameObject interact_game_object;
        WorldGenerationManager.InitNewChild(out interact_game_object, transform, SpawnInstruction.PlacableGameObjectsParrent.interact, typeof(Interact));

        // spawns prefabss
        SpawnPrefabs spawn_prefabs = gameObject.AddComponent<SpawnPrefabs>();
        spawn_prefabs.Spawn(noise_layer_settings.spawn_prefabs, noise_layer_settings.object_density, chunk_details.unit_size * chunk_details.resolution, transform.position, chunk_details.chunk_load_speed);

        Debug.Log("done init chunk");
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
        Debug.Log(this == null);
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
        ground_game_object.isStatic = true;
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
        foliage_game_object.isStatic = true;

        foliage_game_object.AddComponent<MeshFilter>().mesh = mesh;
        foliage_game_object.AddComponent<MeshRenderer>().material = material;

        return foliage_game_object;
    }

    private void Update()
    {
        if ((transform.position - player_transform.position).sqrMagnitude > chunk_unload_distance_squared)
        {
            gameObject.SetActive(false);
        }
    }
}
