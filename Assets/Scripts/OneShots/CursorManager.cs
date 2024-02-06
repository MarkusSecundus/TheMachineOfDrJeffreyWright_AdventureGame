using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] CursorIcon CursorNormalPrefab;
    [SerializeField] CursorIcon CursorHandPrefab;
    [SerializeField] GameObject BloodDropPrefab;
    [SerializeField] float SecondsBetweenBloodDrops = 1f;


    public static CursorManager Instance { get; private set; }

    public bool IsHand => Draggable.SomethingIsBeingDragged;//|| HandCursor.HandCursorRequesters.Count > 0;


    CursorIcon currentCursor => IsHand ? cursorHand : cursorNormal;
    CursorIcon inactiveCursor => IsHand ? cursorNormal: cursorHand;

    CursorIcon cursorNormal, cursorHand;


    public bool IsBloodstained => Time.time < _bloodstainExpirationTime;
    float _bloodstainExpirationTime = float.NegativeInfinity;
    public void ExtendBloodstainDuration(float seconds)
    {
        var begin = IsBloodstained ? _bloodstainExpirationTime : Time.time;
        _bloodstainExpirationTime = begin + seconds;
    }

    private void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);

        Instance = this;
        Object.DontDestroyOnLoad(gameObject);

        //Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        Cursor.visible = false;

        cursorNormal = Instantiate(CursorNormalPrefab);
        cursorHand = Instantiate(CursorHandPrefab);
        cursorNormal.transform.SetParent(this.transform);
        cursorHand.transform.SetParent(this.transform);
        cursorNormal.gameObject.SetActive(false);
        cursorHand.gameObject.SetActive(false);
    }
    private void Start()
    {
        Cursor.visible = false;
        StartCoroutine(droppingBlood());
    }

    IEnumerator droppingBlood()
    {
        while (true)
        {
            if (IsBloodstained)
            {
                var drop = Instantiate(BloodDropPrefab);
                drop.transform.position = MouseRaycast.Instance.State.InterpolatedPoint;
            }
            yield return new WaitForSeconds(SecondsBetweenBloodDrops);
        }
    }

    void Update()
    {
        Cursor.visible = false;
        currentCursor.gameObject.SetActive(true);
        inactiveCursor.gameObject.SetActive(false);
        currentCursor.SetBloodstained(IsBloodstained);

        var pos = (Vector2)Input.mousePosition;// - Camera.main.pixelRect.size / 2;
        currentCursor.transform.position = pos;
    }
    private void LateUpdate()
    {
        Cursor.visible = false;
    }
}
