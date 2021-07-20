using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGolemAvailableNode : Node
{
    private Transform[] available_golems;
    private EnemyAI ai;

    public IsGolemAvailableNode(Transform[] available_golems, EnemyAI ai)
    {
        this.available_golems = available_golems;
        this.ai = ai;
    }

    public override NodeState Evaluate()
    {
        Transform closest_golem = FindClosestGolem();
        ai.SetClosestGolem(closest_golem);
        return closest_golem != null && !ai.has_golem_in_hands ? NodeState.success : NodeState.failure;
    }

    private Transform FindClosestGolem()
    {
        Transform closest_golem = null;
        float min_dist_pow = float.PositiveInfinity;
        float dist_pow = 0f;
        for (int i = 0; i < available_golems.Length; i++)
        {
            if(available_golems[i] == null)
            {
                continue;
            }
            dist_pow = (available_golems[i].position - ai.transform.position).sqrMagnitude;
            if (dist_pow < min_dist_pow)
            {
                min_dist_pow = dist_pow;
                closest_golem = available_golems[i];
            }
        }
        return closest_golem;
    }
}
