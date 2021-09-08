Shader "Custom RP/Unlit"{
    Properties
    {
        _BaseMap("Texture",2D) = "white"{}
        _BaseColor("Color",Color)=(1,1,1,1)
        _Cutoff("Alpha Cutoff",Range(0.0,1.0)) = 0.5 
        [Toggle(_CLIPPING)] _CLipping("Alpha Clipping",float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend",float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DestBlend("DestBlend",float) = 0
        [Enum(off,0,on,1)]_ZWrite("ZWrite",float) = 1
    }
    
    SubShader
    {
        HLSLPROGRAM
        
        ENDHLSL
        
        Pass
        {
            Blend [_SrcBlend] [_DestBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #include "../ShaderLibrary/Common.hlsl"
            #include "UnlitInput.hlsl"
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Tags{"LightMode"="ShadowCaster"}
            ColorMask 0
            HLSLPROGRAM
            #include "../ShaderLibrary/Common.hlsl"
            #include "UnlitInput.hlsl"
            #pragma target 3.5
            //#pragma shader_feature _CLIPPING
            #pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
            #pragma multi_compile_instancing
            #pragma vertex ShadowCasterPassVertex
            #pragma fragment ShadowCasterPassFragment
            #include "ShadowCasterPass.hlsl"
            ENDHLSL
        }
        
        Pass 
        {
            Tags{"LightMode" = "Meta"}
            Cull Off
            
            HLSLPROGRAM
            #pragma target 3.5
            #include "../ShaderLibrary/Common.hlsl"
            #include "UnlitInput.hlsl"
            #pragma vertex MetaPassVertex
            #pragma fragment MetaPassFragment
            #include "MetaPass.hlsl"
            ENDHLSL
        }
    }
    
    CustomEditor "CustomShaderGUI"
}