using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWorldManager : MonoBehaviour
{
    [SerializeField] private float playerMaxDistanceFromOrigo = 50f;
    private CharacterController playerCharacterController;

    public static Vector3 worldOffset;

    private void Awake()
    {
        playerCharacterController = Global.playerTransform.GetComponent<CharacterController>();
    }

    private void MoveDirection(Vector3 direction)
    {
        direction *= playerMaxDistanceFromOrigo;

        worldOffset += direction;
        Shader.SetGlobalVector("_WorldOffset", worldOffset);

        playerCharacterController.enabled = false;
        foreach (Transform child in transform)
        {
            child.localPosition += direction;
        }
        playerCharacterController.enabled = true;
    }

    private void Update()
    {
        return;

        if (Global.playerTransform.position.x > playerMaxDistanceFromOrigo)
        {
            MoveDirection(Vector3.left);
        }
        else if (Global.playerTransform.position.x < -playerMaxDistanceFromOrigo)
        {
            MoveDirection(Vector3.right);
        }
        else if (Global.playerTransform.position.z > playerMaxDistanceFromOrigo)
        {
            MoveDirection(Vector3.back);
        }
        if (Global.playerTransform.position.z < -playerMaxDistanceFromOrigo)
        {
            MoveDirection(Vector3.forward);
        }
    }
}
