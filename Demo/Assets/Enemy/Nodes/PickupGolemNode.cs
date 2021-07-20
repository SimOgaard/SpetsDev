using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupGolemNode : Node
{
    private EnemyAI ai;

    public PickupGolemNode(EnemyAI ai)
    {
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        ai.has_golem_in_hands = true;
        ai.DestroyGolemInHands();
        return NodeState.success;
    }
}
