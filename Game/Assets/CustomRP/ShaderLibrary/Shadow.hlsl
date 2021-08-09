#ifndef CUSTOM_SHADOW_INCLUDED
#define CUSTOM_SHADOW_INCLUDED

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADES 4

struct DirectionalShadowData
{
    float strength;
    float tileIndex;
};

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomSHADOW)
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADES];
CBUFFER_END


float SampleDirectionalShadowAtlas(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas,SHADOW_SAMPLER,positionSTS);
}

float GetDirectionalShadowAttenuation(DirectionalShadowData directionalShadowData,Surface surface)
{
    if(directionalShadowData.strength <=0) return 1.0;
    float3 positionSTS = mul(_DirectionalShadowMatrices[directionalShadowData.tileIndex],float4(surface.position,1.0)).xyz;
    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    return lerp(1.0,shadow,directionalShadowData.strength);
}
#endif