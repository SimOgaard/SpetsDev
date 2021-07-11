using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    [SerializeField]
    private float camera_distance = 200;

    private float camera_distance_to_y = Mathf.Sin(Mathf.Deg2Rad * 30f);
    private float camera_distance_to_z = Mathf.Cos(Mathf.Deg2Rad * 30f);

    private Quaternion camera_quaternion_rotation = Quaternion.Euler(30f, 0f, 0f);

    private Camera m_camera;

    private void SetCameraRotation()
    {
        m_camera.transform.rotation = camera_quaternion_rotation;
    }

    private void MoveCamera()
    {
        Vector3 camera_pos = new Vector3(
            0f,
            50f + camera_distance * camera_distance_to_y,
            0f - camera_distance * camera_distance_to_z
        );
        m_camera.transform.position = camera_pos;
    }

    private void Start()
    {
        m_camera = GetComponent<Camera>();
        SetCameraRotation();
        MoveCamera();
    }
}
