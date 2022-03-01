using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasBillboard : MonoBehaviour
{
    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;

        float oneDiv = 1f / PixelPerfectCameraRotation.pixelsPerUnit;
        transform.localScale = new Vector3(oneDiv / transform.lossyScale.x, oneDiv * SpriteInitializer.yScale / transform.lossyScale.y, oneDiv / transform.lossyScale.z);
    }

    private void LateUpdate()
    {
        Vector3 vecForward = mainCamera.transform.forward;
        vecForward.y = 0f;

        transform.forward = vecForward;
    }
}
