using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasBillboard : MonoBehaviour
{
    private Camera main_camera;
    private void Start()
    {
        main_camera = Camera.main;

        transform.localScale = new Vector3(1f / 5.4f, 1f / 5.4f * SpriteInitializer.y_scale, 1f / 5.4f);
    }

    private void LateUpdate()
    {
        Vector3 vec_forward = main_camera.transform.forward;
        vec_forward.y = 0f;

        transform.forward = vec_forward;
    }
}
