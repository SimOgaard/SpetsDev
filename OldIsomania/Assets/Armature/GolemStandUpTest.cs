using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemStandUpTest : MonoBehaviour
{
    [SerializeField] private Bone armL;
    [SerializeField] private Bone armR;
    [SerializeField] private Bone legL;
    [SerializeField] private Bone legR;

    [Header("Body")]
    [SerializeField] private Transform bodyIk;
    [SerializeField] private float bodyLerpTime;
    private Vector3 savedBodyPosition;
    private Vector3 savedBodyPositionWorld;

    [Header("lol")]
    [SerializeField] private PathCreation.PathCreator pathCreator;
    [SerializeField] private PathCreation.EndOfPathInstruction endOfPathInstruction;

    private void Update()
    {
        
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        /*
        armL.DrawGUI(transform);
        armR.DrawGUI(transform);
        legL.DrawGUI(transform);
        legL.DrawGUI(transform);
        */
    }
#endif
    /*
    private void Start()
    {
        SavePosition();

        DigUnderGround();
        StartCoroutine(RaiseOverGround());
    }
    
    public void DigUnderGround()
    {
        Vector3 handRayOffset = transform.rotation * (handRayOffsetToOrigo * transform.lossyScale.x);
        Vector3 handRayOffsetMirror = Vector3.Reflect(handRayOffset, transform.right);

        Vector3 footRayOffset = transform.rotation * (footRayOffsetToOrigo * transform.lossyScale.x);
        Vector3 footRayOffsetMirror = Vector3.Reflect(footRayOffset, transform.right);

        RaycastHit handRayInfo;
        RaycastHit handRayInfoMirror;
        RaycastHit footRayInfo;
        RaycastHit footRayInfoMirror;

        Physics.Raycast(transform.position + handRayOffset, Vector3.down, out handRayInfo, handRayDistance, Layer.Mask.groundEnemy);
        Physics.Raycast(transform.position + handRayOffsetMirror, Vector3.down, out handRayInfoMirror, handRayDistance, Layer.Mask.groundEnemy);
        Physics.Raycast(transform.position + footRayOffset, Vector3.down, out footRayInfo, footRayDistance, Layer.Mask.groundEnemy);
        Physics.Raycast(transform.position + footRayOffsetMirror, Vector3.down, out footRayInfoMirror, footRayDistance, Layer.Mask.groundEnemy);

        handIkR.position = handRayInfo.point;
        handIkL.position = handRayInfoMirror.point;
        footIkR.position = footRayInfo.point;
        footIkL.position = footRayInfoMirror.point;

        Vector3 armTargetOffset = transform.rotation * (armTargetOffsetToOrigoPoint * transform.lossyScale.x);
        Vector3 armTargetOffsetMirror = Vector3.Reflect(armTargetOffset, transform.right);

        armTargetL.position = transform.position + armTargetOffset;
        armTargetR.position = transform.position + armTargetOffsetMirror;

        Vector3 kneeTargetOffset = transform.rotation * (kneeTargetOffsetToOrigoPoint * transform.lossyScale.x);
        Vector3 kneeTargetOffsetMirror = Vector3.Reflect(kneeTargetOffset, transform.right);

        kneeTargetL.position = transform.position + kneeTargetOffset;
        kneeTargetR.position = transform.position + kneeTargetOffsetMirror;
    }

    public IEnumerator RaiseOverGround()
    {
        // Follow a Bezier Curve.
        // transform.up = Bezier_Curve.dx
        // transform.position = Bezier_Curve;
        float distance = pathCreator.path.length;
        float distanceTravelled = 0f;

        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        float speed = distance / bodyLerpTime;
        while (distanceTravelled <= distance)
        {
            Vector3 armTargetAdd = Vector3.up * armTargetLerpDistance * (Time.deltaTime / bodyLerpTime);
            armTargetL.localPosition += armTargetAdd;
            armTargetR.localPosition += armTargetAdd;

            
            //knee placement should be the same from the start
            //kneeTargetL.localPosition += Vector3.up * timeChange * bodyLerpDistance;
            //kneeTargetR.localPosition += Vector3.up * timeChange * bodyLerpDistance;
            

            distanceTravelled += Time.deltaTime * speed;
            bodyIk.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            bodyIk.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);

            yield return wait;
        }

        // reset positions
        transform.position += (bodyIk.position - savedBodyPositionWorld);
        GoToSavedPosition();

        // set arm poles to side and back of hands target
        // lerp body armature position to position where it is above ground
        // lerp arm poles uppwards

        // when arms are fully extended:
    }

    public void SavePosition()
    {
        savedBodyPosition = bodyIk.localPosition;
        savedBodyPositionWorld = bodyIk.position;

        savedArmTargetLPosition = armTargetL.localPosition;
        savedArmTargetRPosition = armTargetR.localPosition;
        savedKneeTargetLPosition = kneeTargetL.localPosition;
        savedKneeTargetRPosition = kneeTargetR.localPosition;

        savedHandIkLPosition = handIkL.localPosition;
        savedHandIkRPosition = handIkR.localPosition;
        savedFootIkLPosition = footIkL.localPosition;
        savedFootIkRPosition = footIkR.localPosition;
    }

    public void GoToSavedPosition()
    {
        bodyIk.localPosition = savedBodyPosition;

        armTargetL.localPosition = savedArmTargetLPosition;
        armTargetR.localPosition = savedArmTargetRPosition;
        kneeTargetL.localPosition = savedKneeTargetLPosition;
        kneeTargetR.localPosition = savedKneeTargetRPosition;

        handIkL.localPosition = savedHandIkLPosition;
        handIkR.localPosition = savedHandIkRPosition;
        footIkL.localPosition = savedFootIkLPosition;
        footIkR.localPosition = savedFootIkRPosition;
    }

    public void LerpToSavedPosition()
    {
        bodyIk.localPosition = savedBodyPosition;

        armTargetL.localPosition = savedArmTargetLPosition;
        armTargetR.localPosition = savedArmTargetRPosition;
        kneeTargetL.localPosition = savedKneeTargetLPosition;
        kneeTargetR.localPosition = savedKneeTargetRPosition;

        handIkL.localPosition = savedHandIkLPosition;
        handIkR.localPosition = savedHandIkRPosition;
        footIkL.localPosition = savedFootIkLPosition;
        footIkR.localPosition = savedFootIkRPosition;
    }
    */
}
