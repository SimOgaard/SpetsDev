using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    /// Layer index of all objects that should not render to camera.
    /// </summary>
    public static int default_
    {
        get { return LayerMask.NameToLayer("Default"); }
    }

    /// <summary>
    /// Layer index of all objects that should not render to camera.
    /// </summary>
    public static int not_rendered
    {
        get { return LayerMask.NameToLayer("NotRendered"); }
    }

    /// <summary>
    /// Layer index of all static game world objects.
    /// </summary>
    public static int game_world
    {
        get { return LayerMask.NameToLayer("GameWorld"); }
    }

    /// <summary>
    /// Layer index of all game world objects that are not/should not be marked static.
    /// </summary>
    public static int game_world_moving
    {
        get { return LayerMask.NameToLayer("GameWorldMoving"); }
    }

    /// <summary>
    /// Layer index of all objects that should not collide with player.
    /// </summary>
    public static int ignore_player_collision
    {
        get { return LayerMask.NameToLayer("IgnorePlayerCollision"); }
    }

    /// <summary>
    /// Layer index of all objects that should not collide with enemies.
    /// </summary>
    public static int ignore_enemy_collision
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
    public static int ignore_external_forces
    {
        get { return LayerMask.NameToLayer("IgnoreExternalForces"); }
    }

    /// <summary>
    /// Layer index of spawned prefab with trigger collider for raycast overriding layer_game_world.
    /// </summary>
    public static int spawned_game_world_higher_priority
    {
        get { return LayerMask.NameToLayer("GameWorldHighPriority"); }
    }

    /// <summary>
    /// Layer index of spawned prefab with collider that spawning raycast should ignore.
    /// </summary>
    public static int spawned_game_world_no_priority
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
            get { return (1 << default_) | (1 << game_world) | (1 << game_world_moving); }
        }

        /// <summary>
        /// Layer Mask of what is generally concidered static ground.
        /// </summary>
        public static LayerMask static_ground
        {
            get { return (1 << game_world); }
        }

        /// <summary>
        /// Layer Mask of what is concidered ground for player.
        /// </summary>
        public static LayerMask ground_player
        {
            get { return (1 << game_world) | (1 << game_world_moving); }
        }

        /// <summary>
        /// Layer Mask of what is concidered ground for enemies.
        /// </summary>
        public static LayerMask ground_enemy
        {
            get { return (1 << game_world) | (1 << game_world_moving); }
        }

        /// <summary>
        /// Layer Mask of what should be transformed into game world layer.
        /// </summary>
        public static LayerMask spawned_game_world
        {
            get { return (1 << game_world) | (1 << spawned_game_world_higher_priority) | (1 << spawned_game_world_no_priority); }
        }

        /// <summary>
        /// Layer Mask of what is concidered ground for player.
        /// </summary>
        public static LayerMask player_and_enemy
        {
            get { return (1 << enemy) | (1 << player); }
        }
    }

    /// <summary>
    /// Returns true if layer values are the same.
    /// </summary>
    public static bool IsInLayer(int layer_value_1, int layer_value_2)
    {
        return layer_value_1 == layer_value_2;
    }

    /// <summary>
    /// Returns true if layer value and game object layer value is the same.
    /// </summary>
    public static bool IsInLayer(int layer_value, GameObject obj)
    {
        return (layer_value & 1 << obj.layer) != 0;
    }

    /// <summary>
    /// Returns true if layer mask contains layer value.
    /// </summary>
    public static bool IsInLayer(LayerMask layer_mask, int layer_value)
    {
        return (layer_mask.value & 1 << layer_value) != 0;
    }

    /// <summary>
    /// Returns true if layer mask contains layer value of given game object.
    /// </summary>
    public static bool IsInLayer(LayerMask layer_mask, GameObject obj)
    {
        return (layer_mask.value & 1 << obj.layer) != 0;
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