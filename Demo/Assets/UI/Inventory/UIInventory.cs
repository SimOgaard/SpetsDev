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
    public static Image current_weapon_UI_image;
    private Material weapon_cooldown_material;
    [SerializeField] private GameObject current_ability_UI_game_object;
    public static Image current_ability_UI_image;
    private Material ability_cooldown_material;
    [SerializeField] private GameObject current_ultimate_UI_game_object;
    public static Image current_ultimate_UI_image;
    private Material ultimate_cooldown_material;

    [SerializeField] private Color background_color;

    /// <summary>
    /// Retrieves each Image component for each equipment.
    /// Initializes ui images to init state.
    /// </summary>
    private void Start()
    {
        current_weapon_UI_image = current_weapon_UI_game_object.GetComponent<Image>();
        current_ability_UI_image = current_ability_UI_game_object.GetComponent<Image>();
        current_ultimate_UI_image = current_ultimate_UI_game_object.GetComponent<Image>();

        current_weapon_UI_image.color = background_color; //Resources.Load<Sprite>("Sprites/debugger");
        current_ability_UI_image.color = background_color;
        current_ultimate_UI_image.color = background_color;

        RectTransform current_weapon_rect_transform = current_weapon_UI_game_object.GetComponent<RectTransform>();
        RectTransform current_ability_rect_transform = current_ability_UI_game_object.GetComponent<RectTransform>();
        RectTransform current_ultimate_rect_transform = current_ultimate_UI_game_object.GetComponent<RectTransform>();

        current_weapon_rect_transform.sizeDelta = new Vector2(14f, 14f);
        current_ability_rect_transform.sizeDelta = new Vector2(14f, 14f);
        current_ultimate_rect_transform.sizeDelta = new Vector2(19f, 19f);

        weapon_cooldown_material = current_weapon_UI_game_object.transform.GetChild(0).GetComponent<Image>().material;
        ability_cooldown_material = current_ability_UI_game_object.transform.GetChild(0).GetComponent<Image>().material;
        ultimate_cooldown_material = current_ultimate_UI_game_object.transform.GetChild(0).GetComponent<Image>().material;

        weapon_cooldown_material.SetFloat("_Arc1", 0f);
        ability_cooldown_material.SetFloat("_Arc1", 0f);
        ultimate_cooldown_material.SetFloat("_Arc1", 0f);
    }

    private void ChangeCooldown(Equipment.IEquipment equipment, Material material)
    {
        if (equipment == null)
        {
            return;
        }

        float current_cooldown = equipment.current_cooldown;
        float cooldown = equipment.cooldown;
        float cooldown_remap01 = current_cooldown / cooldown;

        material.SetFloat("_Arc1", cooldown_remap01);
    }

    /// <summary>
    /// Updates cooldown for ui element.
    /// </summary>
    private void Update()
    {
        ChangeCooldown(PlayerInventory.weapon, weapon_cooldown_material);
        ChangeCooldown(PlayerInventory.ability, ability_cooldown_material);
        ChangeCooldown(PlayerInventory.ultimate, ultimate_cooldown_material);
    }
}
