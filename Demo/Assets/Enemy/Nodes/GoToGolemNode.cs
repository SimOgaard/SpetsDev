using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoToGolemNode : Node
{
    private Agent agent;
    private EnemyAI ai;

    public GoToGolemNode(Agent agent, EnemyAI ai)
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
            agent.is_stopped = false;
            agent.destination = golem.position;
            return NodeState.running;
        }
        else
        {
            agent.is_stopped = true;
            return NodeState.success;
        }
    }
}
