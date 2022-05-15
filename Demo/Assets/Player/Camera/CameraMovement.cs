using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves point that camera is focused on.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    private Rigidbody playerRigidbody;
    private Vector3 smoothedPosition;

    [Header("Focus Point Position")]
    [SerializeField] private float maxDistance;
    [SerializeField] private float smoothSpeed;
    [SerializeField] [Range(0f, 1f)] private float playerPositionDiffAmplitude;
    [SerializeField] private float playerHeadingXzAmplitude;
    [SerializeField] private float playerHeadingYAmplitude;
    [SerializeField] [Range(0f, 0.2f)] private float playerLookingPlaneAmplitude;
    [SerializeField] private Vector2 playerLookingPlaneAmplitudeXy;
    [SerializeField] [Range(0f, 0.2f)] private float playerLooking_3dAmplitude;
    [SerializeField] private float playerYOffset;

    [Header("Focus Point Rotation")]
    [SerializeField] private AnimationCurve rotationCurve;
    [SerializeField] private float yAxisRotation;
    [SerializeField] private float xAxisTime;
    [SerializeField] private float snapIncrement;
    //[SerializeField] [Range(0.5f, 1f)] private float reccursiveRotationThreshold;
    //[SerializeField] private float lastStoppedRotation;
    
    private void Start()
    {
        playerRigidbody = Global.playerTransform.GetComponent<PlayerMovement>()._rigidbody;
        transform.position = Global.playerTransform.position;
    }

    /// <summary>
    /// Calculates difference between actual player position and camera position, where player is moving twords and where mouse is pointing on a 2d plane and in 3d world and finally returns sum of all positions multiplied by their amplification.
    /// </summary>
    private Vector3 GetLookPoint()
    {
        Vector3 diff = transform.position - Global.playerTransform.position;
        Vector3 playerPositionDiff = (diff) * playerPositionDiffAmplitude;
        //Vector3 playerHeading = Vector3.Scale(playerMovement.controller.velocity, new Vector3(playerHeadingXzAmplitude, playerHeadingYAmplitude, playerHeadingXzAmplitude));
        //Vector3 playerHeading = Vector3.Scale(diff, new Vector3(-playerHeadingXzAmplitude, -playerHeadingYAmplitude, -playerHeadingXzAmplitude));
        Vector3 playerHeading = Vector3.Scale(playerRigidbody.velocity, new Vector3(playerHeadingXzAmplitude, playerHeadingYAmplitude, playerHeadingXzAmplitude));
        Vector3 playerLookingPlane = (MousePoint.MousePositionPlayerPlane(playerLookingPlaneAmplitudeXy.x, playerLookingPlaneAmplitudeXy.y) - Global.playerTransform.position) * playerLookingPlaneAmplitude;
        Vector3 playerLooking_3d = (MousePoint.MousePositionWorld() - Global.playerTransform.position) * playerLooking_3dAmplitude;

        Vector3 clampedLookPoint = Vector3.ClampMagnitude(playerPositionDiff + playerHeading + playerLookingPlane + playerLooking_3d, maxDistance);
        Vector3 scaledForward = Vector3.Scale(clampedLookPoint, Vector3.one + transform.forward * (1f - playerLookingPlaneAmplitudeXy.y));
        Vector3 scaledSideways = Vector3.Scale(clampedLookPoint, Vector3.one + transform.right * (1f - playerLookingPlaneAmplitudeXy.x));

        return scaledSideways + new Vector3(0f, playerYOffset, 0f);
    }

    /// <summary>
    /// Liniarly interpolates camera focus point with player position.
    /// </summary>
    private Vector3 SmoothMovementToPoint(Vector3 focusPoint)
    {
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, focusPoint, smoothSpeed * Time.deltaTime);
        return smoothedPosition;
    }

    [SerializeField] private float currentRotation;
    [SerializeField] private float wantedRotation;
    private float disjointment;
    private float offset;
    private float f(float x)
    {
        return (x - yAxisRotation * (wantedRotation + 0.5f)) / yAxisRotation;
    }
    private float SampleRotationCurve(float x, float yOffset = 0.5f)
    {
        return (Mathf.Abs(x - offset) - Mathf.Abs(x + offset) * 0.5f) + x + yOffset;
    }
    private float SampleDerivative(float x)
    {
        const float h = 0.001f;
        float rotationCurveValue_1 = SampleRotationCurve(x + h);
        float rotationCurveValue_2 = SampleRotationCurve(x - h);

        float derivative = (rotationCurveValue_1 - rotationCurveValue_2) / (h * 2f);
        return derivative;
    }

    /// <summary>
    /// Rotate with input float.
    /// </summary>
    private void Update()
    {
        /*
        float derivative = SampleDerivative(currentRotation);
        Debug.Log(derivative);            
        return;
        */
        if (PlayerInput.GetKeyDown(PlayerInput.leftRotation) || (PlayerInput.GetKeyUp(PlayerInput.rightRotation) && PlayerInput.GetKey(PlayerInput.leftRotation)))
        {
            //wantedRotation += yAxisRotation;
            StopAllCoroutines();
            StartCoroutine(RotateCamera(1));
        }
        else if (PlayerInput.GetKeyDown(PlayerInput.rightRotation) || (PlayerInput.GetKeyUp(PlayerInput.leftRotation) && PlayerInput.GetKey(PlayerInput.rightRotation)))
        {
            //wantedRotation -= yAxisRotation;
            StopAllCoroutines();
            StartCoroutine(RotateCamera(-1));
        }
    }

    [Header("testing")]
    [SerializeField] private float ovalSizeLarge = 1f;
    [SerializeField] private float ovalSizeSmall = 1f;
    [SerializeField] private bool move = false;
    public void SmoothPosition()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif
        
        if (false)
        {
            // find mid pixel of player
            Vector2 playerMiddle = MousePoint.WorldToViewportPoint(Global.playerTransform.position) - new Vector3(0.5f, 0.5f, 0f);

            Debug.Log(playerMiddle);

            // check if it is outside of squarcircle
            float squarcircleValue = Mathf.Pow(playerMiddle.x, 4) + Mathf.Pow(playerMiddle.y, 4);
            if (squarcircleValue > Mathf.Pow(ovalSizeLarge, 4))
            {
                move = true;
            }
            else if (squarcircleValue < Mathf.Pow(ovalSizeSmall, 4))
            {
                move = false;
            }

            if (move)
            {
                smoothedPosition = SmoothMovementToPoint(Global.playerTransform.position);
                transform.position = smoothedPosition;
            }

            // if it is

            // find mid pixel of player
            // check if it is inside of smaller oval
            // if it is
            // stop moving camera
        }
        else
        {
            smoothedPosition = SmoothMovementToPoint(Global.playerTransform.position + GetLookPoint());
            transform.position = smoothedPosition;
        }

        //smoothedPosition = SmoothMovementToPoint(playerTransform.position + GetLookPoint());
        //Vector3 screenPos = cam.WorldToScreenPoint(target.position);
    }

    [SerializeField] private float reccursiveRotationThreshold = 0.5f;
    [SerializeField] private float lastStoppedRotation;
    private float C = 0f;
    private float oldDirectionHeading = 0f;
    //private float oldDirection = 0f;
    private IEnumerator RotateCamera(float direction)
    {
        /*
        if (oldDirection != direction && oldDirection != 0f)
        {
            if (direction < 0f)
            {
                wantedRotation = currentRotation - currentRotation % yAxisRotation;
            }
            else
            {
                wantedRotation = currentRotation - currentRotation % yAxisRotation + yAxisRotation;
            }
        }
        else
        {
            wantedRotation += direction * yAxisRotation;
        }
        oldDirection = direction;
        */
        wantedRotation += direction * yAxisRotation;

        float directionHeading = currentRotation < wantedRotation ? 1f : -1f;
        if (oldDirectionHeading != directionHeading && oldDirectionHeading != 0f)
        {
            lastStoppedRotation = currentRotation - (currentRotation % yAxisRotation) - directionHeading * yAxisRotation;
        }
        oldDirectionHeading = directionHeading;

        float A = directionHeading * yAxisRotation * 0.5f + lastStoppedRotation;
        float B = wantedRotation - directionHeading * yAxisRotation * 0.5f;

        float h = 0.001f;
        float rateOfChangeMiddle = ((rotationCurve.Evaluate(0.5f + h) - rotationCurve.Evaluate(0.5f - h)) / (h * 2f * xAxisTime)) * yAxisRotation;

        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (wantedRotation != currentRotation)
        {
            if (C < A && directionHeading > 0f || C > A && directionHeading < 0f)
            {
                C += directionHeading * ((Time.deltaTime / xAxisTime) * yAxisRotation);
                float xTime = directionHeading * (C - lastStoppedRotation) / yAxisRotation;
                float curveRotationValue = rotationCurve.Evaluate(xTime);
                currentRotation = lastStoppedRotation + directionHeading * curveRotationValue * yAxisRotation;
            }
            else if (C > B && directionHeading > 0f || C < B && directionHeading < 0f)
            {
                float b = wantedRotation - directionHeading * yAxisRotation;
                C += directionHeading * ((Time.deltaTime / xAxisTime) * yAxisRotation);
                float xTime = (directionHeading * (C - B) / yAxisRotation) + 0.5f;
                float curveRotationValue = rotationCurve.Evaluate(xTime);

                bool left = PlayerInput.GetKey(PlayerInput.leftRotation);
                bool right = PlayerInput.GetKey(PlayerInput.rightRotation);
                if (left && !right)
                {
                    currentRotation += directionHeading * rateOfChangeMiddle * Time.deltaTime;
                    C = currentRotation;
                    transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

                    if (xTime > reccursiveRotationThreshold)
                    {
                        StartCoroutine(RotateCamera(1));
                        yield break;
                    }
                    yield return wait;
                    continue;
                }
                if (right && !left)
                {
                    currentRotation += directionHeading * rateOfChangeMiddle * Time.deltaTime;
                    C = currentRotation;
                    transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

                    if (xTime > reccursiveRotationThreshold)
                    {
                        StartCoroutine(RotateCamera(-1));
                        yield break;
                    }
                    yield return wait;
                    continue;
                }

                currentRotation = b + directionHeading * curveRotationValue * yAxisRotation;
            }
            else
            {
                currentRotation += directionHeading * rateOfChangeMiddle * Time.deltaTime;
                C = currentRotation;
            }

            transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
            yield return wait;
        }
        lastStoppedRotation = currentRotation;
        C = currentRotation;
    }


    private IEnumerator RotateCameraAfterCurve(AnimationCurve curve)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        float oldMaxDistance = maxDistance;
        maxDistance = 0f;

        float curveTimeEnd = curve[curve.length - 1].time;
        float curveTimeCurrent = 0f;

        while (curveTimeCurrent < curveTimeEnd)
        {
            curveTimeCurrent += Time.deltaTime;
            Debug.Log(curveTimeCurrent);
            Debug.Log(curve.Evaluate(curveTimeCurrent));
            transform.rotation = Quaternion.Euler(0f, curve.Evaluate(curveTimeCurrent), 0f);
            yield return wait;
        }
        maxDistance = oldMaxDistance;
    }

    private IEnumerator RotateCameraAfterCurveContinue(AnimationCurve curve)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        float curveTimeEnd = curve[curve.length - 1].time;
        float curveTimeCurrent = 0f;

        float currentRotation = transform.rotation.eulerAngles.y;

        while (curveTimeCurrent < curveTimeEnd)
        {
            yield return wait;
            curveTimeCurrent += Time.deltaTime;
            currentRotation += curve.Evaluate(curveTimeCurrent);
            transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
        }
    }
}
