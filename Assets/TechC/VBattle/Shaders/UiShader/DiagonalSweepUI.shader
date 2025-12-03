Shader "Unlit/DiagonalSweepUI"
{
    Properties
    {
        _MainTex("Main Tex (RGBA)", 2D) = "white" {}
        _Color("Tint Color", Color) = (1,1,1,1)

        _SweepColor("Sweep Color", Color) = (1,1,1,0.9)
        _SweepIntensity("Sweep Intensity", Range(0,4)) = 1.2
        _SweepWidth("Sweep Width", Range(0.01,1)) = 0.15
        _SweepSpeed("Sweep Speed", Range(-2,2)) = 0.8
        _Angle("Sweep Angle (deg)", Range(-180,180)) = -30

        _Softness("Edge Softness", Range(0.001,0.5)) = 0.08

        // optional mask: if you want the sweep only in certain parts (alpha mask)
        _UseMask("Use Alpha Mask (0/1)", Float) = 0
        _MaskTex("Mask Tex (alpha)", 2D) = "white" {}
        _MaskCutoff("Mask Cutoff", Range(0,1)) = 0.01
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="False" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            // For additive-look you can use: Blend One One (or change in material)
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            fixed4 _SweepColor;
            float _SweepIntensity;
            float _SweepWidth;
            float _SweepSpeed;
            float _Angle; // degrees
            float _Softness;

            float _UseMask;
            sampler2D _MaskTex;
            float _MaskCutoff;

            float2 RotateUV(float2 uv, float cosA, float sinA, float2 center)
            {
                float2 p = uv - center;
                float2 r;
                r.x = p.x * cosA - p.y * sinA;
                r.y = p.x * sinA + p.y * cosA;
                return r + center;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // base texture
                fixed4 baseCol = tex2D(_MainTex, i.uv) * i.color;
                // if base alpha is 0, early out (optional)
                if (baseCol.a <= 0.0001)
                    discard;

                // compute rotated UV so sweep direction aligns with angle
                float angleRad = radians(_Angle);
                float cosA = cos(angleRad);
                float sinA = sin(angleRad);

                // rotate around center (0.5,0.5)
                float2 rUV = RotateUV(i.uv, cosA, sinA, float2(0.5, 0.5));

                // sweep offset driven by time; maps rUV.x from 0..1
                float t = _Time.y * _SweepSpeed; // _Time.y is seconds
                // make sweep wrap around using frac
                float sweepPos = frac(t);

                // distance from sweep center (in rotated UV space)
                float d = rUV.x - sweepPos;

                // make band symmetric across wrap-around: choose minimal distance considering wrap
                d = min(abs(d), 1.0 - abs(d));

                // smoothstep based band shape
                float halfWidth = _SweepWidth * 0.5;
                float edgeSoft = _Softness;
                float band = smoothstep(halfWidth + edgeSoft, halfWidth - edgeSoft, d);

                // create a highlight gradient across band (optional falloff)
                // we can make the center brighter: use a pow to tighten center
                float centerFalloff = pow(1.0 - saturate(d / halfWidth), 1.5);

                // combined factor
                float sweepFactor = band * centerFalloff * _SweepIntensity;
                sweepFactor = saturate(sweepFactor);

                // final color: additively blend sweepColor over base, multiplied by base alpha
                fixed4 sweepCol = _SweepColor * sweepFactor;
                
                // optionally use mask to restrict sweep
                if (_UseMask > 0.5)
                {
                    float maskA = tex2D(_MaskTex, i.uv).a;
                    if (maskA <= _MaskCutoff) sweepCol.rgb = 0;
                    else sweepCol *= maskA;
                }

                // choose blending: additive on top of base, but respect base alpha
                fixed4 outCol;
                // preserve base alpha
                outCol.rgb = baseCol.rgb + sweepCol.rgb * sweepCol.a;
                outCol.a = baseCol.a;

                // tone down if alpha > 1
                outCol.rgb = outCol.rgb * (1.0 / max(1.0, outCol.a));

                return outCol;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}