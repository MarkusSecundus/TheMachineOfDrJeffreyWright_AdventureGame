using Assets.Scripts.Dialog;
using DG.Tweening;
using MarkusSecundus.PhysicsSwordfight.Utils.Extensions;
using MarkusSecundus.PhysicsSwordfight.Utils.Primitives;
using Unity.VisualScripting;
using UnityEngine;



public class RoomTransitionManager : MonoBehaviour
{
    [SerializeField] float fadeBetween_seconds = 1f;

    [SerializeField] FadeEffect fadeEffect;

    [SerializeField] public Transform CurrentRoot;

    [DoNotSerialize]public Transform LastRoot = null;

    public static RoomTransitionManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        var beginRoom = CurrentRoot;
        CurrentRoot = null;
        DoTransition(beginRoom);
    }


    private Transform _transitionIsInProgress = null;
    public void DoTransition(Transform newRoot, System.Action onTransitionCallback =null)
    {
        if (_transitionIsInProgress)
        {
            Debug.LogError($"Cannot start transition to {newRoot.name} because transition to {_transitionIsInProgress.name} is already in progress!", this);
            return;
        }
        if (!newRoot) return;
        if (!CurrentRoot)
        {
            performTransition();
            return;
        }
        if (newRoot == CurrentRoot)
        {
            Debug.LogError($"Attempting to transition to the same room that already is active ({newRoot.name})", this);
            return;
        }
        _transitionIsInProgress = newRoot;
        fadeEffect.FadeIn(() =>
        {
            performTransition();
            this.PerformWithDelay(() => fadeEffect.FadeOut(()=>_transitionIsInProgress=null), fadeBetween_seconds);
        });

        void performTransition()
        {
            LastRoot = CurrentRoot;
            if (CurrentRoot) CurrentRoot.gameObject.SetActive(false);
            CurrentRoot = newRoot;
            CurrentRoot.gameObject.SetActive(true);
            onTransitionCallback?.Invoke();
        }
    }
}