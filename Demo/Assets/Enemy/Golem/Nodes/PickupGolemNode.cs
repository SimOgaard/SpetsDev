using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupGolemNode : Node
{
    private GolemBehaviour golem_behaviour;

    public PickupGolemNode(GolemBehaviour golem_behaviour)
    {
        this.golem_behaviour = golem_behaviour;
    }

    public override NodeState Evaluate()
    {
        golem_behaviour.has_golem_in_hands = true;
        golem_behaviour.PlaceGolemInHands();
        return NodeState.success;
    }
}
