Shader "Hidden/KriptoFX/PostEffects/WFX_Bloom" {
	Properties {
		_MainTex ("", 2D) = "" {}
		_BaseTex ("", 2D) = "" {}
	}
	SubShader {
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			GpuProgramID 31843
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
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float2 _MainTex_TexelSize;
			float _PrefilterOffs;
			float _Threshold;
			float3 _Curve;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
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
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0.xy = _MainTex_TexelSize * _PrefilterOffs.xx + inp.texcoord.xy;
                tmp0 = tex2D(_MainTex, tmp0.xy);
                tmp0.xyz = min(tmp0.xyz, float3(65000.0, 65000.0, 65000.0));
                tmp1.xyz = tmp0.xyz * float3(0.305306, 0.305306, 0.305306) + float3(0.6821711, 0.6821711, 0.6821711);
                tmp1.xyz = tmp0.xyz * tmp1.xyz + float3(0.0125229, 0.0125229, 0.0125229);
                tmp0.xyz = tmp0.xyz * tmp1.xyz;
                tmp0.w = max(tmp0.y, tmp0.x);
                tmp0.w = max(tmp0.z, tmp0.w);
                tmp1.x = tmp0.w - _Curve.x;
                tmp1.x = max(tmp1.x, 0.0);
                tmp1.x = min(tmp1.x, _Curve.y);
                tmp1.x = tmp1.x * tmp1.x;
                tmp1.x = tmp1.x * _Curve.z;
                tmp1.y = tmp0.w - _Threshold;
                tmp0.w = max(tmp0.w, 0.00001);
                tmp1.x = max(tmp1.y, tmp1.x);
                tmp0.w = tmp1.x / tmp0.w;
                o.sv_target.xyz = tmp0.www * tmp0.xyz;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			GpuProgramID 93333
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
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float2 _MainTex_TexelSize;
			float _PrefilterOffs;
			float _Threshold;
			float3 _Curve;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
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
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                float4 tmp6;
                tmp0 = _MainTex_TexelSize.xyxy * _PrefilterOffs.xxxx + inp.texcoord.xyxy;
                tmp1 = -_MainTex_TexelSize.xxxy * float4(1.0, 0.0, 0.0, 1.0) + tmp0.zwzw;
                tmp2 = tex2D(_MainTex, tmp1.xy);
                tmp1 = tex2D(_MainTex, tmp1.zw);
                tmp1.xyz = min(tmp1.xyz, float3(65000.0, 65000.0, 65000.0));
                tmp2.xyz = min(tmp2.xyz, float3(65000.0, 65000.0, 65000.0));
                tmp3 = tex2D(_MainTex, tmp0.zw);
                tmp0 = _MainTex_TexelSize.xxxy * float4(1.0, 0.0, 0.0, 1.0) + tmp0;
                tmp3.xyz = min(tmp3.xyz, float3(65000.0, 65000.0, 65000.0));
                tmp4.xyz = tmp2.xyz + tmp3.xyz;
                tmp5 = tex2D(_MainTex, tmp0.xy);
                tmp0 = tex2D(_MainTex, tmp0.zw);
                tmp0.xyz = min(tmp0.xyz, float3(65000.0, 65000.0, 65000.0));
                tmp5.xyz = min(tmp5.xyz, float3(65000.0, 65000.0, 65000.0));
                tmp4.xyz = tmp4.xyz + tmp5.xyz;
                tmp6.xyz = min(tmp2.xyz, tmp3.xyz);
                tmp2.xyz = max(tmp2.xyz, tmp3.xyz);
                tmp2.xyz = max(tmp5.xyz, tmp2.xyz);
                tmp3.xyz = min(tmp5.xyz, tmp6.xyz);
                tmp3.xyz = tmp4.xyz - tmp3.xyz;
                tmp2.xyz = tmp3.xyz - tmp2.xyz;
                tmp3.xyz = tmp1.xyz + tmp2.xyz;
                tmp3.xyz = tmp0.xyz + tmp3.xyz;
                tmp4.xyz = min(tmp1.xyz, tmp2.xyz);
                tmp1.xyz = max(tmp1.xyz, tmp2.xyz);
                tmp1.xyz = max(tmp0.xyz, tmp1.xyz);
                tmp0.xyz = min(tmp0.xyz, tmp4.xyz);
                tmp0.xyz = tmp3.xyz - tmp0.xyz;
                tmp0.xyz = tmp0.xyz - tmp1.xyz;
                tmp1.xyz = tmp0.xyz * float3(0.305306, 0.305306, 0.305306) + float3(0.6821711, 0.6821711, 0.6821711);
                tmp1.xyz = tmp0.xyz * tmp1.xyz + float3(0.0125229, 0.0125229, 0.0125229);
                tmp0.xyz = tmp0.xyz * tmp1.xyz;
                tmp0.w = max(tmp0.y, tmp0.x);
                tmp0.w = max(tmp0.z, tmp0.w);
                tmp1.x = tmp0.w - _Curve.x;
                tmp1.x = max(tmp1.x, 0.0);
                tmp1.x = min(tmp1.x, _Curve.y);
                tmp1.x = tmp1.x * tmp1.x;
                tmp1.x = tmp1.x * _Curve.z;
                tmp1.y = tmp0.w - _Threshold;
                tmp0.w = max(tmp0.w, 0.00001);
                tmp1.x = max(tmp1.y, tmp1.x);
                tmp0.w = tmp1.x / tmp0.w;
                o.sv_target.xyz = tmp0.www * tmp0.xyz;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			GpuProgramID 132164
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
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float2 _MainTex_TexelSize;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
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
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0 = _MainTex_TexelSize.xyxy * float4(-1.0, -1.0, 1.0, -1.0) + inp.texcoord.xyxy;
                tmp1 = tex2D(_MainTex, tmp0.xy);
                tmp0 = tex2D(_MainTex, tmp0.zw);
                tmp0.xyz = tmp0.xyz + tmp1.xyz;
                tmp1 = _MainTex_TexelSize.xyxy * float4(-1.0, 1.0, 1.0, 1.0) + inp.texcoord.xyxy;
                tmp2 = tex2D(_MainTex, tmp1.xy);
                tmp1 = tex2D(_MainTex, tmp1.zw);
                tmp0.xyz = tmp0.xyz + tmp2.xyz;
                tmp0.xyz = tmp1.xyz + tmp0.xyz;
                o.sv_target.xyz = tmp0.xyz * float3(0.25, 0.25, 0.25);
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			GpuProgramID 228147
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
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float2 _MainTex_TexelSize;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
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
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0 = _MainTex_TexelSize.xyxy * float4(-1.0, -1.0, 1.0, -1.0) + inp.texcoord.xyxy;
                tmp1 = tex2D(_MainTex, tmp0.zw);
                tmp0 = tex2D(_MainTex, tmp0.xy);
                tmp0.w = max(tmp1.y, tmp1.x);
                tmp0.w = max(tmp1.z, tmp0.w);
                tmp0.w = tmp0.w + 1.0;
                tmp0.w = 1.0 / tmp0.w;
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp1.w = max(tmp0.y, tmp0.x);
                tmp1.w = max(tmp0.z, tmp1.w);
                tmp1.w = tmp1.w + 1.0;
                tmp1.w = 1.0 / tmp1.w;
                tmp0.xyz = tmp0.xyz * tmp1.www + tmp1.xyz;
                tmp0.w = tmp0.w + tmp1.w;
                tmp1 = _MainTex_TexelSize.xyxy * float4(-1.0, 1.0, 1.0, 1.0) + inp.texcoord.xyxy;
                tmp2 = tex2D(_MainTex, tmp1.xy);
                tmp1 = tex2D(_MainTex, tmp1.zw);
                tmp1.w = max(tmp2.y, tmp2.x);
                tmp1.w = max(tmp2.z, tmp1.w);
                tmp1.w = tmp1.w + 1.0;
                tmp1.w = 1.0 / tmp1.w;
                tmp0.xyz = tmp2.xyz * tmp1.www + tmp0.xyz;
                tmp0.w = tmp0.w + tmp1.w;
                tmp1.w = max(tmp1.y, tmp1.x);
                tmp1.w = max(tmp1.z, tmp1.w);
                tmp1.w = tmp1.w + 1.0;
                tmp1.w = 1.0 / tmp1.w;
                tmp0.xyz = tmp1.xyz * tmp1.www + tmp0.xyz;
                tmp0.w = tmp0.w + tmp1.w;
                tmp0.w = 1.0 / tmp0.w;
                o.sv_target.xyz = tmp0.www * tmp0.xyz;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			GpuProgramID 315113
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
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float2 _MainTex_TexelSize;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
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
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0 = _MainTex_TexelSize.xyxy * float4(-1.0, -1.0, 1.0, -1.0) + inp.texcoord.xyxy;
                tmp1 = tex2D(_MainTex, tmp0.xy);
                tmp0 = tex2D(_MainTex, tmp0.zw);
                tmp0.xyz = tmp0.xyz + tmp1.xyz;
                tmp1 = _MainTex_TexelSize.xyxy * float4(-1.0, 1.0, 1.0, 1.0) + inp.texcoord.xyxy;
                tmp2 = tex2D(_MainTex, tmp1.xy);
                tmp1 = tex2D(_MainTex, tmp1.zw);
                tmp0.xyz = tmp0.xyz + tmp2.xyz;
                tmp0.xyz = tmp1.xyz + tmp0.xyz;
                o.sv_target.xyz = tmp0.xyz * float3(0.25, 0.25, 0.25);
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			GpuProgramID 384740
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
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float2 _BaseTex_TexelSize;
			float4 _MainTex_ST;
			float4 _BaseTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float2 _MainTex_TexelSize;
			float _SampleScale;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _BaseTex;
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
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp0.x = _BaseTex_TexelSize.y < 0.0;
                tmp0.y = 1.0 - v.texcoord.y;
                tmp0.zw = v.texcoord.xy * _BaseTex_ST.xy + _BaseTex_ST.zw;
                o.texcoord1.y = tmp0.x ? tmp0.y : tmp0.w;
                o.texcoord1.x = tmp0.z;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0 = _MainTex_TexelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0);
                tmp1.x = _SampleScale * 0.5;
                tmp2 = tmp0.xyzy * tmp1.xxxx + inp.texcoord.xyxy;
                tmp0 = tmp0.xwzw * tmp1.xxxx + inp.texcoord.xyxy;
                tmp1 = tex2D(_MainTex, tmp2.xy);
                tmp2 = tex2D(_MainTex, tmp2.zw);
                tmp1.xyz = tmp1.xyz + tmp2.xyz;
                tmp2 = tex2D(_MainTex, tmp0.xy);
                tmp0 = tex2D(_MainTex, tmp0.zw);
                tmp1.xyz = tmp1.xyz + tmp2.xyz;
                tmp0.xyz = tmp0.xyz + tmp1.xyz;
                tmp1 = tex2D(_BaseTex, inp.texcoord1.xy);
                o.sv_target.xyz = tmp0.xyz * float3(0.25, 0.25, 0.25) + tmp1.xyz;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			GpuProgramID 445521
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
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float2 _BaseTex_TexelSize;
			float4 _MainTex_ST;
			float4 _BaseTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float2 _MainTex_TexelSize;
			float _SampleScale;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _BaseTex;
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
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp0.x = _BaseTex_TexelSize.y < 0.0;
                tmp0.y = 1.0 - v.texcoord.y;
                tmp0.zw = v.texcoord.xy * _BaseTex_ST.xy + _BaseTex_ST.zw;
                o.texcoord1.y = tmp0.x ? tmp0.y : tmp0.w;
                o.texcoord1.x = tmp0.z;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                tmp0.x = 1.0;
                tmp0.z = _SampleScale;
                tmp0 = tmp0.xxzz * _MainTex_TexelSize.xyxy;
                tmp1.zw = float2(-1.0, 0.0);
                tmp1.x = _SampleScale;
                tmp2 = -tmp0.xywy * tmp1.xxwx + inp.texcoord.xyxy;
                tmp3 = tex2D(_MainTex, tmp2.xy);
                tmp2 = tex2D(_MainTex, tmp2.zw);
                tmp2.xyz = tmp2.xyz * float3(2.0, 2.0, 2.0) + tmp3.xyz;
                tmp3.xy = -tmp0.zy * tmp1.zx + inp.texcoord.xy;
                tmp3 = tex2D(_MainTex, tmp3.xy);
                tmp2.xyz = tmp2.xyz + tmp3.xyz;
                tmp3 = tmp0.zwxw * tmp1.zwxw + inp.texcoord.xyxy;
                tmp4 = tmp0.zywy * tmp1.zxwx + inp.texcoord.xyxy;
                tmp0.xy = tmp0.xy * tmp1.xx + inp.texcoord.xy;
                tmp0 = tex2D(_MainTex, tmp0.xy);
                tmp1 = tex2D(_MainTex, tmp3.xy);
                tmp3 = tex2D(_MainTex, tmp3.zw);
                tmp1.xyz = tmp1.xyz * float3(2.0, 2.0, 2.0) + tmp2.xyz;
                tmp2 = tex2D(_MainTex, inp.texcoord.xy);
                tmp1.xyz = tmp2.xyz * float3(4.0, 4.0, 4.0) + tmp1.xyz;
                tmp1.xyz = tmp3.xyz * float3(2.0, 2.0, 2.0) + tmp1.xyz;
                tmp2 = tex2D(_MainTex, tmp4.xy);
                tmp3 = tex2D(_MainTex, tmp4.zw);
                tmp1.xyz = tmp1.xyz + tmp2.xyz;
                tmp1.xyz = tmp3.xyz * float3(2.0, 2.0, 2.0) + tmp1.xyz;
                tmp0.xyz = tmp0.xyz + tmp1.xyz;
                tmp1 = tex2D(_BaseTex, inp.texcoord1.xy);
                o.sv_target.xyz = tmp0.xyz * float3(0.0625, 0.0625, 0.0625) + tmp1.xyz;
                o.sv_target.w = 0.0;
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			GpuProgramID 478899
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
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float2 _BaseTex_TexelSize;
			float4 _MainTex_ST;
			float4 _BaseTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float2 _MainTex_TexelSize;
			float _SampleScale;
			float _Intensity;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
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
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp0.x = _BaseTex_TexelSize.y < 0.0;
                tmp0.y = 1.0 - v.texcoord.y;
                tmp0.zw = v.texcoord.xy * _BaseTex_ST.xy + _BaseTex_ST.zw;
                o.texcoord1.y = tmp0.x ? tmp0.y : tmp0.w;
                o.texcoord1.x = tmp0.z;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0 = _MainTex_TexelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0);
                tmp1.x = _SampleScale * 0.5;
                tmp2 = tmp0.xyzy * tmp1.xxxx + inp.texcoord.xyxy;
                tmp0 = tmp0.xwzw * tmp1.xxxx + inp.texcoord.xyxy;
                tmp1 = tex2D(_MainTex, tmp2.xy);
                tmp2 = tex2D(_MainTex, tmp2.zw);
                tmp1.xyz = tmp1.xyz + tmp2.xyz;
                tmp2 = tex2D(_MainTex, tmp0.xy);
                tmp0 = tex2D(_MainTex, tmp0.zw);
                tmp1.xyz = tmp1.xyz + tmp2.xyz;
                tmp0.xyz = tmp0.xyz + tmp1.xyz;
                tmp0.xyz = tmp0.xyz * _Intensity.xxx;
                tmp0.xyz = tmp0.xyz * float3(0.25, 0.25, 0.25);
                tmp0.xyz = max(tmp0.xyz, float3(0.0, 0.0, 0.0));
                tmp0.xyz = log(tmp0.xyz);
                tmp0.xyz = tmp0.xyz * float3(0.4166667, 0.4166667, 0.4166667);
                tmp0.xyz = exp(tmp0.xyz);
                tmp0.xyz = tmp0.xyz * float3(1.055, 1.055, 1.055) + float3(-0.055, -0.055, -0.055);
                o.sv_target.xyz = max(tmp0.xyz, float3(0.0, 0.0, 0.0));
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			GpuProgramID 542205
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
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float2 _BaseTex_TexelSize;
			float4 _MainTex_ST;
			float4 _BaseTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float2 _MainTex_TexelSize;
			float _SampleScale;
			float _Intensity;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
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
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp0.x = _BaseTex_TexelSize.y < 0.0;
                tmp0.y = 1.0 - v.texcoord.y;
                tmp0.zw = v.texcoord.xy * _BaseTex_ST.xy + _BaseTex_ST.zw;
                o.texcoord1.y = tmp0.x ? tmp0.y : tmp0.w;
                o.texcoord1.x = tmp0.z;
                o.texcoord.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                tmp0.xw = float2(1.0, 0.0);
                tmp0.z = _SampleScale;
                tmp0 = tmp0.xxzw * _MainTex_TexelSize.xyxy;
                tmp1.x = _SampleScale;
                tmp2 = -tmp0.xywy * tmp1.xxxx + inp.texcoord.xyxy;
                tmp3 = tex2D(_MainTex, tmp2.xy);
                tmp2 = tex2D(_MainTex, tmp2.zw);
                tmp2.xyz = tmp2.xyz * float3(2.0, 2.0, 2.0) + tmp3.xyz;
                tmp1.z = -1.0;
                tmp1.yw = -tmp0.zy * tmp1.zx + inp.texcoord.xy;
                tmp3 = tex2D(_MainTex, tmp1.yw);
                tmp2.xyz = tmp2.xyz + tmp3.xyz;
                tmp3 = tmp0.zwxw * tmp1.zxxx + inp.texcoord.xyxy;
                tmp4 = tmp0.zywy * tmp1.zxxx + inp.texcoord.xyxy;
                tmp0.xy = tmp0.xy * tmp1.xx + inp.texcoord.xy;
                tmp0 = tex2D(_MainTex, tmp0.xy);
                tmp1 = tex2D(_MainTex, tmp3.xy);
                tmp3 = tex2D(_MainTex, tmp3.zw);
                tmp1.xyz = tmp1.xyz * float3(2.0, 2.0, 2.0) + tmp2.xyz;
                tmp2 = tex2D(_MainTex, inp.texcoord.xy);
                tmp1.xyz = tmp2.xyz * float3(4.0, 4.0, 4.0) + tmp1.xyz;
                tmp1.xyz = tmp3.xyz * float3(2.0, 2.0, 2.0) + tmp1.xyz;
                tmp2 = tex2D(_MainTex, tmp4.xy);
                tmp3 = tex2D(_MainTex, tmp4.zw);
                tmp1.xyz = tmp1.xyz + tmp2.xyz;
                tmp1.xyz = tmp3.xyz * float3(2.0, 2.0, 2.0) + tmp1.xyz;
                tmp0.xyz = tmp0.xyz + tmp1.xyz;
                tmp0.xyz = tmp0.xyz * _Intensity.xxx;
                tmp0.xyz = tmp0.xyz * float3(0.0625, 0.0625, 0.0625);
                tmp0.xyz = max(tmp0.xyz, float3(0.0, 0.0, 0.0));
                tmp0.xyz = log(tmp0.xyz);
                tmp0.xyz = tmp0.xyz * float3(0.4166667, 0.4166667, 0.4166667);
                tmp0.xyz = exp(tmp0.xyz);
                tmp0.xyz = tmp0.xyz * float3(1.055, 1.055, 1.055) + float3(-0.055, -0.055, -0.055);
                o.sv_target.xyz = max(tmp0.xyz, float3(0.0, 0.0, 0.0));
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
	}
}