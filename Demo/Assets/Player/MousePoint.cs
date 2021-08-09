using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePoint : MonoBehaviour
{
    [SerializeField] private float player_offset_to_ground = -3f;
    private Vector3 MousePos;
    private float rot;
    private Vector3 currentEulerAngles;
    private Quaternion finalRot;
    private Camera cam;

    private void Start()
    {
        cam = GameObject.Find("Camera").GetComponent<Camera>();
    }

    private void Update()
    {
        RotToTarget();
    }

    /// <summary>
    /// Gets position of mouse in camera plain 2D space from camera centre.
    /// </summary>
    public Vector3 GetScreenPosFromCentre()
    {
        Vector3 mousePos = GetInputMousePosition(); 
        mousePos.x -= Screen.width / 2;
        mousePos.y -= (Screen.height / 2);
        return mousePos;
    }

    /// <summary>
    /// Rotates GameObject towards mouse using characters.
    /// (Does not work vey well for aiming projectiles in an isometric view, leaving it in because it may be useful)
    /// </summary>
    public void RotToMouse2D()
    {
        Vector3 relativeMousePos = GetScreenPosFromPlayer();
        rot = Mathf.Atan(relativeMousePos.y / relativeMousePos.x) * 180 / Mathf.PI;
        if (relativeMousePos.x <= 0)
        {
            currentEulerAngles = new Vector3(0, -90 - rot, 0);
        }
        else
        {
            currentEulerAngles = new Vector3(0, 90 - rot, 0);
        }
        finalRot.eulerAngles = currentEulerAngles;
        transform.rotation = finalRot;
    }

    /// <summary>
    /// Gets the target position of the mouse calculated to be on the same plane as the player character. Allways returns value and requires less Compute than GetTargetMousePos();
    /// </summary>
    public Vector3 MousePosition2D()
    {
        float distance;
        Plane plane = new Plane(Vector3.up, transform.position + new Vector3(0f, player_offset_to_ground, 0f));
        ray = cam.ScreenPointToRay(GetInputMousePosition());
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance) - new Vector3(0f, player_offset_to_ground, 0f);
    }

    /// <summary>
    /// Get position of mouse in 2D space relative to player attaching reference point.
    /// </summary>
    public Vector3 GetScreenPosFromPlayer()
    {
        return GetInputMousePosition() - cam.WorldToScreenPoint(transform.position);
    }

    /// <summary>
    /// Gets the target position of the mouse calculated to be on the same plane as the player character.
    /// </summary>

    //Calculates the difference between the y distance between the camera and player as well as the camera and target mouse position
    //that difference is then divided by the distance between target and camera to get a factor that will multiply with the vector
    //that describes the distance between the camera and the target to extend that vector to end at the players y position. This endpoint
    //is then used as the target.
    public Vector3 GetTargetMousePos()
    {
        float RayYDiff;
        float PlayerYDiff;
        float PlayerCamYDiff;
        float factor;
        Vector3 targetPos = new Vector3(0,0,0);
        ray = cam.ScreenPointToRay(GetInputMousePosition());
        if (Physics.Raycast(ray, out hit_data, 250f, layer_mask_world))
        {
            RayYDiff = cam.transform.position.y - hit_data.point.y;
            PlayerYDiff = gameObject.transform.position.y - hit_data.point.y + player_offset_to_ground;
            PlayerCamYDiff = (RayYDiff - PlayerYDiff);
            factor = PlayerCamYDiff / RayYDiff;
            targetPos = cam.transform.position + ((hit_data.point - cam.transform.position - new Vector3(0f, player_offset_to_ground, 0f)) * factor);
        }
        return targetPos;
    }

    /// <summary>
    /// Get world position of object mouse is pointing at.
    /// </summary>
    public Vector3 GetTargetHitPoint ()
    {
        return cam.ScreenToWorldPoint(GetInputMousePosition());
    }

    /// <summary>
    /// Rotate towards target point.
    /// </summary>
    public void RotToTarget()
    {
        transform.LookAt(MousePosition2D());
    }

    /// <summary>
    /// The current mouse position in screen pixel coordinates. Translates the whole 400x255 px render texture to in game 384x216 px.
    /// </summary>
    public Vector3 GetInputMousePosition()
    {
        Vector3 input_mouse_position_raw = Input.mousePosition;
        Vector3 input_mouse_position_offset = new Vector3(384f / 400f, 216f / 225f, 0f);
        return new Vector3(input_mouse_position_raw.x * input_mouse_position_offset.x, input_mouse_position_raw.y * input_mouse_position_offset.y, 0f);
    }

    /// <summary>
    /// Gets world point of mouse cursor ignoring enemies.
    /// </summary>
    public Vector3 GetWorldPoint()
    {
        ray = cam.ScreenPointToRay(GetInputMousePosition());
        if (Physics.Raycast(ray, out hit_data, 250f, layer_mask_world_colliders_1))
        {
            return hit_data.point;
        }
        return MousePosition2D();
    }
    /// <summary>
    /// Gets world point of mouse cursor.
    /// </summary>
    public Vector3 GetWorldAndEnemyPoint()
    {
        ray = cam.ScreenPointToRay(GetInputMousePosition());
        if (Physics.Raycast(ray, out hit_data, 250f, layer_mask_world_colliders_5))
        {
            return hit_data.point;
        }
        return MousePosition2D();
    }
    /// <summary>
    /// Gets world point of mouse cursor and middle of enemy collider if enemy is targeted.
    /// </summary>
    public Vector3 GetWorldPointAndEnemyMid()
    {
        ray = cam.ScreenPointToRay(GetInputMousePosition());
        if (Physics.Raycast(ray, out hit_data, 250f, layer_mask_world_colliders_5))
        {
            if (hit_data.collider.gameObject.layer == 16)
            {
                return hit_data.collider.bounds.center;
            }
            return hit_data.point;
        }
        return MousePosition2D();
    }
    private Ray ray;
    private RaycastHit hit_data;
    /// <summary>
    /// Gameworld Layer.
    /// </summary>
    public static LayerMask layer_mask_world = 1 << 12;
    /// <summary>
    /// Default and Gameworld Layer.
    /// </summary>
    public static LayerMask layer_mask_world_colliders_1 = (1 << 0) | (1 << 12);
    /// <summary>
    /// Default, Gameworld, SpawnedCollider and SpawnedColliderPlayerIgnore Layer.
    /// </summary>
    public static LayerMask layer_mask_world_colliders_2 = (1 << 0) | (1 << 12) | (1 << 17) | (1 << 18);
    /// <summary>
    /// Default, Gameworld, Projectile and ProjectileGoThrough Layer.
    /// </summary>
    public static LayerMask layer_mask_world_colliders_3 = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 15);
    /// <summary>
    /// Default, Gameworld, Projectile, ProjectileGoThrough, SpawnedCollider and SpawnedColliderPlayerIgnore Layer.
    /// </summary>
    public static LayerMask layer_mask_world_colliders_4 = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 15) | (1 << 17) | (1 << 18);
    /// <summary>
    /// Default, Gameworld, Projectile, ProjectileGoThrough, SpawnedCollider, SpawnedColliderPlayerIgnore and Enemy Layer.
    /// </summary>
    public static LayerMask layer_mask_world_colliders_5 = (1 << 0) | (1 << 12) | (1 << 14) | (1 << 15) | (1 << 16) | (1 << 17) | (1 << 18);
}
