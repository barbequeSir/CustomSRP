using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer _cameraRenderer = new CameraRenderer();

    private bool _useDynamicBatch;
    private bool _useGpuInstance;
    public CustomRenderPipeline(bool useDynamicBatch,bool useGpuInstance,bool useSRPBatch)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatch;
        _useDynamicBatch = useDynamicBatch;
        _useGpuInstance = useGpuInstance;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _cameraRenderer.Render(context,camera,_useDynamicBatch,_useGpuInstance);
        }
    }
}
