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

        // Create a render texture matching the main camera's current dimensions.
        render_texture = new RenderTexture(this_camera.pixelWidth, this_camera.pixelHeight, 24);
        // Surface the render texture as a global variable, available to all shaders.
        Shader.SetGlobalTexture("_CameraNormalsTexture", render_texture);

        // Setup a copy of the camera to render the scene using the normals shader.
        GameObject copy = new GameObject("Normals camera");
        camera = copy.AddComponent<Camera>();
        camera.CopyFrom(this_camera);
        camera.transform.SetParent(transform);
        camera.targetTexture = render_texture;
        camera.SetReplacementShader(normals_shader, "RenderType");
        camera.depth = this_camera.depth - 1;
    }
}
