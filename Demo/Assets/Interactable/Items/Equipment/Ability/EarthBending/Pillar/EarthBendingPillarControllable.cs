using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBendingPillarControllable : EarthBendingPillarBase, Equipment.IEquipment
{
    public override void UsePrimary()
    {
        StartCoroutine(FollowMouse());
    }

    public override void StopPrimary()
    {
        StopAllCoroutines();
    }
    
    private IEnumerator FollowMouse()
    {
        if (readyPillars <= 0)
        {
            yield break;
        }

        WaitForSeconds wait = new WaitForSeconds(pillarTraverseTime);
        WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

        Vector3 mousePoint = MousePoint.MousePositionWorldAndEnemy();
        Vector3 lastPillarPoint = mousePoint;
        SpawnRock(mousePoint);
        readyPillars--;

        while (readyPillars > 0)
        {
            yield return wait;

            while (true)
            {
                mousePoint = MousePoint.MousePositionPlane(mousePoint);
                if ((mousePoint - lastPillarPoint).sqrMagnitude > pillarGap)
                {
                    break;
                }
                yield return waitFrame;
            }

            Vector3 direction = (mousePoint - lastPillarPoint);
            direction.y = 0f;
            direction = direction.normalized;

            lastPillarPoint = lastPillarPoint + direction * pillarGap;

            SpawnRock(lastPillarPoint);
            readyPillars--;
        }
    }
}
