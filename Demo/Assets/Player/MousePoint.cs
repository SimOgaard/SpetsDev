using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePoint : MonoBehaviour
{
    [SerializeField] private const float player_offset_to_ground = -3f;
    private static Ray ray;
    private static RaycastHit hit_data;
    private static RaycastHit[] hits;

    /// <summary>
    /// The current mouse position in screen pixel coordinates. Translates the whole 400x255 px render texture to in game 384x216 px.
    /// </summary>
    private static Vector3 GetInputMousePosition()
    {
        Vector3 input_mouse_position_raw = Input.mousePosition;

        Vector2 input_mouse_position_offset = new Vector2(PixelPerfectCameraRotation.resolution.x / PixelPerfectCameraRotation.resolution_extended.x, PixelPerfectCameraRotation.resolution.y / PixelPerfectCameraRotation.resolution_extended.y);
        return new Vector3(input_mouse_position_raw.x * input_mouse_position_offset.x, input_mouse_position_raw.y * input_mouse_position_offset.y, 0f);
    }

    /// <summary>
    /// Gets the target position of the mouse calculated to be on the same plane as the player character. Allways returns value and requires less Compute than GetTargetMousePos();
    /// </summary>
    public static Vector3 MousePositionPlane(Vector3 plane_pos)
    {
        float distance;
        Plane plane = new Plane(Vector3.up, plane_pos);
        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    /// <summary>
    /// Gets the target position of the mouse calculated to be on the same plane as the player character. Allways returns value and requires less Compute than GetTargetMousePos();
    /// </summary>
    public static Vector3 MousePositionPlayerPlane()
    {
        float distance;
        Plane plane = new Plane(Vector3.up, Global.player_transform.position + new Vector3(0f, player_offset_to_ground, 0f));
        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance) - new Vector3(0f, player_offset_to_ground, 0f);
    }

    /// <summary>
    /// Gets the target position of the mouse calculated to be on the same plane as the player character. Allways returns value and requires less Compute than GetTargetMousePos();
    /// </summary>
    public static Vector3 MousePositionPlayerOceanPlane()
    {
        float distance;
        Plane plane = new Plane(Vector3.up, -Water.water_level);
        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    /// <summary>
    /// Get world position of object mouse is pointing at.
    /// </summary>
    public static Vector3 MousePosition()
    {
        return Camera.main.ScreenToWorldPoint(GetInputMousePosition());
    }

    public static Rigidbody MouseHitRigidbody()
    {
        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        hits = Physics.RaycastAll(ray, 250f);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            if (!Layer.IsInLayer(Layer.Mask.ignore_forces, hits[i].transform.gameObject.layer))
            {
                Debug.Log(hits[i].transform.gameObject.layer);
                if (hits[i].rigidbody != null)
                {
                    return hits[i].rigidbody;
                }
                else
                {
                    return RigidbodySetup.AddRigidbody(hits[i].transform.gameObject);
                }
            }
        }
        return null;
    }

    public static Rigidbody MouseHitRigidbody(float sphere_cast_radius)
    {
        Rigidbody exact_rigidbody = MouseHitRigidbody();
        if (exact_rigidbody != null)
        {
            return exact_rigidbody;
        }

        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        hits = Physics.SphereCastAll(ray, sphere_cast_radius, 250f);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            if (!Layer.IsInLayer(Layer.Mask.ignore_forces, hits[i].transform.gameObject.layer))
            {
                Debug.Log(hits[i].transform.gameObject.layer);
                if (hits[i].rigidbody != null)
                {
                    return hits[i].rigidbody;
                }
                else
                {
                    return RigidbodySetup.AddRigidbody(hits[i].transform.gameObject);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Gets world point of mouse cursor ignoring enemies.
    /// </summary>
    public static Vector3 MousePositionWorld()
    {
        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        if (Physics.Raycast(ray, out hit_data, 250f, Layer.Mask.ground))
        {
            return hit_data.point;
        }
        return MousePositionPlayerPlane();
    }

    /// <summary>
    /// Gets world point of mouse cursor.
    /// </summary>
    public static Vector3 MousePositionWorldAndEnemy()
    {
        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        if (Physics.Raycast(ray, out hit_data, 250f, Layer.Mask.ground | (1 << Layer.enemy)))
        {
            return hit_data.point;
        }
        return MousePositionPlayerPlane();
    }

    /// <summary>
    /// Gets world point of mouse cursor and middle of enemy collider if enemy is targeted.
    /// </summary>
    public static Vector3 MousePositionWorldAndEnemyMid()
    {
        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        if (Physics.Raycast(ray, out hit_data, 250f, Layer.Mask.ground | (1 << Layer.enemy)))
        {
            if (Layer.IsInLayer(Layer.enemy, hit_data.collider.gameObject.layer))
            {
                return hit_data.collider.bounds.center;
            }
            return hit_data.point;
        }
        return MousePositionPlayerPlane();
    }
}
