using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    [SerializeField]
    private float cameraDistance = 200;

    private float cameraDistanceToY = Mathf.Sin(Mathf.Deg2Rad * 30f);
    private float cameraDistanceToZ = Mathf.Cos(Mathf.Deg2Rad * 30f);

    private Quaternion cameraQuaternionRotation = Quaternion.Euler(30f, 0f, 0f);

    private Camera mCamera;

    private void SetCameraRotation()
    {
        mCamera.transform.rotation = cameraQuaternionRotation;
    }

    private void MoveCamera()
    {
        Vector3 cameraPos = new Vector3(
            0f,
            50f + cameraDistance * cameraDistanceToY,
            0f - cameraDistance * cameraDistanceToZ
        );
        mCamera.transform.position = cameraPos;
    }

    private void Start()
    {
        mCamera = GetComponent<Camera>();
        SetCameraRotation();
        MoveCamera();
    }
}
