Shader "KY/add_noise_two" {
	Properties {
		_MainTex ("MainTex", 2D) = "white" {}
		_noseTex ("noseTex", 2D) = "white" {}
		_texPower ("texPower", Float) = 2
		_baseTexU ("baseTexU", Float) = 1
		_baseTexV ("baseTexV", Float) = 1
		_baseTexSpdX ("baseTexSpdX", Float) = 0
		_baseTexSpdY ("baseTexSpdY", Float) = 0
		_noiseTexU ("noiseTexU", Float) = 1
		_noiseTexV ("noiseTexV", Float) = 1
		_noiseTexSpdX ("noiseTexSpdX", Float) = 0
		_noiseTexSpdY ("noiseTexSpdY", Float) = 0
		_noiseDensity ("noiseDensity", Float) = 1
		_noisePower ("noisePower", Float) = 0.2
		_vertAlphaDensity ("vertAlphaDensity", Float) = 1
		_baseTexDensity ("baseTexDensity", Float) = 2
		[MaterialToggle] _useTexColor ("useTexColor", Float) = 0
		_UVcompY ("UVcompY", Float) = 1
		[MaterialToggle] _compYInv ("compYInv", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};sparent" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			ZWrite Off
			Cull Off
			GpuProgramID 54029
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
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
			
			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
