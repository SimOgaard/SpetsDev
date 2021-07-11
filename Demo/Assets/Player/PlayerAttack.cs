using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float cooldown;
    public float spellcooldown;
    public float ultimateCooldown; //Cooldowns are tracked in this script
    private string weapon = "Bow";
    private string spell = "Fireball";
    private string ultimate = "Pause";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (cooldown >= 0)
        {
            cooldown -= Time.deltaTime; 
        }
        //<summary>
        //Depending on keypress, this object will send different messages that will call any funcions with matching names in components attached to the game object.
        //<summary>
        else {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                gameObject.SendMessage("WeaponAttack", weapon);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                gameObject.SendMessage("SpellAttack", spell);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                gameObject.SendMessage("UltimateAttack", ultimate);
            }
        }
    }

}
