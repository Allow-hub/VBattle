Shader "UI/ExplodeVoronoi"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Progress("Progress", Range(0,1)) = 0
        _Cells("FragmentCount (per axis)", Float) = 20
        _Amplitude("Move Amount", Float) = 0.6
        _Spread("Spread Factor", Float) = 1.2
        _Rotation("Rotation Strength", Float) = 6.0
        _EdgeSoft("Edge Softness", Float) = 0.1
        _Center("Explosion Center (UV)", Vector) = (0.5,0.5,0,0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Progress;
            float _Cells;
            float _Amplitude;
            float _Spread;
            float _Rotation;
            float _EdgeSoft;
            float4 _Center; // .xy used

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }

            // simple hash -> 0..1
            static float hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            // 2D random vector from cell coords
            static float2 rand2(float2 p)
            {
                float n = hash12(p);
                float a = n * 6.28318530718; // 2pi
                return float2(cos(a), sin(a));
            }

            // rotate vector by angle
            static float2 rot(float2 v, float a)
            {
                float ca = cos(a);
                float sa = sin(a);
                return float2(v.x * ca - v.y * sa, v.x * sa + v.y * ca);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // preserve original texture coords for sampling later
                float2 baseUV = uv;

                // grid cell coordinates
                float2 grid = uv * _Cells;
                float2 cell = floor(grid);
                float2 cellFrac = frac(grid);

                // jitter the cell center to make irregular fragments
                // jitter amplitude [0..0.5)
                float jitterSeedX = hash12(cell + float2(0.12, 0.34));
                float jitterSeedY = hash12(cell + float2(7.89, 4.56));
                float2 jitter = (float2(jitterSeedX, jitterSeedY) - 0.5) * 0.8; // tweak

                // fragment center in UV
                float2 fragCenterUV = (cell + 0.5 + jitter) / _Cells;

                // unique id-ish: pack cell coords to float2 seed for randomness
                float2 cellSeed = cell;

                // distance from explosion center to this fragment's center
                float2 centerUV = _Center.xy;
                float distCenter = distance(fragCenterUV, centerUV);

                // control when this fragment starts moving: the closer to center, earlier it moves
                // compute threshold based on distance and _Spread
                float startThreshold = distCenter * _Spread;

                // progressLocal: 0..1 indicates how much this fragment has been pushed
                float fragmentActive = saturate((_Progress - startThreshold) / (1.0 - startThreshold));
                // small curve
                fragmentActive = smoothstep(0.0, 1.0, fragmentActive);

                // direction for movement (randomized per fragment)
                float2 rndDir = rand2(cellSeed);

                // movement amount scaled by amplitude and eased
                float moveAmount = _Amplitude * pow(fragmentActive, 0.6);

                // rotation
                float angle = (_Progress * _Rotation) * (hash12(cellSeed + 1.234) * 2 - 1);

                // compute uv offset (we offset the sample UV to move fragment)
                float2 uvOffset = rndDir * moveAmount;
                // small micro jitter based on cellFrac to avoid perfect tiling
                uvOffset += (cellFrac - 0.5) * 0.02;

                // apply rotation around fragment center in UV-space
                float2 local = baseUV - fragCenterUV;
                local = rot(local, angle * fragmentActive);
                float2 movedUV = fragCenterUV + local + uvOffset * fragmentActive;

                // sample texture with moved UV
                fixed4 col = tex2D(_MainTex, movedUV);

                // optionally fade edge of fragment to avoid hard seams
                // create mask by distance from local to original cell extents
                // approximate edge factor using length of cellFrac-0.5
                float edge = length(cellFrac - 0.5) * 2.0;
                float edgeMask = smoothstep(1.0 - _EdgeSoft, 1.0, edge); // 0 inside, 1 near edge
                // we want edges to get transparent earlier as they fly away
                float fade = lerp(1.0, 0.0, fragmentActive * (0.6 + edgeMask * 0.6));
                col.a *= fade;

                // overall fade-out with progress so whole image disappears at end
                col.a *= 1.0 - smoothstep(0.9, 1.0, _Progress);

                return col;
            }
            ENDCG
        }
    }
}