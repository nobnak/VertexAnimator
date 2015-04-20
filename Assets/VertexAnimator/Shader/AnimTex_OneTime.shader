Shader "VertexAnim/OneTime" {
	Properties {
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		
		_AnimTex ("PosTex", 2D) = "white" {}
		_AnimTex_Scale ("Scale", Vector) = (1,1,1,1)
		_AnimTex_Offset ("Offset", Vector) = (0,0,0,0)
		_AnimTex_AnimEnd ("End (Time, Frame)", Vector) = (0, 0, 0, 0)
		_AnimTex_T ("Time", float) = 0
		_AnimTex_FPS ("Frame per Sec(FPS)", Float) = 30
	}
	SubShader { 
		Tags { "RenderType"="Opaque" }
		LOD 700 Cull Off
		
		Pass {
			CGPROGRAM
			#pragma target 5.0
			#pragma multi_compile BILINEAR_OFF BILINEAR_ON
			#pragma vertex vert
			#pragma fragment frag
			#include "AnimTexture.cginc"

			struct vsin {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct vs2ps {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			sampler2D _MainTex;
			
			vs2ps vert(vsin IN) {
				float t = _Time.y - _AnimTex_T;
				t = max(0,min(_AnimTex_AnimEnd.x, t));
				float3 v = AnimTexVertexPos(IN.vertex, IN.texcoord1, t);
				
				vs2ps OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, float4(v, 1));
				OUT.uv = IN.texcoord;
				return OUT;
			}

			float4 frag(vs2ps IN) : COLOR {
				return tex2D(_MainTex, IN.uv);
			}
			ENDCG
		}
	}
	FallBack Off
}
