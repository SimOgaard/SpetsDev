using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBendingPillarDirectional : EarthBendingPillarBase , Equipment.IEquipment
{
    private Vector3 start_point;

    public override void UsePrimary()
    {
        if (ready_pillars >= max_pillars)
        {
            start_point = MousePoint.MousePositionWorldAndEnemy();
        }
    }

    public override void StopPrimary()
    {
        Vector3 end_point = MousePoint.MousePositionPlane(start_point);
        Vector3 direction = (end_point - start_point);
        direction.y = 0f;
        direction = direction.normalized;
        StartCoroutine(SpawnStraight(start_point, direction));
    }

    /*
    public override void StopPrimary()
    {
        Vector3 end_point = MousePoint.MousePositionPlane(start_point);

        Vector3 mid_point = Vector3.Lerp(start_point, end_point, 0.5f);

        Vector3 direction = (end_point - start_point);
        direction.y = 0f;
        direction = direction.normalized;

        StartCoroutine(SpawnStraight(mid_point + direction * 0.5f, -direction));
        StartCoroutine(SpawnStraight(mid_point - direction * 0.5f, direction));
    }
    */
}
