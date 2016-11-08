Shader "Sprites/Diffuse Billboard"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert nofog keepalpha
		#pragma multi_compile _ PIXELSNAP_ON
		#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
		
		sampler2D _MainTex;
		fixed4 _Color;
		sampler2D _AlphaTex;


		struct appdata_t {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			float3 normal : NORMAL;
		};
		
		struct Input
		{
			float2 texcoord : TEXCOORD0;
			fixed4 color;
			float4 vertex : SV_POSITION;
		};
		
		float4 _MainTex_ST;
		
		void vert (inout appdata_t v, out Input o)
		{
			#if defined(PIXELSNAP_ON)
			v.vertex = UnityPixelSnap (v.vertex);
			#endif
			
			UNITY_INITIALIZE_OUTPUT(Input, o);
			// Billboard with scale
			float scaleX = length(mul(_Object2World, float4(1.0, 0.0, 0.0, 0.0)));
			float scaleY = length(mul(_Object2World, float4(0.0, 1.0, 0.0, 0.0)));
			o.vertex = mul(UNITY_MATRIX_P, 
			  mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
			  - float4(v.vertex.x * scaleX, v.vertex.y * scaleY, 0.0, 0.0));
			  
			o.color = v.color * _Color;
			o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
		}

		fixed4 SampleSpriteTexture (float2 uv)
		{
			fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
			color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

			return color;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
			o.Albedo = c.rgb * c.a;
			o.Alpha = c.a;
		}
		ENDCG
	}

Fallback "Transparent/VertexLit"
}
