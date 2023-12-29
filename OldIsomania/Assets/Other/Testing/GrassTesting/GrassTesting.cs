using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region old summary
/// <summary>
/// en compute shader ska ta in en funktion som representerar en 3d area. 
///         villkor som m�ste m�tas f�r att �ndra state v triangeln
///         (if inside area and is grass become fire). array av alla
///         trianglars mittpunkter (vector3) i form av en buffer.
///         array av ints som representerar triangelns enum state
///         (nothing, grass, fire) i form av en read write buffer. 
/// 
/// compute shader manager: initierar en compute buffer f�r varje ground mesh in game
/// mitt punkt av alla trianglar i meshen blir ber�knade i create mesh
/// en mesh f�r alla enum states
/// 
/// each chunk should have a list representation of each mesh.
///         (grass mesh list (int, vector)) for easy adding/ removal of element
/// 
/// given bounding box f� fram vilka chunks som �r innom den
/// given chunk index itterate from bounding box corner to opposite corner
///         by using the fact that each triangles index is evenly spaced in x, y
/// check if point is inside volume
/// change point enum state
/// </summary>
#endregion 

/// <summary>
/// Given:
///     master mesh with:
///                     one set of vertices that are fixed
///                     submeshes for every material state (grass/fire/none)
///                     for every submesh keep a triangle dictionary in array of Dictionary
///                     update submesh using SetTriangles(List<int> triangles, int submesh, bool calculateBounds = false, int baseVertex = 0);
///                     
///     
///     how to do this?:
///         given triangle index that is the length of 1/3 of all triangles[]
///         index into the right submesh and its list<int> to find the triangle and delete it
/// 
/// 
///     array of lists containing triangles
///     
/// 
///     vector3 of every point in ground mesh
///     int of every triangle in mesh
///     enum/int of every triangles material state (grass/fire/none) that links to its submesh
///     
///     
///     list for every 
/// </summary>
public class GrassTesting : MonoBehaviour
{
    [SerializeField] private NoiseLayerSettings noiseLayerSettings;
    private Noise.NoiseLayer[] noiseLayers;

    [SerializeField] private Vector2 unitSize;
    [SerializeField] private Vector2Int resolution;

    private enum GroundMeshTypes { none, grass };
    private int groundMeshTypesLength;
    private Mesh[] groundMeshes;

    private Vector3[] fullMeshVertices;
    private int[] fullMeshTriangles;

    [SerializeField] private Material[] groundMaterials;

    int[] triangles;
    private void Awake()
    {
        int trianglesLength = (resolution.x) * (resolution.y) * 2 * 3;
        int verticesLength = (resolution.x + 1) * (resolution.y + 1);
        triangles = new int[trianglesLength];
        for (int i = 0; i < trianglesLength; i++)
        {
            triangles[i] = verticesLength;
        }

        noiseLayers = Noise.CreateNoiseLayers(noiseLayerSettings);

        groundMeshTypesLength = GroundMeshTypes.GetNames(typeof(GroundMeshTypes)).Length;
        groundMeshes = new Mesh[groundMeshTypesLength];
    }

    private void Start()
    {
        Mesh throwawayMesh = new Mesh();
        CreateMesh.CreateMeshByNoise(ref throwawayMesh, noiseLayers, unitSize, resolution, Vector3.zero);
        fullMeshVertices = throwawayMesh.vertices;
        fullMeshTriangles = throwawayMesh.triangles;

        for (int i = 0; i < groundMeshTypesLength; i++)
        {
            CreateChildMesh(i);
        }
    }

    private GameObject CreateChildMesh(int meshTypeIndex)
    {
        GameObject child = new GameObject(GroundMeshTypes.GetName(typeof(GroundMeshTypes), meshTypeIndex));
        CreateMesh.CreateMeshByNoise(ref groundMeshes[meshTypeIndex], noiseLayers, unitSize, resolution, Vector3.zero);

        child.AddComponent<MeshFilter>().mesh = groundMeshes[meshTypeIndex];
        child.AddComponent<MeshRenderer>().material = groundMaterials[meshTypeIndex];

        child.transform.parent = transform;
        child.transform.localPosition = Vector3.zero;
        return child;
    }

    [SerializeField] private Collider testCollider;
    private void FixedUpdate()
    {
        // precalculate middle of triangle and store it in array
        // check if middle of triangle is inside collider

        // this whole thing should be able to be done on another thread
        // you should be able to create mesh using compute shader instead of on cpu
        // you should be able to convert couroutines to async/await https://www.youtube.com/watch?v=WY-mk-ZGAq8&abChannel=Tarodev 

        Vector2 meshSize = Vector2.Scale(unitSize, resolution);
        Vector3 meshSizeHalf = new Vector3(meshSize.x * 0.5f, 0f, meshSize.y * 0.5f);

        Vector3 meshMidPoint = transform.position;
        Vector3 meshOrigoPoint = meshMidPoint - meshSizeHalf;

        Bounds bounds = testCollider.bounds;

        bounds.center -= meshOrigoPoint;

        Vector3 boundsMin = bounds.min;
        Vector3 boundsMax = bounds.max;

        Vector2Int boundsMinToMeshIndex = new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(boundsMin.x / unitSize.x), 0, resolution.x),
            Mathf.Clamp(Mathf.RoundToInt(boundsMin.z / unitSize.y), 0, resolution.y)
        );

        Vector2Int boundsMaxToMeshIndex = new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(boundsMax.x / unitSize.x), 0, resolution.x),
            Mathf.Clamp(Mathf.RoundToInt(boundsMax.z / unitSize.y), 0, resolution.y)
        );

        for (int z = boundsMinToMeshIndex.y * resolution.y * 2 * 3; z < boundsMaxToMeshIndex.y * resolution.y * 2 * 3; z += resolution.y * 2 * 3)
        {
            for (int x = boundsMinToMeshIndex.x * 6; x < boundsMaxToMeshIndex.x * 6; x += 6)
            {
                int triangleIndex = z + x;


                if (true)
                {
                    triangles[triangleIndex] = fullMeshTriangles[triangleIndex];

                    triangles[triangleIndex + 1] = fullMeshTriangles[triangleIndex];

                    triangles[triangleIndex + 2] = fullMeshTriangles[triangleIndex];
                }

                if (true)
                {
                    triangles[triangleIndex + 3] = fullMeshTriangles[triangleIndex];

                    triangles[triangleIndex + 4] = fullMeshTriangles[triangleIndex];

                    triangles[triangleIndex + 5] = fullMeshTriangles[triangleIndex];
                }
            }
        }

        groundMeshes[0].triangles = triangles;
    }

    private void OnDrawGizmos()
    {
        Bounds bounds = testCollider.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        min.y = max.y;

        Gizmos.DrawLine(min, max);
    }
}
