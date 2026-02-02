Shader "Effects/WeaponFX/Ice" {
	Properties {
		_TintColor ("Main Color", Color) = (1,1,1,1)
		_RimColor ("Rim Color", Color) = (1,1,1,0.5)
		_MainTex ("MainTex (r)", 2D) = "black" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_HeightMap ("_HeightMap (r)", 2D) = "black" {}
		_Height ("_Height", Float) = 0.1
		_FPOW ("FPOW Fresnel", Float) = 5
		_R0 ("R0 Fresnel", Float) = 0.05
		_BumpAmt ("Distortion", Float) = 10
		_IceStrength ("Ice Strength", Float) = 2
	}
	SubShader {
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		GrabPass {
		}
		Pass {
			Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Offset -1, -1
			GpuProgramID 26718
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 sv_position : SV_Position0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float2 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
				float4 color : COLOR0;
				float3 texcoord5 : TEXCOORD5;
				float3 texcoord6 : TEXCOORD6;
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
