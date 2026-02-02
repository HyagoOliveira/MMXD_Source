Shader "RainDrop/Internal/RainDistortion (Forward)" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_Strength ("Distortion Strength", Range(0, 550)) = 50
		_Relief ("Relief Value", Range(0, 2)) = 1.5
		_Distortion ("Normalmap", 2D) = "black" {}
		_ReliefTex ("Relief", 2D) = "black" {}
		_Blur ("Blur", Range(0, 50)) = 3
		_Darkness ("Darkness", Range(0, 100)) = 10
	}
	SubShader {
		LOD 100
		Tags { "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		GrabPass {
		}
		Pass {
			LOD 100
			Tags { "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			ColorMask RGB
			ZTest Always
			GpuProgramID 46222
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 sv_position : SV_Position0;
				float2 color : COLOR0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float texcoord2 : TEXCOORD2;
				float texcoord3 : TEXCOORD3;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _GrabTexture_TexelSize;
			float4 _Color;
			float _Strength;
			float _Darkness;
			// $Globals ConstantBuffers for Fragment Shader
			float _Relief;
			float _Blur;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Distortion;
			sampler2D _ReliefTex;
			sampler2D _GrabTexture;
			
			// Keywords: BLUR
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
                tmp0 = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.sv_position = tmp0;
                o.color.xy = v.color.xy;
                o.texcoord.xy = v.texcoord.xy;
                tmp1.xyz = tmp0.xwy * float3(0.5, 0.5, -0.5);
                o.texcoord1.zw = tmp0.zw;
                o.texcoord1.xy = tmp1.yy + tmp1.xz;
                o.texcoord2.x = _GrabTexture_TexelSize.x * _Strength;
                o.texcoord3.x = _Color.w * _Darkness;
                return o;
			}
			// Keywords: BLUR
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                tmp0 = tex2D(_Distortion, inp.texcoord.xy);
                tmp0.x = tmp0.w * tmp0.x;
                tmp0.xy = tmp0.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp0.z = tmp0.x * _Blur;
                tmp0.z = tmp0.z * _GrabTexture_TexelSize.x;
                tmp1.xy = -inp.texcoord2.xx * tmp0.xy + inp.texcoord1.xy;
                tmp0.x = -tmp0.x * _Relief + 1.0;
                tmp2 = tmp0.zzzz * float4(-2000.0, -4000.0, -3000.0, 1000.0) + tmp1.xxxx;
                tmp3 = tmp0.zzzz * float4(2000.0, -1000.0, 3000.0, 4000.0) + tmp1.xxxx;
                tmp1.zw = tmp2.yz;
                tmp4 = tmp1.zywy / inp.texcoord1.wwww;
                tmp5 = tex2D(_GrabTexture, tmp4.zw);
                tmp4 = tex2D(_GrabTexture, tmp4.xy);
                tmp0.yzw = tmp5.xyz * float3(0.09, 0.09, 0.09);
                tmp0.yzw = tmp4.xyz * float3(0.05, 0.05, 0.05) + tmp0.yzw;
                tmp2.y = tmp1.y;
                tmp1.xy = tmp1.xy / inp.texcoord1.ww;
                tmp1 = tex2D(_GrabTexture, tmp1.xy);
                tmp4 = tmp2.xywy / inp.texcoord1.wwww;
                tmp5 = tex2D(_GrabTexture, tmp4.xy);
                tmp4 = tex2D(_GrabTexture, tmp4.zw);
                tmp0.yzw = tmp5.xyz * float3(0.12, 0.12, 0.12) + tmp0.yzw;
                tmp2.z = tmp3.y;
                tmp2.xz = tmp2.zy / inp.texcoord1.ww;
                tmp3.y = tmp2.y;
                tmp2 = tex2D(_GrabTexture, tmp2.xz);
                tmp0.yzw = tmp2.xyz * float3(0.15, 0.15, 0.15) + tmp0.yzw;
                tmp0.yzw = tmp1.xyz * float3(0.18, 0.18, 0.18) + tmp0.yzw;
                tmp0.yzw = tmp4.xyz * float3(0.15, 0.15, 0.15) + tmp0.yzw;
                tmp1 = tmp3.xyzy / inp.texcoord1.wwww;
                tmp2.xy = tmp3.wy / inp.texcoord1.ww;
                tmp2 = tex2D(_GrabTexture, tmp2.xy);
                tmp3 = tex2D(_GrabTexture, tmp1.xy);
                tmp1 = tex2D(_GrabTexture, tmp1.zw);
                tmp0.yzw = tmp3.xyz * float3(0.12, 0.12, 0.12) + tmp0.yzw;
                tmp0.yzw = tmp1.xyz * float3(0.09, 0.09, 0.09) + tmp0.yzw;
                tmp0.yzw = tmp2.xyz * float3(0.05, 0.05, 0.05) + tmp0.yzw;
                tmp1 = tex2D(_ReliefTex, inp.texcoord.xy);
                tmp2.xyz = tmp1.xyz * _Color.xyz;
                tmp0.yzw = tmp2.xyz * _Color.www + tmp0.yzw;
                tmp1.x = tmp1.y * tmp1.x;
                tmp1.x = saturate(tmp1.z * tmp1.x);
                tmp1.x = saturate(-inp.texcoord3.x * tmp1.x + 1.0);
                tmp0.yzw = tmp0.yzw * tmp1.xxx;
                o.sv_target.xyz = tmp0.xxx * tmp0.yzw;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
	}
}