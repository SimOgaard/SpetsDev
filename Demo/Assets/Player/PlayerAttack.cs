using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerInventory player_inventory;
    private PlayerInput player_input;

    private void Start()
    {
        player_inventory = GameObject.Find("EquipmentsInInventory").GetComponent<PlayerInventory>();
        player_input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        //Depending on keypress, this object will send different messages that will call any funcions with matching names in components attached to the game object.
        if (player_inventory.weapon != null)
        {
            if (Input.GetKeyDown(player_input.use_weapon))
            {
                player_inventory.weapon.UsePrimary();
            }
            else if (Input.GetKeyUp(player_input.use_weapon))
            {
                player_inventory.weapon.StopPrimary();
            }
        }

        if (player_inventory.ability != null)
        {
            if (Input.GetKeyDown(player_input.use_ability))
            {
                player_inventory.ability.UsePrimary();
            }
            else if (Input.GetKeyUp(player_input.use_ability))
            {
                player_inventory.ability.StopPrimary();
            }
        }

        if (player_inventory.ultimate != null)
        {
            if (Input.GetKeyDown(player_input.use_ultimate))
            {
                player_inventory.ultimate.UsePrimary();
            }
            else if (Input.GetKeyUp(player_input.use_ultimate))
            {
                player_inventory.ultimate.StopPrimary();
            }
        }
    }
}
