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
        Sprite GetIconSprite();
    }

    /// <summary>
    /// Initializes this Equipment component to one random Equipment based on current equipments in inventory.
    /// </summary>
    public void InitEquipment(IEquipment current_player_weapon, IEquipment current_player_ability, IEquipment current_player_ultimate)
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
    /// Makes all childobjects including this object static/not static.
    /// </summary>
    private void MakeStatic(GameObject game_object, bool static_state)
    {
        game_object.isStatic = static_state;

        foreach (Transform child in game_object.transform)
        {
            MakeStatic(child.gameObject, static_state);
        }
    }

    /// <summary>
    /// Class global values used while transfering Equipment from chest/inventory to game world.
    /// </summary>
    private bool grounded;
    private float initialization_time;
    private static float static_time_threshold = 0.1f;

    private SpriteInitializer sprite_initializer;
    private Sprite not_interacting_with_sprite;
    private TrailRenderer trail_renderer;
    private MeshRenderer orb_mesh_renderer;
    private MeshFilter mesh_filter;

    private Rigidbody equipment_rigidbody;
    private SphereCollider equipment_collider;

    private Transform equipments_in_inventory;
    private Transform equipments_in_air;
    private Transform equipments_on_ground;

    private PlayerInventory player_inventory_controller;
    private UIInventory ui_inventory;

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
    /// Global struct of all vertices for icosahedron.
    /// </summary>
    private static Vector3[] GetVectors()
    {
        float s = 0.3f;
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        return new Vector3[]
        {
            new Vector3(-1,  t,  0) * s,
            new Vector3( 1,  t,  0) * s,
            new Vector3(-1, -t,  0) * s,
            new Vector3( 1, -t,  0) * s,
            new Vector3( 0, -1,  t) * s,
            new Vector3( 0,  1,  t) * s,
            new Vector3( 0, -1, -t) * s,
            new Vector3( 0,  1, -t) * s,
            new Vector3( t,  0, -1) * s,
            new Vector3( t,  0,  1) * s,
            new Vector3(-t,  0, -1) * s,
            new Vector3(-t,  0,  1) * s
        };
    }

    /// <summary>
    /// Global struct of all triangles for icosahedron.
    /// </summary>
    private static int[] GetTriangles()
    {
        return new int[]
        {
             0, 11,  5,
             0,  5,  1,
             0,  1,  7,
             0,  7, 10,
             0, 10, 11,
             1,  5,  9,
             5, 11,  4,
            11, 10,  2,
            10,  7,  6,
             7,  1,  8,
             3,  9,  4,
             3,  4,  2,
             3,  2,  6,
             3,  6,  8,
             3,  8,  9,
             4,  9,  5,
             2,  4, 11,
             6,  2, 10,
             8,  6,  7,
             9,  8,  1
        };
    }

    /// <summary>
    /// Global function returning low poly sphere mesh (icosahedron).
    /// </summary>
    public static Mesh GetLowPolySphereMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = GetVectors();
        mesh.triangles = GetTriangles();
        return mesh;
    }

    /// <summary>
    /// Shades Equipment based on fetched Equipment parent shader data.
    /// </summary>
    public void ShadeDroppedItem()
    {
        trail_renderer = gameObject.AddComponent<TrailRenderer>();
        orb_mesh_renderer = gameObject.AddComponent<MeshRenderer>();
        mesh_filter = gameObject.AddComponent<MeshFilter>();

        mesh_filter.mesh = GetLowPolySphereMesh();

        DroppedItemShaderStruct shader_struct = current_equipment.GetDroppedItemShaderStruct();

        orb_mesh_renderer.material = shader_struct.material;

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
    public void DropEquipment(Vector3 position, float selected_rotation, float force = 5750f)
    {
        grounded = false;
        initialization_time = Time.timeSinceLevelLoad;

        ShadeDroppedItem();

        transform.parent = equipments_in_air;
        transform.position = position;

        equipment_rigidbody = gameObject.AddComponent<Rigidbody>();
        equipment_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        Vector3 thrust = Quaternion.Euler(-Random.Range(72.5f, 82.5f), Random.Range(-selected_rotation * 0.5f + 180f, selected_rotation * 0.5f + 180f), 0) * Vector3.forward * force;
        equipment_rigidbody.AddForce(thrust, ForceMode.Force);
        equipment_collider = gameObject.AddComponent<SphereCollider>();
        equipment_collider.radius = 1f;
        equipment_collider.isTrigger = true;
    }

    /// <summary>
    /// Enables Pickup() interaction and visualizes the object on the ground.
    /// </summary>
    private void EquipmentOnGround()
    {
        grounded = true;
        Destroy(equipment_collider);
        Destroy(equipment_rigidbody);

        sprite_initializer = gameObject.AddComponent<SpriteInitializer>();
        sprite_initializer.Initialize(not_interacting_with_sprite, Vector3.zero);

        current_equipment.OnGround();
        transform.parent = equipments_on_ground;
        MakeStatic(gameObject, true);

        Destroy(trail_renderer);
    }

    /// <summary>
    /// Removes visual components of gameObject and transfers gameObject to player inventory.
    /// </summary>
    public void Pickup()
    {
        // Dropps current equipment and sets variable in PlayerInventoryController
        Vector3 spawn_point = equipments_in_inventory.position;
        switch (current_equipment.GetType().Name)
        {
            case nameof(Weapon):
                ui_inventory.ChangeWeapon(current_equipment);
                if (player_inventory_controller.weapon_equipment != null)
                {
                    player_inventory_controller.weapon_equipment.DropEquipment(spawn_point, 360f);
                }
                player_inventory_controller.weapon_equipment = this;
                player_inventory_controller.weapon_parrent = current_equipment as Weapon;
                player_inventory_controller.weapon = player_inventory_controller.weapon_parrent.current_weapon;
                break;
            case nameof(Ability):
                ui_inventory.ChangeAbility(current_equipment);
                if (player_inventory_controller.ability_equipment != null)
                {
                    player_inventory_controller.ability_equipment.DropEquipment(spawn_point, 360f);
                }
                player_inventory_controller.ability_equipment = this;
                player_inventory_controller.ability_parrent = current_equipment as Ability;
                player_inventory_controller.ability = player_inventory_controller.ability_parrent.current_ability;
                break;
            case nameof(Ultimate):
                ui_inventory.ChangeUltimate(current_equipment);
                if (player_inventory_controller.ultimate_equipment != null)
                {
                    player_inventory_controller.ultimate_equipment.DropEquipment(spawn_point, 360f);
                }
                player_inventory_controller.ultimate_equipment = this;
                player_inventory_controller.ultimate_parrent = current_equipment as Ultimate;
                player_inventory_controller.ultimate = player_inventory_controller.ultimate_parrent.current_ultimate;

                break;
        }

        // Picksup current equipment;
        transform.parent = equipments_in_inventory;
        sprite_initializer.Destroy();
        Destroy(orb_mesh_renderer);
        Destroy(mesh_filter);
        MakeStatic(gameObject, false);
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Detects if gameObject is grounded.
    /// </summary>
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == 12 && Time.timeSinceLevelLoad - initialization_time > static_time_threshold && !grounded)
        {
            EquipmentOnGround();
        }
    }

    private void Start()
    {
        equipments_in_inventory = GameObject.Find("EquipmentsInInventory").transform;
        equipments_in_air = GameObject.Find("EquipmentsInAir").transform;
        equipments_on_ground = GameObject.Find("EquipmentsOnGround").transform;

        player_inventory_controller = equipments_in_inventory.GetComponent<PlayerInventory>();
        not_interacting_with_sprite = Resources.Load<Sprite>("Interactables/not_interacting_with_sprite");

        ui_inventory = GameObject.Find("UIInventory").GetComponent<UIInventory>();
    }
}
