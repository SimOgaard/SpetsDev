using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasGolemInHandsNode : Node
{
    private EnemyAI ai;

    public HasGolemInHandsNode(EnemyAI ai)
    {
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        if (ai.has_golem_in_hands)
        {
            return NodeState.success;
        }
        return NodeState.failure;
    }
}
