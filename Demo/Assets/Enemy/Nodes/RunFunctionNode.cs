using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunFunctionNode : Node
{
    private System.Func<NodeState> method;

    public RunFunctionNode(System.Func<NodeState> method)
    {
        this.method = method;
    }

    public override NodeState Evaluate()
    {
        return method();
    }
}
