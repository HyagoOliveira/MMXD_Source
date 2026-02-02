using UnityEngine;

public class MapObject : MonoBehaviour
{
	[SerializeField]
	private Material Main;

	[SerializeField]
	private Material Main1;

	[SerializeField]
	private Material Main2;

	[SerializeField]
	private float MinEmission = -0.5f;

	[SerializeField]
	private float MaxEmission = 2f;

	[SerializeField]
	private Color EmissionColor = new Color(1f, 0.16f, 0f, 1f);

	private OrangeMaterialProperty materialProperty;

	private Color[] originalColor;

	private void Start()
	{
		materialProperty = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
		originalColor = new Color[3]
		{
			Main.GetColor(materialProperty.i_EmissionColor),
			Main1.GetColor(materialProperty.i_EmissionColor),
			Main2.GetColor(materialProperty.i_EmissionColor)
		};
	}

	private void Update()
	{
		float value = Mathf.PingPong(Time.time, 1f) * (MaxEmission - MinEmission) + MinEmission;
		Color value2 = EmissionColor * Mathf.LinearToGammaSpace(value);
		Main.SetColor(materialProperty.i_EmissionColor, value2);
		float value3 = Mathf.PingPong(Time.time, 0.5f) / 0.5f * 4f + MinEmission;
		Color value4 = Color.red * Mathf.LinearToGammaSpace(value3);
		Main1.SetColor(materialProperty.i_EmissionColor, value4);
		Main2.SetColor(materialProperty.i_EmissionColor, value4);
	}

	private void OnDisable()
	{
		Main.SetColor(materialProperty.i_EmissionColor, originalColor[0]);
		Main1.SetColor(materialProperty.i_EmissionColor, originalColor[1]);
		Main2.SetColor(materialProperty.i_EmissionColor, originalColor[2]);
	}
}
