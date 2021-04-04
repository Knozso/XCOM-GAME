using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialColorPicker : MonoBehaviour
{
    public Color color;

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private static readonly int CachedColorProperty = Shader.PropertyToID("_Color");

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<MeshRenderer>();
    }

    public void SetColor(Color colorToSet)
    {
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(CachedColorProperty, colorToSet);
        _renderer.SetPropertyBlock(_propBlock);
    }
}
