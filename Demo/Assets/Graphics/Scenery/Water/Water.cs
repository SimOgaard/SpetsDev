using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Water : MonoBehaviour
{
    public static float water_level = 0f;
    private float current_water_level = 0f;
    public static float buoyancy_force = 1f;

    private void Update()
    {
        if (water_level != current_water_level)
        {
            current_water_level = water_level;
            transform.position = Vector3.up * water_level;
        }
    }

    private static Mesh BuildQuad(float width, float height)
    {
        Mesh mesh = new Mesh();

        // Setup vertices
        Vector3[] newVertices = new Vector3[4];
        float halfHeight = height * 0.5f;
        float halfWidth = width * 0.5f;
        newVertices[0] = new Vector3(-halfWidth, -halfHeight, 0);
        newVertices[1] = new Vector3(-halfWidth, halfHeight, 0);
        newVertices[2] = new Vector3(halfWidth, -halfHeight, 0);
        newVertices[3] = new Vector3(halfWidth, halfHeight, 0);

        // Setup UVs
        Vector2[] newUVs = new Vector2[newVertices.Length];
        newUVs[0] = new Vector2(0, 0);
        newUVs[1] = new Vector2(0, 1);
        newUVs[2] = new Vector2(1, 0);
        newUVs[3] = new Vector2(1, 1);

        // Setup triangles
        int[] newTriangles = new int[] { 0, 1, 2, 3, 2, 1 };

        // Setup normals
        Vector3[] newNormals = new Vector3[newVertices.Length];
        for (int i = 0; i < newNormals.Length; i++)
        {
            newNormals[i] = Vector3.forward;
        }

        // Create quad
        mesh.vertices = newVertices;
        mesh.uv = newUVs;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;

        return mesh;
    }

    public void Init(Material material, float width, float height, float water_level, Transform parrent)
    {
        gameObject.name = "water";
        transform.parent = parrent;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Water.water_level = water_level;
        gameObject.AddComponent<MeshFilter>().mesh = BuildQuad(width, height);
        MeshRenderer mesh_renderer = gameObject.AddComponent<MeshRenderer>();
        mesh_renderer.material = material;
        mesh_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mesh_renderer.receiveShadows = false;
    }
}
