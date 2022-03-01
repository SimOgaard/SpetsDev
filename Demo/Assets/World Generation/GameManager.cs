using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        Global.Materials.spriteRendererMaterial = Resources.Load<Material>("Sprites/Sprte Billboard Material");

        Global.interactingWithSprite = Resources.Load<Sprite>("Interactables/interactingWithSprite");
        Global.notInteractingWithSprite = Resources.Load<Sprite>("Interactables/notInteractingWithSprite");

        Global.cameraFocusPointTransform = GameObject.Find("cameraFocusPoint").transform;
        Global.playerTransform = GameObject.Find("Player").transform;
        Global.equipmentsInInventory = GameObject.Find("EquipmentsInInventory").transform;
    }
}
