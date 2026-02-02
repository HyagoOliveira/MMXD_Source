Shader "Olanigan/IceBreak" {
	Properties {
		_snow ("snow", 2D) = "white" {}
		_Color ("Color", Vector) = (0.5019608,0.5019608,0.5019608,1)
		_Metallic ("Metallic", Range(0, 1)) = 0.6030321
		_Gloss ("Gloss", Range(0, 1)) = 0.3252537
		_BumpMap ("Normal Map I", 2D) = "bump" {}
		_NormalMapII ("Normal Map II", 2D) = "bump" {}
		_snow_slider ("snow_slider", Range(0, 10)) = 7.705339
		_Freezeeffectnormal ("Freeze effect (normal)", Range(0, 10)) = 4.77537
		[MaterialToggle] _LocalGlobal ("Local/Global", Float) = 0
		_Transparency ("Transparency", Range(-1, 1)) = 0
		_Ice_fresnel ("Ice_fresnel", Range(0, 3)) = 0
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	Fallback "nusSrcAlpha
			GpuProgramID 23624
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;
				float3 texcoord4 : TEXCOORD4;
				float3 texcoord5 : TEXCOORD5;
				float3 texcoord6 : TEXCOORD6;
				float4 texcoord7 : TEXCOORD7;
				float4 texcoord9 : TEXCOORD9;
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
			
			// Keywords: DIRECTIONAL DIRLIGHTMAP_OFF DYNAMICLIGHTMAP_OFF LIGHTMAP_OFF
			v2f vert(appdata_full v)
			{
