using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceInWorld : MonoBehaviour
{
    public SpawnInstruction this_instruction;
    public SpawnInstruction child_instruction;

    private List<Collider> bounding_boxes = new List<Collider>();

    [SerializeField] private int child_transform_amount = 0;

    private void AddToBoundingBoxesLocal(List<Collider> bounds)
    {
        bounding_boxes.AddRange(bounds);
    }

    public static void SetRecursiveToGameWorld(GameObject obj)
    {
        if (Layer.IsInLayer(Layer.Mask.spawned_game_world, obj.layer))
        {
            obj.layer = Layer.game_world;
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
        RaycastHit[] hits = Physics.RaycastAll(origin + Vector3.up * 100f + random_vector, Vector3.down, 400f, placable_layer_mask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            if (Layer.IsInLayer(placable_layer_mask, hits[i].transform.gameObject.layer))
            {
                return new Vector3[] { hits[i].point, hits[i].normal };
            }
        }
        return null;
    }

    private void CountDownChild(bool is_parrent)
    {
        if (is_parrent)
        {
            child_transform_amount = -1;
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

    public static IEnumerator Spawn(WaitForFixedUpdate wait, System.Action<List<Collider>> AddToBoundingBoxesLocal, System.Func<List<Collider>> GetBoundingBoxes, System.Action<bool> CountDownChild, Transform transform, Transform chunk_transform, SpawnInstruction spawn_instruction, bool is_parrent = false)
    {
        // Should this Transform spawn?
        if (spawn_instruction.noise_layer.enabled)
        {
            Noise.NoiseLayer noise = new Noise.NoiseLayer(spawn_instruction.noise_layer);
            float noise_value = noise.GetNoiseValue(transform.position.x, transform.position.z);

            if (!(Random.value <= spawn_instruction.spawn_chance || (noise_value > spawn_instruction.spawn_range_noise.x && noise_value < spawn_instruction.spawn_range_noise.y && Random.value <= spawn_instruction.spawn_chance_noise)))
            {
                Destroy(transform.gameObject);
                CountDownChild(is_parrent);
                yield break;
            }
        }
        else
        {
            if (!(Random.value <= spawn_instruction.spawn_chance))
            {
                Destroy(transform.gameObject);
                CountDownChild(is_parrent);
                yield break;
            }
        }

        // Raycast
        yield return wait;
        if (transform == null)
        {
            yield break;
        }
        Vector3 normal = Vector3.up;
        Vector3[] hit_data = PointNormalWithRayCast(
            transform.position,
            RandomVector(spawn_instruction.min_ray_position, spawn_instruction.max_ray_position, spawn_instruction.shared_ray_position, transform.localScale),
            Vector3.down,
            spawn_instruction.ray_layer_mask
        );

        // Place transform.
        if (hit_data != null && Vector3.Dot(hit_data[1], Vector3.up) >= Mathf.Cos(spawn_instruction.max_rotation * Mathf.Deg2Rad))
        {
            transform.position = hit_data[0];
            normal = hit_data[1];
        }
        else if (spawn_instruction.ray_layer_mask.value != 0)
        {
            Destroy(transform.gameObject);
            CountDownChild(is_parrent);
            yield break;
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
        if (!is_parrent && spawn_instruction.parrent_name != SpawnInstruction.PlacableGameObjectsParrent.keep)
        {
            transform.parent = chunk_transform.Find(SpawnInstruction.GetHierarchyName(spawn_instruction.parrent_name));
        }

        yield return wait;
        if (transform == null)
        {
            yield break;
        }

        if (transform.TryGetComponent(out BoundingBoxes bounding_boxes_object))
        {
            List<Collider> bounds = bounding_boxes_object.GetBoundingBoxes();
            List<Collider> bounds_to_be_removed = new List<Collider>();

            if (!spawn_instruction.ignore_bounding_boxes)
            {
                bool destroy_children = bounding_boxes_object.ShouldDestroyChildren();

                foreach (Collider spawning_collider in bounds)
                {
                    foreach (Collider instanciated_colliders in GetBoundingBoxes())
                    {
                        if (destroy_children)
                        {
                            //Debug.Log("intersected: " + spawning_collider.bounds.Intersects(instanciated_colliders.bounds));
                            if (spawning_collider.bounds.Intersects(instanciated_colliders.bounds))
                            {
                                CountDownChild(is_parrent);
                                bounds_to_be_removed.Add(spawning_collider);
                                Destroy(spawning_collider.gameObject);
                            }
                        }
                        else
                        {
                            //Debug.Log("intersected: " + spawning_collider.bounds.Intersects(instanciated_colliders.bounds));
                            if (spawning_collider.bounds.Intersects(instanciated_colliders.bounds))
                            {
                                CountDownChild(is_parrent);
                                Destroy(transform.gameObject);
                                yield break;
                            }
                        }
                    }
                }

                foreach (Collider bound in bounds_to_be_removed)
                {
                    bounds.Remove(bound);
                }

                AddToBoundingBoxesLocal(bounds);
                Destroy(bounding_boxes_object);
                yield break;
            }

            AddToBoundingBoxesLocal(bounds);
            Destroy(bounding_boxes_object);
            yield break;
        }
        else
        {
            yield break;
        }
    }

    public IEnumerator InitAsChild(WaitForFixedUpdate wait, System.Action<List<Collider>> AddToBoundingBoxesLocal, System.Func<List<Collider>> GetBoundingBoxes, System.Action<bool> CountDownChild, Transform chunk_transform)
    {
        yield return StartCoroutine(Spawn(wait, AddToBoundingBoxesLocal, GetBoundingBoxes, CountDownChild, transform, chunk_transform, this_instruction));
    }

    public IEnumerator InitAsParrent(WaitForFixedUpdate wait, System.Action<List<Collider>> AddToBoundingBoxes, System.Func<List<Collider>> GetBoundingBoxes, float x, float z, Transform chunk_transform)
    {
        // Get amount of children of gameobject. Because Destroy() doesnt regrister in transform.childCount.
        child_transform_amount = transform.childCount;

        // Spawn this gameobject.
        transform.position = new Vector3(x, 0f, z);
        yield return StartCoroutine(Spawn(wait, AddToBoundingBoxesLocal, GetBoundingBoxes, CountDownChild, transform, chunk_transform, this_instruction, true));
        if (this == null)
        {
            yield break;
        }

        if (DestroyOnMissingChildren(child_transform_amount, this_instruction.min_child_amount_required))
        {
            yield break;
        }

        // Spawn children of gameobject.
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out PlaceInWorld place_in_world))
            {
                yield return StartCoroutine(place_in_world.InitAsChild(wait, AddToBoundingBoxesLocal, GetBoundingBoxes, CountDownChild, chunk_transform));
                Destroy(place_in_world);
            }
            else if (child_instruction != null)
            {
                yield return StartCoroutine(Spawn(wait, AddToBoundingBoxesLocal, GetBoundingBoxes, CountDownChild, child, chunk_transform, child_instruction));
            }
            if (DestroyOnMissingChildren(child_transform_amount, this_instruction.min_child_amount_required))
            {
                yield break;
            }
        }

        AddToBoundingBoxes(bounding_boxes);
        transform.parent = chunk_transform.Find(SpawnInstruction.GetHierarchyName(this_instruction.parrent_name));
        Destroy(this);
    }
}
