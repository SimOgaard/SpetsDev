using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColossalPlains : MonoBehaviour, WorldGenerationManager.WorldGeneration
{
    private GameObject land_marks_game_object;
    private GameObject rocks_game_object;
    private GameObject trees_game_object;

    public GroundNoiseSettings ground_noise_settings;

    //public  test_image

    [HideInInspector]
    public bool ground_noise_settings_foldout = true;
    public void CreateGroundMesh()
    {
        Debug.Log("created mesh");

        // creates mesh
        Mesh ground_mesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        // sets noise
        FastNoiseLite noise = new FastNoiseLite(ground_noise_settings.general_noise.seed);
        noise.SetFrequency(ground_noise_settings.general_noise.frequency);
        noise.SetNoiseType(ground_noise_settings.general_noise.noise_type);

        noise.SetFractalType(ground_noise_settings.fractal.fractal_type);
        noise.SetFractalOctaves(ground_noise_settings.fractal.octaves);
        noise.SetFractalLacunarity(ground_noise_settings.fractal.lacunarity);
        noise.SetFractalGain(ground_noise_settings.fractal.gain);
        noise.SetFractalWeightedStrength(ground_noise_settings.fractal.weighted_strength);
        noise.SetFractalPingPongStrength(ground_noise_settings.fractal.ping_pong_strength);

        noise.SetCellularDistanceFunction(ground_noise_settings.cellular.cellular_distance_function);
        noise.SetCellularReturnType(ground_noise_settings.cellular.cellular_return_type);
        noise.SetCellularJitter(ground_noise_settings.cellular.jitter);

        // domain warp
        FastNoiseLite warp = new FastNoiseLite(ground_noise_settings.general_noise.seed);
        warp.SetDomainWarpType(ground_noise_settings.domain_warp.domain_warp_type);
        warp.SetDomainWarpAmp(ground_noise_settings.domain_warp.amplitude);
        warp.SetFrequency(ground_noise_settings.domain_warp.frequency);

        warp.SetFractalType(ground_noise_settings.domain_warp_fractal.fractal_type);
        warp.SetFractalOctaves(ground_noise_settings.domain_warp_fractal.octaves);
        warp.SetFractalLacunarity(ground_noise_settings.domain_warp_fractal.lacunarity);
        warp.SetFractalGain(ground_noise_settings.domain_warp_fractal.gain);

        // adds vertices and triangles to mesh
        Vector3 vertex = Vector3.zero;

        int ground_resolution_x = 128;
        int ground_resolution_z = 128;

        int vertices_length = ground_resolution_x * ground_resolution_z;
        int triangles_length = (ground_resolution_x - 1) * (ground_resolution_z - 1) * 6;
        Vector3[] vertices = new Vector3[vertices_length];
        int[] triangles = new int[triangles_length];
        int vertices_index = 0;
        int triangles_index = 0;

        //transform.position = new Vector3(-(ground_resolution.x - 1f) * triangle_size.x * 0.5f, transform.position.y, -(ground_resolution.y - 1f) * triangle_size.y * 0.5f);

        float triangle_size_z = 1f;
        float triangle_size_x = 1f;

        for (int z = 0; z < ground_resolution_z; z++)
        {
            vertex.z = triangle_size_z * z;

            for (int x = 0; x < ground_resolution_x; x++)
            {
                vertex.x = triangle_size_x * x;

                // use noise
                float x_copy = x;
                float z_copy = z;
                warp.DomainWarp(ref x_copy, ref z_copy);

                vertex.y = noise.GetNoise(x_copy, z_copy) * ground_noise_settings.general.amplitude;

                vertices_index = z * ground_resolution_z + x;
                vertices[vertices_index] = vertex;

                if (x != ground_resolution_x - 1 && z != 0)
                {
                    triangles[triangles_index] = vertices_index;
                    triangles[triangles_index + 1] = vertices_index - ground_resolution_x + 1;
                    triangles[triangles_index + 2] = vertices_index - ground_resolution_x;

                    triangles[triangles_index + 3] = vertices_index;
                    triangles[triangles_index + 4] = vertices_index + 1;
                    triangles[triangles_index + 5] = vertices_index - ground_resolution_x + 1;

                    triangles_index += 6;
                }
            }
        }

        ground_mesh.vertices = vertices;
        ground_mesh.triangles = triangles;

        ground_mesh.RecalculateNormals();
        ground_mesh.RecalculateTangents();
        ground_mesh.RecalculateBounds();
        //projections = CreateProjection(ColossalPlains);

        gameObject.GetComponent<MeshFilter>().mesh = ground_mesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = ground_mesh;
    }

    private void Start()
    {
        // ground is on this.gameObject.
        land_marks_game_object = new GameObject("land_marks");
        rocks_game_object = new GameObject("rocks");
        trees_game_object = new GameObject("trees");

        MeshRenderer mesh_renderer = gameObject.AddComponent<MeshRenderer>();
        mesh_renderer.material = new Material(Shader.Find("Standard"));
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshCollider>();

        // loads in ground  asset 
        ground_noise_settings = Resources.Load<GroundNoiseSettings>("Settings/Colossal Plains Ground");

        // creates ground mesh.
        CreateGroundMesh();
    }

    private Projections projections;
    private interface Projections
    {

    }
    private static Projections CreateProjection()
    {

        return null;
    }
}
