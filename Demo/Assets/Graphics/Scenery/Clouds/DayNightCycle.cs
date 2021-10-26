using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public static float time;
    [SerializeField] private AnimationCurve time_curve;

    [SerializeField] private Vector3 rotation_speed;
    [SerializeField] private Vector3 rotation_snap;
    [SerializeField] private Vector3 current_rotation_euler;

    private GameObject sun;
    private GameObject moon;

    private void Start()
    {
        sun = transform.GetChild(0).gameObject;
        moon = transform.GetChild(1).gameObject;
        current_rotation_euler = transform.rotation.eulerAngles;
    }

    public static Vector3 DivideVector3(Vector3 numerator, Vector3 denominator)
    {
        return new Vector3(numerator.x / denominator.x, numerator.y / denominator.y, numerator.z / denominator.z);
    }

    private void Update()
    {
        current_rotation_euler += rotation_speed * Time.deltaTime;
        Vector3 rounded_rotation = Vector3.Scale(Vector3Int.RoundToInt(DivideVector3(current_rotation_euler, rotation_snap)), rotation_snap);
        transform.rotation = Quaternion.Euler(rounded_rotation);

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
