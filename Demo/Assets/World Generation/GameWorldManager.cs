using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWorldManager : MonoBehaviour
{
    [SerializeField] private float player_max_distance_from_origo = 50f;
    private Transform player_transform;
    private CharacterController player_character_controller;

    public static Vector3 world_offset;

    private void Awake()
    {
        player_transform = GameObject.Find("Player").transform;
        player_character_controller = player_transform.GetComponent<CharacterController>();
    }

    private void MoveDirection(Vector3 direction)
    {
        direction *= player_max_distance_from_origo;

        world_offset += direction;
        Shader.SetGlobalVector("_WorldOffset", world_offset);

        player_character_controller.enabled = false;
        foreach (Transform child in transform)
        {
            child.localPosition += direction;
        }
        player_character_controller.enabled = true;
    }

    private void Update()
    {
        return;

        if (player_transform.position.x > player_max_distance_from_origo)
        {
            MoveDirection(Vector3.left);
        }
        else if (player_transform.position.x < -player_max_distance_from_origo)
        {
            MoveDirection(Vector3.right);
        }
        else if (player_transform.position.z > player_max_distance_from_origo)
        {
            MoveDirection(Vector3.back);
        }
        if (player_transform.position.z < -player_max_distance_from_origo)
        {
            MoveDirection(Vector3.forward);
        }
    }
}
