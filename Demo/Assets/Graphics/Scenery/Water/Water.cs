using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Water : MonoBehaviour
{
    public static float waterLevel = 0f;
    public static float buoyancyForce = 1f;

    [HideInInspector] public WaterSettings waterSettings;

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif
        float waterLevelWave = waterSettings.bobingAmplitude * Mathf.Sin(Time.time * waterSettings.bobingFrequency);
        Water.waterLevel = waterSettings.level + waterLevelWave;

        Vector3 newPos = Global.cameraFocusPointTransform.position;
        newPos.y = waterLevel;
        transform.position = newPos;
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

    public void Init(WaterSettings waterSettings, float width, float height, Transform parrent)
    {
        gameObject.layer = Layer.water;

        this.waterSettings = waterSettings;
        /*
        CurveCreator.AddCurveTexture(ref material, curveColor, "_WaterCurveTexture");
        Texture2D alphaTexture = CurveCreator.CreateCurveTexture(curveAlpha);
        material.SetTexture("_WaterCurveAlpha", alphaTexture);
        */
        gameObject.name = "water";
        transform.parent = parrent;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Water.waterLevel = waterSettings.level;
        gameObject.AddComponent<MeshFilter>().mesh = BuildQuad(width, height);
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = waterSettings.waterMaterial.material;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        //this.material = material;
    }
}
