using UnityEngine;
using UnityEngine.UI;

public class ItemIconWithAmount : ItemIconBase
{
	[SerializeField]
	protected Image imgTextBg;

	[SerializeField]
	protected OrangeText textAmount;

	[SerializeField]
	protected GameObject rareItemEffect;

	[SerializeField]
	protected Image imgCardType;

	private Color alphaWhite = new Color(1f, 1f, 1f, 0.75f);

	public override void Clear()
	{
		base.Clear();
		ClearAmount();
		SetRareItemEffect(false);
	}

	public void ClearAmount()
	{
		imgTextBg.color = clear;
		textAmount.text = string.Empty;
	}

	public void SetAmount(int amount)
	{
		imgTextBg.color = alphaWhite;
		textAmount.text = "x" + amount;
	}

	public void SetRareItemEffect(bool enable)
	{
		if (rareItemEffect != null)
		{
			rareItemEffect.SetActive(enable);
		}
	}

	public void SetCardType(CARD_TABLE card)
	{
		string cardTypeAssetName = ManagedSingleton<OrangeTableHelper>.Instance.GetCardTypeAssetName(card.n_TYPE);
		imgCardType.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, cardTypeAssetName);
		imgCardType.color = Color.white;
	}
}
