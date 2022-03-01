using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RigidbodySetup : MonoBehaviour
{
    private Rigidbody _rigidbody;

    [Header("Sound")]
    public float maxSound = Mathf.Infinity;
    public float minSound = 0f;
    public static float soundAmplifier = 1f;
    private float weight;

    private bool canMakeSound = true;
    private WaitForSeconds wait = new WaitForSeconds(0.1f);

    public IEnumerator DelaySound()
    {
        canMakeSound = false;
        yield return wait;
        canMakeSound = true;
    }

    public Rigidbody AddRigidbody(Density.DensityValues density)
    {
        gameObject.layer = Layer.gameWorldMoving;
        transform.parent = WorldGenerationManager.ReturnNearestChunk(transform.position).transform;
        weight = MeshDensity.WeightOfMesh(gameObject.GetComponent<MeshFilter>().mesh, transform.lossyScale, density);
        this._rigidbody = gameObject.AddComponent<Rigidbody>();
        this._rigidbody.mass = weight;
        this._rigidbody.drag = 2f;

        Debug.Log(weight);
        return this._rigidbody;
    }
    public Rigidbody AddRigidbody(Rigidbody _rigidbody)
    {
        weight = _rigidbody.mass;
        this._rigidbody = _rigidbody;
        return this._rigidbody;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canMakeSound)
        {
            return;
        }

        DelaySound();
        float finalSound = Mathf.Max(Mathf.Min(_rigidbody.velocity.magnitude * weight, maxSound), minSound);
        Enemies.Sound(transform, finalSound);
    }

    public static Rigidbody AddRigidbody(GameObject gameObject)
    {
        RigidbodySetup rigidbodySetup = gameObject.AddComponent<RigidbodySetup>();
        return rigidbodySetup.AddRigidbody(gameObject.GetComponent<Density>().density);
    }
}
