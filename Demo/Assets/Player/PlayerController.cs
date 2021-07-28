using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parrent controller of player.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public enum Status { idle, walking, sprinting, sliding, using_weapon, using_ability, using_ultimate }
    public Status status;

    public LayerMask collision_layer;

    private PlayerMovement movement;
    private PlayerInput player_input;

    private Vector3 move_direction_normalized;

    /// <summary>
    /// Changes current status of player.
    /// </summary>
    public void ChangeStatus(Status s)
    {
        if (status == s)
        {
            return;
        }
        status = s;
    }

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        player_input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        move_direction_normalized = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        UpdateMovingStatus();
    }

    private void UpdateMovingStatus()
    {
        if (move_direction_normalized.sqrMagnitude > 0.02f)
        {
            ChangeStatus((Input.GetKey(player_input.sprint_key)) ? Status.sprinting : Status.walking);
        }
        else
        {
            ChangeStatus(Status.idle);
        }
    }

    public bool is_sprinting()
    {
        return (status == Status.sprinting);
    }
}
