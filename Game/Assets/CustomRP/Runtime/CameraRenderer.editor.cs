using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    partial void DrawUnsupportedShaders();
    partial void DrawGizmos();
    partial void PrepareForSceneWindow();
    partial void PrepareBuffer();
#if UNITY_EDITOR
    private string SampleName { get; set; }
    partial void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        _commandBuffer.name = SampleName = _camera.name;
        Profiler.EndSample();
    }
#else
    private string SampleName = _bufferName;
#endif

#if UNITY_EDITOR
    private static ShaderTagId[] _legacyShaderTagIds = new[]
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),
    };

    private static Material _errorMaterial;

    
    
    partial void PrepareForSceneWindow()
    {
        if (_camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }
    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            _scriptableRenderContext.DrawGizmos(_camera,GizmoSubset.PreImageEffects);
            _scriptableRenderContext.DrawGizmos(_camera,GizmoSubset.PostImageEffects);
        }
    }
    partial void DrawUnsupportedShaders()
    {
        if (_errorMaterial == null)
        {
            _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawSetting = new DrawingSettings(_legacyShaderTagIds[0],new SortingSettings(_camera))
        {
            overrideMaterial = _errorMaterial
        };
        for (int i = 1; i < _legacyShaderTagIds.Length; i++)
        {
            drawSetting.SetShaderPassName(i,_legacyShaderTagIds[i]);
        }
        var filterSetting = FilteringSettings.defaultValue;
        _scriptableRenderContext.DrawRenderers(_cullingResults,ref drawSetting,ref filterSetting);
    }
#endif
}
