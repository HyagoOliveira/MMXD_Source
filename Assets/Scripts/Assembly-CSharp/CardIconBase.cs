using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class CardIconBase : IconBase
{
	private string rare_asset_name = "{0}_{1}";

	private string ball_asset_name = "ball_l_{0}";

	private string bgName = "bg";

	private string frameName = "frame";

	private string small = "_s";

	[SerializeField]
	private bool SmallVer = true;

	[SerializeField]
	private Image imgRareBg;

	[SerializeField]
	private Image[] imgStar;

	[SerializeField]
	private Image imgLockg;

	[SerializeField]
	private Image imgType;

	[SerializeField]
	private Text textLevel;

	[HideIf("SmallVer")]
	[SerializeField]
	private OrangeRareText textRare;

	[HideIf("SmallVer")]
	[SerializeField]
	private Text textName;

	[ShowIf("SmallVer")]
	[SerializeField]
	private Image imgRareFrame;

	public string m_icon_card_format = "ui_iconsource_card_";

	private NetCardInfo m_NetCardInfo;

	private CARD_TABLE m_CardTable;

	private string[] RareStr = new string[7] { "E", "D", "C", "B", "A", "S", "SS" };

	private string GetColorTypeString(int typ)
	{
		string text = "red";
		switch (typ)
		{
		case 1:
			return "red";
		case 2:
			return "yellow";
		case 4:
			return "blue";
		case 8:
			return "green";
		case 16:
			return "gray";
		default:
			return "red";
		}
	}

	public void SetCardInfo(NetCardInfo p_netCardInfo)
	{
		m_NetCardInfo = p_netCardInfo;
		m_CardTable = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[m_NetCardInfo.CardID];
		if (SmallVer)
		{
			SetCardRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, RareStr[m_CardTable.n_RARITY]));
			SetCardRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, RareStr[m_CardTable.n_RARITY]));
			SetCardRareInfo(imgType, string.Format(ball_asset_name, GetColorTypeString(m_CardTable.n_TYPE)));
			imgLockg.gameObject.SetActive(m_NetCardInfo.Protected == 1);
		}
		else
		{
			textRare.UpdateaRare(m_CardTable.n_RARITY);
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, m_CardTable.n_RARITY, bgName));
			textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(m_CardTable.w_NAME);
		}
		SetStar(p_netCardInfo.Star);
	}

	private void SetStar(int p_star)
	{
		for (int i = 0; i < imgStar.Length; i++)
		{
			if (p_star > i)
			{
				imgStar[i].color = white;
			}
		}
	}

	private void SetCardRareInfo(Image image, string assetName, bool whiteColor = true)
	{
		if (null == image)
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(m_icon_card_format, assetName, delegate(Sprite obj)
		{
			if (!(null == image))
			{
				image.sprite = obj;
				image.color = (whiteColor ? white : grey);
			}
		});
	}

	public void EnablePortrait(bool bEnable)
	{
		StartCoroutine(ChangePortraitColor(bEnable));
	}

	private IEnumerator ChangePortraitColor(bool bEnable)
	{
		while (imgIcon.sprite == null)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (bEnable)
		{
			imgIcon.color = Color.white;
			imgRareBg.color = Color.white;
		}
		else
		{
			imgIcon.color = Color.grey;
			imgRareBg.color = Color.grey;
		}
	}
}
