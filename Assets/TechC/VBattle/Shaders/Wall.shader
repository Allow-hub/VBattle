Shader "Custom/NeonWall"
{
    Properties
    {
        _MainTex ("Wall Texture", 2D) = "white" {}
        
        // ベース壁の設定
        _WallColor ("Wall Base Color", Color) = (0.1, 0.1, 0.2, 1)
        _WallBrightness ("Wall Brightness", Range(0, 2)) = 0.3
        
        // ネオンフレーム設定
        _FrameColor1 ("Frame Color 1", Color) = (1, 0, 1, 0.6)
        _FrameColor2 ("Frame Color 2", Color) = (0, 1, 1, 0.6)
        _FrameWidth ("Frame Width", Range(0.01, 0.3)) = 0.08
        _FrameIntensity ("Frame Intensity", Range(0.1, 5)) = 1.5
        _FrameGlow ("Frame Glow Size", Range(0.1, 1.0)) = 0.2
        _FrameAlpha ("Frame Alpha", Range(0, 1)) = 0.7
        
        // アニメーション設定
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1.5
        _FlowSpeed ("Flow Speed", Range(0, 10)) = 2.0
        _FlowDirection ("Flow Direction", Vector) = (1, 0, 0, 0)
        
        // エミッション設定
        _EmissionBoost ("Emission Boost", Range(0.1, 10)) = 2.5
        _GlowFalloff ("Glow Falloff", Range(0.1, 5)) = 1.5
        _OverallAlpha ("Overall Alpha", Range(0, 1)) = 0.8
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 200
        
        // より自然な透明ブレンド
        Blend SrcAlpha OneMinusSrcAlpha
        BlendOp Add
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
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
                float3 worldNormal : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float3 worldPos : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            // ベース壁の設定
            fixed4 _WallColor;
            float _WallBrightness;
            
            // ネオンフレーム設定
            fixed4 _FrameColor1;
            fixed4 _FrameColor2;
            float _FrameWidth;
            float _FrameIntensity;
            float _FrameGlow;
            
            // アニメーション設定
            float _PulseSpeed;
            float _FlowSpeed;
            float4 _FlowDirection;
            
            // エミッション設定
            float _EmissionBoost;
            float _GlowFalloff;
            float _FrameAlpha;
            float _OverallAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // ワールド座標での法線とビュー方向を計算
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // テクスチャサンプリング
                fixed4 tex = tex2D(_MainTex, i.uv);
                
                // ベース壁色
                fixed4 wallBase = _WallColor * tex * _WallBrightness;
                
                // フレーム検出（UVの端を使用）
                float2 frameUV = abs(i.uv - 0.5) * 2.0; // 中央から端への距離
                float frameDistance = max(frameUV.x, frameUV.y); // 矩形フレーム
                
                // フレームマスク（端から内側へのグラデーション）
                float frameMask = smoothstep(1.0 - _FrameWidth, 1.0, frameDistance);
                
                // フローアニメーション
                float flowOffset = _Time.y * _FlowSpeed;
                float flowPattern = sin((frameDistance * 10.0) + flowOffset) * 0.5 + 0.5;
                
                // フレーム色のグラデーション（流れる効果）
                float gradientPos = frac(frameDistance * 3.0 + flowOffset * 0.1);
                fixed4 frameColor = lerp(_FrameColor1, _FrameColor2, gradientPos);
                
                // パルス効果（より穏やかに）
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * 0.15;
                
                // フレネル効果（角度による光り方の変化）
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);
                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                fresnel = pow(fresnel, 2.0); // もう少しソフトに
                
                // グロー効果
                float glowMask = smoothstep(1.0 - (_FrameWidth + _FrameGlow), 1.0 - _FrameWidth, frameDistance);
                glowMask = pow(glowMask, _GlowFalloff);
                
                // フレーム発光（透明度を適用）
                fixed4 frameEmission = frameColor * frameMask * _FrameIntensity * pulse * flowPattern * _FrameAlpha;
                frameEmission += frameColor * glowMask * _FrameIntensity * 0.3 * pulse * _FrameAlpha;
                
                // フレネル効果を追加（より穏やかに）
                frameEmission *= (1.0 + fresnel * 1.0);
                
                // 最終色の合成（エミッションを抑える）
                fixed4 finalColor = wallBase + frameEmission * _EmissionBoost;
                
                // 全体の透明度を調整
                finalColor.a = tex.a * _OverallAlpha;
                
                // 明度を抑える（HDRを避ける）
                finalColor.rgb = saturate(finalColor.rgb);
                
                // フォグ適用
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
}
