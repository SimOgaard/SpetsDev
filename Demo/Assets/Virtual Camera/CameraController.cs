using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    private Transform camera_transform;
    private Transform pivot_transform;

    private Vector3 local_rotation;

    [SerializeField]
    private float camera_distance = 100f;
    [SerializeField]
    private float mouse_sensitivity = 0.75f;
    [SerializeField]
    private float orbit_dampening = 10f;

    [SerializeField]
    private float deg_test = 30f;

    private void Start()
    {
        local_rotation.y = Mathf.Rad2Deg * Mathf.Atan(Mathf.Sqrt(2f));
        local_rotation.y = Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad));
        //local_rotation.y = 30f;
        camera_transform = transform;
        pivot_transform = transform.parent;
    }

    private void LateUpdate()
    {
        local_rotation.y = Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(deg_test * Mathf.Deg2Rad));

        //local_rotation.x += Input.GetAxis("Mouse X") * mouse_sensitivity;

        Quaternion QT = Quaternion.Euler(local_rotation.y, local_rotation.x, 0);
        pivot_transform.rotation = Quaternion.Lerp(pivot_transform.rotation, QT, Time.deltaTime * orbit_dampening);
    }
}
