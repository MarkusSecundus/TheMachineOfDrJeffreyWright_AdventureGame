using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bloodstainable : MonoBehaviour
{
    [SerializeField] UnityEvent OnBloodstained;

    public void DoStainByBlood(DropOfBlood drop, Collision collision)
    {
        OnBloodstained?.Invoke();
    }
}
