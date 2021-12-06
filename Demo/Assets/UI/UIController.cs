using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    /// <summary>
    /// Hides the ui
    /// </summary>
    private void Start()
    {
        gameObject.SetActive(false);
    }
}
