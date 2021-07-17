using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float cooldown;
    public float spellcooldown;
    public float ultimateCooldown; //Cooldowns are tracked in this script
    private PlayerInventory playerInventory;
    // Start is called before the first frame update
    void Start()
    {
        playerInventory = GameObject.Find("EquipmentsInInventory").GetComponent<PlayerInventory>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cooldown >= 0)
        {
            cooldown -= Time.deltaTime; 
        }
        //Depending on keypress, this object will send different messages that will call any funcions with matching names in components attached to the game object.
        else {
            if (Input.GetKey(KeyCode.Mouse0) && playerInventory.weapon != null)
            {
                playerInventory.weapon.UsePrimary();
            }
            else if (Input.GetKey(KeyCode.Mouse1) && playerInventory.ability != null)
            {
                playerInventory.ability.UsePrimary();
            }
            else if (Input.GetKeyDown(KeyCode.Q) && playerInventory.ultimate != null)
            {
                playerInventory.ultimate.UsePrimary();
            }
        }
    }

}
