#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4
CBUFFER_START(_CustomLight)
    int _directionalLightCount;
    float4 _directionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _directionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _directionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light
{
    float3 direction;
    float3 color;
    float attenuation;
};

int GetDirectionalLightCount()
{
    return _directionalLightCount;
}

DirectionalShadowData GetDirectionalShadowData(int lightIndex,ShadowData shadowData)
{
    DirectionalShadowData directionalShadowData;
    directionalShadowData.strength = _directionalLightShadowData[lightIndex].x * shadowData.strength;
    directionalShadowData.tileIndex = _directionalLightShadowData[lightIndex].y + shadowData.cascadeIndex;
    directionalShadowData.normalBias =_directionalLightShadowData[lightIndex].z;
    return directionalShadowData;
}

Light GetDirectionalLight(int index,Surface surface,ShadowData shadowData)
{
    Light light;
    light.color = _directionalLightColors[index];
    light.direction = _directionalLightDirections[index];
    DirectionalShadowData directionalShadowData = GetDirectionalShadowData(index,shadowData);
    light.attenuation = GetDirectionalShadowAttenuation(directionalShadowData,shadowData,surface);
    //light.attenuation = shadowData.cascadeIndex * 0.25;
    return light;
}


#endif