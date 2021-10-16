using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemStandUpTest : MonoBehaviour
{
    [SerializeField] private Bone arm_l;
    [SerializeField] private Bone arm_r;
    [SerializeField] private Bone leg_l;
    [SerializeField] private Bone leg_r;

    [Header("Body")]
    [SerializeField] private Transform body_ik;
    [SerializeField] private float body_lerp_time;
    private Vector3 saved_body_position;
    private Vector3 saved_body_position_world;

    [Header("lol")]
    [SerializeField] private PathCreation.PathCreator pathCreator;
    [SerializeField] private PathCreation.EndOfPathInstruction endOfPathInstruction;

    private void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        /*
        arm_l.DrawGUI(transform);
        arm_r.DrawGUI(transform);
        leg_l.DrawGUI(transform);
        leg_l.DrawGUI(transform);
        */
    }

    /*
    private void Start()
    {
        SavePosition();

        DigUnderGround();
        StartCoroutine(RaiseOverGround());
    }
    
    public void DigUnderGround()
    {
        Vector3 hand_ray_offset = transform.rotation * (hand_ray_offset_to_origo * transform.lossyScale.x);
        Vector3 hand_ray_offset_mirror = Vector3.Reflect(hand_ray_offset, transform.right);

        Vector3 foot_ray_offset = transform.rotation * (foot_ray_offset_to_origo * transform.lossyScale.x);
        Vector3 foot_ray_offset_mirror = Vector3.Reflect(foot_ray_offset, transform.right);

        RaycastHit hand_ray_info;
        RaycastHit hand_ray_info_mirror;
        RaycastHit foot_ray_info;
        RaycastHit foot_ray_info_mirror;

        Physics.Raycast(transform.position + hand_ray_offset, Vector3.down, out hand_ray_info, hand_ray_distance, Layer.Mask.ground_enemy);
        Physics.Raycast(transform.position + hand_ray_offset_mirror, Vector3.down, out hand_ray_info_mirror, hand_ray_distance, Layer.Mask.ground_enemy);
        Physics.Raycast(transform.position + foot_ray_offset, Vector3.down, out foot_ray_info, foot_ray_distance, Layer.Mask.ground_enemy);
        Physics.Raycast(transform.position + foot_ray_offset_mirror, Vector3.down, out foot_ray_info_mirror, foot_ray_distance, Layer.Mask.ground_enemy);

        hand_ik_r.position = hand_ray_info.point;
        hand_ik_l.position = hand_ray_info_mirror.point;
        foot_ik_r.position = foot_ray_info.point;
        foot_ik_l.position = foot_ray_info_mirror.point;

        Vector3 arm_target_offset = transform.rotation * (arm_target_offset_to_origo_point * transform.lossyScale.x);
        Vector3 arm_target_offset_mirror = Vector3.Reflect(arm_target_offset, transform.right);

        arm_target_l.position = transform.position + arm_target_offset;
        arm_target_r.position = transform.position + arm_target_offset_mirror;

        Vector3 knee_target_offset = transform.rotation * (knee_target_offset_to_origo_point * transform.lossyScale.x);
        Vector3 knee_target_offset_mirror = Vector3.Reflect(knee_target_offset, transform.right);

        knee_target_l.position = transform.position + knee_target_offset;
        knee_target_r.position = transform.position + knee_target_offset_mirror;
    }

    public IEnumerator RaiseOverGround()
    {
        // Follow a Bezier Curve.
        // transform.up = Bezier_Curve.dx
        // transform.position = Bezier_Curve;
        float distance = pathCreator.path.length;
        float distanceTravelled = 0f;

        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        float speed = distance / body_lerp_time;
        while (distanceTravelled <= distance)
        {
            Vector3 arm_target_add = Vector3.up * arm_target_lerp_distance * (Time.deltaTime / body_lerp_time);
            arm_target_l.localPosition += arm_target_add;
            arm_target_r.localPosition += arm_target_add;

            
            //knee placement should be the same from the start
            //knee_target_l.localPosition += Vector3.up * time_change * body_lerp_distance;
            //knee_target_r.localPosition += Vector3.up * time_change * body_lerp_distance;
            

            distanceTravelled += Time.deltaTime * speed;
            body_ik.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            body_ik.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);

            yield return wait;
        }

        // reset positions
        transform.position += (body_ik.position - saved_body_position_world);
        GoToSavedPosition();

        // set arm poles to side and back of hands target
        // lerp body armature position to position where it is above ground
        // lerp arm poles uppwards

        // when arms are fully extended:
    }

    public void SavePosition()
    {
        saved_body_position = body_ik.localPosition;
        saved_body_position_world = body_ik.position;

        saved_arm_target_l_position = arm_target_l.localPosition;
        saved_arm_target_r_position = arm_target_r.localPosition;
        saved_knee_target_l_position = knee_target_l.localPosition;
        saved_knee_target_r_position = knee_target_r.localPosition;

        saved_hand_ik_l_position = hand_ik_l.localPosition;
        saved_hand_ik_r_position = hand_ik_r.localPosition;
        saved_foot_ik_l_position = foot_ik_l.localPosition;
        saved_foot_ik_r_position = foot_ik_r.localPosition;
    }

    public void GoToSavedPosition()
    {
        body_ik.localPosition = saved_body_position;

        arm_target_l.localPosition = saved_arm_target_l_position;
        arm_target_r.localPosition = saved_arm_target_r_position;
        knee_target_l.localPosition = saved_knee_target_l_position;
        knee_target_r.localPosition = saved_knee_target_r_position;

        hand_ik_l.localPosition = saved_hand_ik_l_position;
        hand_ik_r.localPosition = saved_hand_ik_r_position;
        foot_ik_l.localPosition = saved_foot_ik_l_position;
        foot_ik_r.localPosition = saved_foot_ik_r_position;
    }

    public void LerpToSavedPosition()
    {
        body_ik.localPosition = saved_body_position;

        arm_target_l.localPosition = saved_arm_target_l_position;
        arm_target_r.localPosition = saved_arm_target_r_position;
        knee_target_l.localPosition = saved_knee_target_l_position;
        knee_target_r.localPosition = saved_knee_target_r_position;

        hand_ik_l.localPosition = saved_hand_ik_l_position;
        hand_ik_r.localPosition = saved_hand_ik_r_position;
        foot_ik_l.localPosition = saved_foot_ik_l_position;
        foot_ik_r.localPosition = saved_foot_ik_r_position;
    }
    */
}
