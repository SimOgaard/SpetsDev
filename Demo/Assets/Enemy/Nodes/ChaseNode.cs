using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseNode : Node
{
    private Agent agent;
    private Transform transform;
    private EnemyAI ai;
    private float speed;

    public ChaseNode(Agent agent, EnemyAI ai, Transform transform, float speed)
    {
        this.agent = agent;
        this.transform = transform;
        this.ai = ai;
        this.speed = speed;
    }

    public override NodeState Evaluate()
    {
        ai.SetColor(Color.yellow);
        Vector3 heading = (ai.chaseTransform.position - transform.position);
        float distance = heading.sqrMagnitude;
        if (distance > 1f)
        {
            agent.desiredSpeed = speed;
            agent.desiredHeading = heading.normalized;
            return NodeState.running;
        }
        else
        {
            agent.desiredSpeed = 0f;
            return NodeState.success;
        }
    }
}
