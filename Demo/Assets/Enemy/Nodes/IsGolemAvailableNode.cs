using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGolemAvailableNode : Node
{
    private Transform throwable_parrent_transform;
    private EnemyAI ai;

    public IsGolemAvailableNode(Transform throwable_parrent_transform, EnemyAI ai)
    {
        this.throwable_parrent_transform = throwable_parrent_transform;
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
        if (throwable_parrent_transform == null)
        {
            return null;
        }

        Transform closest_golem = null;
        float min_dist_pow = float.PositiveInfinity;
        float dist_pow = 0f;
        foreach (Transform golem in throwable_parrent_transform)
        {
            if (golem == null)
            {
                continue;
            }
            dist_pow = (golem.position - ai.transform.position).sqrMagnitude;
            if (dist_pow < min_dist_pow)
            {
                min_dist_pow = dist_pow;
                closest_golem = golem;
            }
        }
        return closest_golem;
    }
}
