using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Iterates through all points in all_fire_spots and returns damage if transform is within given distance of point.
/// </summary>
public class DamageByFire : MonoBehaviour
{
    public List<Vector3> all_fire_spots;

    [SerializeField] private float fire_distance = 2f;
    private float fire_damage = 1f;

    /// <summary>
    /// Updates fire distance and invokes DamageClose() repeating with interval between each invoke to minimize compute and damage applied.
    /// </summary>
    private void Start()
    {
        UpdateFireDistance(fire_distance);
    }

    /// <summary>
    /// Updates the squared fire distance.
    /// </summary>
    public void UpdateFireDistance(float fire_distance)
    {
        this.fire_distance = fire_distance;
    }

    public void UpdateFireDamage(float fire_damage)
    {
        this.fire_damage = fire_damage;
    }

    /// <summary>
    /// Iterates through all points in all_fire_spots and returns damage if position with given radius is within distance of point.
    /// </summary>
    public float Damage(Vector3 position, float radius)
    {
        position.y = 0f;
        foreach (Vector3 fire_pos in all_fire_spots)
        {
            float distance = (position - fire_pos).magnitude;
            if (distance - radius < fire_distance)
            {
                return fire_damage;
            }
        }
        return 0f;
    }
}
