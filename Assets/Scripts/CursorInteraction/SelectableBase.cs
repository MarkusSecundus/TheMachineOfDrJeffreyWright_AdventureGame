using MarkusSecundus.Utils.Datastructs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SelectableBase : MonoBehaviour
{

    const string OutlinePropertyName = "_OutlineThickness";

    [SerializeField] public UnityEvent OnClicked;


    [SerializeField] float outlineWidthMultiplier = 3f;
    protected virtual void OnMouseEnter() => IncreaseBottleOutline();
    protected virtual void OnMouseExit() => ReturnBottleOutline();
    protected virtual void OnDisable() => ReturnBottleOutline();
    protected virtual void OnMouseUpAsButton() { OnClicked?.Invoke(); }


    public static bool SomethingIsBeingDragged => timestampForFrameWhereSomethingWasBeingDragged + 1 >= Time.frameCount;

    private static int timestampForFrameWhereSomethingWasBeingDragged = -1;
    protected virtual void OnMouseDrag() { timestampForFrameWhereSomethingWasBeingDragged = Time.frameCount; }


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
            if(_outlineMaterials.Length > 0)
                _originalOutlineWidth = _outlineMaterials[0].GetFloat(OutlinePropertyName);
        }
        _outlineSprite = GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(r => r.gameObject.CompareTag("ObjectOutline"));
    }


    void IncreaseBottleOutline()
    {
        if(!_outlineMaterials.IsNullOrEmpty())
            foreach(var m in _outlineMaterials) m.SetFloat(OutlinePropertyName, _originalOutlineWidth * outlineWidthMultiplier);
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
