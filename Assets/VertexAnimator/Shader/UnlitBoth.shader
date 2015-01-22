Shader "Custom/UnlitBoth" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Main Color", Color) = (0.5, 0.5, 0.5 , 1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200 Cull Off
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _Color;

			struct Input {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			Input vert(Input IN) {
				Input OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.uv = IN.uv;
				return OUT;
			}
			float4 frag(Input IN) : COLOR {
				return tex2D(_MainTex, IN.uv) * _Color;
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
