using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles how chests respond with player interactions.
/// </summary>
public class Chest : Interactable
{
    // Specifies what Equipment this chest contains.
    [SerializeField] private Equipment.IEquipment chestEquipment;
    [SerializeField] private GameObject chestEquipmentGameObject;
    [SerializeField] private Upgrade.IUpgrade chestUpgrade;
    [SerializeField] private GameObject chestUpgradeGameObject;

    private GameObject chestTop;

    [SerializeField] private float radius = 0f;
    [SerializeField] private float power = 25f;
    [SerializeField] private float upwardsModifier = 5f;
    [SerializeField] private float explosionOffset = 5f;

    /// <summary>
    /// Handles InteractWith function call that is received from InteractableEventHandler.
    /// </summary>
    public override void InteractWith()
    {
        void CreateChestItem()
        {
            if (chestEquipment == null)
            {
                System.Type equipmentType = Equipment.RandomEquipment();
                (chestEquipmentGameObject, chestEquipment) = Equipment.CreateRandomEquipment(equipmentType);

                if (chestUpgrade == null)
                {
                    //(chestUpgradeGameObject, chestUpgrade) = Upgrade.CreateRandomUpgrade(equipmentType);
                }
            }
        }

        void SpawnChestItem()
        {
            float rotation = 90f;
            Vector3 forward = -MainCamera.mCamera.transform.forward;
            forward.y = 0f;

            if (chestEquipment != null)
            {
                chestEquipment.Drop(transform.position, chestEquipment.Thrust(rotation, forward));
            }
            if (chestUpgrade != null)
            {
                //chestUpgrade.Drop(transform.position, chestEquipment.Thrust(rotation, forward));
            }
        }

        void ChestOpeningAnimation()
        {
            gameObject.layer = Layer.gameWorld;
            chestTop.layer = Layer.gameWorldMoving;

            Enemies.Sound(transform, 200f);

            Rigidbody chestTopRigidbody = RigidbodySetup.AddRigidbody(chestTop);
            chestTopRigidbody.AddExplosionForce(power, chestTop.transform.position + new Vector3(Random.Range(-explosionOffset, explosionOffset), 0f, Random.Range(-explosionOffset, explosionOffset)), radius, upwardsModifier, ForceMode.VelocityChange);
        }

        if (CanInteractWith())
        {
            base.InteractWith();
            CreateChestItem();
            SpawnChestItem();
            ChestOpeningAnimation();
        }
    }

    public override void Awake()
    {
        base.Awake();
        chestTop = transform.GetChild(0).gameObject;
    }

    public void Lock(bool state)
    {
        allowsInteraction = state;
    }
}
