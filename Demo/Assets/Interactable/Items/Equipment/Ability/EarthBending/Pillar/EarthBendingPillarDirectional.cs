using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBendingPillarDirectional : EarthBendingPillarBase , Equipment.IEquipment
{
    private Vector3 start_point;

    // 
    public override void UsePrimary()
    {
        start_point = MousePoint.MousePositionWorldAndEnemy();
        /*
        if ((ready_pillars >= max_pillars) && current_cooldown == 0f || bufferable && !casting)
        {
            casting = true;
            pillar_spawn_coroutine = FollowMouse();
            StartCoroutine(pillar_spawn_coroutine);
        }
        */
    }

    public override void StopPrimary()
    {
        // make mousepoint script static or inherrented from equipment
        Vector3 end_point = MousePoint.MousePositionPlane(start_point);
        Vector3 direction = (end_point - start_point);
        direction.y = 0f;
        StartCoroutine(SpawnStraight(start_point, direction.normalized));
        Debug.Log("EarthbendingAbility.StopPrimary");
    }

    public override void Update()
    {
        base.Update();
        /*
        if (Time has gone since use primary and hasnt stopped primary)
        {
            StopPrimary();
        }
        */
    }
}
