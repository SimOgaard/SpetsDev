using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    private bool grounded;
    private float initialization_time;
    private static float static_time_threshold = 0.1f;

    private SpriteInitializer sprite_initializer;
    private Sprite not_interacting_with_sprite;
    private TrailRenderer trail_renderer;
    private MeshRenderer orb_mesh_renderer;
    private MeshFilter mesh_filter;

    private Rigidbody _rigidbody;
    private SphereCollider _collider;

    private Transform items_in_inventory;
    private Transform items_in_air;
    private Transform items_on_ground;

    private DroppedItemShaderStruct shader_struct;
    private System.Action on_drop_function;

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
    /// Starts the drop of item by applying forces and meshes.
    /// </summary>
    public void InitDrop(Vector3 position, float selected_rotation, float force, DroppedItemShaderStruct shader_struct, System.Action on_drop_function)
    {
        this.shader_struct = shader_struct;
        this.on_drop_function = on_drop_function;

        grounded = false;
        initialization_time = Time.timeSinceLevelLoad;

        ShadeDroppedItem();

        transform.parent = items_in_air;
        transform.position = position;

        _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        Vector3 thrust = Quaternion.Euler(-Random.Range(72.5f, 82.5f), Random.Range(-selected_rotation * 0.5f + 180f, selected_rotation * 0.5f + 180f), 0) * Vector3.forward * force;
        _rigidbody.AddForce(thrust, ForceMode.Force);
        _collider = gameObject.AddComponent<SphereCollider>();
        _collider.radius = 1f;
        _collider.isTrigger = true;
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
            color_key[color_index].time = (float)color_index / (float)(color_array_length - 1);
        }

        float[] alpha_array = shader_struct.alpha;
        int alpha_array_length = alpha_array.Length;
        GradientAlphaKey[] alpha_key = new GradientAlphaKey[alpha_array_length];
        for (int alpha_index = 0; alpha_index < alpha_array_length; alpha_index++)
        {
            alpha_key[alpha_index].alpha = alpha_array[alpha_index];
            alpha_key[alpha_index].time = (float)alpha_index / (float)(alpha_array_length - 1);
        }

        gradient.SetKeys(color_key, alpha_key);
        trail_renderer.colorGradient = gradient;
    }

    /// <summary>
    /// Enables Pickup() interaction and visualizes the object on the ground.
    /// </summary>
    private void OnGround()
    {
        grounded = true;
        Destroy(_collider);
        Destroy(_rigidbody);

        sprite_initializer = gameObject.AddComponent<SpriteInitializer>();
        sprite_initializer.Initialize(not_interacting_with_sprite, Vector3.zero);

        on_drop_function();

        transform.parent = items_on_ground;
        MakeStatic(gameObject, true);

        Destroy(trail_renderer);
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
    /// Detects if gameObject is grounded.
    /// </summary>
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == 12 && Time.timeSinceLevelLoad - initialization_time > static_time_threshold && !grounded)
        {
            OnGround();
        }
    }

    /// <summary>
    /// Places item in inventory and cleans gameobject
    /// </summary>
    public void Pickup()
    {
        transform.parent = items_in_inventory;
        sprite_initializer.Destroy();
        Destroy(orb_mesh_renderer);
        Destroy(mesh_filter);
        MakeStatic(gameObject, false);
        transform.localPosition = Vector3.zero;
        Destroy(this);
    }

    /// <summary>
    /// Retrieves all necessary sprites, game objects, tranforms and components.
    /// </summary>
    private void Start()
    {
        items_in_inventory = GameObject.Find("EquipmentsInInventory").transform;
        items_in_air = GameObject.Find("ItemsInAir").transform;
        items_on_ground = GameObject.Find("ItemsOnGround").transform;

        not_interacting_with_sprite = Resources.Load<Sprite>("Interactables/not_interacting_with_sprite");

        gameObject.layer = 11;
    }
}
