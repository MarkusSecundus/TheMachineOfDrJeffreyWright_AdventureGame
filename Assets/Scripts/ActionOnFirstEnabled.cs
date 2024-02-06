using MarkusSecundus.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionOnFirstEnabled : MonoBehaviour
{
    [SerializeField] UnityEvent OnFirstEnable;


    bool wasEnabled = false;
    private void OnEnable()
    {
        Debug.Log($"Enabled {name}");
        if(!Op.post_assign(ref wasEnabled, true))
        {
            OnFirstEnable?.Invoke();
        }
    }
}
