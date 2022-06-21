using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowGolemNode : Node
{
    private Agent agent;
    private EnemyAI ai;
    private GolemBehaviour golemBehaviour;

    public ThrowGolemNode(Agent agent, EnemyAI ai, GolemBehaviour golemBehaviour)
    {
        this.agent = agent;
        this.ai = ai;
        this.golemBehaviour = golemBehaviour;
    }

    public override NodeState Evaluate()
    {
        agent.desiredSpeed = 0f;
        ai.SetColor(Color.magenta);

        Debug.Log("threw golem");
        golemBehaviour.hasGolemInHands = false;

        return NodeState.running;
    }
}
