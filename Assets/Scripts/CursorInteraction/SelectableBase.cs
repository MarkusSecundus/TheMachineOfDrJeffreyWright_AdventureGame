using MarkusSecundus.Utils.Datastructs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SelectableBase : MonoBehaviour
{

    const string OutlinePropertyName = "_OutlineThickness";

    [SerializeField] public UnityEvent OnClicked;


    [SerializeField] float outlineWidthMultiplier = 3f;


    internal static volatile bool InputEnabled = true;


    protected virtual void OnDisable() => OnMouseExit_impl();
    private void OnMouseEnter() { if (InputEnabled) OnMouseEnter_impl(); }
    private void OnMouseExit() { OnMouseExit_impl(); }
    private void OnMouseUpAsButton() { if (InputEnabled) OnMouseUpAsButton_impl(); }
    private void OnMouseDrag() { if (InputEnabled) OnMouseDrag_impl(); }

    protected virtual void OnMouseEnter_impl() => IncreaseBottleOutline();
    protected virtual void OnMouseExit_impl() => ReturnBottleOutline();
    protected virtual void OnMouseUpAsButton_impl() { OnClicked?.Invoke(); }
    public static bool SomethingIsBeingDragged => timestampForFrameWhereSomethingWasBeingDragged + 1 >= Time.frameCount;

    private static int timestampForFrameWhereSomethingWasBeingDragged = -1;
    protected virtual void OnMouseDrag_impl() { timestampForFrameWhereSomethingWasBeingDragged = Time.frameCount; }


    [SerializeField] SpriteRenderer _outlineSprite;
    Material[] _outlineMaterials;
    float _originalOutlineWidth;
    protected virtual void Start()
    {
        //Debug.Log($"material names: {ModelToOutline.materials.Select(m => $"'{m.shader.name}'").MakeString()}");
        var mesh = GetComponentInChildren<MeshRenderer>();
        if (mesh)
        {
            _outlineMaterials = mesh.materials.Where(m => m.shader.name == "Shader Graphs/outline_shader").ToArray();
            if (_outlineMaterials.Length > 0)
                _originalOutlineWidth = _outlineMaterials[0].GetFloat(OutlinePropertyName);
        }
        _outlineSprite = GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(r => r.gameObject.CompareTag("ObjectOutline"));
    }


    void IncreaseBottleOutline()
    {
        if (!_outlineMaterials.IsNullOrEmpty())
            foreach (var m in _outlineMaterials) m.SetFloat(OutlinePropertyName, _originalOutlineWidth * outlineWidthMultiplier);
        if (_outlineSprite)
        {
            _outlineSprite.gameObject.SetActive(true);
            _outlineSprite.enabled = true;
        }
    }
    void ReturnBottleOutline()
    {
        if (!_outlineMaterials.IsNullOrEmpty())
            foreach (var m in _outlineMaterials) m.SetFloat(OutlinePropertyName, _originalOutlineWidth);
        if (_outlineSprite)
        {
            _outlineSprite.enabled = false;
            _outlineSprite.gameObject.SetActive(false);
        }
    }

}
