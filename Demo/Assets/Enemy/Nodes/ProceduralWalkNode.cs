using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralWalkNode : Node
{
    private Transform target;
    private float wander_strength;

    public ProceduralWalkNode(Transform target, float wander_strength)
    {
        this.target = target;
        this.wander_strength = wander_strength;
    }

    public override NodeState Evaluate()
    {
        Vector2 random_point = Random.insideUnitCircle * wander_strength * Time.deltaTime;
        target.position += new Vector3(random_point.x, 0f, random_point.y);
        return NodeState.success;
    }
}
