using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WeaponBow : MonoBehaviour, Weapon.IWeapon
{
    public GameObject arrow;
    private Transform mouseRot;
    private PlayerAttack playerAttack;
    /// <summary>
    /// Destroys itself.
    /// </summary>
    public void Destroy()
    {
        Destroy(this);
    }

    /// <summary>
    /// Visualizes that transmission of this fucntion reached this child component.
    /// </summary>
    public void OnGround()
    {
        transform.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
    }

    /// <summary>
    /// Example of how this specific equipments can be used with character interactions.
    /// Should be called from master Equipment component.
    /// </summary>
    public void UsePrimary()
    {
        Debug.Log(mouseRot.position);
        Debug.Log(Quaternion.identity);
        if (transform.parent.gameObject.name == "EquipmentsInInventory")
        {
            var newArrow = Instantiate(arrow, mouseRot.position, Quaternion.identity);
            newArrow.transform.rotation = mouseRot.transform.rotation;
            newArrow.transform.parent = null;
            playerAttack.cooldown = 0.4f;
        }
    }

    /// <summary>
    /// Returns sword icon for ui element.
    /// </summary>
    private Sprite icon_sprite;
    public Sprite GetIconSprite()
    {
        return icon_sprite;
    }
    private void Start()
    {
        mouseRot = GameObject.Find("MouseRot").transform;
        arrow = Resources.Load<GameObject>("Prefabs/Arrow");
        playerAttack = GameObject.Find("Player").GetComponent<PlayerAttack>();
        icon_sprite = Resources.Load<Sprite>("Sprites/debugger");
    }
}


