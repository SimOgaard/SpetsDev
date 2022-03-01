using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script initilizes and controlls rock game object that all Earthbending Equipments utilizes.
/// </summary>
public class EarthBendingRock : MonoBehaviour
{
    public enum MoveStates { up, still, down };
    public MoveStates moveState = MoveStates.up;

    public Rigidbody rockRigidbody;

    public float sleepTime;            // time in seconds rock should be still for.
    public float growthSpeed;          // how quickly rock should move.
    public float currentSleepTime;

    private const float rayClearance = 50f;

    public bool shouldBeDeleted = false;

    public bool dealDamageByCollision = false;
    public bool dealDamageByTrigger = false;
    public float damage = 0f;

    private float _growthTime;
    public float growthTime
    {
        get { return _growthTime; }
        set { _growthTime = Mathf.Clamp01(value); }
    }
    
    //private List<CombineInstance> combinedMeshInstance = new List<CombineInstance>();

    public float soundAmplifier = 0f;
    public float maxSound = Mathf.Infinity;

    public void SetSound(float soundAmplifier, float maxSound = Mathf.Infinity)
    {
        this.soundAmplifier = soundAmplifier;
        this.maxSound = maxSound;
    }

    public float underGroundHeight = 1f;

    /// <summary>
    /// Merges theese rocks which this script ultimately controls.
    /// </summary>
    /*
    public void InitEarthbendingPillar(GameObject rockGameObjectToMerge)
    {
        MeshFilter meshFilter = rockGameObjectToMerge.GetComponent<MeshFilter>();
        CombineInstance combine = new CombineInstance();
        combine.mesh = meshFilter.sharedMesh;
        combine.transform = transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
        combinedMeshInstance.Add(combine);
    }
    
    /// <summary>
    /// Merges meshes and sets values for merged rock this script controls.
    /// </summary>
    public void SetSharedValues(float sleepTime, float moveSpeed, float height, Material material)
    {
        this.sleepTime = sleepTime;
        currentSleepTime = sleepTime;
        this.moveSpeed = moveSpeed;

        this.underGroundPoint = transform.position;
        this.groundPoint = transform.position + transform.rotation * new Vector3(0f, height, 0f);

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combinedMeshInstance.ToArray());
        gameObject.AddComponent<MeshFilter>().mesh = combinedMesh;
        gameObject.AddComponent<MeshCollider>().sharedMesh = combinedMesh;
        gameObject.AddComponent<MeshRenderer>().material = material;
    }
    */

    /// <summary>
    /// Initilizes a kinematic rigidbody.
    /// </summary>
    private void Awake()
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rock.transform.parent = transform;
        rock.transform.localPosition = Vector3.up * 0.5f;
        rock.GetComponent<MeshRenderer>().material = Global.Materials.stoneMaterial;

        gameObject.layer = Layer.gameWorldMoving;
        rock.layer = Layer.gameWorldMoving;

        Density density = rock.AddComponent<Density>();
        rockRigidbody = rock.AddComponent<Rigidbody>();
        rockRigidbody.isKinematic = true;
    }

    /// <summary>
    /// Places rock at ground given a point.
    /// </summary>
    public virtual void PlacePillar(Vector3 point, Quaternion rotation, Vector3 scale)
    {
        transform.rotation = rotation;
        transform.localScale = scale;
        (Vector3 placePoint, Vector3 normal) = GetRayHitData(point, rotation, scale);
        transform.position = placePoint;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Returns ground point given a point and rotation and scale.
    /// </summary>
    public (Vector3 point, Vector3 normal) GetRayHitData(Vector3 point, Quaternion rotation, Vector3 scale)
    {
        float longestDist = Mathf.NegativeInfinity;
        Vector3 normSum = Vector3.zero;
        RaycastHit hitData;
        for (float x = -scale.x * 0.5f; x <= scale.x * 0.5f; x += scale.x)
        {
            for (float z = -scale.z * 0.5f; z <= scale.z * 0.5f; z += scale.z)
            {
                Vector3 offset = rotation * new Vector3(x, 0f, z);
                GetRayHitData(point + offset, rotation, out hitData);
                normSum += hitData.normal;
                if (hitData.distance > longestDist)
                {
                    longestDist = hitData.distance;
                }
            }
        }
        Debug.Log(longestDist);
        Vector3 spawnPoint = point + rotation * Vector3.down * (longestDist - rayClearance);
        return (spawnPoint, normSum.normalized);
    }

    /// <summary>
    /// Return ground point given a point and rotation.
    /// </summary>
    public void GetRayHitData(Vector3 point, Quaternion rotation, out RaycastHit hitData)
    {
        Vector3 up = rotation * Vector3.up;
        Physics.Raycast(point + up * rayClearance, -up, out hitData, rayClearance * 2f, Layer.Mask.staticGround);
    }

    /*
    /// <summary>
    /// Places rock at ground if ground position in y axis is in range of starting point.
    /// Otherwise places rock at given point.
    /// </summary>
    public void PlacePillar(Vector3 point, float yMaxDiff, bool ignore)
    {
        if (Physics.Raycast(point + new Vector3(0f, 20f, 0f), Vector3.down, out hitData, 40f, Layer.Mask.staticGround))
        {
            if (Mathf.Abs(hitData.point.y - point.y) > yMaxDiff && !ignore)
            {
                PlacePillarPoint(point);
                return;
            }

            transform.position = hitData.point - transform.rotation * new Vector3(0f, transform.localScale.y * 0.5f, 0f);
            this.groundPoint = hitData.point;
            this.underGroundPoint = transform.position;
        }
        else
        {
            PlacePillarPoint(point);
        }
    }
    */
    /*
    /// <summary>
    /// Places rock at given point.
    /// </summary>
    public void PlacePillarPoint(Vector3 point)
    {
        transform.position = point - transform.rotation * new Vector3(0f, transform.localScale.y * 0.5f, 0f);
        this.groundPoint = point;
        this.underGroundPoint = transform.position;
    }
    */
    /// <summary>
    /// Calculates point where rock is hidden nomatter its rotation.
    /// </summary>
    /*
    public void PlacePillarHidden(Vector3 point, int solveItteration = 10)
    {
        float height = transform.localScale.y;
        float width = transform.localScale.x;
        Quaternion rotation = transform.rotation;

        Vector3 rockDownDirection = rotation * Vector3.down;
        float heightByItteration = height / solveItteration;

        Vector3 groundPointMiddle = GetGroundPoint(point);

        float maxYDisplacement = 0f;
        for (int i = 1; i < solveItteration; i++)
        {
            Vector3 testPoint = point + rockDownDirection * heightByItteration * i;
            Vector3 groundPointCurrent = GetGroundPoint(testPoint);
            float yDiff = groundPointCurrent.y - testPoint.y;
            if (yDiff < maxYDisplacement)
            {
                maxYDisplacement = yDiff;
            }
        }
        groundPointMiddle.y += maxYDisplacement * 2f;

        PlacePillarPoint(groundPointMiddle);
    }
    */
    /*

    public Vector3 GetGroundPoint(Vector3 point, float heightOffset = 20f, float rayMaxDistance = 40f)
    {
        if (Physics.Raycast(point + new Vector3(0f, 20f, 0f), Vector3.down, out hitData, 40f, Layer.Mask.staticGround))
        {
            return hitData.point;
        }
        return point;
    }
    */
    private string damageId = "";
    public void SetDamageId(string damageId)
    {
        this.damageId = damageId;
    }

    public void DealDamageByTrigger(Vector3 center, float radius)
    {
        dealDamageByTrigger = true;
        SphereCollider damageColliderTrigger = gameObject.AddComponent<SphereCollider>();
        damageColliderTrigger.isTrigger = true;
        damageColliderTrigger.center = center;
        damageColliderTrigger.radius = radius;
    }

    public void DealDamageByTrigger()
    {
        dealDamageByTrigger = true;
        SphereCollider damageColliderTrigger = gameObject.AddComponent<SphereCollider>();
        damageColliderTrigger.isTrigger = true;
        damageColliderTrigger.center = new Vector3(0f, 0.5f, 0f);
        damageColliderTrigger.radius = 0.05f;
    }

    public void SetTrigger()
    {
        dealDamageByTrigger = true;
        GetComponent<BoxCollider>().isTrigger = true;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (dealDamageByCollision && Layer.IsInLayer(Layer.enemy, collision.gameObject.layer) && moveState == MoveStates.up)
        {
            collision.transform.parent.GetComponent<EnemyAI>().Damage(damage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (dealDamageByTrigger && Layer.IsInLayer(Layer.enemy, other.gameObject.layer) && moveState == MoveStates.up)
        {
            Transform parent = other.transform.parent;
            if (parent.GetComponent<EnemyAI>().Damage(damage, damageId, 0.25f))
            {
                parent.GetComponent<Agent>().AddForce(750f * transform.up, ForceMode.Impulse);
            }
        }
    }
}
