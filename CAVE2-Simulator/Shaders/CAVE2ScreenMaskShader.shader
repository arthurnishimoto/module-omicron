Shader "Unlit/CAVE2ScreenMaskShader"
{
	Properties
	{
		_Color ("Color", Color) = (0.19, 0.30, 0.47,1)
		[KeywordEnum(Less, Greater, LEqual, GEqual, Equal, NotEqual, Always)] _ZTest("Z Test", Float) = 2
		[KeywordEnum(Back, Front, Off)] _Cull("Culling", Float) = 0
	}
	SubShader{
		Tags{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
		}
		LOD 100
		ZTest [_ZTest]
		Lighting Off
		Cull [_Cull]
		ZWrite Off

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 col = _Color;
				return col;
			}
			ENDCG
		}
	}
}
