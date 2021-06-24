using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteInitializer : MonoBehaviour
{
    [SerializeField]
    private Sprite debugger;

    private Quaternion rotation = Quaternion.Euler(Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad)), 0f, 0f);

    private void Start()
    {
        GameObject game_object = new GameObject();
        game_object.AddComponent<SpriteRenderer>();
        game_object.GetComponent<SpriteRenderer>().sprite = debugger;
        game_object.transform.parent = transform;

        game_object.transform.rotation = rotation;
    }
}
