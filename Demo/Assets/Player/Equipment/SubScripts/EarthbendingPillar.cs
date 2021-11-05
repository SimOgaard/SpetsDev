using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script initilizes and controlls pillar game object that all Earthbending Equipments utilizes.
/// </summary>
public class EarthbendingPillar : MonoBehaviour
{
    public enum MoveStates { up, still, down };
    public MoveStates move_state = MoveStates.up;

    private Rigidbody pillar_rigidbody;

    public Vector3 ground_point;       // position you should move twords in the beginning, then sleep for some time.
    public Vector3 under_ground_point; // position you should spawn object in and move twords in the end then delete.
    private float sleep_time;           // time in seconds pillar should be still for.
    private float move_speed;           // how quickly pillar should move.
    public float current_sleep_time;

    private RaycastHit hit_data;

    public bool should_be_deleted = false;

    public bool deal_damage_by_collision = false;
    public bool deal_damage_by_trigger = false;
    public float damage = 0f;
    public float tumble_time;

    private float _move_time;
    private float move_time
    {
        get { return _move_time; }
        set { _move_time = Mathf.Clamp01(value); }
    }

    List<CombineInstance> combined_mesh_instance = new List<CombineInstance>();

    private float sound_amplifier = 0f;
    private float max_sound = Mathf.Infinity;
    private float hearing_threshold_change = 1f;

    public void SetSound(float sound_amplifier, float max_sound = Mathf.Infinity, float hearing_threshold_change = 1f)
    {
        this.sound_amplifier = sound_amplifier;
        this.max_sound = max_sound;
        this.hearing_threshold_change = hearing_threshold_change;
    }

    /// <summary>
    /// Changes the pillar this script controls.
    /// </summary>
    public void InitEarthbendingPillar(float height, float width, Quaternion rotation, float sleep_time, float move_speed)
    {
        ChangePillar(height, width, rotation, sleep_time, move_speed);
    }

    /// <summary>
    /// Merges theese pillars which this script ultimately controls.
    /// </summary>
    public void InitEarthbendingPillar(GameObject pillar_game_object_to_merge)
    {
        MeshFilter mesh_filter = pillar_game_object_to_merge.GetComponent<MeshFilter>();
        CombineInstance combine = new CombineInstance();
        combine.mesh = mesh_filter.sharedMesh;
        combine.transform = mesh_filter.transform.localToWorldMatrix;
        combined_mesh_instance.Add(combine);
    }

    /// <summary>
    /// Merges meshes and sets values for merged pillar this script controls.
    /// </summary>
    public void SetSharedValues(float sleep_time, float move_speed, float height, Material material)
    {
        this.sleep_time = sleep_time;
        current_sleep_time = sleep_time;
        this.move_speed = move_speed;
        transform.position = Vector3.zero;
        this.ground_point = new Vector3(0f, height, 0f);
        this.under_ground_point = transform.position;
        Mesh combined_mesh = new Mesh();
        combined_mesh.CombineMeshes(combined_mesh_instance.ToArray());
        gameObject.AddComponent<MeshFilter>().mesh = combined_mesh;
        gameObject.AddComponent<MeshCollider>().sharedMesh = combined_mesh;
        gameObject.AddComponent<MeshRenderer>().material = material;
    }

    /// <summary>
    /// Initilizes a kinematic rigidbody.
    /// </summary>
    private void Awake()
    {
        pillar_rigidbody = gameObject.AddComponent<Rigidbody>();
        pillar_rigidbody.isKinematic = true;
    }

    /// <summary>
    /// Changes scale, rotation, move speed and sleep time of this pillar.
    /// </summary>
    public void ChangePillar(float height, float width, Quaternion rotation, float sleep_time, float move_speed)
    {
        transform.localScale = new Vector3(width, height * 2f, width);
        transform.rotation = rotation;
        this.sleep_time = sleep_time;
        current_sleep_time = sleep_time;
        this.move_speed = move_speed;
    }

    /// <summary>
    /// Places pillar at ground given a point.
    /// </summary>
    public void PlacePillar(Vector3 point)
    {
        if (Physics.Raycast(point + new Vector3(0f, 20f, 0f), Vector3.down, out hit_data, 40f, Layer.Mask.static_ground))
        {
            transform.position = hit_data.point - transform.rotation * new Vector3(0f, transform.localScale.y * 0.5f, 0f);
            this.ground_point = hit_data.point;
            this.under_ground_point = transform.position;
        }
        else
        {
            PlacePillarPoint(point);
        }
    }

    /// <summary>
    /// Places pillar at ground if ground position in y axis is in range of starting point.
    /// Otherwise places pillar at given point.
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

    /// <summary>
    /// Places pillar at given point.
    /// </summary>
    public void PlacePillarPoint(Vector3 point)
    {
        transform.position = point - transform.rotation * new Vector3(0f, transform.localScale.y * 0.5f, 0f);
        this.ground_point = point;
        this.under_ground_point = transform.position;
    }

    /// <summary>
    /// Calculates point where pillar is hidden nomatter its rotation.
    /// </summary>
    public void PlacePillarHidden(Vector3 point, int solve_itteration = 10)
    {
        float height = transform.localScale.y;
        float width = transform.localScale.x;
        Quaternion rotation = transform.rotation;

        Vector3 pillar_down_direction = rotation * Vector3.down;
        float height_by_itteration = height / solve_itteration;

        Vector3 ground_point_middle = GetGroundPoint(point);

        float max_y_displacement = 0f;
        for (int i = 1; i < solve_itteration; i++)
        {
            Vector3 test_point = point + pillar_down_direction * height_by_itteration * i;
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

    public Vector3 GetGroundPoint(Vector3 point, float height_offset = 20f, float ray_max_distance = 40f)
    {
        if (Physics.Raycast(point + new Vector3(0f, 20f, 0f), Vector3.down, out hit_data, 40f, Layer.Mask.static_ground))
        {
            return hit_data.point;
        }
        return point;
    }

    private string damage_id = "";
    public void SetDamageId(string damage_id)
    {
        this.damage_id = damage_id;
    }

    public void DealDamageByCollision()
    {
        deal_damage_by_collision = true;
    }

    public void DealDamageByTrigger(Vector3 center, float radius, float tumble_time)
    {
        this.tumble_time = tumble_time;
        deal_damage_by_trigger = true;
        SphereCollider damage_collider_trigger = gameObject.AddComponent<SphereCollider>();
        damage_collider_trigger.isTrigger = true;
        damage_collider_trigger.center = center;
        damage_collider_trigger.radius = radius;
    }

    public void DealDamageByTrigger(float tumble_time)
    {
        this.tumble_time = tumble_time;
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

    /// <summary>
    /// Controlls all three stages a pillar goes through.
    /// Moving up and down and standing still. States can be changed outside of script.
    /// </summary>
    private void FixedUpdate()
    {
        float move_diff;
        switch (move_state)
        {
            case MoveStates.up:
                if (move_time == 1f)
                {
                    current_sleep_time = sleep_time;
                    pillar_rigidbody.Sleep();
                    move_state = MoveStates.still;
                    break;
                }
                move_diff = move_speed * Time.fixedDeltaTime;
                Enemies.Sound(transform, move_diff * sound_amplifier, max_sound, hearing_threshold_change);
                move_time += move_diff;
                pillar_rigidbody.MovePosition(Vector3.Lerp(under_ground_point, ground_point, move_time));
                break;
            case MoveStates.still:
                current_sleep_time -= Time.deltaTime;
                if (current_sleep_time < 0f)
                {
                    pillar_rigidbody.WakeUp();
                    move_state = MoveStates.down;
                    break;
                }
                break;
            case MoveStates.down:
                if (move_time == 0f)
                {
                    move_state = MoveStates.up;
                    if (should_be_deleted)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        pillar_rigidbody.Sleep();
                        gameObject.SetActive(false);
                    }
                    break;
                }
                move_diff = move_speed * Time.fixedDeltaTime;
                Enemies.Sound(transform, move_diff * sound_amplifier, max_sound, hearing_threshold_change);
                move_time -= move_diff;
                pillar_rigidbody.MovePosition(Vector3.Lerp(under_ground_point, ground_point, move_time));
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (deal_damage_by_collision && Layer.IsInLayer(Layer.enemy, collision.gameObject.layer) && move_state == MoveStates.up)
        {
            collision.gameObject.GetComponent<EnemyAI>().Damage(damage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (deal_damage_by_trigger && Layer.IsInLayer(Layer.enemy, other.gameObject.layer) && move_state == MoveStates.up)
        {
            if (other.gameObject.GetComponent<EnemyAI>().Damage(damage, damage_id, 0.25f))
            {
                other.gameObject.GetComponent<Agent>().AddForce(750f * transform.up, ForceMode.Impulse, tumble_time);
            }
        }
    }
}
