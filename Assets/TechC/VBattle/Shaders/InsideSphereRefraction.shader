// =============================================================================
// 空間歪曲エフェクト Shader（Bleach リバソル風）
// Unity URP/Built-in Pipeline対応
// =============================================================================

Shader "Custom/SpaceDistortion"
{
    Properties
    {
        _MainTex ("Screen Texture", 2D) = "white" {}
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.5
        _DistortionRadius ("Distortion Radius", Range(0.1, 2.0)) = 1.0
        _EdgeSharpness ("Edge Sharpness", Range(1, 20)) = 5.0
        _WaveFrequency ("Wave Frequency", Range(1, 10)) = 3.0
        _TimeScale ("Time Scale", Range(0.1, 5.0)) = 1.0
        _CenterPoint ("Center Point", Vector) = (0.5, 0.5, 0, 0)
        _DistortionType ("Distortion Type", Range(0, 2)) = 0
        // 0: Spiral, 1: Radial Wave, 2: Vortex
        
        // 追加エフェクト
        _RippleCount ("Ripple Count", Range(1, 5)) = 3
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.1)) = 0.02
        _Brightness ("Brightness", Range(0.5, 2.0)) = 1.0
        _Contrast ("Contrast", Range(0.5, 2.0)) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+100"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "SpaceDistortionPass"
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // プロパティ
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _DistortionStrength;
                float _DistortionRadius;
                float _EdgeSharpness;
                float _WaveFrequency;
                float _TimeScale;
                float4 _CenterPoint;
                float _DistortionType;
                float _RippleCount;
                float _ChromaticAberration;
                float _Brightness;
                float _Contrast;
            CBUFFER_END
            
            // =============================================================================
            // ユーティリティ関数
            // =============================================================================
            
            // 回転行列（2D）
            float2x2 rotate2D(float angle)
            {
                float c = cos(angle);
                float s = sin(angle);
                return float2x2(c, -s, s, c);
            }
            
            // スムーズステップ関数（カスタム）
            float smootherstep(float edge0, float edge1, float x)
            {
                x = saturate((x - edge0) / (edge1 - edge0));
                return x * x * x * (x * (x * 6.0 - 15.0) + 10.0);
            }
            
            // ノイズ関数
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }
            
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            // =============================================================================
            // 歪曲エフェクト関数
            // =============================================================================
            
            // スパイラル歪曲
            float2 spiralDistortion(float2 uv, float2 center, float time)
            {
                float2 delta = uv - center;
                float dist = length(delta);
                
                if (dist > _DistortionRadius) return uv;
                
                float influence = 1.0 - smootherstep(0.0, _DistortionRadius, dist);
                influence = pow(influence, _EdgeSharpness * 0.1);
                
                float angle = time * _TimeScale * 2.0 + dist * _WaveFrequency * 10.0;
                float2 rotatedDelta = mul(rotate2D(angle * influence * _DistortionStrength), delta);
                
                // 追加の波動効果
                float wave = sin(dist * _WaveFrequency * 15.0 - time * _TimeScale * 8.0) * 0.1;
                rotatedDelta *= (1.0 + wave * influence);
                
                return center + rotatedDelta;
            }
            
            // ラジアル波動歪曲
            float2 radialWaveDistortion(float2 uv, float2 center, float time)
            {
                float2 delta = uv - center;
                float dist = length(delta);
                
                if (dist > _DistortionRadius) return uv;
                
                float influence = 1.0 - smootherstep(0.0, _DistortionRadius, dist);
                influence = pow(influence, _EdgeSharpness * 0.1);
                
                // 複数の波動リング
                float waveSum = 0.0;
                for (int i = 1; i <= _RippleCount; i++)
                {
                    float freq = _WaveFrequency * i;
                    float phase = time * _TimeScale * (2.0 + i * 0.5);
                    waveSum += sin(dist * freq * 20.0 - phase) / i;
                }
                
                float2 normal = normalize(delta);
                float2 tangent = float2(-normal.y, normal.x);
                
                float radialDisp = waveSum * influence * _DistortionStrength * 0.1;
                float tangentialDisp = cos(dist * _WaveFrequency * 8.0 - time * _TimeScale * 3.0) 
                                     * influence * _DistortionStrength * 0.05;
                
                return uv + normal * radialDisp + tangent * tangentialDisp;
            }
            
            // ボルテックス歪曲
            float2 vortexDistortion(float2 uv, float2 center, float time)
            {
                float2 delta = uv - center;
                float dist = length(delta);
                
                if (dist > _DistortionRadius) return uv;
                
                float influence = 1.0 - smootherstep(0.0, _DistortionRadius, dist);
                influence = pow(influence, _EdgeSharpness * 0.1);
                
                // ボルテックス回転
                float angle = influence * _DistortionStrength * 3.14159;
                angle += time * _TimeScale * 2.0;
                
                // ノイズによる乱流効果
                float2 noiseCoord = uv * 10.0 + time * _TimeScale;
                float turbulence = (noise(noiseCoord) - 0.5) * 2.0;
                angle += turbulence * influence * 0.5;
                
                float2 rotatedDelta = mul(rotate2D(angle), delta);
                
                // 中心への引き込み効果
                float suck = influence * _DistortionStrength * 0.3;
                rotatedDelta *= (1.0 - suck);
                
                return center + rotatedDelta;
            }
            
            // =============================================================================
            // 色収差エフェクト
            // =============================================================================
            
            float3 sampleWithChromaticAberration(TEXTURE2D(tex), SAMPLER(samp), float2 uv, float strength)
            {
                float2 center = float2(0.5, 0.5);
                float2 direction = normalize(uv - center);
                float distance = length(uv - center);
                
                float aberration = strength * distance;
                
                float r = SAMPLE_TEXTURE2D(tex, samp, uv - direction * aberration).r;
                float g = SAMPLE_TEXTURE2D(tex, samp, uv).g;
                float b = SAMPLE_TEXTURE2D(tex, samp, uv + direction * aberration).b;
                
                return float3(r, g, b);
            }
            
            // =============================================================================
            // 頂点シェーダー
            // =============================================================================
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = vertexInput.positionCS;
                output.worldPos = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }
            
            // =============================================================================
            // フラグメントシェーダー
            // =============================================================================
            
            float4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;
                float2 screenUV = input.uv;
                float2 center = _CenterPoint.xy;
                
                // 歪曲タイプに基づいてUV座標を変換
                float2 distortedUV = screenUV;
                
                if (_DistortionType < 0.5)
                {
                    // スパイラル歪曲
                    distortedUV = spiralDistortion(screenUV, center, time);
                }
                else if (_DistortionType < 1.5)
                {
                    // ラジアル波動歪曲
                    distortedUV = radialWaveDistortion(screenUV, center, time);
                }
                else
                {
                    // ボルテックス歪曲
                    distortedUV = vortexDistortion(screenUV, center, time);
                }
                
                // 境界チェック
                distortedUV = saturate(distortedUV);
                
                // 色収差を適用してサンプリング
                float3 color;
                if (_ChromaticAberration > 0.001)
                {
                    color = sampleWithChromaticAberration(_MainTex, sampler_MainTex, distortedUV, _ChromaticAberration);
                }
                else
                {
                    color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV).rgb;
                }
                
                // 色調補正
                color = saturate((color - 0.5) * _Contrast + 0.5);
                color *= _Brightness;
                
                // エッジグロー効果
                float2 delta = screenUV - center;
                float dist = length(delta);
                float edgeGlow = 0.0;
                
                if (dist < _DistortionRadius)
                {
                    float influence = 1.0 - smootherstep(0.0, _DistortionRadius, dist);
                    float edgeProximity = 1.0 - smootherstep(_DistortionRadius * 0.8, _DistortionRadius, dist);
                    
                    // 脈動するエッジグロー
                    float pulse = sin(time * _TimeScale * 4.0) * 0.5 + 0.5;
                    edgeGlow = edgeProximity * pulse * 0.3;
                    
                    // エネルギーフラッシュ効果
                    float flash = sin(time * _TimeScale * 12.0 + dist * 30.0) * 0.5 + 0.5;
                    edgeGlow += influence * flash * 0.1;
                }
                
                // 最終色合成
                color += edgeGlow * float3(0.8, 0.9, 1.0); // 青白いエネルギー色
                
                // 中心部の強化効果
                float centerDist = length(screenUV - center);
                if (centerDist < _DistortionRadius * 0.2)
                {
                    float centerEffect = 1.0 - smootherstep(0.0, _DistortionRadius * 0.2, centerDist);
                    float energyBoost = sin(time * _TimeScale * 8.0) * 0.5 + 0.5;
                    color += centerEffect * energyBoost * float3(1.0, 0.8, 0.6) * 0.2;
                }
                
                // アルファ値の計算（エフェクト範囲）
                float alpha = 0.0;
                if (dist < _DistortionRadius)
                {
                    alpha = 1.0 - smootherstep(0.0, _DistortionRadius, dist);
                    alpha = pow(alpha, _EdgeSharpness * 0.2) * 0.8;
                }
                
                return float4(color, alpha);
            }
            ENDHLSL
        }
    }
    
    // Built-in Pipeline用のフォールバック
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+100"
        }
        
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _DistortionStrength;
            float _DistortionRadius;
            float _EdgeSharpness;
            float _WaveFrequency;
            float _TimeScale;
            float4 _CenterPoint;
            float _DistortionType;
            float _RippleCount;
            float _ChromaticAberration;
            float _Brightness;
            float _Contrast;
            
            // ユーティリティ関数（CG版）
            float2 rotate2D_CG(float2 v, float angle)
            {
                float c = cos(angle);
                float s = sin(angle);
                return float2(v.x * c - v.y * s, v.x * s + v.y * c);
            }
            
            float smootherstep_CG(float edge0, float edge1, float x)
            {
                x = saturate((x - edge0) / (edge1 - edge0));
                return x * x * x * (x * (x * 6.0 - 15.0) + 10.0);
            }
            
            // 歪曲関数（CG版）
            float2 spiralDistortionCG(float2 uv, float2 center, float time)
            {
                float2 delta = uv - center;
                float dist = length(delta);
                
                if (dist > _DistortionRadius) return uv;
                
                float influence = 1.0 - smootherstep_CG(0.0, _DistortionRadius, dist);
                influence = pow(influence, _EdgeSharpness * 0.1);
                
                float angle = time * _TimeScale * 2.0 + dist * _WaveFrequency * 10.0;
                float2 rotatedDelta = rotate2D_CG(delta, angle * influence * _DistortionStrength);
                
                float wave = sin(dist * _WaveFrequency * 15.0 - time * _TimeScale * 8.0) * 0.1;
                rotatedDelta *= (1.0 + wave * influence);
                
                return center + rotatedDelta;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float time = _Time.y;
                float2 screenUV = i.uv;
                float2 center = _CenterPoint.xy;
                
                // 歪曲適用
                float2 distortedUV = spiralDistortionCG(screenUV, center, time);
                distortedUV = saturate(distortedUV);
                
                // 色収差サンプリング
                float3 color;
                if (_ChromaticAberration > 0.001)
                {
                    float2 direction = normalize(distortedUV - float2(0.5, 0.5));
                    float distance = length(distortedUV - float2(0.5, 0.5));
                    float aberration = _ChromaticAberration * distance;
                    
                    float r = tex2D(_MainTex, distortedUV - direction * aberration).r;
                    float g = tex2D(_MainTex, distortedUV).g;
                    float b = tex2D(_MainTex, distortedUV + direction * aberration).b;
                    color = float3(r, g, b);
                }
                else
                {
                    color = tex2D(_MainTex, distortedUV).rgb;
                }
                
                // 色調補正
                color = saturate((color - 0.5) * _Contrast + 0.5);
                color *= _Brightness;
                
                // エッジグロー
                float dist = length(screenUV - center);
                float alpha = 0.0;
                if (dist < _DistortionRadius)
                {
                    alpha = 1.0 - smootherstep_CG(0.0, _DistortionRadius, dist);
                    alpha = pow(alpha, _EdgeSharpness * 0.2) * 0.8;
                    
                    float pulse = sin(time * _TimeScale * 4.0) * 0.5 + 0.5;
                    color += alpha * pulse * float3(0.8, 0.9, 1.0) * 0.3;
                }
                
                return float4(color, alpha);
            }
            ENDCG
        }
    }
    
    FallBack "Sprites/Default"
}