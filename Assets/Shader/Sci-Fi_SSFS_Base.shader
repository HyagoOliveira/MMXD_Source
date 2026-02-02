Shader "Sci-Fi/SSFS/Base" {
	Properties {
		[HideInInspector] _BlendSrc ("Blend Src", Float) = 1
		[HideInInspector] _BlendDst ("Blend Dst", Float) = 0
		[HideInInspector] _Cull ("", Float) = 2
		[HideInInspector] _ZWrite ("", Float) = 8
		[HideInInspector] _ZTest ("", Float) = 0
		_MainTex ("", 2D) = "white" {}
		[HideInInspector] _MainTex2 ("", 2D) = "black" {}
		_Color ("", Color) = (1,1,1,1)
		_Color2 ("", Color) = (1,1,1,1)
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
		_BackfaceVisibility ("", Range(0, 1)) = 1
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
			GpuProgramID 63853
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
			float4 _MainTex_ST;
			// $Globals ConstantBuffers for Fragment Shader
			float4 _Color;
			float4 _Color2;
			float4 _TileCount;
			float4 _Scaling;
			float4 _PhaseDirection;
			float _Phase;
			float _PhaseSharpness;
			float _Scattering;
			float _InvertPhase;
			float _Overbright;
			float _FlashAmount;
			float _SquareTiles;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _Noise;
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
                o.texcoord1.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                o.texcoord.xy = v.texcoord.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                tmp0.x = _SquareTiles > 0.5;
                tmp0.y = tmp0.x ? _TileCount.x : _TileCount.y;
                tmp0.x = _TileCount.x;
                tmp0.zw = tmp0.xy * inp.texcoord.xy;
                tmp0.zw = floor(tmp0.zw);
                tmp0.zw = tmp0.zw / tmp0.xy;
                tmp0.xy = float2(0.5, 0.5) / tmp0.xy;
                tmp0.xy = tmp0.xy + tmp0.zw;
                tmp0.zw = tmp0.xy - float2(0.5, 0.5);
                tmp1 = tex2D(_Noise, tmp0.xy);
                tmp0.x = tmp1.x - 0.5;
                tmp0.y = _PhaseDirection.x * 6.28318;
                tmp1.x = sin(tmp0.y);
                tmp2.x = cos(tmp0.y);
                tmp2.y = -tmp1.x;
                tmp0.y = max(abs(tmp1.x), abs(tmp2.x));
                tmp0.z = dot(tmp0.xy, tmp2.xy);
                tmp0.y = saturate(tmp0.z * tmp0.y + 0.5);
                tmp0.z = 1.0 - tmp0.y;
                tmp0.w = _InvertPhase > 0.5;
                tmp0.y = tmp0.w ? tmp0.z : tmp0.y;
                tmp0.z = _Phase - 0.5;
                tmp0.x = tmp0.z + tmp0.x;
                tmp0.z = _Scattering + _Scattering;
                tmp0.x = tmp0.z * tmp0.x;
                tmp0.z = _PhaseSharpness * 15.0 + 1.0;
                tmp0.w = 1.0 / tmp0.z;
                tmp0.x = tmp0.x * tmp0.w + tmp0.y;
                tmp0.y = 1.0 - _Phase;
                tmp0.x = tmp0.x - tmp0.y;
                tmp0.x = tmp0.z * tmp0.x + tmp0.y;
                tmp0.y = tmp0.y * 2.0 + -1.0;
                tmp0.x = saturate(tmp0.x - tmp0.y);
                tmp0.x = 1.0 - tmp0.x;
                tmp0.y = tmp0.x * tmp0.x;
                tmp0.x = tmp0.y * tmp0.x;
                tmp0.y = -tmp0.y * tmp0.y + 1.0;
                tmp0.zw = tmp0.xx * _Scaling.xy + float2(1.0, 1.0);
                tmp0.x = tmp0.x * _FlashAmount;
                tmp1.xy = saturate(_Scaling.zw);
                tmp1.zw = inp.texcoord.xy - tmp1.xy;
                tmp0.zw = tmp1.zw * tmp0.zw + tmp1.xy;
                tmp0.zw = tmp0.zw * _MainTex_ST.xy + _MainTex_ST.zw;
                tmp1 = tex2D(_MainTex, tmp0.zw);
                tmp1 = tmp0.yyyy * tmp1;
                tmp0.y = tmp0.y * tmp0.x;
                tmp0.y = tmp0.y * 10.0 + 1.0;
                tmp2 = _Color2 - _Color;
                tmp2 = tmp0.xxxx * tmp2 + _Color;
                tmp1 = tmp1 * tmp2;
                tmp0.xyz = tmp0.yyy * tmp1.xyz;
                tmp0.w = max(tmp1.w, 0.0);
                tmp0.xyz = tmp0.www * tmp0.xyz;
                o.sv_target.w = tmp0.w;
                tmp0.w = _Overbright * _Overbright;
                tmp0.w = tmp0.w * 16.0 + 1.0;
                o.sv_target.xyz = tmp0.www * tmp0.xyz;
                return o;
			}
			ENDCG
		}
	}
	CustomEditor "SSFS_Editor"
}