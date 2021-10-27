using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMesh : MonoBehaviour
{
    private MeshCollider mesh_collider;

    private GameObject grass_game_object;
    private MeshFilter grass_mesh_filter;
    private MeshRenderer grass_mesh_renderer;
    private Material grass_material;
    private NoiseLayerSettings.Curve grass_curve;

    public static Mesh CreateMeshByNoise(Noise.NoiseLayer[] noise_layers, Vector2 unit_size, Vector2Int resolution, Vector3 offset)
    {
        Mesh mesh = new Mesh() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt16 };

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

        int noise_length = noise_layers.Length;
        for (int z = 0; z < resolution.y; z++)
        {
            reused_point.z = unit_size.y * z - (resolution.y - 1) * unit_size.y * 0.5f;

            for (int x = 0; x < resolution.x; x++)
            {
                vertices_index = z * resolution.x + x;
                reused_point.x = unit_size.x * x - (resolution.x - 1) * unit_size.x * 0.5f;
                reused_point.y = 0f;

                float x_value = x + offset.x;
                float z_value = z + offset.z;
                for (int q = 0; q < noise_length; q++)
                {
                    if (noise_layers[q].enabled)
                    {
                        // CAN GIVE ERRORS WITH WRONG VALUES, TRY EXCEPT OR MAX/MIN/RANGE VALUE
                        reused_point.y += noise_layers[q].GetNoiseValue(x_value, z_value);
                    }
                }

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

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }

    public static Mesh DropMeshVertices(Mesh reference_mesh, NoiseLayerSettings.NoiseLayer settings_noise_layer, Vector2 keep_range_noise, float keep_range_random_noise, float keep_range_random, Vector3 offset)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Noise.NoiseLayer noise_layer = new Noise.NoiseLayer(settings_noise_layer);
        Random.InitState(settings_noise_layer.general_noise.seed);

        Vector3[] vertices_copy = reference_mesh.vertices;
        int[] triangles_copy = reference_mesh.triangles;
        for (int triangle_index = 0; triangle_index < triangles_copy.Length; triangle_index += 3)
        {
            Vector3 vertice_1 = vertices_copy[triangles_copy[triangle_index]];
            Vector3 vertice_2 = vertices_copy[triangles_copy[triangle_index + 1]];
            Vector3 vertice_3 = vertices_copy[triangles_copy[triangle_index + 2]];

            float x = (vertice_1.x + vertice_2.x + vertice_3.x) / 3;
            float z = (vertice_1.z + vertice_2.z + vertice_3.z) / 3;

            float noise_value = noise_layer.GetNoiseValue(x, z);
            if (noise_value > keep_range_noise.x && noise_value < keep_range_noise.y && Random.value <= keep_range_random_noise || Random.value <= keep_range_random)
            {
                int index = vertices.Count;

                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(index + 2);

                vertices.Add(vertice_1 + offset);
                vertices.Add(vertice_2 + offset);
                vertices.Add(vertice_3 + offset);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.Optimize();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }

    public static Mesh CubeMesh()
    {
        Vector3[] vertices = {
            new Vector3 (-0.5f, -0.5f, -0.5f),
            new Vector3 (0.5f, -0.5f, -0.5f),
            new Vector3 (0.5f, 0.5f, -0.5f),
            new Vector3 (-0.5f, 0.5f, -0.5f),
            new Vector3 (-0.5f, 0.5f, 0.5f),
            new Vector3 (0.5f, 0.5f, 0.5f),
            new Vector3 (0.5f, -0.5f, 0.5f),
            new Vector3 (-0.5f, -0.5f, 0.5f),
        };

        int[] triangles = {
            0, 2, 1, //face front
			0, 3, 2,
            2, 3, 4, //face top
			2, 4, 5,
            1, 2, 5, //face right
			1, 5, 6,
            0, 7, 4, //face left
			0, 4, 3,
            5, 4, 7, //face back
			5, 7, 6,
            0, 6, 7, //face bottom
			0, 1, 6
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.Optimize();
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

    public static GameObject CreatePrimitive(Mesh mesh, Material material, string name = "Primitive")
    {
        
        GameObject game_object = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MeshRenderer mesh_renderer = game_object.GetComponent<MeshRenderer>();
        mesh_renderer.material = material;
        return game_object;
        
        /*
        GameObject game_object = new GameObject(name);
        MeshFilter mesh_filter = game_object.AddComponent<MeshFilter>();
        mesh_filter.mesh = mesh;
        MeshRenderer mesh_renderer = game_object.AddComponent<MeshRenderer>();
        mesh_renderer.material = material;
        return game_object;
        */
    }
}
