Shader "Custom/RevealFromBottom"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _RevealProgress ("Reveal Progress", Range(0, 1)) = 0
        _NoiseScale ("Noise Scale", Float) = 10.0
        _NoiseStrength ("Noise Strength (Edge Width)", Range(0, 0.3)) = 0.05
        _EdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
        _EdgeWidth ("Edge Glow Width", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        // 透明描画のためのタグ設定
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        LOD 200

        Pass
        {
            // アルファブレンディング有効化
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float  worldY : TEXCOORD1; // ワールド空間のY座標
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _Color;
            float     _RevealProgress;
            float     _NoiseScale;
            float     _NoiseStrength;
            float4    _EdgeColor;
            float     _EdgeWidth;

            // ─────────────────────────────────────────
            // シンプルなハッシュ関数（2D → float）
            // ─────────────────────────────────────────
            float hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            // ─────────────────────────────────────────
            // Value Noise（双線形補間）
            // ─────────────────────────────────────────
            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                // スムーズステップ
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));

                return lerp(lerp(a, b, u.x),
                            lerp(c, d, u.x), u.y);
            }

            // ─────────────────────────────────────────
            // フラクタルノイズ（オクターブ重ね合わせ）
            // ─────────────────────────────────────────
            float fbm(float2 p)
            {
                float value = 0.0;
                float amp   = 0.5;
                float freq  = 1.0;

                for (int i = 0; i < 4; i++)
                {
                    value += amp * valueNoise(p * freq);
                    amp  *= 0.5;
                    freq *= 2.0;
                }
                return value;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos    = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);

                // ワールド空間のY座標を渡す（重力方向で確実に「下から」表示できる）
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldY = worldPos.y;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // ── ワールドY座標を [0, 1] に正規化 ──────────────────────
                // オブジェクトのワールドY中心と、バウンズ半分の高さを基準にする
                float objCenterY = unity_ObjectToWorld[1][3];             // Y位置
                float objScaleY  = length(float3(unity_ObjectToWorld[0][1],
                                                 unity_ObjectToWorld[1][1],
                                                 unity_ObjectToWorld[2][1])); // Yスケール
                float halfHeight = objScaleY * 0.5;

                // normalizedY: 0=オブジェクト下端, 1=オブジェクト上端
                float normalizedY = (i.worldY - (objCenterY - halfHeight)) / (objScaleY);
                normalizedY = clamp(normalizedY, 0.0, 1.0);

                // ── ノイズ計算 ──────────────────────────────────────────
                // ノイズはUV座標ベース（テクスチャの模様に沿ったゆらぎ）
                float2 noiseUV     = i.uv * _NoiseScale;
                float  noise       = fbm(noiseUV);
                float  noiseOffset = (noise - 0.5) * _NoiseStrength;

                // ── 境界判定 ────────────────────────────────────────────
                // normalizedY が低い（下側）ほど先に表示される
                // Progress 0→1 で threshold が 0→1 に増え、下から上へ表示が広がる
                float threshold = _RevealProgress + noiseOffset;

                // normalizedY < threshold → 表示（alpha=1）
                // normalizedY > threshold → 非表示（alpha=0）
                float alpha = smoothstep(threshold + 0.005, threshold - 0.005, normalizedY);

                // ── エッジグロー ────────────────────────────────────────
                float edgeDist = abs(normalizedY - threshold);
                float edgeMask = 1.0 - smoothstep(0.0, _EdgeWidth, edgeDist);

                // ── テクスチャカラー ────────────────────────────────────
                fixed4 texColor   = tex2D(_MainTex, i.uv) * _Color;
                fixed4 finalColor = lerp(texColor, _EdgeColor, edgeMask * alpha);
                finalColor.a      = alpha;

                clip(alpha - 0.001);

                return finalColor;
            }
            ENDCG
        }
    }

    FallBack "Transparent/Diffuse"
}