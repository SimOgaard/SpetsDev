using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private float ambient;
    [SerializeField] private AnimationCurve ambient_light;

    [SerializeField] private float darkest;
    [SerializeField] private AnimationCurve darkest_value;

    [SerializeField] private float water_offset;
    [SerializeField] private AnimationCurve water_col_offset;

    [SerializeField] private Vector3 rotation_speed;
    [SerializeField] private Vector3 rotation_snap;
    public Vector3 current_rotation_euler;

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

        // Reposition directional light to be over player to keep shadows


        Vector3 test = MousePoint.PositionRayPlane(Vector3.zero, -transform.forward, Global.camera_focus_point_transform.position, transform.forward);
        //Vector3 new_pos = Global.camera_focus_point_transform.position;
        transform.position = test;

        float x = Vector3.Dot(transform.forward, Vector3.up);
        ambient = ambient_light.Evaluate(x);
        Shader.SetGlobalFloat("_Ambient", ambient);

        darkest = darkest_value.Evaluate(x);
        Shader.SetGlobalFloat("_Darkest", darkest);

        water_offset = water_col_offset.Evaluate(x);
        Global.Materials.water_material.SetFloat("_WaterColOffset", water_offset);

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
