Shader "Custom/URP_SphereShockWave"
{
     Properties
    {
        _FocalPoint("Focal Point", Vector) = (0.5,0.5,0,0)
        _Radius("Max Radius", Float) = 0.5
        _Strength("Distortion Strength", Float) = 2.0
        _PulseInterval("Pulse Interval (sec)", Float) = 2.0
        _WaveWidth("Ring Width", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_CameraColorTexture);
            SAMPLER(sampler_CameraColorTexture);

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _FocalPoint;
            float _Radius;
            float _Strength;
            float _PulseInterval;
            float _WaveWidth;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // スクリーンUV
                float2 screenUV = IN.positionCS.xy / IN.positionCS.w * 0.5 + 0.5;

                // 中心からの距離
                float2 dir = screenUV - _FocalPoint.xy;
                float dist = length(dir);

                // 球体内部マスク
                float mask = smoothstep(_Radius, 0.0, dist);

                // パルス時間（0→1）
                float pulseTime = frac(_Time.y / _PulseInterval);
                float waveCenter = pulseTime * _Radius;

                // リング状衝撃波
                float shock = smoothstep(waveCenter - _WaveWidth, waveCenter, dist)
                            - smoothstep(waveCenter, waveCenter + _WaveWidth, dist);
                shock = saturate(shock);

                // 歪みベクトル
                float2 offset = normalize(dir) * _Strength * shock;

                // Scene Colorサンプリング
                float4 sceneCol = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, screenUV + offset);

                // 球体自体は透明、衝撃波リングだけ表示
                float4 outCol = float4(0,0,0,0); 
                outCol.rgb = sceneCol.rgb;
                outCol.a = shock;

                return outCol;
            }
            ENDHLSL
        }
    }
}