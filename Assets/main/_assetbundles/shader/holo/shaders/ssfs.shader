Shader "Sci-Fi/SSFS/Base" {
	Properties {
		[HideInInspector] _BlendSrc ("Blend Src", Float) = 1
		[HideInInspector] _BlendDst ("Blend Dst", Float) = 0
		[HideInInspector] _Cull ("", Float) = 2
		[HideInInspector] _ZWrite ("", Float) = 8
		[HideInInspector] _ZTest ("", Float) = 0
		_MainTex ("", 2D) = "white" {}
		[HideInInspector] _MainTex2 ("", 2D) = "black" {}
		_Color ("", Vector) = (1,1,1,1)
		_Color2 ("", Vector) = (1,1,1,1)
		_Overbright ("", Range(0, 1)) = 0.25
		[NoScaleOffset] _Noise ("", 2D) = "gray" {}
		_TileCount ("", Vector) = (25,25,0,0)
		_SquareTiles ("", Float) = 0
		_Phase ("", Range(0, 1)) = 1
		[Toggle] _InvertPhase ("", Float) = 0
		_IdleData ("", Vector) = (0.1,0.1,0,0)
		_PhaseDirection ("", Vector) = (0,0,0,0)
		_PhaseSharpness ("", Range(0, 1)) = 0.5
		_Scattering ("", Float) = 0.25
		_Scaling ("", Vector) = (1,1,0.5,0.5)
		_Aberration ("", Range(0, 1)) = 0.5
		_EffectAberration ("", Range(0, 1)) = 0.5
		_FlashAmount ("", Range(0, 1)) = 0.5
		_Flicker ("", Range(0, 0.2)) = 0.1
		_BafaceVisibility ("", Range(0, 1)) = 1
		_ScanlineData ("", Vector) = (0.2,0.5,0,0)
		_ScaleAroundTile ("", Float) = 1
		[Toggle] _ClippedTiles ("", Float) = 1
		[Toggle] _RoundClipping ("", Float) = 0
	}
	SubShader {
		Tags { "PreviewType" = "Plane" "QUEUE" = "Transparent" }
		Pass {
			Tags { "PreviewType" = "Plane" "QUEUE" = "Transparent" }
			Blend Zero Zero, Zero Zero
			ZWrite Off
			Cull Off
			GpuProgramID 61409
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_TARGET0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float4 _MainTex_ST;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
