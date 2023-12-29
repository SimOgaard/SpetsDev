using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropTrigger : MonoBehaviour
{
    private bool triggerEnabled = false;
    private WaitForSeconds TriggerWaitSeconds = new WaitForSeconds(0.1f);
    private IEnumerator TriggerWait()
    {
        yield return TriggerWaitSeconds;
        triggerEnabled = true;
    }

    private void Awake()
    {
        StartCoroutine(TriggerWait());
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (triggerEnabled && !collision.isTrigger && Layer.IsInLayer(Layer.Mask.staticGround, collision.gameObject.layer))
        {
            triggerEnabled = false;
            transform.GetChild(0).SendMessage("OnGround");
        }
    }
}
