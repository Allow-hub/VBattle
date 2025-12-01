Shader "Custom/Trail_AlphaBlend"
{
    	Properties
	{
		_MainTex("Main Texture (RGBA)", 2D) = "white" {}
		_MainTexAPow("MainTex Alpha Gamma", Float) = 1
		_SoftAlpha("Soft Alpha", Range(0, 1)) = 1
		_TintTex("Tint Texture (RGB)", 2D) = "white" {}
		_MainScrollSpeedU("Main Scroll U Speed", Float) = 10
		_MainScrollSpeedV("Main Scroll V Speed", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha // 一般的な透過ブレンド
		ZWrite Off

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
				half4 color : COLOR;
			};

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				half4 color : COLOR;
			};

			sampler2D _MainTex;
			sampler2D _TintTex;

			CBUFFER_START(UnityPerMaterial)
				half4 _MainTex_ST;
				half _MainTexAPow;
				half _SoftAlpha;
				half _MainScrollSpeedU;
				half _MainScrollSpeedV;
				
				// MoveToMaterialUV スクリプトから渡される UV オフセット値。
				// これは「プロパティが存在するかどうか」をチェックしないようにしている。
				// もし存在チェックを行うとマテリアルのバージョンが毎フレーム更新されたとみなされてしまい、エディタ上で常にリビルドされてしまうため。
				half _MoveToMaterialUV;
			CBUFFER_END

			Varyings vert(Attributes IN)
			{
				Varyings o;
				o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
				o.uv.x -= frac(_Time.x * _MainScrollSpeedU) + _MoveToMaterialUV;
				o.uv.y -= frac(_Time.x * _MainScrollSpeedV);
				o.color = IN.color;
				return o;
			}

			half4 frag(Varyings IN) : SV_Target
			{
				half4 mainTex = tex2D(_MainTex, IN.uv);

				// アルファ処理
				mainTex.a = pow(abs(mainTex.a), _MainTexAPow);
				half toonAlpha = saturate((mainTex.a - (1 - IN.color.a)) / _SoftAlpha);
				half alpha = mainTex.a * IN.color.a;
				half alphaMix = lerp(alpha, toonAlpha, IN.color.a);

				// アルファ値をテクスチャ座標にして Tint テクスチャから色を取得
				half4 tintCol = tex2D(_TintTex, half2(alphaMix, 0.5));
				
				half4 col;
				col.rgb = lerp(tintCol.rgb * mainTex.rgb, tintCol.rgb, IN.color.a);
				col.rgb *= IN.color.rgb;
				col.a = alphaMix;

				return col;
			}
			ENDHLSL
		}
	}
}