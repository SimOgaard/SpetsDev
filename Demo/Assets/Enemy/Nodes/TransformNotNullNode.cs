using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformNotNullNode : Node
{
    private EnemyAI enemy_ai;

    public TransformNotNullNode(EnemyAI enemy_ai)
    {
        this.enemy_ai = enemy_ai;
    }

    public override NodeState Evaluate()
    {
        if (enemy_ai.chase_transform == null)
        {
            return NodeState.failure;
        }
        else
        {
            return NodeState.success;
        }
    }
}
