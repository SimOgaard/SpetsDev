using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWalkNode : Node
{
    private EnemyAI enemy_ai;
    private float wander_strength;

    public RandomWalkNode(EnemyAI enemy_ai, float wander_strength)
    {
        this.enemy_ai = enemy_ai;
    }

    public override NodeState Evaluate()
    {
        Vector2 random_point = Random.insideUnitCircle * wander_strength * Time.deltaTime;
        enemy_ai.old_chase_position += new Vector3(random_point.x, 0f, random_point.y);
        return NodeState.success;
    }
}
