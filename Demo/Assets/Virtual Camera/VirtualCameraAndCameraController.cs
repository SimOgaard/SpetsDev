using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// alike pixelperfect camera 2d
public class VirtualCameraAndCameraController : MonoBehaviour
{

    private Transform virtual_camera_transform;
    private Transform camera_transform;
    private Transform pivot_transform;

    private Vector3 local_rotation;

    [SerializeField]
    private float mouse_sensitivity = 8f;
    [SerializeField]
    private float move_sensitivity = 4f;


    private void Start()
    {
        camera_transform = transform;
        pivot_transform = transform.parent;
        virtual_camera_transform = pivot_transform.parent.GetChild(1).GetChild(0);

        local_rotation.y = Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad));
        UpdateCameraRotation();
    }

    private void UpdateCameraRotation()
    {
        float input_rotation = Input.GetAxis("Mouse X") * mouse_sensitivity;
        local_rotation.x += Input.GetAxis("Mouse X") * mouse_sensitivity * Time.deltaTime;

        pivot_transform.rotation = Quaternion.Euler(local_rotation.y, local_rotation.x, 0);
    }

    private void MoveCamera()
    {
        // moves camera
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);

        pivot_transform.position += movement * move_sensitivity * Time.deltaTime;
    }

    private void ResetCamera()
    {
        pivot_transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        // resets virtual screen
    }

    private void MoveVirtualScreen()
    {
        // moves virtual screen
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);

        virtual_camera_transform.position -= movement * move_sensitivity * Time.deltaTime * 5f;
    }

    private void ResetVirtualScreen()
    {
        virtual_camera_transform.position = new Vector3(0.0f, 0.0f, 1.0f);
        // resets virtual screen
    }

    private void SnapCamera()
    {
        // snaps camera to pixels
        ResetVirtualScreen();
    }

    private void LateUpdate()
    {
        if (Input.GetButton("Fire1"))
        {
            //UpdateCameraRotation();
            MoveCamera();
        }
        else
        {
            MoveVirtualScreen();
        }

        if (Input.GetKey(KeyCode.R))
        {
            ResetVirtualScreen();
            ResetCamera();
        }
    }
}
