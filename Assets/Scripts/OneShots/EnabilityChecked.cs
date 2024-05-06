using MarkusSecundus.Utils.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnabilityChecked : MonoBehaviour
{
    [SerializeField] UnityEvent EventOnAwake;
    [SerializeField] UnityEvent EventOnEnable;
    [SerializeField] UnityEvent EventOnDisable;

    protected virtual void Awake() { if(EventOnAwake.IsNotNil()) EventOnAwake.Invoke(); }
    protected virtual void OnEnable() { if (EventOnEnable.IsNotNil()) EventOnEnable.Invoke(); }
    protected virtual void OnDisable() { if (EventOnDisable.IsNotNil()) EventOnDisable.Invoke(); }
}
