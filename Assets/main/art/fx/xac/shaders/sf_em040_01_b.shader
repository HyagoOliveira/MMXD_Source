Shader "Shader Forge/SF_Em040_01_B" {
	Properties {
		_BodyColor ("Body Color", Color) = (0.7686275, 0.627451, 0, 1)
		_EyeColor ("Eye Color", Color) = (1, 0, 0, 1)
		_Map ("Map", 2D) = "white" {}
		_EyeLight ("Eye Light", Range(0, 1)) = 1
	}
	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Pass {
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
			};

			sampler2D _Map;
			float4 _Map_ST;
			float4 _BodyColor;
			float4 _EyeColor;
			float _EyeLight;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _Map);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			float4 frag(v2f inp) : SV_Target
			{
				float4 mapTex = tex2D(_Map, inp.uv);
				float3 normalDir = normalize(inp.worldNormal);
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

				// Half-lambert diffuse calculation
				float nDotL = dot(normalDir, lightDir) * 0.5 + 0.5;
				float3 lightContribution = (_LightColor0.rgb * nDotL) + ShadeSH9(float4(normalDir, 1.0));

				// Body surface texturing:
				// Green and blue channels act as surface texture intensity
				float surfaceIntensity = saturate(mapTex.b * (0.6 + mapTex.g * 0.5));
				float3 baseBody = _BodyColor.rgb * surfaceIntensity;

				// Eye masking via red channel:
				// Isolates the eye center/ring and blends out the body yellow
				float eyeMask = saturate(mapTex.r);
				float3 albedo = lerp(baseBody, _EyeColor.rgb, eyeMask);

				// Emission on the eye driven by _EyeLight and the red mask
				float3 emission = _EyeColor.rgb * (eyeMask * _EyeLight);

				return float4((albedo * lightContribution) + emission, 1.0);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}