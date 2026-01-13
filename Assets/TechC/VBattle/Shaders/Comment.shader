Shader "Custom/CommentNeonGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // グラデーション設定
        _GradientColor1 ("Gradient Color 1", Color) = (1, 0, 1, 0.7)
        _GradientColor2 ("Gradient Color 2", Color) = (0, 1, 1, 0.7)
        _GradientColor3 ("Gradient Color 3", Color) = (1, 1, 0, 0.7)
        _GradientDirection ("Gradient Direction", Vector) = (0, 1, 0, 0)
        _GradientSpeed ("Gradient Animation Speed", Range(0, 5)) = 1.0
        
        // ネオン効果設定
        _NeonIntensity ("Neon Intensity", Range(0, 3)) = 1.2
        _GlowSize ("Glow Size", Range(0.1, 2)) = 1.0
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 2.0
        
        // ベース設定
        _BaseColor ("Base Color", Color) = (0.9, 0.9, 0.9, 0.8)
        _EmissionBoost ("Emission Boost", Range(0.1, 5)) = 1.5
        
        // アウトライン設定
        _OutlineWidth ("Outline Width", Range(0.001, 0.1)) = 0.015
        _OutlineIntensity ("Outline Intensity", Range(0.5, 5)) = 2.0
        _OutlinePulse ("Outline Pulse Speed", Range(0, 5)) = 1.5
        
        // 透明度制御
        _TextAlpha ("Text Alpha", Range(0, 1)) = 0.85
        _GlowAlpha ("Glow Alpha", Range(0, 1)) = 0.7
        _OverallAlpha ("Overall Alpha", Range(0, 1)) = 0.9
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 100
        
        // ネオン効果のためのAdditiveブレンド
        Blend One One
        ZWrite Off
        Cull Off

        // アウトラインパス（縁光り効果）
        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite Off
            Blend One One
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _GradientColor1;
            fixed4 _GradientColor2;
            fixed4 _GradientColor3;
            float4 _GradientDirection;
            float _GradientSpeed;
            float _OutlineWidth;
            float _OutlineIntensity;
            float _OutlinePulse;
            
            // 透明度制御（アウトラインパス用）
            float _TextAlpha;
            float _GlowAlpha;
            float _OverallAlpha;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // 法線方向に頂点を押し出してアウトライン作成
                float3 norm = normalize(v.normal);
                float3 outlineVertex = v.vertex.xyz + norm * _OutlineWidth;
                
                o.vertex = UnityObjectToClipPos(float4(outlineVertex, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // テクスチャサンプリング
                fixed4 tex = tex2D(_MainTex, i.uv);
                
                // グラデーション計算（メインパスと同じ）
                float gradientPos = dot(i.uv - 0.5, _GradientDirection.xy) + 0.5;
                gradientPos += sin(_Time.y * _GradientSpeed) * 0.1;
                gradientPos = saturate(gradientPos);
                
                // 3色グラデーション
                fixed4 gradientColor;
                if (gradientPos < 0.5)
                {
                    gradientColor = lerp(_GradientColor1, _GradientColor2, gradientPos * 2.0);
                }
                else
                {
                    gradientColor = lerp(_GradientColor2, _GradientColor3, (gradientPos - 0.5) * 2.0);
                }
                
                // パルス効果（穏やかに）
                float pulse = 1.0 + sin(_Time.y * _OutlinePulse) * 0.15;
                
                // アウトライン色（透明度を適用）
                fixed4 outlineColor = gradientColor * _OutlineIntensity * pulse * tex.a * _GlowAlpha * 0.8;
                
                // 明度制限
                outlineColor.rgb = saturate(outlineColor.rgb);
                
                UNITY_APPLY_FOG(i.fogCoord, outlineColor);
                return outlineColor;
            }
            ENDCG
        }

        // メインパス
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            // グラデーション設定
            fixed4 _GradientColor1;
            fixed4 _GradientColor2;
            fixed4 _GradientColor3;
            float4 _GradientDirection;
            float _GradientSpeed;
            
            // ネオン効果設定
            float _NeonIntensity;
            float _GlowSize;
            float _FresnelPower;
            
            // ベース設定
            fixed4 _BaseColor;
            float _EmissionBoost;
            
            // アウトライン設定（メインパスでも使用）
            float _OutlineWidth;
            float _OutlineIntensity;
            float _OutlinePulse;
            
            // 透明度制御
            float _TextAlpha;
            float _GlowAlpha;
            float _OverallAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // ワールド座標での法線とビュー方向を計算
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // テクスチャサンプリング
                fixed4 tex = tex2D(_MainTex, i.uv);
                
                // グラデーション計算
                float gradientPos = dot(i.uv - 0.5, _GradientDirection.xy) + 0.5;
                gradientPos += sin(_Time.y * _GradientSpeed) * 0.1; // アニメーション
                gradientPos = saturate(gradientPos);
                
                // 3色グラデーション
                fixed4 gradientColor;
                if (gradientPos < 0.5)
                {
                    gradientColor = lerp(_GradientColor1, _GradientColor2, gradientPos * 2.0);
                }
                else
                {
                    gradientColor = lerp(_GradientColor2, _GradientColor3, (gradientPos - 0.5) * 2.0);
                }
                
                // フレネル効果の計算（強化）
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);
                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                fresnel = pow(fresnel, _FresnelPower);
                
                // ベースカラー（テキスト部分）
                fixed4 baseText = _BaseColor * tex;
                
                // ネオングロー効果（穏やかに）
                float glowMask = smoothstep(0.3, 1.0, fresnel);
                float pulseFactor = 1.0 + sin(_Time.y * 3.0) * 0.1; // パルス効果を抑える
                fixed4 neonGlow = gradientColor * glowMask * _NeonIntensity * pulseFactor * _GlowAlpha;
                
                // 内側グロー（テキスト全体に色付け）
                fixed4 innerGlow = gradientColor * tex.a * 0.6 * _TextAlpha;
                
                // エッジ検出（縁をより強く光らせる）
                float edgeDetection = 1.0 - smoothstep(0.1, 0.9, length(frac(i.uv * 10.0) - 0.5));
                fixed4 edgeGlow = gradientColor * edgeDetection * tex.a * _OutlineIntensity * 0.2 * _GlowAlpha;
                
                // 外側エミッション（抑える）
                fixed4 emission = (neonGlow + edgeGlow) * _EmissionBoost;
                
                // 最終色の合成
                fixed4 finalColor = baseText * _TextAlpha + innerGlow + emission;
                
                // HDRエミッション効果（抑える）
                finalColor.rgb *= 1.0 + (fresnel * _GlowSize * 0.5);
                
                // 明度を制限
                finalColor.rgb = saturate(finalColor.rgb);
                
                // 全体の透明度を適用
                finalColor.a = tex.a * _OverallAlpha;
                
                // フォグ適用
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
}
