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
            if (PlayerInput.GetKeyDown(PlayerInput.useWeapon))
            {
                PlayerInventory.weapon.UsePrimary();
            }
            else if (PlayerInput.GetKeyUp(PlayerInput.useWeapon))
            {
                PlayerInventory.weapon.StopPrimary();
            }
        }

        if (PlayerInventory.ability != null)
        {
            if (PlayerInput.GetKeyDown(PlayerInput.useAbility))
            {
                PlayerInventory.ability.UsePrimary();
            }
            else if (PlayerInput.GetKeyUp(PlayerInput.useAbility))
            {
                PlayerInventory.ability.StopPrimary();
            }
        }

        if (PlayerInventory.ultimate != null)
        {
            if (PlayerInput.GetKeyDown(PlayerInput.useUltimate))
            {
                PlayerInventory.ultimate.UsePrimary();
            }
            else if (PlayerInput.GetKeyUp(PlayerInput.useUltimate))
            {
                PlayerInventory.ultimate.StopPrimary();
            }
        }
    }
}
