#ifndef ANIM_TEXTURE_INCLUDE
#define ANIM_TEXTURE_INCLUDE



static const float COLOR_DEPTH = 255;

sampler2D _AnimTex;
float4 _AnimTex_Scale;
float4 _AnimTex_Offset;
float4 _AnimTex_AnimEnd;
float _AnimTex_T;
float _AnimTex_FPS;

half4 _AnimTex_TexelSize;


			
float3 AnimTexVertexPos(float3 vertex, float2 texcoord1, float t) {
	float frame = min(t * _AnimTex_FPS, _AnimTex_AnimEnd.y);
	#ifdef BILINEAR_OFF
	float frame1 = frame;
	#else
	float frame1 = floor(frame);
	float frame2 = min(frame1 + 1, _AnimTex_AnimEnd.y);
	float tFilter = frame - frame1;
	#endif
	
	float4 uv = 0;
	uv.xy = texcoord1 + float2(0, frame1 * _AnimTex_TexelSize.y);
	float3 pos1 = tex2Dlod(_AnimTex, uv).rgb;
	uv.y += 0.5;
	float3 pos2 = tex2Dlod(_AnimTex, uv).rgb;
	float3 pos = (pos1 + pos2 / COLOR_DEPTH) * _AnimTex_Scale.xyz + _AnimTex_Offset.xyz;
	
	#ifdef BILINEAR_OFF
	vertex.xyz += pos;
	#else
	uv.xy = texcoord1 + float2(0, frame2 * _AnimTex_TexelSize.y);
	pos1 = tex2Dlod(_AnimTex, uv).rgb;
	uv.y += 0.5;
	pos2 = tex2Dlod(_AnimTex, uv).rgb;
	pos2 = (pos1 + pos2 / COLOR_DEPTH) * _AnimTex_Scale.xyz + _AnimTex_Offset.xyz;
	
	vertex.xyz += lerp(pos, pos2, tFilter);
	#endif
	
	return vertex;
}

#endif