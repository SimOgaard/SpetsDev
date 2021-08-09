using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Updates all meshes that holds every point that is on fire or is ashed.
/// </summary>
public class SetFire : MonoBehaviour
{
    [SerializeField] private GameObject game_object_flammable;
    [SerializeField] private GameObject game_object_non_flammable;
    [SerializeField] private GameObject game_object_flammable_ash;
    [SerializeField] private GameObject game_object_non_flammable_ash;

    private Mesh mesh_flammable;
    private MeshFilter mesh_filter_flammable;
    private MeshRenderer mesh_renderer_flammable;
    private Mesh mesh_non_flammable;
    private MeshFilter mesh_filter_non_flammable;
    private MeshRenderer mesh_renderer_non_flammable;

    private Mesh mesh_flammable_ash;
    private MeshFilter mesh_filter_flammable_ash;
    private MeshRenderer mesh_renderer_flammable_ash;
    private Mesh mesh_non_flammable_ash;
    private MeshFilter mesh_filter_non_flammable_ash;
    private MeshRenderer mesh_renderer_non_flammable_ash;

    private List<Vector3> vertices_flammable = new List<Vector3>();
    private List<int> triangles_flammable = new List<int>();

    private List<Vector3> vertices_non_flammable = new List<Vector3>();
    private List<int> triangles_non_flammable = new List<int>();

    private List<Vector3> vertices_flammable_ash = new List<Vector3>();
    private List<int> triangles_flammable_ash = new List<int>();

    private List<Vector3> vertices_non_flammable_ash = new List<Vector3>();
    private List<int> triangles_non_flammable_ash = new List<int>();

    private DamageByFire damage_by_fire;

    private float fire_width = 0.5f;

    /// <summary>
    /// Given point place triangle with normal.vector.up to mesh that holds flammable material.
    /// </summary>
    public void UpdateFlammableFire(Vector3 point, Vector3 normal, float time)
    {
        damage_by_fire.all_fire_spots.Add(new Vector3(point.x, 0f, point.z));

        Vector3 bottom_left_point = point + Quaternion.AngleAxis(0f, normal) * new Vector3(1f, 0f, 1f);
        Vector3 top_point = point + Quaternion.AngleAxis(120f, normal) * new Vector3(1f, 0f, 1f);
        Vector3 bottom_right_point = point + Quaternion.AngleAxis(240f, normal) * new Vector3(1f, 0f, 1f);

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

    /// <summary>
    /// Updates flammable fire mesh.
    /// </summary>
    private void UpdateFlammableMesh()
    {
        mesh_flammable.vertices = vertices_flammable.ToArray();
        mesh_flammable.triangles = triangles_flammable.ToArray();
        mesh_flammable.RecalculateNormals();
        mesh_flammable.RecalculateTangents();
        mesh_flammable.RecalculateBounds();
        mesh_filter_flammable.mesh = mesh_flammable;
    }

    /// <summary>
    /// Given point place triangle with normal.vector.up to mesh that holds flammable ash material.
    /// </summary>
    private IEnumerator UpdateFlammableAsh(Vector3 point, Vector3 bottom_left_point, Vector3 top_point, Vector3 bottom_right_point, float time)
    {
        yield return new WaitForSeconds(time);

        damage_by_fire.all_fire_spots.Remove(new Vector3(point.x, 0f, point.z));

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

    /// <summary>
    /// Updates flammable ash mesh.
    /// </summary>
    private void UpdateFlammableAshMesh()
    {
        mesh_flammable_ash.vertices = vertices_flammable_ash.ToArray();
        mesh_flammable_ash.triangles = triangles_flammable_ash.ToArray();
        mesh_flammable_ash.RecalculateNormals();
        mesh_flammable_ash.RecalculateTangents();
        mesh_flammable_ash.RecalculateBounds();
        mesh_filter_flammable_ash.mesh = mesh_flammable_ash;
    }

    /// <summary>
    /// Given point place triangle with normal.vector.up to mesh that holds non flammable material.
    /// </summary>
    public void UpdateNonFlammableFire(Vector3 point, Vector3 normal, float time)
    {
        Vector3 bottom_left_point = point + Quaternion.AngleAxis(0f, normal) * new Vector3(1f, 0f, 1f);
        Vector3 top_point = point + Quaternion.AngleAxis(120f, normal) * new Vector3(1f, 0f, 1f);
        Vector3 bottom_right_point = point + Quaternion.AngleAxis(240f, normal) * new Vector3(1f, 0f, 1f);

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

    /// <summary>
    /// Updates non flammable mesh.
    /// </summary>
    private void UpdateNonFlammableMesh()
    {
        mesh_non_flammable.vertices = vertices_non_flammable.ToArray();
        mesh_non_flammable.triangles = triangles_non_flammable.ToArray();
        mesh_non_flammable.RecalculateNormals();
        mesh_non_flammable.RecalculateTangents();
        mesh_non_flammable.RecalculateBounds();
        mesh_filter_non_flammable.mesh = mesh_non_flammable;
    }

    /// <summary>
    /// Given point place triangle with normal.vector.up to mesh that holds non flammable ash material.
    /// </summary>
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

    /// <summary>
    /// Updates non flammable ash mesh.
    /// </summary>
    private void UpdateNonFlammableAshMesh()
    {
        mesh_non_flammable_ash.vertices = vertices_non_flammable_ash.ToArray();
        mesh_non_flammable_ash.triangles = triangles_non_flammable_ash.ToArray();
        mesh_non_flammable_ash.RecalculateNormals();
        mesh_non_flammable_ash.RecalculateTangents();
        mesh_non_flammable_ash.RecalculateBounds();
        mesh_filter_non_flammable_ash.mesh = mesh_non_flammable_ash;
    }

    /// <summary>
    /// Updates visual component of fire on ground when FireballAbility upgrades.
    /// NOT YET IMPLEMENTED
    /// </summary>
    public void UpdateFire(float ground_fire_radius)
    {
        return;

        fire_width = ground_fire_radius / 8f;

        return;
        mesh_renderer_flammable.material.SetFloat("_BladeWidth", ground_fire_radius / 4f);
        mesh_renderer_flammable.material.SetFloat("_BladeHeight", ground_fire_radius / 3f);
        mesh_renderer_non_flammable.material.SetFloat("_BladeWidth", ground_fire_radius / 8f);
        mesh_renderer_non_flammable.material.SetFloat("_BladeHeight", ground_fire_radius / 12f);

        Material old_material_non_flammable_ash = mesh_renderer_flammable_ash.material;
        Material old_material_flammable_ash = mesh_renderer_non_flammable_ash.material;

        // Set FlammableAsh and NonFlamableAsh to new game objects to keep ashes
        game_object_flammable_ash = new GameObject("FlammableAsh");
        game_object_non_flammable_ash = new GameObject("NonFlammableAsh");
        game_object_flammable_ash.transform.parent = transform;
        game_object_non_flammable_ash.transform.parent = transform;
        mesh_flammable_ash = new Mesh();
        mesh_non_flammable_ash = new Mesh();
        mesh_filter_flammable_ash = game_object_flammable_ash.AddComponent<MeshFilter>();
        mesh_filter_non_flammable_ash = game_object_non_flammable_ash.AddComponent<MeshFilter>();
        mesh_renderer_flammable_ash = game_object_flammable_ash.AddComponent<MeshRenderer>();
        mesh_renderer_non_flammable_ash = game_object_non_flammable_ash.AddComponent<MeshRenderer>();

        // new ashes for new fire
        mesh_renderer_flammable_ash.material = old_material_flammable_ash;
        mesh_renderer_flammable_ash.material.SetFloat("_BladeWidth", ground_fire_radius / 4f);
        mesh_renderer_flammable_ash.material.SetFloat("_BladeHeight", ground_fire_radius / 3f);
        mesh_renderer_non_flammable_ash.material = old_material_non_flammable_ash;
        mesh_renderer_non_flammable_ash.material.SetFloat("_BladeWidth", ground_fire_radius / 8f);
        mesh_renderer_non_flammable_ash.material.SetFloat("_BladeHeight", ground_fire_radius / 12f);
    }

    /// <summary>
    /// Grabs all components and initializes all meshes.
    /// </summary>
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
        mesh_renderer_flammable = game_object_flammable.GetComponent<MeshRenderer>();
        mesh_renderer_non_flammable = game_object_non_flammable.GetComponent<MeshRenderer>();
        mesh_renderer_flammable_ash = game_object_flammable_ash.GetComponent<MeshRenderer>();
        mesh_renderer_non_flammable_ash = game_object_non_flammable_ash.GetComponent<MeshRenderer>();
        
        damage_by_fire = game_object_flammable.GetComponent<DamageByFire>();
    }
}
