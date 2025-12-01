Shader "Custom/MoveTrail_Add"
{
    Properties
	{
		_MainTex("Main Texture (RGB)", 2D) = "white" {}
		_MainTexVFade("MainTex V Fade", Range(0, 1)) = 0
		_MainTexVFadePow("MainTex V Fade Pow", Float) = 1
		_MainTexPow("Main Texture Gamma", Float) = 1
		_MainTexMultiplier("Main Texture Multiplier", Float) = 1
		_TintTex("Tint Texture (RGB)", 2D) = "white" {}
		_Multiplier("Multiplier", Float) = 1
		_MainScrollSpeedU("Main Scroll U Speed", Float) = 10
		_MainScrollSpeedV("Main Scroll V Speed", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"}
		Blend One One // 加算合成
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
				float2 uv : TEXCOORD0;
				float2 uvOrigin : TEXCOORD1; // 元の UV
				float4 positionHCS : SV_POSITION;
				half4 color : COLOR;
			};

			sampler2D _MainTex;
			sampler2D _TintTex;

			CBUFFER_START(UnityPerMaterial)
				half4 _MainTex_ST;
				half _MainTexVFade;
				half _MainTexVFadePow;
				half _MainTexPow;
				half _MainTexMultiplier;
				half _Multiplier;
				half _MainScrollSpeedU;
				half _MainScrollSpeedV;
				
				// MoveToMaterialUV スクリプトから受け取る UV スクロール値。
				// Material にプロパティが存在しない場合もある。
				// （シェーダーグラフで Expose されていたら SavedProperty に残ってしまい、Dirty 状態になることがある）
				half _MoveToMaterialUV;
			CBUFFER_END

			Varyings vert(Attributes IN)
			{
				Varyings o;
				o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
				o.uv.x -= frac(_Time.x * _MainScrollSpeedU) + _MoveToMaterialUV;
				o.uv.y -= frac(_Time.x * _MainScrollSpeedV);
				o.uvOrigin = IN.uv;
				o.color = IN.color;
				return o;
			}

			half4 frag(Varyings IN) : SV_Target
			{
				half4 mainTex = tex2D(_MainTex, IN.uv);

				// 縦方向のフェード処理
				half vFade = 1 - abs(IN.uvOrigin.y - 0.5) * 2; // 中央からの距離を反映
				vFade = pow(abs(vFade), _MainTexVFadePow); // 中央寄りを強調したり、均等にしたり
				vFade = lerp(1, vFade, _MainTexVFade);
				mainTex.rgb *= vFade; // メインテクスチャにフェードを反映
				mainTex.rgb = pow(abs(mainTex.rgb), _MainTexPow) * _MainTexMultiplier; // ガンマ補正＆乗算
				
				// カラーの強度。Trail のアルファも乗算。
				half intensity = _Multiplier * IN.color.a;

				// Tint テクスチャの参照
				half avr = mainTex.r * 0.3333 + mainTex.g * 0.3334 + mainTex.b * 0.3333;
				avr = saturate(avr * intensity); // 強度が 1 の時にちょうど 1 になるように調整
				half4 col = tex2D(_TintTex, half2(avr, 0.5));

				// 強度が 1 未満なら 1 に固定。1 より大きい場合は HDR 表現に拡張。
				half intensityHigh = max(1, intensity);
				col.rgb *= intensityHigh * IN.color.rgb;
				return col;
			}
			ENDHLSL
		}
	}
}