using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseNode : Node
{
    private Transform target;
    private NavMeshAgent agent;
    private EnemyAI ai;

    public ChaseNode(Transform target, NavMeshAgent agent, EnemyAI ai)
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
            agent.isStopped = false;
            agent.SetDestination(target.position);
            return NodeState.running;
        }
        else
        {
            agent.isStopped = true;
            return NodeState.success;
        }
    }
}
