using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic movement so i am able to work with the camera.
/// Movement should be ordinary 3D movement, does not need to be translated to 2D isometric movement.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rigid_body;
    [SerializeField]
    private float move_sensitivity = 10f;
    [HideInInspector]
    public Vector3 movement_normal;

    private void MovePlayer()
    {
        movement_normal = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 movement = movement_normal * move_sensitivity * Time.deltaTime;

        rigid_body.MovePosition(movement + transform.position);
    }

    private void Awake()
    {
        rigid_body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        MovePlayer();
    }
}
