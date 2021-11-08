using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseVectorNode : Node
{
    private Agent agent;
    private EnemyAI ai;
    private Transform transform;

    public ChaseVectorNode(EnemyAI ai, Agent agent, Transform transform)
    {
        this.agent = agent;
        this.transform = transform;
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        ai.SetColor(Color.yellow);
        float distance = (ai.old_chase_position - transform.position).sqrMagnitude;
        if (distance > 1f)
        {
            agent.StartMoving();
            agent.destination = ai.old_chase_position;
            return NodeState.running;
        }
        else
        {
            agent.StopMoving();
            return NodeState.success;
        }
    }
}
