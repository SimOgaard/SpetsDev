using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script translates imported pixelart to worlds space.
/// The Sprite Initializer component initializes gameobject with sprite renderer at an angle calculated to offset cameras isometric view.
/// Alternatively this component stretches sprite in Y scale to offset cameras isometric view.
/// Also works for Animations.
/// Sprites must be set to:
///                         texture_type.Sprite
///                         pixels_per_unit = 5.4
///                         filter_mode.Point
///                         compression.None
/// </summary>
public class SpriteInitializer : MonoBehaviour
{
    // Sprite that gets rendered
    [SerializeField]
    private Sprite sprite_to_render;
    [SerializeField]
    private RuntimeAnimatorController animation_to_render;

    /// <summary>
    /// Initializes gameobject at an angle calculated to offset cameras isometric view
    /// <summary>
    private void InitializeRotated(Sprite sprite)
    {
        // Initializes gameobject as child with sprite renderer component and given sprite
        GameObject game_object = new GameObject();
        game_object.AddComponent<SpriteRenderer>();
        game_object.GetComponent<SpriteRenderer>().sprite = sprite;
        game_object.transform.parent = transform;

        // Applies rotation to gameobject so it is facing the camera
        Quaternion rotation = Quaternion.Euler(30f, 0f, 0f);
        game_object.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        game_object.transform.rotation = rotation;
    }
    private void InitializeRotated(RuntimeAnimatorController animation)
    {
        GameObject game_object = new GameObject();
        game_object.AddComponent<SpriteRenderer>();
        game_object.AddComponent<Animator>();
        game_object.GetComponent<Animator>().runtimeAnimatorController = animation;
        game_object.transform.parent = transform;

        Quaternion rotation = Quaternion.Euler(30f, 0f, 0f);
        game_object.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        game_object.transform.rotation = rotation;
    }

    /// <summary>
    /// Initializes gameobject stretched in Y scale to offset cameras isometric view
    /// <summary>
    private void InitializeUpright(Sprite sprite)
    {
        // Initializes gameobject as child with sprite renderer component and given sprite
        GameObject game_object = new GameObject();
        game_object.AddComponent<SpriteRenderer>();
        game_object.GetComponent<SpriteRenderer>().sprite = sprite;
        game_object.transform.parent = transform;

        // Applies scale to gameobject to correct camera rotation
        float y_scale = 1f / Mathf.Cos(Mathf.Deg2Rad * 30f);
        game_object.transform.localScale = new Vector3(1f, y_scale, 1f);
        game_object.transform.localPosition = new Vector3(0f, y_scale * 3f, 0f);
    }
    private void InitializeUpright(RuntimeAnimatorController animation)
    {
        GameObject game_object = new GameObject();
        game_object.AddComponent<SpriteRenderer>();
        game_object.AddComponent<Animator>();
        game_object.GetComponent<Animator>().runtimeAnimatorController = animation;
        game_object.transform.parent = transform;

        float y_scale = 1f / Mathf.Cos(Mathf.Deg2Rad * 30f);
        game_object.transform.localScale = new Vector3(1f, y_scale, 1f);
        game_object.transform.localPosition = new Vector3(0f, y_scale * 3f, 0f);
    }

    private void Start()
    {
        if (sprite_to_render != null)
        {
            InitializeUpright(sprite_to_render);
        }
        else if (animation_to_render != null)
        {
            InitializeUpright(animation_to_render);
        }
    }
}
