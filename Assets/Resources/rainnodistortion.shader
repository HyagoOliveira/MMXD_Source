Shader "RainDrop/Internal/RainNoDistortion" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Distortion ("Normalmap", 2D) = "black" {}
		_Relief ("Relief Value", Range(0, 2)) = 1.5
		_Darkness ("Darkness", Range(0, 100)) = 10
	}
	SubShader {
		LOD 100
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			LOD 100
			Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			GpuProgramID 45411
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 sv_position : SV_Position0;
				float4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Color;
			float _Darkness;
			// $Globals ConstantBuffers for Fragment Shader
			float _Relief;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Distortion;
			sampler2D _MainTex;
			
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
                tmp0.x = saturate(1.0 - _Darkness);
                o.color.xyz = tmp0.xxx * _Color.xyz;
                o.color.w = _Color.w;
                o.texcoord.xy = v.texcoord.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = tex2D(_Distortion, inp.texcoord.xy);
                tmp0.x = dot(tmp0.xy, tmp0.xy);
                tmp0.x = tmp0.x - 1.0;
                tmp0.x = -tmp0.x * _Relief + 1.0;
                tmp1 = tex2D(_MainTex, inp.texcoord.xy);
                tmp0.yzw = tmp1.xyz * inp.color.xyz;
                o.sv_target.xyz = tmp0.xxx * tmp0.yzw;
                tmp0.x = tmp1.y * tmp1.x;
                tmp0.x = tmp1.z * tmp0.x;
                o.sv_target.w = tmp0.x * inp.color.w;
                return o;
			}
			ENDCG
		}
	}
}