using System.Collections.Generic;
using UnityEngine;

public class MapBackgroundEnv : MonoBehaviour
{
	public Material[] AnimMatList;

	public float[] AnimEmission;

	public Color[] AnimColor;

	public float[] AnimSpeed;

	private OrangeMaterialProperty materialProperty;

	[SerializeField]
	private float MinEmission = -0.5f;

	public List<Color> originalColor;

	private void Start()
	{
		materialProperty = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
		originalColor = new List<Color>();
		for (int i = 0; i < AnimMatList.Length; i++)
		{
			Color color = AnimMatList[i].GetColor(materialProperty.i_EmissionColor);
			originalColor.Add(color);
		}
	}

	private void Update()
	{
		float num = 0f;
		for (int i = 0; i < AnimMatList.Length; i++)
		{
			num = Mathf.PingPong(Time.time, AnimSpeed[i]) * AnimEmission[i] + MinEmission;
			Color value = AnimColor[i] * Mathf.LinearToGammaSpace(num);
			AnimMatList[i].SetColor(materialProperty.i_EmissionColor, value);
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < AnimMatList.Length; i++)
		{
			AnimMatList[i].SetColor(materialProperty.i_EmissionColor, originalColor[i]);
		}
	}
}
