using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform pivot_transform;

    private Vector3 local_rotation;

    [SerializeField]
    private float mouse_sensitivity = 8f;
    [SerializeField]
    private float move_sensitivity = 4f;

    private void Start()
    {
        pivot_transform = transform.parent;

        local_rotation.y = Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad));
        UpdateCameraRotation();
    }

    private void UpdateCameraRotation()
    {
        local_rotation.x += Input.GetAxis("Mouse X") * mouse_sensitivity * Time.deltaTime;

        pivot_transform.rotation = Quaternion.Euler(local_rotation.y, local_rotation.x, 0);
    }

    private void MoveCamera()
    {
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);

        pivot_transform.position += movement * move_sensitivity * Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (Input.GetButton("Fire1"))
        {
            UpdateCameraRotation();
        }
        MoveCamera();
    }
}
