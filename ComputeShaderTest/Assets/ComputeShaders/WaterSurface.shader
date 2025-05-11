Shader "Custom/WaterSurface"
{
    Properties
    { 
        _NormalTex1("Normal Texture 1", 2D) = "bump" {}
        _NormalTex2("Normal Texture 2", 2D) = "bump" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)

        _Scale("Noise Scale", Range(0, 1)) = 0.5
        _Amplitude("Amplitude", Range(0.01, 0.1)) = 0.015
        _Speed("Speed", Range(0.01, 0.3)) = 0.15
        _NormalStrength("Normal Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            sampler2D _NoiseTex;
            sampler2D _NormalTex1;
            sampler2D _NormalTex2;

            float _Scale;
            float _Amplitude;
            float _Speed;
            float _NormalStrength;

            // The structure definition defines which variables it contains.
            // This example uses the VertexIn structure as an input structure in
            // the VertexIn shader.
            struct VertexIn
            {
                // The positionOS variable contains the VertexIn positions in object
                // space.
                float4 positionOS   : POSITION;     
                float2 uv : TEXCOORD0;  
            };

            struct VertexOut
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float2 uv : TEXCOORD0;
            };            

            // The VertexIn shader definition with properties defined in the VertexOut 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            VertexOut vert(VertexIn IN)
            {
                VertexOut OUT;

                //Apply values to texture coords
                float2 noiseUV = float2(IN.uv.xy + _Time * _Speed) * _Scale;

                //Get noise value from noise texture
                float noiseValue = tex2Dlod(_NoiseTex, float4(noiseUV, 0, 0)).x * _Amplitude;

                //Apply to sin wave
                IN.positionOS = IN.positionOS + float4(0, sin(_Time * noiseValue), 0, 0); 

                //Transform from object space to homogenous space
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = noiseUV;



                return OUT;
            }

            // The fragment shader definition.            
            half4 frag(VertexOut i) : SV_Target
            {
                
                
                return _Color;
            }
            ENDHLSL
        }
    }
}