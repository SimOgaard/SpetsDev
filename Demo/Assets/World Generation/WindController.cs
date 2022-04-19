using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindController : MonoBehaviour
{
    [SerializeField] private Texture2D StartWindDistortionMap;
    private static Texture2D WindDistortionMap;
    public static Texture2D _WindDistortionMap {
        get { return WindDistortionMap; }
        set { WindDistortionMap = value; Shader.SetGlobalTexture("_WindDistortionMap", value); }
    }

    [SerializeField] private Vector4 StartWindDistortionMap_ST;
    private static Vector4 WindDistortionMap_ST;
    public static Vector4 _WindDistortionMap_ST
    {
        get { return WindDistortionMap_ST; }
        set { WindDistortionMap_ST = value; Shader.SetGlobalVector("_WindDistortionMap_ST", value); }
    }

    [SerializeField] private Vector2 StartWindFrequency;
    private static Vector2 WindFrequency;
    public static Vector2 _WindFrequency
    {
        get { return WindFrequency; }
        set { WindFrequency = value; Shader.SetGlobalVector("_WindFrequency", value); }
    }

    [SerializeField] private float StartWindStrength;
    private static float WindStrength;
    public static float _WindStrength
    {
        get { return WindStrength; }
        set { WindStrength = value; Shader.SetGlobalFloat("_WindStrength", value); }
    }

    private void Awake()
    {
        _WindDistortionMap = StartWindDistortionMap;
        _WindDistortionMap_ST = StartWindDistortionMap_ST;
        _WindFrequency = StartWindFrequency;
        _WindStrength = StartWindStrength;
    }

    private void Update()
    {
        //_WindStrength -= 0.1f;
    }
}
