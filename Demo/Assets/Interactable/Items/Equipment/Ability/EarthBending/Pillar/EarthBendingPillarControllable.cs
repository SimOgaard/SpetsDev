using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBendingPillarControllable : EarthBendingPillarBase, Equipment.IEquipment
{
    public override void StopPrimary()
    {
        Debug.Log("EarthbendingAbility.StopPrimary");
    }

    private IEnumerator FollowMouse()
    {
        yield return null;
    }
}
