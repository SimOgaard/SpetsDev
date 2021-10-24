using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class Bone
{
    public Target ik_target;
    public Target pole_target;
    [System.Serializable]
    public class Target
    {
        public Transform target;

        /*
        private static float start_to_end_distance = 0f;
        [SerializeField] private Vector3 _start_position;
        public Vector3 start_position
        {
            get { return _start_position; }
            set
            {
                _start_position = value;
                start_to_end_distance = (_start_position - end_position).magnitude * target.lossyScale.x;
                time = 0;
                target.position = value;
            }
        }
        [SerializeField] private Vector3 _end_position;
        public Vector3 end_position
        {
            get { return _end_position; }
            set
            {
                _end_position = value;
                start_to_end_distance = (start_position - _end_position).magnitude * target.lossyScale.x;
                time = ((target.position - start_position).magnitude * target.lossyScale.x) / start_to_end_distance;
            }
        }
        public void SetPosition(Vector3 start, Vector3 end)
        {
            target.position = start;
            _start_position = start;
            _end_position = end;
            start_to_end_distance = (_start_position - end_position).magnitude * target.lossyScale.x;
            time = 0f;
        }

        [SerializeField] private AnimationCurve lerp_function;
        [SerializeField] private float lerp_speed = 1f;
        private static float _time;
        private static float time
        {
            get { return _time; }
            set { _time = Mathf.Clamp01(value); }
        }
        public void LerpTarget(float delta_time)
        {
            time += delta_time * (lerp_speed / start_to_end_distance);
            float lerp_value = lerp_function.Evaluate(time);
            target.position = Vector3.Lerp(start_position, end_position, lerp_value);
        }
        */
        [SerializeField] private float lerp_speed = 1f;
        private static float time;
        public PathCreation.PathCreator path_creator;
        public PathCreation.EndOfPathInstruction end_of_path_instruction;
        public void LerpPath(float delta_time)
        {
            time += delta_time * lerp_speed;
            target.position = path_creator.path.GetPointAtDistance(time, end_of_path_instruction);
        }

        // Function that moves the whole transform of path_creator so that beginning matches with target.position

        // Question: vill vi att han ska slå efter body eller armature?
        // Body: animationer följer mer realistiskt
        // Armature animationer följer 
    }

    public RaycastGround raycast_ground;
    [System.Serializable]
    public class RaycastGround
    {
        public Transform ray_transform;
        public float distance;

        public Ray GetRay()
        {
            return new Ray(ray_transform.position, ray_transform.forward);
        }

        public RaycastHit RayCast()
        {
            RaycastHit raycast_hit;

            Ray ray = GetRay();
            Physics.Raycast(ray, out raycast_hit, distance * ray_transform.lossyScale.x, Layer.Mask.ground_enemy);

            return raycast_hit;
        }
    }

    [Header("Graphical User Interface")]
    [SerializeField] private Color gui_color = Color.green;
    [SerializeField] private float gui_radius = 0.25f;
#if UNITY_EDITOR
    public void DrawGUI(Transform armature_transform)
    {
        Gizmos.color = gui_color;

        // kinematics
        Gizmos.DrawSphere(ik_target.target.position, gui_radius);
        /*
        Gizmos.DrawSphere(ik_target.start_position + armature_transform.position, gui_radius);
        Gizmos.DrawSphere(ik_target.end_position + armature_transform.position, gui_radius);
        Gizmos.DrawLine(ik_target.start_position + armature_transform.position, ik_target.end_position + armature_transform.position);
        */
        // pole
        Gizmos.DrawSphere(pole_target.target.position, gui_radius);
        /*
        Gizmos.DrawSphere(pole_target.start_position + armature_transform.position, gui_radius);
        Gizmos.DrawSphere(pole_target.end_position + armature_transform.position, gui_radius);
        Gizmos.DrawLine(pole_target.start_position + armature_transform.position, pole_target.end_position + armature_transform.position);
        */
        // rays
        Ray ray = raycast_ground.GetRay();
        Gizmos.DrawLine(ray.origin, ray.GetPoint(raycast_ground.distance * armature_transform.lossyScale.x));

        // bone structure
        Transform current = leaf;
        for (int i = 0; i < chain_length && current != null && current.parent != null; i++)
        {
            float scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = gui_color;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            current = current.parent;
        }
    }
#endif
    /// <summary>
    /// INVERSE KINEMATICS
    /// </summary>
    [Header("Inverse Kinematics")]
    public Transform leaf;
    [SerializeField] private int chain_length = 1;

    [SerializeField] private int solve_iterations = 5;
    [SerializeField] private float solved_delta = 0.01f;
    [Range(0f, 1f)] [SerializeField] private float snap_back_strength = 1f;

    private float[] bones_length; //Target to Origin
    private float complete_length;
    private Transform[] bones;
    private Vector3[] positions;
    private Vector3[] start_direction_succ;
    private Quaternion[] start_rotation_bone;
    private Quaternion start_rotation_target;
    private Transform root;
    private float solved_delta_squared;

    public void Init()
    {
        // ik_target.SetPosition(ik_target.target.localPosition, ik_target.target.localPosition);
        // pole_target.SetPosition(pole_target.target.localPosition, pole_target.target.localPosition);

        //initial array
        bones = new Transform[chain_length + 1];
        positions = new Vector3[chain_length + 1];
        bones_length = new float[chain_length];
        start_direction_succ = new Vector3[chain_length + 1];
        start_rotation_bone = new Quaternion[chain_length + 1];
        solved_delta_squared = solved_delta * solved_delta;

        //find root
        root = leaf;
        for (var i = 0; i <= chain_length; i++)
        {
            root = root.parent;
        }

        //init data
        Transform current = leaf;
        complete_length = 0;
        for (var i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = current;
            start_rotation_bone[i] = GetRotationRootSpace(current);

            if (i == bones.Length - 1)
            {
                //leaf
                start_direction_succ[i] = GetPositionRootSpace(ik_target.target) - GetPositionRootSpace(current);
            }
            else
            {
                //mid bone
                start_direction_succ[i] = GetPositionRootSpace(bones[i + 1]) - GetPositionRootSpace(current);
                bones_length[i] = start_direction_succ[i].magnitude;
                complete_length += bones_length[i];
            }

            current = current.parent;
        }
    }

    public void ResolveIK()
    {
        //get position
        for (int i = 0; i < bones.Length; i++)
        {
            positions[i] = GetPositionRootSpace(bones[i]);
        }
        Vector3 target_position = GetPositionRootSpace(ik_target.target);
        Quaternion target_rotation = GetRotationRootSpace(ik_target.target);

        //1st is possible to reach?
        if ((target_position - GetPositionRootSpace(bones[0])).sqrMagnitude >= complete_length * complete_length)
        {
            //just strech it
            Vector3 direction = (target_position - positions[0]).normalized;
            //set everything after root
            for (int i = 1; i < positions.Length; i++)
            {
                positions[i] = positions[i - 1] + direction * bones_length[i - 1];
            }
        }
        else
        {
            for (int i = 0; i < positions.Length - 1; i++)
            {
                positions[i + 1] = Vector3.Lerp(positions[i + 1], positions[i] + start_direction_succ[i], snap_back_strength);
            }

            for (int iteration = 0; iteration < solve_iterations; iteration++)
            {
                for (int i = positions.Length - 1; i > 0; i--)
                {
                    if (i == positions.Length - 1)
                    {
                        //set it to target
                        positions[i] = target_position;
                    }
                    else
                    {
                        //set in line on distance
                        positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * bones_length[i];
                    }
                }

                //forward
                for (int i = 1; i < positions.Length; i++)
                {
                    positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * bones_length[i - 1];
                }

                //close enough?
                if ((positions[positions.Length - 1] - target_position).sqrMagnitude < solved_delta_squared)
                {
                    break;
                }
            }
        }

        //move towards pole
        if (pole_target.target != null)
        {
            Vector3 pole_position = GetPositionRootSpace(pole_target.target);
            for (int i = 1; i < positions.Length - 1; i++)
            {
                Plane plane = new Plane(positions[i + 1] - positions[i - 1], positions[i - 1]);
                Vector3 projected_pole = plane.ClosestPointOnPlane(pole_position);
                Vector3 projected_bone = plane.ClosestPointOnPlane(positions[i]);
                float angle = Vector3.SignedAngle(projected_bone - positions[i - 1], projected_pole - positions[i - 1], plane.normal);
                positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) + positions[i - 1];
            }
        }

        //set position & rotation
        for (int i = 0; i < positions.Length; i++)
        {
            if (i == positions.Length - 1)
            {
                SetRotationRootSpace(bones[i], Quaternion.Inverse(target_rotation) * start_rotation_target * Quaternion.Inverse(start_rotation_bone[i]));
            }
            else
            {
                SetRotationRootSpace(bones[i], Quaternion.FromToRotation(start_direction_succ[i], positions[i + 1] - positions[i]) * Quaternion.Inverse(start_rotation_bone[i]));
            }
            SetPositionRootSpace(bones[i], positions[i]);
        }
    }

    private Vector3 GetPositionRootSpace(Transform current)
    {
        if (root == null)
        {
            return current.position;
        }
        else
        {
            return Quaternion.Inverse(root.rotation) * (current.position - root.position);
        }
    }

    private void SetPositionRootSpace(Transform current, Vector3 position)
    {
        if (root == null)
        {
            current.position = position;
        }
        else
        {
            current.position = root.rotation * position + root.position;
        }
    }

    private Quaternion GetRotationRootSpace(Transform current)
    {
        //inverse(after) * before => rot: before -> after
        if (root == null)
        {
            return current.rotation;
        }
        else
        {
            return Quaternion.Inverse(current.rotation) * root.rotation;
        }
    }

    private void SetRotationRootSpace(Transform current, Quaternion rotation)
    {
        if (root == null)
        {
            current.rotation = rotation;
        }
        else
        {
            current.rotation = root.rotation * rotation;
        }
    }
}
