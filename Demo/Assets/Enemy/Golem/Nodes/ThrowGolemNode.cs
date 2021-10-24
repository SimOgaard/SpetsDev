using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowGolemNode : Node
{
    private Agent agent;
    private EnemyAI ai;
    private GolemBehaviour golem_behaviour;

    public ThrowGolemNode(Agent agent, EnemyAI ai, GolemBehaviour golem_behaviour)
    {
        this.agent = agent;
        this.ai = ai;
        this.golem_behaviour = golem_behaviour;
    }

    public override NodeState Evaluate()
    {
        agent.StopMoving();
        ai.SetColor(Color.magenta);

        Debug.Log("threw golem");
        golem_behaviour.has_golem_in_hands = false;

        return NodeState.running;
    }
}
