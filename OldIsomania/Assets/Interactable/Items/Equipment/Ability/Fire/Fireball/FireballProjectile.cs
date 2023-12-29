using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls fireball game objects.
/// </summary>
public class FireballProjectile : MonoBehaviour
{
    /*
    private string damageId = System.Guid.NewGuid().ToString();

    private FireballAbility fireballAbility;
    private Rigidbody rigidBody;
    private MeshRenderer meshRenderer;

    [SerializeField] private Vector3 fireHeatLiftForce = new Vector3(0f, 25f, 0f);
    [SerializeField] private float airEffectMagnitude = 0.1f;
    public bool isBurnedOut = false;

    private Vector3 lastBurnContactPoint = Vector3.zero;

    private bool hasExploded = false;

    /// <summary>
    /// Returns ability on hit damage.
    /// </summary>
    public float OnHitDamage()
    {
        return isBurnedOut ? fireballAbility.onCollisionDamageBurnedOut : fireballAbility.onCollisionDamage;
    }

    /// <summary>
    /// Set variables when initializing fireball in FireballAbility.
    /// </summary>
    public void InitVar(FireballAbility fireballAbility, Rigidbody rigidBody, MeshRenderer meshRenderer)
    {
        this.fireballAbility = fireballAbility;
        this.rigidBody = rigidBody;
        this.meshRenderer = meshRenderer;
    }

    /// <summary>
    /// Updates collisions.
    /// </summary>
    public void UpgradeVar()
    {
        gameObject.layer = fireballAbility.penetrateEnemies ? Layer.ignoreEnemyCollision : Layer.ignorePlayerCollision;
        rigidBody.drag = fireballAbility.rigidbodyDrag;
        rigidBody.angularDrag = fireballAbility.rigidbodyAngularDrag;
        rigidBody.mass = fireballAbility.rigidbodyMass;
    }

    /// <summary>
    /// Reset position of fireball and stop all coroutines.
    /// </summary>
    public void Reset(Vector3 pos)
    {
        StopAllCoroutines();
        transform.position = pos;
    }

    private void Start()
    {
        StartBurning();
    }

    /// <summary>
    /// Change material to burning fireball and start BurnOut() coroutine.
    /// </summary>
    public void StartBurning()
    {
        meshRenderer.material = fireballAbility.fireballMaterial;
        isBurnedOut = false;
        StartCoroutine(BurnOut(Random.Range(fireballAbility.fireballFireTimeMin, fireballAbility.fireballFireTimeMax)));
    }

    /// <summary>
    /// Apply velocity to rigidbody
    /// </summary>
    public void ApplyVelocity(Vector3 vel)
    {
        hasExploded = false;
        rigidBody.velocity = vel;
    }

    /// <summary>
    /// Update material properties to animate fireball.
    /// </summary>
    private void Update()
    {
        if (transform.position.y < Water.waterLevel && !isBurnedOut)
        {
            isBurnedOut = true;
            StopAllCoroutines();
            StartCoroutine(BurnOut(0f));
            StartCoroutine(BurnedOut(fireballAbility.fireballActiveTime));
        }

        meshRenderer.material.SetVector("_FireBallCentre", transform.position);
        meshRenderer.material.SetVector("_FireDirection", rigidBody.velocity * airEffectMagnitude - fireHeatLiftForce);
    }

    /// <summary>
    /// Deals damage around fireball and applies force to enemies.
    /// </summary>
    public void Explode()
    {
        if (!isBurnedOut && !hasExploded && fireballAbility.explodeOnFirstHit)
        {
            Enemies.Sound(transform, fireballAbility.explosionSound);

            Vector3 fireballPos = transform.position;
            Collider[] allCollisions = Physics.OverlapSphere(fireballPos, fireballAbility.explosionRadius);
            foreach (Collider collider in allCollisions)
            {
                if (collider.gameObject == gameObject || Layer.IsInLayer(Layer.ignoreExternalForces, collider.gameObject.layer))
                {
                    continue;
                }
                if (Layer.IsInLayer(Layer.enemy, collider.gameObject.layer))
                {
                    Transform parent = collider.transform.parent;
                    if(parent.GetComponent<EnemyAI>().Damage(fireballAbility.explosionDamage, damageId, 0.1f))
                    {
                        parent.GetComponent<Agent>().AddExplosionForce(fireballAbility.explosionForce, fireballPos, 0f, 1f, ForceMode.Impulse);
                    }
                }
                else if (collider.TryGetComponent(out Rigidbody rigidBody))
                {
                    rigidBody.AddExplosionForce(fireballAbility.explosionForce, fireballPos, 0f, fireballAbility.explosionUpwardsModifier, ForceMode.Impulse);
                }
            }
            fireballAbility.explosionPrefab.transform.position = transform.position;
            fireballAbility.explosionPrefab.SetActive(false);
            fireballAbility.explosionPrefab.SetActive(true);
            hasExploded = true;

            StartCoroutine(RaycastFibonacciSphere(100, 0.75f, 2f, 0.0075f, 0.025f));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private IEnumerator RaycastFibonacciSphere(int amount, float cutoff, float yOffset, float timeBetween, float timeBegining)
    {
        Vector3 startPoint = transform.position + Vector3.up * yOffset;

        float phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));

        yield return new WaitForSeconds(timeBegining);
        WaitForSeconds waitBetween = new WaitForSeconds(timeBetween);

        for (int i = amount; i > amount * cutoff; i--)
        {
            yield return waitBetween;

            float y = 1f - (i / (amount - 1f)) * 2f;
            float radius = Mathf.Sqrt(1f - y * y);

            float theta = phi * i;

            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;

            Vector3 point = new Vector3(x, y, z);

            RaycastHit hit;
            if (Physics.Raycast(startPoint, point, out hit, fireballAbility.explosionRadius, Layer.Mask.staticGround))
            {
                if (Tag.IsTaggedWith(hit.collider.tag, Tag.flammable))
                {
                    fireballAbility.setFire.UpdateFlammableFire(hit.point, hit.normal, Random.Range(fireballAbility.groundFireTimeMin, fireballAbility.groundFireTimeMax));
                }
                else
                {
                    fireballAbility.setFire.UpdateNonFlammableFire(hit.point, hit.normal, Random.Range(fireballAbility.groundFireTimeMin / fireballAbility.groundNoneFlammableTimeDecreesement, fireballAbility.groundFireTimeMax / fireballAbility.groundNoneFlammableTimeDecreesement));
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    /// <summary>
    /// While colliding with ground check collision tag and update fire in SetFire.
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        float sound = rigidBody.velocity.magnitude * fireballAbility.frictionSound;
        Enemies.Sound(transform, sound, Time.fixedDeltaTime);

        if (isBurnedOut || !Layer.IsInLayer(Layer.Mask.staticGround, collision.gameObject.layer))
        {
            return;
        }

        ContactPoint contactPoint = collision.GetContact(0);
        Vector3 newBurnContactPoint = contactPoint.point;
        Vector3 normal = contactPoint.normal;
        if ((lastBurnContactPoint - newBurnContactPoint).sqrMagnitude > 3f)
        {
            if (Tag.IsTaggedWith(collision.gameObject.tag, Tag.flammable))
            {
                fireballAbility.setFire.UpdateFlammableFire(newBurnContactPoint, normal, Random.Range(fireballAbility.groundFireTimeMin, fireballAbility.groundFireTimeMax));
            }
            else
            {
                fireballAbility.setFire.UpdateNonFlammableFire(newBurnContactPoint, normal, Random.Range(fireballAbility.groundFireTimeMin / fireballAbility.groundNoneFlammableTimeDecreesement, fireballAbility.groundFireTimeMax / fireballAbility.groundNoneFlammableTimeDecreesement));
            }
            lastBurnContactPoint = newBurnContactPoint;
        }
    }

    /// <summary>
    /// After given time stop adding contact points to SetFire during collisions and change material of fireball.
    /// </summary>
    private IEnumerator BurnOut(float time)
    {
        yield return new WaitForSeconds(time);
        isBurnedOut = true;
        meshRenderer.material = fireballAbility.fireballBurnedOutMaterial;
        StartCoroutine(BurnedOut(fireballAbility.fireballActiveTime));
    }

    /// <summary>
    /// Disable game object after given time.
    /// </summary>
    private IEnumerator BurnedOut(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
    */
}
