Shader "FX/AlphaFade" {
	Properties {
		_Color ("Color", Color) = (0,1,1,1)
		_AlphaTexture ("Alpha Texture", 2D) = "white" {}
		_Tiling ("Tiling", Float) = 3
		_Edge ("Edge", Float) = 0.1
		_Threshold ("Threshold", Float) = 3
		_ScrollSpeed ("Scroll Speed", Range(0, 5)) = 1
		_Intensity ("Intensity", Float) = 0.5
		_GlitchSpeed ("Glitch Speed", Range(0, 50)) = 50
		_GlitchIntensity ("Glitch Intensity", Range(0, 0.1)) = 0
	}
	SubShader {
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Overlay" "RenderType" = "Transparent" }
		Pass {
			Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Overlay" "RenderType" = "Transparent" }
			Blend SrcAlpha One, SrcAlpha One
			GpuProgramID 33828
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float texcoord2 : TEXCOORD2;
				float3 texcoord1 : TEXCOORD1;
				float3 normal : NORMAL0;
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
