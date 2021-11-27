using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// en compute shader ska ta in en funktion som representerar en 3d area. 
///         villkor som måste mötas för att ändra state v triangeln
///         (if inside area and is grass become fire). array av alla
///         trianglars mittpunkter (vector3) i form av en buffer.
///         array av ints som representerar triangelns enum state
///         (nothing, grass, fire) i form av en read write buffer. 
/// 
/// compute shader manager: initierar en compute buffer för varje ground mesh in game
/// mitt punkt av alla trianglar i meshen blir beräknade i create mesh
/// en mesh för alla enum states
/// 
/// each chunk should have a list representation of each mesh.
///         (grass mesh list (int, vector)) for easy adding/ removal of element
/// 
/// given bounding box få fram vilka chunks som är innom den
/// given chunk index itterate from bounding box corner to opposite corner
///         by using the fact that each triangles index is evenly spaced in x, y
/// check if point is inside volume
/// change point enum state
/// </summary>
public class GrassTesting : MonoBehaviour
{
    [SerializeField] private NoiseLayerSettings noise_layer_settings;
    private Noise.NoiseLayer[] noise_layers;

    [SerializeField] private Vector2 unit_size;
    [SerializeField] private Vector2Int resolution;

    private enum GroundMeshTypes { none, grass };
    private int ground_mesh_types_length;
    private Mesh[] ground_meshes;

    private Vector3[] full_mesh_vertices;
    private int[] full_mesh_triangles;

    [SerializeField] private Material[] ground_materials;

    int[] triangles;
    private void Awake()
    {
        int triangles_length = (resolution.x - 1) * (resolution.y - 1) * 2 * 3;
        triangles = new int[triangles_length];
        for (int i = 0; i < triangles_length; i++)
        {
            triangles[i] = 0;
        }

        noise_layers = Noise.CreateNoiseLayers(noise_layer_settings);

        ground_mesh_types_length = GroundMeshTypes.GetNames(typeof(GroundMeshTypes)).Length;
        ground_meshes = new Mesh[ground_mesh_types_length];
    }

    private void Start()
    {
        Mesh throwaway_mesh = new Mesh();
        CreateMesh.CreateMeshByNoise(ref throwaway_mesh, noise_layers, unit_size, resolution, Vector3.zero);
        full_mesh_vertices = throwaway_mesh.vertices;
        full_mesh_triangles = throwaway_mesh.triangles;

        for (int i = 0; i < ground_mesh_types_length; i++)
        {
            CreateChildMesh(i);
        }
    }

    private GameObject CreateChildMesh(int mesh_type_index)
    {
        GameObject child = new GameObject(GroundMeshTypes.GetName(typeof(GroundMeshTypes), mesh_type_index));
        CreateMesh.CreateMeshByNoise(ref ground_meshes[mesh_type_index], noise_layers, unit_size, resolution, Vector3.zero);

        child.AddComponent<MeshFilter>().mesh = ground_meshes[mesh_type_index];
        child.AddComponent<MeshRenderer>().material = ground_materials[mesh_type_index];

        child.transform.parent = transform;
        child.transform.localPosition = Vector3.zero;
        return child;
    }

    [SerializeField] private Collider test_collider;
    private void FixedUpdate()
    {
        Vector2 mesh_size = Vector2.Scale(unit_size, resolution);
        Vector3 mesh_size_half = new Vector3(mesh_size.x * 0.5f, 0f, mesh_size.y * 0.5f);

        Vector3 mesh_mid_point = transform.position;
        Vector3 mesh_origo_point = mesh_mid_point - mesh_size_half;

        Bounds bounds = test_collider.bounds;

        bounds.center -= mesh_origo_point;

        Vector3 bounds_min = bounds.min;
        Vector3 bounds_max = bounds.max;

        Vector2Int bounds_min_to_mesh_index = new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(bounds_min.x / unit_size.x), 0, resolution.x),
            Mathf.Clamp(Mathf.RoundToInt(bounds_min.z / unit_size.y), 0, resolution.y)
        );

        Vector2Int bounds_max_to_mesh_index = new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(bounds_max.x / unit_size.x), 0, resolution.x),
            Mathf.Clamp(Mathf.RoundToInt(bounds_max.z / unit_size.y), 0, resolution.y)
        );

        for (int z = bounds_min_to_mesh_index.y * resolution.y * 2 * 3; z < bounds_max_to_mesh_index.y * resolution.y * 2 * 3; z += resolution.y * 2 * 3)
        {
            for (int x = bounds_min_to_mesh_index.x * 6; x < bounds_max_to_mesh_index.x * 6; x += 6)
            {
                int triangle_index = z + x;
                triangles[triangle_index] = full_mesh_triangles[triangle_index];

                triangle_index++;
                triangles[triangle_index] = full_mesh_triangles[triangle_index];

                triangle_index++;
                triangles[triangle_index] = full_mesh_triangles[triangle_index];

                triangle_index++;
                triangles[triangle_index] = full_mesh_triangles[triangle_index];

                triangle_index++;
                triangles[triangle_index] = full_mesh_triangles[triangle_index];

                triangle_index++;
                triangles[triangle_index] = full_mesh_triangles[triangle_index];
            }
        }

        ground_meshes[0].triangles = triangles;
    }

    private void OnDrawGizmos()
    {
        Bounds bounds = test_collider.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        min.y = max.y;

        Gizmos.DrawLine(min, max);
    }
}
