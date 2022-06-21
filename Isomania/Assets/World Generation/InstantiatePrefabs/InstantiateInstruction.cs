using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WorldGeneration/Instanciate/InstanciateInstructions", order = 1)]
[System.Serializable]
public class InstantiateInstruction : ScriptableObject
{
    public enum SharedXYZ { none, xy, xz, yz, xyz }
    public enum PlacableGameObjectsParrent { keep, groundMesh, landMarks, rocks, trees, chests, enemies, lgolems, mgolems, sgolems }
    public static readonly string[] PlacableGameObjectsParrentString = { "keep", "groundMesh", "landMarks", "rocks", "trees", "chests", "Enemies", "Enemies/lGolems", "Enemies/mGolems", "Enemies/sGolems" };

    [Header("Hiearchy")]
    public PlacableGameObjectsParrent parrentName = PlacableGameObjectsParrent.landMarks;
    public int minChildAmountRequired = 0;

    [Header("Spawning Bounds")]
    public bool ignoreBoundingBoxes = false;

    [Header("Spawning Chance")]
    [Range(0f, 1f)] public float spawnChance = 1f;
    public Vector2 spawnRangeNoise;
    [Range(0f, 1f)] public float spawnChanceNoise = 1f;
    public NoiseLayerSettings.NoiseLayer noiseLayer;

    [Header("Spawning Ray")]
    public LayerMask rayLayerMask = (1 << 16) | (1 << 20);
    public SharedXYZ sharedRayPosition = SharedXYZ.none;
    public Vector3 minRayPosition = new Vector3(0, 0, 0);
    public Vector3 maxRayPosition = new Vector3(0, 0, 0);

    [Header("Global Spawning Location")]
    public SharedXYZ globalSharedPosition = SharedXYZ.none;
    public Vector3 minPosition = new Vector3(0, 0, 0);
    public Vector3 maxPosition = new Vector3(0, 0, 0);

    [Header("Local Spawning Location")]
    public SharedXYZ localSharedPosition = SharedXYZ.none;
    public Vector3 minPositionLocal = new Vector3(0, 0, 0);
    public Vector3 maxPositionLocal = new Vector3(0, 0, 0);

    [Header("Spawning Rotation")]
    public bool rotateTwordsGroundNormal = false;
    public bool resetRotation = true;
    [Range(0f, 359f)] public float rotationOffset = 0f;
    [Range(0f, 359f)] public float rotationIncrement = 0f;
    [Range(0f, 90f)] public float maxRotation = 90f;

    [Header("Spawning Scale")]
    public SharedXYZ sharedScales = SharedXYZ.none;
    public Vector3 minScale = new Vector3(1, 1, 1);
    public Vector3 maxScale = new Vector3(1, 1, 1);

    [Header("Density")]
    public Density.DensityValues density = Density.DensityValues.ignore;

    public static string GetHierarchyName(PlacableGameObjectsParrent index)
    {
        return PlacableGameObjectsParrentString[(int)index];
    }
}
