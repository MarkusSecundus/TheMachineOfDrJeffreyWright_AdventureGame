using Assets.Scripts.DialogSystem.Actions;
using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.TinyDialog.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BlinkImage : ActionDialogCallback
{
    [SerializeField] protected float fadeInSeconds = 0.3f;
    [SerializeField] protected float fadeOutSeconds = 5f;
    protected SpriteRenderer _sprite;

    protected virtual void Awake()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _sprite.gameObject.SetActive(false);
    }
    public sealed override void Invoke(ExpressionValue _)
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
        var originalAlph = _sprite.color.a;
        _sprite.color = _sprite.color.With(a: 0f);
        _sprite.gameObject.SetActive(true);
        _sprite.DOFade(originalAlph, fadeInSeconds).OnComplete(() => _sprite.DOFade(0f, fadeOutSeconds).OnComplete(()=>_sprite.gameObject.SetActive(false)));
    }
}
