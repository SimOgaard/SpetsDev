using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBendingPillarDirectional : EarthBendingPillarBase , Equipment.IEquipment
{
    private Vector3 startPoint;

    public override void UsePrimary()
    {
        if (readyPillars >= maxPillars)
        {
            startPoint = MousePoint.MousePositionWorldAndEnemy();
        }
    }

    public override void StopPrimary()
    {
        Vector3 endPoint = MousePoint.MousePositionPlane(startPoint);
        Vector3 direction = (endPoint - startPoint);
        direction.y = 0f;
        direction = direction.normalized;
        StartCoroutine(SpawnStraight(startPoint, direction));
    }

    /*
    public override void StopPrimary()
    {
        Vector3 endPoint = MousePoint.MousePositionPlane(startPoint);

        Vector3 midPoint = Vector3.Lerp(startPoint, endPoint, 0.5f);

        Vector3 direction = (endPoint - startPoint);
        direction.y = 0f;
        direction = direction.normalized;

        StartCoroutine(SpawnStraight(midPoint + direction * 0.5f, -direction));
        StartCoroutine(SpawnStraight(midPoint - direction * 0.5f, direction));
    }
    */
}
