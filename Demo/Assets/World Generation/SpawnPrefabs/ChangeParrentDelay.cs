using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeParrentDelay : MonoBehaviour
{
    private Transform soon_to_be_new_parrent;
    public void SetParrent(Transform parrent)
    {
        soon_to_be_new_parrent = parrent;
    }

    private void Start()
    {
        transform.parent = soon_to_be_new_parrent;
        Destroy(this);
    }
}
