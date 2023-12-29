using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

/// <summary>
/// Static helper class for handeling tags.
/// </summary>
public static class Tag
{
    /// <summary>
    /// Tag for all flammable game objects.
    /// </summary>
    public static string flammable
    {
        get { return "Flammable"; }
    }

    /// <summary>
    /// Tag for all game objects that wish to be merged.
    /// </summary>
    public static string merge
    {
        get { return "Merge"; }
    }

    /// <summary>
    /// Returns true if tags are the same.
    /// </summary>
    public static bool IsTaggedWith(string tag1, string tag2)
    {
        return tag1 == tag2;
    }
}

/// <summary>
/// Static helper class for handeling layers.
/// </summary>
public static class Layer
{
    /// <summary>
    /// Sets gameobject and all its children to specified layer
    /// </summary>
    public static void SetRecursiveTo(GameObject gameObject, int layer)
    {
        gameObject.layer = layer;

        foreach (Transform child in gameObject.transform)
        {
            SetRecursiveTo(child.gameObject, layer);
        }
    }

    /// <summary>
    /// Default layer index.
    /// </summary>
    public static int default_
    {
        get { return LayerMask.NameToLayer("Default"); }
    }

    /// <summary>
    /// Layer index of all objects that should not render to camera.
    /// </summary>
    public static int notRendered
    {
        get { return LayerMask.NameToLayer("NotRendered"); }
    }

    /// <summary>
    /// Layer index of water.
    /// </summary>
    public static int water
    {
        get { return LayerMask.NameToLayer("Water"); }
    }

    /// <summary>
    /// Layer index of all game world objects.
    /// </summary>
    public static int gameWorld
    {
        get { return LayerMask.NameToLayer("GameWorld"); }
    }

    /// <summary>
    /// Layer index of all game world objects that are moving.
    /// </summary>
    public static int gameWorldMoving
    {
        get { return LayerMask.NameToLayer("GameWorldMoving"); }
    }

    /// <summary>
    /// Layer index of all static game world objects.
    /// </summary>
    public static int gameWorldStatic
    {
        get { return LayerMask.NameToLayer("GameWorldStatic"); }
    }    

    /// <summary>
    /// Layer index of all objects that should not collide with player.
    /// </summary>
    public static int ignorePlayerCollision
    {
        get { return LayerMask.NameToLayer("IgnorePlayerCollision"); }
    }

    /// <summary>
    /// Layer index of all objects that should not collide with enemies.
    /// </summary>
    public static int ignoreEnemyCollision
    {
        get { return LayerMask.NameToLayer("IgnoreEnemyCollision"); }
    }

    /// <summary>
    /// Layer index of Player.
    /// </summary>
    public static int player
    {
        get { return LayerMask.NameToLayer("Player"); }
    }

    /// <summary>
    /// Layer index of all enemies.
    /// </summary>
    public static int enemy
    {
        get { return LayerMask.NameToLayer("Enemy"); }
    }
    
    /// <summary>
    /// Layer index of game objects that should not be affected by external forces.
    /// </summary>
    public static int ignoreExternalForces
    {
        get { return LayerMask.NameToLayer("IgnoreExternalForces"); }
    }

    /// <summary>
    /// Layer index of spawned prefab with trigger collider for raycast overriding layerGameWorld.
    /// </summary>
    public static int spawnedGameWorldHigherPriority
    {
        get { return LayerMask.NameToLayer("GameWorldHighPriority"); }
    }

    /// <summary>
    /// Layer index of spawned prefab with collider that spawning raycast should ignore.
    /// </summary>
    public static int spawnedGameWorldNoPriority
    {
        get { return LayerMask.NameToLayer("GameWorldIgnore"); }
    }

    /// <summary>
    /// Static helper class for handeling layer masks.
    /// </summary>
    public static class Mask
    {
        /// <summary>
        /// Layer Mask of what is generally concidered ground.
        /// </summary>
        public static LayerMask ground
        {
            get { return (1 << default_) | (1 << gameWorld) | (1 << gameWorldMoving) | (1 << gameWorldStatic); }
        }

        /// <summary>
        /// Layer Mask of what is generally concidered static ground.
        /// </summary>
        public static LayerMask staticGround
        {
            get { return (1 << gameWorld) | (1 << gameWorldStatic); }
        }

        /// <summary>
        /// Layer Mask of what is concidered ground for player.
        /// </summary>
        public static LayerMask groundPlayer
        {
            get { return (1 << gameWorld) | (1 << gameWorldMoving) | (1 << gameWorldStatic); }
        }

        /// <summary>
        /// Layer Mask of what is concidered ground for enemies.
        /// </summary>
        public static LayerMask groundEnemy
        {
            get { return (1 << gameWorld) | (1 << gameWorldMoving) | (1 << gameWorldStatic); }
        }

        /// <summary>
        /// Layer Mask of what should be transformed into game world layer.
        /// </summary>
        public static LayerMask spawnedGameWorld
        {
            get { return (1 << gameWorld) | (1 << spawnedGameWorldHigherPriority) | (1 << spawnedGameWorldNoPriority); }
        }

        /// <summary>
        /// Layer Mask of what is concidered ground for player.
        /// </summary>
        public static LayerMask playerAndEnemy
        {
            get { return (1 << enemy) | (1 << player); }
        }
        
        /// <summary>
        /// Layer Mask of what should ignore forces.
        /// </summary>
        public static LayerMask ignoreForces
        {
            get { return (1 << gameWorldStatic) | (1 << ignoreExternalForces) | (1 << player); }
        }
    }

    /// <summary>
    /// Returns true if layer values are the same.
    /// </summary>
    public static bool IsInLayer(int layerValue_1, int layerValue_2)
    {
        return layerValue_1 == layerValue_2;
    }

    /// <summary>
    /// Returns true if layer value and game object layer value is the same.
    /// </summary>
    public static bool IsInLayer(int layerValue, GameObject obj)
    {
        return layerValue == obj.layer;
    }

    /// <summary>
    /// Returns true if layer mask contains layer value.
    /// </summary>
    public static bool IsInLayer(LayerMask layerMask, int layerValue)
    {
        return (layerMask.value & 1 << layerValue) != 0;
    }

    /// <summary>
    /// Returns true if layer mask contains layer value of given game object.
    /// </summary>
    public static bool IsInLayer(LayerMask layerMask, GameObject obj)
    {
        return (layerMask.value & 1 << obj.layer) != 0;
    }
}

public class VectorPid
{
    public float pFactor, iFactor, dFactor;

    private Vector3 integral;
    private Vector3 lastError;

    public VectorPid(float pFactor, float iFactor, float dFactor)
    {
        this.pFactor = pFactor;
        this.iFactor = iFactor;
        this.dFactor = dFactor;
    }

    public Vector3 Update(Vector3 currentError, float timeFrame)
    {
        integral += currentError * timeFrame;
        var deriv = (currentError - lastError) / timeFrame;
        lastError = currentError;
        return currentError * pFactor
            + integral * iFactor
            + deriv * dFactor;
    }
}

public static class Copy
{
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }
}

public static class Global
{
    public static Material waterMaterial;

    public static Sprite interactingWithSprite;
    public static Sprite notInteractingWithSprite;

    public static Transform cameraFocusPointTransform;
    public static Transform playerTransform;
    public static Transform equipmentsInInventoryTransform;

    public static DayNight dayNight;

    public static WindController windController;

    public static WorldGenerationManager worldGenerationManager;

    /// <summary>
    /// What assets need to be loaded in before world is created
    /// </summary>
    public static void PreLoad()
    {
        interactingWithSprite = Resources.Load<Sprite>("Interactables/interactingWithSprite");
        notInteractingWithSprite = Resources.Load<Sprite>("Interactables/notInteractingWithSprite");

        cameraFocusPointTransform = GameObject.Find("cameraFocusPoint").transform;
        playerTransform = GameObject.Find("Player").transform;
        equipmentsInInventoryTransform = GameObject.Find("EquipmentsInInventory").transform;

        dayNight = GameObject.FindObjectOfType<DayNight>();
        windController = GameObject.FindObjectOfType<WindController>();

        worldGenerationManager = GameObject.FindObjectOfType<WorldGenerationManager>();
    }

    /// <summary>
    /// What assets need to be loaded in after world is created
    /// </summary>
    public static void PostLoad()
    {
    }

    public static Vector3 normalGravity = Physics.gravity;
    public static Vector3 currentGravity = Physics.gravity;
}

public static class GameTime
{
    private static bool _isPaused = false;
    public static bool isPaused
    {
        get { return _isPaused; }
        set {
            if (value != _isPaused)
            {
                _isPaused = value;
            }
        }
    }

    public static void PauseGame(float timeScale = 0f)
    {
        Time.timeScale = timeScale;
    }

    public static void ResumeGame(float timeScale = 1f)
    {
        Time.timeScale = timeScale;
    }
}

public static class GameObjectFunctions
{
    public static T GetComponentFromGameObjectName<T>(string name)
    {
        GameObject gameObject = GameObject.Find(name);
        if (gameObject != null)
        {
            return (T) gameObject.GetComponent<T>();
        }
        return default(T);
    }
}