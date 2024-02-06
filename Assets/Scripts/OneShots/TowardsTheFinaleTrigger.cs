using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TowardsTheFinaleTrigger : MonoBehaviour
{
    [SerializeField] UnityEvent BeginFinaleEvent;
    [SerializeField] GameObject[] Triggers;
    [SerializeField] int MinimumTriggersActive;

    private void OnEnable()
    {
        var triggersActive = Triggers.Count(t => t.gameObject.activeSelf);
        Debug.Log($"Checking for finale trigger - active {triggersActive} out of {Triggers.Length} (required at least {MinimumTriggersActive})", this);
        if (triggersActive >= MinimumTriggersActive)
        {
            Debug.Log($"Activating finale trigger", this);
            BeginFinaleEvent?.Invoke();
            Destroy(this);
        }
    }
}
