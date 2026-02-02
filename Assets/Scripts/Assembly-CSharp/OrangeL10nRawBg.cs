using UnityEngine;

public class OrangeL10nRawBg : OrangeBgBase
{
	public const string bgFormat = "_BG";

	private L10nRawImage.ImageType ImgType = L10nRawImage.ImageType.Texture;

	[SerializeField]
	private L10nRawImage img;

	public void UpdateImg(string imgName, L10nRawImage.ImageEffect imageEffect = L10nRawImage.ImageEffect.None, bool cache = true)
	{
		img.Init(ImgType, imgName, null, imageEffect, cache);
	}
}
