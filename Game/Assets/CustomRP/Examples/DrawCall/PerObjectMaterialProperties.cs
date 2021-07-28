using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    private static int _baseColorId = Shader.PropertyToID("_BaseColor");
    [SerializeField] private Color _baseColor= Color.white;

    private MaterialPropertyBlock _materialPropertyBlock;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        if (_materialPropertyBlock == null)
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }
        
        _materialPropertyBlock.SetColor(_baseColorId,_baseColor);
        
        GetComponent<Renderer>().SetPropertyBlock(_materialPropertyBlock);
    }
}
