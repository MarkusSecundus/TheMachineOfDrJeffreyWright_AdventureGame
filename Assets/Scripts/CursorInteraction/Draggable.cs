using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Datastructs;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Draggable : SelectableBase
{

    [SerializeField] float DragForce = 800f;
    //[SerializeField] float DragHeight = 30f;

    [SerializeField] Vector3 DragAngularVelocity = Vector3.zero;
    [SerializeField] float DragAngularForce = 0f;

    [SerializeField] UnityEvent OnTouched;
    [SerializeField] UnityEvent OnPickedUp;

    internal static bool CursorIsBeingHoveredOverSomething => _lastHoverTimestamp + 1 >= Time.frameCount;
    static int _lastHoverTimestamp = -2;
    private void OnMouseOver()
    {
        _lastHoverTimestamp = Time.frameCount;
    }


    Rigidbody rb;
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        //Debug.Log($"Starting draggable - {rb}", this);
    }


    int lastDraggedFrame = -1;
    protected override void OnMouseDrag_impl()
    {
        base.OnMouseDrag_impl();
        lastDraggedFrame = Time.frameCount;
    }
    private void FixedUpdate()
    {
        if(Time.frameCount <= lastDraggedFrame + 1)
        {
            BeginDrag();
            //Debug.Log($"Dragging!", this);
            //if(MouseRaycast.DoMouseRaycastWithHeight(DragHeight, out var point, out var ray, out var originalInfo, out var groundInfo))
            if(MouseRaycast.Instance.State.IsCurrent)
            {
                MoveToPosition(MouseRaycast.Instance.State.InterpolatedPoint);
                MoveToRotation();
            }
            else
            {
                Debug.LogError($"No raycast hit!", this);
            }
        }
        else
        {
            EndDrag();
        }
    }

    void MoveToPosition(Vector3 point)
    {
        var direction = (point - rb.position).Normalized(out var directionMagnitude);
        var velocityDirection = direction - rb.linearVelocity.normalized;
        rb.AddForce(velocityDirection * directionMagnitude * DragForce, ForceMode.Acceleration);
    }
    void MoveToRotation()
    {
        if (DragAngularForce <= 0f) return;
        //Debug.Log($"angular velocity: {rb.angularVelocity}");
        var angularDirection = (DragAngularVelocity - rb.angularVelocity);
        rb.AddTorque(angularDirection * DragAngularForce, ForceMode.Acceleration);
    }

    public static new bool SomethingIsBeingDragged { get; private set; }
    bool _isBeingDragged = false;
    void BeginDrag()
    {
        if(!_isBeingDragged)
            OnTouched?.Invoke();
        SomethingIsBeingDragged = true;
        _isBeingDragged = true;
        rb.useGravity = false;
    }
    void EndDrag()
    {
        if(_isBeingDragged) SomethingIsBeingDragged = false;
        _isBeingDragged = false;
        rb.useGravity = true;
    }

    protected override void OnDisable()
    {
        if (_isBeingDragged) SomethingIsBeingDragged = false;
        _isBeingDragged = false;
    }
    private void OnDestroy() => OnDisable();


    const string PickUpTriggerTag = "PickUpTrigger";
    private void OnTriggerEnter(Collider other)
    {
        if (_isBeingDragged && other.CompareTag(PickUpTriggerTag))
        {
            Debug.Log($"Picked up {name}");
            OnPickedUp?.Invoke();
        }
    }
}
