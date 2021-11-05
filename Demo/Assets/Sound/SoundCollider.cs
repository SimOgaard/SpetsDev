using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCollider : MonoBehaviour
{
    private Rigidbody _rigidbody;
    public float max_sound = Mathf.Infinity;
    public float sound_amplifier;
    public bool time_dependent = false;
    private bool can_make_sound = true;

    public IEnumerator DelaySound(float time)
    {
        can_make_sound = false;
        yield return new WaitForSeconds(time);
        can_make_sound = true;
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
        if (!can_make_sound)
        {
            return;
        }

        if (time_dependent)
        {
            float sound = _rigidbody.velocity.magnitude * sound_amplifier * Time.deltaTime;
            Enemies.Sound(transform, sound, max_sound * Time.deltaTime, Time.deltaTime);
        }
        else
        {
            float sound = _rigidbody.velocity.magnitude * sound_amplifier;
            Enemies.Sound(transform, sound, max_sound);
        }
    }
}
