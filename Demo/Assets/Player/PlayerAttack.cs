using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Depending on keypress, this object will send different messages that will call any funcions with matching names in components attached to the game object.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    private PlayerInput player_input;

    private void Start()
    {
        player_input = GetComponent<PlayerInput>();
    }

    /// <summary>
    /// Continually checks for key presses.
    /// </summary>
    private void Update()
    {
        if (PlayerInventory.weapon != null)
        {
            if (Input.GetKeyDown(player_input.use_weapon))
            {
                PlayerInventory.weapon.UsePrimary();
            }
            else if (Input.GetKeyUp(player_input.use_weapon))
            {
                PlayerInventory.weapon.StopPrimary();
            }
        }

        if (PlayerInventory.ability != null)
        {
            if (Input.GetKeyDown(player_input.use_ability))
            {
                PlayerInventory.ability.UsePrimary();
            }
            else if (Input.GetKeyUp(player_input.use_ability))
            {
                PlayerInventory.ability.StopPrimary();
            }
        }

        if (PlayerInventory.ultimate != null)
        {
            if (Input.GetKeyDown(player_input.use_ultimate))
            {
                PlayerInventory.ultimate.UsePrimary();
            }
            else if (Input.GetKeyUp(player_input.use_ultimate))
            {
                PlayerInventory.ultimate.StopPrimary();
            }
        }
    }
}
