Shader "Orange/Model/OrangeMapObject" {
	Properties {
		[TCP2HeaderHelp(BASE, Base Properties)] _MainTex ("Base (RGB)", 2D) = "white" {}
		[TCP2Separator] [Header(Masks)] [NoScaleOffset] _Mask2 ("Mask 2 (Emission)", 2D) = "black" {}
		[TCP2Separator] [TCP2HeaderHelp(EMISSION, Emission)] [HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
	}
	SubShader {
		LOD 150
		Tags { "RenderType" = "Opaque" }
		Pass {
			Name "FORWARD"
			LOD 150
			Tags { "LIGHTMODE" = "FORWARDBASE" "RenderType" = "Opaque" "SHADOWSUPPORT" = "true" }
			GpuProgramID 60679
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float3 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float4 texcoord5 : TEXCOORD5;
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
			sampler2D _MainTex;
			sampler2D _Mask2;
			
			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
