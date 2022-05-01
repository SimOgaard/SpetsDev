using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceInWorld : MonoBehaviour
{
    public InstantiateInstruction thisInstruction;
    public InstantiateInstruction childInstruction;

    private List<Collider> boundingBoxes = new List<Collider>();

    [SerializeField] private int childTransformAmount = 0;

    private void AddToBoundingBoxesLocal(List<Collider> bounds)
    {
        boundingBoxes.AddRange(bounds);
    }

    public static void SetRecursiveToGameWorld(GameObject obj)
    {
        if (Layer.IsInLayer(Layer.Mask.spawnedGameWorld, obj.layer))
        {
            obj.layer = Layer.gameWorld;
        }

        foreach (Transform child in obj.transform)
        {
            SetRecursiveToGameWorld(child.gameObject);
        }
    }

    public static Quaternion RandomRotationObject(float rotationOffset, float rotationIncrement)
    {
        return Quaternion.Euler(0f, RandomRotationAroundAxis(rotationOffset, rotationIncrement), 0f);
    }

    public static float RandomRotationAroundAxis(float rotationOffset, float rotationIncrement)
    {
        return rotationOffset + rotationIncrement * Mathf.RoundToInt(Random.Range(0f, 360f));
    }

    public static Vector3 GetVectorBySharedXYZ(float x, float y, float z, InstantiateInstruction.SharedXYZ sharedXYZ)
    {
        switch (sharedXYZ)
        {
            case InstantiateInstruction.SharedXYZ.none:
                return new Vector3(x, y, z);
            case InstantiateInstruction.SharedXYZ.xy:
                return new Vector3(x, x, z);
            case InstantiateInstruction.SharedXYZ.xz:
                return new Vector3(x, y, x);
            case InstantiateInstruction.SharedXYZ.yz:
                return new Vector3(x, y, y);
            case InstantiateInstruction.SharedXYZ.xyz:
                return new Vector3(x, x, x);
        }
        return Vector3.zero;
    }

    public static Vector3 RandomVector(Vector3 minScale, Vector3 maxScale, InstantiateInstruction.SharedXYZ sharedXYZ)
    {
        float x = Random.Range(minScale.x, maxScale.x);
        float y = Random.Range(minScale.y, maxScale.y);
        float z = Random.Range(minScale.z, maxScale.z);

        return GetVectorBySharedXYZ(x, y, z, sharedXYZ);
    }

    public static Vector3 RandomVector(Vector3 minScale, Vector3 maxScale, InstantiateInstruction.SharedXYZ sharedXYZ, Vector3 currentScale)
    {
        float x = Random.Range(minScale.x, maxScale.x) * currentScale.x;
        float y = Random.Range(minScale.y, maxScale.y) * currentScale.y;
        float z = Random.Range(minScale.z, maxScale.z) * currentScale.z;

        return GetVectorBySharedXYZ(x, y, z, sharedXYZ);
    }

    public static Vector3 RandomVector(Vector3 minScale, Vector3 maxScale, InstantiateInstruction.SharedXYZ sharedXYZ, Quaternion rotation)
    {
        float x = Random.Range(minScale.x, maxScale.x);
        float y = Random.Range(minScale.y, maxScale.y);
        float z = Random.Range(minScale.z, maxScale.z);

        return rotation * GetVectorBySharedXYZ(x, y, z, sharedXYZ);
    }

    public static (Vector3 point, Vector3 normal, bool rayHit) PointNormalWithRayCast(Vector3 origin, Vector3 randomVector, Vector3 direction, LayerMask placableLayerMask)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin + Vector3.up * 100f + randomVector, direction, 400f, placableLayerMask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            return (hits[i].point, hits[i].normal, true);
        }
        return (Vector3.zero, Vector3.zero, false);
    }

    private void CountDownChild(bool isParrent)
    {
        if (isParrent)
        {
            childTransformAmount = -1;
        }
        else
        {
            childTransformAmount--;
        }
    }

    private bool DestroyOnMissingChildren(int childCount, int minChildAmountRequired)
    {
        if (childCount < minChildAmountRequired)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    public static IEnumerator Spawn(WaitForFixedUpdate wait, System.Action<List<Collider>> AddToBoundingBoxesLocal, System.Func<List<Collider>> GetBoundingBoxes, System.Action<bool> CountDownChild, Transform transform, Transform chunkTransform, InstantiateInstruction InstantiateInstruction, bool isParrent = false)
    {
        //Debug.Log("Spawn");
        // Should this Transform spawn?
        if (InstantiateInstruction.noiseLayer.enabled)
        {
            Noise.NoiseLayer noise = new Noise.NoiseLayer(InstantiateInstruction.noiseLayer);
            float noiseValue = noise.GetNoiseValue(transform.position.x, transform.position.z);

            if (!(Random.value <= InstantiateInstruction.spawnChance || (noiseValue > InstantiateInstruction.spawnRangeNoise.x && noiseValue < InstantiateInstruction.spawnRangeNoise.y && Random.value <= InstantiateInstruction.spawnChanceNoise)))
            {
                Destroy(transform.gameObject);
                CountDownChild(isParrent);
                //Debug.Log("Random destroy noise");
                yield break;
            }
        }
        else
        {
            if (!(Random.value <= InstantiateInstruction.spawnChance))
            {
                Destroy(transform.gameObject);
                CountDownChild(isParrent);
                //Debug.Log("Random destroy");
                yield break;
            }
        }

        // Raycast
        yield return wait;
        if (transform == null)
        {
            //Debug.Log("transform == null");
            yield break;
        }
        (Vector3 point, Vector3 normal, bool rayHit) = PointNormalWithRayCast(
            transform.position,
            RandomVector(InstantiateInstruction.minRayPosition, InstantiateInstruction.maxRayPosition, InstantiateInstruction.sharedRayPosition, transform.localScale),
            -transform.up,
            InstantiateInstruction.rayLayerMask
        );

        // Place transform.
        if (rayHit && Vector3.Dot(normal, Vector3.up) >= Mathf.Cos(InstantiateInstruction.maxRotation * Mathf.Deg2Rad))
        {
            transform.position = point;
        }
        else // if (InstantiateInstruction.rayLayerMask.value != 0)
        {
            Destroy(transform.gameObject);
            CountDownChild(isParrent);
            //Debug.Log("ray didnt hit");
            yield break;
        }

        // Apply rotation, scale and position offsets.
        if (InstantiateInstruction.rotateTwordsGroundNormal)
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            transform.RotateAround(transform.position, transform.up, RandomRotationAroundAxis(InstantiateInstruction.rotationOffset, InstantiateInstruction.rotationIncrement));
        }
        else if (InstantiateInstruction.rotationOffset + InstantiateInstruction.rotationIncrement != 0)
        {
            if (InstantiateInstruction.resetRotation)
            {
                transform.rotation = Quaternion.identity;
            }
            transform.rotation = RandomRotationObject(InstantiateInstruction.rotationOffset, InstantiateInstruction.rotationIncrement);
        }
        transform.localScale = RandomVector(InstantiateInstruction.minScale, InstantiateInstruction.maxScale, InstantiateInstruction.sharedScales, transform.localScale);
        transform.localPosition += transform.rotation * RandomVector(InstantiateInstruction.minPositionLocal, InstantiateInstruction.maxPositionLocal, InstantiateInstruction.localSharedPosition, transform.localScale);
        transform.localPosition += RandomVector(InstantiateInstruction.minPosition, InstantiateInstruction.maxPosition, InstantiateInstruction.globalSharedPosition, transform.localScale);

        // Change parrent on delay so that destroys further in this script can effect them aswell.
        if (!isParrent && InstantiateInstruction.parrentName != InstantiateInstruction.PlacableGameObjectsParrent.keep)
        {
            transform.parent = chunkTransform.Find(InstantiateInstruction.GetHierarchyName(InstantiateInstruction.parrentName));
        }

        yield return wait;
        if (transform == null)
        {
            //Debug.Log("transform == null 2");
            yield break;
        }

        if (InstantiateInstruction.density != Density.DensityValues.ignore)
        {
            Density density = JoinMeshes.GetAddComponent(transform.gameObject, typeof(Density)) as Density;
            density.density = InstantiateInstruction.density;
        }

        if (transform.TryGetComponent(out BoundingBoxes boundingBoxesObject))
        {
            //Debug.Log("BoundingBoxes found");
            List<Collider> bounds = boundingBoxesObject.GetBoundingBoxes();
            List<Collider> boundsToBeRemoved = new List<Collider>();

            if (!InstantiateInstruction.ignoreBoundingBoxes)
            {
                //Debug.Log("Do not ignore bounding boxes");
                bool destroyChildren = boundingBoxesObject.ShouldDestroyChildren();

                foreach (Collider spawningCollider in bounds)
                {
                    //Debug.Log(spawningCollider);

                    foreach (Collider instanciatedColliders in GetBoundingBoxes())
                    {
                        if (destroyChildren)
                        {
                            ////Debug.Log("intersected: " + spawningCollider.bounds.Intersects(instanciatedColliders.bounds));
                            if (spawningCollider.bounds.Intersects(instanciatedColliders.bounds))
                            {
                                CountDownChild(isParrent);
                                boundsToBeRemoved.Add(spawningCollider);
                                Destroy(spawningCollider.gameObject);
                            }
                        }
                        else
                        {
                            ////Debug.Log("intersected: " + spawningCollider.bounds.Intersects(instanciatedColliders.bounds));
                            if (spawningCollider.bounds.Intersects(instanciatedColliders.bounds))
                            {
                                CountDownChild(isParrent);
                                Destroy(transform.gameObject);
                                //Debug.Log("spawningCollider intersects with instanciatedColliders");
                                //Debug.Log(transform.gameObject, transform.gameObject);
                                yield break;
                            }
                        }
                    }
                }

                foreach (Collider bound in boundsToBeRemoved)
                {
                    bounds.Remove(bound);
                }

                AddToBoundingBoxesLocal(bounds);
                Destroy(boundingBoxesObject);
                //Debug.Log("added bounds");
                yield break;
            }

            AddToBoundingBoxesLocal(bounds);
            Destroy(boundingBoxesObject);
            //Debug.Log("added bounds");
            yield break;
        }
        else
        {
            //Debug.Log("no BoundingBoxes found");
            yield break;
        }
    }

    public IEnumerator InitAsChild(WaitForFixedUpdate wait, System.Action<List<Collider>> AddToBoundingBoxesLocal, System.Func<List<Collider>> GetBoundingBoxes, System.Action<bool> CountDownChild, Transform chunkTransform)
    {
        yield return StartCoroutine(Spawn(wait, AddToBoundingBoxesLocal, GetBoundingBoxes, CountDownChild, transform, chunkTransform, thisInstruction));
    }

    public IEnumerator InitAsParrent(WaitForFixedUpdate wait, System.Action<List<Collider>> AddToBoundingBoxes, System.Func<List<Collider>> GetBoundingBoxes, float x, float z, Transform chunkTransform)
    {
        //Debug.Log("IsInitAsParrent");

        // Get amount of children of gameobject. Because Destroy() doesnt regrister in transform.childCount.
        childTransformAmount = transform.childCount;

        // Spawn this gameobject.
        transform.position += new Vector3(x, 0f, z);
        yield return StartCoroutine(Spawn(wait, AddToBoundingBoxesLocal, GetBoundingBoxes, CountDownChild, transform, chunkTransform, thisInstruction, true));
        if (this == null)
        {
            //Debug.Log("IsInitAsParrent == null");
            yield break;
        }

        if (DestroyOnMissingChildren(childTransformAmount, thisInstruction.minChildAmountRequired))
        {
            //Debug.Log("ChildCount: " + childTransformAmount);
            yield break;
        }

        // this is neccesary since we destroy children
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        // Spawn children of gameobject.
        foreach (Transform child in children)
        {
            //Debug.Log(child, child);
            if (child.TryGetComponent(out PlaceInWorld placeInWorld))
            {
                //Debug.Log("place in world found");
                yield return StartCoroutine(placeInWorld.InitAsChild(wait, AddToBoundingBoxesLocal, GetBoundingBoxes, CountDownChild, chunkTransform));
                Destroy(placeInWorld);
            }
            else if (childInstruction != null)
            {
                //Debug.Log("child instruction found");
                yield return StartCoroutine(Spawn(wait, AddToBoundingBoxesLocal, GetBoundingBoxes, CountDownChild, child, chunkTransform, childInstruction));
            }
            if (DestroyOnMissingChildren(childTransformAmount, thisInstruction.minChildAmountRequired))
            {
                //Debug.Log("ChildCount: " + childTransformAmount);
                yield break;
            }
        }

        //Debug.Log("add to global boundingBoxes");
        AddToBoundingBoxes(boundingBoxes);
        transform.parent = chunkTransform.Find(InstantiateInstruction.GetHierarchyName(thisInstruction.parrentName));

        if (gameObject.TryGetComponent<SetAsEnemy>(out SetAsEnemy setAsEnemy))
        {
            setAsEnemy.Init();
        }

        Destroy(this);
    }
}
