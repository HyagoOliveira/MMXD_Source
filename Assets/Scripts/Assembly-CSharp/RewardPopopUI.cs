using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Better;
using CallbackDefs;
using OrangeUIAnimEnums;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class RewardPopopUI : OrangeUIBase
{
	private AnimStatus animState;

	private readonly Vector2[] imgBgSize = new Vector2[2]
	{
		new Vector2(1920f, 305f),
		new Vector2(1920f, 452f)
	};

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private RectTransform imgBg;

	[SerializeField]
	private RectTransform rewardContent;

	[SerializeField]
	private RewardPopupUIUnit rewardPopupUIUnit;

	[SerializeField]
	public OrangeText titleText;

	[SerializeField]
	public GameObject orgDbPos;

	[SerializeField]
	public GameObject normalReward;

	[SerializeField]
	private Button btnReward;

	[SerializeField]
	private Canvas canvasTargetItem;

	[SerializeField]
	private RewardPopupUIUnit targetReward;

	private List<NetRewardInfo> listNetGachaRewardInfo;

	private List<RewardPopupUIUnit> listUnit = new List<RewardPopupUIUnit>();

	private System.Collections.Generic.Dictionary<NetRewardInfo, int> dictNetGachaRewardInfo = new Better.Dictionary<NetRewardInfo, int>();

	private WaitForSeconds waitForSec;

	private WaitForSeconds waitForSec2;

	public SystemSE GetRewardSE = SystemSE.CRI_SYSTEMSE_SYS_EFFECT04;

	private bool createConvertDict = true;

	private int ratingScore;

	private int scoreCharacter = 60;

	private int scoreWeapon = 35;

	private int scorePickupCharacter = 100;

	private int scorePickupWeapon = 70;

	private bool isCeiling;

	private List<int> listItems = new List<int>();

	protected override void Awake()
	{
		base.Awake();
		animState = AnimStatus.LOADING;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		waitForSec = new WaitForSeconds(0.1f);
		waitForSec2 = new WaitForSeconds(0.1f);
		ratingScore = 0;
	}

	public void Setup(List<NetRewardInfo> p_listNetGachaRewardInfo, float delayTime = 0f)
	{
		animState = AnimStatus.LOADING;
		listNetGachaRewardInfo = p_listNetGachaRewardInfo;
		CraeteConvertDict();
		RecreateUnit();
		imgBg.sizeDelta = ((listNetGachaRewardInfo.Count <= 5) ? imgBgSize[0] : imgBgSize[1]);
		StartCoroutine(OnStartSetUnitData(delayTime));
	}

	private void CraeteConvertDict()
	{
		if (!createConvertDict)
		{
			return;
		}
		dictNetGachaRewardInfo.Clear();
		foreach (NetRewardInfo item in listNetGachaRewardInfo)
		{
			dictNetGachaRewardInfo.Add(item, 0);
		}
	}

	public void Setup(NetRewardsEntity p_rewardsEntity, bool p_isCeiling = false)
	{
		animState = AnimStatus.LOADING;
		createConvertDict = false;
		dictNetGachaRewardInfo.Clear();
		isCeiling = p_isCeiling;
		List<NetCharacterInfo> list = new List<NetCharacterInfo>(p_rewardsEntity.CharacterList);
		List<NetWeaponInfo> list2 = new List<NetWeaponInfo>(p_rewardsEntity.WeaponList);
		foreach (NetRewardInfo rewardInfo in p_rewardsEntity.RewardList)
		{
			int num = 0;
			switch ((RewardType)rewardInfo.RewardType)
			{
			case RewardType.Character:
			{
				NetCharacterInfo netCharacterInfo = list.FirstOrDefault((NetCharacterInfo x) => x.CharacterID == rewardInfo.RewardID);
				if (netCharacterInfo != null)
				{
					list.Remove(netCharacterInfo);
					num = -1;
				}
				else
				{
					num = OrangeConst.GACHA_CHARA_CHANGE_ITEM;
				}
				break;
			}
			case RewardType.Weapon:
			{
				NetWeaponInfo netWeaponInfo = list2.FirstOrDefault((NetWeaponInfo x) => x.WeaponID == rewardInfo.RewardID);
				if (netWeaponInfo != null)
				{
					list2.Remove(netWeaponInfo);
					num = -1;
				}
				else
				{
					num = OrangeConst.GACHA_WEAPON_CHANGE_ITEM;
				}
				break;
			}
			default:
				num = 0;
				break;
			}
			dictNetGachaRewardInfo.Add(rewardInfo, num);
		}
		Setup(p_rewardsEntity.RewardList, 0.1f);
	}

    [System.Obsolete]
    private IEnumerator OnStartSetUnitData(float delay)
	{
		yield return new WaitForSeconds(delay);
		listItems.Clear();
		animState = AnimStatus.SHOWING;
		if (GetRewardSE != 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(GetRewardSE);
		}
		int i = 0;
		int highRare = 5;
		sbyte characterType = 3;
		sbyte weaponType = 2;
		sbyte cardType = 9;
		System.Collections.Generic.Dictionary<int, GACHA_TABLE> GachaTableDict = ManagedSingleton<ExtendDataHelper>.Instance.GACHA_TABLE_DICT;
		foreach (KeyValuePair<NetRewardInfo, int> kvp in dictNetGachaRewardInfo)
		{
			yield return waitForSec;
			NetRewardInfo key = kvp.Key;
			string bundlePath = string.Empty;
			string assetPath = string.Empty;
			int rare = 0;
			bool flag = false;
			int[] rewardSpritePath = MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(key, ref bundlePath, ref assetPath, ref rare);
			GACHA_TABLE value = null;
			if (GachaTableDict.TryGetValue(key.GachaID, out value))
			{
				flag = value.n_PICKUP == 1;
			}
			if (rare == highRare)
			{
				if (key.RewardType == characterType)
				{
					ratingScore += (flag ? scorePickupCharacter : scoreCharacter);
				}
				else if (key.RewardType == weaponType)
				{
					ratingScore += (flag ? scorePickupWeapon : scoreWeapon);
				}
			}
			RewardPopupUIUnit rewardPopupUIUnit = listUnit[i];
			rewardPopupUIUnit.gameObject.SetActive(true);
			rewardPopupUIUnit.transform.SetParent(rewardContent);
			int p_idx = i;
			int p_amount = key.Amount;
			CallbackIdx p_cb;
			if (kvp.Value > 0)
			{
				p_cb = OnClickConvertUnit;
				p_idx = kvp.Value;
				p_amount = GetConvertAmount(kvp.Value, rare);
			}
			else
			{
				p_cb = OnClickUnit;
			}
			if (key.RewardType == cardType)
			{
				CARD_TABLE value2 = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(key.RewardID, out value2))
				{
					string cardTypeAssetName = ManagedSingleton<OrangeTableHelper>.Instance.GetCardTypeAssetName(value2.n_TYPE);
					rewardPopupUIUnit.SetCardType(true, AssetBundleScriptableObject.Instance.m_texture_ui_common, cardTypeAssetName);
					bundlePath = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value2.n_PATCH);
					assetPath = value2.s_ICON;
					rare = value2.n_RARITY;
				}
			}
			else
			{
				rewardPopupUIUnit.SetCardType(false);
			}
			rewardPopupUIUnit.Setup(p_idx, bundlePath, assetPath, rare, p_amount, p_cb);
			rewardPopupUIUnit.SetPieceActive(rewardSpritePath[0] == 1 && rewardSpritePath[1] == 4);
			if (rewardSpritePath[0] == 1)
			{
				listItems.Add(key.RewardID);
			}
			if (!createConvertDict)
			{
				rewardPopupUIUnit.SetConvertAnim(kvp.Value);
			}
			scrollRect.verticalNormalizedPosition = 0f;
			i++;
		}
		yield return waitForSec2;
		if (dictNetGachaRewardInfo.Count > 10 && scrollRect.content != null)
		{
			scrollRect.content.anchoredPosition = new Vector2(0f, scrollRect.content.sizeDelta.y);
		}
		if (!createConvertDict)
		{
			foreach (RewardPopupUIUnit item in listUnit)
			{
				item.PlayConvertAnim();
			}
			yield return waitForSec2;
		}
		SetTargetItem();
		yield return waitForSec2;
		animState = AnimStatus.COMPLETE;
	}

	public void OnClickScreen()
	{
		switch (animState)
		{
		case AnimStatus.SHOWING:
			animState = AnimStatus.LOADING;
			foreach (RewardPopupUIUnit item in listUnit)
			{
				item.IgonreTween();
			}
			waitForSec = null;
			break;
		case AnimStatus.COMPLETE:
			if (btnReward != null)
			{
				btnReward.onClick.Invoke();
			}
			else
			{
				OnClickCloseBtn();
			}
			break;
		case AnimStatus.LOADING:
			break;
		}
	}

	public override void OnClickCloseBtn()
	{
		if (animState == AnimStatus.COMPLETE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
			base.OnClickCloseBtn();
			if (!isCeiling && ratingScore >= 100)
			{
				ManagedSingleton<StoreHelper>.Instance.OpenStoreReview();
			}
		}
	}

	private int GetConvertAmount(int p_convertItemID, int rare)
	{
		if (p_convertItemID == OrangeConst.GACHA_CHARA_CHANGE_ITEM)
		{
			switch ((ItemRarity)(short)rare)
			{
			case ItemRarity.S:
				return OrangeConst.GACHA_CHARA_CHANGE_ITEM_S;
			case ItemRarity.A:
				return OrangeConst.GACHA_CHARA_CHANGE_ITEM_A;
			case ItemRarity.B:
				return OrangeConst.GACHA_CHARA_CHANGE_ITEM_B;
			default:
				return 0;
			}
		}
		switch ((ItemRarity)(short)rare)
		{
		case ItemRarity.S:
			return OrangeConst.GACHA_WEAPON_CHANGE_ITEM_S;
		case ItemRarity.A:
			return OrangeConst.GACHA_WEAPON_CHANGE_ITEM_A;
		case ItemRarity.B:
			return OrangeConst.GACHA_WEAPON_CHANGE_ITEM_B;
		default:
			return 0;
		}
	}

	private void RecreateUnit()
	{
		int num = 0;
		if (listUnit.Count > 0)
		{
			for (num = 0; num < listUnit.Count; num++)
			{
				Object.Destroy(listUnit[num].gameObject);
			}
			listUnit.Clear();
		}
		for (num = 0; num < listNetGachaRewardInfo.Count; num++)
		{
			RewardPopupUIUnit rewardPopupUIUnit = Object.Instantiate(this.rewardPopupUIUnit, base.transform);
			rewardPopupUIUnit.gameObject.SetActive(false);
			listUnit.Add(rewardPopupUIUnit);
		}
	}

	private void OnClickUnit(int p_idx)
	{
		NetRewardInfo netRewardInfo = listNetGachaRewardInfo[p_idx];
		switch ((RewardType)netRewardInfo.RewardType)
		{
		case RewardType.Item:
		{
			ITEM_TABLE item = null;
			if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(netRewardInfo.RewardID, out item))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(item);
				});
			}
			break;
		}
		case RewardType.Character:
		{
			CHARACTER_TABLE character = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(netRewardInfo.RewardID, out character))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(character);
				});
			}
			break;
		}
		case RewardType.Weapon:
		{
			WEAPON_TABLE weapon = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(netRewardInfo.RewardID, out weapon))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(weapon);
				});
			}
			break;
		}
		case RewardType.Card:
		{
			CARD_TABLE value = null;
			if (!ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(netRewardInfo.RewardID, out value))
			{
				break;
			}
			int nSeqID = 0;
			List<CardInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].netCardInfo.Exp == 0 && list[i].netCardInfo.CardID == value.n_ID)
				{
					nSeqID = list[i].netCardInfo.CardSeqID;
					break;
				}
			}
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardInfo", delegate(CardInfoUI ui)
			{
				ui.bOnlyShowBasic = true;
				ui.bNeedInitList = true;
				ui.nTargetCardSeqID = nSeqID;
			});
			break;
		}
		}
	}

	private void OnClickConvertUnit(int p_convertItemID)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			ui.CanShowHow2Get = false;
			ui.Setup(ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[p_convertItemID]);
		});
	}

	public void ChangeTitle(string title)
	{
		titleText.text = title;
	}

	public void OnDestroy()
	{
		StopAllCoroutines();
	}

	public void SetTargetItem()
	{
		ItemHowToGetUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<ItemHowToGetUI>("UI_ItemHowToGet");
		if (!uI)
		{
			return;
		}
		ItemHowToGetUI.TargetItem target = uI.Target;
		if (target.IsExist && listItems.Contains(target.ItemId))
		{
			ITEM_TABLE value;
			if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(target.ItemId, out value))
			{
				targetReward.IgonreTween();
				targetReward.Setup(0, AssetBundleScriptableObject.Instance.GetIconItem(value.s_ICON), value.s_ICON, value.n_RARE, 0, null);
				targetReward.SetTextAmount(target.ToDisplayString);
				canvasTargetItem.enabled = true;
			}
			else
			{
				canvasTargetItem.enabled = false;
			}
		}
		else
		{
			canvasTargetItem.enabled = false;
		}
	}
}
