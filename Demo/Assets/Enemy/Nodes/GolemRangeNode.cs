using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemRangeNode : Node
{
    private float range;
    private EnemyAI ai;

    public GolemRangeNode(float range, EnemyAI ai)
    {
        this.range = range;
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        float distance = (ai.GetClosestGolem().position - ai.transform.position).sqrMagnitude;
        return distance <= range ? NodeState.success : NodeState.failure;
    }
}
