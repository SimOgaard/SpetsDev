using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBendingRockTranspose : EarthBendingRock
{
    
    public Vector3 groundPoint;        // position you should move twords in the beginning, then sleep for some time.
    public Vector3 underGroundPoint;  // position you should spawn object in and move twords in the end then delete.
    /*
    /// <summary>
    /// Changes the rock this script controls.
    /// </summary>
    public void InitEarthbendingPillar(float height, float width, Quaternion rotation, float sleepTime, float growthSpeed)
    {
        ChangePillar(height, width, rotation, sleepTime, growthSpeed);
    }

    /// <summary>
    /// Changes scale, rotation, move speed and sleep time of this rock.
    /// </summary>
    public void ChangePillar(float height, float width, Quaternion rotation, float sleepTime, float growthSpeed)
    {
        transform.localScale = new Vector3(width, height * 2f, width);
        transform.rotation = rotation;
        this.sleepTime = sleepTime;
        currentSleepTime = sleepTime;
        this.growthSpeed = growthSpeed;
    }
    */

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
                rockRigidbody.MovePosition(Vector3.Lerp(underGroundPoint, groundPoint, growthTime));
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
                rockRigidbody.MovePosition(Vector3.Lerp(underGroundPoint, groundPoint, growthTime));
                break;
        }
    }

}
