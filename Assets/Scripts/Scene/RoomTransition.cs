using MarkusSecundus.PhysicsSwordfight.Utils.Extensions;
using MarkusSecundus.TinyDialog;
using MarkusSecundus.TinyDialog.Expressions;
using UnityEngine;



public class RoomTransition : AtomicDialogCallback
{
    [SerializeField] Transform RoomRoot;

    RoomTransitionManager _manager;
    protected void Start()
    {
        _manager = GameObject.FindAnyObjectByType<RoomTransitionManager>();
    }

    public override void Invoke(ExpressionValue _)
    {
        this.PerformWithDelay(() =>
        {
            var transitionTarget = RoomRoot ? RoomRoot : _manager.LastRoot;
            _manager.DoTransition(transitionTarget);
        }, null);
    }
}
