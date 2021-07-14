using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Node
{
    protected NodeState _node_state;
    public NodeState node_state { get { return _node_state; } }
    public abstract NodeState Evaluate();
}

public enum NodeState
{
    running,
    success,
    failure
}
