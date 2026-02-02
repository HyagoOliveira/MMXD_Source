using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Better;
using CallbackDefs;
using Coffee.UIExtensions;
using OrangeUIAnimEnums;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class NewRewardPopupUI : OrangeUIBase
{
	public enum PopupTheme
	{
		GoldenWithGlow = 0,
		Purple = 1,
		Blue = 2
	}

	[Serializable]
	private struct TitleColorSetting
	{
		public PopupTheme Theme;

		public Color FontColor;

		public Color GradientColor;

		public Color BorderColor;
	}

	private AnimStatus _animState;

	private readonly Vector2[] imgBgSize = new Vector2[2]
	{
		new Vector2(1920f, 305f),
		new Vector2(1920f, 452f)
	};

	[SerializeField]
	private TitleColorSetting[] _titleColorSettings;

	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private RectTransform _rewardPanel;

	[SerializeField]
	private ImageSpriteSwitcher _backgroundSwitcher;

	[SerializeField]
	private GameObject _goTitleGlow;

	[SerializeField]
	private GameObject _goTitleDragonBone;

	[SerializeField]
	private ImageSpriteSwitcher _titleCircleSwitcher;

	[SerializeField]
	private RectTransform _rewardContent;

	[SerializeField]
	private RewardPopupUIUnit _rewardPopupUnit;

	[SerializeField]
	private OrangeText _textTitle;

	[SerializeField]
	private UIGradient _textGradient;

	[SerializeField]
	private UIShadow _textBorder;

	[SerializeField]
	private Button _buttonReward;

	private List<NetRewardInfo> _rewardInfoList;

	private List<RewardPopupUIUnit> _rewardUnitList = new List<RewardPopupUIUnit>();

	private System.Collections.Generic.Dictionary<NetRewardInfo, int> _rewardInfoDict = new Better.Dictionary<NetRewardInfo, int>();

	private WaitForSeconds waitForSec;

	private WaitForSeconds waitForSec2;

	public SystemSE GetRewardSE = SystemSE.CRI_SYSTEMSE_SYS_EFFECT04;

	private bool _createConvertDict = true;

	private int _ratingScore;

	private int _scoreCharacter = 60;

	private int _scoreWeapon = 35;

	private int _scorePickupCharacter = 100;

	private int _scorePickupWeapon = 70;

	private bool isCeiling;

	protected override void Awake()
	{
		base.Awake();
		_animState = AnimStatus.LOADING;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		waitForSec = new WaitForSeconds(0.1f);
		waitForSec2 = new WaitForSeconds(0.1f);
		_ratingScore = 0;
	}

	public void Setup(List<NetRewardInfo> p_listNetGachaRewardInfo, float delayTime = 0f, PopupTheme theme = PopupTheme.GoldenWithGlow)
	{
		ChangeTheme(theme);
		_animState = AnimStatus.LOADING;
		_rewardInfoList = p_listNetGachaRewardInfo;
		CraeteConvertDict();
		RecreateUnit();
		_rewardPanel.sizeDelta = ((_rewardInfoList.Count <= 5) ? imgBgSize[0] : imgBgSize[1]);
		StartCoroutine(OnSetUnitDataCoroutine(delayTime));
	}

	public void Setup(NetRewardsEntity p_rewardsEntity, bool p_isCeiling = false, PopupTheme theme = PopupTheme.GoldenWithGlow)
	{
		_animState = AnimStatus.LOADING;
		_createConvertDict = false;
		_rewardInfoDict.Clear();
		isCeiling = p_isCeiling;
		List<NetRewardInfo> list = p_rewardsEntity.RewardList.ToList();
		List<int> list2 = p_rewardsEntity.CharacterList.Select((NetCharacterInfo character) => character.CharacterID).ToList();
		List<int> list3 = p_rewardsEntity.WeaponList.Select((NetWeaponInfo weapon) => weapon.WeaponID).ToList();
		foreach (NetRewardInfo item in list)
		{
			int num = 0;
			switch ((RewardType)item.RewardType)
			{
			case RewardType.Character:
				if (list2.Contains(item.RewardID))
				{
					list2.Remove(item.RewardID);
					num = -1;
				}
				else
				{
					num = OrangeConst.GACHA_CHARA_CHANGE_ITEM;
				}
				break;
			case RewardType.Weapon:
				if (list3.Contains(item.RewardID))
				{
					list3.Remove(item.RewardID);
					num = -1;
				}
				else
				{
					num = OrangeConst.GACHA_WEAPON_CHANGE_ITEM;
				}
				break;
			default:
				num = 0;
				break;
			}
			_rewardInfoDict.Add(item, num);
		}
		Setup(p_rewardsEntity.RewardList, 0.1f, theme);
	}

	private void CraeteConvertDict()
	{
		if (_createConvertDict)
		{
			_rewardInfoDict = Enumerable.ToDictionary(_rewardInfoList, (NetRewardInfo rewardInfo) => rewardInfo, (NetRewardInfo rewardInfo) => 0);
		}
	}

    [Obsolete]
    private IEnumerator OnSetUnitDataCoroutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		_animState = AnimStatus.SHOWING;
		if (GetRewardSE != 0)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(GetRewardSE);
		}
		int i = 0;
		int highRare = 5;
		sbyte characterType = 3;
		sbyte weaponType = 2;
		sbyte cardType = 9;
		foreach (KeyValuePair<NetRewardInfo, int> kvp in _rewardInfoDict)
		{
			yield return waitForSec;
			NetRewardInfo key = kvp.Key;
			string bundlePath = string.Empty;
			string assetPath = string.Empty;
			int rare = 0;
			bool flag = false;
			int[] rewardSpritePath = MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(key, ref bundlePath, ref assetPath, ref rare);
			GACHA_TABLE value;
			if (ManagedSingleton<ExtendDataHelper>.Instance.GACHA_TABLE_DICT.TryGetValue(key.GachaID, out value))
			{
				flag = value.n_PICKUP == 1;
			}
			if (rare == highRare)
			{
				if (key.RewardType == characterType)
				{
					_ratingScore += (flag ? _scorePickupCharacter : _scoreCharacter);
				}
				else if (key.RewardType == weaponType)
				{
					_ratingScore += (flag ? _scorePickupWeapon : _scoreWeapon);
				}
			}
			RewardPopupUIUnit rewardPopupUIUnit = _rewardUnitList[i];
			rewardPopupUIUnit.gameObject.SetActive(true);
			rewardPopupUIUnit.transform.SetParent(_rewardContent);
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
				CARD_TABLE value2;
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
			if (!_createConvertDict)
			{
				rewardPopupUIUnit.SetConvertAnim(kvp.Value);
			}
			_scrollRect.verticalNormalizedPosition = 0f;
			i++;
		}
		yield return waitForSec2;
		if (_rewardInfoDict.Count > 10 && _scrollRect.content != null)
		{
			_scrollRect.content.anchoredPosition = new Vector2(0f, _scrollRect.content.sizeDelta.y);
		}
		if (!_createConvertDict)
		{
			foreach (RewardPopupUIUnit rewardUnit in _rewardUnitList)
			{
				rewardUnit.PlayConvertAnim();
			}
			yield return waitForSec2;
		}
		yield return waitForSec2;
		_animState = AnimStatus.COMPLETE;
	}

	public void OnClickScreen()
	{
		switch (_animState)
		{
		case AnimStatus.SHOWING:
			_animState = AnimStatus.LOADING;
			foreach (RewardPopupUIUnit rewardUnit in _rewardUnitList)
			{
				rewardUnit.IgonreTween();
			}
			waitForSec = null;
			break;
		case AnimStatus.COMPLETE:
			if (_buttonReward != null)
			{
				_buttonReward.onClick.Invoke();
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
		if (_animState == AnimStatus.COMPLETE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
			base.OnClickCloseBtn();
			if (!isCeiling && _ratingScore >= 100)
			{
				ManagedSingleton<StoreHelper>.Instance.OpenStoreReview();
			}
		}
	}

	private int GetConvertAmount(int p_convertItemID, int rarity)
	{
		if (p_convertItemID == OrangeConst.GACHA_CHARA_CHANGE_ITEM)
		{
			switch ((ItemRarity)(short)rarity)
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
		switch ((ItemRarity)(short)rarity)
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
		if (_rewardUnitList.Count > 0)
		{
			for (int i = 0; i < _rewardUnitList.Count; i++)
			{
				UnityEngine.Object.Destroy(_rewardUnitList[i].gameObject);
			}
			_rewardUnitList.Clear();
		}
		for (int j = 0; j < _rewardInfoList.Count; j++)
		{
			RewardPopupUIUnit rewardPopupUIUnit = UnityEngine.Object.Instantiate(_rewardPopupUnit, base.transform);
			rewardPopupUIUnit.gameObject.SetActive(false);
			_rewardUnitList.Add(rewardPopupUIUnit);
		}
	}

	private void OnClickUnit(int p_idx)
	{
		NetRewardInfo netRewardInfo = _rewardInfoList[p_idx];
		switch ((RewardType)netRewardInfo.RewardType)
		{
		case RewardType.Item:
		{
			ITEM_TABLE item;
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
			CHARACTER_TABLE character;
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
			WEAPON_TABLE weapon;
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
			CARD_TABLE value;
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

	public void ChangeTitle(string title, FontStyle fontStyle = FontStyle.Normal)
	{
		_textTitle.text = title;
		_textTitle.fontStyle = fontStyle;
	}

	private void ChangeTheme(PopupTheme theme)
	{
		_goTitleDragonBone.SetActive(false);
		_goTitleGlow.SetActive(false);
		_titleCircleSwitcher.gameObject.SetActive(false);
		TitleColorSetting titleColorSetting = _titleColorSettings.FirstOrDefault((TitleColorSetting setting) => setting.Theme == theme);
		_textTitle.color = titleColorSetting.FontColor;
		_textGradient.color1 = titleColorSetting.FontColor;
		_textGradient.color2 = titleColorSetting.GradientColor;
		_textBorder.effectColor = titleColorSetting.BorderColor;
		switch (theme)
		{
		case PopupTheme.GoldenWithGlow:
			_goTitleDragonBone.SetActive(true);
			_goTitleGlow.SetActive(true);
			_backgroundSwitcher.ChangeImage(0);
			break;
		case PopupTheme.Purple:
			_titleCircleSwitcher.gameObject.SetActive(true);
			_titleCircleSwitcher.ChangeImage(0);
			_backgroundSwitcher.ChangeImage(1);
			break;
		case PopupTheme.Blue:
			_titleCircleSwitcher.gameObject.SetActive(true);
			_titleCircleSwitcher.ChangeImage(1);
			_backgroundSwitcher.ChangeImage(2);
			break;
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}
