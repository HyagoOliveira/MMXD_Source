using System.Collections;
using CallbackDefs;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class CommonIconBase : IconBase
{
	public enum WeaponRank
	{
		RANK1 = 1,
		RANK2 = 2,
		RANK3 = 3,
		RANK4 = 4,
		RANK5 = 5
	}

	public enum WeaponEquipType
	{
		UnEquip = 0,
		Main = 1,
		Sub = 2
	}

	private string rare_asset_name = "UI_iconsource_{0}_{1}";

	private string bgName = "BG";

	private string frameName = "frame";

	private string small = "_small";

	private string weaponLvlName = "UI_iconsource_powerupicon_Lv{0}";

	private string rarityImgName = "UI_Common_word_{0}";

	private string tempAssetName = "";

	private string[] strRarity = new string[7] { "Dummy", "D", "C", "B", "A", "S", "SS" };

	[SerializeField]
	private bool SmallVer = true;

	[SerializeField]
	private Image imgRareBg;

	[SerializeField]
	private Transform imgStarRoot;

	[SerializeField]
	private Image[] imgStar;

	[SerializeField]
	private Image imgPlaying;

	[SerializeField]
	private Image imgTest;

	[SerializeField]
	private Image imgWeaponLvl;

	[SerializeField]
	private Image imgMainWeapon;

	[SerializeField]
	private Image imgSubWeapon;

	[SerializeField]
	private Image imgBackWeapon;

	[SerializeField]
	private Transform starBgGroup;

	[SerializeField]
	private Transform starBgLVGroup;

	[ShowIf("SmallVer")]
	[SerializeField]
	private Image imgFragmentPiece;

	[ShowIf("SmallVer")]
	[SerializeField]
	private GameObject GroupAmount;

	[ShowIf("SmallVer")]
	[SerializeField]
	private Image imgPlus;

	[ShowIf("SmallVer")]
	[SerializeField]
	private Image imgCardType;

	[HideIf("SmallVer")]
	[SerializeField]
	private OrangeText textName;

	[HideIf("SmallVer")]
	[SerializeField]
	private Image imgRare;

	[SerializeField]
	private Image imgRareFrame;

	[SerializeField]
	private Image imgRareFrameShiny;

	[HideIf("SmallVer")]
	[SerializeField]
	private Image imgLock;

	[SerializeField]
	private Transform tipUnlockable;

	[SerializeField]
	private Transform barUnlock;

	[SerializeField]
	private Image imgBarUnlock;

	[SerializeField]
	private OrangeText textUnlockCount;

	[HideIf("SmallVer")]
	[SerializeField]
	private Image imgChip;

	[SerializeField]
	private Transform tipUsed;

	[SerializeField]
	private GameObject BonusGroup;

	[SerializeField]
	private BonusInfoTag BonusTag;

	[ShowIf("SmallVer")]
	[SerializeField]
	private GameObject FatiguedImageRoot;

	[SerializeField]
	private Image[] FatiguedImages;

	[SerializeField]
	private Image imgRedDot;

	[SerializeField]
	private Image imgFavorite;

	[SerializeField]
	private Transform TipFavorite;

	private bool bInitialized;

	private int[] FATIGUED_RANK_MIN = new int[4]
	{
		0,
		OrangeConst.FATIGUE_RANGE_1 + 1,
		OrangeConst.FATIGUE_RANGE_2 + 1,
		OrangeConst.FATIGUE_RANGE_3 + 1
	};

	private int[] FATIGUED_RANK_MAX = new int[4]
	{
		OrangeConst.FATIGUE_RANGE_1,
		OrangeConst.FATIGUE_RANGE_2,
		OrangeConst.FATIGUE_RANGE_3,
		2147483647
	};

	public bool IsEnabled
	{
		get
		{
			return imgIcon.color == white;
		}
	}

    [System.Obsolete]
    public override void Setup(int p_idx, string p_bundleName, string p_assetName, CallbackIdx clickCB = null, bool whiteColor = true)
	{
		base.Setup(p_idx, p_bundleName, p_assetName, clickCB, whiteColor);
		tempAssetName = p_assetName;
	}

	private NetCharacterInfo GetFakeCharacter(int characterId, sbyte star)
	{
		return new NetCharacterInfo
		{
			CharacterID = characterId,
			Star = star
		};
	}

    [System.Obsolete]
    public void SetItemWithAmount(int itemID, int amount, CallbackIdx clickCB = null)
	{
		SetItemWithAmount(itemID, amount, amount, clickCB);
	}

    [System.Obsolete]
    public void SetItemWithAmount(int itemID, int amountMin, int amountMax, CallbackIdx clickCB = null)
	{
		SetupItem(itemID, itemID, clickCB);
		GroupAmount.SetActive(true);
		if (amountMin == amountMax)
		{
			GroupAmount.GetComponentInChildren<OrangeText>().text = string.Format("X {0}", amountMin);
		}
		else
		{
			GroupAmount.GetComponentInChildren<OrangeText>().text = string.Format("X {0}～{1}", amountMin, amountMax);
		}
		imgPlus.gameObject.SetActive(false);
	}

    [System.Obsolete]
    public int SetupMaterial(int materialTableId, int materialIndex, CallbackIdx clickCB = null)
	{
		int num = 0;
		MATERIAL_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(materialTableId, out value))
		{
			int num2 = 0;
			bool flag = false;
			switch (materialIndex)
			{
			default:
				num = value.n_MATERIAL_MOUNT1;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_MATERIAL_1))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[value.n_MATERIAL_1].netItemInfo.Stack;
				}
				flag = ((num2 >= num) ? true : false);
				SetupItem(value.n_MATERIAL_1, value.n_MATERIAL_1, clickCB, flag);
				break;
			case 1:
				num = value.n_MATERIAL_MOUNT2;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_MATERIAL_2))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[value.n_MATERIAL_2].netItemInfo.Stack;
				}
				flag = ((num2 >= num) ? true : false);
				SetupItem(value.n_MATERIAL_2, value.n_MATERIAL_2, clickCB, flag);
				break;
			case 2:
				num = value.n_MATERIAL_MOUNT3;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_MATERIAL_3))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[value.n_MATERIAL_3].netItemInfo.Stack;
				}
				flag = ((num2 >= num) ? true : false);
				SetupItem(value.n_MATERIAL_3, value.n_MATERIAL_3, clickCB, flag);
				break;
			case 3:
				num = value.n_MATERIAL_MOUNT4;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_MATERIAL_4))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[value.n_MATERIAL_4].netItemInfo.Stack;
				}
				flag = ((num2 >= num) ? true : false);
				SetupItem(value.n_MATERIAL_4, value.n_MATERIAL_4, clickCB, flag);
				break;
			case 4:
				num = value.n_MATERIAL_MOUNT5;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_MATERIAL_5))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[value.n_MATERIAL_5].netItemInfo.Stack;
				}
				flag = ((num2 >= num) ? true : false);
				SetupItem(value.n_MATERIAL_5, value.n_MATERIAL_5, clickCB, flag);
				break;
			}
			GroupAmount.SetActive(true);
			GroupAmount.GetComponentInChildren<OrangeText>().text = string.Format("{0}/{1}", num2, num);
			if (num2 >= num)
			{
				flag = true;
				GroupAmount.GetComponentInChildren<Image>(true).gameObject.SetActive(false);
				GroupAmount.GetComponentInChildren<OrangeText>(true).color = Color.white;
			}
			else
			{
				flag = false;
				GroupAmount.GetComponentInChildren<Image>(true).gameObject.SetActive(true);
				GroupAmount.GetComponentInChildren<OrangeText>(true).color = new Color(0.99f, 0.24f, 0.23f);
			}
		}
		return num;
	}

    [System.Obsolete]
    public bool SetupMaterialEx(int materialTableId, int materialIndex, CallbackIdx clickCB = null)
	{
		bool result = false;
		MATERIAL_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(materialTableId, out value))
		{
			int num = 0;
			int num2 = 0;
			bool flag = false;
			switch (materialIndex)
			{
			default:
				num = value.n_MATERIAL_MOUNT1;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_MATERIAL_1))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[value.n_MATERIAL_1].netItemInfo.Stack;
				}
				flag = ((num2 >= num) ? true : false);
				SetupItem(value.n_MATERIAL_1, 0, clickCB, flag);
				break;
			case 1:
				num = value.n_MATERIAL_MOUNT2;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_MATERIAL_2))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[value.n_MATERIAL_2].netItemInfo.Stack;
				}
				flag = ((num2 >= num) ? true : false);
				SetupItem(value.n_MATERIAL_2, 1, clickCB, flag);
				break;
			case 2:
				num = value.n_MATERIAL_MOUNT3;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_MATERIAL_3))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[value.n_MATERIAL_3].netItemInfo.Stack;
				}
				flag = ((num2 >= num) ? true : false);
				SetupItem(value.n_MATERIAL_3, 2, clickCB, flag);
				break;
			case 3:
				num = value.n_MATERIAL_MOUNT4;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_MATERIAL_4))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[value.n_MATERIAL_4].netItemInfo.Stack;
				}
				flag = ((num2 >= num) ? true : false);
				SetupItem(value.n_MATERIAL_4, 3, clickCB, flag);
				break;
			case 4:
				num = value.n_MATERIAL_MOUNT5;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(value.n_MATERIAL_5))
				{
					num2 = ManagedSingleton<PlayerNetManager>.Instance.dicItem[value.n_MATERIAL_5].netItemInfo.Stack;
				}
				flag = ((num2 >= num) ? true : false);
				SetupItem(value.n_MATERIAL_5, 4, clickCB, flag);
				break;
			}
			GroupAmount.SetActive(true);
			GroupAmount.GetComponentInChildren<OrangeText>().text = string.Format("{0}/{1}", num2, num);
			if (num2 >= num)
			{
				flag = true;
				GroupAmount.GetComponentInChildren<Image>(true).gameObject.SetActive(false);
				GroupAmount.GetComponentInChildren<OrangeText>(true).color = Color.white;
				result = true;
			}
			else
			{
				flag = false;
				GroupAmount.GetComponentInChildren<Image>(true).gameObject.SetActive(true);
				GroupAmount.GetComponentInChildren<OrangeText>(true).color = new Color(0.99f, 0.24f, 0.23f);
				result = false;
			}
		}
		return result;
	}

    [System.Obsolete]
    public void SetupItem(int itemID, int callbackNum = 0, CallbackIdx clickCB = null, bool isUnlock = true)
	{
		ITEM_TABLE value;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(itemID, out value))
		{
			base.Setup(callbackNum, AssetBundleScriptableObject.Instance.GetIconItem(value.s_ICON), value.s_ICON, clickCB, isUnlock);
			imgFragmentPiece.gameObject.SetActive(value.n_TYPE == 4);
			imgCardType.gameObject.SetActive(false);
			tempAssetName = value.s_ICON;
			if (SmallVer)
			{
				SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[value.n_RARE]));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[value.n_RARE] + small), isUnlock);
			}
			else
			{
				SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[value.n_RARE] + "_L"));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[value.n_RARE]), isUnlock);
			}
		}
		imgPlaying.gameObject.SetActive(false);
		imgStarRoot.gameObject.SetActive(false);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		SetStar(0);
		EnableLevel(false);
		EnableWeaponRank(false);
		if ((bool)starBgGroup)
		{
			starBgGroup.gameObject.SetActive(false);
		}
	}

    [System.Obsolete]
    public void SetupSkin(CharacterInfo chinfo, NetCharacterInfo info, int skinID, int idx = 0, CallbackIdx clickCB = null)
	{
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[info.CharacterID];
		bool flag = false;
		flag = ((skinID == -1) ? (info.State == 1) : chinfo.netSkinList.Contains(skinID));
		SKIN_TABLE value = null;
		string s_ICON = cHARACTER_TABLE.s_ICON;
		if (skinID != -1 && ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(skinID, out value))
		{
			s_ICON = value.s_ICON;
		}
		base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + s_ICON), "icon_" + s_ICON, clickCB, flag);
		tempAssetName = cHARACTER_TABLE.s_ICON;
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY] + small), flag);
		}
		else
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY]), flag);
			string assetName = string.Format(rarityImgName, strRarity[cHARACTER_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME);
			if (value != null)
			{
				l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.SKINTEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
			}
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 2) + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
		}
		imgPlaying.GetComponentInChildren<OrangeText>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_IS_EQUIP");
		if (skinID != -1)
		{
			if (chinfo.netInfo.Skin == skinID)
			{
				imgPlaying.gameObject.SetActive(true);
			}
			else
			{
				imgPlaying.gameObject.SetActive(false);
			}
		}
		else if (chinfo.netInfo.Skin > 0)
		{
			imgPlaying.gameObject.SetActive(false);
		}
		if (flag)
		{
			EnableRedDot(false);
		}
		else
		{
			EnableRedDot(IsSkinUnlockable(value));
		}
		EnableLevel(false);
		starBgGroup.gameObject.SetActive(false);
		imgStarRoot.gameObject.SetActive(false);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		EnableWeaponRank(false);
	}

	private bool IsSkinUnlockable(SKIN_TABLE skinTable)
	{
		if (skinTable == null)
		{
			return false;
		}
		int n_UNLOCK_COUNT = skinTable.n_UNLOCK_COUNT;
		int num = 0;
		int n_UNLOCK_ID = skinTable.n_UNLOCK_ID;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicItem.ContainsKey(n_UNLOCK_ID))
		{
			num = ((n_UNLOCK_ID != OrangeConst.ITEMID_FREE_JEWEL) ? ManagedSingleton<PlayerNetManager>.Instance.dicItem[skinTable.n_UNLOCK_ID].netItemInfo.Stack : ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel());
		}
		return num >= n_UNLOCK_COUNT;
	}

	public void SetOtherSkinInfo(CharacterInfo characterInfo, int skinID, bool isEquipped = false)
	{
		bool flag = skinID == 0 || characterInfo.netSkinList.Contains(skinID);
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[characterInfo.netInfo.CharacterID];
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY] + small));
		}
		else
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY]));
			string assetName = string.Format(rarityImgName, strRarity[cHARACTER_TABLE.n_RARITY]);
			if (imgRare != null)
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
				{
					imgRare.sprite = obj;
					imgRare.color = white;
				});
			}
		}
		imgPlaying.gameObject.SetActive(isEquipped);
		imgStarRoot.gameObject.SetActive(true);
		imgIcon.color = (flag ? Color.white : Color.grey);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		imgStarRoot.gameObject.SetActive(false);
		EnableWeaponRank(false);
	}

	private IEnumerator UpdateRedDot(NetCharacterInfo info)
	{
		while (ManagedSingleton<CharacterHelper>.Instance.IsUpgradeChecking())
		{
			yield return null;
		}
		EnableRedDot(ManagedSingleton<CharacterHelper>.Instance.IsCharacterUpgradeAvailable(info.CharacterID));
	}

	public void EnableRedDot(bool bEnable)
	{
		if (imgRedDot != null)
		{
			imgRedDot.gameObject.SetActive(bEnable);
		}
	}

    [System.Obsolete]
    public void SetupCharacter(NetCharacterInfo inputNetCharacterInfo, int idx = 0, CallbackIdx clickCB = null, bool bSetFavorite = false)
	{
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[inputNetCharacterInfo.CharacterID];
		CharacterInfo value = null;
		bool flag = false;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(inputNetCharacterInfo.CharacterID, out value))
		{
			flag = value.netDNAInfoDic.Count >= 8;
		}
		bool flag2 = inputNetCharacterInfo.State == 1;
		bool flag3 = bSetFavorite && inputNetCharacterInfo.Favorite != 0;
		if (base.gameObject.activeSelf)
		{
			StartCoroutine(UpdateRedDot(inputNetCharacterInfo));
		}
		if (inputNetCharacterInfo.Skin > 0)
		{
			SKIN_TABLE value2 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(inputNetCharacterInfo.Skin, out value2))
			{
				tempAssetName = value2.s_ICON;
			}
			base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + tempAssetName), "icon_" + tempAssetName, clickCB, flag2 && !flag3);
			tempAssetName = cHARACTER_TABLE.s_ICON;
		}
		else
		{
			base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON, clickCB, flag2 && !flag3);
			tempAssetName = cHARACTER_TABLE.s_ICON;
		}
		if (SmallVer)
		{
			if (flag && imgRareFrameShiny != null)
			{
				imgRareFrame.gameObject.SetActive(false);
				imgRareFrameShiny.gameObject.SetActive(true);
				SetRareInfo(imgRareFrameShiny, string.Format(rare_asset_name, frameName, "UP_" + strRarity[cHARACTER_TABLE.n_RARITY]));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, "UP_" + strRarity[cHARACTER_TABLE.n_RARITY] + small), flag2 && !flag3);
			}
			else
			{
				imgRareFrame.gameObject.SetActive(true);
				imgRareFrameShiny.gameObject.SetActive(false);
				SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY]));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY] + small), flag2 && !flag3);
			}
		}
		else
		{
			if (flag && imgRareFrameShiny != null)
			{
				imgRareFrame.gameObject.SetActive(false);
				imgRareFrameShiny.gameObject.SetActive(true);
				SetRareInfo(imgRareFrameShiny, string.Format(rare_asset_name, frameName, "UP_" + strRarity[cHARACTER_TABLE.n_RARITY] + "_L"));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, "UP_" + strRarity[cHARACTER_TABLE.n_RARITY]), flag2 && !flag3);
			}
			else
			{
				imgRareFrame.gameObject.SetActive(true);
				imgRareFrameShiny.gameObject.SetActive(false);
				SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY] + "_L"));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY]), flag2 && !flag3);
			}
			string assetName = string.Format(rarityImgName, strRarity[cHARACTER_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME);
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 2) + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
		}
		if (imgFavorite != null)
		{
			imgFavorite.gameObject.SetActive(inputNetCharacterInfo.Favorite != 0);
		}
		CheckUnlockable(inputNetCharacterInfo);
		imgStarRoot.gameObject.SetActive(flag2);
		SetStar(inputNetCharacterInfo.Star);
		CHARACTER_TABLE standByChara = ManagedSingleton<OrangeTableHelper>.Instance.GetStandByChara(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara);
		imgPlaying.gameObject.SetActive(standByChara == cHARACTER_TABLE);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		EnableWeaponRank(false);
		EnableLevel(false);
		if (TipFavorite != null)
		{
			TipFavorite.gameObject.SetActive(bSetFavorite && inputNetCharacterInfo.Favorite != 0);
		}
	}

    [System.Obsolete]
    public void SetupCharacterForPlayerInfo(CharacterInfo tCharacterInfo, int idx = 0, CallbackIdx clickCB = null)
	{
		NetCharacterInfo netInfo = tCharacterInfo.netInfo;
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[netInfo.CharacterID];
		bool flag = false;
		flag = tCharacterInfo.netDNAInfoDic.Count >= 8;
		bool flag2 = netInfo.State == 1;
		if (netInfo.Skin > 0)
		{
			SKIN_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(netInfo.Skin, out value))
			{
				tempAssetName = value.s_ICON;
			}
			base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + tempAssetName), "icon_" + tempAssetName, clickCB, flag2);
			tempAssetName = cHARACTER_TABLE.s_ICON;
		}
		else
		{
			base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON, clickCB, flag2);
			tempAssetName = cHARACTER_TABLE.s_ICON;
		}
		if (SmallVer)
		{
			if (flag && imgRareFrameShiny != null)
			{
				imgRareFrame.gameObject.SetActive(false);
				imgRareFrameShiny.gameObject.SetActive(true);
				SetRareInfo(imgRareFrameShiny, string.Format(rare_asset_name, frameName, "UP_" + strRarity[cHARACTER_TABLE.n_RARITY]));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, "UP_" + strRarity[cHARACTER_TABLE.n_RARITY] + small), flag2);
			}
			else
			{
				imgRareFrame.gameObject.SetActive(true);
				imgRareFrameShiny.gameObject.SetActive(false);
				SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY]));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY] + small), flag2);
			}
		}
		else
		{
			if (flag && imgRareFrameShiny != null)
			{
				imgRareFrame.gameObject.SetActive(false);
				imgRareFrameShiny.gameObject.SetActive(true);
				SetRareInfo(imgRareFrameShiny, string.Format(rare_asset_name, frameName, "UP_" + strRarity[cHARACTER_TABLE.n_RARITY] + "_L"));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, "UP_" + strRarity[cHARACTER_TABLE.n_RARITY]), flag2);
			}
			else
			{
				imgRareFrame.gameObject.SetActive(true);
				imgRareFrameShiny.gameObject.SetActive(false);
				SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY] + "_L"));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY]), flag2);
			}
			string assetName = string.Format(rarityImgName, strRarity[cHARACTER_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME);
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 2) + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
		}
		CheckUnlockable(netInfo);
		imgStarRoot.gameObject.SetActive(flag2);
		SetStar(netInfo.Star);
		CHARACTER_TABLE standByChara = ManagedSingleton<OrangeTableHelper>.Instance.GetStandByChara(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara);
		imgPlaying.gameObject.SetActive(standByChara == cHARACTER_TABLE);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		EnableWeaponRank(false);
		EnableLevel(false);
	}

    [System.Obsolete]
    public void SetupWanted(int idx, NetCharacterInfo netCharacterInfo, CallbackIdx clickCB = null, bool isEnable = true)
	{
		CHARACTER_TABLE value;
		if (!ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(netCharacterInfo.CharacterID, out value))
		{
			return;
		}
		if (netCharacterInfo.Skin > 0)
		{
			SKIN_TABLE value2;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(netCharacterInfo.Skin, out value2))
			{
				tempAssetName = value2.s_ICON;
			}
			base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + tempAssetName), "icon_" + tempAssetName, clickCB, isEnable);
		}
		else
		{
			base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + value.s_ICON), "icon_" + value.s_ICON, clickCB, isEnable);
		}
		tempAssetName = value.s_ICON;
		if (SmallVer)
		{
			imgRareFrame.gameObject.SetActive(true);
			imgRareFrameShiny.gameObject.SetActive(false);
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[value.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[value.n_RARITY] + small), isEnable);
		}
		else
		{
			imgRareFrame.gameObject.SetActive(true);
			imgRareFrameShiny.gameObject.SetActive(false);
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[value.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[value.n_RARITY]), isEnable);
			string assetName = string.Format(rarityImgName, strRarity[value.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 2) + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
		}
		CheckUnlockable(netCharacterInfo);
		imgStarRoot.gameObject.SetActive(true);
		SetStar(netCharacterInfo.Star);
		imgMainWeapon.gameObject.SetActive(false);
		imgPlaying.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		EnableWeaponRank(false);
		EnableLevel(false);
	}

    [System.Obsolete]
    public void SetupPlayerIcon(int CharId, int idx, CallbackIdx clickCB = null)
	{
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[CharId];
		base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON, clickCB);
		tempAssetName = cHARACTER_TABLE.s_ICON;
		imgRareFrame.gameObject.SetActive(false);
		imgTest.gameObject.SetActive(false);
		imgStarRoot.gameObject.SetActive(false);
		imgPlaying.gameObject.SetActive(false);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		EnableWeaponRank(false);
		EnableLevel(false);
	}

	public void SetOtherInfo(NetCharacterInfo p_netCharacter, bool isPlaying = true, bool isTest = false, bool bUsed = false)
	{
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[p_netCharacter.CharacterID];
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY] + small), !bUsed);
		}
		else
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY]), !bUsed);
			string assetName = string.Format(rarityImgName, strRarity[cHARACTER_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME);
			string warpString = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName);
			string[] array = warpString.Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 2) + "...";
			}
			else
			{
				textName.text = warpString;
			}
		}
		imgPlaying.gameObject.SetActive(isPlaying);
		imgStarRoot.gameObject.SetActive(!isTest);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		SetStar(p_netCharacter.Star);
		EnableWeaponRank(false);
		tipUsed.gameObject.SetActive(bUsed);
		if (imgFavorite != null)
		{
			imgFavorite.gameObject.SetActive(p_netCharacter.Favorite != 0);
		}
	}

	public void SetOtherInfoRB(RBCharacterInfo tRBCharacterInfo, bool isPlaying = true, bool isTest = false)
	{
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[tRBCharacterInfo.CharacterID];
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY] + small));
		}
		else
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY]));
			string assetName = string.Format(rarityImgName, strRarity[cHARACTER_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME);
			string warpString = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName);
			string[] array = warpString.Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 2) + "...";
			}
			else
			{
				textName.text = warpString;
			}
		}
		imgPlaying.gameObject.SetActive(isPlaying);
		imgStarRoot.gameObject.SetActive(!isTest);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		SetStar(tRBCharacterInfo.Star);
		EnableWeaponRank(false);
	}

	public void SetOtherInfo(NetItemInfo p_netItem)
	{
		ITEM_TABLE value;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(p_netItem.ItemID, out value))
		{
			if (SmallVer)
			{
				SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[value.n_RARE]));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[value.n_RARE] + small));
			}
			else
			{
				SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[value.n_RARE] + "_L"));
				SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[value.n_RARE]));
			}
		}
		imgPlaying.gameObject.SetActive(false);
		imgStarRoot.gameObject.SetActive(false);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		SetStar(0);
	}

	public void SetOtherInfoWeaponFB(NetWeaponInfo netWeaponInfo, WeaponEquipType p_weaponEquipType)
	{
		WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[netWeaponInfo.WeaponID];
		imgStarRoot.gameObject.SetActive(true);
		SetStar(netWeaponInfo.Star);
		EnableWeaponRank(false);
		EnableLevel(true, ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponRank(netWeaponInfo.Exp));
		imgBackWeapon.gameObject.SetActive(false);
		imgMainWeapon.gameObject.SetActive(p_weaponEquipType == WeaponEquipType.Main);
		imgSubWeapon.gameObject.SetActive(p_weaponEquipType == WeaponEquipType.Sub);
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[wEAPON_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[wEAPON_TABLE.n_RARITY] + small));
			return;
		}
		string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(wEAPON_TABLE.w_NAME);
		string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
		if (array.Length > 1)
		{
			string text = array[0];
			textName.text = text.Substring(0, text.Length - 3) + "...";
		}
		else
		{
			textName.text = l10nValue;
		}
		SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[wEAPON_TABLE.n_RARITY] + "_L"));
		SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[wEAPON_TABLE.n_RARITY]));
		string assetName = string.Format(rarityImgName, strRarity[wEAPON_TABLE.n_RARITY]);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
		{
			imgRare.sprite = obj;
			imgRare.color = white;
		});
	}

    [System.Obsolete]
    public void SetDeepRecordWepaonInfo(NetWeaponInfo netWeaponInfo, int p_idx = 0, CallbackIdx p_cb = null)
	{
		WEAPON_TABLE value;
		if (netWeaponInfo == null || netWeaponInfo.WeaponID <= 0 || !ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(netWeaponInfo.WeaponID, out value))
		{
			Setup(0, "", "");
			SetOtherInfo(null, WeaponEquipType.UnEquip);
			EnableIsPlayingBadge(false);
		}
		else
		{
			Setup(p_idx, AssetBundleScriptableObject.Instance.m_iconWeapon, value.s_ICON, p_cb);
			SetOtherInfoWeaponFB(netWeaponInfo, WeaponEquipType.UnEquip);
			EnableIsPlayingBadge(false);
		}
	}

	public void SetOtherInfo(NetWeaponInfo p_netWeapon, WeaponEquipType p_weaponEquipType, bool bHasWeapon = false, int nSetExpertLV = -1, bool bShowBackup = true, bool bUsed = false, bool bShowRedDot = false, bool bFavoriteSet = false)
	{
		bool bUnLocked = false;
		WEAPON_TABLE wEAPON_TABLE;
		if (p_netWeapon == null)
		{
			p_netWeapon = new NetWeaponInfo();
			wEAPON_TABLE = new WEAPON_TABLE();
			wEAPON_TABLE.n_RARITY = 1;
			wEAPON_TABLE.w_NAME = "NoEquip";
			bUnLocked = true;
		}
		else
		{
			wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[p_netWeapon.WeaponID];
		}
		bool flag = false;
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.ContainsKey(p_netWeapon.WeaponID) && !bHasWeapon)
		{
			imgStarRoot.gameObject.SetActive(false);
			imgMainWeapon.gameObject.SetActive(false);
			imgSubWeapon.gameObject.SetActive(false);
			imgBackWeapon.gameObject.SetActive(false);
			EnableLevel(false);
			EnableWeaponRank(false);
			flag = false;
			int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(wEAPON_TABLE.n_UNLOCK_ID);
			int n_UNLOCK_COUNT = wEAPON_TABLE.n_UNLOCK_COUNT;
			CheckUnlockable(bUnLocked, itemValue, n_UNLOCK_COUNT);
			imgRedDot.gameObject.SetActive(false);
		}
		else
		{
			imgStarRoot.gameObject.SetActive(true);
			CheckUnlockable(true);
			SetStar(p_netWeapon.Star);
			switch (p_weaponEquipType)
			{
			case WeaponEquipType.UnEquip:
				imgMainWeapon.gameObject.SetActive(false);
				imgSubWeapon.gameObject.SetActive(false);
				imgBackWeapon.gameObject.SetActive(false);
				if (bShowBackup)
				{
					bool active = ManagedSingleton<EquipHelper>.Instance.GetWeaponBenchSlot(p_netWeapon.WeaponID) != 0;
					imgBackWeapon.gameObject.SetActive(active);
				}
				break;
			case WeaponEquipType.Main:
				imgMainWeapon.gameObject.SetActive(true);
				imgSubWeapon.gameObject.SetActive(false);
				imgBackWeapon.gameObject.SetActive(false);
				break;
			case WeaponEquipType.Sub:
				imgMainWeapon.gameObject.SetActive(false);
				imgSubWeapon.gameObject.SetActive(true);
				imgBackWeapon.gameObject.SetActive(false);
				break;
			}
			EnableLevel(true, ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponRank(p_netWeapon.Exp));
			int num = 0;
			if (nSetExpertLV == -1)
			{
				WeaponInfo weaponInfo = ManagedSingleton<PlayerNetManager>.Instance.dicWeapon[p_netWeapon.WeaponID];
				if (weaponInfo.netExpertInfos != null)
				{
					for (int i = 0; i < weaponInfo.netExpertInfos.Count; i++)
					{
						num += weaponInfo.netExpertInfos[i].ExpertLevel;
					}
				}
			}
			else
			{
				num = nSetExpertLV;
			}
			EnableWeaponRank(true, num);
			flag = true;
			imgRedDot.gameObject.SetActive(ManagedSingleton<EquipHelper>.Instance.IsCanWeaponUpgradeStart(p_netWeapon) && bShowRedDot);
			if (imgFavorite != null)
			{
				imgFavorite.gameObject.SetActive(p_netWeapon.Favorite != 0);
			}
		}
		bool bFavoriteGray = p_netWeapon.Favorite != 0 && bFavoriteSet;
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[wEAPON_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[wEAPON_TABLE.n_RARITY] + small), flag && !bUsed && !bFavoriteGray);
		}
		else
		{
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(wEAPON_TABLE.w_NAME);
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 3) + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[wEAPON_TABLE.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[wEAPON_TABLE.n_RARITY]), flag && !bUsed && !bFavoriteGray);
			string assetName = string.Format(rarityImgName, strRarity[wEAPON_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
			imgChip.gameObject.SetActive(false);
			if (p_netWeapon.Chip != 0)
			{
				DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[p_netWeapon.Chip];
				if (imgChip != null)
				{
					string assetName2 = dISC_TABLE.s_ICON + "_m";
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_iconChip, assetName2, delegate(Sprite obj)
					{
						imgChip.gameObject.SetActive(true);
						imgChip.sprite = obj;
						imgChip.color = ((!bUsed && !bFavoriteGray) ? white : grey);
					});
				}
			}
		}
		tipUsed.gameObject.SetActive(bUsed);
		if (TipFavorite != null)
		{
			TipFavorite.gameObject.SetActive(bFavoriteGray);
		}
		imgPlaying.gameObject.SetActive(false);
	}

	public void SetOtherInfoRB(RBWeaponInfo tRBWeaponInfo, WeaponEquipType p_weaponEquipType)
	{
		WEAPON_TABLE wEAPON_TABLE;
		if (tRBWeaponInfo == null)
		{
			tRBWeaponInfo = new RBWeaponInfo();
			wEAPON_TABLE = new WEAPON_TABLE();
			wEAPON_TABLE.n_RARITY = 1;
			wEAPON_TABLE.w_NAME = "NoEquip";
		}
		else
		{
			wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[tRBWeaponInfo.WeaponID];
		}
		bool flag = false;
		imgStarRoot.gameObject.SetActive(true);
		CheckUnlockable(true);
		SetStar(tRBWeaponInfo.Star);
		switch (p_weaponEquipType)
		{
		case WeaponEquipType.UnEquip:
			imgMainWeapon.gameObject.SetActive(false);
			imgSubWeapon.gameObject.SetActive(false);
			imgBackWeapon.gameObject.SetActive(false);
			break;
		case WeaponEquipType.Main:
			imgMainWeapon.gameObject.SetActive(true);
			imgSubWeapon.gameObject.SetActive(false);
			imgBackWeapon.gameObject.SetActive(false);
			break;
		case WeaponEquipType.Sub:
			imgMainWeapon.gameObject.SetActive(false);
			imgSubWeapon.gameObject.SetActive(true);
			imgBackWeapon.gameObject.SetActive(false);
			break;
		}
		EnableLevel(true, ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponRank(tRBWeaponInfo.Exp));
		EnableWeaponRank(true, tRBWeaponInfo.ExpertNum);
		flag = true;
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[wEAPON_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[wEAPON_TABLE.n_RARITY] + small), flag);
		}
		else
		{
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(wEAPON_TABLE.w_NAME);
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 3) + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[wEAPON_TABLE.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[wEAPON_TABLE.n_RARITY]), flag);
			string assetName = string.Format(rarityImgName, strRarity[wEAPON_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
		}
		imgPlaying.gameObject.SetActive(false);
	}

	public void SetOtherInfo(NetChipInfo p_netChip, bool bShowRedDot = false)
	{
		DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[p_netChip.ChipID];
		bool flag = false;
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicChip.ContainsKey(p_netChip.ChipID))
		{
			imgStarRoot.gameObject.SetActive(false);
			EnableLevel(false);
			flag = false;
			int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(dISC_TABLE.n_UNLOCK_ID);
			int n_UNLOCK_COUNT = dISC_TABLE.n_UNLOCK_COUNT;
			CheckUnlockable(false, itemValue, n_UNLOCK_COUNT);
			imgRedDot.gameObject.SetActive(false);
		}
		else
		{
			ChipInfo chipInfo = ManagedSingleton<PlayerNetManager>.Instance.dicChip[p_netChip.ChipID];
			imgStarRoot.gameObject.SetActive(true);
			CheckUnlockable(true);
			SetStar(p_netChip.Star);
			EnableLevel(true, ManagedSingleton<OrangeTableHelper>.Instance.GetChipRank(p_netChip.Exp));
			flag = true;
			imgRedDot.gameObject.SetActive((ManagedSingleton<EquipHelper>.Instance.IsCanChipUpgradeStart(p_netChip) || ManagedSingleton<EquipHelper>.Instance.IsCanChipAnalyse(p_netChip)) && bShowRedDot);
		}
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[dISC_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[dISC_TABLE.n_RARITY] + small), flag);
		}
		else
		{
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.DISCTEXT_TABLE_DICT.GetL10nValue(dISC_TABLE.w_NAME);
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 3) + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[dISC_TABLE.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[dISC_TABLE.n_RARITY]), flag);
			string assetName = string.Format(rarityImgName, strRarity[dISC_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
		}
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		EnableWeaponRank(false);
		imgPlaying.gameObject.SetActive(false);
		imgStarRoot.gameObject.SetActive(true);
	}

    [System.Obsolete]
    public void SetEquipInfo(NetEquipmentInfo p_netEquip, CallbackObj clickCB = null)
	{
		EQUIP_TABLE eQUIP_TABLE = ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT[p_netEquip.EquipItemID];
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[eQUIP_TABLE.n_RARE]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[eQUIP_TABLE.n_RARE] + small));
		}
		else
		{
			textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.DISCTEXT_TABLE_DICT.GetL10nValue(eQUIP_TABLE.w_NAME);
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[eQUIP_TABLE.n_RARE] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[eQUIP_TABLE.n_RARE]));
			string assetName = string.Format(rarityImgName, strRarity[eQUIP_TABLE.n_RARE]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
		}
		ManagedSingleton<EquipHelper>.Instance.GetEquipRank(p_netEquip);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		EnableWeaponRank(false);
		imgPlaying.gameObject.SetActive(false);
		imgStarRoot.gameObject.SetActive(false);
	}

	private void SetStar(int p_star)
	{
		for (int i = 0; i < imgStar.Length; i++)
		{
			if (p_star > i)
			{
				imgStar[i].color = white;
			}
			else
			{
				imgStar[i].color = clear;
			}
		}
	}

	public void SetAmount(int needed, int owned)
	{
		GroupAmount.SetActive(true);
		string text = string.Format("{0}/{1}", owned, needed);
		OrangeText componentInChildren = GroupAmount.GetComponentInChildren<OrangeText>();
		componentInChildren.text = text;
		if (owned >= needed)
		{
			imgPlus.gameObject.SetActive(false);
			componentInChildren.color = Color.white;
		}
		else
		{
			imgPlus.gameObject.SetActive(true);
			componentInChildren.color = Color.red;
		}
	}

	public void SetAmount(int owned)
	{
		GroupAmount.SetActive(true);
		GroupAmount.GetComponentInChildren<OrangeText>().text = string.Format("X {0}", owned);
		imgPlus.gameObject.SetActive(false);
	}

	public void EnableLevel(bool bEnable, int level = 1)
	{
		if ((bool)starBgLVGroup && (bool)starBgGroup)
		{
			if (bEnable)
			{
				starBgLVGroup.gameObject.SetActive(true);
				starBgGroup.gameObject.SetActive(false);
				starBgLVGroup.GetComponentInChildren<OrangeText>().text = level.ToString();
			}
			else
			{
				starBgLVGroup.gameObject.SetActive(false);
				starBgGroup.gameObject.SetActive(true);
			}
		}
	}

	public void EnableWeaponRank(bool bEnable, int num = 1)
	{
		if ((bool)imgWeaponLvl)
		{
			imgWeaponLvl.gameObject.SetActive(bEnable);
			imgWeaponLvl.GetComponentInChildren<Text>().text = num.ToString();
			int num2 = num / 10 + 1;
			if (num2 > 5)
			{
				num2 = 5;
			}
			string assetName = string.Format(weaponLvlName, num2);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgWeaponLvl.sprite = obj;
				imgWeaponLvl.color = white;
			});
		}
	}

    [System.Obsolete]
    public void SetupSeasonIcon(int n_ID, int n_STAR, int n_SKIN, int idx, bool bWeapon = false, CallbackIdx clickCB = null)
	{
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		if (!bWeapon)
		{
			CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[n_ID];
			tempAssetName = cHARACTER_TABLE.s_ICON;
			if (n_SKIN > 0)
			{
				SKIN_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(n_SKIN, out value))
				{
					tempAssetName = value.s_ICON;
				}
			}
			base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + tempAssetName), "icon_" + tempAssetName, clickCB);
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY] + small));
		}
		else
		{
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[n_ID];
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[wEAPON_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[wEAPON_TABLE.n_RARITY] + small));
			if (idx == 0)
			{
				imgMainWeapon.gameObject.SetActive(true);
			}
			else
			{
				imgSubWeapon.gameObject.SetActive(true);
			}
		}
		imgStarRoot.gameObject.SetActive(true);
		SetStar(n_STAR);
		imgTest.gameObject.SetActive(false);
		imgPlaying.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		EnableWeaponRank(false);
		EnableLevel(false);
	}

	public void EnableIsPlayingBadge(bool bEnable)
	{
		imgPlaying.gameObject.SetActive(bEnable);
	}

	private void CheckUnlockable(NetCharacterInfo characterInfo)
	{
		if (!(tipUnlockable == null))
		{
			CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[characterInfo.CharacterID];
			int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(cHARACTER_TABLE.n_UNLOCK_ID);
			int n_UNLOCK_COUNT = cHARACTER_TABLE.n_UNLOCK_COUNT;
			float x = Mathf.Clamp((float)itemValue / (float)n_UNLOCK_COUNT, 0f, 1f);
			if (characterInfo.State != 1)
			{
				tipUnlockable.gameObject.SetActive(itemValue >= n_UNLOCK_COUNT);
				barUnlock.gameObject.SetActive(true);
				imgBarUnlock.transform.localScale = new Vector3(x, 1f, 1f);
				textUnlockCount.text = string.Format("{0}/{1}", itemValue, n_UNLOCK_COUNT);
			}
			else
			{
				tipUnlockable.gameObject.SetActive(false);
				barUnlock.gameObject.SetActive(false);
			}
		}
	}

	private void CheckUnlockable(bool bUnLocked, int nHasCount = 1, int nNeedCount = 1)
	{
		if (!(tipUnlockable == null))
		{
			float x = 0f;
			if (nNeedCount != 0)
			{
				x = Mathf.Clamp((float)nHasCount / (float)nNeedCount, 0f, 1f);
			}
			if (!bUnLocked)
			{
				tipUnlockable.gameObject.SetActive(nHasCount >= nNeedCount);
				barUnlock.gameObject.SetActive(true);
				imgBarUnlock.transform.localScale = new Vector3(x, 1f, 1f);
				textUnlockCount.text = string.Format("{0}/{1}", nHasCount, nNeedCount);
			}
			else
			{
				tipUnlockable.gameObject.SetActive(false);
				barUnlock.gameObject.SetActive(false);
			}
		}
	}

	public void SetBonusInfoEnabled(bool b)
	{
		BonusGroup.SetActive(b);
		if (b)
		{
			BonusTag.StartRolling();
			return;
		}
		BonusTag.StopRolling();
		BonusTag.ClearContent();
	}

	public void AddBonusInfo(BonusType t, int numText)
	{
		BonusTag.AddBonusTag(t, numText);
	}

    [System.Obsolete]
    public void SetPlayerCharacterInfo(NetCharacterInfo inputNetCharacterInfo, int idx = 0, CallbackIdx clickCB = null)
	{
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[inputNetCharacterInfo.CharacterID];
		bool flag = inputNetCharacterInfo.State == 1;
		if (inputNetCharacterInfo.Skin > 0)
		{
			SKIN_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(inputNetCharacterInfo.Skin, out value))
			{
				tempAssetName = value.s_ICON;
			}
			base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + tempAssetName), "icon_" + tempAssetName, clickCB, flag);
			tempAssetName = cHARACTER_TABLE.s_ICON;
		}
		else
		{
			base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON, clickCB, flag);
			tempAssetName = cHARACTER_TABLE.s_ICON;
		}
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY] + small), flag);
		}
		else
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cHARACTER_TABLE.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cHARACTER_TABLE.n_RARITY]), flag);
			string assetName = string.Format(rarityImgName, strRarity[cHARACTER_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cHARACTER_TABLE.w_NAME);
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 2) + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
		}
		CheckUnlockable(inputNetCharacterInfo);
		imgStarRoot.gameObject.SetActive(flag);
		SetStar(inputNetCharacterInfo.Star);
		imgPlaying.gameObject.SetActive(true);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		EnableWeaponRank(false);
		EnableLevel(false);
	}

	public void SetPlayerWeaponInfo(WeaponInfo tWeaponInfo, NetWeaponInfo p_netWeapon, WeaponEquipType p_weaponEquipType, bool bHasWeapon = false, int nSetExpertLV = -1, bool bShowBackup = true)
	{
		WEAPON_TABLE wEAPON_TABLE;
		if (p_netWeapon == null)
		{
			p_netWeapon = new NetWeaponInfo();
			wEAPON_TABLE = new WEAPON_TABLE();
			wEAPON_TABLE.n_RARITY = 1;
			wEAPON_TABLE.w_NAME = "NoEquip";
		}
		else
		{
			wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[p_netWeapon.WeaponID];
		}
		bool flag = false;
		imgStarRoot.gameObject.SetActive(true);
		CheckUnlockable(true);
		SetStar(p_netWeapon.Star);
		switch (p_weaponEquipType)
		{
		case WeaponEquipType.UnEquip:
			imgMainWeapon.gameObject.SetActive(false);
			imgSubWeapon.gameObject.SetActive(false);
			imgBackWeapon.gameObject.SetActive(false);
			if (bShowBackup)
			{
				bool active = ManagedSingleton<EquipHelper>.Instance.GetWeaponBenchSlot(p_netWeapon.WeaponID) != 0;
				imgBackWeapon.gameObject.SetActive(active);
			}
			break;
		case WeaponEquipType.Main:
			imgMainWeapon.gameObject.SetActive(true);
			imgSubWeapon.gameObject.SetActive(false);
			imgBackWeapon.gameObject.SetActive(false);
			break;
		case WeaponEquipType.Sub:
			imgMainWeapon.gameObject.SetActive(false);
			imgSubWeapon.gameObject.SetActive(true);
			imgBackWeapon.gameObject.SetActive(false);
			break;
		}
		EnableLevel(true, ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponRank(p_netWeapon.Exp));
		int num = 0;
		if (nSetExpertLV == -1)
		{
			if (tWeaponInfo.netExpertInfos != null)
			{
				for (int i = 0; i < tWeaponInfo.netExpertInfos.Count; i++)
				{
					num += tWeaponInfo.netExpertInfos[i].ExpertLevel;
				}
			}
		}
		else
		{
			num = nSetExpertLV;
		}
		EnableWeaponRank(true, num);
		flag = true;
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[wEAPON_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[wEAPON_TABLE.n_RARITY] + small), flag);
		}
		else
		{
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(wEAPON_TABLE.w_NAME);
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 3) + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[wEAPON_TABLE.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[wEAPON_TABLE.n_RARITY]), flag);
			string assetName = string.Format(rarityImgName, strRarity[wEAPON_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
			imgChip.gameObject.SetActive(false);
			if (p_netWeapon.Chip != 0)
			{
				DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[p_netWeapon.Chip];
				if (imgChip != null)
				{
					string assetName2 = dISC_TABLE.s_ICON + "_m";
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_iconChip, assetName2, delegate(Sprite obj)
					{
						imgChip.gameObject.SetActive(true);
						imgChip.sprite = obj;
					});
				}
			}
		}
		imgPlaying.gameObject.SetActive(false);
	}

	public void SetPlayerChipInfo(ChipInfo tChipInfo, NetChipInfo p_netChip)
	{
		DISC_TABLE dISC_TABLE = ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT[p_netChip.ChipID];
		bool flag = false;
		imgStarRoot.gameObject.SetActive(true);
		CheckUnlockable(true);
		SetStar(p_netChip.Star);
		EnableLevel(true, ManagedSingleton<OrangeTableHelper>.Instance.GetChipRank(p_netChip.Exp));
		flag = true;
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[dISC_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[dISC_TABLE.n_RARITY] + small), flag);
		}
		else
		{
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.DISCTEXT_TABLE_DICT.GetL10nValue(dISC_TABLE.w_NAME);
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 3) + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[dISC_TABLE.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[dISC_TABLE.n_RARITY]), flag);
			string assetName = string.Format(rarityImgName, strRarity[dISC_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
		}
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		EnableWeaponRank(false);
		imgPlaying.gameObject.SetActive(false);
		imgStarRoot.gameObject.SetActive(true);
	}

	public void UpdateFatiguedImage(int FatiguedValue)
	{
		bool bTowerBase = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bTowerBase;
		FatiguedImageRoot.SetActive(bTowerBase);
		if (bTowerBase)
		{
			for (int i = 0; i < FATIGUED_RANK_MIN.Length; i++)
			{
				bool active = FatiguedValue <= FATIGUED_RANK_MAX[i] && FatiguedValue >= FATIGUED_RANK_MIN[i];
				FatiguedImages[i].gameObject.SetActive(active);
			}
		}
	}

	public void SetCardInfo(NetCardInfo p_netCardInfo)
	{
		CARD_TABLE cARD_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[p_netCardInfo.CardID];
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cARD_TABLE.n_RARITY]));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cARD_TABLE.n_RARITY] + small));
		}
		else
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[cARD_TABLE.n_RARITY] + "_L"));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[cARD_TABLE.n_RARITY]));
			string assetName = string.Format(rarityImgName, strRarity[cARD_TABLE.n_RARITY]);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
			{
				imgRare.sprite = obj;
				imgRare.color = white;
			});
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(cARD_TABLE.w_NAME);
			string warpString = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName);
			string[] array = warpString.Split('\n');
			if (array.Length > 1)
			{
				string text = array[0];
				textName.text = text.Substring(0, text.Length - 2) + "...";
			}
			else
			{
				textName.text = warpString;
			}
		}
		imgStarRoot.gameObject.SetActive(true);
		EnableWeaponRank(true, ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(p_netCardInfo.Exp));
		imgPlaying.gameObject.SetActive(false);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		SetStar(p_netCardInfo.Star);
		imgIcon.color = Color.white;
	}

    [System.Obsolete]
    public void SetItemWithAmountForCard(int itemID, int amount, CallbackIdx clickCB = null)
	{
		SetItemWithAmountForCard(itemID, amount, amount, clickCB);
	}

    [System.Obsolete]
    public void SetItemWithAmountForCard(int itemID, int amountMin, int amountMax, CallbackIdx clickCB = null)
	{
		SetupItemForCard(itemID, itemID, clickCB);
		GroupAmount.SetActive(true);
		if (amountMin == amountMax)
		{
			GroupAmount.GetComponentInChildren<OrangeText>().text = string.Format("X {0}", amountMin);
		}
		else
		{
			GroupAmount.GetComponentInChildren<OrangeText>().text = string.Format("X {0}～{1}", amountMin, amountMax);
		}
		imgPlus.gameObject.SetActive(false);
	}

    [System.Obsolete]
    public void SetupItemForCard(int itemID, int idx = 0, CallbackIdx clickCB = null, bool isUnlock = true)
	{
		ITEM_TABLE value;
		if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(itemID, out value))
		{
			CARD_TABLE value2 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)value.f_VALUE_Y, out value2))
			{
				string s_ICON = value2.s_ICON;
				string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value2.n_PATCH);
				string cardTypeAssetName = ManagedSingleton<OrangeTableHelper>.Instance.GetCardTypeAssetName(value2.n_TYPE);
				imgCardType.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, cardTypeAssetName);
				imgCardType.gameObject.SetActive(true);
				base.Setup(idx, p_bundleName, s_ICON, clickCB, isUnlock);
				imgFragmentPiece.gameObject.SetActive(value.n_TYPE == 4);
				if (SmallVer)
				{
					SetRareInfo(imgRareFrame, string.Format(rare_asset_name, frameName, strRarity[value2.n_RARITY]));
					SetRareInfo(imgRareBg, string.Format(rare_asset_name, bgName, strRarity[value2.n_RARITY] + small), isUnlock);
				}
			}
		}
		imgPlaying.gameObject.SetActive(false);
		imgStarRoot.gameObject.SetActive(false);
		imgMainWeapon.gameObject.SetActive(false);
		imgSubWeapon.gameObject.SetActive(false);
		imgBackWeapon.gameObject.SetActive(false);
		SetStar(0);
		EnableLevel(false);
		EnableWeaponRank(false);
		if ((bool)starBgGroup)
		{
			starBgGroup.gameObject.SetActive(false);
		}
	}
}
