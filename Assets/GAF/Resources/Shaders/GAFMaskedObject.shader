Shader "GAF/GAFMaskedObject"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_StencilID ("Stencil ID", Float) = 0
		_TintColor ("Per vertex color", Color) = (1, 1, 1, 1)
		_TintColorOffset ("Per vertex color offset", Vector) = (0, 0, 0, 0)
		_CustomColor ("Custom Color", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Tags 
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
		}
		
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Zwrite Off
		Lighting Off
		
		Stencil
		{
			Ref [_StencilID]
			Comp equal
			Pass keep
			Fail keep
		}
		
		Pass 
		{
			CGPROGRAM
			
			#include "GAFShaderBase.cginc"

			#pragma vertex gaf_base_vert
			#pragma fragment gaf_base_frag

			ENDCG
		}
	}
}
