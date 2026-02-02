using UnityEngine;
using UnityEngine.UI;

public class ItemIconBase : IconBase
{
	[SerializeField]
	protected Sprite clearBg;

	[SerializeField]
	protected Image rareBg;

	[SerializeField]
	protected Image rareFrame;

	public override void Clear()
	{
		base.Clear();
		rareFrame.color = clear;
		if ((bool)clearBg)
		{
			rareBg.sprite = clearBg;
			rareBg.color = white;
		}
		else
		{
			rareBg.color = clear;
		}
	}

	public void SetRare(int p_rare)
	{
		SetRareInfo(rareBg, AssetBundleScriptableObject.Instance.GetIconRareBgSmall(p_rare));
		SetRareInfo(rareFrame, AssetBundleScriptableObject.Instance.GetIconRareFrameSmall(p_rare));
		rareBg.type = Image.Type.Sliced;
		rareFrame.type = Image.Type.Sliced;
	}
}
