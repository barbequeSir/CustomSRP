using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const int _maxDirLightCount = 4;
    private const string _bufferName = "Lighting";
    private static int _dirLightCountId = Shader.PropertyToID("_directionalLightCount");
    private static  int _dirLightColorsId = Shader.PropertyToID("_directionalLightColors");
    private static  int _dirLightDirectionsId = Shader.PropertyToID("_directionalLightDirections");
    
    static Vector4[] 
        _dirLightColors = new Vector4[_maxDirLightCount],
        _dirLightDirections = new Vector4[_maxDirLightCount];
    
    private CommandBuffer _commandBuffer = new CommandBuffer()
    {
        name = _bufferName
    };

    private CullingResults _cullingResults;
    public void Setup(ScriptableRenderContext context,CullingResults cullingResults)
    {
        _cullingResults = cullingResults;
        _commandBuffer.BeginSample(_bufferName);
        SetupLights();
        _commandBuffer.EndSample(_bufferName);
        context.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();
    }

    private void SetupLights()
    {
        int dirLightCount = 0;
        NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(dirLightCount++,ref visibleLight);
                if (dirLightCount >= _maxDirLightCount)
                {
                    break;
                }    
            }
        }
        
        _commandBuffer.SetGlobalInt(_dirLightCountId,dirLightCount);
        _commandBuffer.SetGlobalVectorArray(_dirLightColorsId,_dirLightColors);
        _commandBuffer.SetGlobalVectorArray(_dirLightDirectionsId,_dirLightDirections);
    }
    
    private void SetupDirectionalLight(int index,ref VisibleLight visibleLight)
    {
        _dirLightColors[index] = visibleLight.finalColor;
        _dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
    }
}
