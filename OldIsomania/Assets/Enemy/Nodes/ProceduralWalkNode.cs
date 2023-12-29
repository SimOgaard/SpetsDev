using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralWalkNode : Node
{
    private Transform target;
    private float wanderStrength;

    public ProceduralWalkNode(Transform target, float wanderStrength)
    {
        this.target = target;
        this.wanderStrength = wanderStrength;
    }

    public override NodeState Evaluate()
    {
        if (target == null)
        {
            return NodeState.failure;
        }
        Vector2 randomPoint = Random.insideUnitCircle * wanderStrength * Time.deltaTime;
        target.position += new Vector3(randomPoint.x, 0f, randomPoint.y);
        return NodeState.success;
    }
}
