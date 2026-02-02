using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageColorController : MonoBehaviour
{
	[HideInInspector]
	public float ColorH;

	[HideInInspector]
	public float ColorS;

	[HideInInspector]
	public float ColorV;

	[HideInInspector]
	public float ColorR;

	[HideInInspector]
	public float ColorG;

	[HideInInspector]
	public float ColorB;

	[HideInInspector]
	public float ColorA;

	private Image _image;

	private void Awake()
	{
		_image = GetComponent<Image>();
	}

	public void SetHSVColor(float colorH, float colorS = 1f, float colorV = 1f)
	{
		ColorH = colorH;
		ColorS = colorS;
		ColorV = colorV;
		Color color = Color.HSVToRGB(colorH, colorS, colorV);
		ColorR = color.r;
		ColorG = color.g;
		ColorB = color.b;
		ColorA = 1f;
		_image.color = color;
	}

	public void SetRGBColor(float colorR, float colorG, float colorB, float colorA)
	{
		ColorR = colorR;
		ColorG = colorG;
		ColorB = colorB;
		Color color = new Color(colorR, colorG, colorB, colorA);
		Color.RGBToHSV(color, out ColorH, out ColorS, out ColorV);
		_image.color = color;
	}
}
