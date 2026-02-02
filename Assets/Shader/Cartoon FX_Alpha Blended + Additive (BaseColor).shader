Shader "Cartoon FX/Alpha Blended + Additive (BaseColor)" {
	Properties {
		_BaseColor ("Base Color", Color) = (0.5,0.5,0.5,0.5)
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01, 3)) = 1
	}
	SubShader {
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		UsePass "Hidden/Cartoon FX/Particles/ALPHA_BLENDED"
		UsePass "Hidden/Cartoon FX/Particles/ADDITIVE"
	}
}