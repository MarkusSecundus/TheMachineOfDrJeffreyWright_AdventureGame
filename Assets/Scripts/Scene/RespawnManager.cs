using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Datastructs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] List<GameObject> ToRespawn;


    List<(GameObject Instance, GameObject Supplement)> _respawnableInstances;

    private void Awake()
    {
        foreach (var template in ToRespawn) template.SetActive(false);
    }
    private void OnEnable()
    {
        doCleanup();
        doRespawn();
    }

    private void OnDisable()
    {
        doCleanup();
    }

    void doCleanup()
    {
        if (_respawnableInstances.IsNullOrEmpty()) return;
        foreach(var (instance, supplement) in _respawnableInstances)
        {
            if (!instance && !supplement)
                Debug.LogWarning($"Despawnable - For some element, both instance and supplement are invalid!");
            Destroy(instance? instance : supplement);
        }
        _respawnableInstances = null;
    }
    void doRespawn()
    {
        _respawnableInstances = ToRespawn.Select(template =>
        {
            var instance = template.InstantiateWithTransform();
            instance.SetActive(true);
            var supplement = instance.FindChildWithTag("DespawnSupplement");
            return (instance, supplement);
        }).ToList(); 
    }

    public void Unregister(GameObject template)
    {
        
        var i = ToRespawn.FindIndex(t => t&& t == template);
        if (i < 0)
        {
            Debug.LogWarning($"Unregistering object {template.name} which is not registered as respawnable in {name}");
            return;
        }
        ToRespawn.RemoveAt(i);
    }
}
