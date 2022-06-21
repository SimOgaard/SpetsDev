using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Iterates through all points in allFireSpots and returns damage if transform is within given distance of point.
/// </summary>
public class DamageByFire : MonoBehaviour
{
    public List<Vector3> allFireSpots;

    [SerializeField] private float fireDistance = 2f;
    private float fireDamage = 1f;

    /// <summary>
    /// Updates fire distance and invokes DamageClose() repeating with interval between each invoke to minimize compute and damage applied.
    /// </summary>
    private void Start()
    {
        UpdateFireDistance(fireDistance);
    }

    /// <summary>
    /// Updates the squared fire distance.
    /// </summary>
    public void UpdateFireDistance(float fireDistance)
    {
        this.fireDistance = fireDistance;
    }

    public void UpdateFireDamage(float fireDamage)
    {
        this.fireDamage = fireDamage;
    }

    /// <summary>
    /// Iterates through all points in allFireSpots and returns combined damage if position with given radius is within distance of point.
    /// </summary>
    public float DamageStacked(Vector3 position, float radius)
    {
        float damage = 0f;
        position.y = 0f;
        foreach (Vector3 firePos in allFireSpots)
        {
            float distance = (position - firePos).magnitude;
            if (distance - radius < fireDistance)
            {
                damage += fireDamage;
            }
        }
        return damage;
    }

    /// <summary>
    /// Iterates through all points in allFireSpots and returns damage if position with given radius is within distance of point.
    /// </summary>
    public float Damage(Vector3 position, float radius)
    {
        position.y = 0f;
        foreach (Vector3 firePos in allFireSpots)
        {
            float distance = (position - firePos).magnitude;
            if (distance - radius < fireDistance)
            {
                return fireDamage;
            }
        }
        return 0f;
    }
}
