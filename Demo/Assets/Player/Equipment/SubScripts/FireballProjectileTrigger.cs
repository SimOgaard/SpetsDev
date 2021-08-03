using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handels enemy trigger for fireball projectile.
/// </summary>
public class FireballProjectileTrigger : MonoBehaviour
{
    private FireballProjectile fireball_projectile;

    public void Init(FireballProjectile fireball_projectile)
    {
        this.fireball_projectile = fireball_projectile;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 16)
        {
            other.gameObject.GetComponent<EnemyAI>().current_health -= fireball_projectile.on_collision_damage;
            fireball_projectile.Explode();
            return;
        }
    }
}
