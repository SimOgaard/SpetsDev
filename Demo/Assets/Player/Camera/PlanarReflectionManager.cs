using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflectionManager : MonoBehaviour
{
    private Camera reflection_camera;
    
    private Transform camera_focus_point;

    [SerializeField] private float camera_distance = 125f;

    private void Awake()
    {
        reflection_camera = GetComponent<Camera>();
        camera_focus_point = new GameObject("reflection_focus_point").transform;
        camera_focus_point.parent = transform.parent;
    }

    private void SetCameraNearClippingPlane()
    {
        Vector4 clipPlaneWorldSpace = new Vector4(0f, 1f, 0f, -Water.water_level);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(reflection_camera.worldToCameraMatrix)) * clipPlaneWorldSpace;
        reflection_camera.projectionMatrix = Camera.main.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    private void OnPreCull()
    {
        PixelPerfectCameraRotation.MoveCamera(ref reflection_camera, camera_focus_point, new Vector3(-30f, 0f, 0f), camera_distance);
        Vector2 camera_offset = PixelPerfectCameraRotation.PixelSnap(ref reflection_camera);
        SetCameraNearClippingPlane();
    }
}
