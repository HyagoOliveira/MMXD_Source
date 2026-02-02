using UnityEngine;
using UnityEngine.UI;

internal class CoopRoomMember : ScrollIndexCallback
{
	[SerializeField]
	private CoopRoomMainUI Parent;

	[SerializeField]
	private FriendPVPRoomMain parentFriendPVP;

	[SerializeField]
	private Text textName;

	[SerializeField]
	private GameObject objReady;

	[SerializeField]
	private Transform stParent;

	[SerializeField]
	private CommonIconBase[] weapons;

	[SerializeField]
	private Image frameMain;

	[SerializeField]
	private Image frameSub;

	[SerializeField]
	private OrangeText textPowerValue;

	private MemberInfo memberInfo;

	private bool IgnoreFristSE = true;

	public override void ScrollCellIndex(int p_idx)
	{
		base.enabled = true;
		memberInfo = MonoBehaviourSingleton<OrangeMatchManager>.Instance.ListMemberInfo[p_idx];
		textName.text = memberInfo.Nickname;
		SetWeapon(0, memberInfo.netSealBattleSettingInfo.MainWeaponInfo, CommonIconBase.WeaponEquipType.Main);
		SetWeapon(1, memberInfo.netSealBattleSettingInfo.SubWeaponInfo, CommonIconBase.WeaponEquipType.Sub);
		UpdateBattlePower();
		CHARACTER_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(memberInfo.netSealBattleSettingInfo.CharacterList[0].CharacterID, out value))
		{
			string text = "st_" + value.s_ICON;
			SKIN_TABLE value2 = null;
			if (memberInfo.netSealBattleSettingInfo.CharacterList[0].Skin != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(memberInfo.netSealBattleSettingInfo.CharacterList[0].Skin, out value2))
			{
				text = "st_" + value2.s_ICON;
			}
			for (int num = stParent.transform.childCount - 1; num >= 0; num--)
			{
				Object.Destroy(stParent.transform.GetChild(num).gameObject);
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_texture_2d_stand_st, text), text, delegate(GameObject obj)
			{
				Object.Instantiate(obj).GetComponent<StandBase>().Setup(stParent);
			});
		}
		if (memberInfo.PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			frameMain.color = Color.white;
			frameSub.color = Color.clear;
			UpdateReadyState(memberInfo.bPrepared);
		}
		else
		{
			frameMain.color = Color.clear;
			frameSub.color = Color.white;
			UpdateReadyState(memberInfo.bPrepared);
		}
		if (IsRoomMaster())
		{
			IgnoreFristSE = false;
		}
	}

	private void LateUpdate()
	{
		if (memberInfo == null)
		{
			return;
		}
		if (memberInfo.bPrepared != objReady.gameObject.activeSelf && memberInfo.PlayerId != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			if (!IgnoreFristSE)
			{
				if (memberInfo.bPrepared)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR05);
				}
				else
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
				}
			}
			else
			{
				IgnoreFristSE = false;
			}
		}
		objReady.gameObject.SetActive(memberInfo.bPrepared);
	}

	private void SetWeapon(int idx, NetWeaponInfo netWeaponInfo, CommonIconBase.WeaponEquipType type)
	{
		WEAPON_TABLE value = null;
		if (netWeaponInfo.WeaponID != 0 && ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(netWeaponInfo.WeaponID, out value))
		{
			int num = 0;
			foreach (NetWeaponExpertInfo weaponExpert in memberInfo.netSealBattleSettingInfo.WeaponExpertList)
			{
				if (weaponExpert.WeaponID == netWeaponInfo.WeaponID)
				{
					num += weaponExpert.ExpertLevel;
				}
			}
			weapons[idx].gameObject.SetActive(true);
			weapons[idx].Setup(idx, AssetBundleScriptableObject.Instance.m_iconWeapon, value.s_ICON);
			weapons[idx].SetOtherInfo(netWeaponInfo, type, true, num, false);
		}
		else
		{
			weapons[idx].gameObject.SetActive(true);
			weapons[idx].Setup(0, "", "");
			weapons[idx].SetOtherInfo(null, type);
		}
	}

	private void UpdateReadyState(bool p_isReady)
	{
		objReady.gameObject.SetActive(p_isReady);
	}

	private void UpdateBattlePower()
	{
		WeaponInfo weaponInfo = new WeaponInfo();
		WeaponInfo weaponInfo2 = new WeaponInfo();
		weaponInfo.netInfo = memberInfo.netSealBattleSettingInfo.MainWeaponInfo;
		weaponInfo2.netInfo = memberInfo.netSealBattleSettingInfo.SubWeaponInfo;
		for (int i = 0; i < memberInfo.netSealBattleSettingInfo.WeaponExpertList.Count; i++)
		{
			weaponInfo.AddNetWeaponExpertInfo(memberInfo.netSealBattleSettingInfo.WeaponExpertList[i]);
			weaponInfo2.AddNetWeaponExpertInfo(memberInfo.netSealBattleSettingInfo.WeaponExpertList[i]);
		}
		PlayerStatus playerStatusX = ManagedSingleton<StatusHelper>.Instance.GetPlayerStatusX(memberInfo.netSealBattleSettingInfo.PlayerInfo.Exp);
		playerStatusX += ManagedSingleton<StatusHelper>.Instance.GetMemberEquipStatus(memberInfo);
		WeaponStatus weaponStatusX = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(weaponInfo, 0, false, null, null, memberInfo.netSealBattleSettingInfo.PlayerInfo.Exp);
		WeaponStatus weaponStatusX2 = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatusX(weaponInfo2, 0, false, null, null, memberInfo.netSealBattleSettingInfo.PlayerInfo.Exp);
		WeaponStatus memberChipStatus = ManagedSingleton<StatusHelper>.Instance.GetMemberChipStatus(memberInfo, memberInfo.netSealBattleSettingInfo.PlayerInfo.Exp);
		foreach (NetFinalStrikeInfo totalFS in memberInfo.netSealBattleSettingInfo.TotalFSList)
		{
			FinalStrikeInfo finalStrikeInfo = new FinalStrikeInfo();
			finalStrikeInfo.netFinalStrikeInfo = totalFS;
			playerStatusX += ManagedSingleton<StatusHelper>.Instance.GetFinalStrikeStatusX(finalStrikeInfo);
		}
		playerStatusX += ManagedSingleton<StatusHelper>.Instance.GetIllustrationStatus(memberInfo.netSealBattleSettingInfo);
		playerStatusX += ManagedSingleton<StatusHelper>.Instance.GetBackupWeaponStatus(false, memberInfo.netSealBattleSettingInfo.BenchSlotInfoList, memberInfo.netSealBattleSettingInfo.BenchWeaponInfoList, memberInfo.netSealBattleSettingInfo.WeaponExpertList, memberInfo.netSealBattleSettingInfo.WeaponSkillList);
		playerStatusX += ManagedSingleton<StatusHelper>.Instance.GetCardSystemStatus(false, memberInfo.netSealBattleSettingInfo.CharacterList[0].CharacterID, memberInfo.netSealBattleSettingInfo.CharacterList, memberInfo.netSealBattleSettingInfo.CardInfoList, memberInfo.netSealBattleSettingInfo.CharacterCardSlotInfoList);
		playerStatusX += ManagedSingleton<StatusHelper>.Instance.GetSkinStatus(memberInfo.netSealBattleSettingInfo.TotalCharacterSkinList);
		textPowerValue.text = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(weaponStatusX, weaponStatusX2, playerStatusX + memberChipStatus).ToString();
	}

	public override void BackToPool()
	{
		base.BackToPool();
		base.enabled = false;
	}

	private bool IsRoomMaster()
	{
		if ((bool)Parent)
		{
			return Parent.IsRoomMaster;
		}
		if ((bool)parentFriendPVP)
		{
			return parentFriendPVP.IsHost;
		}
		return false;
	}
}
