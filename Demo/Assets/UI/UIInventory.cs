using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    [SerializeField]
    private GameObject current_weapon_UI_game_object;
    private Image current_weapon_UI_image;
    [SerializeField]
    private GameObject current_ability_UI_game_object;
    private Image current_ability_UI_image;
    [SerializeField]
    private GameObject current_ultimate_UI_game_object;
    private Image current_ultimate_UI_image;

    private Equipment.IEquipment current_weapon;
    private Equipment.IEquipment current_ability;
    private Equipment.IEquipment current_ultimate;

    public void ChangeWeapon(Equipment.IEquipment equipment)
    {
        current_weapon = equipment;
        current_weapon_UI_image.sprite = current_weapon.GetIconSprite();
        current_weapon_UI_image.color = Color.white;
    }
    public void ChangeAbility(Equipment.IEquipment equipment)
    {
        current_ability = equipment;
        current_ability_UI_image.sprite = current_ability.GetIconSprite();
        current_ability_UI_image.color = Color.white;
    }
    public void ChangeUltimate(Equipment.IEquipment equipment)
    {
        current_ultimate = equipment;
        current_ultimate_UI_image.sprite = current_ultimate.GetIconSprite();
        current_ultimate_UI_image.color = Color.white;
    }

    private void Start()
    {
        current_weapon_UI_image = current_weapon_UI_game_object.GetComponent<Image>();
        current_ability_UI_image = current_ability_UI_game_object.GetComponent<Image>();
        current_ultimate_UI_image = current_ultimate_UI_game_object.GetComponent<Image>();
    }
}
