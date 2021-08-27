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
        Camera this_camera = GetComponent<Camera>();

        render_texture = new RenderTexture(this_camera.pixelWidth, this_camera.pixelHeight, 24);
        Shader.SetGlobalTexture("_CameraNormalsTexture", render_texture);

        camera = CopyCamera(this_camera, render_texture, transform, "Normals camera");
        camera.SetReplacementShader(normals_shader, "RenderType");
    }

    /// <summary>
    /// Setup a copy of given camera.
    /// </summary>
    public static Camera CopyCamera(Camera reference_camera, RenderTexture render_texture, Transform parrent, string name)
    {
        GameObject copy = new GameObject(name);
        Camera camera = copy.AddComponent<Camera>();
        camera.CopyFrom(reference_camera);
        camera.transform.SetParent(parrent);
        camera.targetTexture = render_texture;
        camera.depth = reference_camera.depth - 1;
        return camera;
    }
}
