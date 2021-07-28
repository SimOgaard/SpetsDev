using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlls how equipment should be viewed in inventory.
/// Changes ui image for weapon, ability and ultimate.
/// </summary>
public class UIInventory : MonoBehaviour
{
    [SerializeField] private GameObject current_weapon_UI_game_object;
    private Image current_weapon_UI_image;
    private RectTransform current_weapon_rect_transform;
    public Material weapon_cooldown_mateiral;
    [SerializeField] private GameObject current_ability_UI_game_object;
    private Image current_ability_UI_image;
    private RectTransform current_ability_rect_transform;
    public Material ability_cooldown_mateiral;
    [SerializeField] private GameObject current_ultimate_UI_game_object;
    private Image current_ultimate_UI_image;
    private RectTransform current_ultimate_rect_transform;
    public Material ultimate_cooldown_mateiral;

    private Equipment.IEquipment current_weapon;
    private Equipment.IEquipment current_ability;
    private Equipment.IEquipment current_ultimate;

    [SerializeField] private Color background_color;

    /// <summary>
    /// Changes weapon ui image to given equipment sprite retrieved by GetIconSprite().
    /// </summary>
    public void ChangeWeapon(Equipment.IEquipment equipment)
    {
        current_weapon = equipment;
        current_weapon_UI_image.sprite = current_weapon.GetIconSprite();
    }
    /// <summary>
    /// Changes ability ui image to given equipment sprite retrieved by GetIconSprite().
    /// </summary>
    public void ChangeAbility(Equipment.IEquipment equipment)
    {
        current_ability = equipment;
        current_ability_UI_image.color = Color.white;
        current_ability_UI_image.sprite = current_ability.GetIconSprite();
        current_ability_rect_transform.sizeDelta = new Vector2(14f, 14f);
    }
    /// <summary>
    /// Changes ultimate ui image to given equipment sprite retrieved by GetIconSprite().
    /// </summary>
    public void ChangeUltimate(Equipment.IEquipment equipment)
    {
        current_ultimate = equipment;
        current_ultimate_UI_image.color = Color.white;
        current_ultimate_UI_image.sprite = current_ultimate.GetIconSprite();
        current_ultimate_rect_transform.sizeDelta = new Vector2(19f, 19f);
    }

    /// <summary>
    /// Retrieves each Image component for each equipment.
    /// Initializes ui images to init state.
    /// </summary>
    private void Start()
    {
        current_weapon_UI_image = current_weapon_UI_game_object.GetComponent<Image>();
        current_ability_UI_image = current_ability_UI_game_object.GetComponent<Image>();
        current_ultimate_UI_image = current_ultimate_UI_game_object.GetComponent<Image>();

        current_weapon_UI_image.sprite = Resources.Load<Sprite>("Sprites/debugger");
        current_ability_UI_image.color = background_color;
        current_ultimate_UI_image.color = background_color;

        current_weapon_rect_transform = current_weapon_UI_game_object.GetComponent<RectTransform>();
        current_ability_rect_transform = current_ability_UI_game_object.GetComponent<RectTransform>();
        current_ultimate_rect_transform = current_ultimate_UI_game_object.GetComponent<RectTransform>();

        current_ability_rect_transform.sizeDelta = new Vector2(12f, 12f);
        current_ultimate_rect_transform.sizeDelta = new Vector2(17f, 17f);

        weapon_cooldown_mateiral = current_weapon_UI_game_object.transform.GetChild(0).GetComponent<Image>().material;
        ability_cooldown_mateiral = current_ability_UI_game_object.transform.GetChild(0).GetComponent<Image>().material;
        ultimate_cooldown_mateiral = current_ultimate_UI_game_object.transform.GetChild(0).GetComponent<Image>().material;

        weapon_cooldown_mateiral.SetFloat("_Arc1", 0f);
        ability_cooldown_mateiral.SetFloat("_Arc1", 0f);
        ultimate_cooldown_mateiral.SetFloat("_Arc1", 0f);
    }

    private void ChangeCooldown(Equipment.IEquipment equipment, Material material)
    {
        if (equipment == null)
        {
            return;
        }

        float current_cooldown = equipment.GetCurrentCooldown();
        float cooldown = equipment.GetCooldown();
        float cooldown_remap01 = current_cooldown / cooldown;

        material.SetFloat("_Arc1", cooldown_remap01);
    }

    /// <summary>
    /// Updates cooldown for ui element.
    /// </summary>
    private void Update()
    {
        ChangeCooldown(current_weapon, weapon_cooldown_mateiral);
        ChangeCooldown(current_ability, ability_cooldown_mateiral);
        ChangeCooldown(current_ultimate, ultimate_cooldown_mateiral);
    }
}
