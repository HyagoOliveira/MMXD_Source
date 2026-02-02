Shader "KY/add_two" {
	Properties {
		_MainTex ("MainTex", 2D) = "white" {}
		_emissive ("emissive", Float) = 1
		_baseTexSpdX ("baseTexSpdX", Float) = 0
		_baseTexSpdY ("baseTexSpdY", Float) = 0
		_baseTexDensity ("baseTexDensity", Float) = 1
		[MaterialToggle] _useTexColor ("useTexColor", Float) = 0
		_alphaDensity ("alphaDensity", Float) = 5
		_vertColorDensity ("vertColorDensity", Float) = 1
		_alphaPower ("alphaPower", Float) = 1
		[MaterialToggle] _haveAlpha ("haveAlpha", Float) = 0
		[MaterialToggle] _notUseRch ("notUseRch", Float) = 0
		[MaterialToggle] _notUseGch ("notUseGch", Float) = 0
		[MaterialToggle] _notUseBch ("notUseBch", Float) = 0
		_channelDivid ("channelDivid", Float) = 3
		[MaterialToggle] _useDepthBlend ("useDepthBlend", Float) = 1
		_depthBlend ("depthBlend", Float) = 1
		[MaterialToggle] _notUseFresnel ("notUseFresnel", Float) = 1
		_fresPower ("fresPower", Float) = 1
		[MaterialToggle] _fresInv ("fresInv", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Onsparent" "RenderType" = "Transparent" }
		Pass {
			Name "FORWARD"
			Tags { "IGNOREPROJECTOR" = "true" "LIGHTMODE" = "FORWARDBASE" "QUEUE" = "Transparent" "RenderType" = "Transparent" "SHADOWSUPPORT" = "true" }
			Blend One One, One One
			ZWrite Off
			Cull Off
			GpuProgramID 51798
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2;
				float4 color : COLOR0;
				float4 texcoord3 : TEXCOORD3;
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
