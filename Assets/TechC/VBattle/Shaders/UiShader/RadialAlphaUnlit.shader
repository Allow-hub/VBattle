Shader "TechC/FX/RadialAlphaUnlitHDR"
{
  Properties
    {
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        [HDR]_Tint ("Tint (HDR)", Color) = (5,5,5,1)

        _Center  ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Radius  ("Radius", Range(0,1)) = 0.25
        _Feather ("Feather", Range(0.0001, 1)) = 0.2
        _Invert  ("Invert (0/1)", Range(0,1)) = 0
        _Alpha   ("Global Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend One One
        ZWrite Off
        Cull Off
        ZTest Always // UI用に常に描画

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Tint;
                float4 _Center;
                float  _Radius;
                float  _Feather;
                float  _Invert;
                float  _Alpha;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

           half4 frag (Varyings IN) : SV_Target
{
    float2 uv = IN.uv;

    // 中心からの距離
    float d = distance(uv, _Center.xy);

    // フェザー付きマスク（小さめで中心集中）
    float feather = max(_Feather, 1e-5);
    float mask = saturate((_Radius - d) / feather);
    if (_Invert > 0.5) mask = 1.0 - mask;

    // HDR色をさらに強める
    float hdrMultiplier = 5.0; // 明るさ強調
    float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Tint * hdrMultiplier;

    // 発光マスク（RGBのみ）＋グローバルアルファ強化
    col.rgb *= mask * _Alpha * 2.0; // 倍率で光らせる

    // 加算用にアルファは固定1
    col.a = 1;

    return col;
}

            ENDHLSL
        }
    }
}