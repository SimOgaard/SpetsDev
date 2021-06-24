using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// pixel snaps
[RequireComponent(typeof(Camera))]
public class PixelSnapCameraController : MonoBehaviour
{
    [SerializeField]
    private float move_sensitivity = 1f;

    [SerializeField]
    private float camera_distance_origo_z = -200f;

    private float units_per_pixel = 40f / 216f;

    private Camera m_camera;
    private Transform camera_focus_point;
    private Rigidbody camera_focus_rigid_body;

    private Vector3 RoundToPixel(Vector3 position)
    {
        if (units_per_pixel == 0.0f)
            return position;

        Vector3 result;
        result.x = Mathf.Round(position.x / units_per_pixel) * units_per_pixel;
        result.y = Mathf.Round(position.y / units_per_pixel) * units_per_pixel;
        result.z = Mathf.Round(position.z / units_per_pixel) * units_per_pixel;

        return result;
    }

    private void PixelSnap()
    {
        Vector3 camera_position = m_camera.transform.rotation * m_camera.transform.position;
        Vector3 rounded_camera_position = RoundToPixel(camera_position);
        Vector3 offset = rounded_camera_position - camera_position;
        offset.z = -offset.z;
        Matrix4x4 offset_matrix = Matrix4x4.TRS(-offset, Quaternion.identity, new Vector3(1.0f, 1.0f, -1.0f));

        m_camera.worldToCameraMatrix = offset_matrix * m_camera.transform.worldToLocalMatrix;
    }

    private void UpdateCameraRotation()
    {
        m_camera.transform.rotation = Quaternion.Euler(Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad)), 0f, 0f);
    }

    private void MoveCamera()
    {
        Vector3 camera_pos = new Vector3(camera_focus_point.position.x, (camera_focus_point.position.z - camera_distance_origo_z) * Mathf.Sin(30f * Mathf.Deg2Rad) + camera_focus_point.position.y, camera_distance_origo_z);
        m_camera.transform.position = camera_pos;
    }

    private void MoveFocusPoint()
    {
        Vector3 movement_normal = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        Vector3 movement = movement_normal * move_sensitivity * Time.deltaTime;

        camera_focus_rigid_body.MovePosition(camera_focus_point.position + movement);
    }

    private void Awake()
    {
        camera_focus_point = transform.parent.GetChild(1);
        camera_focus_rigid_body = camera_focus_point.GetComponent<Rigidbody>();
        m_camera = GetComponent<Camera>();
    }

    private void Start()
    {
        UpdateCameraRotation();
    }

    private void LateUpdate()
    {
        MoveFocusPoint();
        MoveCamera();
    }

    private void OnPreCull()
    {
        PixelSnap();
    }
}
