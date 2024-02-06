using MarkusSecundus.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionOnStart : MonoBehaviour
{
    [SerializeField] UnityEvent OnStart;

    private void Start()
    {
        OnStart?.Invoke();
    }
}
