using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script translates imported pixelart to worlds space.
/// The Sprite Initializer component initializes gameobject with sprite renderer at an angle calculated to offset cameras isometric view.
/// Alternatively this component stretches sprite in Y scale to offset cameras isometric view.
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
        Quaternion rotation = Quaternion.Euler(Mathf.Rad2Deg * Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad)), 0f, 0f);
        game_object.transform.localPosition = new Vector3(0f, 2.5f, 0f); // TODO Y position according to rotation
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
        float y_scale = 0.5f / Mathf.Sin(Mathf.Atan(Mathf.Sin(30f * Mathf.Deg2Rad)));
        game_object.transform.localScale = new Vector3(1f, y_scale, 1f);
        game_object.transform.localPosition = new Vector3(0f, 2.333f, 0f); // TODO bottom of sprite flush with ground
    }

    private void Start()
    {
        InitializeUpright(sprite_to_render);
    }
}
