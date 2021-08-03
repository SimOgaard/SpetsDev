using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AI;

public class Ground : MonoBehaviour
{
    [Header("Shape")]
    [SerializeField]
    private Vector2 triangle_size = new Vector2(5f, 5f);
    [SerializeField]
    private Vector2 ground_resolution = new Vector2(100f, 100f);

    [Header("Terrain")]
    [SerializeField]
    private bool update_mesh = false;
    //[SerializeField]
    //private int octaves = 3;
    [SerializeField]
    private float frequency_1 = 0.02f;
    [SerializeField]
    private float frequency_2 = 0.1f;
    [SerializeField]
    private float frequency_3 = 1f;
    //[SerializeField]
    //private float offset = 2f;
    [SerializeField]
    private float amplitude_1 = 10f;
    [SerializeField]
    private float amplitude_2 = 1f;
    [SerializeField]
    private float amplitude_3 = 2.5f;
    //[SerializeField]
    //private float amplitude_change = 0.5f;

    private Vector3[] vertices;
    private int[] triangles;
    private Mesh ground_mesh;

    public Texture2D TEST_IMAGE_NOISE;

    /*
    float Random(Vector2 position)
    {
        float rnd = Mathf.Sin(Vector2.Dot(position, new Vector2(12.9898f, 78.233f))) * 43758.5453123f;
        return rnd - Mathf.Floor(rnd);
    }

    float Noise(Vector2 position)
    {
        Vector2 i = new Vector2(Mathf.Floor(position.x), Mathf.Floor(position.y));
        Vector2 f = new Vector2(position.x - Mathf.Floor(position.x), position.y - Mathf.Floor(position.y));

        float a = Random(i);
        float b = Random(i + new Vector2(1.0f, 0.0f));
        float c = Random(i + new Vector2(0.0f, 1.0f));
        float d = Random(i + new Vector2(1.0f, 1.0f));

        Vector2 u = Vector2.Scale(Vector2.Scale(f, f), (3.0f - 2.0f) * f);

        return Mathf.Lerp(a, b, u.x) + (c - a) * u.y * (1.0f - u.x) + (d - b) * u.x * u.y;
    }

    float Fbm(Vector2 position)
    {
        float value = 0.0f;
        float current_amplitude = amplitude;

        for (int i = 0; i < octaves; i++)
        {
            value += current_amplitude * Noise(position * frequency);
            position *= offset;
            current_amplitude *= amplitude_change;
        }
        return value;
    }
    */

    void RenderMyMesh()
    {
        ground_mesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt16
        };

        ground_mesh.vertices = vertices;
        ground_mesh.triangles = triangles;

        ground_mesh.RecalculateNormals();
        ground_mesh.RecalculateTangents();
        ground_mesh.RecalculateBounds();
    }

    private void InitMesh()
    {
        // TEST
        TEST_IMAGE_NOISE = new Texture2D((int) Mathf.Floor(ground_resolution.x), (int) Mathf.Floor(ground_resolution.y));

        // TEST 2
        /*TEST_IMAGE_NOISE = new Texture2D(128, 128);
        for (int x = 0; x < 128; x++)
        {
            for (int y = 0; y < 128; y++)
            {
                float newX = x / 64f;
                float newY = y / 64f;

                float noiseValue = (Fbm(new Vector2(newX, newY)) + 1.0f) * 0.5f;
                Color color = new Color(noiseValue, noiseValue, noiseValue);
                TEST_IMAGE_NOISE.SetPixel(x, y, color);
            }
        }
        TEST_IMAGE_NOISE.Apply();*/

        Vector3 vertex = Vector3.zero;

        ground_resolution.x = Mathf.Floor(ground_resolution.x) + 1;
        ground_resolution.y = Mathf.Floor(ground_resolution.y) + 1;

        int vertices_length = (int)ground_resolution.x * (int)ground_resolution.y;
        int triangles_length = ((int)ground_resolution.x - 1) * ((int)ground_resolution.y - 1) * 6;
        vertices = new Vector3[vertices_length];
        triangles = new int[triangles_length];
        int vertices_index = 0;
        int triangles_index = 0;

        transform.position = Quaternion.Euler(0f, 45f, 0f) * new Vector3(-(ground_resolution.x - 1f) * triangle_size.x * 0.5f, transform.position.y, -(ground_resolution.y - 1f) * triangle_size.y * 0.5f);
        transform.rotation = Quaternion.Euler(0f, 45f, 0f);

        for (int z = 0; z < (int)ground_resolution.y; z++)
        {
            vertex.z = triangle_size.y * z;

            for (int x = 0; x < (int)ground_resolution.x; x++)
            {
                vertex.x = triangle_size.x * x;
                vertex.y = Mathf.PerlinNoise(x * frequency_1, z * frequency_1) * amplitude_1;
                vertex.y += Mathf.PerlinNoise(x * frequency_2, z * frequency_2) * amplitude_2;
                vertex.y += Mathf.PerlinNoise(x * frequency_3, z * frequency_3) * amplitude_3;
                //vertex.y = Fbm(new Vector2(x, z));

                vertices_index = z * (int)ground_resolution.y + x;
                vertices[vertices_index] = vertex;

                if (x != ground_resolution.x - 1 && z != 0)
                {
                    triangles[triangles_index] = vertices_index;
                    triangles[triangles_index + 1] = vertices_index - (int)ground_resolution.x + 1;
                    triangles[triangles_index + 2] = vertices_index - (int)ground_resolution.x;

                    triangles[triangles_index + 3] = vertices_index;
                    triangles[triangles_index + 4] = vertices_index + 1;
                    triangles[triangles_index + 5] = vertices_index - (int)ground_resolution.x + 1;

                    triangles_index += 6;
                }

                // TESTING
                float col = vertex.y / amplitude_1;
                Color color = new Color(col, col, col);
                TEST_IMAGE_NOISE.SetPixel(x, z, color);
            }
        }

        RenderMyMesh();

        // TESTING
        TEST_IMAGE_NOISE.Apply();

        gameObject.GetComponent<MeshFilter>().mesh = ground_mesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = ground_mesh;

        transform.GetChild(0).GetComponent<MeshFilter>().mesh = ground_mesh;
    }

    private void Start()
    {
        //gameObject.AddComponent(typeof(MeshFilter));
        //gameObject.AddComponent(typeof(MeshRenderer));
        //gameObject.AddComponent<MeshCollider>();
        InitMesh();
        NavMeshBuilder.BuildNavMesh();
    }

    private void Update()
    {
        if (update_mesh)
        {
            update_mesh = false;
            InitMesh();
        }
    }
}
