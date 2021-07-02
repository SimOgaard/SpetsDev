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

    private SpriteRenderer sprite_renderer;
    private Animator animator;

    private GameObject game_object;

    /// <summary>
    /// Initializes gameobject stretched in Y scale to offset cameras isometric view
    /// <summary>
    public void InitializeUpright(Sprite sprite)
    {
        // Initializes gameobject as child with sprite renderer component and given sprite
        game_object = new GameObject();

        sprite_renderer = game_object.AddComponent<SpriteRenderer>();
        sprite_renderer.sprite = sprite;
        animator = null;
        
        game_object.transform.parent = transform;

        // Applies scale to gameobject to correct camera rotation
        float y_scale = 1f / Mathf.Cos(Mathf.Deg2Rad * 30f);
        game_object.transform.rotation = Quaternion.identity;                   // only works for y rotation
        game_object.transform.localScale = new Vector3(1f, y_scale, 1f);
        game_object.transform.localPosition = new Vector3(0f, y_scale * 3f, 0f);
    }
    public void InitializeUpright(RuntimeAnimatorController animation)
    {
        game_object = new GameObject();

        sprite_renderer = game_object.AddComponent<SpriteRenderer>();
        animator = game_object.AddComponent<Animator>();
        animator.runtimeAnimatorController = animation;

        game_object.transform.parent = transform;

        float y_scale = 1f / Mathf.Cos(Mathf.Deg2Rad * 30f);
        game_object.transform.rotation = Quaternion.identity;
        game_object.transform.localScale = new Vector3(1f, y_scale, 1f);
        game_object.transform.localPosition = new Vector3(0f, y_scale * 3f, 0f);
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
        Destroy(game_object);
        Destroy(this);
    }

    /// <summary>
    /// Renders possible Scene assigned sprites.
    /// <summary>
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
