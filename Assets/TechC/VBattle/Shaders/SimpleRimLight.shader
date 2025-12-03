Shader "Custom/SimpleRimLight"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _RimColor ("Rim Light Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power (Sharpness)", Range(0.1, 10)) = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" 
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normalDir : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            float4 _MainColor;
            float4 _RimColor;
            float _RimPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // カメラ方向と法線のdotを取る
                float rim = 1.0 - saturate(dot(i.viewDir, normalize(i.normalDir)));

                // シャープさを調整
                rim = pow(rim, _RimPower);

                // メインカラーとリムライトカラーを合成
                fixed4 col = _MainColor + rim * _RimColor;
                col.a = 1.0; // アルファは常に1にする

                return col;
            }
            ENDCG
        }
    }
}
