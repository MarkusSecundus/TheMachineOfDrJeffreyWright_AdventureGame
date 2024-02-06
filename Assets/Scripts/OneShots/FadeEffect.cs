using DG.Tweening;
using MarkusSecundus.PhysicsSwordfight.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    [SerializeField] float duration_seconds = 1f;
    [SerializeField] float alphaLow = 0f;
    [SerializeField] float alphaHigh = 1f;
    [SerializeField] bool fadeOutOnStart = false;
    private void Start()
    {
        if (fadeOutOnStart) FadeOut();
    }
    public void FadeOut() => RunEffect(alphaHigh, alphaLow);
    public void FadeOut(System.Action onFinished) => RunEffect(alphaHigh, alphaLow, onFinished);
    public void FadeIn() => RunEffect(alphaLow, alphaHigh);
    public void FadeIn(System.Action onFinished) => RunEffect(alphaLow, alphaHigh, onFinished);
    void RunEffect(float alphaBegin, float alphaEnd, System.Action onFinished=null)
    {
        gameObject.SetActive(true);
        Tween last = null;
        foreach(var rend in GetComponentsInChildren<Graphic>(true))
        {
            rend.gameObject.SetActive(true);
            rend.color = rend.color.With(a: alphaBegin);
            var tween = last = rend.DOFade(alphaEnd, duration_seconds);
            if (alphaEnd <= 0f) tween.OnComplete(() => rend.gameObject.SetActive(false));
        }
        if (last != null) last.OnComplete(() =>
        {
            onFinished?.Invoke();
        });
        else
            onFinished?.Invoke();
    }
}
