using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer _cameraRenderer = new CameraRenderer();

    private bool _useDynamicBatch;
    private bool _useGpuInstance;
    private ShadowSettings _shadowSettings;
    public CustomRenderPipeline(bool useDynamicBatch,bool useGpuInstance,bool useSRPBatch,ShadowSettings shadowSettings)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatch;
        _useDynamicBatch = useDynamicBatch;
        _useGpuInstance = useGpuInstance;
        _shadowSettings = shadowSettings;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _cameraRenderer.Render(context,camera,_useDynamicBatch,_useGpuInstance,_shadowSettings);
        }
    }
}
