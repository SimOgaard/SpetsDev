using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseVectorNode : Node
{
    private Agent agent;
    private EnemyAI ai;
    private Transform transform;
    private float speed;

    public ChaseVectorNode(EnemyAI ai, Agent agent, Transform transform, float speed)
    {
        this.agent = agent;
        this.transform = transform;
        this.ai = ai;
        this.speed = speed;
    }

    public override NodeState Evaluate()
    {
        ai.SetColor(Color.yellow);
        Vector3 heading = (ai.old_chase_position - agent.transform.position);
        float distance = heading.sqrMagnitude;
        if (distance > 1f)
        {
            agent.desired_speed = speed;
            agent.desired_heading = heading.normalized;
            return NodeState.running;
        }
        else
        {
            agent.desired_speed = 0f;
            return NodeState.success;
        }
    }
}
