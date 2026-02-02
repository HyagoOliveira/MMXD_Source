using UnityEngine;

public class OrangeMaterialProperty : MonoBehaviourSingleton<OrangeMaterialProperty>
{
	private const string s_Color = "_Color";

	private const string s_HColor = "_HColor";

	private const string s_SColor = "_SColor";

	private const string s_MainTex = "_MainTex";

	private const string s_RampThreshold = "_RampThreshold";

	private const string s_RampSmooth = "_RampSmooth";

	private const string s_Mask2 = "_Mask2";

	private const string s_EmissionColor = "_EmissionColor";

	private const string s_Intensity = "_Intensity";

	private const string s_SpecColor = "_SpecColor";

	private const string s_Smoothness = "_Smoothness";

	private const string s_SpecSmooth = "_SpecSmooth";

	private const string s_GradientMax = "_GradientMax";

	private const string s_SpecColorTex = "_SpecColorTex";

	private const string s_RimColor = "_RimColor";

	private const string s_RimMin = "_RimMin";

	private const string s_RimMax = "_RimMax";

	private const string s_RimDir = "_RimDir";

	private const string s_OutlineColor = "_OutlineColor";

	private const string s_Outline = "_Outline";

	private const string s_DissolveValue = "_DissolveValue";

	private const string s_DissolveEdge = "_DissolveEdge";

	private const string s_DissolveModelHeight = "_DissolveModelHeight";

	private const string s_DissolveMap = "_DissolveMap";

	private const string s_DissolveRamp = "_DissolveRamp";

	private const string s_AlphaValue = "_AlphaValue";

	private const string s_TintColor = "_TintColor";

	public int i_Color;

	public int i_HColor;

	public int i_SColor;

	public int i_MainTex;

	public int i_RampThreshold;

	public int i_RampSmooth;

	public int i_Mask2;

	public int i_EmissionColor;

	public int i_Intensity;

	public int i_SpecColor;

	public int i_Smoothness;

	public int i_SpecSmooth;

	public int i_GradientMax;

	public int i_SpecColorTex;

	public int i_RimColor;

	public int i_RimMin;

	public int i_RimMax;

	public int i_RimDir;

	public int i_OutlineColor;

	public int i_Outline;

	public int i_DissolveValue;

	public int i_DissolveEdge;

	public int i_DissolveModelHeight;

	public int i_DissolveMap;

	public int i_DissolveRamp;

	public int i_AlphaValue;

	public int i_TintColor;

	public const float DissolveStart = 1f;

	public const float DissolveEnd = 0f;

	public const float HurtRimStart = 0f;

	public const float HurtRimEnd = 0.5f;

	public const float HurtRimMax = 0.5f;

	public Color HurtColor = Color.white;

	public Color IColor = new Color(0.55f, 0.55f, 0.55f, 1f);

	public readonly Color Battle_RenderColor = new Color(1f, 1f, 1f);

	public readonly float Battle_Outline = 0.8f;

	public readonly Color UI_RenderColor = new Color(0.6f, 0.6f, 0.6f);

	public readonly float UI_RampThreshold;

	public readonly float UI_Smoothness = 0.271f;

	public readonly float UI_SpecSmooth = 0.701f;

	public readonly float UI_GradientMax = 1f;

	public readonly float UI_RimMin = 0.99f;

	public readonly float UI_RimMax = 1f;

	public readonly Color UI_RimColor = new Color(0f, 0f, 0f);

	public readonly Color UI_SpecColor = new Color(0f, 0f, 0f);

	public readonly float UI_Outline = 0.25f;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		i_Color = Shader.PropertyToID("_Color");
		i_HColor = Shader.PropertyToID("_HColor");
		i_SColor = Shader.PropertyToID("_SColor");
		i_MainTex = Shader.PropertyToID("_MainTex");
		i_RampThreshold = Shader.PropertyToID("_RampThreshold");
		i_RampSmooth = Shader.PropertyToID("_RampSmooth");
		i_Mask2 = Shader.PropertyToID("_Mask2");
		i_EmissionColor = Shader.PropertyToID("_EmissionColor");
		i_Intensity = Shader.PropertyToID("_Intensity");
		i_SpecColor = Shader.PropertyToID("_SpecColor");
		i_Smoothness = Shader.PropertyToID("_Smoothness");
		i_SpecSmooth = Shader.PropertyToID("_SpecSmooth");
		i_GradientMax = Shader.PropertyToID("_GradientMax");
		i_SpecColorTex = Shader.PropertyToID("_SpecColorTex");
		i_RimColor = Shader.PropertyToID("_RimColor");
		i_RimMin = Shader.PropertyToID("_RimMin");
		i_RimMax = Shader.PropertyToID("_RimMax");
		i_RimDir = Shader.PropertyToID("_RimDir");
		i_OutlineColor = Shader.PropertyToID("_OutlineColor");
		i_Outline = Shader.PropertyToID("_Outline");
		i_DissolveValue = Shader.PropertyToID("_DissolveValue");
		i_DissolveEdge = Shader.PropertyToID("_DissolveEdge");
		i_DissolveModelHeight = Shader.PropertyToID("_DissolveModelHeight");
		i_DissolveMap = Shader.PropertyToID("_DissolveMap");
		i_DissolveRamp = Shader.PropertyToID("_DissolveRamp");
		i_AlphaValue = Shader.PropertyToID("_AlphaValue");
		i_TintColor = Shader.PropertyToID("_TintColor");
	}
}
