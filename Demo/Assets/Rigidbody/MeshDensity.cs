using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDensity : MonoBehaviour
{
    public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;

        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    public static float VolumeOfMesh(Mesh mesh, Vector3 lossy_scale)
    {
        float volume = 0;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        volume *= lossy_scale.x * lossy_scale.y * lossy_scale.z;
        return Mathf.Abs(volume);
    }

    public static float WeightOfMesh(Mesh mesh, Vector3 lossy_scale, Density.DensityValues density)
    {
        return VolumeOfMesh(mesh, lossy_scale) * (float) density * Density.global_density;
    }
}
