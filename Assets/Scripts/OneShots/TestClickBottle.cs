using MarkusSecundus.Utils.Datastructs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TestClickBottle : SelectableBase
{
    protected override void OnMouseUpAsButton_impl() => HandleClick();
    public void HandleClick()
    {
        Debug.Log($"Clicked the bottle {name}");
    }
}
