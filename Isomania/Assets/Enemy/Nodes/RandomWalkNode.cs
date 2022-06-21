using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWalkNode : Node
{
    private EnemyAI enemyAi;
    private float wanderStrength;

    public RandomWalkNode(EnemyAI enemyAi, float wanderStrength)
    {
        this.enemyAi = enemyAi;
    }

    public override NodeState Evaluate()
    {
        Vector2 randomPoint = Random.insideUnitCircle * wanderStrength * Time.deltaTime;
        enemyAi.oldChasePosition += new Vector3(randomPoint.x, 0f, randomPoint.y);
        return NodeState.success;
    }
}
