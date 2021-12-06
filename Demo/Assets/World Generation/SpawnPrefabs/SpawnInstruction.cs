using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SpawnInstruction : ScriptableObject
{
    public enum SharedXYZ { none, xy, xz, yz, xyz }
    public enum PlacableGameObjectsParrent { keep, ground_mesh, land_marks, rocks, trees, chests, enemies, lgolems, mgolems, sgolems }
    public static readonly string[] PlacableGameObjectsParrentString = { "keep", "ground_mesh", "land_marks", "rocks", "trees", "chests", "Enemies", "Enemies/lGolems", "Enemies/mGolems", "Enemies/sGolems" };

    [Header("Hiearchy")]
    public PlacableGameObjectsParrent parrent_name = PlacableGameObjectsParrent.land_marks;
    public int min_child_amount_required = 0;

    [Header("Spawning Bounds")]
    public bool ignore_bounding_boxes = false;

    [Header("Spawning Chance")]
    [Range(0f, 1f)] public float spawn_chance = 1f;
    public Vector2 spawn_range_noise;
    [Range(0f, 1f)] public float spawn_chance_noise = 1f;
    public NoiseLayerSettings.NoiseLayer noise_layer;

    [Header("Spawning Ray")]
    public LayerMask ray_layer_mask = (1 << 16) | (1 << 20);
    public SharedXYZ shared_ray_position = SharedXYZ.none;
    public Vector3 min_ray_position = new Vector3(0, 0, 0);
    public Vector3 max_ray_position = new Vector3(0, 0, 0);

    [Header("Global Spawning Location")]
    public SharedXYZ global_shared_position = SharedXYZ.none;
    public Vector3 min_position = new Vector3(0, 0, 0);
    public Vector3 max_position = new Vector3(0, 0, 0);

    [Header("Local Spawning Location")]
    public SharedXYZ local_shared_position = SharedXYZ.none;
    public Vector3 min_position_local = new Vector3(0, 0, 0);
    public Vector3 max_position_local = new Vector3(0, 0, 0);

    [Header("Spawning Rotation")]
    public bool rotate_twords_ground_normal = false;
    public bool reset_rotation = true;
    [Range(0f, 359f)] public float rotation_offset = 0f;
    [Range(0f, 359f)] public float rotation_increment = 0f;
    [Range(0f, 90f)] public float max_rotation = 90f;

    [Header("Spawning Scale")]
    public SharedXYZ shared_scales = SharedXYZ.none;
    public Vector3 min_scale = new Vector3(1, 1, 1);
    public Vector3 max_scale = new Vector3(1, 1, 1);

    [Header("Density")]
    public Density.DensityValues density = Density.DensityValues.ignore;

    public static string GetHierarchyName(PlacableGameObjectsParrent index)
    {
        return PlacableGameObjectsParrentString[(int)index];
    }
}
