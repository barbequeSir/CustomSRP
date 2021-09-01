using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    private static int _dirctionalShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    private static int _directionalShadowMatrixsId = Shader.PropertyToID("_DirectionalShadowMatrices");
    private static int _cascadeCountId = Shader.PropertyToID("_CascadeCount");
    private static int _cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSheres");
    private static int _cascadeDataId = Shader.PropertyToID("_CascadeData");
    private static int _shadowDistancFadeId = Shader.PropertyToID("_ShadowDistanceFade");

    private static int _shadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize");

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
    static Vector4[] _cascadeCullingSperes = new Vector4[_maxCascades];
    static Vector4[] _cascadeData = new Vector4[_maxCascades];
    struct ShadowdDirectionalLight
    {
        public int visibleLightIndex;
        public float slopScaleBias;
        public float nearPlaneOffset;
    }
    
    ShadowdDirectionalLight[] _shadowdDirectionalLights = new ShadowdDirectionalLight[_maxShadowdDirectionalLightCount];

    private static string[] directionalFilterKeywords =
    {
        "_DIRECTIONAL_PCF3",
        "_DIRECTIONAL_PCF5",
        "_DIRECTIONAL_PCF7",
    };

    private static string[] cascadeBlendKeywords =
    {
        "_CASCADE_BLEND_SOFT",
        "_CASCADE_BLEND_DITHER"
    };
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

    void SetKeyword(string[] keywords,int enableIndex)
    {
        for (int i = 0; i < directionalFilterKeywords.Length; i++)
        {
            if (i == enableIndex)
            {
                _commandBuffer.EnableShaderKeyword(directionalFilterKeywords[i]);
            }
            else
            {
                _commandBuffer.DisableShaderKeyword(directionalFilterKeywords[i]);
            }
        }
    }
    public Vector3 ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (_shadowdDirectionalLightCount < _maxShadowdDirectionalLightCount
        && light.shadows!=LightShadows.None && light.shadowStrength>0f
        && _cullingResults.GetShadowCasterBounds(visibleLightIndex,out Bounds b))
        {
            _shadowdDirectionalLights[_shadowdDirectionalLightCount] = new ShadowdDirectionalLight(){visibleLightIndex = visibleLightIndex,slopScaleBias = light.shadowBias,nearPlaneOffset = light.shadowNearPlane};
            return new Vector3(light.shadowStrength, _shadowSettings.directional.cascadeCount * _shadowdDirectionalLightCount++,light.shadowNormalBias);
        }
        return Vector3.zero;
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
        _commandBuffer.SetGlobalInt(_cascadeCountId,_shadowSettings.directional.cascadeCount);
        _commandBuffer.SetGlobalVectorArray(_cascadeCullingSpheresId,_cascadeCullingSperes);

        float f = 1f - _shadowSettings.directional.cascadeFade;
        _commandBuffer.SetGlobalVector(_shadowDistancFadeId,new Vector4(1/_shadowSettings.maxDistance,1/_shadowSettings.distanceFade,1/(1-f*f)));
        
        _commandBuffer.SetGlobalVectorArray(_cascadeDataId,_cascadeData);
        
        SetKeyword(directionalFilterKeywords,(int)_shadowSettings.directional.filter -1);
        SetKeyword(cascadeBlendKeywords, (int) _shadowSettings.directional.cascadeBlend - 1);
        _commandBuffer.SetGlobalVector(_shadowAtlasSizeId,new Vector4(atlasSize,1f/atlasSize));
        _commandBuffer.EndSample(_bufferName);
        ExecuteBuffer();
    }

    void SetCascadeData(int index, Vector4 cullingSphere, float tileSize)
    {
        float texelSize = 2f * cullingSphere.w / tileSize;
        float filterSize = texelSize * ((float) _shadowSettings.directional.filter + 1);

        cullingSphere.w -= filterSize;
        cullingSphere.w *= cullingSphere.w;
        _cascadeCullingSperes[index] = cullingSphere;
        _cascadeData[index] = new Vector4(1f/cullingSphere.w,filterSize * 1.414f);
    }
    private void RenderDirectionalShadows(int index,int split, int tileSize)
    {
        ShadowdDirectionalLight light = _shadowdDirectionalLights[index];
        ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings(_cullingResults,light.visibleLightIndex);

        int cascadeCount = _shadowSettings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 cascadeRatios = _shadowSettings.directional.CascadeRatios;

        float cullingFactor = Mathf.Max(0f,0.8f - _shadowSettings.directional.cascadeFade);
        for (int i = 0; i < cascadeCount; i++)
        {
            _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, i, cascadeCount,
                cascadeRatios, tileSize, light.nearPlaneOffset,
                out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData);
            
            shadowSplitData.shadowCascadeBlendCullingFactor = cullingFactor;
            shadowSettings.splitData = shadowSplitData;
            
            if (index == 0)
            {
                var splitData = shadowSplitData.cullingSphere;
                
                SetCascadeData(i,splitData,tileSize);
            }
            
            int tileIndex = i + tileOffset;
            Vector2 offset =SetTileViewport(tileIndex,split,tileSize);

            _diretionalShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projMatrix * viewMatrix,offset,split);
            
            
            _commandBuffer.SetViewProjectionMatrices(viewMatrix,projMatrix);
            _commandBuffer.SetGlobalDepthBias(0,light.slopScaleBias);
            ExecuteBuffer();
            _context.DrawShadows(ref shadowSettings);
            _commandBuffer.SetGlobalDepthBias(0,0);
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
        
        //(m + 1 )/2   (-1,1)=>(0,1)
        /* matrixOffset1 +1
         *    1    0    0    0    
         *    0    1    0    0
         *    0    0    1    0
         *    1    1    1    1
         */
        /* matrixScale1 /2
         *    1/2    0      0    0    
         *    0      1/2    0    0
         *    0      0      1/2  0
         *    0      0      0    1
         */   
        
        /* matrixOffset2  + offset.xy
        *    1            0    0    0    
        *    0            1    0    0
        *    0            0    1    0
        *    offset.x    offset.y    0    1
        */   

        /* matrixScale2  *scale
         *    1/2    0      0    0    
         *    0      1/2    0    0
         *    0      0      1/2  0
         *    0      0      0    1
         */
        
        //m = m * matrixOffset1 * matrixScale1 * matrixOffset2 * matrixScale2;
        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
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
