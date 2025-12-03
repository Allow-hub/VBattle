Shader "Custom/ImpactDistortion"
{
    Properties
    {
        _ImpactCenter ("Impact Center", Vector) = (0, 0, 0, 0)
        _MaxRadius ("Max Radius", Range(0, 10)) = 3.0
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 5.0
        _DistortionStrength ("Distortion Strength", Range(0, 5)) = 1.0
        _RingWidth ("Ring Width", Range(0.1, 2)) = 0.5
        _DistortionColor ("Distortion Color", Color) = (1, 1, 1, 0.8)
        [Toggle] _AutoExpand ("Auto Expand", Float) = 1
        _ManualRadius ("Manual Radius", Range(0, 10)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 originalPos : TEXCOORD1;
                float distanceFromCenter : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float3 _ImpactCenter;
                float _MaxRadius;
                float _WaveSpeed;
                float _DistortionStrength;
                float _RingWidth;
                float4 _DistortionColor;
                float _AutoExpand;
                float _ManualRadius;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                
                // オブジェクト座標をワールド座標に変換
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.originalPos = worldPos;
                
                // 衝撃中心からの距離を計算
                float3 toCenter = worldPos - _ImpactCenter;
                float distanceFromCenter = length(toCenter);
                o.distanceFromCenter = distanceFromCenter;
                
                float3 offset = float3(0, 0, 0);
                
                // 現在の衝撃波の半径を計算
                float currentRadius;
                if (_AutoExpand > 0.5)
                {
                    // 自動拡散：時間に基づいて拡散
                    currentRadius = fmod(_Time.y * _WaveSpeed, _MaxRadius);
                }
                else
                {
                    // 手動制御
                    currentRadius = _ManualRadius;
                }
                
                // 衝撃波のリング範囲内かどうかを判定
                float ringDistance = abs(distanceFromCenter - currentRadius);
                float ringMask = 1.0 - smoothstep(0, _RingWidth, ringDistance);
                
                if (ringMask > 0.01)
                {
                    // 衝撃波による歪み
                    float3 directionFromCenter = normalize(toCenter);
                    
                    // 放射状の歪み（外向き）
                    float radialWave = sin(distanceFromCenter * 5.0 - _Time.y * _WaveSpeed * 2.0) * ringMask;
                    offset += directionFromCenter * radialWave * _DistortionStrength * 0.3;
                    
                    // 円周方向の歪み
                    float3 tangent1 = normalize(cross(directionFromCenter, float3(0, 1, 0)));
                    if (length(tangent1) < 0.1)
                        tangent1 = normalize(cross(directionFromCenter, float3(1, 0, 0)));
                    
                    float3 tangent2 = cross(directionFromCenter, tangent1);
                    
                    float circumferentialWave1 = cos(distanceFromCenter * 8.0 - _Time.y * _WaveSpeed * 1.5) * ringMask;
                    float circumferentialWave2 = sin(distanceFromCenter * 6.0 - _Time.y * _WaveSpeed * 1.8) * ringMask;
                    
                    offset += tangent1 * circumferentialWave1 * _DistortionStrength * 0.4;
                    offset += tangent2 * circumferentialWave2 * _DistortionStrength * 0.4;
                    
                    // 高周波の細かい歪み
                    float highFreqWave = sin(distanceFromCenter * 15.0 - _Time.y * _WaveSpeed * 3.0) * 
                                        cos(distanceFromCenter * 12.0 - _Time.y * _WaveSpeed * 2.5);
                    offset += directionFromCenter * highFreqWave * ringMask * _DistortionStrength * 0.15;
                    
                    // ランダムな方向の微細な歪み
                    float randomSeed1 = sin(worldPos.x * 43.758 + worldPos.y * 12.9898 + worldPos.z * 37.719);
                    float randomSeed2 = cos(worldPos.x * 37.719 + worldPos.y * 43.758 + worldPos.z * 12.9898);
                    float3 randomDir = normalize(float3(randomSeed1, randomSeed2, sin(randomSeed1 * randomSeed2)));
                    
                    float microDistortion = sin(_Time.y * _WaveSpeed * 4.0 + distanceFromCenter * 20.0) * ringMask;
                    offset += randomDir * microDistortion * _DistortionStrength * 0.1;
                }

                o.positionCS = TransformWorldToHClip(worldPos + offset);
                o.positionWS = worldPos + offset;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // 現在の衝撃波の半径を再計算
                float currentRadius;
                if (_AutoExpand > 0.5)
                {
                    currentRadius = fmod(_Time.y * _WaveSpeed, _MaxRadius);
                }
                else
                {
                    currentRadius = _ManualRadius;
                }
                
                // 歪みの強さを計算
                float distortionAmount = length(i.positionWS - i.originalPos);
                
                // 衝撃波のリング範囲を再計算
                float ringDistance = abs(i.distanceFromCenter - currentRadius);
                float ringMask = 1.0 - smoothstep(0, _RingWidth, ringDistance);
                
                // 歪み強度に基づく可視化
                float visibility = saturate(distortionAmount * 8.0) * ringMask;
                
                // 歪みがある部分のみ表示
                if (visibility < 0.05)
                    discard;
                
                // 衝撃波の強度に応じた色の変化
                float intensity = visibility * (1.0 + sin(_Time.y * _WaveSpeed * 2.0) * 0.2);
                half3 color = _DistortionColor.rgb * intensity;
                
                return half4(color, _DistortionColor.a * visibility);
            }
            ENDHLSL
        }
    }
}