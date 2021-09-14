using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CreateMesh : MonoBehaviour
{
    private MeshCollider mesh_collider;

    private GameObject grass_game_object;
    private MeshFilter grass_mesh_filter;
    private MeshRenderer grass_mesh_renderer;
    private Material grass_material;
    private AnimationCurve grass_curve;

    private GameObject water_game_object;
    private MeshFilter water_mesh_filter;
    private MeshRenderer water_mesh_renderer;
    private Material water_material;

    private Noise[] noise;

    public Mesh CreateMeshByNoise(NoiseLayerSettings noise_layer_settings)
    {
        grass_material = noise_layer_settings.material_grass;
        grass_curve = noise_layer_settings.light_curve_grass;
        water_material = noise_layer_settings.material_water;

        Vector2 unit_size = noise_layer_settings.unit_size;
        Vector2Int resolution = noise_layer_settings.resolution;

        Mesh mesh = new Mesh();

        resolution.x = Mathf.Max(resolution.x + 1, 2);
        resolution.y = Mathf.Max(resolution.y + 1, 2);

        unit_size.x = Mathf.Max(unit_size.x, 0.01f);
        unit_size.y = Mathf.Max(unit_size.y, 0.01f);

        int vertices_length = resolution.x * resolution.y;
        int triangles_length = (resolution.x - 1) * (resolution.y - 1) * 2 * 3;

        Vector3[] vertices = new Vector3[vertices_length];
        int[] triangles = new int[triangles_length];

        Vector3 reused_point = Vector3.zero;
        int vertices_index = 0;
        int triangles_index = 0;
        for (int z = 0; z < resolution.y; z++)
        {
            reused_point.z = unit_size.y * z - (resolution.y - 1) * unit_size.y * 0.5f;

            for (int x = 0; x < resolution.x; x++)
            {
                vertices_index = z * resolution.x + x;
                reused_point.x = unit_size.x * x - (resolution.x - 1) * unit_size.x * 0.5f;

                vertices[vertices_index] = reused_point;

                if (x != resolution.x - 1 && z != 0)
                {
                    triangles[triangles_index] = vertices_index;
                    triangles[triangles_index + 1] = vertices_index - resolution.x + 1;
                    triangles[triangles_index + 2] = vertices_index - resolution.x;

                    triangles[triangles_index + 3] = vertices_index;
                    triangles[triangles_index + 4] = vertices_index + 1;
                    triangles[triangles_index + 5] = vertices_index - resolution.x + 1;

                    triangles_index += 6;
                }
            }
        }

        int noise_length = noise_layer_settings.terrain_noise_layers.Length;
        noise = new Noise[noise_length];
        for (int i = 0; i < noise_length; i++)
        {
            noise[i] = new Noise(noise_layer_settings.terrain_noise_layers[i]);
        }

        for (int q = 0; q < noise_length; q++)
        {
            if (noise[q].enabled)
            {
                for (int i = 0; i < vertices_length; i++)
                {
                    // CAN GIVE ERRORS WITH WRONG VALUES, TRY EXCEPT OR MAX/MIN/RANGE VALUE
                    vertices[i].y += noise[q].GetNoiseValue(vertices[i].x, vertices[i].z);
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }

    public Mesh DropMeshVertices(Mesh reference_mesh, NoiseLayerSettings.NoiseLayer noise_layer, Vector2 keep_range_noise, float keep_range_random_noise, float keep_range_random)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Noise noise_class = new Noise(noise_layer);

        Vector3[] vertices_copy = reference_mesh.vertices;
        int[] triangles_copy = reference_mesh.triangles;
        for (int triangle_index = 0; triangle_index < triangles_copy.Length; triangle_index += 3)
        {
            Vector3 vertice_1 = vertices_copy[triangles_copy[triangle_index]];
            Vector3 vertice_2 = vertices_copy[triangles_copy[triangle_index + 1]];
            Vector3 vertice_3 = vertices_copy[triangles_copy[triangle_index + 2]];

            float x = (vertice_1.x + vertice_2.x + vertice_3.x) / 3;
            float z = (vertice_1.z + vertice_2.z + vertice_3.z) / 3;

            float noise_value = noise_class.GetNoiseValue(x, z);
            if (noise_value > keep_range_noise.x && noise_value < keep_range_noise.y && Random.value <= keep_range_random_noise || Random.value <= keep_range_random)
            {
                int index = vertices.Count;

                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(index + 2);

                vertices.Add(vertice_1);
                vertices.Add(vertice_2);
                vertices.Add(vertice_3);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }

    public static Mesh QuadMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, 0.5f)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        mesh.triangles = tris;

        return mesh;
    }

    public Texture2D[] GetNoiseTextures()
    {
        Texture2D[] noise_textures = new Texture2D[noise.Length];

        for (int i = 0; i < noise_textures.Length; i++)
        {
            noise_textures[i] = new Texture2D(224, 224);
            for (int x = 0; x < 224; x++)
            {
                for (int y = 0; y < 224; y++)
                {
                    float noise_value = noise[i].GetNoiseValueColor(x, y);

                    Color color = new Color(noise_value, noise_value, noise_value);
                    noise_textures[i].SetPixel(x, y, color);
                }
            }
            noise_textures[i].Apply();
        }

        return noise_textures;
    }

    public class Noise
    {
        private FastNoiseLite warp;
        private FastNoiseLite noise;

        public bool enabled;

        private float amplitude;
        private Vector2 offsett;

        private float min_value;
        private float smoothing_min;
        private float max_value;
        private float smoothing_max;

        private float SmoothMin(float a)
        {
            float h = Mathf.Max(0f, Mathf.Min(1f, (max_value - a + smoothing_max) / (2f * smoothing_max)));
            return a * h + max_value * (1f - h) - smoothing_max * h * (1f - h);
        }

        private float SmoothMax(float a)
        {
            float h = Mathf.Max(0f, Mathf.Min(1f, (min_value - a + smoothing_min) / (2f * smoothing_min)));
            return a * h + min_value * (1f - h) - smoothing_min * h * (1f - h);
        }

        public Noise(NoiseLayerSettings.NoiseLayer noise_layer)
        {
            // sets noise
            noise = new FastNoiseLite(noise_layer.general_noise.seed);
            noise.SetFrequency(noise_layer.general_noise.frequency);
            noise.SetNoiseType(noise_layer.general_noise.noise_type);

            noise.SetFractalType(noise_layer.fractal.fractal_type);
            noise.SetFractalOctaves(noise_layer.fractal.octaves);
            noise.SetFractalLacunarity(noise_layer.fractal.lacunarity);
            noise.SetFractalGain(noise_layer.fractal.gain);
            noise.SetFractalWeightedStrength(noise_layer.fractal.weighted_strength);
            noise.SetFractalPingPongStrength(noise_layer.fractal.ping_pong_strength);

            noise.SetCellularDistanceFunction(noise_layer.cellular.cellular_distance_function);
            noise.SetCellularReturnType(noise_layer.cellular.cellular_return_type);
            noise.SetCellularJitter(noise_layer.cellular.jitter);

            // domain warp
            warp = new FastNoiseLite(noise_layer.general_noise.seed);
            warp.SetDomainWarpType(noise_layer.domain_warp.domain_warp_type);
            warp.SetDomainWarpAmp(noise_layer.domain_warp.amplitude);
            warp.SetFrequency(noise_layer.domain_warp.frequency);

            warp.SetFractalType(noise_layer.domain_warp_fractal.fractal_type);
            warp.SetFractalOctaves(noise_layer.domain_warp_fractal.octaves);
            warp.SetFractalLacunarity(noise_layer.domain_warp_fractal.lacunarity);
            warp.SetFractalGain(noise_layer.domain_warp_fractal.gain);

            enabled = noise_layer.enabled;

            amplitude = noise_layer.general.amplitude;
            offsett = noise_layer.general.offsett;

            min_value = noise_layer.general.min_value;
            smoothing_max = Mathf.Max(0f, noise_layer.general.smoothing_max);
            max_value = noise_layer.general.max_value;
            smoothing_min = Mathf.Min(0f, -noise_layer.general.smoothing_min);
        }

        public float GetNoiseValueColor(float x, float z)
        {
            x += offsett.x;
            z += offsett.y;
            warp.DomainWarp(ref x, ref z);
            float noise_value = noise.GetNoise(x, z);
            noise_value = (noise_value + 1f) * 0.5f;
            noise_value = SmoothMin(noise_value);
            noise_value = SmoothMax(noise_value);
            return noise_value;
        }

        public float GetNoiseValue(float x, float z)
        {
            return GetNoiseValueColor(x, z) * amplitude;
        }
    }

    public void CreateGrass(Mesh mesh)
    {
        if (grass_material == null)
        {
            return;
        }

        gameObject.layer = Layer.game_world;
        gameObject.isStatic = true;
        grass_mesh_filter = gameObject.AddComponent<MeshFilter>();
        grass_mesh_renderer = gameObject.AddComponent<MeshRenderer>();
        mesh_collider = gameObject.AddComponent<MeshCollider>();
        mesh_collider.sharedMesh = mesh;

        grass_mesh_filter.mesh = mesh;
        CurveCreator.AddCurveTexture(ref grass_material, grass_curve);
        grass_mesh_renderer.material = grass_material;
    }

    public void CreateGrass(Mesh mesh, float y_cut_off)
    {
        // Not implemented
        if(grass_material == null)
        {
            return;
        }

        grass_game_object = new GameObject("grass");
        grass_game_object.layer = Layer.game_world;
        grass_game_object.isStatic = true;
        grass_game_object.transform.parent = transform;
        grass_mesh_filter = grass_game_object.AddComponent<MeshFilter>();
        grass_mesh_renderer = grass_game_object.AddComponent<MeshRenderer>();

        List<int> triangles = new List<int>();

        Vector3[] points = mesh.vertices;
        int[] tri = mesh.triangles;
        for (int i = 0; i < tri.Length; i+=3)
        {
            Vector3 point1 = points[tri[i]];
            Vector3 point2 = points[tri[i + 1]];
            Vector3 point3 = points[tri[i + 2]];

            float points_sum_y = point1.y + point2.y + point3.y;
            float points_mid_y = points_sum_y / 3f;

            if (points_mid_y > y_cut_off)
            {
                triangles.Add(tri[i]);
                triangles.Add(tri[i + 1]);
                triangles.Add(tri[i + 2]);
            }
        }

        Mesh copy_mesh = new Mesh();

        copy_mesh.vertices = points;
        copy_mesh.triangles = triangles.ToArray();
        copy_mesh.Optimize();
        grass_mesh_filter.mesh = copy_mesh;
        CurveCreator.AddCurveTexture(ref grass_material, grass_curve);
        grass_mesh_renderer.material = grass_material;
    }

    public void UpdateGrass(Mesh mesh)
    {
        if (grass_material == null)
        {
            return;
        }

        grass_mesh_filter.mesh = mesh;
        mesh_collider.sharedMesh = mesh;
        CurveCreator.AddCurveTexture(ref grass_material, grass_curve);
        grass_mesh_renderer.material = grass_material;
    }

    public GameObject CreateRandomFoliage(Mesh mesh, Material material, string name)
    {
        GameObject foliage_game_object = new GameObject(name);

        foliage_game_object.AddComponent<MeshFilter>().mesh = mesh;
        foliage_game_object.AddComponent<MeshRenderer>().material = material;

        return foliage_game_object;
    }
}
