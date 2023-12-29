using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPointMiniMap : MonoBehaviour
{
    [SerializeField]
    private float yConst;

    private Vector3 positionVector;

    private Transform transformFollow;

    private void Start()
    {
        transformFollow = Global.playerTransform;
    }

    private void Update()
    {
        positionVector = transformFollow.position;
        positionVector.y = yConst;
        transform.position = positionVector;
    }
}
