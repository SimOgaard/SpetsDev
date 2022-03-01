using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformNotNullNode : Node
{
    private EnemyAI enemyAi;

    public TransformNotNullNode(EnemyAI enemyAi)
    {
        this.enemyAi = enemyAi;
    }

    public override NodeState Evaluate()
    {
        if (enemyAi.chaseTransform == null)
        {
            return NodeState.failure;
        }
        else
        {
            return NodeState.success;
        }
    }
}
