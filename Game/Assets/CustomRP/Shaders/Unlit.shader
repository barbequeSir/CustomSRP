Shader "Custom RP/Unlit"{
    Properties
    {
        _BaseColor("Color",Color)=(1,1,1,1)
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend",float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DestBlend("DestBlend",float) = 0
        [Enum(off,0,on,1)]_ZWrite("ZWrite",float) = 1
    }
    
    SubShader
    {
        Pass
        {
            Blend [_SrcBlend] [_DestBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"
            ENDHLSL
        }
    }
}