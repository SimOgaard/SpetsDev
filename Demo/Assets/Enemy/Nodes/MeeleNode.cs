using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeeleNode : Node
{
    private Agent agent;
    private EnemyAI ai;

    public MeeleNode(Agent agent, EnemyAI ai)
    {
        this.agent = agent;
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        agent.is_stopped = true;
        ai.SetColor(Color.red);
        return NodeState.running;
    }
}
