﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeshBalls : MonoBehaviour
{
    private static int _baseColorId = Shader.PropertyToID("_BaseColor");
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    
    [SerializeField] private int _count = 2000;
    
    Matrix4x4[] _matrices;
    Vector4[] _colors;
    private MaterialPropertyBlock _materialPropertyBlock;
    
    private void Awake()
    {
        _matrices = new Matrix4x4[_count];
        _colors = new Vector4[_count];
        for (int i = 0; i < _matrices.Length; i++)
        {
            _matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10, Quaternion.identity, Vector3.one);
            _colors[i] = new Vector4(Random.value,Random.value,Random.value,1);
        }
    }

    private void Update()
    {
        if (_materialPropertyBlock == null)
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
            _materialPropertyBlock.SetVectorArray(_baseColorId,_colors);
        }
        
        Graphics.DrawMeshInstanced(_mesh,0,_material,_matrices,_count,_materialPropertyBlock);
    }
}
