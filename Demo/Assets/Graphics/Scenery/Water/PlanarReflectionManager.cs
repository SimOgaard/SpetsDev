using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflectionManager : MonoBehaviour
{
    private Camera reflection_camera;

    [SerializeField] private float camera_far_clipping_plane = 150f;

    private void Awake()
    {
        reflection_camera = GetComponent<Camera>();
        reflection_camera.farClipPlane = camera_far_clipping_plane;
    }

    public void SetCameraNearClippingPlane()
    {
        Vector4 clipPlaneWorldSpace = new Vector4(0f, 1f, 0f, -Water.water_level);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(reflection_camera.worldToCameraMatrix)) * clipPlaneWorldSpace;
        reflection_camera.projectionMatrix = Camera.main.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }
  
    public void ConstructMatrix4X4Ortho(Ray bot_right_ray, Ray bot_left_ray, float camera_orthographic_height, Quaternion camera_rotation)
    {
        // Construct plane representing the water level
        Plane plane = new Plane(Vector3.up, -Water.water_level);

        // Assign floats of ray distance for bottom corners
        float bot_right_distance;
        float bot_left_distance;

        // Raycast to plain
        plane.Raycast(bot_right_ray, out bot_right_distance);
        plane.Raycast(bot_left_ray, out bot_left_distance);

        // Get each position of all four camera corners where they hit the water plain
        Vector3 top_right_position = bot_right_ray.GetPoint(bot_right_distance);
        Vector3 top_left_position = bot_left_ray.GetPoint(bot_left_distance);

        // Get sidebar of new camera
        Vector3 camera_sidebar_direction = camera_rotation * Vector3.down;
        Vector3 camera_sidebar = camera_sidebar_direction * camera_orthographic_height;

        // Create the vectors representing bottom corners of our new worldToCameraMatrix.
        Vector3 bot_right_position = top_right_position + camera_sidebar;
        Vector3 bot_left_position = top_left_position + camera_sidebar;

        // Get middle point of our four points
        Vector3 middle = (top_right_position + top_left_position + bot_right_position + bot_left_position) * 0.25f;

        // Assign that point to the camera position and rotate camera
        reflection_camera.transform.position = middle;
        reflection_camera.transform.rotation = camera_rotation;

        //Matrix4x4 world_to_camera_matrix = Matrix4x4.Ortho(-10, 10, -10, 10, 0, 10); // You could probably create an ortho matrix from theese cords but i do not now how
    }
}
