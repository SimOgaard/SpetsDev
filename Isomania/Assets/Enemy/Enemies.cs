using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    public static readonly string[] enemyNames = { "lGolems", "mGolems", "sGolems" };

    /// <summary>
    /// List of all enemies.
    /// </summary>
    public static List<EnemyAI> allEnemyAis = new List<EnemyAI>();

    /*
    private void AddAllEnemiesInHierarchy(Transform parrent)
    {
        foreach (Transform child in parrent)
        {
            if (child.TryGetComponent(out EnemyAI enemyAi))
            {
                AddEnemyToList(enemyAi);
            }
            else
            {
                AddAllEnemiesInHierarchy(child);
            }
        }
    }
    */

    private void AddAllEnemiesInHierarchy()
    {
        for (int i = 0; i < enemyNames.Length; i++)
        {
            new GameObject(enemyNames[i]).transform.parent = transform;
        }
    }

    private void Awake()
    {
        AddAllEnemiesInHierarchy();
    }

    /// <summary>
    /// Adds EnemyAi component pointer to allEnemyAis list.
    /// </summary>
    public static void AddEnemyToList(EnemyAI enemyAi)
    {
        allEnemyAis.Add(enemyAi);
    }

    /// <summary>
    /// Removes ALL null objects in allEnemyAis list.
    /// </summary>
    public static void RemoveEnemyFromList()
    {
        allEnemyAis.RemoveAll(enemyAi => enemyAi == null);
    }
    
    public static void RemoveEnemyFromList(EnemyAI enemyToRemove)
    {
        allEnemyAis.Remove(enemyToRemove);
    }

    public static void RemoveEnemyFromList(List<EnemyAI> enemiesToRemove)
    {
        allEnemyAis.RemoveAll(enemy => enemiesToRemove.Contains(enemy));
    }

    public void MoveParrent()
    {
        string name;
        foreach (Transform enemyParrent in transform)
        {
            name = enemyParrent.name;

            foreach (Transform child in enemyParrent)
            {
                Transform newTransform = WorldGenerationManager.ReturnNearestChunk(child.position).transform;

                if (newTransform == null)
                {
                    Debug.Log("thismofo got shot to infinity");
                    EnemyAI enemyAi = child.GetComponent<EnemyAI>();
                    Enemies.RemoveEnemyFromList(enemyAi);
                    Destroy(child);
                }
                else if (newTransform != transform.parent)
                {
                    child.parent = newTransform.Find("Enemies").Find(name);
                }
                else
                {
                    EnemyAI enemyAi = child.GetComponent<EnemyAI>();
                    enemyAi.AttendToSound(null, -Mathf.Infinity);
                    Enemies.RemoveEnemyFromList(enemyAi);
                }
            }
        }
    }

    private void OnEnable()
    {
        foreach (Transform enemyParrent in transform)
        {
            foreach (Transform child in enemyParrent)
            {
                Enemies.AddEnemyToList(child.GetComponent<EnemyAI>());
            }
        }
    }

    public static void Sound(Transform soundOrigin, float sound, float timeSpan = 1f)
    {
        if (sound == 0f)
        {
            return;
        }
        foreach (EnemyAI enemyAi in allEnemyAis)
        {
            if (enemyAi.gameObject.activeInHierarchy)
            {
                float distanceValue = (enemyAi.transform.position - soundOrigin.position).sqrMagnitude + 0.0001f;
                float soundLevel = (sound * enemyAi.hearingAmplification) / distanceValue;
                enemyAi.AttendToSound(soundOrigin, soundLevel, timeSpan);
            }
        }
    }

    public static void Vision(Transform visionOrigin, float visibility, float timeSpan = 1f)
    {
        if (visibility == 0f)
        {
            return;
        }
        foreach (EnemyAI enemyAi in allEnemyAis)
        {
            if (enemyAi.gameObject.activeInHierarchy && enemyAi.eyesOpen)
            {
                Transform enemyTransform = enemyAi.transform;
                Vector3 vectorToOrigin = visionOrigin.position - enemyTransform.position;
                float dot = Vector3.Dot(enemyTransform.forward, vectorToOrigin.normalized);
                if (dot < enemyAi.fov - 1f)
                {
                    continue;
                }
                float distance = vectorToOrigin.sqrMagnitude;
                if (distance > enemyAi.visionDistance)
                {
                    continue;
                }
                if (!Physics.Linecast(enemyTransform.position, visionOrigin.position, ~Layer.Mask.playerAndEnemy))
                {
                    float visionLevel = ((visibility * dot * enemyAi.visionAmplification) / distance);
                    enemyAi.AttendToVision(visionOrigin, visibility, timeSpan);
                }
            }
        }
    }

    /*
    private void OnDrawGizmos()
    {
        foreach (EnemyAI enemyAi in allEnemyAis)
        {
            Gizmos.DrawLine(enemyAi.transform.position, GameObject.Find("Player").transform.position);
        }
    }
    */
}
