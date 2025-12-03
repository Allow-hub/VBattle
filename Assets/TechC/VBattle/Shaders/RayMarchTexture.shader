Shader "TechC/RayMarchTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthColor ("Depth Color", Color) = (0,0,1,1) // 深い時の色（インスペクターから設定）
        _StepSize ("Step Size", Float) = 0.01
        _MaxSteps ("Max Steps", Float) = 100
        _Threshold ("Threshold", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Properties
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _DepthColor;
            float _StepSize;
            float _MaxSteps;
            float _Threshold;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float3 rayPos = float3(uv, 0.0);
                float3 rayDir = float3(0.0, 0.0, 1.0); // z方向に進める
                [loop]
                for (int i = 0; i < (int)_MaxSteps; i++)
                {
                    if (rayPos.x < 0 || rayPos.x > 1 || rayPos.y < 0 || rayPos.y > 1)
                        break;

                    float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, rayPos.xy);

                    // テクスチャの色をチェックしてヒット判定
                    if (texColor.r > _Threshold)
                    {
                        float depthFactor = rayPos.z; // 深さ (0〜1)
                        return lerp(half4(1,1,1,1), _DepthColor, depthFactor);
                    }

                    rayPos += rayDir * _StepSize;
                }

                // ヒットしなかったら透明黒
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
