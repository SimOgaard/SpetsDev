using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private Text fps_text;
    [SerializeField] private float hud_refresh_rate = 0.1f;
    private float timer = 0f;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
    }

    private void Update()
    {
        if (Time.unscaledTime > timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            fps_text.text = "FPS: " + fps;
            timer = Time.unscaledTime + hud_refresh_rate;
        }
    }
}
