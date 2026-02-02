using UnityEngine;

public class CH043_FxShield : FxBase
{
	[SerializeField]
	private ParticleSystem[] particleArray;

	private Color[] colorArray;

	private bool resetColor;

	protected override void Awake()
	{
		base.Awake();
		if (particleArray.Length != 0)
		{
			colorArray = new Color[particleArray.Length];
			for (int i = 0; i < particleArray.Length; i++)
			{
				ParticleSystem.MainModule main = particleArray[i].main;
				colorArray[i] = main.startColor.color;
			}
		}
	}

	private void Update()
	{
		if (resetColor)
		{
			resetColor = false;
			for (int i = 0; i < particleArray.Length; i++)
			{
				ParticleSystem.MainModule main = particleArray[i].main;
				main.startColor = colorArray[i];
			}
		}
	}

	public override void Active(params object[] p_params)
	{
		base.Active(p_params);
		resetColor = true;
	}
}
