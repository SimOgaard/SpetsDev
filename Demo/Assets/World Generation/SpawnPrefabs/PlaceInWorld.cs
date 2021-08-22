using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceInWorld : MonoBehaviour
{
    public SpawnInstruction this_instruction;
    public SpawnInstruction child_instruction;

    public static void SetRecursiveToGameWorld(GameObject obj)
    {
        if ((MousePoint.layer_mask_world_colliders_6.value & 1 << obj.layer) != 0)
        {
            obj.isStatic = true;
        }
        else if (((1 << 17) & 1 << obj.layer) != 0)
        {
            obj.layer = 0;
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

    public static Vector3 GetVectorBySharedXYZ(float x, float y, float z, SpawnInstruction.SharedXYZ sharedXYZ)
    {
        switch (sharedXYZ)
        {
            case SpawnInstruction.SharedXYZ.none:
                return new Vector3(x, y, z);
            case SpawnInstruction.SharedXYZ.xy:
                return new Vector3(x, x, z);
            case SpawnInstruction.SharedXYZ.xz:
                return new Vector3(x, y, x);
            case SpawnInstruction.SharedXYZ.yz:
                return new Vector3(x, y, y);
            case SpawnInstruction.SharedXYZ.xyz:
                return new Vector3(x, x, x);
        }
        return Vector3.zero;
    }

    public static Vector3 RandomVector(Vector3 min_scale, Vector3 max_scale, SpawnInstruction.SharedXYZ sharedXYZ)
    {
        float x = Random.Range(min_scale.x, max_scale.x);
        float y = Random.Range(min_scale.y, max_scale.y);
        float z = Random.Range(min_scale.z, max_scale.z);

        return GetVectorBySharedXYZ(x, y, z, sharedXYZ);
    }

    public static Vector3 RandomVector(Vector3 min_scale, Vector3 max_scale, SpawnInstruction.SharedXYZ sharedXYZ, Vector3 current_scale)
    {
        float x = Random.Range(min_scale.x, max_scale.x) * current_scale.x;
        float y = Random.Range(min_scale.y, max_scale.y) * current_scale.y;
        float z = Random.Range(min_scale.z, max_scale.z) * current_scale.z;

        return GetVectorBySharedXYZ(x, y, z, sharedXYZ);
    }

    public static Vector3 RandomVector(Vector3 min_scale, Vector3 max_scale, SpawnInstruction.SharedXYZ sharedXYZ, Quaternion rotation)
    {
        float x = Random.Range(min_scale.x, max_scale.x);
        float y = Random.Range(min_scale.y, max_scale.y);
        float z = Random.Range(min_scale.z, max_scale.z);

        return rotation * GetVectorBySharedXYZ(x, y, z, sharedXYZ);
    }

    public static Vector3[] PointNormalWithRayCast(Vector3 origin, Vector3 random_vector, Vector3 direction, LayerMask placable_layer_mask)
    {
        Physics.SyncTransforms();

        RaycastHit[] hits = Physics.RaycastAll(origin + Vector3.up * 100f + random_vector, Vector3.down, 400f, placable_layer_mask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            if ((placable_layer_mask.value & 1 << hits[i].transform.gameObject.layer) != 0)
            {
                return new Vector3[] { hits[i].point, hits[i].normal };
            }
        }
        return null;
    }

    public static void CountDownChild(ref int child_transform_amount, bool is_parrent)
    {
        if (is_parrent)
        {
            child_transform_amount = 0;
        }
        else
        {
            child_transform_amount--;
        }
    }

    private bool DestroyOnMissingChildren(int child_count, int min_child_amount_required)
    {
        if (child_count < min_child_amount_required)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    public static void Spawn(Transform transform, SpawnInstruction spawn_instruction, ref int child_transform_amount, bool is_parrent = false)
    {
        // Should this Transform spawn?
        if (Random.value > spawn_instruction.spawn_chance)
        {
            Destroy(transform.gameObject);
            CountDownChild(ref child_transform_amount, is_parrent);
            return;
        }

        // Raycast
        Vector3 normal = Vector3.up;
        Vector3[] hit_data = PointNormalWithRayCast(
            transform.position,
            RandomVector(spawn_instruction.min_ray_position, spawn_instruction.max_ray_position, spawn_instruction.shared_ray_position, transform.localScale),
            Vector3.down,
            spawn_instruction.ray_layer_mask
        );

        // Place transform.
        if (hit_data != null)
        {
            transform.position = hit_data[0];
            normal = hit_data[1];
        }
        else if (spawn_instruction.ray_layer_mask.value != 0)
        {
            Destroy(transform.gameObject);
            CountDownChild(ref child_transform_amount, is_parrent);
            return;
        }

        // Apply rotation, scale and position offsets.
        if (spawn_instruction.rotate_twords_ground_normal)
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            transform.RotateAround(transform.position, transform.up, RandomRotationAroundAxis(spawn_instruction.rotation_offset, spawn_instruction.rotation_increment));
        }
        else if (spawn_instruction.rotation_offset + spawn_instruction.rotation_increment != 0)
        {
            if (spawn_instruction.reset_rotation)
            {
                transform.rotation = Quaternion.identity;
            }
            transform.rotation = RandomRotationObject(spawn_instruction.rotation_offset, spawn_instruction.rotation_increment);
        }
        transform.localScale = RandomVector(spawn_instruction.min_scale, spawn_instruction.max_scale, spawn_instruction.shared_scales, transform.localScale);
        transform.localPosition += RandomVector(spawn_instruction.min_position, spawn_instruction.max_position, spawn_instruction.shared_position, transform.localScale);

        // Change parrent on delay so that destroys further in this script can effect them aswell.
        if (spawn_instruction.parrent_name != SpawnInstruction.PlacableGameObjectsParrent.keep)
        {
            transform.gameObject.AddComponent<ChangeParrentDelay>().SetParrent(GameObject.Find(spawn_instruction.parrent_name.ToString()).transform);
        }
    }

    public void InitAsChild(ref int child_transform_amount)
    {
        Spawn(transform, this_instruction, ref child_transform_amount);
    }

    public void InitAsParrent(float x, float z)
    {
        // Get amount of children of gameobject. Because Destroy() doesnt regrister in transform.childCount.
        int child_transform_amount = transform.childCount;

        // Spawn this gameobject.
        transform.position = new Vector3(x, 0f, z);
        Spawn(transform, this_instruction, ref child_transform_amount, true);
        if (DestroyOnMissingChildren(child_transform_amount, this_instruction.min_child_amount_required))
        {
            Destroy(gameObject);
            return;
        }

        // Spawn children of gameobject.
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out PlaceInWorld place_in_world))
            {
                place_in_world.InitAsChild(ref child_transform_amount);
            }
            else if (child_instruction != null)
            {
                Spawn(child, child_instruction, ref child_transform_amount);
            }
            if (DestroyOnMissingChildren(child_transform_amount, this_instruction.min_child_amount_required))
            {
                return;
            }
        }

        // Check if bounding boxes intersects.
        Collider[] colliders = GetComponents<Collider>();
        if (!this_instruction.ignore_bounding_boxes)
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
        if (transform.parent == null)
        {
            SetRecursiveToGameWorld(gameObject);
        }
        Destroy(this);
    }
    /*
    public void Place(Transform child, ref int child_count, LayerMask placable_layer_mask)
    {
        if (Random.value > child_spawn_chanse)
        {
            Destroy(child.gameObject);
            child_count--;
            return;
        }

        Vector3 normal = Vector3.up;
        if (!keep_child_local_position)
        {
            Vector3[] hit_data = PointNormalWithRayCast(transform.position + Vector3.up * 100f + RandomVector(min_position_ray, max_position_ray, shared_position_ray), Vector3.down, placable_layer_mask);
            if (hit_data != null)
            {
                child.position = hit_data[0];
                normal = hit_data[1];
            }
            else
            {
                Destroy(gameObject);
                child_count--;
                return;
            }
        }

        if (rotate_twords_ground_normal)
        {
            child.rotation = Quaternion.FromToRotation(Vector3.up, normal);
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
    }

    private void PlaceParrent(Transform child, ref int child_count, LayerMask placable_layer_mask)
    {
        gameObject.SetActive(false);
        if (Random.value > spawn_chance)
        {
            child_count = 0;
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 100f + RandomVector(min_position_ray, max_position_ray, shared_position_ray), Vector3.down, out hit, Mathf.Infinity, placable_layer_mask))
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

    //private bool is_child = false;
    //private int child_count;

    private void Awake()
    {
        is_child = transform.parent != null;
        if (is_child)
        {
            return;
        }

        gameObject.layer = 17;
        child_count = transform.childCount;

        PlaceParrent();
        if (child_count < min_child_amount_required)
        {
            Destroy(gameObject);
            return;
        }
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out PlaceInWorld place_in_world))
            {
                place_in_world.PlaceParrent(child, ref child_count, child_spawning_layer_mask);
            }
            else
            {
                Place(child, ref child_count, child_spawning_layer_mask);
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

        SetRecursiveToGameWorld(gameObject);
    }

    private void Start()
    {
        if (parrent_name !=  PlacableGameObjectsParrent.keep || !is_child)
        {
            transform.parent = GameObject.Find(parrent_name.ToString()).transform;
        }
    }
    */
}
