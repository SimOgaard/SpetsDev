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
        bool isAnyNodeRunning = false;
        foreach (var node in nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.running:
                    isAnyNodeRunning = true;
                    break;
                case NodeState.success:
                    break;
                case NodeState.failure:
                    _nodeState = NodeState.failure;
                    return _nodeState;
            }
        }
        _nodeState = isAnyNodeRunning ? NodeState.running : NodeState.success;
        return _nodeState;
    }
}
