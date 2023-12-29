using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Interactable
{
    public static GameObject droppedShaderPrefab; // loaded in InteractableEventHandler
    private GameObject thisDroppedGameObject;
    
    public virtual void Drop(Vector3 position, Vector3 thrust)
    {
        Enable();
        allowsInteraction = false;

        thisDroppedGameObject = Instantiate(droppedShaderPrefab, position, Quaternion.identity);
        thisDroppedGameObject.layer = Layer.ignoreExternalForces;
        transform.parent = thisDroppedGameObject.transform;
        transform.localPosition = Vector3.zero;

        Rigidbody rigidbody = thisDroppedGameObject.GetComponent<Rigidbody>();
        rigidbody.AddForce(thrust);
    }

    public virtual Vector3 Thrust(float selectedRotation, Vector3 forwardVector, float force = 5750f)
    {
        float startRotation = 0f;
        if (forwardVector != Vector3.zero)
        {
            startRotation = -((Mathf.Atan2(forwardVector.z, forwardVector.x) * Mathf.Rad2Deg) + 450f) % 360f;
        }
        return Quaternion.Euler(-Random.Range(72.5f, 82.5f), Random.Range(-selectedRotation * 0.5f + 180f + startRotation, selectedRotation * 0.5f + 180f + startRotation), 0) * Vector3.forward * force;
    }

    public override void InteractWith()
    {
        Disable();
    }

    public virtual void OnGround()
    {
        allowsInteraction = true;
        thisDroppedGameObject.transform.DetachChildren();
        Destroy(thisDroppedGameObject);
    }

    public Sprite iconSprite;
}
