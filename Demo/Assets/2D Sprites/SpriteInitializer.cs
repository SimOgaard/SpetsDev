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
    [SerializeField] private Sprite sprite_to_render;
    [SerializeField] private RuntimeAnimatorController animation_to_render;

    private GameObject sprite_game_object;
    private SpriteRenderer sprite_renderer;
    private Animator animator;

    /// <summary>
    /// Initializes gameobject with scale to offset cameras isometric view
    /// <summary>
    public void Initialize(Sprite sprite, Vector3 rotation_in_vec_space, int render_order = 0)
    {
        if (sprite_game_object != null)
        {
            Destroy(sprite_game_object);
        }

        // Initializes gameobject as child with sprite renderer component and given sprite
        sprite_game_object = new GameObject();
        sprite_game_object.transform.parent = transform;
        sprite_game_object.transform.rotation = Quaternion.Euler(rotation_in_vec_space);

        sprite_renderer = sprite_game_object.AddComponent<SpriteRenderer>();
        sprite_renderer.sprite = sprite;
        sprite_renderer.sortingOrder = render_order;
        animator = null;

        // Applies scale to gameobject to correct camera rotation
        float y_scale = 1f / Mathf.Cos(Mathf.Deg2Rad * (rotation_in_vec_space.x - 30f));
        sprite_game_object.transform.localScale = new Vector3(1f, y_scale, 1f);
        sprite_game_object.transform.localPosition = new Vector3(0f, 3.5f, 0f);
    }
    public void Initialize(RuntimeAnimatorController animation, Vector3 rotation_in_vec_space)
    {
        if (sprite_game_object != null)
        {
            Destroy(sprite_game_object);
        }

        sprite_game_object = new GameObject();
        sprite_game_object.transform.parent = transform;
        sprite_game_object.transform.rotation = Quaternion.Euler(rotation_in_vec_space);

        sprite_renderer = sprite_game_object.AddComponent<SpriteRenderer>();
        animator = sprite_game_object.AddComponent<Animator>();
        animator.runtimeAnimatorController = animation;

        float y_scale = 1f / Mathf.Cos(Mathf.Deg2Rad * (rotation_in_vec_space.x - 30f));
        sprite_game_object.transform.localScale = new Vector3(1f, y_scale, 1f);
        sprite_game_object.transform.localPosition = new Vector3(0f, 3.5f, 0f);
    }

    /// <summary>
    /// Changes sprite/animation to render
    /// <summary>
    public void ChangeRender(Sprite sprite)
    {
        sprite_renderer.sprite = sprite;
    }
    public void ChangeRender(RuntimeAnimatorController animation)
    {
        animator.runtimeAnimatorController = animation;
    }

    /// <summary>
    /// Disable and enable render, keeps SpriteInitializer state
    /// <summary>
    public void ChangeRenderState(bool render_state)
    {
        sprite_renderer.enabled = render_state;
        if (animator != null)
        {
            animator.enabled = render_state;
        }
    }

    /// <summary>
    /// Destroys all initializations stemming from this component.
    /// <summary>
    public void Destroy()
    {
        Destroy(sprite_game_object);
        Destroy(this);
    }

    /// <summary>
    /// Renders possible Scene assigned sprites.
    /// <summary>
    private void Start()
    {
        if (sprite_to_render != null)
        {
            Initialize(sprite_to_render, Vector3.zero);
        }
        else if (animation_to_render != null)
        {
            Initialize(animation_to_render, Vector3.zero);
        }
    }
}
