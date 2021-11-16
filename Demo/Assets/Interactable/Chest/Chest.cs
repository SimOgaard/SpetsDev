using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles how chests respond with player interactions.
/// </summary>
public class Chest : Interactable
{
    // Specifies what Equipment this chest contains.
    [SerializeField] private Equipment.IEquipment chest_equipment;
    [SerializeField] private GameObject chest_equipment_game_object;
    [SerializeField] private Upgrade.IUpgrade chest_upgrade;
    [SerializeField] private GameObject chest_upgrade_game_object;

    private GameObject chest_top;

    [SerializeField] private float radius = 0f;
    [SerializeField] private float power = 25f;
    [SerializeField] private float upwards_modifier = 5f;
    [SerializeField] private float explosion_offset = 5f;

    /// <summary>
    /// Handles InteractWith function call that is received from InteractableEventHandler.
    /// </summary>
    public override void InteractWith()
    {
        void CreateChestItem()
        {
            if (chest_equipment == null)
            {
                System.Type equipment_type = Equipment.RandomEquipment();
                (chest_equipment_game_object, chest_equipment) = Equipment.CreateRandomEquipment(equipment_type);

                if (chest_upgrade == null)
                {
                    //(chest_upgrade_game_object, chest_upgrade) = Upgrade.CreateRandomUpgrade(equipment_type);
                }
            }
        }

        void SpawnChestItem()
        {
            float rotation = 90f;
            Vector3 forward = -Camera.main.transform.forward;
            forward.y = 0f;

            if (chest_equipment != null)
            {
                chest_equipment.Drop(transform.position, chest_equipment.Thrust(rotation, forward));
            }
            if (chest_upgrade != null)
            {
                //chest_upgrade.Drop(transform.position, chest_equipment.Thrust(rotation, forward));
            }
        }

        void ChestOpeningAnimation()
        {
            gameObject.layer = Layer.game_world;
            chest_top.layer = Layer.game_world_moving;

            Enemies.Sound(transform, 200f);

            Rigidbody chest_top_rigidbody = RigidbodySetup.AddRigidbody(chest_top);
            chest_top_rigidbody.AddExplosionForce(power, chest_top.transform.position + new Vector3(Random.Range(-explosion_offset, explosion_offset), 0f, Random.Range(-explosion_offset, explosion_offset)), radius, upwards_modifier, ForceMode.VelocityChange);
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
        chest_top = transform.GetChild(0).gameObject;
    }

    public void Lock(bool state)
    {
        allows_interaction = state;
    }
}
