using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.TinyDialog;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class IIllustrationEffect : AtomicDialogCallback
{
    protected IllustrationEffectsManager _manager => GetComponentInParent<IllustrationEffectsManager>();
    IEnumerable<SpriteRenderer> _sprites_fld;
    [SerializeField] protected float fadeOutSeconds = 1f;
    protected IEnumerable<SpriteRenderer> _sprites => _sprites_fld ??= GetComponentsInChildren<SpriteRenderer>(true);

    protected List<Tween> _runningTweens = new();


    protected virtual void Awake()
    {
        foreach (var r in GetComponentsInChildren<SpriteRenderer>()) r.gameObject.SetActive(false);
    }
    public sealed override void Invoke(MarkusSecundus.TinyDialog.Expressions.ExpressionValue _)
    {
        gameObject.SetActive(true);
        StartCoroutine(impl());
        IEnumerator impl()
        {
            yield return null;
            Display();
        }
    }
    public virtual void Display()
    {
        gameObject.SetActive(true);
        var manager = _manager;
        manager.SetActiveEffect(this);
    }


    public virtual void Undisplay()
    {
        StopAllCoroutines();
        foreach (var tw in _runningTweens) if (tw.active) tw.Kill();
        _runningTweens.Clear();

        IsFullyDisplayed = false;
        foreach (var r in _sprites)
        {
            _runningTweens.Add(r.DOFade(0f, fadeOutSeconds));
        }
    }

    public bool IsFullyDisplayed { get; protected set; }


}

public class BasicIllustrationEffect : IIllustrationEffect
{
    [SerializeField] float fadeInSeconds = 1f;

    public override void Display()
    {
        base.Display();
        Debug.Log($"{name}: {_sprites.Count()} sprites", this);
        Tween last = null;
        foreach(var r in _sprites)
        {
            r.color = r.color.With(a: 0f);
            r.gameObject.SetActive(true);
            Debug.Log($"Using sprite {r}", this);
            _runningTweens.Add(last = r.DOFade(1f, fadeInSeconds));
        }
        if (last.IsNotNil()) last.OnComplete(() => IsFullyDisplayed = true);
    }

}
