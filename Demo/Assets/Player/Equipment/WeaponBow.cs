using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBow : MonoBehaviour
{
    private PlayerAttack playerAttack;
    public Transform mouseRot;
    public GameObject arrow;
    // Start is called before the first frame update
    void Start()
    {
        playerAttack = GetComponent<PlayerAttack>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WeaponAttack(string weapon)
    {
        if (weapon == "Bow")
        {
            var newArrow = Instantiate(arrow, mouseRot.transform.position, Quaternion.identity);
            newArrow.transform.rotation = mouseRot.transform.rotation;
            newArrow.transform.parent = null;
            playerAttack.cooldown = 0.4f;
        }
    }
}
