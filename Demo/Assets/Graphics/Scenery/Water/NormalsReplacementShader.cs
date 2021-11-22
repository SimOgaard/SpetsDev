using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalsReplacementShader : MonoBehaviour
{
    [SerializeField] private Shader normals_shader;

    private RenderTexture render_texture;
    private new Camera camera;

    private void Start()
    {
        PixelPerfectCameraRotation this_camera = GetComponent<PixelPerfectCameraRotation>();

        render_texture = new RenderTexture((int) PixelPerfectCameraRotation.resolution_extended.x,(int) PixelPerfectCameraRotation.resolution_extended.y, 24);
        render_texture.filterMode = FilterMode.Point;
        Shader.SetGlobalTexture("_CameraNormalsTexture", render_texture);

        camera = CopyCamera(this_camera, transform.parent, "Normals camera", 2);
        camera.targetTexture = render_texture;
        camera.SetReplacementShader(normals_shader, "RenderType");
        camera.gameObject.AddComponent<CopyCameraPosition>();

        this_camera.n_camera = camera;
    }

    /// <summary>
    /// Setup a copy of given camera.
    /// </summary>
    public static Camera CopyCamera(PixelPerfectCameraRotation reference_camera, Transform parrent, string name, int depth)
    {
        GameObject copy = new GameObject(name);
        Camera camera = copy.AddComponent<Camera>();
        camera.CopyFrom(reference_camera.m_camera);
        camera.transform.SetParent(parrent);
        camera.depth = reference_camera.m_camera.depth - depth;
        return camera;
    }
}
