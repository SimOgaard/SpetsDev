using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public static List<Interactable> interactables = new List<Interactable>();
    public bool isInInteractables = false;
    private void OnEnable()
    {
        if (!isUsed)
        {
            Enable();
        }
    }
    public virtual void Enable()
    {
        if (!isInInteractables)
        {
            isInInteractables = true;
            interactingSprite.Active(true);
            interactables.Add(this);
        }
    }
    private void OnDisable()
    {
        if (!isUsed)
        {
            Disable();
        }
    }
    public virtual void Disable()
    {
        if (isInInteractables)
        {
            isInInteractables = false;
            interactingSprite.Active(false);
            interactables.Remove(this);
        }
    }

    public SpriteInitializer interactingSprite;

    private float _playerMinDistance = 10f;
    public float playerMinDistance
    {
        get { return _playerMinDistance; }
        set { _playerMinDistance = value; }
    }

    private bool _allowsInteraction = true;
    public bool allowsInteraction
    {
        get { return _allowsInteraction; }
        set { _allowsInteraction = value; }
    }

    private bool _isUsed = false;
    public bool isUsed
    {
        get { return _isUsed; }
        set { _isUsed = value; }
    }

    public virtual bool CanInteractWith()
    {
        return allowsInteraction && !isUsed;
    }

    public virtual void InteractWith()
    {
        Disable();
        isUsed = true;
    }

    public virtual void ChangeInteractingSprite(Sprite sprite)
    {
        if (!isUsed)
        {
            interactingSprite.ChangeRender(sprite);
        }
    }

    public virtual void Awake()
    {
        interactingSprite = gameObject.AddComponent<SpriteInitializer>();
        interactingSprite.Initialize(Global.notInteractingWithSprite, Quaternion.identity, 5f);
    }
}
