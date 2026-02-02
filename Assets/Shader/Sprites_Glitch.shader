Shader "Sprites/Glitch" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_GlitchInterval ("Glitch interval time [seconds]", Float) = 0.16
		_DispProbability ("Displacement Glitch Probability", Float) = 0.022
		_DispIntensity ("Displacement Glitch Intensity", Float) = 0.09
		_ColorProbability ("Color Glitch Probability", Float) = 0.02
		_ColorIntensity ("Color Glitch Intensity", Float) = 0.07
		[MaterialToggle] _WrapDispCoords ("Wrap disp glitch (off = clamp)", Float) = 1
		[MaterialToggle] _DispGlitchOn ("Displacement Glitch On", Float) = 1
		[MaterialToggle] _ColorGlitchOn ("Color Glitch On", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard nderType" = "Transparent" }
			Blend One OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			Cull Off
			Fog {
				Mode 0
			}
			GpuProgramID 2507
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
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
