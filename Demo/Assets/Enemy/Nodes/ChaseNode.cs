using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseNode : Node
{
    private Transform target;
    private Agent agent;
    private Transform transform;
    private EnemyAI ai;

    public ChaseNode(Transform target, Agent agent, EnemyAI ai, Transform transform)
    {
        this.target = target;
        this.agent = agent;
        this.transform = transform;
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        ai.SetColor(Color.yellow);
        float distance = (target.position - transform.position).sqrMagnitude;
        if (distance > 1f)
        {
            agent.StartMoving();
            agent.destination = target.position;
            return NodeState.running;
        }
        else
        {
            agent.StopMoving();
            return NodeState.success;
        }
    }
}
