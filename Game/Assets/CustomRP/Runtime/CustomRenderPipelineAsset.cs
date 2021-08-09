using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField] private bool _useDynamicBatch = true;
    [SerializeField] private bool _useGPUBatch = true;
    [SerializeField] private bool _useSRPBatch = true;

    [SerializeField] private ShadowSettings _shadowSettings = default;
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(_useDynamicBatch,_useGPUBatch,_useSRPBatch,_shadowSettings);
    }
}
