using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is master component of Equipment hierarchy.
/// Initializes equipment in game world and routes all specific Equipment calls to current_equipment (Equipment parent).
/// </summary>
public class Equipment : MonoBehaviour
{
    // Stores all Equipment types globally. Is used to specify what to initialize.
    public enum EEquipment { Weapon, Ability, Ultimate }
    public enum EEquipmentUpgrade { Weapon, Ability, Ultimate }

    // Current Equipment this component controlls.
    public IEquipment current_equipment;

    /// <summary>
    /// Rulesets for parent Equipment to follow.
    /// </summary>
    public interface IEquipment
    {
        void Destroy();
        DroppedItemShaderStruct GetDroppedItemShaderStruct();
        void OnGround();
        void UsePrimary();
    }

    /// <summary>
    /// Initializes this Equipment component to one random Equipment based on current equipments in inventory.
    /// </summary>
    public void InitEquipment(IEquipment[] current_player_equipments)
    {
        
    }
    /// <summary>
    /// Initializes this Equipment component to specified Equipment parent.
    /// </summary>
    public void InitEquipment(EEquipment equipment_type)
    {
        switch (equipment_type)
        {
            case EEquipment.Weapon:
                current_equipment = gameObject.AddComponent<Weapon>();
                break;
            case EEquipment.Ability:
                current_equipment = gameObject.AddComponent<Ability>();
                break;
            case EEquipment.Ultimate:
                current_equipment = gameObject.AddComponent<Ultimate>();
                break;
        }
    }

    /// <summary>
    /// Deletes hierarchy from the bottom up.
    /// </summary>
    public void DestroyEquipment()
    {
        current_equipment.Destroy();
        Destroy(this);
    }

    /// <summary>
    /// Class global values used while transfering Equipment from chest/inventory to game world.
    /// </summary>
    private bool grounded;
    private float initialization_time;
    private static float static_time_threshold = 0.1f;
    private SpriteInitializer sprite_initializer;
    private GameObject dropped_equipment_game_object;

    /// <summary>
    /// Global struct used for populating data to shaders.
    /// </summary>
    public struct DroppedItemShaderStruct
    {
        public float time;
        public float start_width;
        public float end_width;
        public Color[] color;
        public float[] alpha;
        public Material trail_material;
        public Material material;
    }

    /// <summary>
    /// Shades Equipment based on fetched Equipment parent shader data.
    /// </summary>
    public void ShadeDroppedItem(GameObject dropped_equipment_game_object)
    {
        TrailRenderer trail_renderer = gameObject.AddComponent<TrailRenderer>();
        MeshRenderer orb = dropped_equipment_game_object.GetComponent<MeshRenderer>();

        DroppedItemShaderStruct shader_struct = current_equipment.GetDroppedItemShaderStruct();

        orb.material = shader_struct.material;

        trail_renderer.time = shader_struct.time;
        trail_renderer.startWidth = shader_struct.start_width;
        trail_renderer.endWidth = shader_struct.end_width;
        trail_renderer.material = shader_struct.trail_material;

        Gradient gradient = new Gradient();

        Color[] color_array = shader_struct.color;
        int color_array_length = color_array.Length;
        GradientColorKey[] color_key = new GradientColorKey[color_array_length];
        for (int color_index = 0; color_index < color_array_length; color_index++)
        {
            color_key[color_index].color = color_array[color_index];
            color_key[color_index].time = (float)color_index / (float)(color_array_length-1);
        }

        float[] alpha_array = shader_struct.alpha;
        int alpha_array_length = alpha_array.Length;
        GradientAlphaKey[] alpha_key = new GradientAlphaKey[alpha_array_length];
        for (int alpha_index = 0; alpha_index < alpha_array_length; alpha_index++)
        {
            alpha_key[alpha_index].alpha = alpha_array[alpha_index];
            alpha_key[alpha_index].time = (float)alpha_index / (float)(alpha_array_length-1);
        }

        gradient.SetKeys(color_key, alpha_key);
        trail_renderer.colorGradient = gradient;
    }

    /// <summary>
    /// Creates rigidbody Apply forces .
    /// </summary>
    public void DropEquipment(Vector3 position, float selected_rotation, float force = 11500f)
    {
        grounded = false;

        transform.parent = GameObject.Find("EquipmentsInAir").transform;

        dropped_equipment_game_object = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(dropped_equipment_game_object.GetComponent<SphereCollider>());
        dropped_equipment_game_object.transform.parent = transform;
        Rigidbody equipment_rigidbody = gameObject.AddComponent<Rigidbody>();
        SphereCollider equipment_collider = gameObject.AddComponent<SphereCollider>();

        equipment_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        Physics.IgnoreCollision(GameObject.Find("Player").GetComponent<CapsuleCollider>(), equipment_collider);

        ShadeDroppedItem(dropped_equipment_game_object);

        Vector3 thrust = Quaternion.Euler(-Random.Range(72.5f, 82.5f), Random.Range(-selected_rotation * 0.5f + 180f, selected_rotation * 0.5f + 180f), 0) * Vector3.forward * force;
        transform.position = position;
        gameObject.GetComponent<Rigidbody>().AddForce(thrust);
    }

    /// <summary>
    /// Enables Pickup() interaction and visualizes the object on the ground.
    /// </summary>
    private void EquipmentOnGround()
    {
        grounded = true;
        Destroy(gameObject.GetComponent<SphereCollider>());
        Destroy(gameObject.GetComponent<Rigidbody>());

        current_equipment.OnGround();

        sprite_initializer = gameObject.AddComponent<SpriteInitializer>();
        Sprite not_interacting_with_sprite = Resources.Load<Sprite>("Interactables/not_interacting_with_sprite");
        sprite_initializer.InitializeUpright(not_interacting_with_sprite);

        transform.parent = GameObject.Find("EquipmentsOnGround").transform;
        gameObject.isStatic = true;

        StartCoroutine(TrailFade(GetComponent<TrailRenderer>().time));
    }

    /// <summary>
    /// Removes visual components of gameObject and transfers gameObject to player inventory.
    /// </summary>
    public void Pickup()
    {
        transform.parent = GameObject.Find("EquipmentsInInventory").transform;
        sprite_initializer.Destroy();
        Destroy(dropped_equipment_game_object);
        gameObject.isStatic = false;
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Removes TrailRenderer component when trail is no longer visible.
    /// </summary>
    private IEnumerator TrailFade(float delay_time)
    {
        yield return new WaitForSeconds(delay_time);
        Destroy(gameObject.GetComponent<TrailRenderer>());
    }

    /// <summary>
    /// Detects if gameObject is grounded.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "GameWorld" && Time.timeSinceLevelLoad - initialization_time > static_time_threshold && !grounded)
        {
            EquipmentOnGround();
        }
    }

    private void Start()
    {
        initialization_time = Time.timeSinceLevelLoad;
    }
}
