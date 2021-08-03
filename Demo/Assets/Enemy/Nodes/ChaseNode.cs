using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseNode : Node
{
    private Transform target;
    private Agent agent;
    private EnemyAI ai;

    public ChaseNode(Transform target, Agent agent, EnemyAI ai)
    {
        this.target = target;
        this.agent = agent;
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        ai.SetColor(Color.yellow);
        float distance = (target.position - agent.transform.position).sqrMagnitude;
        if (distance > 1f)
        {
            agent.is_stopped = false;
            agent.destination = target.position;
            return NodeState.running;
        }
        else
        {
            agent.is_stopped = true;
            return NodeState.success;
        }
    }
}
