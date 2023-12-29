using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemRangeNode : Node
{
    private float range;
    private GolemBehaviour golemBehaviour;

    public GolemRangeNode(float range, GolemBehaviour golemBehaviour)
    {
        this.range = range * range;
        this.golemBehaviour = golemBehaviour;
    }

    public override NodeState Evaluate()
    {
        float distance = (golemBehaviour.GetClosestGolem().position - golemBehaviour.transform.position).sqrMagnitude;
        return distance <= range ? NodeState.success : NodeState.failure;
    }
}
