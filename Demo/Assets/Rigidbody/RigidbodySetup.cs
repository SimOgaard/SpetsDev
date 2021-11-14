using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RigidbodySetup : MonoBehaviour
{
    private Rigidbody _rigidbody;

    [Header("Sound")]
    public float max_sound = Mathf.Infinity;
    public float min_sound = 0f;
    public static float sound_amplifier = 1f;
    private float weight;

    private bool can_make_sound = true;
    private WaitForSeconds wait = new WaitForSeconds(0.1f);

    public IEnumerator DelaySound()
    {
        can_make_sound = false;
        yield return wait;
        can_make_sound = true;
    }

    public Rigidbody AddRigidbody(Density.DensityValues density)
    {
        gameObject.layer = Layer.game_world_moving;
        transform.parent = WorldGenerationManager.ReturnNearestChunk(transform.position);
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
        if (!can_make_sound)
        {
            return;
        }

        DelaySound();
        float final_sound = Mathf.Max(Mathf.Min(_rigidbody.velocity.magnitude * weight, max_sound), min_sound);
        Enemies.Sound(transform, final_sound);
    }

    public static Rigidbody AddRigidbody(GameObject game_object)
    {
        RigidbodySetup rigidbody_setup = game_object.AddComponent<RigidbodySetup>();
        return rigidbody_setup.AddRigidbody(game_object.GetComponent<Density>().density);
    }
}
