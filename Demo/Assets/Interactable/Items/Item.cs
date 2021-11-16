using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Interactable
{
    public static GameObject dropped_shader_prefab; // loaded in InteractableEventHandler
    private GameObject this_dropped_game_object;
    
    public virtual void Drop(Vector3 position, Vector3 thrust)
    {
        Enable();
        allows_interaction = false;

        this_dropped_game_object = Instantiate(dropped_shader_prefab, position, Quaternion.identity);
        this_dropped_game_object.layer = Layer.ignore_external_forces;
        transform.parent = this_dropped_game_object.transform;
        transform.localPosition = Vector3.zero;

        Rigidbody rigidbody = this_dropped_game_object.GetComponent<Rigidbody>();
        rigidbody.AddForce(thrust);
    }

    public virtual Vector3 Thrust(float selected_rotation, Vector3 forward_vector, float force = 5750f)
    {
        float start_rotation = 0f;
        if (forward_vector != Vector3.zero)
        {
            start_rotation = -((Mathf.Atan2(forward_vector.z, forward_vector.x) * Mathf.Rad2Deg) + 450f) % 360f;
        }
        return Quaternion.Euler(-Random.Range(72.5f, 82.5f), Random.Range(-selected_rotation * 0.5f + 180f + start_rotation, selected_rotation * 0.5f + 180f + start_rotation), 0) * Vector3.forward * force;
    }

    public override void InteractWith()
    {
        Disable();
    }

    public virtual void OnGround()
    {
        allows_interaction = true;
        this_dropped_game_object.transform.DetachChildren();
        Destroy(this_dropped_game_object);
    }

    public Sprite icon_sprite;
}
