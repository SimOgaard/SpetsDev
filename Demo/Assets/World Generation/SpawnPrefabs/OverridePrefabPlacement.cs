using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverridePrefabPlacement : MonoBehaviour
{
    [Header("Spawning Chance")]
    [Range(0f, 1f)] [SerializeField] private float spawn_chance = 1f;

    [Header("Spawning Rotation")]
    [SerializeField] private bool rotate_twords_ground_normal = false;
    [SerializeField] private bool reset_rotation = true;
    [Range(0f, 359f)] [SerializeField] private float rotation_offset = 45f;
    [Range(0f, 359f)] [SerializeField] private float rotation_increment = 0f;

    [Header("Spawning Location")]
    [SerializeField] private bool keep_child_local_position = true;
    [SerializeField] private PlaceInWorld.SharedXYZ shared_position_ray = PlaceInWorld.SharedXYZ.none;
    [SerializeField] private Vector3 min_position_ray = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 max_position_ray = new Vector3(0, 0, 0);
    [SerializeField] private PlaceInWorld.SharedXYZ shared_position = PlaceInWorld.SharedXYZ.none;
    [SerializeField] private Vector3 min_position = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 max_position = new Vector3(0, 0, 0);

    [Header("Spawning Scale")]
    [SerializeField] private PlaceInWorld.SharedXYZ shared_scales = PlaceInWorld.SharedXYZ.none;
    [SerializeField] private Vector3 min_scale = new Vector3(1, 1, 1);
    [SerializeField] private Vector3 max_scale = new Vector3(1, 1, 1);

    [Header("Hiearchy")]
    [SerializeField] private bool override_parrent = false;
    [SerializeField] private PlaceInWorld.PlacableGameObjectsParrent parrent_name = PlaceInWorld.PlacableGameObjectsParrent.Interactables;

    public void Place(ref int child_count, LayerMask placable_layer_mask)
    {
        if (Random.value > spawn_chance)
        {
            Destroy(gameObject);
            child_count--;
            return;
        }

        RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 100f + PlaceInWorld.RandomVector(min_position_ray, max_position_ray, shared_position_ray), Vector3.down, 400f, placable_layer_mask, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
        Vector3 hit_normal = Vector3.up;
        if (!keep_child_local_position)
        {
            bool broke_loop = false;
            for (int i = 0; i < hits.Length; i++)
            {
                if ((placable_layer_mask.value & 1 << hits[i].transform.gameObject.layer) != 0)
                {
                    transform.position = hits[i].point;
                    hit_normal = hits[i].normal;
                    broke_loop = true;
                    break;
                }
            }
            if (!broke_loop)
            {
                Destroy(gameObject);
                child_count--;
                return;
            }
        }

        if (rotate_twords_ground_normal)
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit_normal);
            transform.RotateAround(transform.position, transform.up, PlaceInWorld.RandomRotationAroundAxis(rotation_offset, rotation_increment));
        }
        else if (rotation_offset + rotation_increment != 0)
        {
            if (reset_rotation)
            {
                transform.rotation = Quaternion.identity;
            }
            transform.rotation = PlaceInWorld.RandomRotationObject(rotation_offset, rotation_increment);
        }
        transform.localScale = PlaceInWorld.RandomVector(min_scale, max_scale, shared_scales, transform.localScale);
        transform.localPosition += PlaceInWorld.RandomVector(min_position, max_position, shared_position, transform.rotation);
        PlaceInWorld.SetRecursiveToGameWorld(gameObject);
    }

    private void Start()
    {
        if (override_parrent)
        {
            transform.parent = GameObject.Find(parrent_name.ToString()).transform;
        }
    }
}
