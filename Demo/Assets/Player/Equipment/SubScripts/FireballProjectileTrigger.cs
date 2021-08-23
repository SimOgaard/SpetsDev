using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handels enemy trigger for fireball projectile.
/// </summary>
public class FireballProjectileTrigger : MonoBehaviour
{
    private FireballProjectile fireball_projectile;
    private string damage_id = System.Guid.NewGuid().ToString();

    public void Init(FireballProjectile fireball_projectile)
    {
        this.fireball_projectile = fireball_projectile;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Layer.IsInLayer(Layer.enemy, other.gameObject.layer))
        {
            other.gameObject.GetComponent<EnemyAI>().Damage(fireball_projectile.OnHitDamage(), damage_id, 0.25f);
            fireball_projectile.Explode();
            return;
        }
    }
}
