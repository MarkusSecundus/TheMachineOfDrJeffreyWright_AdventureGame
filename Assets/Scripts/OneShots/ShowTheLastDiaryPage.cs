using MarkusSecundus.TinyDialog;
using MarkusSecundus.TinyDialog.Expressions;
using System.Collections;
using UnityEngine;

public class ShowTheLastDiaryPage : DialogCallback
{
    [SerializeField] Rigidbody Target;
    [SerializeField] Vector3 ShootForce;
    [SerializeField] Vector3 ShootTorque;
    [SerializeField] ForceMode ShootForceMode;
    [SerializeField] float ForceTime;
    [SerializeField] float WaitTime;

    public override IEnumerator InvokeCoroutine(ExpressionValue argument = default)
    {
        Target.gameObject.SetActive(true);
        var endTime = Time.time + ForceTime;
        while (endTime > Time.time)
        {
            Target.AddForce(ShootForce, ShootForceMode);
            Target.AddTorque(ShootTorque, ShootForceMode);
            yield return null;
        }
        yield return new WaitForSeconds(WaitTime);
    }

}

