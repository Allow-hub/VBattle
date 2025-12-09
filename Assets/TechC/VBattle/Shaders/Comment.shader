Shader "Custom/CommentNeon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NeonColor ("Neon Color", Color) = (1, 1, 1, 1)
        _NeonIntensity ("Neon Intensity", Range(0, 2)) = 0.3
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 1.5
        _BaseColor ("Base Color", Color) = (0.7, 0.7, 0.7, 1)
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
            fixed4 _NeonColor;
            float _NeonIntensity;
            float _FresnelPower;
            fixed4 _BaseColor;

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
                
                // フレネル効果の計算
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);
                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                fresnel = pow(fresnel, _FresnelPower);
                
                // 文字の可読性を保つためのベース色（明るめ）
                fixed4 baseText = _BaseColor * tex;
                
                // エッジグローの計算（控えめに）
                float glowMask = smoothstep(0.4, 1.0, fresnel);
                fixed4 edgeGlow = _NeonColor * glowMask * _NeonIntensity * 0.5;
                
                // 最終色：ベース色 + エッジグロー
                fixed4 finalColor = baseText + edgeGlow;
                
                // アルファは元のテクスチャを基準
                finalColor.a = tex.a;
                
                // フォグ適用
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
}
