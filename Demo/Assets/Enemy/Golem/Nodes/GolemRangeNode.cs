using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemRangeNode : Node
{
    private float range;
    private GolemBehaviour golem_behaviour;

    public GolemRangeNode(float range, GolemBehaviour golem_behaviour)
    {
        this.range = range * range;
        this.golem_behaviour = golem_behaviour;
    }

    public override NodeState Evaluate()
    {
        float distance = (golem_behaviour.GetClosestGolem().position - golem_behaviour.transform.position).sqrMagnitude;
        return distance <= range ? NodeState.success : NodeState.failure;
    }
}
