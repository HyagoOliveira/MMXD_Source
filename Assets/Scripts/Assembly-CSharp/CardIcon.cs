#define RELEASE
using UnityEngine;
using UnityEngine.UI;

public class CardIcon : ItemIconBase
{
	[SerializeField]
	private StarClearComponent EquipStar;

	[SerializeField]
	private OrangeText Level;

	[SerializeField]
	private Image LockImage;

	[SerializeField]
	private Image ColorTypeImage;

	[SerializeField]
	private Image IconImage;

	private bool starInst;

	[SerializeField]
	private Image FavoriteImage;

	public void CardSetup(int CardSeqID, bool SetSprite = false)
	{
		CardInfo value = null;
		ManagedSingleton<PlayerNetManager>.Instance.dicCard.TryGetValue(CardSeqID, out value);
		if (value == null)
		{
			return;
		}
		NetCardInfo netCardInfo = value.netCardInfo;
		EXP_TABLE cardExpTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCardExpTable(netCardInfo.Exp);
		CARD_TABLE tCardTable = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[netCardInfo.CardID];
		SetStarAndLv(netCardInfo.Star, cardExpTable.n_ID);
		SetLockImage(netCardInfo.Protected == 1);
		SetTypeImage(tCardTable.n_TYPE);
		SetRare(tCardTable.n_RARITY);
		if (FavoriteImage != null)
		{
			FavoriteImage.gameObject.SetActive(netCardInfo.Favorite == 1);
		}
		if (!SetSprite)
		{
			return;
		}
		string bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, tCardTable.n_PATCH);
		string s_ICON = tCardTable.s_ICON;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(bundleName, s_ICON, delegate(Sprite asset)
		{
			if (asset != null)
			{
				IconImage.sprite = asset;
			}
			else
			{
				Debug.LogWarning("CardTable: unable to load sprite " + tCardTable.s_ICON);
			}
		});
	}

	public void SetStarAndLv(int star, int lv)
	{
		EquipStar.gameObject.SetActive(true);
		EquipStar.SetActiveStar(star);
		Level.text = lv.ToString();
	}

	public void SetLockImage(bool b = false)
	{
		LockImage.gameObject.SetActive(b);
	}

	public void SetFavoriteImage(bool bFavorite)
	{
		if (FavoriteImage != null)
		{
			FavoriteImage.gameObject.SetActive(bFavorite);
		}
	}

	public void SetLv(int lv)
	{
		Level.text = lv.ToString();
	}

	public void SetTypeImage(int typ)
	{
		string cardTypeAssetName = ManagedSingleton<OrangeTableHelper>.Instance.GetCardTypeAssetName(typ);
		ColorTypeImage.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, cardTypeAssetName);
	}

	public override void Clear()
	{
		base.Clear();
		EquipStar.gameObject.SetActive(false);
	}
}
