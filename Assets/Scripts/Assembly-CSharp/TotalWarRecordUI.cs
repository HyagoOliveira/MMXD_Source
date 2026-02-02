using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class TotalWarRecordUI : OrangeUIBase
{
	[Header("TotalWarRecordUI")]
	public GameObject refCommonIconBase;

	public GameObject BattleRecordGroup;

	public RectTransform SVContent01;

	public Text TitileScoreText;

	public Toggle[] Toggle;

	public Vector2 spacing = Vector2.zero;

	public Vector3 ToggleSpacing = Vector2.zero;

	private Vector2 currentPos = Vector2.zero;

	private bool bNextLine;

	private Vector2 lastsizeDelta;

	[BoxGroup("Sound")]
	[Tooltip("切換tab")]
	[SerializeField]
	private SystemSE m_ToggleBtn = SystemSE.CRI_SYSTEMSE_SYS_CURSOR07;

	[SerializeField]
	[ReadOnly]
	private Toggle currentToggle;

	private List<NetTWStageRecord> listCopyRecords = new List<NetTWStageRecord>();

	private int[] nStageType;

	private bool b_first = true;

	public void Setup(List<NetTWStageRecord> listRecords, int nTotalScore)
	{
		nStageType = new int[3] { 11, 12, 13 };
		listCopyRecords.Clear();
		listCopyRecords.AddRange(listRecords);
		TitileScoreText.text = nTotalScore.ToString();
		SwitchToggleBtns(15);
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void PlaySystemSECheckFirst(SystemSE cueid)
	{
		if (b_first)
		{
			b_first = false;
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(cueid);
		}
	}

	private void AddTestPlayer()
	{
		RBCharacterInfo rBCharacterInfo = new RBCharacterInfo();
		RBWeaponInfo rBWeaponInfo = new RBWeaponInfo();
		rBCharacterInfo.CharacterID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara;
		rBWeaponInfo.WeaponID = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID;
		AddPlayerRecord(rBCharacterInfo, rBWeaponInfo, 42155, 1253);
	}

	private void SwitchToggleBtns(int nSwitchBits)
	{
		if (Toggle.Length == 0)
		{
			return;
		}
		Vector3 localPosition = Toggle[0].transform.localPosition;
		bool flag = false;
		for (int i = 0; i < Toggle.Length; i++)
		{
			if ((nSwitchBits & (1 << i)) != 0)
			{
				Toggle[i].gameObject.SetActive(true);
				Toggle[i].transform.localPosition = localPosition;
				localPosition += ToggleSpacing;
				if (!flag)
				{
					Toggle[i].isOn = true;
					flag = true;
				}
			}
			else
			{
				Toggle[i].gameObject.SetActive(false);
			}
		}
	}

	public void OneRType0Page(bool bIsOn)
	{
		if (!bIsOn)
		{
			return;
		}
		if (currentToggle != Toggle[0])
		{
			PlaySystemSECheckFirst(m_ToggleBtn);
			currentToggle = Toggle[0];
		}
		for (int num = SVContent01.transform.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(SVContent01.transform.GetChild(num).gameObject);
		}
		currentPos = Vector2.zero;
		bNextLine = false;
		for (int i = 0; i < listCopyRecords.Count; i++)
		{
			if (listCopyRecords[i].StageType == nStageType[0])
			{
				for (int j = 0; j < listCopyRecords[i].TWBattleRecords.Count && j < OrangeConst.TOTALWAR_LIST_MAX; j++)
				{
					AddPlayerRecord(listCopyRecords[i].TWBattleRecords[j].UseCharacter, listCopyRecords[i].TWBattleRecords[j].UseMainWeapon, listCopyRecords[i].TWBattleRecords[j].BattleTime, listCopyRecords[i].TWBattleRecords[j].Score);
				}
				TitileScoreText.text = listCopyRecords[i].StageScore.ToString();
			}
		}
	}

	public void OneRType1Page(bool bIsOn)
	{
		if (!bIsOn)
		{
			return;
		}
		if (currentToggle != Toggle[1])
		{
			PlaySystemSECheckFirst(m_ToggleBtn);
			currentToggle = Toggle[1];
		}
		for (int num = SVContent01.transform.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(SVContent01.transform.GetChild(num).gameObject);
		}
		currentPos = Vector2.zero;
		bNextLine = false;
		for (int i = 0; i < listCopyRecords.Count; i++)
		{
			if (listCopyRecords[i].StageType == nStageType[1])
			{
				for (int j = 0; j < listCopyRecords[i].TWBattleRecords.Count && j < OrangeConst.TOTALWAR_LIST_MAX; j++)
				{
					AddPlayerRecord(listCopyRecords[i].TWBattleRecords[j].UseCharacter, listCopyRecords[i].TWBattleRecords[j].UseMainWeapon, listCopyRecords[i].TWBattleRecords[j].BattleTime, listCopyRecords[i].TWBattleRecords[j].Score);
				}
				TitileScoreText.text = listCopyRecords[i].StageScore.ToString();
			}
		}
	}

	public void OneRType2Page(bool bIsOn)
	{
		if (!bIsOn)
		{
			return;
		}
		if (currentToggle != Toggle[2])
		{
			PlaySystemSECheckFirst(m_ToggleBtn);
			currentToggle = Toggle[2];
		}
		for (int num = SVContent01.transform.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(SVContent01.transform.GetChild(num).gameObject);
		}
		currentPos = Vector2.zero;
		bNextLine = false;
		for (int i = 0; i < listCopyRecords.Count; i++)
		{
			if (listCopyRecords[i].StageType == nStageType[2])
			{
				for (int j = 0; j < listCopyRecords[i].TWBattleRecords.Count && j < OrangeConst.TOTALWAR_LIST_MAX; j++)
				{
					AddPlayerRecord(listCopyRecords[i].TWBattleRecords[j].UseCharacter, listCopyRecords[i].TWBattleRecords[j].UseMainWeapon, listCopyRecords[i].TWBattleRecords[j].BattleTime, listCopyRecords[i].TWBattleRecords[j].Score);
				}
				TitileScoreText.text = listCopyRecords[i].StageScore.ToString();
			}
		}
	}

	private void AddPlayerRecord(RBCharacterInfo tRBCharacterInfo, RBWeaponInfo tRBWeaponInfo0, int nBallteTime, int nScore)
	{
		lastsizeDelta = ((RectTransform)BattleRecordGroup.transform).sizeDelta;
		Transform obj = UnityEngine.Object.Instantiate(BattleRecordGroup.transform, SVContent01);
		obj.gameObject.SetActive(true);
		obj.localPosition = currentPos;
		Transform transform = obj.transform.Find("playericonroot");
		Transform transform2 = obj.transform.Find("mainweaponroot");
		Text component = obj.transform.Find("RecordTimeText").GetComponent<Text>();
		Text component2 = obj.transform.Find("RecordScoreText").GetComponent<Text>();
		CommonIconBase component3 = UnityEngine.Object.Instantiate(refCommonIconBase, transform2.transform).GetComponent<CommonIconBase>();
		CommonIconBase component4 = UnityEngine.Object.Instantiate(refCommonIconBase, transform.transform).GetComponent<CommonIconBase>();
		CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[tRBCharacterInfo.CharacterID];
		component4.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON);
		component4.SetOtherInfoRB(tRBCharacterInfo, false);
		SetWeaponIcon(component3, tRBWeaponInfo0, CommonIconBase.WeaponEquipType.Main);
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
