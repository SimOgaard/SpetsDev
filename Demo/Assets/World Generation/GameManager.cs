using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        Global.Materials.sprite_renderer_material = Resources.Load<Material>("Sprites/Sprte Billboard Material");

        Global.interacting_with_sprite = Resources.Load<Sprite>("Interactables/interacting_with_sprite");
        Global.not_interacting_with_sprite = Resources.Load<Sprite>("Interactables/not_interacting_with_sprite");

        Global.camera_focus_point_transform = GameObject.Find("camera_focus_point").transform;
        Global.player_transform = GameObject.Find("Player").transform;
        Global.equipments_in_inventory = GameObject.Find("EquipmentsInInventory").transform;
    }
}
