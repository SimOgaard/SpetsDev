using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script initilizes and controlls rock game object that all Earthbending Equipments utilizes.
/// </summary>
public class EarthBendingRock : MonoBehaviour
{
    public enum MoveStates { up, still, down };
    public MoveStates move_state = MoveStates.up;

    public Rigidbody rock_rigidbody;

    public float sleep_time;            // time in seconds rock should be still for.
    public float growth_speed;          // how quickly rock should move.
    public float current_sleep_time;

    private const float ray_clearance = 50f;

    public bool should_be_deleted = false;

    public bool deal_damage_by_collision = false;
    public bool deal_damage_by_trigger = false;
    public float damage = 0f;

    private float _growth_time;
    public float growth_time
    {
        get { return _growth_time; }
        set { _growth_time = Mathf.Clamp01(value); }
    }
    
    //private List<CombineInstance> combined_mesh_instance = new List<CombineInstance>();

    public float sound_amplifier = 0f;
    public float max_sound = Mathf.Infinity;

    public void SetSound(float sound_amplifier, float max_sound = Mathf.Infinity)
    {
        this.sound_amplifier = sound_amplifier;
        this.max_sound = max_sound;
    }

    public float under_ground_height = 1f;

    /// <summary>
    /// Merges theese rocks which this script ultimately controls.
    /// </summary>
    /*
    public void InitEarthbendingPillar(GameObject rock_game_object_to_merge)
    {
        MeshFilter mesh_filter = rock_game_object_to_merge.GetComponent<MeshFilter>();
        CombineInstance combine = new CombineInstance();
        combine.mesh = mesh_filter.sharedMesh;
        combine.transform = transform.worldToLocalMatrix * mesh_filter.transform.localToWorldMatrix;
        combined_mesh_instance.Add(combine);
    }
    
    /// <summary>
    /// Merges meshes and sets values for merged rock this script controls.
    /// </summary>
    public void SetSharedValues(float sleep_time, float move_speed, float height, Material material)
    {
        this.sleep_time = sleep_time;
        current_sleep_time = sleep_time;
        this.move_speed = move_speed;

        this.under_ground_point = transform.position;
        this.ground_point = transform.position + transform.rotation * new Vector3(0f, height, 0f);

        Mesh combined_mesh = new Mesh();
        combined_mesh.CombineMeshes(combined_mesh_instance.ToArray());
        gameObject.AddComponent<MeshFilter>().mesh = combined_mesh;
        gameObject.AddComponent<MeshCollider>().sharedMesh = combined_mesh;
        gameObject.AddComponent<MeshRenderer>().material = material;
    }
    */

    /// <summary>
    /// Initilizes a kinematic rigidbody.
    /// </summary>
    private void Awake()
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rock.transform.parent = transform;
        rock.transform.localPosition = Vector3.up * 0.5f;
        rock.GetComponent<MeshRenderer>().material = Global.stone_material;

        gameObject.layer = Layer.game_world_moving;
        rock.layer = Layer.game_world_moving;

        Density density = rock.AddComponent<Density>();
        rock_rigidbody = rock.AddComponent<Rigidbody>();
        rock_rigidbody.isKinematic = true;
    }

    /// <summary>
    /// Places rock at ground given a point.
    /// </summary>
    public virtual void PlacePillar(Vector3 point, Quaternion rotation, Vector3 scale)
    {
        transform.rotation = rotation;
        transform.localScale = scale;
        (Vector3 place_point, Vector3 normal) = GetRayHitData(point, rotation, scale);
        transform.position = place_point;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Returns ground point given a point and rotation and scale.
    /// </summary>
    public (Vector3 point, Vector3 normal) GetRayHitData(Vector3 point, Quaternion rotation, Vector3 scale)
    {
        float longest_dist = Mathf.NegativeInfinity;
        Vector3 norm_sum = Vector3.zero;
        RaycastHit hit_data;
        for (float x = -scale.x * 0.5f; x <= scale.x * 0.5f; x += scale.x)
        {
            for (float z = -scale.z * 0.5f; z <= scale.z * 0.5f; z += scale.z)
            {
                Vector3 offset = rotation * new Vector3(x, 0f, z);
                GetRayHitData(point + offset, rotation, out hit_data);
                norm_sum += hit_data.normal;
                if (hit_data.distance > longest_dist)
                {
                    longest_dist = hit_data.distance;
                }
            }
        }
        Debug.Log(longest_dist);
        Vector3 spawn_point = point + rotation * Vector3.down * (longest_dist - ray_clearance);
        return (spawn_point, norm_sum.normalized);
    }

    /// <summary>
    /// Return ground point given a point and rotation.
    /// </summary>
    public void GetRayHitData(Vector3 point, Quaternion rotation, out RaycastHit hit_data)
    {
        Vector3 up = rotation * Vector3.up;
        Physics.Raycast(point + up * ray_clearance, -up, out hit_data, ray_clearance * 2f, Layer.Mask.static_ground);
    }

    /*
    /// <summary>
    /// Places rock at ground if ground position in y axis is in range of starting point.
    /// Otherwise places rock at given point.
    /// </summary>
    public void PlacePillar(Vector3 point, float y_max_diff, bool ignore)
    {
        if (Physics.Raycast(point + new Vector3(0f, 20f, 0f), Vector3.down, out hit_data, 40f, Layer.Mask.static_ground))
        {
            if (Mathf.Abs(hit_data.point.y - point.y) > y_max_diff && !ignore)
            {
                PlacePillarPoint(point);
                return;
            }

            transform.position = hit_data.point - transform.rotation * new Vector3(0f, transform.localScale.y * 0.5f, 0f);
            this.ground_point = hit_data.point;
            this.under_ground_point = transform.position;
        }
        else
        {
            PlacePillarPoint(point);
        }
    }
    */
    /*
    /// <summary>
    /// Places rock at given point.
    /// </summary>
    public void PlacePillarPoint(Vector3 point)
    {
        transform.position = point - transform.rotation * new Vector3(0f, transform.localScale.y * 0.5f, 0f);
        this.ground_point = point;
        this.under_ground_point = transform.position;
    }
    */
    /// <summary>
    /// Calculates point where rock is hidden nomatter its rotation.
    /// </summary>
    /*
    public void PlacePillarHidden(Vector3 point, int solve_itteration = 10)
    {
        float height = transform.localScale.y;
        float width = transform.localScale.x;
        Quaternion rotation = transform.rotation;

        Vector3 rock_down_direction = rotation * Vector3.down;
        float height_by_itteration = height / solve_itteration;

        Vector3 ground_point_middle = GetGroundPoint(point);

        float max_y_displacement = 0f;
        for (int i = 1; i < solve_itteration; i++)
        {
            Vector3 test_point = point + rock_down_direction * height_by_itteration * i;
            Vector3 ground_point_current = GetGroundPoint(test_point);
            float y_diff = ground_point_current.y - test_point.y;
            if (y_diff < max_y_displacement)
            {
                max_y_displacement = y_diff;
            }
        }
        ground_point_middle.y += max_y_displacement * 2f;

        PlacePillarPoint(ground_point_middle);
    }
    */
    /*

    public Vector3 GetGroundPoint(Vector3 point, float height_offset = 20f, float ray_max_distance = 40f)
    {
        if (Physics.Raycast(point + new Vector3(0f, 20f, 0f), Vector3.down, out hit_data, 40f, Layer.Mask.static_ground))
        {
            return hit_data.point;
        }
        return point;
    }
    */
    private string damage_id = "";
    public void SetDamageId(string damage_id)
    {
        this.damage_id = damage_id;
    }

    public void DealDamageByTrigger(Vector3 center, float radius)
    {
        deal_damage_by_trigger = true;
        SphereCollider damage_collider_trigger = gameObject.AddComponent<SphereCollider>();
        damage_collider_trigger.isTrigger = true;
        damage_collider_trigger.center = center;
        damage_collider_trigger.radius = radius;
    }

    public void DealDamageByTrigger()
    {
        deal_damage_by_trigger = true;
        SphereCollider damage_collider_trigger = gameObject.AddComponent<SphereCollider>();
        damage_collider_trigger.isTrigger = true;
        damage_collider_trigger.center = new Vector3(0f, 0.5f, 0f);
        damage_collider_trigger.radius = 0.05f;
    }

    public void SetTrigger()
    {
        deal_damage_by_trigger = true;
        GetComponent<BoxCollider>().isTrigger = true;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (deal_damage_by_collision && Layer.IsInLayer(Layer.enemy, collision.gameObject.layer) && move_state == MoveStates.up)
        {
            collision.transform.parent.GetComponent<EnemyAI>().Damage(damage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (deal_damage_by_trigger && Layer.IsInLayer(Layer.enemy, other.gameObject.layer) && move_state == MoveStates.up)
        {
            Transform parent = other.transform.parent;
            if (parent.GetComponent<EnemyAI>().Damage(damage, damage_id, 0.25f))
            {
                parent.GetComponent<Agent>().AddForce(750f * transform.up, ForceMode.Impulse);
            }
        }
    }
}
