using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this SpriteInitializer places it to worldspace and snaps to nearest pixel, should not be used for character
public class SpriteInitializer : MonoBehaviour
{
    [SerializeField]
    private Sprite debugger;

    private void InitializeRotated()
    {
        GameObject game_object = new GameObject();
        game_object.AddComponent<SpriteRenderer>();
        game_object.GetComponent<SpriteRenderer>().sprite = debugger;
        game_object.transform.parent = transform;

        Quaternion rotation = Quaternion.Euler(Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad)), 0f, 0f);
        game_object.transform.rotation = rotation;
    }

    private void InitializeUpright()
    {
        GameObject game_object = new GameObject();
        game_object.AddComponent<SpriteRenderer>();
        game_object.GetComponent<SpriteRenderer>().sprite = debugger;
        game_object.transform.parent = transform;

        float y_scale = 0.5f / Mathf.Sin(Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad)));
        game_object.transform.localScale = new Vector3(1f, y_scale, 1f);
        game_object.transform.localPosition = new Vector3(0f, 2.5f, 0f);
    }

    private void Start()
    {
        InitializeUpright();
    }
}
