using MarkusSecundus.TinyDialog;
using MarkusSecundus.TinyDialog.Expressions;
using System.Collections;
using UnityEngine;

public class WaitCoroutine : DialogCallback
{
    [SerializeField] float waitSeconds = 1f;

    public override IEnumerator InvokeCoroutine(ExpressionValue argument = default)
    {
        Debug.Log("Starting routine");
        yield return new WaitForSeconds(waitSeconds);
        Debug.Log($"Ending routing after {waitSeconds} s");
    }
}
