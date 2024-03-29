﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthNode : Node
{
    private EnemyAI ai;

    public HealthNode(EnemyAI ai)
    {
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        return ai.currentHealth <= 0f ? NodeState.success : NodeState.failure;
    }
}
