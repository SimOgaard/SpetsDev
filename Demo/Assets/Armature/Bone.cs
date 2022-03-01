using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class Bone
{
    public Target ikTarget;
    public Target poleTarget;
    [System.Serializable]
    public class Target
    {
        public Transform target;

        /*
        private static float startToEndDistance = 0f;
        [SerializeField] private Vector3 _startPosition;
        public Vector3 startPosition
        {
            get { return _startPosition; }
            set
            {
                _startPosition = value;
                startToEndDistance = (_startPosition - endPosition).magnitude * target.lossyScale.x;
                time = 0;
                target.position = value;
            }
        }
        [SerializeField] private Vector3 _endPosition;
        public Vector3 endPosition
        {
            get { return _endPosition; }
            set
            {
                _endPosition = value;
                startToEndDistance = (startPosition - _endPosition).magnitude * target.lossyScale.x;
                time = ((target.position - startPosition).magnitude * target.lossyScale.x) / startToEndDistance;
            }
        }
        public void SetPosition(Vector3 start, Vector3 end)
        {
            target.position = start;
            _startPosition = start;
            _endPosition = end;
            startToEndDistance = (_startPosition - endPosition).magnitude * target.lossyScale.x;
            time = 0f;
        }

        [SerializeField] private AnimationCurve lerpFunction;
        [SerializeField] private float lerpSpeed = 1f;
        private static float _time;
        private static float time
        {
            get { return _time; }
            set { _time = Mathf.Clamp01(value); }
        }
        public void LerpTarget(float deltaTime)
        {
            time += deltaTime * (lerpSpeed / startToEndDistance);
            float lerpValue = lerpFunction.Evaluate(time);
            target.position = Vector3.Lerp(startPosition, endPosition, lerpValue);
        }
        */
        [SerializeField] private float lerpSpeed = 1f;
        private static float time;
        public PathCreation.PathCreator pathCreator;
        public PathCreation.EndOfPathInstruction endOfPathInstruction;
        public void LerpPath(float deltaTime)
        {
            time += deltaTime * lerpSpeed;
            target.position = pathCreator.path.GetPointAtDistance(time, endOfPathInstruction);
        }

        // Function that moves the whole transform of pathCreator so that beginning matches with target.position

        // Question: vill vi att han ska sl� efter body eller armature?
        // Body: animationer f�ljer mer realistiskt
        // Armature animationer f�ljer 
    }

    public RaycastGround raycastGround;
    [System.Serializable]
    public class RaycastGround
    {
        public Transform rayTransform;
        public float distance;

        public Ray GetRay()
        {
            return new Ray(rayTransform.position, rayTransform.forward);
        }

        public RaycastHit RayCast()
        {
            RaycastHit raycastHit;

            Ray ray = GetRay();
            Physics.Raycast(ray, out raycastHit, distance * rayTransform.lossyScale.x, Layer.Mask.groundEnemy);

            return raycastHit;
        }
    }

    [Header("Graphical User Interface")]
    [SerializeField] private Color guiColor = Color.green;
    [SerializeField] private float guiRadius = 0.25f;
#if UNITY_EDITOR
    public void DrawGUI(Transform armatureTransform)
    {
        Gizmos.color = guiColor;

        // kinematics
        Gizmos.DrawSphere(ikTarget.target.position, guiRadius);
        /*
        Gizmos.DrawSphere(ikTarget.startPosition + armatureTransform.position, guiRadius);
        Gizmos.DrawSphere(ikTarget.endPosition + armatureTransform.position, guiRadius);
        Gizmos.DrawLine(ikTarget.startPosition + armatureTransform.position, ikTarget.endPosition + armatureTransform.position);
        */
        // pole
        Gizmos.DrawSphere(poleTarget.target.position, guiRadius);
        /*
        Gizmos.DrawSphere(poleTarget.startPosition + armatureTransform.position, guiRadius);
        Gizmos.DrawSphere(poleTarget.endPosition + armatureTransform.position, guiRadius);
        Gizmos.DrawLine(poleTarget.startPosition + armatureTransform.position, poleTarget.endPosition + armatureTransform.position);
        */
        // rays
        Ray ray = raycastGround.GetRay();
        Gizmos.DrawLine(ray.origin, ray.GetPoint(raycastGround.distance * armatureTransform.lossyScale.x));

        // bone structure
        Transform current = leaf;
        for (int i = 0; i < chainLength && current != null && current.parent != null; i++)
        {
            float scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = guiColor;
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
    [SerializeField] private int chainLength = 1;

    [SerializeField] private int solveIterations = 5;
    [SerializeField] private float solvedDelta = 0.01f;
    [Range(0f, 1f)] [SerializeField] private float snapBackStrength = 1f;

    private float[] bonesLength; //Target to Origin
    private float completeLength;
    private Transform[] bones;
    private Vector3[] positions;
    private Vector3[] startDirectionSucc;
    private Quaternion[] startRotationBone;
    private Quaternion startRotationTarget;
    private Transform root;
    private float solvedDeltaSquared;

    public void Init()
    {
        // ikTarget.SetPosition(ikTarget.target.localPosition, ikTarget.target.localPosition);
        // poleTarget.SetPosition(poleTarget.target.localPosition, poleTarget.target.localPosition);

        //initial array
        bones = new Transform[chainLength + 1];
        positions = new Vector3[chainLength + 1];
        bonesLength = new float[chainLength];
        startDirectionSucc = new Vector3[chainLength + 1];
        startRotationBone = new Quaternion[chainLength + 1];
        solvedDeltaSquared = solvedDelta * solvedDelta;

        //find root
        root = leaf;
        for (var i = 0; i <= chainLength; i++)
        {
            root = root.parent;
        }

        //init data
        Transform current = leaf;
        completeLength = 0;
        for (var i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = current;
            startRotationBone[i] = GetRotationRootSpace(current);

            if (i == bones.Length - 1)
            {
                //leaf
                startDirectionSucc[i] = GetPositionRootSpace(ikTarget.target) - GetPositionRootSpace(current);
            }
            else
            {
                //mid bone
                startDirectionSucc[i] = GetPositionRootSpace(bones[i + 1]) - GetPositionRootSpace(current);
                bonesLength[i] = startDirectionSucc[i].magnitude;
                completeLength += bonesLength[i];
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
        Vector3 targetPosition = GetPositionRootSpace(ikTarget.target);
        Quaternion targetRotation = GetRotationRootSpace(ikTarget.target);

        //1st is possible to reach?
        if ((targetPosition - GetPositionRootSpace(bones[0])).sqrMagnitude >= completeLength * completeLength)
        {
            //just strech it
            Vector3 direction = (targetPosition - positions[0]).normalized;
            //set everything after root
            for (int i = 1; i < positions.Length; i++)
            {
                positions[i] = positions[i - 1] + direction * bonesLength[i - 1];
            }
        }
        else
        {
            for (int i = 0; i < positions.Length - 1; i++)
            {
                positions[i + 1] = Vector3.Lerp(positions[i + 1], positions[i] + startDirectionSucc[i], snapBackStrength);
            }

            for (int iteration = 0; iteration < solveIterations; iteration++)
            {
                for (int i = positions.Length - 1; i > 0; i--)
                {
                    if (i == positions.Length - 1)
                    {
                        //set it to target
                        positions[i] = targetPosition;
                    }
                    else
                    {
                        //set in line on distance
                        positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * bonesLength[i];
                    }
                }

                //forward
                for (int i = 1; i < positions.Length; i++)
                {
                    positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * bonesLength[i - 1];
                }

                //close enough?
                if ((positions[positions.Length - 1] - targetPosition).sqrMagnitude < solvedDeltaSquared)
                {
                    break;
                }
            }
        }

        //move towards pole
        if (poleTarget.target != null)
        {
            Vector3 polePosition = GetPositionRootSpace(poleTarget.target);
            for (int i = 1; i < positions.Length - 1; i++)
            {
                Plane plane = new Plane(positions[i + 1] - positions[i - 1], positions[i - 1]);
                Vector3 projectedPole = plane.ClosestPointOnPlane(polePosition);
                Vector3 projectedBone = plane.ClosestPointOnPlane(positions[i]);
                float angle = Vector3.SignedAngle(projectedBone - positions[i - 1], projectedPole - positions[i - 1], plane.normal);
                positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) + positions[i - 1];
            }
        }

        //set position & rotation
        for (int i = 0; i < positions.Length; i++)
        {
            if (i == positions.Length - 1)
            {
                SetRotationRootSpace(bones[i], Quaternion.Inverse(targetRotation) * startRotationTarget * Quaternion.Inverse(startRotationBone[i]));
            }
            else
            {
                SetRotationRootSpace(bones[i], Quaternion.FromToRotation(startDirectionSucc[i], positions[i + 1] - positions[i]) * Quaternion.Inverse(startRotationBone[i]));
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
