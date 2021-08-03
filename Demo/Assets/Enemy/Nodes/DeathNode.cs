using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DeathNode : Node
{
    private EnemyAI ai;
    private Agent agent;

    public DeathNode(Agent agent, EnemyAI ai)
    {
        this.agent = agent;
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        agent.Die();
        ai.SetColor(Color.black);
        return NodeState.running;
    }
}
