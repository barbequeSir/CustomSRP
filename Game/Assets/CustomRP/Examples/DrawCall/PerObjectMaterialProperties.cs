using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    private static int _baseColorId = Shader.PropertyToID("_BaseColor");
    private static int _cutoffId = Shader.PropertyToID("_Cutoff");
    [SerializeField] private Color _baseColor= Color.white;
    [SerializeField,Range(0f,1f)] private float _cutoff = 0.5f;
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
        _materialPropertyBlock.SetFloat(_cutoffId,_cutoff);
        GetComponent<Renderer>().SetPropertyBlock(_materialPropertyBlock);
    }
}
