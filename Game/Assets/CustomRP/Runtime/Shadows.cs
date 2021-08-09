using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    private static int _dirctionalShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    private static int _directionalShadowMatrixsId = Shader.PropertyToID("_DirectionalShadowMatrices");
    
    private const string _bufferName = "ShadowsClass";
    private CommandBuffer _commandBuffer = new CommandBuffer()
    {
        name = _bufferName
    };

    private CullingResults _cullingResults;
    private ShadowSettings _shadowSettings;
    private ScriptableRenderContext _context;

    private const int _maxShadowdDirectionalLightCount = 4;
    private const int _maxCascades = 4;
    private int _shadowdDirectionalLightCount;
    
    static Matrix4x4[] _diretionalShadowMatrices = new Matrix4x4[_maxShadowdDirectionalLightCount * _maxCascades];
    struct ShadowdDirectionalLight
    {
        public int visibleLightIndex;
    }
    
    ShadowdDirectionalLight[] _shadowdDirectionalLights = new ShadowdDirectionalLight[_maxShadowdDirectionalLightCount];


    public void Setup(ScriptableRenderContext context,CullingResults cullingResults,ShadowSettings shadowSettings)
    {
        _context = context;
        _cullingResults = cullingResults;
        _shadowSettings = shadowSettings;
        _shadowdDirectionalLightCount = 0;
    }

    private void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();
    }

    public Vector2 ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (_shadowdDirectionalLightCount < _maxShadowdDirectionalLightCount
        && light.shadows!=LightShadows.None && light.shadowStrength>0f
        && _cullingResults.GetShadowCasterBounds(visibleLightIndex,out Bounds b))
        {
            _shadowdDirectionalLights[_shadowdDirectionalLightCount] = new ShadowdDirectionalLight(){visibleLightIndex = visibleLightIndex};
            return new Vector2(light.shadowStrength, _shadowSettings.directional.cascadeCount * _shadowdDirectionalLightCount++);
        }
        return Vector2.zero;
    }

    public void Render()
    {
        if (_shadowdDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            _commandBuffer.GetTemporaryRT(_dirctionalShadowAtlasId,1,1,32,FilterMode.Bilinear,RenderTextureFormat.Shadowmap);
        }
    }

    private void RenderDirectionalShadows()
    {
        int atlasSize = (int)_shadowSettings.directional.atlasSize;
        _commandBuffer.GetTemporaryRT(_dirctionalShadowAtlasId, atlasSize,atlasSize,32,FilterMode.Bilinear,RenderTextureFormat.Shadowmap);
        _commandBuffer.SetRenderTarget(_dirctionalShadowAtlasId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
        _commandBuffer.ClearRenderTarget(true,false,Color.clear);
        
        _commandBuffer.BeginSample(_bufferName);

        int tiles = _shadowdDirectionalLightCount * _shadowSettings.directional.cascadeCount;
        int split = tiles <= 1 ? 1 : tiles<= 4 ? 2: 4;
        int tileSize = atlasSize / split;
        for (int i = 0; i < _shadowdDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(_shadowdDirectionalLights[i].visibleLightIndex,split,tileSize);
        }
        
        _commandBuffer.SetGlobalMatrixArray(_directionalShadowMatrixsId,_diretionalShadowMatrices);
        _commandBuffer.EndSample(_bufferName);
        ExecuteBuffer();
    }

    private void RenderDirectionalShadows(int index,int split, int tileSize)
    {
        ShadowdDirectionalLight light = _shadowdDirectionalLights[index];
        ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings(_cullingResults,light.visibleLightIndex);

        int cascadeCount = _shadowSettings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 cascadeRatios = _shadowSettings.directional.CascadeRatios;

        for (int i = 0; i < cascadeCount; i++)
        {
            _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, i, cascadeCount,
                cascadeRatios, tileSize, 0f,
                out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData);
            
            shadowSettings.splitData = shadowSplitData;
            
            
            int tileIndex = i + tileOffset;
            Vector2 offset =SetTileViewport(tileIndex,split,tileSize);

            _diretionalShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projMatrix * viewMatrix,offset,split);
            
            _commandBuffer.SetViewProjectionMatrices(viewMatrix,projMatrix);
            ExecuteBuffer();
            _context.DrawShadows(ref shadowSettings);
        }
    }

    private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }
        
        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = (0.5f * (m.m20 + m.m30) + offset.y * m.m30) * scale;
        m.m21 = (0.5f * (m.m21 + m.m31) + offset.y * m.m31) * scale;
        m.m22 = (0.5f * (m.m22 + m.m32) + offset.y * m.m32) * scale;
        m.m23 = (0.5f * (m.m23 + m.m33) + offset.y * m.m33) * scale;
        
        return m;
    }
    private Vector2 SetTileViewport(int index, int split,int tileSize)
    {
        Vector2 offset = new Vector2(index% split,index / split);
        _commandBuffer.SetViewport(new Rect(offset.x * tileSize,offset.y * tileSize,tileSize,tileSize));
        return offset;
    }
    public void Cleanup()
    {
        _commandBuffer.ReleaseTemporaryRT(_dirctionalShadowAtlasId);
        ExecuteBuffer();
    }
}
