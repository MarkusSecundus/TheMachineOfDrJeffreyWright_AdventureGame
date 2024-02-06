using Assets.Scripts.Dialog;
using MarkusSecundus.Utils.Datastructs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CodeEntryManager : MonoBehaviour
{
    [SerializeField] CodeEntryDigit[] Digits;
    [SerializeField] string GoalCode;
    [SerializeField] UnityEvent OnRightCodeEntered;

    string _totalText => string.Concat(Digits.Select(d => d.GetValue()));

    public void ReportChanged()
    {
        if(_totalText == GoalCode)
        {
            OnRightCodeEntered?.Invoke();
        }
    }
    public void Show()
    {
        UICamera.TurnOn(this);
        gameObject.SetActive(true);
    }

    public void Unshow()
    {
        UICamera.TurnOff(this);
        gameObject.SetActive(false);
    }
}
