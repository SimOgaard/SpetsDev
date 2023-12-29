using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGolemAvailableNode : Node
{
    private Transform throwableParrentTransform;
    private GolemBehaviour golemBehaviour;

    public IsGolemAvailableNode(Transform throwableParrentTransform, GolemBehaviour golemBehaviour)
    {
        this.throwableParrentTransform = throwableParrentTransform;
        this.golemBehaviour = golemBehaviour;
    }

    public override NodeState Evaluate()
    {
        Transform closestGolem = FindClosestGolem();
        golemBehaviour.SetClosestGolem(closestGolem);
        return closestGolem != null && !golemBehaviour.hasGolemInHands ? NodeState.success : NodeState.failure;
    }

    private Transform FindClosestGolem()
    {
        if (throwableParrentTransform == null)
        {
            return null;
        }

        Transform closestGolem = null;
        float minDistPow = float.PositiveInfinity;
        float distPow = 0f;
        foreach (Transform golem in throwableParrentTransform)
        {
            if (golem == null)
            {
                continue;
            }
            distPow = (golem.position - golemBehaviour.transform.position).sqrMagnitude;
            if (distPow < minDistPow)
            {
                minDistPow = distPow;
                closestGolem = golem;
            }
        }
        return closestGolem;
    }
}
