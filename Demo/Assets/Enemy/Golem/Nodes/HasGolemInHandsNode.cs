using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasGolemInHandsNode : Node
{
    private GolemBehaviour golem_behaviour;

    public HasGolemInHandsNode(GolemBehaviour golem_behaviour)
    {
        this.golem_behaviour = golem_behaviour;
    }

    public override NodeState Evaluate()
    {
        if (golem_behaviour.has_golem_in_hands)
        {
            return NodeState.success;
        }
        return NodeState.failure;
    }
}
