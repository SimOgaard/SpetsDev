using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsBuriedNode : Node
{
    private GolemBehaviour golem_behaviour;

    public IsBuriedNode(GolemBehaviour golem_behaviour)
    {
        this.golem_behaviour = golem_behaviour;
    }

    public override NodeState Evaluate()
    {
        return golem_behaviour.is_buried ? NodeState.success : NodeState.failure;
    }
}
