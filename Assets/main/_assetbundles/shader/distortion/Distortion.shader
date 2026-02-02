Shader "Distortion" {
	Properties {
		_Refraction ("Refraction", Range(0, 10)) = 1
		_Power ("Power", Range(1, 10)) = 1
		_AlphaPower ("Vertex Alpha Power", Range(1, 10)) = 1
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_Cull ("Face Culling", Float) = 2
	}
	SubShader {
		Tags { "QUEUE" = "Transparent+1" }
		GrabPass {
			"_GrabTexture"
		}
		Pass {
			Tags { "QUEUE" = "Transparent+1" }
			Cull Off
			GpuProgramID 11021
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 texcoord : TEXCOORD0;
				float4 position : SV_POSITION0;
				float4 color : COLOR0;
				float2 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
				float3 texcoord4 : TEXCOORD4;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
