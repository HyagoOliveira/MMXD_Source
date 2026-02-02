Shader "Effects/WeaponFX/DistortPerlinMobileEx" {
	Properties {
		_TintColor ("Main Color", Color) = (1,1,1,1)
		_RimColorA ("Rim ColorA", Color) = (1,1,1,0.5)
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_PerlinNoise ("Perlin Noise Map (r)", 2D) = "white" {}
		_DropWavesScale ("Waves Scale (X) Height (YZ) Time (W)", Vector) = (1,1,1,1)
		_NoiseScale ("Noize Scale (XYZ) Height (W)", Vector) = (1,1,1,0.2)
		_Speed ("Distort Direction Speed (XY)", Vector) = (1,0,0,0)
		_FPOW ("FPOW Fresnel", Float) = 5
		_R0 ("R0 Fresnel", Float) = 0.05
		_BumpAmt ("Distortion Scale", Float) = 10
	}
	SubShader {
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		GrabPass {
			"_GrabTexture"
		}
		Pass {
			Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			GpuProgramID 2845
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 sv_position : SV_Position0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float3 texcoord3 : TEXCOORD3;
				float4 color : COLOR0;
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
