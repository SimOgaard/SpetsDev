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
    [SerializeField] private GameObject currentWeapon_UIGameObject;
    public static Image currentWeapon_UIImage;
    private Material weaponCooldownMaterial;
    [SerializeField] private GameObject currentAbility_UIGameObject;
    public static Image currentAbility_UIImage;
    private Material abilityCooldownMaterial;
    [SerializeField] private GameObject currentUltimate_UIGameObject;
    public static Image currentUltimate_UIImage;
    private Material ultimateCooldownMaterial;

    [SerializeField] private Color backgroundColor;

    /// <summary>
    /// Retrieves each Image component for each equipment.
    /// Initializes ui images to init state.
    /// </summary>
    private void Awake()
    {
        currentWeapon_UIImage = currentWeapon_UIGameObject.GetComponent<Image>();
        currentAbility_UIImage = currentAbility_UIGameObject.GetComponent<Image>();
        currentUltimate_UIImage = currentUltimate_UIGameObject.GetComponent<Image>();

        currentWeapon_UIImage.color = backgroundColor; //Resources.Load<Sprite>("Sprites/debugger");
        currentAbility_UIImage.color = backgroundColor;
        currentUltimate_UIImage.color = backgroundColor;

        RectTransform currentWeaponRectTransform = currentWeapon_UIGameObject.GetComponent<RectTransform>();
        RectTransform currentAbilityRectTransform = currentAbility_UIGameObject.GetComponent<RectTransform>();
        RectTransform currentUltimateRectTransform = currentUltimate_UIGameObject.GetComponent<RectTransform>();

        currentWeaponRectTransform.sizeDelta = new Vector2(14f, 14f);
        currentAbilityRectTransform.sizeDelta = new Vector2(14f, 14f);
        currentUltimateRectTransform.sizeDelta = new Vector2(19f, 19f);

        weaponCooldownMaterial = currentWeapon_UIGameObject.transform.GetChild(0).GetComponent<Image>().material;
        abilityCooldownMaterial = currentAbility_UIGameObject.transform.GetChild(0).GetComponent<Image>().material;
        ultimateCooldownMaterial = currentUltimate_UIGameObject.transform.GetChild(0).GetComponent<Image>().material;

        weaponCooldownMaterial.SetFloat("_Arc1", 0f);
        abilityCooldownMaterial.SetFloat("_Arc1", 0f);
        ultimateCooldownMaterial.SetFloat("_Arc1", 0f);
    }

    private void ChangeCooldown(Equipment.IEquipment equipment, Material material)
    {
        if (equipment == null)
        {
            return;
        }

        float currentCooldown = equipment.currentCooldown;
        float cooldown = equipment.cooldown;
        float cooldownRemap01 = currentCooldown / cooldown;

        material.SetFloat("_Arc1", cooldownRemap01);
    }

    /// <summary>
    /// Updates cooldown for ui element.
    /// </summary>
    private void Update()
    {
        ChangeCooldown(PlayerInventory.weapon, weaponCooldownMaterial);
        ChangeCooldown(PlayerInventory.ability, abilityCooldownMaterial);
        ChangeCooldown(PlayerInventory.ultimate, ultimateCooldownMaterial);
    }
}
