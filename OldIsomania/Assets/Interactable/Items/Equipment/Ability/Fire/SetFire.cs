using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Updates all meshes that holds every point that is on fire or is ashed.
/// </summary>
public class SetFire : MonoBehaviour
{
    [SerializeField] private GameObject gameObjectFlammable;
    [SerializeField] private GameObject gameObjectNonFlammable;
    [SerializeField] private GameObject gameObjectFlammableAsh;
    [SerializeField] private GameObject gameObjectNonFlammableAsh;

    private Mesh meshFlammable;
    private MeshFilter meshFilterFlammable;
    private MeshRenderer meshRendererFlammable;
    private Mesh meshNonFlammable;
    private MeshFilter meshFilterNonFlammable;
    private MeshRenderer meshRendererNonFlammable;

    private Mesh meshFlammableAsh;
    private MeshFilter meshFilterFlammableAsh;
    private MeshRenderer meshRendererFlammableAsh;
    private Mesh meshNonFlammableAsh;
    private MeshFilter meshFilterNonFlammableAsh;
    private MeshRenderer meshRendererNonFlammableAsh;

    private List<Vector3> verticesFlammable = new List<Vector3>();
    private List<int> trianglesFlammable = new List<int>();

    private List<Vector3> verticesNonFlammable = new List<Vector3>();
    private List<int> trianglesNonFlammable = new List<int>();

    private List<Vector3> verticesFlammableAsh = new List<Vector3>();
    private List<int> trianglesFlammableAsh = new List<int>();

    private List<Vector3> verticesNonFlammableAsh = new List<Vector3>();
    private List<int> trianglesNonFlammableAsh = new List<int>();

    private DamageByFire damageByFire;

    private float fireWidth = 0.5f;

    /// <summary>
    /// Given point place triangle with normal.vector.up to mesh that holds flammable material.
    /// </summary>
    public void UpdateFlammableFire(Vector3 point, Vector3 normal, float time)
    {
        if (point.y < Water.waterLevel)
        {
            return;
        }

        damageByFire.allFireSpots.Add(new Vector3(point.x, 0f, point.z));

        Vector3 bottomLeftPoint = point + Quaternion.AngleAxis(0f, normal) * new Vector3(1f, 0f, 1f);
        Vector3 topPoint = point + Quaternion.AngleAxis(120f, normal) * new Vector3(1f, 0f, 1f);
        Vector3 bottomRightPoint = point + Quaternion.AngleAxis(240f, normal) * new Vector3(1f, 0f, 1f);

        int count = verticesFlammable.Count;

        int verticesFlammableIndex = verticesFlammable.FindIndex(ind => ind.Equals(Vector3.zero));
        if (verticesFlammableIndex != -1)
        {
            verticesFlammable[verticesFlammableIndex] = bottomLeftPoint;
            verticesFlammable[verticesFlammableIndex + 1] = topPoint;
            verticesFlammable[verticesFlammableIndex + 2] = bottomRightPoint;
        }
        else
        {
            verticesFlammable.Add(bottomLeftPoint);
            trianglesFlammable.Add(count);
            verticesFlammable.Add(topPoint);
            trianglesFlammable.Add(count + 1);
            verticesFlammable.Add(bottomRightPoint);
            trianglesFlammable.Add(count + 2);
        }

        StartCoroutine(UpdateFlammableAsh(point, bottomLeftPoint, topPoint, bottomRightPoint, time));

        UpdateFlammableMesh();
    }

    /// <summary>
    /// Updates flammable fire mesh.
    /// </summary>
    private void UpdateFlammableMesh()
    {
        meshFlammable.vertices = verticesFlammable.ToArray();
        meshFlammable.triangles = trianglesFlammable.ToArray();
        meshFlammable.RecalculateNormals();
        meshFlammable.RecalculateTangents();
        meshFlammable.RecalculateBounds();
        meshFilterFlammable.mesh = meshFlammable;
    }

    /// <summary>
    /// Given point place triangle with normal.vector.up to mesh that holds flammable ash material.
    /// </summary>
    private IEnumerator UpdateFlammableAsh(Vector3 point, Vector3 bottomLeftPoint, Vector3 topPoint, Vector3 bottomRightPoint, float time)
    {
        yield return new WaitForSeconds(time);

        damageByFire.allFireSpots.Remove(new Vector3(point.x, 0f, point.z));

        // remove triangle from flammable
        int verticesFlammableIndex = verticesFlammable.FindIndex(ind => ind.Equals(bottomLeftPoint));
        verticesFlammable[verticesFlammableIndex] = Vector3.zero;
        verticesFlammable[verticesFlammableIndex + 1] = Vector3.zero;
        verticesFlammable[verticesFlammableIndex + 2] = Vector3.zero;

        // add triangle to flammableAsh
        int count = verticesFlammableAsh.Count;

        verticesFlammableAsh.Add(bottomLeftPoint);
        trianglesFlammableAsh.Add(count);
        verticesFlammableAsh.Add(topPoint);
        trianglesFlammableAsh.Add(count + 1);
        verticesFlammableAsh.Add(bottomRightPoint);
        trianglesFlammableAsh.Add(count + 2);

        UpdateFlammableAshMesh();
        UpdateFlammableMesh();
    }

    /// <summary>
    /// Updates flammable ash mesh.
    /// </summary>
    private void UpdateFlammableAshMesh()
    {
        meshFlammableAsh.vertices = verticesFlammableAsh.ToArray();
        meshFlammableAsh.triangles = trianglesFlammableAsh.ToArray();
        meshFlammableAsh.RecalculateNormals();
        meshFlammableAsh.RecalculateTangents();
        meshFlammableAsh.RecalculateBounds();
        meshFilterFlammableAsh.mesh = meshFlammableAsh;
    }

    /// <summary>
    /// Given point place triangle with normal.vector.up to mesh that holds non flammable material.
    /// </summary>
    public void UpdateNonFlammableFire(Vector3 point, Vector3 normal, float time)
    {
        if (point.y < Water.waterLevel)
        {
            return;
        }

        Vector3 bottomLeftPoint = point + Quaternion.AngleAxis(0f, normal) * new Vector3(1f, 0f, 1f);
        Vector3 topPoint = point + Quaternion.AngleAxis(120f, normal) * new Vector3(1f, 0f, 1f);
        Vector3 bottomRightPoint = point + Quaternion.AngleAxis(240f, normal) * new Vector3(1f, 0f, 1f);

        int count = verticesNonFlammable.Count;

        int verticesNonFlammableIndex = verticesNonFlammable.FindIndex(ind => ind.Equals(Vector3.zero));
        if (verticesNonFlammableIndex != -1)
        {
            verticesNonFlammable[verticesNonFlammableIndex] = bottomLeftPoint;
            verticesNonFlammable[verticesNonFlammableIndex + 1] = topPoint;
            verticesNonFlammable[verticesNonFlammableIndex + 2] = bottomRightPoint;
        }
        else
        {
            verticesNonFlammable.Add(bottomLeftPoint);
            trianglesNonFlammable.Add(count);
            verticesNonFlammable.Add(topPoint);
            trianglesNonFlammable.Add(count + 1);
            verticesNonFlammable.Add(bottomRightPoint);
            trianglesNonFlammable.Add(count + 2);
        }

        StartCoroutine(UpdateNonFlammableAsh(bottomLeftPoint, topPoint, bottomRightPoint, time));

        UpdateNonFlammableMesh();
    }

    /// <summary>
    /// Updates non flammable mesh.
    /// </summary>
    private void UpdateNonFlammableMesh()
    {
        meshNonFlammable.vertices = verticesNonFlammable.ToArray();
        meshNonFlammable.triangles = trianglesNonFlammable.ToArray();
        meshNonFlammable.RecalculateNormals();
        meshNonFlammable.RecalculateTangents();
        meshNonFlammable.RecalculateBounds();
        meshFilterNonFlammable.mesh = meshNonFlammable;
    }

    /// <summary>
    /// Given point place triangle with normal.vector.up to mesh that holds non flammable ash material.
    /// </summary>
    private IEnumerator UpdateNonFlammableAsh(Vector3 bottomLeftPoint, Vector3 topPoint, Vector3 bottomRightPoint, float time)
    {
        yield return new WaitForSeconds(time);

        // remove triangle from flammable
        int verticesNonFlammableIndex = verticesNonFlammable.FindIndex(ind => ind.Equals(bottomLeftPoint));
        verticesNonFlammable[verticesNonFlammableIndex] = Vector3.zero;
        verticesNonFlammable[verticesNonFlammableIndex + 1] = Vector3.zero;
        verticesNonFlammable[verticesNonFlammableIndex + 2] = Vector3.zero;

        // add triangle to flammableAsh
        int count = verticesNonFlammableAsh.Count;

        verticesNonFlammableAsh.Add(bottomLeftPoint);
        trianglesNonFlammableAsh.Add(count);
        verticesNonFlammableAsh.Add(topPoint);
        trianglesNonFlammableAsh.Add(count + 1);
        verticesNonFlammableAsh.Add(bottomRightPoint);
        trianglesNonFlammableAsh.Add(count + 2);

        UpdateNonFlammableAshMesh();
        UpdateNonFlammableMesh();
    }

    /// <summary>
    /// Updates non flammable ash mesh.
    /// </summary>
    private void UpdateNonFlammableAshMesh()
    {
        meshNonFlammableAsh.vertices = verticesNonFlammableAsh.ToArray();
        meshNonFlammableAsh.triangles = trianglesNonFlammableAsh.ToArray();
        meshNonFlammableAsh.RecalculateNormals();
        meshNonFlammableAsh.RecalculateTangents();
        meshNonFlammableAsh.RecalculateBounds();
        meshFilterNonFlammableAsh.mesh = meshNonFlammableAsh;
    }

    /// <summary>
    /// Updates visual component of fire on ground when FireballAbility upgrades.
    /// NOT YET IMPLEMENTED
    /// </summary>
    public void UpdateFire(float groundFireRadius)
    {
        return;

        fireWidth = groundFireRadius / 8f;

        return;
        meshRendererFlammable.material.SetFloat("_BladeWidth", groundFireRadius / 4f);
        meshRendererFlammable.material.SetFloat("_BladeHeight", groundFireRadius / 3f);
        meshRendererNonFlammable.material.SetFloat("_BladeWidth", groundFireRadius / 8f);
        meshRendererNonFlammable.material.SetFloat("_BladeHeight", groundFireRadius / 12f);

        Material oldMaterialNonFlammableAsh = meshRendererFlammableAsh.material;
        Material oldMaterialFlammableAsh = meshRendererNonFlammableAsh.material;

        // Set FlammableAsh and NonFlamableAsh to new game objects to keep ashes
        gameObjectFlammableAsh = new GameObject("FlammableAsh");
        gameObjectNonFlammableAsh = new GameObject("NonFlammableAsh");
        gameObjectFlammableAsh.transform.parent = transform;
        gameObjectNonFlammableAsh.transform.parent = transform;
        meshFlammableAsh = new Mesh();
        meshNonFlammableAsh = new Mesh();
        meshFilterFlammableAsh = gameObjectFlammableAsh.AddComponent<MeshFilter>();
        meshFilterNonFlammableAsh = gameObjectNonFlammableAsh.AddComponent<MeshFilter>();
        meshRendererFlammableAsh = gameObjectFlammableAsh.AddComponent<MeshRenderer>();
        meshRendererNonFlammableAsh = gameObjectNonFlammableAsh.AddComponent<MeshRenderer>();

        // new ashes for new fire
        meshRendererFlammableAsh.material = oldMaterialFlammableAsh;
        meshRendererFlammableAsh.material.SetFloat("_BladeWidth", groundFireRadius / 4f);
        meshRendererFlammableAsh.material.SetFloat("_BladeHeight", groundFireRadius / 3f);
        meshRendererNonFlammableAsh.material = oldMaterialNonFlammableAsh;
        meshRendererNonFlammableAsh.material.SetFloat("_BladeWidth", groundFireRadius / 8f);
        meshRendererNonFlammableAsh.material.SetFloat("_BladeHeight", groundFireRadius / 12f);
    }

    /// <summary>
    /// Grabs all components and initializes all meshes.
    /// </summary>
    private void Start()
    {
        meshFlammable = new Mesh();
        meshNonFlammable = new Mesh();
        meshFlammableAsh = new Mesh();
        meshNonFlammableAsh = new Mesh();
        meshFilterFlammable = gameObjectFlammable.GetComponent<MeshFilter>();
        meshFilterNonFlammable = gameObjectNonFlammable.GetComponent<MeshFilter>();
        meshFilterFlammableAsh = gameObjectFlammableAsh.GetComponent<MeshFilter>();
        meshFilterNonFlammableAsh = gameObjectNonFlammableAsh.GetComponent<MeshFilter>();
        meshRendererFlammable = gameObjectFlammable.GetComponent<MeshRenderer>();
        meshRendererNonFlammable = gameObjectNonFlammable.GetComponent<MeshRenderer>();
        meshRendererFlammableAsh = gameObjectFlammableAsh.GetComponent<MeshRenderer>();
        meshRendererNonFlammableAsh = gameObjectNonFlammableAsh.GetComponent<MeshRenderer>();
        
        damageByFire = gameObjectFlammable.GetComponent<DamageByFire>();
    }
}
