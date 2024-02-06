using MarkusSecundus.PhysicsSwordfight.Utils.Primitives;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CycleBetweenImagesHectically : IIllustrationEffect
{
    [SerializeField] SpriteRenderer[] Sprites;
    [SerializeField] float SpriteFadeIn;
    [SerializeField] float SpriteFadeBetween;
    [SerializeField] float SpriteFadeOut;
    [SerializeField] float BetweenSprites;
    [SerializeField] float MaxAlpha;


    public override void Display()
    {
        base.Display();

        StartCoroutine(impl());
        IEnumerator impl()
        {
            int i = 0;
            while (true)
            {
                var r = Sprites[i];
                i = (i + 1).Mod(Sprites.Length);
                r.color = r.color.With(a: 0f);
                r.gameObject.SetActive(true);

                _runningTweens.Add(r.DOFade(MaxAlpha, SpriteFadeIn).OnComplete(() =>
                {
                    _runningTweens.Add(r.DOFade(0f, SpriteFadeOut).SetDelay(SpriteFadeBetween)/*.OnComplete(()=>r.gameObject.SetActive(false))*/);
                }));

                yield return new WaitForSeconds(BetweenSprites);
            }
        }
    }
}
