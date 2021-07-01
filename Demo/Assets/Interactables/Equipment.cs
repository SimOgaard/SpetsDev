using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script controlls equipments that are in game world
public class Equipment : MonoBehaviour
{
    public enum Type { test }
    private Type equipment_type;

    private BoxCollider equipment_collider;
    private Rigidbody equipment_rigidbody;

    private TrailRenderer trail_renderer;

    private float initialization_time;
    private static float static_time_threshold = 0.1f;

    public void EquipmentInAir(Type equipment_type)
    {
        Destroy(gameObject.GetComponent<SphereCollider>());

        this.equipment_type = equipment_type;

        equipment_rigidbody = gameObject.AddComponent<Rigidbody>();
        equipment_collider = gameObject.AddComponent<BoxCollider>();

        equipment_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        Physics.IgnoreCollision(GameObject.Find("Player").GetComponent<CapsuleCollider>(), equipment_collider);

        initialization_time = Time.timeSinceLevelLoad;

        trail_renderer = gameObject.AddComponent<TrailRenderer>();
        trail_renderer.time = 0.175f;

        Gradient gradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[2];
        colorKey[0].color = Color.cyan;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.blue;
        colorKey[1].time = 1.0f;
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 0.0f;
        alphaKey[1].time = 1.0f;
        gradient.SetKeys(colorKey, alphaKey);
        trail_renderer.colorGradient = gradient;

        trail_renderer.startWidth = 0.75f;
        trail_renderer.endWidth = 0.1f;

        trail_renderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

        gameObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/ItemTestShader"));
    }

    private void EquipmentOnGround()
    {
        Destroy(equipment_collider);
        Destroy(equipment_rigidbody);

        // remove trail
        // change material and mesh to equipment

        SpriteInitializer sprite_initializer = gameObject.AddComponent<SpriteInitializer>();
        Sprite not_interacting_with_sprite = Resources.Load<Sprite>("Interactables/not_interacting_with_sprite");
        sprite_initializer.InitializeUpright(not_interacting_with_sprite);

        transform.parent = GameObject.Find("Interactables").transform;
        gameObject.isStatic = true;

        StartCoroutine(TrailFade(trail_renderer.time));
    }

    private IEnumerator TrailFade(float delay_time)
    {
        yield return new WaitForSeconds(delay_time);
        Destroy(trail_renderer);
    }

    public void InteractWith()
    {
        Debug.Log("item picked up");
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "GameWorld" && Time.timeSinceLevelLoad - initialization_time > static_time_threshold)
        {
            EquipmentOnGround();
        }
    }
}
