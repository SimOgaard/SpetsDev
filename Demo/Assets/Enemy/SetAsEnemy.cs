using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAsEnemy : MonoBehaviour
{
    private void RecursiveEnemy(GameObject game_object)
    {
        if (Layer.IsInLayer(Layer.spawned_game_world_higher_priority, game_object.layer))
        {
            Collider[] colliders = game_object.GetComponents<Collider>();
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

        game_object.layer = Layer.enemy;

        foreach (Transform child in game_object.transform)
        {
            RecursiveEnemy(child.gameObject);
        }
    }

    public void Init()
    {
        JoinMeshes join_meshes = gameObject.AddComponent<JoinMeshes>();
        join_meshes.SetMergeByTags(true);

        Rigidbody enemy_rigidbody = gameObject.AddComponent<Rigidbody>();
        GetComponent<Agent>().AddRigidBody(enemy_rigidbody);

        Enemies.AddEnemyToList(GetComponent<EnemyAI>());

        RecursiveEnemy(gameObject);
        Destroy(this);
    }
}
