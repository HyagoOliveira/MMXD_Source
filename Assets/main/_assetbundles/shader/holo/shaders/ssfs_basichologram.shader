Shader "Sci-Fi/SSFS/Simple Hologram" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_Intensity ("Intensity", Range(1, 10)) = 2
		_MainTex ("Model Texture", 2D) = "white" {}
		_ScreenTex ("Screen Space Texture", 2D) = "white" {}
		_ScreenTexWeight ("Screen Space Texture Weight", Range(0, 1)) = 0.25
		_Noise ("Noise", 2D) = "gray" {}
		_Aberration ("Aberration", Range(0, 1)) = 0.5
		_Glitch ("Glitch", Range(0, 1)) = 0.5
		_GlitchSpeed ("Glitch Speed", Float) = 0.5
		_GlitchResolution ("Glitch Resolution", Range(1, 10)) = 5
		_GlitchColor ("Shimmer Color", Vector) = (1,1,1,1)
		_ScanlineStrength ("Scanline Strength", Range(0, 1)) = 0.5
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
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o. : TEXCOOORD2;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float4 texcoord3 : TEXCOORD3;
				float3 texcoord4 : TEXCOORD4;
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
