using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoToGolemNode : Node
{
    private Agent agent;
    private EnemyAI ai;
    private GolemBehaviour golem_behaviour;

    public GoToGolemNode(Agent agent, EnemyAI ai, GolemBehaviour golem_behaviour)
    {
        this.agent = agent;
        this.ai = ai;
        this.golem_behaviour = golem_behaviour;
    }

    public override NodeState Evaluate()
    {
        Transform golem = golem_behaviour.GetClosestGolem();
        if (golem == null)
        {
            return NodeState.failure;
        }

        ai.SetColor(Color.blue);
        float distance = (golem.position - agent.transform.position).sqrMagnitude;
        if (distance > 1f)
        {
            agent.StartMoving();
            agent.destination = golem.position;
            return NodeState.running;
        }
        else
        {
            agent.StopMoving();
            return NodeState.success;
        }
    }
}
