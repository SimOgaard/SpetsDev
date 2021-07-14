using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : Node
{
    protected List<Node> nodes = new List<Node>();

    public Selector(List<Node> nodes)
    {
        this.nodes = nodes;
    }

    public override NodeState Evaluate()
    {
        foreach (var node in nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.running:
                    _node_state = NodeState.running;
                    return _node_state;
                case NodeState.success:
                    _node_state = NodeState.success;
                    return _node_state;
                case NodeState.failure:
                    break;
            }
        }
        _node_state = NodeState.failure;
        return _node_state;
    }
}
