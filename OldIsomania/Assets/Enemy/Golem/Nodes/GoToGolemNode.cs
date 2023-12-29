using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToGolemNode : Node
{
    private Agent agent;
    private EnemyAI ai;
    private GolemBehaviour golemBehaviour;
    private float speed;

    public GoToGolemNode(Agent agent, EnemyAI ai, GolemBehaviour golemBehaviour, float speed)
    {
        this.agent = agent;
        this.ai = ai;
        this.golemBehaviour = golemBehaviour;
        this.speed = speed;
    }

    public override NodeState Evaluate()
    {
        Transform golem = golemBehaviour.GetClosestGolem();
        if (golem == null)
        {
            return NodeState.failure;
        }

        ai.SetColor(Color.blue);
        Vector3 heading = (golem.position - agent.transform.position);
        float distance = heading.sqrMagnitude;
        if (distance > 1f)
        {
            agent.desiredSpeed = speed;
            agent.desiredHeading = heading.normalized;
            return NodeState.running;
        }
        else
        {
            agent.desiredSpeed = 0;
            return NodeState.success;
        }
    }
}
