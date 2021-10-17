using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public static float time;
    [SerializeField] private AnimationCurve time_curve;

    [SerializeField] private float rotation_speed = 1f;
    private GameObject sun;
    private GameObject moon;

    private void Start()
    {
        sun = transform.GetChild(0).gameObject;
        moon = transform.GetChild(1).gameObject;
    }

    private void Update()
    {
        transform.RotateAround(transform.up, rotation_speed * Time.deltaTime);

        float x = Vector3.Dot(transform.forward, Vector3.up);
        time = time_curve.Evaluate(x);
        Shader.SetGlobalFloat("_DayNightTime", time);

        if (sun.transform.forward.y < 0 && !sun.activeInHierarchy)
        {
            sun.SetActive(true);
            moon.SetActive(false);
        }
        else if (moon.transform.forward.y < 0 && !moon.activeInHierarchy)
        {
            moon.SetActive(true);
            sun.SetActive(false);
        }
    }
}
