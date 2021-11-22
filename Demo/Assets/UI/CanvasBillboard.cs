using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasBillboard : MonoBehaviour
{
    private Camera main_camera;
    private void Start()
    {
        main_camera = Camera.main;

        float one_div = 1f / PixelPerfectCameraRotation.pixels_per_unit;
        transform.localScale = new Vector3(one_div, one_div * SpriteInitializer.y_scale, one_div);
    }

    private void LateUpdate()
    {
        Vector3 vec_forward = main_camera.transform.forward;
        vec_forward.y = 0f;

        transform.forward = vec_forward;
    }
}
