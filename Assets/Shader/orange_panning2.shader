Shader "orange/panning2" {
	Properties {
		_MainTex_Color ("MainTex_Color", Color) = (1,0.1530337,0.004716992,1)
		_MainTex ("MainTex", 2D) = "white" {}
		_x_speed_maintex ("x_speed_maintex", Float) = -0.8
		_y_speed_maintex ("y_speed_maintex", Float) = 0
		_Mask ("Mask", 2D) = "white" {}
		_x_speed_noise ("x_speed_noise", Float) = -0.5
		_y_speed_noise ("y_speed_noise", Float) = 0
		_claer_mask ("claer_mask", Float) = 5
		_Dissolver_amount ("Dissolver_amount", Range(0, 10)) = 10
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	SubShader {
		Tags { "QUEUE" = "AlphaTest" "RenderType" = "TransparentCutout" }
		Pass {
			Name "FORWARD"
			Tags { "LIGHTMODE" = "FORWARDBASE" "QUEUE" = "AlphaTest" "RenderType" = "TransparentCutout" "SHADOWSUPPORT" = "true" }
			Cull Off
			Stencil {
				Ref 128
			}
			GpuProgramID 8163
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
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
			
			// Keywords: DIRECTIONAL
			v2f vert(appdata_full v)
			{
