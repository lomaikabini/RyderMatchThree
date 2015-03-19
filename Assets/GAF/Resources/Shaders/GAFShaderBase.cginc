#ifndef GAF_SHADER_BASE_INCLUDED
#define GAF_SHADER_BASE_INCLUDED

#include "UnityCG.cginc"

////////////////////////////
// DEFINITIONS
////////////////////////////

sampler2D 			_MainTex;
float4 				_MainTex_ST;
uniform float4		_MainTex_TexelSize;

float4		_TintColor;
float4		_TintColorOffset;
fixed4		_CustomColor;

////////////////////////////
// STRUCTS
////////////////////////////

struct gaf_v2f_minimal
{
	float4 position		: SV_POSITION;
	float2 texcoord		: TEXCOORD0;
};

struct gaf_v2f_base
{
	float4 position		: SV_POSITION;
	float2 texcoord		: TEXCOORD0;
	fixed4 color		: COLOR;
	fixed4 colorOffset	: TEXCOORD1;
};

////////////////////////////
// FUNCTIONS VERTEX
////////////////////////////

gaf_v2f_base gaf_base_vert_group (appdata_full input)
{
	gaf_v2f_base output;

#if GAF_VERTICES_TRANSFORM_ON
	output.position = mul (UNITY_MATRIX_MVP, input.vertex);
#else
	output.position = input.vertex;

#if !UNITY_UV_STARTS_AT_TOP
    output.position.y = -output.position.y;
#endif // UNITY_UV_STARTS_AT_TOP

#endif // GAF_VERTICES_TRANSFORM_ON

	output.texcoord		= TRANSFORM_TEX(input.texcoord, _MainTex);
	output.color		= input.color;
	output.colorOffset	= input.tangent;

	return output;
}

gaf_v2f_base gaf_base_vert(appdata_base input)
{
	gaf_v2f_base output;

	output.position		= mul (UNITY_MATRIX_MVP, input.vertex);
	output.texcoord		= TRANSFORM_TEX(input.texcoord, _MainTex);
	output.color		= _TintColor;
	output.colorOffset	= _TintColorOffset;

	return output;
}

gaf_v2f_minimal gaf_minimal_vert(appdata_base input)
{
	gaf_v2f_minimal output;
 
	output.position = mul(UNITY_MATRIX_MVP, input.vertex);
	output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
 
	return output;
}

////////////////////////////
// FUNCTIONS FRAGMENT
////////////////////////////

fixed4 gaf_frag(gaf_v2f_base input)
{
	return (tex2D(_MainTex, input.texcoord) * input.color + input.colorOffset) * _CustomColor;
}

fixed4 gaf_base_frag (gaf_v2f_base input) : SV_Target
{
	return gaf_frag(input);
}

fixed4 gaf_mask_frag (gaf_v2f_minimal input) : SV_Target
{
	fixed4 resultColor = tex2D(_MainTex, input.texcoord);

	if (resultColor.a < 0.01)
	{
		discard;
	}

	return resultColor;
}

#endif
