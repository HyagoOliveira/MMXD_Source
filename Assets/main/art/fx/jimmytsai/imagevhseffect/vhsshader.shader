Shader "Custom/VHSShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_VhsVSpeed ("VHS V Speed", Float) = 1
		_VhsHSpeed ("VHS H Speed", Float) = 1
		_VhsVOffect ("VHS V Offset", Float) = 64
		[MaterialToggle] _VHSOn ("VHS Effect On", Float) = 1
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		_NoiseXSpeed ("Noise X Speed", Float) = 100
		_NoiseYSpeed ("Noise Y Speed", Float) = 100
		_NoiseCutoff ("Noise Cutoff", Range(0, 1)) = 0
		_DistortionSrength ("Distortion Strength", Float) = 1
	}
	SubShader {
		Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off
			Fog {
				Mode 0
			}
			GpuProgramID 42637
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR0;
				float4 position : SV_POSITION0;
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
			
			// Keywords: DUMMY
			v2f vert(appdata_full v)
			{
