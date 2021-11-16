using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public static List<Interactable> interactables = new List<Interactable>();
    public bool is_in_interactables = false;
    private void OnEnable()
    {
        if (!is_used)
        {
            Enable();
        }
    }
    public virtual void Enable()
    {
        if (!is_in_interactables)
        {
            is_in_interactables = true;
            interacting_sprite.Active(true);
            interactables.Add(this);
        }
    }
    private void OnDisable()
    {
        if (!is_used)
        {
            Disable();
        }
    }
    public virtual void Disable()
    {
        if (is_in_interactables)
        {
            is_in_interactables = false;
            interacting_sprite.Active(false);
            interactables.Remove(this);
        }
    }

    public SpriteInitializer interacting_sprite;

    private float _player_min_distance = 10f;
    public float player_min_distance
    {
        get { return _player_min_distance; }
        set { _player_min_distance = value; }
    }

    private bool _allows_interaction = true;
    public bool allows_interaction
    {
        get { return _allows_interaction; }
        set { _allows_interaction = value; }
    }

    private bool _is_used = false;
    public bool is_used
    {
        get { return _is_used; }
        set { _is_used = value; }
    }

    public virtual bool CanInteractWith()
    {
        return allows_interaction && !is_used;
    }

    public virtual void InteractWith()
    {
        Disable();
        is_used = true;
    }

    public virtual void ChangeInteractingSprite(Sprite sprite)
    {
        if (!is_used)
        {
            interacting_sprite.ChangeRender(sprite);
        }
    }

    public virtual void Awake()
    {
        interacting_sprite = gameObject.AddComponent<SpriteInitializer>();
        interacting_sprite.Initialize(Global.not_interacting_with_sprite, Quaternion.identity, 5f);
    }
}
