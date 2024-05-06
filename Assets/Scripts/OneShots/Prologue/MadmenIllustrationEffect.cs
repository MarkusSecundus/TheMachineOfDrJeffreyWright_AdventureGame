using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MadmenIllustrationEffect : IIllustrationEffect
{
    [SerializeField] SpriteRenderer[] madmen;
    SpriteRenderer lastMadman => madmen[^1];
    [SerializeField] float intervalBetweenMadmen_seconds = 0.2f;
    [SerializeField] float madmanFadeIn_seconds = 0.4f;
    [SerializeField] float madmanFadeOut_seconds = 0.4f;
    [SerializeField] float intervalBetweenFaces_seconds = 0.2f;
    [SerializeField] float faceFadeIn_seconds = 0.2f;
    [SerializeField] SpriteRenderer[] faces;


    bool _shouldUndisplay = false;
    public override void Undisplay()
    {
        base.Undisplay();
        _shouldUndisplay = true;
    }

    public override void Display()
    {
        base.Display();
        _shouldUndisplay = false;

        StartCoroutine(impl());
        IEnumerator impl()
        {
            Tween last = null;
            foreach (var madman in madmen)
            {
                if (_shouldUndisplay) yield break;
                madman.color = madman.color.With(a: 0f);
                madman.gameObject.SetActive(true);

                _runningTweens.Add(last = madman.DOFade(1f, madmanFadeIn_seconds));
                yield return new WaitForSeconds(intervalBetweenMadmen_seconds);
            }
            if(last.IsNotNil()) last.OnComplete(() =>
            {
                if (_shouldUndisplay) return;
                Tween last = null;
                foreach (var madman in madmen)
                {
                    if (_shouldUndisplay) return;

                    if (madman == lastMadman) continue;
                    _runningTweens.Add(last = madman.DOFade(0f, madmanFadeOut_seconds));
                }
                if (last.IsNotNil()) last.OnComplete(() =>
                {
                    if (_shouldUndisplay) return;
                    StartCoroutine(innerImpl());
                    IEnumerator innerImpl()
                    {
                        Tween last = null;
                        foreach (var face in faces)
                        {
                            if (_shouldUndisplay) yield break;
                            face.color = face.color.With(a: 0f);
                            face.gameObject.SetActive(true);
                            _runningTweens.Add(last = face.DOFade(1f, faceFadeIn_seconds));
                            yield return new WaitForSeconds(intervalBetweenFaces_seconds);
                        }
                        last.OnComplete(() => this.IsFullyDisplayed = true);
                    }
                });
            });
        }
    }
}
