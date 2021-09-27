using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DeathNode : Node
{
    private EnemyAI ai;

    public DeathNode(EnemyAI ai)
    {
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        ai.Die();
        ai.SetColor(Color.black);
        return NodeState.running;
    }
}
