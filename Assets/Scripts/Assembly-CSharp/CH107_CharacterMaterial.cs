using UnityEngine;

public class CH107_CharacterMaterial : CharacterMaterial
{
	[SerializeField]
	private bool canChangeColor;

	[SerializeField]
	private bool canChangeDissolve;

	private const float factor = 3f;

	private Color[] colors = new Color[2]
	{
		new Color(2.035294f, 2.2470589f, 2.2470589f, 1f),
		new Color(1.5411766f, 2.2470589f, 2.2117648f, 1f)
	};

	protected override void UpdatePropertyBlock()
	{
		UpdateColor();
		UpdateDissolve();
		base.UpdatePropertyBlock();
	}

	private void Start()
	{
		UpdatePropertyBlock();
	}

	private void UpdateColor()
	{
		if (canChangeColor)
		{
			OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
			if (base.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.DefaultLayer)
			{
				mpb.SetColor(instance.i_TintColor, colors[0]);
			}
			else
			{
				mpb.SetColor(instance.i_TintColor, colors[1]);
			}
		}
	}

	private void UpdateDissolve()
	{
		if (canChangeDissolve)
		{
			OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
			float a = 1f - mpb.GetFloat(instance.i_DissolveValue);
			float @float = mpb.GetFloat(instance.i_Intensity);
			mpb.SetFloat(instance.i_AlphaValue, Mathf.Min(a, @float));
		}
	}
}
