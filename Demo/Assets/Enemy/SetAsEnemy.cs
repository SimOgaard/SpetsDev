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
                Destroy(colliders[i]);
            }
        }

        game_object.layer = Layer.enemy;
        game_object.isStatic = false;

        foreach (Transform child in game_object.transform)
        {
            RecursiveEnemy(child.gameObject);
        }
    }

    private void Start()
    {
        JoinMeshes join_meshes = gameObject.AddComponent<JoinMeshes>();
        join_meshes.SetMaterial(new Material(Shader.Find("Custom/Stone Shader")));
        join_meshes.SetMergeByTags(true);
        RecursiveEnemy(gameObject);
        Destroy(this);
    }
}
