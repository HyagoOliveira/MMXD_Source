Shader "Orange/Ghara/HDRFlowingUV" {
	Properties {
		_EmissionColor ("Emission Color", Color) = (1,1,1,1)
		_DiffuseTex ("Diffuse Texture", 2D) = "white" {}
		_MaskTex ("Mask Texture", 2D) = "white" {}
		_FlowX ("X", Range(0, 1)) = 0
		_FlowY ("Y", Range(0, 1)) = 0
		_Intensity ("Intensity", Range(0, 10)) = 2
	}
	SubShader {
		Tags { "QUEUE" = "Transparent" }
		Pass {
			Tags { "QUEUE" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			GpuProgramID 2549
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 sv_position : SV_Position0;
				float2 texcoord : TEXCOORD0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			float3 _EmissionColor;
			float4 _DiffuseTex_ST;
			float4 _MaskTex_ST;
			float _FlowX;
			float _FlowY;
			float _Intensity;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _DiffuseTex;
			sampler2D _MaskTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.sv_position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                tmp0.xy = inp.texcoord.xy * _DiffuseTex_ST.xy + _DiffuseTex_ST.zw;
                tmp0.xy = float2(_FlowX.x, _FlowY.x) * _Time.yy + tmp0.xy;
                tmp0 = tex2D(_DiffuseTex, tmp0.xy);
                tmp0.xyz = tmp0.xyz * _EmissionColor;
                o.sv_target.xyz = tmp0.xyz * _Intensity.xxx;
                tmp0.xy = inp.texcoord.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
                tmp0 = tex2D(_MaskTex, tmp0.xy);
                o.sv_target.w = tmp0.w;
                return o;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}