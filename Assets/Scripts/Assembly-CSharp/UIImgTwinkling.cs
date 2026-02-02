using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIImgTwinkling : MonoBehaviour
{
	private Image tImg;

	private float fApha;

	private float fAphaAdd = 0.5f;

	private void Start()
	{
		tImg = GetComponent<Image>();
		if (tImg == null)
		{
			base.enabled = false;
		}
	}

	private void LateUpdate()
	{
		fApha += fAphaAdd * Time.deltaTime;
		if (fApha > 1f)
		{
			fApha = 1f;
			fAphaAdd = -0.5f;
		}
		else if (fApha < 0f)
		{
			fApha = 0f;
			fAphaAdd = 0.5f;
		}
		Color color = tImg.color;
		color.a = fApha;
		tImg.color = color;
	}
}
