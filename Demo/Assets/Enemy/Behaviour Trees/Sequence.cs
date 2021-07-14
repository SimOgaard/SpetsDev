using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : Node
{
    protected List<Node> nodes = new List<Node>();

    public Sequence(List<Node> nodes)
    {
        this.nodes = nodes;
    }

    public override NodeState Evaluate()
    {
        bool is_any_node_running = false;
        foreach (var node in nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.running:
                    is_any_node_running = true;
                    break;
                case NodeState.success:
                    break;
                case NodeState.failure:
                    _node_state = NodeState.failure;
                    return _node_state;
            }
        }
        _node_state = is_any_node_running ? NodeState.running : NodeState.success;
        return _node_state;
    }
}
