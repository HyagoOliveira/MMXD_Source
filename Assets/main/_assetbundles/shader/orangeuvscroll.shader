Shader "Orange/UVScroll" {
	Properties {
		_MainTex ("Base layer (RGB)", 2D) = "white" {}
		_ScrollX ("Base layer Scroll speed X", Float) = 1
		_ScrollY ("Base layer Scroll speed Y", Float) = 0
		_Intensity ("Intensity", Float) = 1
		_Alpha ("Alpha", Range(0, 1)) = 1
	}
	SubShader {
		LOD 150
		Tags { "RenderType" = "Opaque" }
		Pass {
			LOD 100
			Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			Fog {
				Mode 0
			}
			GpuProgramID 25676
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
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
			
			// Keywords: LIGHTMAP_OFF
			v2f vert(appdata_full v)
			{
