using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeshBalls : MonoBehaviour
{
    private static int _baseColorId = Shader.PropertyToID("_BaseColor");
    private static int _cutoffId = Shader.PropertyToID("_Cutoff");
    private static int _metallicId = Shader.PropertyToID("_Metallic");
    private static int _smoothnessId = Shader.PropertyToID("_Smoothness");
    
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    
    [SerializeField] private int _count = 1023;
    
    Matrix4x4[] _matrices;
    Vector4[] _colors;
    private float[] _cutoffs;
    private float[] _metallics;
    private float[] _smoothnesss;
    private MaterialPropertyBlock _materialPropertyBlock;
    
    private void Awake()
    {
        _matrices = new Matrix4x4[_count];
        _colors = new Vector4[_count];
        _cutoffs = new float[_count];
        _metallics = new float[_count];
        _smoothnesss = new float[_count];
        for (int i = 0; i < _matrices.Length; i++)
        {
            _matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10, Quaternion.identity, Vector3.one);
            _colors[i] = new Vector4(Random.value,Random.value,Random.value,1);
            _cutoffs[i] = Random.value;
            _smoothnesss[i] = Random.Range(0.05f, 0.95f);
            _metallics[i] =  Random.value < 0.25f ? 1f : 0f;;
        }
    }

    private void Update()
    {
        if (_materialPropertyBlock == null)
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
            _materialPropertyBlock.SetVectorArray(_baseColorId,_colors);
            _materialPropertyBlock.SetFloatArray(_cutoffId,_cutoffs);
            _materialPropertyBlock.SetFloatArray(_metallicId,_metallics);
            _materialPropertyBlock.SetFloatArray(_smoothnessId,_smoothnesss);
        }
        
        Graphics.DrawMeshInstanced(_mesh,0,_material,_matrices,_count,_materialPropertyBlock);
    }
}
