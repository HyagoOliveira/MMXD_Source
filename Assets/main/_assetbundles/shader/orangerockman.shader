Shader "Orange/Model/OrangeRockman" {
	Properties {
		[TCP2HeaderHelp(BASE, Base Properties)] _Color ("Color", Vector) = (1,1,1,1)
		_HColor ("Highlight Color", Vector) = (0.785,0.785,0.785,1)
		_SColor ("Shadow Color", Vector) = (0.195,0.195,0.195,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		[TCP2Separator] [TCP2Header(RAMP SETTINGS)] _RampThreshold ("Ramp Threshold", Range(0, 1)) = 0.5
		_RampSmooth ("Ramp Smoothing", Range(0.001, 1)) = 0.1
		[TCP2Separator] [Header(Masks)] [NoScaleOffset] _Mask2 ("Mask 2 (Emission)", 2D) = "black" {}
		[TCP2Separator] [TCP2HeaderHelp(Intensity, Intensity)] _Intensity ("Intensity (Emission)", Range(0, 10)) = 2
		[TCP2Separator] [TCP2HeaderHelp(SPECULAR, Specular)] _SpecColor ("Specular Color", Vector) = (0.5,0.5,0.5,1)
		_Smoothness ("Smoothness", Range(0, 1)) = 0.336
		_SpecSmooth ("SpecSmooth", Range(0, 1)) = 1
		_GradientMax ("Gradient Max", Range(0, 1)) = 0.8
		_SpecColorTex ("Specular Color Texture", 2D) = "white" {}
		[TCP2Separator] [TCP2HeaderHelp(RIM, Rim)] _RimColor ("Rim Color", Vector) = (0.8,0.8,0.8,0.6)
		_RimMin ("Rim Min", Range(0, 1)) = 0.57
		_RimMax ("Rim Max", Range(0, 1)) = 0.98
		_RimDir ("Rim Direction", Vector) = (0,0,1,0)
		[TCP2Separator] [TCP2HeaderHelp(OUTLINE, Outline)] _OutlineColor ("Outline Color", Vector) = (0.2,0.2,0.2,1)
		_Outline ("Outline Width", Float) = 0.2
		[Toggle(TCP2_OUTLINE_TEXTURED)] _EnableTexturedOutline ("Color from Texture", Float) = 0
		[TCP2KeywordFilter(TCP2_OUTLINE_TEXTURED)] _TexLod ("Texture LOD", Range(0, 10)) = 5
		[Toggle(TCP2_OUTLINE_CONST_SIZE)] _EnableConstSizeOutline ("Constant Size Outline", Float) = 1
		[Toggle(TCP2_ZSMOOTH_ON)] _EnableZSmooth ("Correct Z Artefacts", Float) = 1
		[TCP2KeywordFilter(TCP2_ZSMOOTH_ON)] _ZSmooth ("Z Correction", Range(-3, 3)) = 3
		[TCP2KeywordFilter(TCP2_ZSMOOTH_ON)] _Offset1 ("Z Offset 1", Float) = 5
		[TCP2KeywordFilter(TCP2_ZSMOOTH_ON)] _Offset2 ("Z Offset 2", Float) = 0
		[TCP2OutlineNormalsGUI] __outline_gui_dummy__ ("unused", Float) = 0
		[TCP2Separator] [TCP2HeaderHelp(TRANSPARENCY)] [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendTCP2 ("Blending Source", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlendTCP2 ("Blending Dest", Float) = 10
		[TCP2Separator] [TCP2HeaderHelp(DISSOLVE)] [NoScaleOffset] _DissolveMap ("Dissolve Map", 2D) = "white" {}
		_DissolveValue ("Dissolve Value", Range(0, 1)) = 0.5
		[TCP2Gradient] _DissolveRamp ("Dissolve Ramp", 2D) = "white" {}
		_DissolveGradientWidth ("Ramp Width", Range(0, 1)) = 0.7
		[HDR] _DissolveEdge ("Dissolve Edge", Vector) = (0,1,0,1)
		_DissolveEdgeOffset ("Dissolve Edge Width", Range(0, 1)) = 0.1
		_DissolveModelHeight ("Dissolve Model Height", Range(0, 10)) = 1
		[TCP2Separator] [HideInInspector] __dummy__ ("unused", Float) = 0
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
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{ue" "SHADOWSUPPORT" = "true" }
			Blend Zero Zero, Zero Zero
			Stencil {
				Comp Always
				Pass Replace
				Fail Keep
				ZFail Keep
			}
			GpuProgramID 28587
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
				float3 texcoord3 : TEXCOORD3;
				float4 texcoord6 : TEXCOORD6;
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
			sampler2D _SpecColorTex;
			sampler2D _DissolveMap;
			sampler2D _DissolveRamp;
			
			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
