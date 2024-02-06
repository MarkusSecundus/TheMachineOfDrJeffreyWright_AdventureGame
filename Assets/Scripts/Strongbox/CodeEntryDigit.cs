using Assets.Scripts.Dialog;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MarkusSecundus.PhysicsSwordfight.Utils.Primitives;

public class CodeEntryDigit : MonoBehaviour
{
    [SerializeField] public string[] Variants = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
    [SerializeField] int DefaultIndex = 0;

    
    int _currentIndex { get => DefaultIndex; set => DefaultIndex = value; }
    TextMeshPro _text;
    CodeEntryManager _manager;
    private void Awake()
    {
        _manager = GetComponentInParent<CodeEntryManager>();
        _text = GetComponentInChildren<TextMeshPro>();
        Increment(0);
    }

    public void Increment(int shift)
    {
        _currentIndex = (_currentIndex + shift).Mod(Variants.Length);
        _text.text = Variants[_currentIndex];
        _manager.ReportChanged();
    }

    public string GetValue() => Variants[_currentIndex];

}
