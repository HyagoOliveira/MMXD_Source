using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldBossRecordUI : OrangeUIBase
{
	public GameObject refCommonIconBase;

	public GameObject BattleRecordGroup;

	public GameObject ScoreRewardGroup;

	public RectTransform SVContent01;

	public Text TitileScoreText;

	public Vector2 spacing = Vector2.zero;

	public Vector3 ToggleSpacing = Vector2.zero;

	private Vector2 currentPos = Vector2.zero;

	private bool bNextLine;

	private Vector2 lastsizeDelta;

	public void Setup(List<NetRBBattleRecord> RBBattleRecordList, int nTotleScore)
	{
		TitileScoreText.text = nTotleScore.ToString();
		for (int i = 0; i < RBBattleRecordList.Count; i++)
		{
			AddPlayerRecord(RBBattleRecordList[i].UseCharacter, RBBattleRecordList[i].UseMainWeapon, RBBattleRecordList[i].UseSubWeapon, RBBattleRecordList[i].BattleTime, RBBattleRecordList[i].Score);
		}
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private NetCharacterInfo GetNetCharacterInfo(RBCharacterInfo tRBCharacterInfo)
	{
		return new NetCharacterInfo
		{
			CharacterID = tRBCharacterInfo.CharacterID,
			Star = tRBCharacterInfo.Star,
			Skin = tRBCharacterInfo.Skin
		};
	}

	private WeaponInfo GetWeaponInfo(RBWeaponInfo tRBWeaponInfo)
	{
		WeaponInfo weaponInfo = new WeaponInfo();
		weaponInfo.netInfo = new NetWeaponInfo();
		weaponInfo.netInfo.WeaponID = tRBWeaponInfo.WeaponID;
		weaponInfo.netInfo.Exp = tRBWeaponInfo.Exp;
		weaponInfo.netInfo.Star = tRBWeaponInfo.Star;
		weaponInfo.netInfo.Skin = tRBWeaponInfo.Skin;
		return weaponInfo;
	}

	private void AddPlayerRecord(RBCharacterInfo tRBCharacterInfo, RBWeaponInfo tRBWeaponInfo0, RBWeaponInfo tRBWeaponInfo1, int nBallteTime, int nScore)
	{
		lastsizeDelta = ((RectTransform)BattleRecordGroup.transform).sizeDelta;
		Transform obj = UnityEngine.Object.Instantiate(BattleRecordGroup.transform, SVContent01);
		obj.gameObject.SetActive(true);
		obj.localPosition = currentPos;
		Transform transform = obj.transform.Find("playericonroot");
		Transform transform2 = obj.transform.Find("mainweaponroot");
		Transform transform3 = obj.transform.Find("subweaponroot");
		Text component = obj.transform.Find("RecordTimeText").GetComponent<Text>();
		Text component2 = obj.transform.Find("RecordScoreText").GetComponent<Text>();
		CommonIconBase component3 = UnityEngine.Object.Instantiate(refCommonIconBase, transform2.transform).GetComponent<CommonIconBase>();
		CommonIconBase component4 = UnityEngine.Object.Instantiate(refCommonIconBase, transform3.transform).GetComponent<CommonIconBase>();
		CommonIconBase component5 = UnityEngine.Object.Instantiate(refCommonIconBase, transform.transform).GetComponent<CommonIconBase>();
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[tRBCharacterInfo.CharacterID];
		component5.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON);
		component5.SetOtherInfoRB(tRBCharacterInfo);
		SetWeaponIcon(component3, tRBWeaponInfo0, CommonIconBase.WeaponEquipType.Main);
		SetWeaponIcon(component4, tRBWeaponInfo1, CommonIconBase.WeaponEquipType.Sub);
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
		TimeSpan timeSpan = TimeSpan.FromSeconds(nBallteTime);
		component.text = (dateTime + timeSpan).ToString("yyyy/MM/dd hh:mm tt");
		component2.text = nScore.ToString();
		SVContent01.sizeDelta = new Vector2(SVContent01.sizeDelta.x, Mathf.Abs(currentPos.y - lastsizeDelta.y - spacing.y));
		if (!bNextLine)
		{
			currentPos.x += lastsizeDelta.x + spacing.x;
		}
		else
		{
			currentPos.x = 0f;
			currentPos.y -= lastsizeDelta.y + spacing.y;
		}
		bNextLine = !bNextLine;
	}

	private void AddPlayerRecord(NetCharacterInfo tNetCharacterInfo, WeaponInfo tWeaponInfo0, WeaponInfo tWeaponInfo1, float fTime, int nScore)
	{
		lastsizeDelta = ((RectTransform)BattleRecordGroup.transform).sizeDelta;
		Transform obj = UnityEngine.Object.Instantiate(BattleRecordGroup.transform, SVContent01);
		obj.gameObject.SetActive(true);
		obj.localPosition = currentPos;
		Transform transform = obj.transform.Find("playericonroot");
		Transform transform2 = obj.transform.Find("mainweaponroot");
		Transform transform3 = obj.transform.Find("subweaponroot");
		Text component = obj.transform.Find("RecordTimeText").GetComponent<Text>();
		Text component2 = obj.transform.Find("RecordScoreText").GetComponent<Text>();
		CommonIconBase component3 = UnityEngine.Object.Instantiate(refCommonIconBase, transform2.transform).GetComponent<CommonIconBase>();
		CommonIconBase component4 = UnityEngine.Object.Instantiate(refCommonIconBase, transform3.transform).GetComponent<CommonIconBase>();
		CommonIconBase component5 = UnityEngine.Object.Instantiate(refCommonIconBase, transform.transform).GetComponent<CommonIconBase>();
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[tNetCharacterInfo.CharacterID];
		component5.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON);
		component5.SetOtherInfo(tNetCharacterInfo);
		SetWeaponIcon(component3, tWeaponInfo0, CommonIconBase.WeaponEquipType.Main);
		SetWeaponIcon(component4, tWeaponInfo1, CommonIconBase.WeaponEquipType.Sub);
		component.text = fTime.ToString("0.0");
		component2.text = nScore.ToString();
		SVContent01.sizeDelta = new Vector2(SVContent01.sizeDelta.x, Mathf.Abs(currentPos.y - lastsizeDelta.y - spacing.y));
		if (!bNextLine)
		{
			currentPos.x += lastsizeDelta.x + spacing.x;
		}
		else
		{
			currentPos.x = 0f;
			currentPos.y -= lastsizeDelta.y + spacing.y;
		}
		bNextLine = !bNextLine;
	}

	private static void SetWeaponIcon(CommonIconBase tIcon, WeaponInfo tWeaponInfo, CommonIconBase.WeaponEquipType tType)
	{
		WEAPON_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(tWeaponInfo.netInfo.WeaponID, out value))
		{
			tIcon.Setup(0, "", "");
			tIcon.SetOtherInfo(null, tType);
			return;
		}
		tIcon.gameObject.SetActive(true);
		tIcon.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, value.s_ICON);
		if (tWeaponInfo != null)
		{
			tIcon.SetOtherInfo(tWeaponInfo.netInfo, tType);
		}
		else
		{
			tIcon.SetOtherInfo(null, tType);
		}
	}

	private static void SetWeaponIcon(CommonIconBase tIcon, RBWeaponInfo tRBWeaponInfo, CommonIconBase.WeaponEquipType tType)
	{
		WEAPON_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(tRBWeaponInfo.WeaponID, out value))
		{
			tIcon.Setup(0, "", "");
			tIcon.SetOtherInfoRB(null, tType);
			return;
		}
		tIcon.gameObject.SetActive(true);
		tIcon.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, value.s_ICON);
		if (tRBWeaponInfo != null)
		{
			tIcon.SetOtherInfoRB(tRBWeaponInfo, tType);
		}
		else
		{
			tIcon.SetOtherInfoRB(null, tType);
		}
	}
}
