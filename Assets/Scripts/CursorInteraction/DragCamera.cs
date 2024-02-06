using MarkusSecundus.PhysicsSwordfight.Utils.Primitives;
using UnityEngine;

public class DragCamera : MonoBehaviour
{
    [SerializeField] Camera toMove;
    [SerializeField] Transform minBoundary;
    [SerializeField] Transform maxBoundary;
    [SerializeField] Interval<float> zoomRange;

    [SerializeField] Vector3 speedX = new Vector3(10f, 0f, 0f);
    [SerializeField] Vector3 speedY = new Vector3(0f, 10f, 0f);
    [SerializeField] float zoomSpeed = 1f;


    float _screensizeEqualizer => Mathf.Max(Camera.main.pixelWidth, Camera.main.pixelHeight);

    Vector2 mousePos => Input.mousePosition / _screensizeEqualizer;

    public void Start()
    {
        Debug.Log($"Start DragCamera");
        _originalPosition = mousePos;
        _originalZoom = toMove.fieldOfView;
    }

    float _originalZoom;
    float currentZoom = 1f;
    Vector2 _originalPosition;

    float _realCurrentZoom = 1f;

    public void Update()
    {
        var newPos = mousePos;
        var delta = newPos - _originalPosition;
        _originalPosition = newPos;

        if((Input.GetKey(KeyCode.Mouse0) && !SelectableBase.SomethingIsBeingDragged) || Input.GetKey(KeyCode.Mouse2))
        {
            //Debug.Log($"Dragging the camera!");

            toMove.transform.position = (toMove.transform.position + delta.x * speedX + delta.y * speedY).ClampFields(Interval.Make(minBoundary.position, maxBoundary.position));
        }
        {
            currentZoom = (currentZoom + Input.mouseScrollDelta.y * zoomSpeed).Clamp(zoomRange);
            _realCurrentZoom = Mathf.Lerp(_realCurrentZoom, currentZoom, 0.2f);
            this.toMove.fieldOfView = _originalZoom * _realCurrentZoom;
        }
    }
}
