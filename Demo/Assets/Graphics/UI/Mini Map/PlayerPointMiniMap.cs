using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPointMiniMap : MonoBehaviour
{
    [SerializeField]
    private float y_const;

    private Vector3 position_vector;

    private Transform transform_follow;

    private void Start()
    {
        transform_follow = Global.player_transform;
    }

    private void Update()
    {
        position_vector = transform_follow.position;
        position_vector.y = y_const;
        transform.position = position_vector;
    }
}
