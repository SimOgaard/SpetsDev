using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armature : MonoBehaviour
{
    public Bone armL;
    public Bone armR;
    public Bone legL;
    public Bone legR;

    private void Awake()
    {
        armL.Init();
        armR.Init();
        legL.Init();
        legR.Init();
    }

    private void Update()
    {
        armL.ikTarget.LerpPath(Time.deltaTime);
        legL.ikTarget.LerpPath(Time.deltaTime);

        armL.ResolveIK();
        armR.ResolveIK();
        legL.ResolveIK();
        legR.ResolveIK();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        armL.DrawGUI(transform);
        armR.DrawGUI(transform);
        legL.DrawGUI(transform);
        legR.DrawGUI(transform);
    }
#endif
}
