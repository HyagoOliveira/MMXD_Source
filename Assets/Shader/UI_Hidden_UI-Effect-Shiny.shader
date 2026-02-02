Shader "UI/Hidden/UI-Effect-Shiny" {
	Properties {
		[PerRendererData] _MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		_ParamTex ("Parameter Texture", 2D) = "white" {}
	}
	SubShader {
		Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Pass {
			Name "Default"
			Tags { "CanUseSpriteAtlas" = "true" "IGNOREPROJECTOR" = "true" "PreviewType" = "Plane" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ColorMask 0
			ZWrite Off
			Cull Off
			Stencil {
				ReadMask 0
				WriteMask 0
				Comp [Disabled]
				Pass Keep
				Fail Keep
				ZFail Keep
			}
			GpuProgramID 59277
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord2 : TEXCOORD2;
				float4 texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _Color;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _TextureSampleAdd;
			float4 _ClipRect;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			sampler2D _ParamTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.color = v.color * _Color;
                tmp0 = v.texcoord.xxyy * float4(4096.0, 0.0002441, 4096.0, 0.0002441);
                tmp0.yw = floor(tmp0.yw);
                tmp1 = tmp0.xxzz >= -tmp0.xxzz;
                tmp1 = tmp1 ? float4(4096.0, 0.0002441, 4096.0, 0.0002441) : float4(-4096.0, -0.0002441, -4096.0, -0.0002441);
                tmp2 = tmp0.yyww * float4(4096.0, 4096.0, 4096.0, 4096.0);
                tmp2 = tmp2 >= -tmp2.yyww;
                tmp2 = tmp2 ? float4(4096.0, 0.0002441, 4096.0, 0.0002441) : float4(-4096.0, -0.0002441, -4096.0, -0.0002441);
                tmp0.xy = tmp0.yw * tmp2.yw;
                tmp0.xy = frac(tmp0.xy);
                tmp0.xy = tmp0.xy * tmp2.xz;
                o.texcoord2 = tmp0.xy * float2(0.0002442, 0.0002442);
                tmp0.xy = tmp1.yw * v.texcoord.xy;
                tmp0.xy = frac(tmp0.xy);
                tmp0.xy = tmp0.xy * tmp1.xz;
                o.texcoord2 = tmp0.xy * float2(0.0002442, 0.0002442);
                o.texcoord1 = v.vertex;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.xz = float2(0.25, 0.75);
                tmp0.yw = inp.texcoord2.yy;
                tmp1 = tex2D(_ParamTex, tmp0.xy);
                tmp0 = tex2D(_ParamTex, tmp0.zw);
                tmp0.y = tmp1.x * 2.0 + -0.5;
                tmp0.y = inp.texcoord2.x - tmp0.y;
                tmp0.y = tmp0.y / tmp1.y;
                tmp0.y = min(abs(tmp0.y), 1.0);
                tmp0.y = 1.0 - tmp0.y;
                tmp0.z = tmp1.z + tmp1.z;
                tmp0.z = 1.0 / tmp0.z;
                tmp0.y = saturate(tmp0.z * tmp0.y);
                tmp0.z = tmp0.y * -2.0 + 3.0;
                tmp0.y = tmp0.y * tmp0.y;
                tmp0.y = tmp0.y * tmp0.z;
                tmp0.y = tmp0.y * 0.5;
                tmp0.zw = inp.texcoord1.xy >= _ClipRect.xy;
                tmp0.zw = tmp0.zw ? 1.0 : 0.0;
                tmp1.xy = _ClipRect.zw >= inp.texcoord1.xy;
                tmp1.xy = tmp1.xy ? 1.0 : 0.0;
                tmp0.zw = tmp0.zw * tmp1.xy;
                tmp0.z = tmp0.w * tmp0.z;
                tmp2 = tex2D(_MainTex, inp.texcoord.xy);
                tmp2 = tmp2 + _TextureSampleAdd;
                tmp2 = tmp2 * inp.color;
                tmp0.z = tmp0.z * tmp2.w;
                tmp0.y = tmp0.y * tmp0.z;
                o.sv_target.w = tmp0.z;
                tmp0.y = tmp1.w * tmp0.y;
                tmp1.xyz = tmp2.xyz * float3(10.0, 10.0, 10.0) + float3(-1.0, -1.0, -1.0);
                tmp0.xzw = tmp0.xxx * tmp1.xyz + float3(1.0, 1.0, 1.0);
                o.sv_target.xyz = tmp0.yyy * tmp0.xzw + tmp2.xyz;
                return o;
			}
			ENDCG
		}
	}
}