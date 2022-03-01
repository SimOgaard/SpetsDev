using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupGolemNode : Node
{
    private GolemBehaviour golemBehaviour;

    public PickupGolemNode(GolemBehaviour golemBehaviour)
    {
        this.golemBehaviour = golemBehaviour;
    }

    public override NodeState Evaluate()
    {
        golemBehaviour.hasGolemInHands = true;
        golemBehaviour.PlaceGolemInHands();
        return NodeState.success;
    }
}
