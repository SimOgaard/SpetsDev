using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalsReplacementShader : MonoBehaviour
{
    [SerializeField] private Shader normalsShader;

    private RenderTexture renderTexture;
    private new Camera camera;

    private void Start()
    {
        PixelPerfectCameraRotation thisCamera = GetComponent<PixelPerfectCameraRotation>();

        renderTexture = new RenderTexture(PixelPerfectCameraRotation.widthExtended, PixelPerfectCameraRotation.heightExtended, 24);
        renderTexture.filterMode = FilterMode.Point;
        Shader.SetGlobalTexture("_CameraNormalsTexture", renderTexture);

        camera = CopyCamera(thisCamera, transform.parent, "Normals camera", 2);
        camera.targetTexture = renderTexture;
        camera.SetReplacementShader(normalsShader, "RenderType");
        camera.gameObject.AddComponent<CopyCameraPosition>();

        thisCamera.nCamera = camera;
    }

    /// <summary>
    /// Setup a copy of given camera.
    /// </summary>
    public static Camera CopyCamera(PixelPerfectCameraRotation referenceCamera, Transform parrent, string name, int depth)
    {
        GameObject copy = new GameObject(name);
        Camera camera = copy.AddComponent<Camera>();
        camera.CopyFrom(referenceCamera.mCamera);
        camera.transform.SetParent(parrent);
        camera.depth = referenceCamera.mCamera.depth - depth;
        return camera;
    }
}
