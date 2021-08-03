using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private const string _bufferName = "Render Camera 1";

    private CommandBuffer _commandBuffer = new CommandBuffer()
    {
        name = _bufferName
    };
    private ScriptableRenderContext _scriptableRenderContext;
    private Camera _camera;
    private CullingResults _cullingResults;
    private static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static ShaderTagId _litSahderTagId = new ShaderTagId("CustomLit");

    private Lighting _lighting = new Lighting();
    public void Render(ScriptableRenderContext context, Camera camera,bool useDynamicBatch,bool useGpuInstance)
    {
        _scriptableRenderContext = context;
        _camera = camera;
        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull())
        {
            return;
        }
        Setup();
        _lighting.Setup(context,_cullingResults);
        DrawVisibleGeometry(useDynamicBatch,useGpuInstance);
        DrawUnsupportedShaders();
        DrawGizmos();
        Summit();
    }

    void DrawVisibleGeometry(bool useDynamicBatch,bool useGpuInstance)
    {
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        //filteringSettings.renderQueueRange = RenderQueueRange.all;
        var sortingSettings = new SortingSettings(_camera);
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        DrawingSettings drawingSettings = new DrawingSettings(_unlitShaderTagId,sortingSettings)
        {
            enableInstancing = useDynamicBatch,
            enableDynamicBatching = useGpuInstance
        };
        drawingSettings.SetShaderPassName(1,_litSahderTagId);
        _scriptableRenderContext.DrawRenderers(_cullingResults,ref drawingSettings,ref filteringSettings);
        _scriptableRenderContext.DrawSkybox(this._camera);
        
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        _scriptableRenderContext.DrawRenderers(_cullingResults,ref drawingSettings,ref filteringSettings);
    }

    void Summit()
    { 
        _commandBuffer.EndSample(SampleName);
        ExecuteBuffer();
        _scriptableRenderContext.Submit();
    }

    void Setup()
    {
        _scriptableRenderContext.SetupCameraProperties(_camera);
        CameraClearFlags clearFlags = _camera.clearFlags;
        _commandBuffer.ClearRenderTarget(clearFlags<=CameraClearFlags.Depth,clearFlags == CameraClearFlags.Color,clearFlags == CameraClearFlags.Color ? _camera.backgroundColor.linear:Color.white);
        _commandBuffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    void ExecuteBuffer()
    {
        _scriptableRenderContext.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();
    }

    bool Cull()
    {
        if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            _cullingResults = _scriptableRenderContext.Cull(ref p);
            return true;
        }

        return false;
    }
    
    
}
