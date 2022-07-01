using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Snaps child transform to global grid, so that each vertice snaps when the parrent vertice snaps
/// </summary>
public class SnapChildToGrid : MonoBehaviour
{
    private void Awake()
    {
        // grab global scale
        Vector3 lossyScale = transform.lossyScale;

        // transform local transformation to global values
        Vector3 globalLocalTransformation = Vector3.Scale(lossyScale, transform.localPosition);

        // snap that value to grid
        Vector3 roundedGlobalLocalTransformation = PixelPerfect.RoundToPixel(globalLocalTransformation);

        // transform back to local
        Vector3 roundedLocalTransformation = Vector3.Scale(DayNight.DivideVector3(Vector3.one, lossyScale), roundedGlobalLocalTransformation);

        // and apply
        transform.localPosition = roundedLocalTransformation;
    }
}
