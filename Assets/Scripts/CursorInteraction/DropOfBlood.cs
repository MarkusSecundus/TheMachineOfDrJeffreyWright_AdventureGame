using DG.Tweening;
using MarkusSecundus.PhysicsSwordfight.Utils.Extensions;
using MarkusSecundus.PhysicsSwordfight.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DropOfBlood : MonoBehaviour
{
    [SerializeField] GameObject[] BloodstainPrefabs;
    [SerializeField] float BloodstainFadeInSeconds = 0.8f;
    const string AltarBowlTag = "AltarBowl";


    private void OnCollisionEnter(Collision collision)
    {
        var bloodstainable = collision.collider.GetComponent<Bloodstainable>();
        if (bloodstainable)
        {
            bloodstainable.DoStainByBlood(this, collision);
        }
        NormalCollissionBehavior(collision);
        Destroy(gameObject);
    }

    void NormalCollissionBehavior(Collision collision)
    {
        var bloodstainPrefab = BloodstainPrefabs[Random.Range(0, BloodstainPrefabs.Length)];
        var bloodstain = Instantiate(bloodstainPrefab);
        bloodstain.transform.SetParent(RoomTransitionManager.Instance.CurrentRoot);
        var point = collision.GetContact(0);
        bloodstain.transform.position = point.point;
        bloodstain.transform.rotation = Quaternion.LookRotation(point.normal);
        foreach(var spr in bloodstain.GetComponentsInChildren<SpriteRenderer>())
        {
            spr.color = spr.color.With(a: 0f);
            spr.DOFade(1f, BloodstainFadeInSeconds);
        }
        var originalScale = bloodstain.transform.localScale;
        bloodstain.transform.localScale *= 0.001f;
        bloodstain.transform.DOScale(originalScale, BloodstainFadeInSeconds);
    }
}
