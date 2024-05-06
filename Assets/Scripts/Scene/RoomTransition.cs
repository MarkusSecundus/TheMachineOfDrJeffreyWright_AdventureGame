using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.TinyDialog;
using MarkusSecundus.TinyDialog.Expressions;
using UnityEngine;
using UnityEngine.Events;



public class RoomTransition : AtomicDialogCallback
{
    [SerializeField] Transform RoomRoot;
    [SerializeField] UnityEvent OnTransition;

    RoomTransitionManager _manager;
    protected void Start()
    {
        _manager = GameObject.FindAnyObjectByType<RoomTransitionManager>();
    }

    public override void Invoke(ExpressionValue _)
    {
        this.InvokeWithDelay(() =>
        {
            var transitionTarget = RoomRoot ? RoomRoot : _manager.LastRoot;
            _manager.DoTransition(transitionTarget, ()=>OnTransition?.Invoke());
        }, null);
    }
}
