using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script translates imported pixelart to worlds space.
/// The Sprite Initializer component initializes gameobject with a sprite renderer, scale in Y to offset cameras isometric view and local position to render sprite from ground level.
/// Also works for Animations.
/// Sprites must be set to:
///                         textureType.Sprite
///                         pixelsPerUnit = 5.4
///                         filterMode.Point
///                         compression.None
/// </summary>
public class SpriteInitializer : MonoBehaviour
{
    [SerializeField] private Sprite spriteToRender;
    [SerializeField] private RuntimeAnimatorController animationToRender;

    private GameObject spriteGameObject;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    public static float yScale = 1f / Mathf.Cos(Mathf.Deg2Rad * (-30f));

    /// <summary>
    /// Initializes gameobject with scale to offset cameras isometric view
    /// <summary>
    public void Initialize(Sprite sprite, Quaternion rotation, float yPos = 3.5f, int renderOrder = 1)
    {
        if (spriteGameObject != null)
        {
            Destroy(spriteGameObject);
        }

        // Initializes gameobject as child with sprite renderer component and given sprite
        spriteGameObject = new GameObject();
        //spriteGameObject.transform.rotation = rotation;
        spriteGameObject.transform.parent = transform;

        spriteRenderer = spriteGameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = renderOrder;
        spriteRenderer.material = Global.Materials.spriteRendererMaterial;
        animator = null;

        // Applies scale to gameobject to correct camera rotation
        spriteGameObject.transform.localScale = new Vector3(1f / transform.lossyScale.x, yScale / transform.lossyScale.y, 1f / transform.lossyScale.z);
        spriteGameObject.transform.localPosition = new Vector3(0f, yPos / transform.lossyScale.y, 0f);
    }
    public void Initialize(RuntimeAnimatorController animation, Quaternion rotation, float yPos = 3.5f, int renderOrder = 1)
    {
        if (spriteGameObject != null)
        {
            Destroy(spriteGameObject);
        }

        spriteGameObject = new GameObject();
        //spriteGameObject.transform.rotation = rotation;
        spriteGameObject.transform.parent = transform;

        spriteRenderer = spriteGameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = renderOrder;
        spriteRenderer.material = Global.Materials.spriteRendererMaterial;
        animator = spriteGameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = animation;

        spriteGameObject.transform.localScale = new Vector3(1f / transform.lossyScale.x, yScale / transform.lossyScale.y, 1f / transform.lossyScale.z);
        spriteGameObject.transform.localPosition = new Vector3(0f, yPos / transform.lossyScale.y, 0f);
    }

    /// <summary>
    /// Changes sprite/animation to render
    /// <summary>
    public void ChangeRender(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
    public void ChangeRender(RuntimeAnimatorController animation)
    {
        animator.runtimeAnimatorController = animation;
    }

    /// <summary>
    /// Disable and enable render, keeps SpriteInitializer state
    /// <summary>
    public void ChangeRenderState(bool renderState)
    {
        spriteRenderer.enabled = renderState;
        if (animator != null)
        {
            animator.enabled = renderState;
        }
    }

    /// <summary>
    /// Destroys all initializations stemming from this component.
    /// <summary>
    public void Destroy()
    {
        Destroy(spriteGameObject);
        Destroy(this);
    }

    /// <summary>
    /// Disable all initializations stemming from this component.
    /// <summary>
    public void Active()
    {
        bool state = !this.enabled;
        spriteGameObject.SetActive(state);
        this.enabled = state;
    }
    public void Active(bool state)
    {
        if (state != this.enabled)
        {
            spriteGameObject.SetActive(state);
            this.enabled = state;
        }
    }

    /// <summary>
    /// Renders possible Scene assigned sprites.
    /// <summary>
    private void Start()
    {
        if (spriteToRender != null)
        {
            Initialize(spriteToRender, Quaternion.identity, 0f);
        }
        else if (animationToRender != null)
        {
            Initialize(animationToRender, Quaternion.identity, 0f);
        }
    }
}
