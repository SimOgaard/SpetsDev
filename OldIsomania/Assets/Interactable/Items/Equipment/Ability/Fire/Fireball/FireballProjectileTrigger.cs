using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handels enemy trigger for fireball projectile.
/// </summary>
public class FireballProjectileTrigger : MonoBehaviour
{
    /*
    private FireballProjectile fireballProjectile;
    private string damageId = System.Guid.NewGuid().ToString();

    public void Init(FireballProjectile fireballProjectile)
    {
        this.fireballProjectile = fireballProjectile;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Layer.IsInLayer(Layer.enemy, other.gameObject.layer))
        {
            other.transform.parent.GetComponent<EnemyAI>().Damage(fireballProjectile.OnHitDamage(), damageId, 0.25f);
            fireballProjectile.Explode();
            return;
        }
    }
    */
}
