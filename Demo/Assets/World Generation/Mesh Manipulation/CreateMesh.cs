using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMesh : MonoBehaviour
{
    public static IEnumerator CreateMeshByNoise(WaitForFixedUpdate wait, Noise.NoiseLayer[] noise_layers, Vector2 unit_size, Vector2Int resolution, Vector3 offset)
    {
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
            yield return wait;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        yield return wait;
        mesh.RecalculateTangents();
        yield return wait;
        mesh.RecalculateBounds();
        yield return mesh;
    }

    public static IEnumerator DropMeshVertices(WaitForFixedUpdate wait, Mesh reference_mesh, NoiseLayerSettings.NoiseLayer settings_noise_layer, Vector2 keep_range_noise, float keep_range_random_noise, float keep_range_random, Vector3 offset, Vector3 transform_offset)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Noise.NoiseLayer noise_layer = new Noise.NoiseLayer(settings_noise_layer);
        Random.InitState(settings_noise_layer.general_noise.seed);

        Vector3[] vertices_copy = reference_mesh.vertices;
        int[] triangles_copy = reference_mesh.triangles;

        void AddVertice(Vector3 vertice_1, Vector3 vertice_2, Vector3 vertice_3)
        {
            int index = vertices.Count;

            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);

            vertices.Add(vertice_1 + offset);
            vertices.Add(vertice_2 + offset);
            vertices.Add(vertice_3 + offset);
        }

        for (int triangle_index = 0; triangle_index < triangles_copy.Length; triangle_index += 3)
        {
            Vector3 vertice_1 = vertices_copy[triangles_copy[triangle_index]];
            Vector3 vertice_2 = vertices_copy[triangles_copy[triangle_index + 1]];
            Vector3 vertice_3 = vertices_copy[triangles_copy[triangle_index + 2]];

            float x = (vertice_1.x + vertice_2.x + vertice_3.x) / 3 + transform_offset.x;
            float z = (vertice_1.z + vertice_2.z + vertice_3.z) / 3 + transform_offset.z;

            float random_value = Random.value;
            if (random_value <= keep_range_random)
            {
                AddVertice(vertice_1, vertice_2, vertice_3);
            }
            else
            {
                float noise_value = noise_layer.GetNoiseValue(x, z);
                if (noise_value > keep_range_noise.x && noise_value < keep_range_noise.y && noise_value * random_value <= keep_range_random_noise)
                {
                    AddVertice(vertice_1, vertice_2, vertice_3);
                }
            }

            if (triangle_index % 999 == 0)
            {
                yield return wait;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.Optimize();
        yield return wait;
        mesh.RecalculateNormals();
        yield return wait;
        mesh.RecalculateTangents();
        yield return wait;
        mesh.RecalculateBounds();
        yield return mesh;
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

    public static Mesh LowPolySphereMesh()
    {
        /// <summary>
        /// Global struct of all vertices for icosahedron.
        /// </summary>
        Vector3[] GetVectors()
        {
            float s = 0.3f;
            float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

            return new Vector3[]
            {
            new Vector3(-1,  t,  0) * s,
            new Vector3( 1,  t,  0) * s,
            new Vector3(-1, -t,  0) * s,
            new Vector3( 1, -t,  0) * s,
            new Vector3( 0, -1,  t) * s,
            new Vector3( 0,  1,  t) * s,
            new Vector3( 0, -1, -t) * s,
            new Vector3( 0,  1, -t) * s,
            new Vector3( t,  0, -1) * s,
            new Vector3( t,  0,  1) * s,
            new Vector3(-t,  0, -1) * s,
            new Vector3(-t,  0,  1) * s
            };
        }

        /// <summary>
        /// Global struct of all triangles for icosahedron.
        /// </summary>
        int[] GetTriangles()
        {
            return new int[]
            {
             0, 11,  5,
             0,  5,  1,
             0,  1,  7,
             0,  7, 10,
             0, 10, 11,
             1,  5,  9,
             5, 11,  4,
            11, 10,  2,
            10,  7,  6,
             7,  1,  8,
             3,  9,  4,
             3,  4,  2,
             3,  2,  6,
             3,  6,  8,
             3,  8,  9,
             4,  9,  5,
             2,  4, 11,
             6,  2, 10,
             8,  6,  7,
             9,  8,  1
            };
        }

        /// <summary>
        /// Global function returning low poly sphere mesh (icosahedron).
        /// </summary>
        Mesh mesh = new Mesh();
        mesh.vertices = GetVectors();
        mesh.triangles = GetTriangles();
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
