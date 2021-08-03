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
    private int layer_mask = 1 << 12;

    public bool should_be_deleted = false;

    public bool deal_damage = false;
    public float damage = 0f;

    List<CombineInstance> combined_mesh_instance = new List<CombineInstance>();

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
    public void SetSharedValues(float sleep_time, float move_speed, float height)
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
        gameObject.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
    }

    /// <summary>
    /// Initilizes a kinematic rigidbody.
    /// </summary>
    private void Start()
    {
        pillar_rigidbody = gameObject.AddComponent<Rigidbody>();
        pillar_rigidbody.isKinematic = true;
        gameObject.layer = 17;
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
        if (Physics.Raycast(point + new Vector3(0f, 20f, 0f), Vector3.down, out hit_data, 40f, layer_mask))
        {
            transform.position = hit_data.point - transform.rotation * new Vector3(0f, transform.localScale.y * 0.5f, 0f);
            this.ground_point = hit_data.point;
            this.under_ground_point = transform.position;
        }
        else
        {
            transform.position = point - transform.rotation * new Vector3(0f, transform.localScale.y * 0.5f, 0f);
            this.ground_point = point;
            this.under_ground_point = transform.position;
        }
    }

    /// <summary>
    /// Places pillar at ground given a point.
    /// </summary>
    public void PlacePillar(Vector3 point, float player_y, float y_displacement_ignore_threshold)
    {
        if (Physics.Raycast(point + new Vector3(0f, 20f, 0f), Vector3.down, out hit_data, 40f, layer_mask))
        {
            if (hit_data.point.y - player_y > y_displacement_ignore_threshold)
            {
                hit_data.point = point;
            }

            transform.position = hit_data.point - transform.rotation * new Vector3(0f, transform.localScale.y * 0.5f, 0f);
            this.ground_point = hit_data.point;
            this.under_ground_point = transform.position;
        }
        else
        {
            transform.position = point - transform.rotation * new Vector3(0f, transform.localScale.y * 0.5f, 0f);
            this.ground_point = point;
            this.under_ground_point = transform.position;
        }
    }

    /// <summary>
    /// Controlls all three stages a pillar goes through.
    /// Moving up and down and standing still. States can be changed outside of script.
    /// </summary>
    private void Update()
    {
        switch (move_state)
        {
            case MoveStates.up:
                Vector3 diff_up = ground_point - transform.position;
                if (diff_up.sqrMagnitude < 0.1f)
                {
                    current_sleep_time = sleep_time;
                    move_state = MoveStates.still;
                    transform.position = ground_point;
                    break;
                }

                Vector3 dir_up = diff_up.normalized;
                pillar_rigidbody.MovePosition(transform.position + dir_up * move_speed * Time.deltaTime);
                break;
            case MoveStates.still:
                current_sleep_time -= Time.deltaTime;
                if (current_sleep_time < 0f)
                {
                    pillar_rigidbody.WakeUp();
                    move_state = MoveStates.down;
                    break;
                }
                pillar_rigidbody.Sleep();
                break;
            case MoveStates.down:
                Vector3 diff_down = under_ground_point - transform.position;
                if (diff_down.sqrMagnitude < 0.1f)
                {
                    move_state = MoveStates.up;
                    if (should_be_deleted)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                    break;
                }

                Vector3 dir_down = diff_down.normalized;
                pillar_rigidbody.MovePosition(transform.position + dir_down * move_speed * Time.deltaTime);
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (deal_damage && collision.gameObject.layer == 16)
        {
            collision.gameObject.GetComponent<EnemyAI>().current_health -= damage;
        }
    }
}
