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
                _node_state = NodeState.running;
                break;
            case NodeState.success:
                _node_state = NodeState.failure;
                break;
            case NodeState.failure:
                _node_state = NodeState.success;
                break;
        }
        return _node_state;
    }
}
