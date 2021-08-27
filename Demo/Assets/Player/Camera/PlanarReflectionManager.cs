using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflectionManager : MonoBehaviour
{
    private Camera reflection_camera;
    private Camera this_camera;
    private RenderTexture render_texture;
    private Transform reflection_plain;

    private void Start()
    {
        this_camera = GetComponent<Camera>();
        render_texture = new RenderTexture(this_camera.pixelWidth, this_camera.pixelHeight, 24);
        Shader.SetGlobalTexture("_WaterReflectionTexture", render_texture);

        reflection_camera = NormalsReplacementShader.CopyCamera(this_camera, render_texture, transform, "ReflectionCamera");
        reflection_plain = GameObject.Find("water").transform;
    }

    private void SetCameraToMirror()
    {
        Vector3 camera_direction_world_space = this_camera.transform.forward;
        Vector3 camera_up_world_space = this_camera.transform.up;
        Vector3 camera_position_world_space = this_camera.transform.position;

        Vector3 camera_direction_plain_space = reflection_plain.InverseTransformDirection(camera_direction_world_space);
        Vector3 camera_up_plane_space = reflection_plain.InverseTransformDirection(camera_up_world_space);
        Vector3 camera_position_plain_space = reflection_plain.InverseTransformPoint(camera_position_world_space);

        camera_direction_plain_space.y *= -1f;
        camera_up_plane_space.y *= -1f;
        camera_position_plain_space.y *= -1f;

        camera_direction_world_space = reflection_plain.InverseTransformDirection(camera_direction_plain_space);
        camera_up_world_space = reflection_plain.InverseTransformDirection(camera_up_plane_space);
        camera_position_world_space = reflection_plain.InverseTransformPoint(camera_position_plain_space);

        reflection_camera.transform.position = camera_position_world_space;
        reflection_camera.transform.LookAt(camera_position_world_space + camera_direction_world_space, camera_up_world_space);
    }

    private void SetCameraNearClippingPlane()
    {
        Vector4 clipPlaneWorldSpace = new Vector4(0f, 1f, 0f, -reflection_plain.position.y);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(reflection_camera.worldToCameraMatrix)) * clipPlaneWorldSpace;
        reflection_camera.projectionMatrix = Camera.main.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    private void OnPreCull()
    {
        SetCameraToMirror();
        SetCameraNearClippingPlane();
    }

    private void RenderReflection()
    {

        reflection_camera.Render();
    }
}
