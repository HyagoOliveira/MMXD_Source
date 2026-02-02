Shader "KY/add_anime_dyn_two" {
	Properties {
		_MainTex ("MainTex", 2D) = "white" {}
		[MaterialToggle] _pickupCh ("pickupCh", Float) = 0
		_emissive ("emissive", Float) = 2
		_texDensity ("texDensity", Float) = 2
		[MaterialToggle] _useR ("useR", Float) = 0
		[MaterialToggle] _useG ("useG", Float) = 0
		[MaterialToggle] _useB ("useB", Float) = 0
		_alphaDensity ("alphaDensity", Float) = 1
		_alphaPower ("alphaPower", Float) = 1
		_vertColorDensity ("vertColorDensity", Float) = 1
		_color ("color", Vector) = (1,1,1,1)
		_uvDynCorrect ("uvDynCorrect", Float) = 1
		[MaterialToggle] _haveAlpha ("haveAlpha", Float) = 0
		_depthBlend ("depthBlend", Float) = 1
		[MaterialToggle] _useDepthBlend ("useDepthBlend", Float) = 1
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
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_Maine One
			ZWrite Off
			Cull Off
			GpuProgramID 3529
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR0;
				float4 texcoord1 : TEXCOORD1;
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
