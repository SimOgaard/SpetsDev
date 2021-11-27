using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToGolemNode : Node
{
    private Agent agent;
    private EnemyAI ai;
    private GolemBehaviour golem_behaviour;
    private float speed;

    public GoToGolemNode(Agent agent, EnemyAI ai, GolemBehaviour golem_behaviour, float speed)
    {
        this.agent = agent;
        this.ai = ai;
        this.golem_behaviour = golem_behaviour;
        this.speed = speed;
    }

    public override NodeState Evaluate()
    {
        Transform golem = golem_behaviour.GetClosestGolem();
        if (golem == null)
        {
            return NodeState.failure;
        }

        ai.SetColor(Color.blue);
        Vector3 heading = (golem.position - agent.transform.position);
        float distance = heading.sqrMagnitude;
        if (distance > 1f)
        {
            agent.desired_speed = speed;
            agent.desired_heading = heading.normalized;
            return NodeState.running;
        }
        else
        {
            agent.desired_speed = 0;
            return NodeState.success;
        }
    }
}
