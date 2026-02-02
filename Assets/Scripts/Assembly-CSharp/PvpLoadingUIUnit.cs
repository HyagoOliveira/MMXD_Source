using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class PvpLoadingUIUnit : MonoBehaviour
{
	[SerializeField]
	private CommonIconBase[] weapons;

	[SerializeField]
	private OrangeText textName;

	[SerializeField]
	private PvpPlayerLoadingProgress pvpPlayerLoadingProgress;

	[SerializeField]
	private Image imgSt;

	[SerializeField]
	private Transform starsRoot;

	[SerializeField]
	private Image[] starImgs;

	private MemberInfo member;

	public void Setup(int memberIdx, Callback p_cb)
	{
		member = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[memberIdx];
		textName.font = MonoBehaviourSingleton<LocalizationManager>.Instance.ArialFont;
		textName.text = member.Nickname;
		SetWeapon(member.netSealBattleSettingInfo.MainWeaponInfo, ref weapons[0]);
		SetWeapon(member.netSealBattleSettingInfo.SubWeaponInfo, ref weapons[1]);
		pvpPlayerLoadingProgress.Setup(member.PlayerId);
		CHARACTER_TABLE characterTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(member.netSealBattleSettingInfo.CharacterList[0].CharacterID);
		SKIN_TABLE skinTable = null;
		if (GetSkin(member.netSealBattleSettingInfo.CharacterList[0].Skin, out skinTable))
		{
			SetStandIcon(skinTable.s_ICON, p_cb);
		}
		else
		{
			SetStandIcon(characterTable.s_ICON, p_cb);
		}
		if ((bool)starsRoot && starImgs.Length == 5)
		{
			int star = member.netSealBattleSettingInfo.CharacterList[0].Star;
			starsRoot.gameObject.SetActive(true);
			for (int i = 0; i < 5; i++)
			{
				starImgs[i].gameObject.SetActive(i < star);
			}
		}
	}

	public void Setup(CHARACTER_TABLE characterTable, int skinId, int starCount, Callback p_cb)
	{
		textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(characterTable.w_NAME);
		if (characterTable == null)
		{
			p_cb.CheckTargetToInvoke();
			return;
		}
		SKIN_TABLE skinTable = null;
		if (GetSkin(skinId, out skinTable))
		{
			SetStandIcon(skinTable.s_ICON, p_cb);
		}
		else
		{
			SetStandIcon(characterTable.s_ICON, p_cb);
		}
		if ((bool)starsRoot && starImgs.Length == 5)
		{
			starsRoot.gameObject.SetActive(true);
			for (int i = 0; i < 5; i++)
			{
				starImgs[i].gameObject.SetActive(i < starCount);
			}
		}
	}

	private bool GetSkin(int skinId, out SKIN_TABLE skinTable)
	{
		if (skinId > 0)
		{
			return ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(skinId, out skinTable);
		}
		skinTable = null;
		return false;
	}

	private void SetWeapon(NetWeaponInfo tNetWeaponInfo, ref CommonIconBase weaponIcon)
	{
		if (tNetWeaponInfo.WeaponID != 0)
		{
			int num = 0;
			foreach (NetWeaponExpertInfo weaponExpert in member.netSealBattleSettingInfo.WeaponExpertList)
			{
				if (weaponExpert.WeaponID == tNetWeaponInfo.WeaponID)
				{
					num += weaponExpert.ExpertLevel;
				}
			}
			weaponIcon.GetComponent<Canvas>().enabled = true;
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[tNetWeaponInfo.WeaponID];
			weaponIcon.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON);
			weaponIcon.SetOtherInfo(tNetWeaponInfo, CommonIconBase.WeaponEquipType.UnEquip, true, num, false);
			weaponIcon.EnableLevel(false);
			weaponIcon.EnableWeaponRank(false);
		}
		else
		{
			weaponIcon.GetComponent<Canvas>().enabled = false;
		}
	}

	private void SetStandIcon(string s_ICON, Callback p_cb)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter2("icon_" + s_ICON), "icon_" + s_ICON, delegate(Sprite spr)
		{
			if (spr != null)
			{
				imgSt.sprite = spr;
				imgSt.color = Color.white;
			}
			else
			{
				imgSt.color = Color.clear;
			}
			p_cb.CheckTargetToInvoke();
		});
	}
}
