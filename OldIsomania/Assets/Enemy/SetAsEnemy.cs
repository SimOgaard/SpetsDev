using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAsEnemy : MonoBehaviour
{
    private void RecursiveEnemy(GameObject gameObject)
    {
        if (Layer.IsInLayer(Layer.spawnedGameWorldHigherPriority, gameObject.layer))
        {
            Collider[] colliders = gameObject.GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].isTrigger)
                {
                    Destroy(colliders[i]);
                }
                else
                {
                    colliders[i].enabled = true;
                }
            }
        }

        gameObject.layer = Layer.enemy;

        foreach (Transform child in gameObject.transform)
        {
            RecursiveEnemy(child.gameObject);
        }
    }

    public void Init()
    {
        /*
        JoinMeshes joinMeshes = gameObject.AddComponent<JoinMeshes>();
        joinMeshes.SetCollider();
        joinMeshes.SetMergeByTags(true);
        */
        SoundCollider soundCollider = gameObject.AddComponent<SoundCollider>();
        soundCollider.soundAmplifier = 7.5f;
        soundCollider.minSound = 50f;
        soundCollider.AddRigidbody(GetComponent<Agent>()._rigidbody);

        Enemies.AddEnemyToList(GetComponent<EnemyAI>());

        RecursiveEnemy(gameObject);
        Destroy(this);
    }
}
