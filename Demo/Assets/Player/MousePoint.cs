using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePoint : MonoBehaviour
{
    [SerializeField] private const float playerOffsetToGround = -3f;
    private static Ray ray;
    private static RaycastHit hitData;
    private static RaycastHit[] hits;

    /// <summary>
    /// The current mouse position in screen pixel coordinates. Translates the whole 400x255 px render texture to in game 384x216 px.
    /// </summary>
    private static Vector3 GetInputMousePosition(float widthScale = 1f, float heightScale = 1f)
    {
        Vector3 inputMousePositionRaw = Input.mousePosition;
        inputMousePositionRaw.x *= widthScale;
        inputMousePositionRaw.y *= heightScale;

        return new Vector3(inputMousePositionRaw.x * PixelPerfectCameraRotation.cameraScaleWidth, inputMousePositionRaw.y * PixelPerfectCameraRotation.cameraScaleHeight, 0f);
    }

    public static Vector3 WorldToViewportPoint(Vector3 point)
    {
        return Camera.main.WorldToViewportPoint(point);
    }

    /// <summary>
    /// Gets the target position of the mouse calculated to be on the same plane as the player character. Allways returns value and requires less Compute than GetTargetMousePos();
    /// </summary>
    public static Vector3 PositionRayPlane(Vector3 planePos, Vector3 planeNormal, Vector3 rayOrigin, Vector3 rayDirection)
    {
        float distance;
        Plane plane = new Plane(planeNormal, planePos);
        ray = new Ray(rayOrigin, rayDirection);
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    /// <summary>
    /// Gets the target position of the mouse calculated to be on the same plane as the player character. Allways returns value and requires less Compute than GetTargetMousePos();
    /// </summary>
    public static Vector3 PositionRayPlane(Vector3 planePos, Vector3 planeNormal, Ray ray)
    {
        float distance;
        Plane plane = new Plane(planeNormal, planePos);
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    /// <summary>
    /// Gets the target position of the mouse calculated to be on the same plane as the player character. Allways returns value and requires less Compute than GetTargetMousePos();
    /// </summary>
    public static Vector3 MousePositionPlane(Vector3 planePos)
    {
        float distance;
        Plane plane = new Plane(Vector3.up, planePos);
        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    /// <summary>
    /// Gets the target position of the mouse calculated to be on the same plane as the player character. Allways returns value and requires less Compute than GetTargetMousePos();
    /// </summary>
    public static Vector3 MousePositionPlayerPlane(float widthScale = 1f, float heightScale = 1f)
    {
        float distance;
        Plane plane = new Plane(Vector3.up, Global.playerTransform.position + new Vector3(0f, playerOffsetToGround, 0f));
        ray = Camera.main.ScreenPointToRay(GetInputMousePosition(widthScale, heightScale));
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance) - new Vector3(0f, playerOffsetToGround, 0f);
    }

    /// <summary>
    /// Gets the target position of the mouse calculated to be on the same plane as the player character. Allways returns value and requires less Compute than GetTargetMousePos();
    /// </summary>
    public static Vector3 MousePositionPlayerOceanPlane()
    {
        float distance;
        Plane plane = new Plane(Vector3.up, -Water.waterLevel);
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
            if (!Layer.IsInLayer(Layer.Mask.ignoreForces, hits[i].transform.gameObject.layer))
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

    public static Rigidbody MouseHitRigidbody(float sphereCastRadius)
    {
        Rigidbody exactRigidbody = MouseHitRigidbody();
        if (exactRigidbody != null)
        {
            return exactRigidbody;
        }

        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        hits = Physics.SphereCastAll(ray, sphereCastRadius, 250f);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            if (!Layer.IsInLayer(Layer.Mask.ignoreForces, hits[i].transform.gameObject.layer))
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
        if (Physics.Raycast(ray, out hitData, 250f, Layer.Mask.ground))
        {
            return hitData.point;
        }
        return MousePositionPlayerPlane();
    }

    /// <summary>
    /// Gets world point of mouse cursor.
    /// </summary>
    public static Vector3 MousePositionWorldAndEnemy()
    {
        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        if (Physics.Raycast(ray, out hitData, 250f, Layer.Mask.ground | (1 << Layer.enemy)))
        {
            return hitData.point;
        }
        return MousePositionPlayerPlane();
    }

    /// <summary>
    /// Gets world point of mouse cursor and middle of enemy collider if enemy is targeted.
    /// </summary>
    public static Vector3 MousePositionWorldAndEnemyMid()
    {
        ray = Camera.main.ScreenPointToRay(GetInputMousePosition());
        if (Physics.Raycast(ray, out hitData, 250f, Layer.Mask.ground | (1 << Layer.enemy)))
        {
            if (Layer.IsInLayer(Layer.enemy, hitData.collider.gameObject.layer))
            {
                return hitData.collider.bounds.center;
            }
            return hitData.point;
        }
        return MousePositionPlayerPlane();
    }
}
