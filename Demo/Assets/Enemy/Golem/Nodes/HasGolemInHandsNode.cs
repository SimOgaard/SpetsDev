using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasGolemInHandsNode : Node
{
    private GolemBehaviour golemBehaviour;

    public HasGolemInHandsNode(GolemBehaviour golemBehaviour)
    {
        this.golemBehaviour = golemBehaviour;
    }

    public override NodeState Evaluate()
    {
        if (golemBehaviour.hasGolemInHands)
        {
            return NodeState.success;
        }
        return NodeState.failure;
    }
}
