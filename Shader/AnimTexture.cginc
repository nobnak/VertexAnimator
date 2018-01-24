// Upgrade NOTE: upgraded instancing buffer 'MyProps' to new syntax.

#ifndef ANIM_TEXTURE_INCLUDE
#define ANIM_TEXTURE_INCLUDE


static const float COLOR_DEPTH = 255;
static const float COLOR_DEPTH_INV = 1.0 / COLOR_DEPTH;

sampler2D _AnimTex;
sampler2D _AnimTex_NormalTex;
float4 _AnimTex_Scale;
float4 _AnimTex_Offset;
float4 _AnimTex_AnimEnd;
float _AnimTex_FPS;

half4 _AnimTex_TexelSize;
half4 _AnimTex_NormalTex_TexelSize;

#if defined(INSTANCING_ON)
UNITY_INSTANCING_BUFFER_START(MyProps)
UNITY_DEFINE_INSTANCED_PROP(float, _AnimTex_T)
#define _AnimTex_T_arr MyProps
UNITY_INSTANCING_BUFFER_END(MyProps)
#else
float _AnimTex_T;
#endif


float3 AnimTexVertexPos_Bilinear(uint vid, float t) {
    float frame = min(t * _AnimTex_FPS, _AnimTex_AnimEnd.y);
    float frame1 = frame;

    float2 uv = 0;
    uv.xy = (0.5 + float2(vid, frame1)) * _AnimTex_TexelSize;
    float3 pos1 = tex2Dlod(_AnimTex, float4(uv, 0, 0)).rgb;
    uv.y += 0.5;
    float3 pos2 = tex2Dlod(_AnimTex, float4(uv, 0, 0)).rgb;
    float3 pos = (pos1 + pos2 * COLOR_DEPTH_INV) * _AnimTex_Scale.xyz + _AnimTex_Offset.xyz;
    
    return pos;
}
float3 AnimTexVertexPos_Point(uint vid, float t) {
    float frame = min(t * _AnimTex_FPS, _AnimTex_AnimEnd.y);
    float frame1 = floor(frame);
    float frame2 = min(frame1 + 1, _AnimTex_AnimEnd.y);
    float tFilter = frame - frame1;

    float4 uv = 0;
    uv.xy = (0.5 + float2(vid, frame1)) * _AnimTex_TexelSize;
    float3 pos1 = tex2Dlod(_AnimTex, uv).rgb;
    uv.y += 0.5;
    float3 pos2 = tex2Dlod(_AnimTex, uv).rgb;
    float3 pos = (pos1 + pos2 * COLOR_DEPTH_INV) * _AnimTex_Scale.xyz + _AnimTex_Offset.xyz;
    
    uv.xy = (0.5 + float2(vid, frame2)) * _AnimTex_TexelSize;
    pos1 = tex2Dlod(_AnimTex, uv).rgb;
    uv.y += 0.5;
    pos2 = tex2Dlod(_AnimTex, uv).rgb;
    pos2 = (pos1 + pos2 / COLOR_DEPTH) * _AnimTex_Scale.xyz + _AnimTex_Offset.xyz;
    
    return lerp(pos, pos2, tFilter);
}			
float3 AnimTexVertexPos(uint vid, float t) {
	#ifdef BILINEAR_ON
    return AnimTexVertexPos_Point(vid, t);
	#else
    return AnimTexVertexPos_Bilinear(vid, t);
	#endif
}

float3 AnimTexNormal(uint vid, float t) {
    float frame = min(t * _AnimTex_FPS, _AnimTex_AnimEnd.y);
    float frame1 = frame;

    float2 uv = 0;
    uv.xy = (0.5 + float2(vid, frame1)) * _AnimTex_NormalTex_TexelSize;
    float3 n1 = tex2Dlod(_AnimTex_NormalTex, float4(uv, 0, 0)).rgb;
    uv.y += 0.5;
    float3 n2 = tex2Dlod(_AnimTex_NormalTex, float4(uv, 0, 0)).rgb;
    float3 n = 2.0 * (n1 + n2 * COLOR_DEPTH_INV) - 1.0;

    return n;
}
#endif
