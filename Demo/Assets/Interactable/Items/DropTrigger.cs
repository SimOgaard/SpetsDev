using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropTrigger : MonoBehaviour
{
    private bool trigger_enabled = false;
    private WaitForSeconds TriggerWaitSeconds = new WaitForSeconds(0.1f);
    private IEnumerator TriggerWait()
    {
        yield return TriggerWaitSeconds;
        trigger_enabled = true;
    }

    private void Awake()
    {
        StartCoroutine(TriggerWait());
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (trigger_enabled && !collision.isTrigger && Layer.IsInLayer(Layer.Mask.static_ground, collision.gameObject.layer))
        {
            trigger_enabled = false;
            transform.GetChild(0).SendMessage("OnGround");
        }
    }
}
