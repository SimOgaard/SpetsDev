using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ThrowGolemNode : Node
{
    private Agent agent;
    private EnemyAI ai;

    public ThrowGolemNode(Agent agent, EnemyAI ai)
    {
        this.agent = agent;
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        agent.is_stopped = true;
        ai.SetColor(Color.magenta);

        Debug.Log("threw golem");
        ai.has_golem_in_hands = false;

        return NodeState.running;
    }
}
