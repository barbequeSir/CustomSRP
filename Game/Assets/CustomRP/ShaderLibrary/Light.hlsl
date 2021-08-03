#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4
CBUFFER_START(_CustomLight)
    int _directionalLightCount;
    float4 _directionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _directionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light
{
    float3 direction;
    float3 color;
};

int GetDirectionalLightCount()
{
    return _directionalLightCount;
}

Light GetDirectionalLight(int index)
{
    Light light;
    light.color = _directionalLightColors[index];
    light.direction = _directionalLightDirections[index];
    return light;
}
#endif