using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoToGolemNode : Node
{
    private NavMeshAgent agent;
    private EnemyAI ai;

    public GoToGolemNode(NavMeshAgent agent, EnemyAI ai)
    {
        this.agent = agent;
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        Transform golem = ai.GetClosestGolem();
        if (golem == null)
        {
            return NodeState.failure;
        }

        ai.SetColor(Color.blue);
        float distance = (golem.position - agent.transform.position).sqrMagnitude;
        if (distance > 1f)
        {
            agent.isStopped = false;
            agent.SetDestination(golem.position);
            return NodeState.running;
        }
        else
        {
            agent.isStopped = true;
            return NodeState.success;
        }
    }
}
