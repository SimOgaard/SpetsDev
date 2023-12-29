using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBendingRockScale : EarthBendingRock
{
    public float endScale;

    /// <summary>
    /// Places rock at ground given a point.
    /// </summary>
    public override void PlacePillar(Vector3 point, Quaternion rotation, Vector3 scale)
    {
        transform.rotation = rotation;
        endScale = scale.y;
        scale.y = underGroundHeight;
        transform.localScale = scale;
        (Vector3 placePoint, Vector3 normal) = GetRayHitData(point, rotation, scale);
        transform.position = placePoint + rotation * Vector3.down * underGroundHeight;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Controlls all three stages a rock goes through.
    /// Moving up and down and standing still. States can be changed outside of script.
    /// </summary>
    private void FixedUpdate()
    {
        float moveDiff;
        float sound;
        switch (moveState)
        {
            case MoveStates.up:
                if (growthTime == 1f)
                {
                    currentSleepTime = sleepTime;
                    rockRigidbody.Sleep();
                    moveState = MoveStates.still;
                    break;
                }
                moveDiff = growthSpeed * Time.fixedDeltaTime;
                sound = Mathf.Min(growthSpeed * soundAmplifier, maxSound);
                Enemies.Sound(transform, sound, Time.fixedDeltaTime);
                growthTime += moveDiff;
                transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(underGroundHeight, endScale, growthTime), transform.localScale.z);
                break;
            case MoveStates.still:
                currentSleepTime -= Time.deltaTime;
                if (currentSleepTime < 0f)
                {
                    rockRigidbody.WakeUp();
                    moveState = MoveStates.down;
                    break;
                }
                break;
            case MoveStates.down:
                if (growthTime == 0f)
                {
                    moveState = MoveStates.up;
                    if (shouldBeDeleted)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        rockRigidbody.Sleep();
                        gameObject.SetActive(false);
                    }
                    break;
                }
                moveDiff = growthSpeed * Time.fixedDeltaTime;
                sound = Mathf.Min(growthSpeed * soundAmplifier, maxSound);
                Enemies.Sound(transform, sound, Time.fixedDeltaTime);
                growthTime -= moveDiff;
                transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(underGroundHeight, endScale, growthTime), transform.localScale.z);
                break;
        }
    }

}
