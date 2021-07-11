using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellFireball : MonoBehaviour
{
    private PlayerAttack playerAttack;
    public GameObject fireball;
    public Transform mouseRot;
    // Start is called before the first frame update
    void Start()
    {
        playerAttack = GetComponent<PlayerAttack>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpellAttack(string spell)
    {
        if (spell == "Fireball")
        {
            var newFireball = Instantiate(fireball, mouseRot.transform.position, Quaternion.identity);
            newFireball.transform.rotation = mouseRot.transform.rotation;
            newFireball.transform.parent = null;
            playerAttack.cooldown = 0.4f;
            playerAttack.spellcooldown = 8;
        }
    } 
}
