using UnityEngine;
using UnityEngine.UI;

public class GachaResultLight : MonoBehaviour
{
	[SerializeField]
	private Image[] imgBgs;

	[SerializeField]
	private Image[] imgLights;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Color colorBgS;

	[SerializeField]
	private Color colorBgA;

	[SerializeField]
	private Color colorLightS;

	[SerializeField]
	private Color colorLightA;

	private void Awake()
	{
		SetCanvas(false);
	}

	public void Setup(int rarity)
	{
		if (rarity < 4)
		{
			SetCanvas(false);
			return;
		}
		Color color = colorBgA;
		Color color2 = colorLightA;
		if (rarity == 5)
		{
			color = colorBgS;
			color2 = colorLightS;
		}
		Image[] array = imgBgs;
		foreach (Image image in array)
		{
			image.color = new Color(color.r, color.g, color.b, image.color.a);
		}
		array = imgLights;
		foreach (Image image2 in array)
		{
			image2.color = new Color(color2.r, color2.g, color2.b, image2.color.a);
		}
		SetCanvas(true);
	}

	private void SetCanvas(bool enable)
	{
		canvas.enabled = enable;
	}
}
