Shader "Custom/ParticleSpheres" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_CutoffMask ("Cutoff mask", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Speed ("Speed", Float) = 1
		_Amount ("Amount", Float) = 5
		_Distance ("Distance", Float) = 0.1
		_MeltStrength ("Melt strength", Float) = 1
		_MeltStartHeight ("Melt start height", Float) = 0.25
		[Space(20)] _CutoffCube ("Cutoff cube", Cube) = "" {}
		_CutoffThreshold ("Cutoff threshold", Float) = 1
		[Space(20)] _BulgeStrength ("Bulge strength", Float) = 1
		_OffsetStrength ("Offset strength", Float) = 1
		_NoiseTex ("_NoiseTex", 2D) = "white" {}
		_Tiling ("Noise Tiling", Vector) = (1,1,1,1)
		_NoiseSpeed ("_NoiseSpeed", Vector) = (1,1,1,1)
		_CollapseStrength ("Collapse Strength", Range(-5, 5)) = 0.5
		_Gravity ("Gravity Strength", Float) = -9.81
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROD 200
			Tags { "LIGHTMODE" = "FORWARDBASE" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			GpuProgramID 22097
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord5 : TEXCOORD5;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;
				float3 texcoord6 : TEXCOORD6;
				float4 texcoord8 : TEXCOORD8;
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
			sampler2D _CutoffMask;
			
			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
