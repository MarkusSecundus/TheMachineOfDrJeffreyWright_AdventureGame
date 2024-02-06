using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorIcon : MonoBehaviour
{
    [SerializeField] GameObject NormalSprite;
    [SerializeField] GameObject BloodstainsSprite;

    public void SetBloodstained(bool visible)
    {
        NormalSprite.SetActive(!visible);
        BloodstainsSprite.SetActive(visible);
    }
    public bool IsBloodstained => BloodstainsSprite.activeSelf;
}
