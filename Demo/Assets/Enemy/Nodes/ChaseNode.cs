﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseNode : Node
{
    private Agent agent;
    private Transform transform;
    private EnemyAI ai;

    public ChaseNode(Agent agent, EnemyAI ai, Transform transform)
    {
        this.agent = agent;
        this.transform = transform;
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        ai.SetColor(Color.yellow);
        float distance = (ai.chase_transform.position - transform.position).sqrMagnitude;
        if (distance > 1f)
        {
            agent.StartMoving();
            agent.destination = ai.chase_transform.position;
            return NodeState.running;
        }
        else
        {
            agent.StopMoving();
            return NodeState.success;
        }
    }
}
