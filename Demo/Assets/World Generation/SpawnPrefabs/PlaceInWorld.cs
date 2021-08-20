using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceInWorld : MonoBehaviour
{
    [Header("Spawning Chance")]
    [Range(0f, 1f)] [SerializeField] private float spawn_chance = 1f;
    [Range(0f, 1f)] [SerializeField] private float child_spawn_chanse = 1f;

    [Header("Spawning Rotation")]
    [SerializeField] private bool rotate_twords_ground_normal = false;
    [SerializeField] private bool reset_rotation = true;
    [Range(0f, 359f)] [SerializeField] private float rotation_offset = 0f;
    [Range(0f, 359f)] [SerializeField] private float rotation_increment = 0f;
    [Range(0f, 359f)] [SerializeField] private float child_rotation_offset = 0f;
    [Range(0f, 359f)] [SerializeField] private float child_rotation_increment = 0f;

    public enum SharedXYZ { none, xy, xz, yz, xyz }

    [Header("Spawning Scale")]
    [SerializeField] private SharedXYZ shared_scales = SharedXYZ.none;
    [SerializeField] private Vector3 min_scale = new Vector3(1, 1, 1);
    [SerializeField] private Vector3 max_scale = new Vector3(1, 1, 1);

    [Header("Spawning Location")]
    [SerializeField] private SharedXYZ shared_position_ray = SharedXYZ.none;
    [SerializeField] private Vector3 min_position_ray = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 max_position_ray = new Vector3(0, 0, 0);
    [SerializeField] private SharedXYZ shared_position = SharedXYZ.none;
    [SerializeField] private Vector3 min_position = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 max_position = new Vector3(0, 0, 0);

    [Header("Child Spawning Scale")]
    [SerializeField] private SharedXYZ child_shared_scales = SharedXYZ.none;
    [SerializeField] private Vector3 child_min_scale = new Vector3(1, 1, 1);
    [SerializeField] private Vector3 child_max_scale = new Vector3(1, 1, 1);

    [Header("Child Spawning Location")]
    [SerializeField] private bool keep_child_local_position = true;
    [SerializeField] private SharedXYZ child_shared_position_ray = SharedXYZ.none;
    [SerializeField] private Vector3 child_min_position_ray = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 child_max_position_ray = new Vector3(0, 0, 0);
    [SerializeField] private SharedXYZ child_shared_position = SharedXYZ.none;
    [SerializeField] private Vector3 child_min_position = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 child_max_position = new Vector3(0, 0, 0);

    [Header("Spawning Bounds")]
    [SerializeField] private bool ignore_bounding_boxes = false;

    [Header("Hiearchy")]
    [SerializeField] private PlacableGameObjectsParrent parrent_name = PlacableGameObjectsParrent.land_marks;
    public enum PlacableGameObjectsParrent { ground_mesh, land_marks, rocks, trees, Interactables, UsedInteractables, Enemies, lGolems, mGolems, sGolems }

    [Header("Child Spawning LayerMask")]
    [SerializeField] private LayerMask child_spawning_layer_mask = (1 << 12);

    private int child_count;

    public static void SetRecursiveToGameWorld(GameObject obj)
    {
        if ((MousePoint.layer_mask_world_colliders_6.value & 1 << obj.layer) == 0)
        {
            obj.isStatic = true;
        }

        foreach (Transform child in obj.transform)
        {
            SetRecursiveToGameWorld(child.gameObject);
        }
    }

    public static Quaternion RandomRotationObject(float rotation_offset, float rotation_increment)
    {
        return Quaternion.Euler(0f, RandomRotationAroundAxis(rotation_offset, rotation_increment), 0f);
    }

    public static float RandomRotationAroundAxis(float rotation_offset, float rotation_increment)
    {
        return rotation_offset + rotation_increment * Mathf.RoundToInt(Random.Range(0f, 360f));
    }

    public static Vector3 RandomVector(Vector3 min_scale, Vector3 max_scale, SharedXYZ sharedXYZ)
    {
        float x = Random.Range(min_scale.x, max_scale.x);
        float y = Random.Range(min_scale.y, max_scale.y);
        float z = Random.Range(min_scale.z, max_scale.z);

        switch (sharedXYZ)
        {
            case SharedXYZ.none:
                return new Vector3(x, y, z);
            case SharedXYZ.xy:
                return new Vector3(x, x, z);
            case SharedXYZ.xz:
                return new Vector3(x, y, x);
            case SharedXYZ.yz:
                return new Vector3(x, y, y);
            case SharedXYZ.xyz:
                return new Vector3(x, x, x);
        }
        return Vector3.zero;
    }

    public static Vector3 RandomVector(Vector3 min_scale, Vector3 max_scale, SharedXYZ sharedXYZ, Vector3 current_scale)
    {
        float x = Random.Range(min_scale.x, max_scale.x) * current_scale.x;
        float y = Random.Range(min_scale.y, max_scale.y) * current_scale.y;
        float z = Random.Range(min_scale.z, max_scale.z) * current_scale.z;

        switch (sharedXYZ)
        {
            case SharedXYZ.none:
                return new Vector3(x, y, z);
            case SharedXYZ.xy:
                return new Vector3(x, x, z);
            case SharedXYZ.xz:
                return new Vector3(x, y, x);
            case SharedXYZ.yz:
                return new Vector3(x, y, y);
            case SharedXYZ.xyz:
                return new Vector3(x, x, x);
        }
        return Vector3.zero;
    }

    public static Vector3 RandomVector(Vector3 min_scale, Vector3 max_scale, SharedXYZ sharedXYZ, Quaternion rotation)
    {
        float x = Random.Range(min_scale.x, max_scale.x);
        float y = Random.Range(min_scale.y, max_scale.y);
        float z = Random.Range(min_scale.z, max_scale.z);

        switch (sharedXYZ)
        {
            case SharedXYZ.none:
                return rotation * new Vector3(x, y, z);
            case SharedXYZ.xy:
                return rotation * new Vector3(x, x, z);
            case SharedXYZ.xz:
                return rotation * new Vector3(x, y, x);
            case SharedXYZ.yz:
                return rotation * new Vector3(x, y, y);
            case SharedXYZ.xyz:
                return rotation * new Vector3(x, x, x);
        }
        return Vector3.zero;
    }

    private void Place(Transform child)
    {
        if (Random.value > child_spawn_chanse)
        {
            Destroy(child.gameObject);
            child_count--;
            return;
        }

        RaycastHit hit;
        Vector3 hit_normal = Vector3.up;
        if (!keep_child_local_position)
        {
            if (Physics.Raycast(child.position + Vector3.up * 100f + RandomVector(child_min_position_ray, child_max_position_ray, child_shared_position_ray), Vector3.down, out hit, 400f, child_spawning_layer_mask))
            {
                child.position = hit.point;
                hit_normal = hit.normal;
            }
            else
            {
                Destroy(child.gameObject);
                child_count--;
                return;
            }
        }

        if (rotate_twords_ground_normal)
        {
            child.rotation = Quaternion.FromToRotation(Vector3.up, hit_normal);
            child.RotateAround(child.position, child.up, RandomRotationAroundAxis(child_rotation_offset, child_rotation_increment));
        }
        else if (child_rotation_offset + child_rotation_increment != 0)
        {
            if (reset_rotation)
            {
                child.rotation = Quaternion.identity;
            }
            child.rotation = RandomRotationObject(child_rotation_offset, child_rotation_increment);
        }
        child.localScale = RandomVector(child_min_scale, child_max_scale, child_shared_scales, child.localScale);
        child.localPosition += RandomVector(child_min_position, child_max_position, child_shared_position);
        SetRecursiveToGameWorld(child.gameObject);
    }

    private void PlaceParrent()
    {
        gameObject.SetActive(false);
        if (Random.value > spawn_chance)
        {
            child_count = 0;
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 100f + RandomVector(min_position_ray, max_position_ray, shared_position_ray), Vector3.down, out hit, Mathf.Infinity, MousePoint.layer_mask_world))
        {
            transform.position = hit.point;
            transform.rotation = RandomRotationObject(rotation_offset, rotation_increment);

            transform.localScale = RandomVector(min_scale, max_scale, shared_scales, transform.localScale);
            transform.localPosition += RandomVector(min_position, max_position, shared_position);
        }
        else
        {
            child_count = 0;
        }
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        gameObject.layer = 17;
        child_count = transform.childCount;
        if (parrent_name == PlacableGameObjectsParrent.sGolems || parrent_name == PlacableGameObjectsParrent.mGolems)
        {
            child_count-=1;
        }

        PlaceParrent();
        if (child_count <= 0)
        {
            Destroy(gameObject);
            return;
        }
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out OverridePrefabPlacement override_prefab_placement))
            {
                override_prefab_placement.Place(ref child_count, child_spawning_layer_mask);
            }
            else
            {
                Place(child);
            }

            if (child_count <= 0)
            {
                Destroy(gameObject);
                return;
            }
        }

        Collider[] colliders = GetComponents<Collider>();
        if (!ignore_bounding_boxes)
        {
            foreach (Collider col in SpawnPrefabs.bounding_boxes)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].bounds.Intersects(col.bounds))
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }

        SpawnPrefabs.bounding_boxes.AddRange(colliders);
    }

    private void Start()
    {
        transform.parent = GameObject.Find(parrent_name.ToString()).transform;
    }
}
