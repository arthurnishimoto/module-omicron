Shader "StereoscopicShader"
{
	Properties
	{
		_LeftTex("Left Eye Texture", 2D) = "white" {}
		_RightTex("Right Eye TTexture", 2D) = "white" {}
		_ResultTex("Result Texture", 2D) = "white" {}

		_RenderWidth("Render Width", Float) = 1920
		_RenderHeight("Render Height", Float) = 1080
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
	sampler2D _ResultTex;

	float4 _LeftTex_ST;
	float4 _RightTex_ST;

	uniform float _RenderWidth;
	uniform float _RenderHeight;

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
		// sample the texture
		//fixed4 col = tex2D(_LeftTex, i.uv);
		// apply fog
		//UNITY_APPLY_FOG(i.fogCoord, col);

		//return col;

		float3 left = tex2D(_LeftTex, i.uv).rgb;		// Sample scene texture
		float3 right = tex2D(_RightTex, i.uv).rgb;	// Sample scene texture

		int x = fmod(floor(i.uv.x*_RenderWidth) + floor(i.uv.y*_RenderHeight), 2) < 1;
		return float4(lerp(left, right, x), 0.0);


	}
		ENDCG
	}
	}
}
