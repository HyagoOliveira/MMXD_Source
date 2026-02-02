#define RELEASE
using SSFS;
using UnityEngine;

[CreateAssetMenu(fileName = "SSFS Generator", menuName = "New SSFS Generator", order = 1)]
public class SSFSGenerator : ScriptableObject
{
	public TextureList textures = new TextureList();

	public TextureList scatters = new TextureList();

	public RandomColor baseTint = new RandomColor();

	public RandomColor transitionTint = new RandomColor();

	public boolchance useTextureSwap = new boolchance(false, 0.1f);

	public boolchance useImageAsScatter = new boolchance(false, 0.1f);

	public boolchance useRandomTileCount = new boolchance(true, 1f);

	public boolchance allowIdleAnimation = new boolchance(true, 0.5f);

	public boolchance allowIdleNoise = new boolchance(true, 0.2f);

	public RandomRange idleNoiseStrength = new RandomRange();

	public RandomRange idleIntensity = new RandomRange();

	public RandomRange idleSpeed = new RandomRange();

	public boolchance allowIdleReverse = new boolchance(true, 0.2f);

	public boolchance allowTileClipping = new boolchance(true, 0.2f);

	public boolchance allowRadial = new boolchance(true, 0.5f);

	public RandomRange phaseDirection = new RandomRange();

	public boolchance separateTileCounts = new boolchance(true, 0.2f);

	public RandomRangeInt tileCountUniform = new RandomRangeInt();

	public RandomRangeInt tileCountX = new RandomRangeInt();

	public RandomRangeInt tileCountY = new RandomRangeInt();

	public boolchance separateAxisScaling = new boolchance(true, 0.2f);

	public boolchance allowTileCentricScaling = new boolchance(true, 0.2f);

	public RandomRange scalingUniform = new RandomRange();

	public RandomRange scalingX = new RandomRange();

	public RandomRange scalingY = new RandomRange();

	public RandomRange scattering = new RandomRange();

	public RandomRange phaseSharpness = new RandomRange();

	public RandomRange overbright = new RandomRange();

	public RandomRange aberration = new RandomRange();

	public RandomRange effectAberration = new RandomRange();

	public RandomRange scanlineIntensity = new RandomRange();

	public RandomRange scanlineDistortion = new RandomRange();

	public RandomRange scanlineScale = new RandomRange();

	public RandomRange scanlineSpeed = new RandomRange();

	public RandomRange flash = new RandomRange();

	public RandomRange flicker = new RandomRange();

	public Material GenerateMaterial()
	{
		Material m = SSFSCore.newMaterial;
		genmat(ref m);
		return m;
	}

	public void GenerateMaterial(ref Material existingMaterial)
	{
		if (existingMaterial == null)
		{
			existingMaterial = SSFSCore.newMaterial;
		}
		else if (existingMaterial.shader != SSFSCore.shader)
		{
			existingMaterial.shader = SSFSCore.shader;
		}
		genmat(ref existingMaterial);
	}

	private bool SyncKeyword(ref Material m, string keyword, bool value)
	{
		if (value)
		{
			m.EnableKeyword(keyword);
		}
		else
		{
			m.DisableKeyword(keyword);
		}
		return value;
	}

	private void genmat(ref Material m)
	{
		if (m == null)
		{
			Debug.Log("Null material passed to SSFSGenerator");
			return;
		}
		m.EnableKeyword("COMPLEX");
		m.DisableKeyword("POST");
		m.DisableKeyword("WORLD_SPACE_SCANLINES");
		m.SetFloat("_Cull", 0f);
		m.SetFloat("_BlendSrc", 1f);
		m.SetFloat("_BlendDst", 1f);
		m.SetFloat("_ZWrite", 0f);
		m.SetFloat("_ZTest", 4f);
		m.SetTexture("_MainTex", textures.texture);
		bool check = useTextureSwap.check;
		m.SetTexture("_MainTex2", check ? textures.texture : null);
		SyncKeyword(ref m, "TEXTURE_SWAP", check);
		m.SetTexture("_Noise", useImageAsScatter.check ? m.GetTexture("_MainTex") : scatters.texture);
		m.SetColor("_Color", baseTint.get_color);
		m.SetColor("_Color2", transitionTint.get_color);
		m.SetFloat("_Phase", 1f);
		m.SetVector("_PhaseDirection", new Vector4(phaseDirection.get_float, allowRadial.check ? 1f : 0f, 0f, 0f));
		float num = (allowIdleAnimation.check ? idleIntensity.get_float : 0f);
		float get_float = idleSpeed.get_float;
		float z = (allowIdleNoise.check ? idleNoiseStrength.get_float : 0f);
		float w = (allowIdleReverse.check ? 1f : 0f);
		SyncKeyword(ref m, "IDLE", num > 0f);
		m.SetVector("_IdleData", new Vector4(num, get_float, z, w));
		bool check2 = separateTileCounts.check;
		m.SetFloat("_SquareTiles", check2 ? 1f : 0f);
		int num2 = (check2 ? tileCountUniform.get_int : tileCountX.get_int);
		int num3 = (check2 ? tileCountUniform.get_int : tileCountY.get_int);
		m.SetVector("_TileCount", new Vector4(num2, num3, 0f, 0f));
		bool check3 = separateAxisScaling.check;
		float num4 = (check3 ? scalingUniform.get_float : scalingX.get_float);
		float num5 = (check3 ? scalingUniform.get_float : scalingY.get_float);
		m.SetVector("_Scaling", new Vector4(num4 * 3.5f - 0.5f, num5 * 3.5f - 0.5f, 0.5f, 0.5f));
		m.SetFloat("_ScaleAroundTile", SyncKeyword(ref m, "SCALE_AROUND_TILE", allowTileCentricScaling.check) ? 1f : 0f);
		m.SetFloat("_Scattering", scattering.get_float);
		m.SetFloat("_PhaseSharpness", phaseSharpness.get_float);
		m.SetFloat("_Overbright", overbright.get_float);
		float get_float2 = aberration.get_float;
		float get_float3 = effectAberration.get_float;
		SyncKeyword(ref m, "ABERRATION", Mathf.Max(get_float2, get_float3) > 0f);
		m.SetFloat("_Aberration", get_float2);
		m.SetFloat("_EffectAberration", get_float3);
		m.SetFloat("_Flash", flash.get_float);
		m.SetFloat("_Flicker", flicker.get_float);
		float get_float4 = scanlineIntensity.get_float;
		float get_float5 = scanlineDistortion.get_float;
		SyncKeyword(ref m, "SCAN_LINES", Mathf.Max(get_float4, get_float5) > 0f);
		m.SetVector("_ScanlineData", new Vector4(get_float4, scanlineScale.get_float, get_float5, scanlineSpeed.get_float));
		m.SetFloat("_ClippedTiles", SyncKeyword(ref m, "CLIPPING", allowTileClipping.check) ? 1f : 0f);
	}
}
