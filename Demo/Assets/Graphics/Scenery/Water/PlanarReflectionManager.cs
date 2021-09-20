using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarReflectionManager : MonoBehaviour
{
    private Camera reflection_camera;
    private Camera m_camera;

    private Transform camera_focus_point;

    [SerializeField] private float camera_distance = 125f;

    private void Awake()
    {
        m_camera = Camera.main;
        reflection_camera = GetComponent<Camera>();
        camera_focus_point = new GameObject("reflection_focus_point").transform;
        camera_focus_point.parent = transform.parent;
    }

    /*
    private Matrix4x4 CopyCameraMatrix(Camera camera)
    {
        return camera.worldToCameraMatrix;
    }
    */
    /*
    private void RotateMatrix(ref Matrix4x4 matrix4x4, Quaternion quaternion)
    {
        Matrix4x4 matrix4x4_rotation = Matrix4x4.Rotate(quaternion);
        matrix4x4 *= matrix4x4_rotation;
    }
    */
    /*
    public static Quaternion ExtractRotation(Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public static Vector3 ExtractPosition(Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }

    public static Vector3 ExtractScale(Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
    */

    public void SetCameraNearClippingPlane()
    {
        Vector4 clipPlaneWorldSpace = new Vector4(0f, 1f, 0f, -Water.water_level);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(reflection_camera.worldToCameraMatrix)) * clipPlaneWorldSpace;
        reflection_camera.projectionMatrix = Camera.main.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }
    
    public Matrix4x4 RotationMatrixAroundAxis(Vector3 point, Vector3 axis, float rotation)
    {
        return Matrix4x4.TRS(-point, Quaternion.AngleAxis(rotation, axis), Vector3.one)
             * Matrix4x4.TRS(point, Quaternion.identity, Vector3.one);
    }
    
    public void CopyMatrix(Matrix4x4 matrix4X4, Transform transform)
    {
        this.transform.localRotation = transform.rotation;
        this.transform.position = transform.position;

        reflection_camera.worldToCameraMatrix = matrix4X4;
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

        Vector3 middle = (top_right_position + top_left_position + bot_right_position + bot_left_position) * 0.25f;

        reflection_camera.transform.position = middle;

        //Matrix4x4 world_to_camera_matrix = Matrix4x4.Ortho(-10, 10, -10, 10, 0, 10);
        //reflection_camera.worldToCameraMatrix = world_to_camera_matrix;
    }

    public void RotateAround(Vector3 point, Vector3 axis)
    {
        transform.RotateAround(point, axis, -60f);

        // currently "best option"
        //Matrix4x4 rot_matrix4x4 = RotationMatrixAroundAxis(point, axis, 60f);
        //reflection_camera.worldToCameraMatrix = reflection_camera.worldToCameraMatrix * rot_matrix4x4;
        
        
        
        
        
        
        //reflection_camera.worldToCameraMatrix = Matrix4x4.Inverse(reflection_camera.worldToCameraMatrix);

        //Matrix4x4 matrix4X4 = reflection_camera.worldToCameraMatrix;
        //reflection_camera.transform.position = ExtractPosition(matrix4X4);
        //reflection_camera.transform.rotation = ExtractRotation(matrix4X4);

        //reflection_camera.worldToCameraMatrix *= RotationMatrixAroundAxis(point, axis, 60f);
        //reflection_camera.worldToCameraMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(30f, 0, 0), Vector3.one);


        //reflection_camera.transform.RotateAround(point, axis, 30f);

        //reflection_camera.worldToCameraMatrix = reflection_camera.worldToCameraMatrix * Matrix4x4.Rotate(quaternion);
    }

    private void OnPreCull()
    {
        //PixelPerfectCameraRotation.MoveCamera(ref reflection_camera, camera_focus_point, new Vector3(-30f, 0f, 0f), camera_distance);
        //Vector2 camera_offset = PixelPerfectCameraRotation.PixelSnap(ref reflection_camera);

        //Matrix4x4 matrix4x4 = CopyCameraMatrix(m_camera);
        //RotateMatrix(ref matrix4x4, Quaternion.Euler(60, 0, 0));
        //reflection_camera.worldToCameraMatrix = matrix4x4;

        //reflection_camera.transform.RotateAround(m_camera.transform.right, 30f);
        //reflection_camera.worldToCameraMatrix = m_camera.worldToCameraMatrix;

        //SetCameraNearClippingPlane();
    }
}
