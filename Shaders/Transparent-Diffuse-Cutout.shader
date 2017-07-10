// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Transparent/Cutout Billboard"
{
	Properties {
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
	}

	Category {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off
		
		SubShader {
			Pass {
			
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				fixed4 _TintColor;
				
				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
				};
				
				float4 _MainTex_ST;

				v2f vert (appdata_t v)
				{
					v2f o;
					//o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

					// Billboard with scale
					float scaleX = length(mul(unity_ObjectToWorld, float4(1.0, 0.0, 0.0, 0.0)));
					float scaleY = length(mul(unity_ObjectToWorld, float4(0.0, 1.0, 0.0, 0.0)));
					o.vertex = mul(UNITY_MATRIX_P, 
					 mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
					 - float4(v.vertex.x * scaleX, v.vertex.y * -scaleY, 0.0, 0.0));

					o.color = v.color;
					o.texcoord = v.texcoord;
					
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = i.color * _TintColor * tex2D(_MainTex, i.texcoord.xy);

					return col;
				}
				ENDCG 
			}
		}	
	}
}
