using UnityEngine;
using UnityEngine.UI;

public class ResearchIcon : ItemIconWithAmount
{
	[SerializeField]
	private Image imgFragmentPiece;

	[SerializeField]
	private Image imgProgressBG;

	[SerializeField]
	private Image imgProgressFG;

	[SerializeField]
	private Image imgComplete;

	public void SetProgress(int numerator, int denominator)
	{
		imgProgressFG.fillAmount = (float)numerator / (float)denominator;
		if (numerator >= denominator)
		{
			imgProgressFG.fillAmount = 1f;
			imgComplete.gameObject.SetActive(true);
		}
		else
		{
			imgComplete.gameObject.SetActive(false);
		}
	}

	public void Show(bool isShow, bool isShard)
	{
		imgProgressBG.gameObject.SetActive(isShow);
		imgProgressFG.gameObject.SetActive(isShow);
		imgComplete.gameObject.SetActive(isShow);
		imgIcon.gameObject.SetActive(isShow);
		imgTextBg.gameObject.SetActive(isShow);
		textAmount.gameObject.SetActive(isShow);
		imgProgressBG.gameObject.SetActive(isShow);
		imgFragmentPiece.gameObject.SetActive(isShow && isShard);
		rareBg.gameObject.SetActive(true);
		rareFrame.gameObject.SetActive(isShow);
	}

	public void SetResearchDefault()
	{
		SetRareInfo(rareBg, "UI_iconsource_Lab01");
	}

	public void SetSize(Vector2 sizeDelta)
	{
		imgIcon.rectTransform.sizeDelta = sizeDelta;
	}
}
