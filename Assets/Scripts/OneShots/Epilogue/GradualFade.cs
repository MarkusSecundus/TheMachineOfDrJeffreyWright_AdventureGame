using MarkusSecundus.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradualFade : IIllustrationEffect
{
    [SerializeField] float alphaIncrease = 1f;
    [SerializeField] float scaleIncrease = 1f;

    public override void Display()
    {
        StartCoroutine(impl());
        IEnumerator impl()
        {
            foreach (var spr in _sprites)
            {
                spr.color = spr.color.With(a: 0.005f);
                spr.gameObject.SetActive(true);
            }
                while (true)
            {
                foreach (var spr in _sprites)
                {
                    spr.transform.localScale *= scaleIncrease;
                    spr.color = spr.color.With(a: Mathf.Clamp(spr.color.a * alphaIncrease, 0f, 1f));
                }
                yield return null;
            }
        }
    }

}
