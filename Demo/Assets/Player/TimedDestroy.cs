using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    public void StartDestruction(float time)
    {
        StartCoroutine("DestroyTimed", time);
    }

    private IEnumerator DestroyTimed(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
