Shader "StereoscopicShader"
{
	Properties
	{
		_LeftTex("Left Eye Texture", 2D) = "white" {}
		_RightTex("Right Eye Texture", 2D) = "white" {}

		_RenderWidth("Render Width", Float) = 1366
		_RenderHeight("Render Height", Float) = 768

		[KeywordEnum(LeftOnly, RightOnly, Interleaved, Checkerboard)] _StereoMode("Stereo mode", Float) = 0
		_InvertEyes("Invert Eyes", Float) = 0
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
			};

			sampler2D _LeftTex;
			sampler2D _RightTex;

			float4 _LeftTex_ST;
			float4 _RightTex_ST;

			uniform float _RenderWidth;
			uniform float _RenderHeight;
			uniform float _StereoMode;
			uniform float _InvertEyes;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _LeftTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// Sample scene texture
				float3 left = tex2D(_LeftTex, i.uv).rgb;
				float3 right = tex2D(_RightTex, i.uv).rgb;

				if (_StereoMode == 0)
				{
					// Left Only
					return float4(left, 1.0f);
				}
				else if (_StereoMode == 1)
				{
					// Right Only
					return float4(right, 1.0f);
				}
				else if (_StereoMode == 2)
				{
					// Interleaved
					if ((i.uv.y *_RenderHeight) % 2.0f > 1)
					{
						if(_InvertEyes == 1)
							return float4(right, 1.0f);
						else
							return float4(left, 1.0f);
					}
					else
					{
						if (_InvertEyes == 1)
							return float4(left, 1.0f);
						else
							return float4(right, 1.0f);
					}
				}
				else if (_StereoMode == 3)
				{
					// Checkerboard
					int x = fmod(floor(i.uv.x*_RenderWidth) + floor(i.uv.y*_RenderHeight), 2) < 1;
					float4 color = float4(lerp(_InvertEyes == 0 ? left : right, _InvertEyes == 0 ? right : left, x), 0.0);
					return color;
				}

				// Left Only
				return float4(left, 1.0f);
			}
			ENDCG
		}
	}
}
