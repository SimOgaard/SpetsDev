using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script translates imported pixelart to worlds space.
/// The Sprite Initializer component initializes gameobject with a sprite renderer, scale in Y to offset cameras isometric view and local position to render sprite from ground level.
/// Also works for Animations.
/// Sprites must be set to:
///                         texture_type.Sprite
///                         pixels_per_unit = 5.4
///                         filter_mode.Point
///                         compression.None
/// </summary>
public class SpriteInitializer : MonoBehaviour
{
    [SerializeField]
    private Sprite sprite_to_render;
    [SerializeField]
    private RuntimeAnimatorController animation_to_render;

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
