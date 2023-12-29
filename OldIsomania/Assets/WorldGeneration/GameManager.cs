using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        MainCamera.mCamera = Camera.main;
        Screen.fullScreenMode = FullScreenMode.Windowed;
    }
}
