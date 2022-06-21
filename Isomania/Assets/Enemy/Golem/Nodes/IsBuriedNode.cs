using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsBuriedNode : Node
{
    private GolemBehaviour golemBehaviour;

    public IsBuriedNode(GolemBehaviour golemBehaviour)
    {
        this.golemBehaviour = golemBehaviour;
    }

    public override NodeState Evaluate()
    {
        return golemBehaviour.isBuried ? NodeState.success : NodeState.failure;
    }
}
