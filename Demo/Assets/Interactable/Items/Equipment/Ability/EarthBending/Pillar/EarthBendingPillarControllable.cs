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
        if (ready_pillars <= 0)
        {
            yield break;
        }

        WaitForSeconds wait = new WaitForSeconds(pillar_traverse_time);
        WaitForEndOfFrame wait_frame = new WaitForEndOfFrame();

        Vector3 mouse_point = MousePoint.MousePositionWorldAndEnemy();
        Vector3 last_pillar_point = mouse_point;
        SpawnRock(mouse_point);
        ready_pillars--;

        while (ready_pillars > 0)
        {
            yield return wait;

            while (true)
            {
                mouse_point = MousePoint.MousePositionPlane(mouse_point);
                if ((mouse_point - last_pillar_point).sqrMagnitude > pillar_gap)
                {
                    break;
                }
                yield return wait_frame;
            }

            Vector3 direction = (mouse_point - last_pillar_point);
            direction.y = 0f;
            direction = direction.normalized;

            last_pillar_point = last_pillar_point + direction * pillar_gap;

            SpawnRock(last_pillar_point);
            ready_pillars--;
        }
    }
}
