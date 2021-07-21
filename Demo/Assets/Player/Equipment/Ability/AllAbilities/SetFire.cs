using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFire : MonoBehaviour
{
    [SerializeField] private GameObject game_object_flammable;
    [SerializeField] private GameObject game_object_non_flammable;
    [SerializeField] private GameObject game_object_flammable_ash;
    [SerializeField] private GameObject game_object_non_flammable_ash;

    private Mesh mesh_flammable;
    private MeshFilter mesh_filter_flammable;
    private Mesh mesh_non_flammable;
    private MeshFilter mesh_filter_non_flammable;

    private Mesh mesh_flammable_ash;
    private MeshFilter mesh_filter_flammable_ash;
    private Mesh mesh_non_flammable_ash;
    private MeshFilter mesh_filter_non_flammable_ash;

    private List<Vector3> vertices_flammable = new List<Vector3>();
    private List<int> triangles_flammable = new List<int>();

    private List<Vector3> vertices_non_flammable = new List<Vector3>();
    private List<int> triangles_non_flammable = new List<int>();

    private List<Vector3> vertices_flammable_ash = new List<Vector3>();
    private List<int> triangles_flammable_ash = new List<int>();

    private List<Vector3> vertices_non_flammable_ash = new List<Vector3>();
    private List<int> triangles_non_flammable_ash = new List<int>();

    private DamageByFire damage_by_fire;

    public void UpdateFlammableFire(Vector3 point, float time)
    {
        damage_by_fire.all_fire_spots.Add(point);

        Vector3 bottom_left_point = point + new Vector3(-0.5f, 0f, -0.5f);
        Vector3 top_point = point + new Vector3(0f, 0f, 0.5f);
        Vector3 bottom_right_point = point + new Vector3(0.5f, 0f, -0.5f);

        int count = vertices_flammable.Count;

        int vertices_flammable_index = vertices_flammable.FindIndex(ind => ind.Equals(Vector3.zero));
        if (vertices_flammable_index != -1)
        {
            vertices_flammable[vertices_flammable_index] = bottom_left_point;
            vertices_flammable[vertices_flammable_index + 1] = top_point;
            vertices_flammable[vertices_flammable_index + 2] = bottom_right_point;
        }
        else
        {
            vertices_flammable.Add(bottom_left_point);
            triangles_flammable.Add(count);
            vertices_flammable.Add(top_point);
            triangles_flammable.Add(count + 1);
            vertices_flammable.Add(bottom_right_point);
            triangles_flammable.Add(count + 2);
        }

        StartCoroutine(UpdateFlammableAsh(point, bottom_left_point, top_point, bottom_right_point, time));

        UpdateFlammableMesh();
    }

    private void UpdateFlammableMesh()
    {
        mesh_flammable.vertices = vertices_flammable.ToArray();
        mesh_flammable.triangles = triangles_flammable.ToArray();
        mesh_flammable.RecalculateNormals();
        mesh_flammable.RecalculateTangents();
        mesh_flammable.RecalculateBounds();
        mesh_filter_flammable.mesh = mesh_flammable;
    }

    private IEnumerator UpdateFlammableAsh(Vector3 point, Vector3 bottom_left_point, Vector3 top_point, Vector3 bottom_right_point, float time)
    {
        yield return new WaitForSeconds(time);

        damage_by_fire.all_fire_spots.Remove(point);

        // remove triangle from flammable
        int vertices_flammable_index = vertices_flammable.FindIndex(ind => ind.Equals(bottom_left_point));
        vertices_flammable[vertices_flammable_index] = Vector3.zero;
        vertices_flammable[vertices_flammable_index + 1] = Vector3.zero;
        vertices_flammable[vertices_flammable_index + 2] = Vector3.zero;

        // add triangle to flammable_ash
        int count = vertices_flammable_ash.Count;

        vertices_flammable_ash.Add(bottom_left_point);
        triangles_flammable_ash.Add(count);
        vertices_flammable_ash.Add(top_point);
        triangles_flammable_ash.Add(count + 1);
        vertices_flammable_ash.Add(bottom_right_point);
        triangles_flammable_ash.Add(count + 2);

        UpdateFlammableAshMesh();
        UpdateFlammableMesh();
    }

    private void UpdateFlammableAshMesh()
    {
        mesh_flammable_ash.vertices = vertices_flammable_ash.ToArray();
        mesh_flammable_ash.triangles = triangles_flammable_ash.ToArray();
        mesh_flammable_ash.RecalculateNormals();
        mesh_flammable_ash.RecalculateTangents();
        mesh_flammable_ash.RecalculateBounds();
        mesh_filter_flammable_ash.mesh = mesh_flammable_ash;
    }

    public void UpdateNonFlammableFire(Vector3 point, float time)
    {
        Vector3 bottom_left_point = point + new Vector3(-0.5f, 0f, -0.5f);
        Vector3 top_point = point + new Vector3(0f, 0f, 0.5f);
        Vector3 bottom_right_point = point + new Vector3(0.5f, 0f, -0.5f);

        int count = vertices_non_flammable.Count;

        int vertices_non_flammable_index = vertices_non_flammable.FindIndex(ind => ind.Equals(Vector3.zero));
        if (vertices_non_flammable_index != -1)
        {
            vertices_non_flammable[vertices_non_flammable_index] = bottom_left_point;
            vertices_non_flammable[vertices_non_flammable_index + 1] = top_point;
            vertices_non_flammable[vertices_non_flammable_index + 2] = bottom_right_point;
        }
        else
        {
            vertices_non_flammable.Add(bottom_left_point);
            triangles_non_flammable.Add(count);
            vertices_non_flammable.Add(top_point);
            triangles_non_flammable.Add(count + 1);
            vertices_non_flammable.Add(bottom_right_point);
            triangles_non_flammable.Add(count + 2);
        }

        StartCoroutine(UpdateNonFlammableAsh(bottom_left_point, top_point, bottom_right_point, time));

        UpdateNonFlammableMesh();
    }

    private void UpdateNonFlammableMesh()
    {
        mesh_non_flammable.vertices = vertices_non_flammable.ToArray();
        mesh_non_flammable.triangles = triangles_non_flammable.ToArray();
        mesh_non_flammable.RecalculateNormals();
        mesh_non_flammable.RecalculateTangents();
        mesh_non_flammable.RecalculateBounds();
        mesh_filter_non_flammable.mesh = mesh_non_flammable;
    }

    private IEnumerator UpdateNonFlammableAsh(Vector3 bottom_left_point, Vector3 top_point, Vector3 bottom_right_point, float time)
    {
        yield return new WaitForSeconds(time);

        // remove triangle from flammable
        int vertices_non_flammable_index = vertices_non_flammable.FindIndex(ind => ind.Equals(bottom_left_point));
        vertices_non_flammable[vertices_non_flammable_index] = Vector3.zero;
        vertices_non_flammable[vertices_non_flammable_index + 1] = Vector3.zero;
        vertices_non_flammable[vertices_non_flammable_index + 2] = Vector3.zero;

        // add triangle to flammable_ash
        int count = vertices_non_flammable_ash.Count;

        vertices_non_flammable_ash.Add(bottom_left_point);
        triangles_non_flammable_ash.Add(count);
        vertices_non_flammable_ash.Add(top_point);
        triangles_non_flammable_ash.Add(count + 1);
        vertices_non_flammable_ash.Add(bottom_right_point);
        triangles_non_flammable_ash.Add(count + 2);

        UpdateNonFlammableAshMesh();
        UpdateNonFlammableMesh();
    }

    private void UpdateNonFlammableAshMesh()
    {
        mesh_non_flammable_ash.vertices = vertices_non_flammable_ash.ToArray();
        mesh_non_flammable_ash.triangles = triangles_non_flammable_ash.ToArray();
        mesh_non_flammable_ash.RecalculateNormals();
        mesh_non_flammable_ash.RecalculateTangents();
        mesh_non_flammable_ash.RecalculateBounds();
        mesh_filter_non_flammable_ash.mesh = mesh_non_flammable_ash;
    }

    private void Start()
    {
        mesh_flammable = new Mesh();
        mesh_non_flammable = new Mesh();
        mesh_flammable_ash = new Mesh();
        mesh_non_flammable_ash = new Mesh();
        mesh_filter_flammable = game_object_flammable.GetComponent<MeshFilter>();
        mesh_filter_non_flammable = game_object_non_flammable.GetComponent<MeshFilter>();
        mesh_filter_flammable_ash = game_object_flammable_ash.GetComponent<MeshFilter>();
        mesh_filter_non_flammable_ash = game_object_non_flammable_ash.GetComponent<MeshFilter>();
        damage_by_fire = game_object_flammable.GetComponent<DamageByFire>();
    }
}
