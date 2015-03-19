Shader "GAF/GAFMaskObject"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_StencilID ("Stencil ID", Float) = 0
	}

	SubShader
	{
		Tags 
		{
			"Queue"="Transparent"
		}

		ColorMask 0
		ZWrite off
		Cull off
		
		Stencil 
		{
			Ref [_StencilID]
			Comp always
			Pass replace
		}

		Pass
		{
			CGPROGRAM 
			
			#include "GAFShaderBase.cginc"

			#pragma vertex gaf_minimal_vert  
			#pragma fragment gaf_mask_frag 
 
			ENDCG
		}
	}
}
