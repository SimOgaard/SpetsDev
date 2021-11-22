using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Depending on keypress, this object will send different messages that will call any funcions with matching names in components attached to the game object.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    /// <summary>
    /// Continually checks for key presses.
    /// </summary>
    private void Update()
    {
        if (PlayerInventory.weapon != null)
        {
            if (PlayerInput.GetKeyDown(PlayerInput.use_weapon))
            {
                PlayerInventory.weapon.UsePrimary();
            }
            else if (PlayerInput.GetKeyUp(PlayerInput.use_weapon))
            {
                PlayerInventory.weapon.StopPrimary();
            }
        }

        if (PlayerInventory.ability != null)
        {
            if (PlayerInput.GetKeyDown(PlayerInput.use_ability))
            {
                PlayerInventory.ability.UsePrimary();
            }
            else if (PlayerInput.GetKeyUp(PlayerInput.use_ability))
            {
                PlayerInventory.ability.StopPrimary();
            }
        }

        if (PlayerInventory.ultimate != null)
        {
            if (PlayerInput.GetKeyDown(PlayerInput.use_ultimate))
            {
                PlayerInventory.ultimate.UsePrimary();
            }
            else if (PlayerInput.GetKeyUp(PlayerInput.use_ultimate))
            {
                PlayerInventory.ultimate.StopPrimary();
            }
        }
    }
}
