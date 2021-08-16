Shader "Custom RP/Lit"{
    Properties
    {
        _BaseMap("Texture",2D) = "white"{}
        _BaseColor("Color",Color)=(0.5,0.5,0.5,1)
        _Cutoff("Alpha Cutoff",Range(0.0,1.0)) = 0.5 
        _Metallic("Metallic",Range(0,1)) = 0
        _Smoothness("Smoothness",Range(0,1)) = 0.5
        [Toggle(_CLIPPING)] _Clipping("Alpha Clipping",float) = 0
        [KeywordEnum(On,Clip,Dither,Off)] _Shadows("Shadows",Float) = 0
        [Toggle(_PREMULTIPLY_ALPHA)] _PremultiplyAlpha("PremultiplyAlpha",float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend",float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DestBlend("DestBlend",float) = 0
        [Enum(off,0,on,1)]_ZWrite("ZWrite",float) = 1
        [Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows("Receive_Shadows",FLOAT) = 1
    }
    
    SubShader
    {
        Pass
        {
            Tags
            {
                "LightMode" = "CustomLit"
            }
            Blend [_SrcBlend] [_DestBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #pragma shader_feature _ _RECEIVE_SHADOWS
            #pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
            
            #pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
           
            #pragma shader_feature _PREMULTIPLY_ALPHA
            #pragma multi_compile_instancing
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "LitPass.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Tags{"LightMode"="ShadowCaster"}
            
            ColorMask 0
            
            HLSLPROGRAM
            #pragma target 3.5
            //#pragma shader_feature _CLIPPING
            #pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
            #pragma multi_compile_instancing
            #pragma vertex ShadowCasterPassVertex
            #pragma fragment ShadowCasterPassFragment
            #include "ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    
    CustomEditor "CustomShaderGUI"
}