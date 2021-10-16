using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armature : MonoBehaviour
{
    public Bone arm_l;
    public Bone arm_r;
    public Bone leg_l;
    public Bone leg_r;

    private void Awake()
    {
        arm_l.Init();
        arm_r.Init();
        leg_l.Init();
        leg_r.Init();
    }

    private void Update()
    {
        arm_l.ik_target.LerpPath(Time.deltaTime);
        leg_l.ik_target.LerpPath(Time.deltaTime);

        arm_l.ResolveIK();
        arm_r.ResolveIK();
        leg_l.ResolveIK();
        leg_r.ResolveIK();
    }

    private void OnDrawGizmos()
    {
        arm_l.DrawGUI(transform);
        arm_r.DrawGUI(transform);
        leg_l.DrawGUI(transform);
        leg_r.DrawGUI(transform);
    }
}
