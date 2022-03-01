using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inverter : Node
{
    protected Node node;

    public Inverter(Node node)
    {
        this.node = node;
    }

    public override NodeState Evaluate()
    {
        switch (node.Evaluate())
        {
            case NodeState.running:
                _nodeState = NodeState.running;
                break;
            case NodeState.success:
                _nodeState = NodeState.failure;
                break;
            case NodeState.failure:
                _nodeState = NodeState.success;
                break;
        }
        return _nodeState;
    }
}
