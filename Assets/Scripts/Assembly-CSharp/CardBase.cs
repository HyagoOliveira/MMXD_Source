#define RELEASE
using CallbackDefs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardBase : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private bool MiddleVer;

	[SerializeField]
	private OrangeText LevelText;

	[SerializeField]
	private OrangeText NameText;

	[SerializeField]
	private OrangeText NumberText;

	[SerializeField]
	private Image[] BackgroundRoots;

	[SerializeField]
	private Image[] TypeRoots;

	[SerializeField]
	private Image[] FrameRoots;

	[SerializeField]
	private Image[] StarRoots;

	[SerializeField]
	private Image LockImage;

	[SerializeField]
	private Image IconImage;

	[SerializeField]
	private GameObject EquipCharRoot;

	[SerializeField]
	private Image EquipCharImage;

	[SerializeField]
	private Image FavoriteImage;

	private int idx;

    [System.Obsolete]
    public event CallbackIdx callback;

    [System.Obsolete]
    public void Setup(int p_idx, string p_bundleName, string p_assetName, CallbackIdx clickCB = null, bool whiteColor = true)
	{
		idx = p_idx;
		this.callback = clickCB;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			this.callback.CheckTargetToInvoke(idx);
		}
	}

	public void CardSetup(NetCardInfo info, bool bSmall = false)
	{
		if (info == null)
		{
			return;
		}
		EXP_TABLE cardExpTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCardExpTable(info.Exp);
		CARD_TABLE tCardTable = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[info.CardID];
		LevelText.text = cardExpTable.n_ID.ToString();
		if (!MiddleVer)
		{
			NameText.text = ManagedSingleton<OrangeTextDataManager>.Instance.CARDTEXT_TABLE_DICT.GetL10nValue(tCardTable.w_NAME);
			NumberText.text = tCardTable.n_ID.ToString();
		}
		LockImage.gameObject.SetActive(info.Protected == 1);
		SetRarity(tCardTable.n_RARITY);
		SetColorType(tCardTable.n_TYPE);
		SetStar(info.Star);
		if (FavoriteImage != null)
		{
			FavoriteImage.gameObject.SetActive(info.Favorite == 1);
		}
		string text = "";
		text = ((!bSmall) ? (AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_l_format, tCardTable.n_PATCH)) : (AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_m_format, tCardTable.n_PATCH)));
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(text, tCardTable.s_CARDIMG, delegate(Sprite asset)
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

	public void CardSetupUpdate(NetCardInfo info, bool bSmall = false)
	{
		if (info != null)
		{
			EXP_TABLE cardExpTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCardExpTable(info.Exp);
			LevelText.text = cardExpTable.n_ID.ToString();
			SetStar(info.Star);
		}
	}

	private int GetTypeIndex(int typ)
	{
		switch (typ)
		{
		case 1:
			return 0;
		case 2:
			return 1;
		case 4:
			return 2;
		case 8:
			return 3;
		case 16:
			return 4;
		default:
			return -1;
		}
	}

	public void SetColorType(int typ)
	{
		for (int i = 0; i < FrameRoots.Length; i++)
		{
			TypeRoots[i].gameObject.SetActive(i == GetTypeIndex(typ));
		}
	}

	public void SetRarity(int nRarity)
	{
		for (int i = 0; i < FrameRoots.Length; i++)
		{
			BackgroundRoots[i].gameObject.SetActive(nRarity > i);
			FrameRoots[i].gameObject.SetActive(nRarity > i);
		}
	}

	public void OnSetLockImage(bool bProtected)
	{
		LockImage.gameObject.SetActive(bProtected);
	}

	public void OnSetFavoriteImage(bool bFavorite)
	{
		if (FavoriteImage != null)
		{
			FavoriteImage.gameObject.SetActive(bFavorite);
		}
	}

	private void SetStar(int nStar)
	{
		for (int i = 0; i < FrameRoots.Length; i++)
		{
			StarRoots[i].gameObject.SetActive(nStar > i);
		}
	}

	public void SetEquipCharImage(int SeqID)
	{
		int cardEquipCharacterID = ManagedSingleton<EquipHelper>.Instance.GetCardEquipCharacterID(SeqID);
		EquipCharRoot.SetActive(cardEquipCharacterID > 0);
		if (cardEquipCharacterID <= 0)
		{
			return;
		}
		CHARACTER_TABLE characterTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(cardEquipCharacterID);
		if (characterTable == null)
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + characterTable.s_ICON), "icon_" + characterTable.s_ICON, delegate(Sprite sp)
		{
			if (sp != null)
			{
				EquipCharImage.sprite = sp;
			}
		});
	}
}
