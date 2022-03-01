using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCollider : MonoBehaviour
{
    private Rigidbody _rigidbody;
    public float maxSound = Mathf.Infinity;
    public float minSound = 0f;
    public float soundAmplifier;
    public bool timeDependent = false;
    private bool canMakeSound = true;

    public IEnumerator DelaySound(float time)
    {
        canMakeSound = false;
        yield return new WaitForSeconds(time);
        canMakeSound = true;
    }

    public Rigidbody AddRigidbody()
    {
        this._rigidbody = gameObject.AddComponent<Rigidbody>();
        return this._rigidbody;
    }
    public Rigidbody AddRigidbody(Rigidbody _rigidbody)
    {
        this._rigidbody = _rigidbody;
        return this._rigidbody;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canMakeSound)
        {
            return;
        }

        if (timeDependent)
        {
            float sound = Mathf.Max(Mathf.Min(_rigidbody.velocity.magnitude * soundAmplifier, maxSound), minSound);
            Enemies.Sound(transform, sound);
        }
        else
        {
            float sound = Mathf.Max(Mathf.Min(_rigidbody.velocity.magnitude * soundAmplifier, maxSound), minSound);
            Enemies.Sound(transform, sound);
        }
    }
}
