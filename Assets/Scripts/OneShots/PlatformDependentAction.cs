using MarkusSecundus.Utils.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlatformDependentAction : MonoBehaviour
{
    [SerializeField] bool _runOnStartup = false;
    [SerializeField] SerializableDictionary<RuntimePlatform[], UnityEvent> _actionByPlatforms;

    void Start()
    {
        if (!_runOnStartup) return;
        foreach(var (platforms, action) in _actionByPlatforms.Values)
        {
            if (Array.IndexOf(platforms, Application.platform) >= 0)
                action?.Invoke();
        }
    }

}
