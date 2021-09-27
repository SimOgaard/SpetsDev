using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGolemAvailableNode : Node
{
    private Transform throwable_parrent_transform;
    private GolemBehaviour golem_behaviour;

    public IsGolemAvailableNode(Transform throwable_parrent_transform, GolemBehaviour golem_behaviour)
    {
        this.throwable_parrent_transform = throwable_parrent_transform;
        this.golem_behaviour = golem_behaviour;
    }

    public override NodeState Evaluate()
    {
        Transform closest_golem = FindClosestGolem();
        golem_behaviour.SetClosestGolem(closest_golem);
        return closest_golem != null && !golem_behaviour.has_golem_in_hands ? NodeState.success : NodeState.failure;
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
            dist_pow = (golem.position - golem_behaviour.transform.position).sqrMagnitude;
            if (dist_pow < min_dist_pow)
            {
                min_dist_pow = dist_pow;
                closest_golem = golem;
            }
        }
        return closest_golem;
    }
}
