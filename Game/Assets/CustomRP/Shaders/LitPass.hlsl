#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Shadow.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/GI.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

struct Attribute{
    float3 positionOS:POSITION;
    float3 normalOS:NORMAL;
    float2 baseUV:TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    GI_ATTRIBUTE_DATA
};

struct Varyings{
    float4 positionCS:SV_POSITION;
    float3 positionWS:VAR_POSITION;
    float3 normalWS:VAR_NORAML;
    float2 baseUV:VAR_BASE_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    GI_VARYINGS_DATA
};


Varyings LitPassVertex(Attribute input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input,output);
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.normalWS =   TransformObjectToWorldNormal(input.normalOS);
    TRANSFER_GI_DATA(input,output)
    #if UNITY_REVERSED_Z
        output.positionCS.z = min(output.positionCS.z,output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
        output.positionCS.z = max(output.positionCS.z,output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif
    
    output.baseUV = TransformBaseUV(input.baseUV);
    return output;
}



float4 LitPassFragment(Varyings input):SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    
    float4 base = GetBase(input.baseUV);
    Surface surface;
    surface.normal = normalize(input.normalWS);
    surface.color = base.rgb;
    surface.alpha = base.a;
    surface.metallic = GetMetallic(input.baseUV);
    surface.smoothness = GetSmoothness(input.baseUV);
    surface.viewDirection = normalize( _WorldSpaceCameraPos - input.positionWS);
    surface.position = input.positionWS; 
    surface.depth = -TransformWorldToView(input.positionWS).z;
    surface.dither = InterleavedGradientNoise(input.positionCS.xy,0);
    #if defined(_CLIPPING)
    clip(base.a - GetCutoff(input.baseUV));
    #endif
    #if defined(_PREMULTIPLY_ALPHA)
        BRDF brdf = GetBRDF(surface,true);
    #else
        BRDF brdf = GetBRDF(surface);
    #endif
    GI gi = GetGI(GI_FRAGMENT_DATA(input),surface);
    float3 color = GetLighting(surface,brdf,gi);
    return float4(color,surface.alpha);
    
}

#endif