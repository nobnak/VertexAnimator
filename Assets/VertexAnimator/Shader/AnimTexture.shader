Shader "VertexAnim/oneshot" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_AnimTex ("PosTex", 2D) = "white" {}
	_Scale ("scale", Vector) = (1,1,1,1)
	_Offset ("Offset", Vector) = (0,0,0,0)
	_AnimEnd ("End (Time, Frame)", Vector) = (0, 0, 0, 0)
	_T ("Time", float) = 0
	_FPS ("Frame per Sec(FPS)", Float) = 30
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 700 Cull Off
	
		CGPROGRAM
		#pragma multi_compile BILINEAR_OFF BILINEAR_ON
		#pragma surface surf Unlit vertex:vert
		#pragma target 5.0

		sampler2D _MainTex;
		sampler2D _AnimTex;
		float4 _Scale;
		float4 _Offset;
		float4 _AnimEnd;
		float _T;
		float _FPS;

		half4 _AnimTex_TexelSize;

		struct appdata {
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
		};

		struct Input {
			float2 uv_MainTex;
			float time;
		};
		
		 fixed4 LightingUnlit(SurfaceOutput s, fixed3 lightDir, fixed atten) {
		         fixed4 c;
		         c.rgb = s.Albedo; 
		         c.a = s.Alpha;
		         return c;
	     }	

		void vert(inout appdata v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			
			float t = _Time.y - _T;
			t = max(0,min(_AnimEnd.x, t));
			o.time = t;
			
			float frame = min(t * _FPS, _AnimEnd.y);
			#ifdef BILINEAR_OFF
			float frame1 = frame;
			#else
			float frame1 = floor(frame);
			float frame2 = min(frame1 + 1, _AnimEnd.y);
			float tFilter = frame - frame1;
			#endif
			
			float4 uv = 0;
			uv.xy = v.texcoord1 + float2(0, frame1 * _AnimTex_TexelSize.y);
			float3 pos1 = tex2Dlod(_AnimTex, uv).rgb;
			uv.y += 0.5;
			float3 pos2 = tex2Dlod(_AnimTex, uv).rgb;
			float3 pos = (pos1 + pos2 / 256.0) * _Scale.xyz + _Offset.xyz;
			
			#ifdef BILINEAR_OFF
			v.vertex.xyz += pos;
			#else
			uv.xy = v.texcoord1 + float2(0, frame2 * _AnimTex_TexelSize.y);
			pos1 = tex2Dlod(_AnimTex, uv).rgb;
			uv.y += 0.5;
			pos2 = tex2Dlod(_AnimTex, uv).rgb;
			pos2 = (pos1 + pos2 / 256.0) * _Scale.xyz + _Offset.xyz;
			
			v.vertex.xyz += lerp(pos, pos2, tFilter);
			#endif
		}

		void surf (Input IN, inout SurfaceOutput o) {
			float4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Emission = c.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
