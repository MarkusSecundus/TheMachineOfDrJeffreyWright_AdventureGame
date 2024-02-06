using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IllustrationEffectsManager : MonoBehaviour
{
    public IIllustrationEffect ActiveEffect { get; private set; }

    public void SetActiveEffect(IIllustrationEffect effect)
    {
        ActiveEffect?.Undisplay();
        ActiveEffect = effect;
    }
}
