#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class CardDeployMain : OrangeUIBase
{
	[SerializeField]
	private OrangeText m_AllATKText;

	[SerializeField]
	private OrangeText m_AllMHPText;

	[SerializeField]
	private OrangeText m_AllDEFText;

	[SerializeField]
	private OrangeText m_ATKText;

	[SerializeField]
	private OrangeText m_MHPText;

	[SerializeField]
	private OrangeText m_DEFText;

	[SerializeField]
	private GameObject[] m_CardSlotRoots;

	[SerializeField]
	private GameObject[] m_CardSlotFrameRoots;

	[SerializeField]
	private GameObject CardSkillRoot1;

	[SerializeField]
	private GameObject CardSkillRoot2;

	[SerializeField]
	private Image CardSkillImage1;

	[SerializeField]
	private Image CardSkillImage2;

	[SerializeField]
	private GameObject SkillTooltipBG;

	[SerializeField]
	private GameObject[] SkillTooltipRoots;

	[SerializeField]
	private Text[] SkillTooltipNameRoots;

	[SerializeField]
	private Text[] SkillTooltipTipRoots;

	[SerializeField]
	private Button BtnEquip;

	[SerializeField]
	private Button BtnInfo;

	[SerializeField]
	private CardSkillColor[] CardSkillColorRoot1;

	[SerializeField]
	private CardSkillColor[] CardSkillColorRoot2;

	[SerializeField]
	private GameObject[] SkillFrameRoots;

	[SerializeField]
	private GameObject[] SkillMaskCharRoots;

	[SerializeField]
	private GameObject[] SkillMaskBasicRoots;

	[SerializeField]
	private Image[] CardSkillIconCharFrameRoots;

	[SerializeField]
	private Image[] CardSkillIconBasicFrameRoots;

	[SerializeField]
	private GameObject CardBaseRoot;

	[SerializeField]
	private GameObject refPrefab;

	[SerializeField]
	private GameObject refPrefabSmall;

	[SerializeField]
	private Text CardSlotLockText0;

	[SerializeField]
	private Text CardSlotLockText3;

	[SerializeField]
	private Text CardSlotLockText5;

	[SerializeField]
	private GameObject[] TooltipSkillConditionRoots;

	[SerializeField]
	private Text[] TooltipSkillConditionName;

	[SerializeField]
	private CardSkillColor[] TooltipSkillColorRoot1;

	[SerializeField]
	private CardSkillColor[] TooltipSkillColorRoot2;

	[SerializeField]
	private CardDeployMenu CardDeployMeunRoot;

	[SerializeField]
	private GameObject CloseBackgroundRoot;

	[SerializeField]
	private GameObject BtnHLG;

	[SerializeField]
	private Button BtnRename;

	[SerializeField]
	private Button BtnDeploySave;

	[SerializeField]
	private Text CurrentTargetDeployText;

	[SerializeField]
	private Button BtnClearSet;

	private bool bIsDeploy;

	private bool bIsSetDeploy;

	public int CurrentDeployIndex = -1;

	private int[] CurrentCardDeploySeqIDBackup;

	public bool bCurrentDeploySlotDirty;

	private bool bCloseBackgroundRootFlag;

	private NetCharacterCardSlotInfo[] arrCardSlotInfo = new NetCharacterCardSlotInfo[3];

	private int CuttentTargetSlot;

	private CharacterInfo CuttentCharaterInfo;

	private NetCardInfo CurrentNetCardInfo;

	private EXP_TABLE CurrentExpTable;

	private CARD_TABLE CurrentCardTable;

	private bool initSE;

	private int bRecheckCardSlot;

	private void Update()
	{
		if (bRecheckCardSlot == CuttentTargetSlot)
		{
			return;
		}
		if (arrCardSlotInfo[CuttentTargetSlot] == null || arrCardSlotInfo[CuttentTargetSlot].CardSeqID == 0)
		{
			m_ATKText.text = "+0";
			m_MHPText.text = "+0";
			m_DEFText.text = "+0";
			CardSkillRoot1.SetActive(false);
			CardSkillRoot2.SetActive(false);
			CardBaseRoot.gameObject.SetActive(false);
			bRecheckCardSlot = CuttentTargetSlot;
		}
		else
		{
			for (int i = 0; i < m_CardSlotFrameRoots.Length; i++)
			{
				m_CardSlotFrameRoots[i].SetActive(CuttentTargetSlot == i);
			}
			InitCardInfo(arrCardSlotInfo[CuttentTargetSlot].CardSeqID);
			bRecheckCardSlot = CuttentTargetSlot;
		}
		InitButtonStatus();
	}

	public void Setup(CharacterInfo info, bool bDeploy = false, bool bSetDeploy = false)
	{
		BtnClearSet.gameObject.SetActive(true);
		bIsDeploy = bDeploy;
		bIsSetDeploy = bSetDeploy;
		if (bIsSetDeploy)
		{
			if (CurrentDeployIndex == -1)
			{
				CurrentDeployIndex = 1;
			}
			CloseBackgroundRoot.SetActive(true);
			BtnEquip.interactable = CurrentDeployIndex != -1;
			TestDeployInit();
			CardDeployMeunRoot.gameObject.SetActive(true);
			CardDeployMeunRoot.Setup(true);
		}
		else if (bIsDeploy)
		{
			CloseBackgroundRoot.SetActive(bCloseBackgroundRootFlag);
			TestDeployInit();
			CardDeployMeunRoot.gameObject.SetActive(true);
			CardDeployMeunRoot.Setup();
		}
		else
		{
			CloseBackgroundRoot.SetActive(bCloseBackgroundRootFlag);
			CardDeployMeunRoot.gameObject.SetActive(false);
		}
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		Invoke("CancelConnectingMask", 0.5f);
		if (!bSetDeploy)
		{
			ManagedSingleton<EquipHelper>.Instance.ResetCardEquipCharInfo();
		}
		CardSlotLockText0.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_UNLOCK");
		CardSlotLockText3.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_UNLOCK_RANK"), OrangeConst.CARD_EQUIPSLOT2_RANK);
		CardSlotLockText5.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_UNLOCK_RANK"), OrangeConst.CARD_EQUIPSLOT3_RANK);
		int star = info.netInfo.Star;
		CardSlotLockText0.gameObject.SetActive(info.netInfo.State != 1);
		CardSlotLockText3.gameObject.SetActive(star < OrangeConst.CARD_EQUIPSLOT2_RANK);
		CardSlotLockText5.gameObject.SetActive(star < OrangeConst.CARD_EQUIPSLOT3_RANK);
		CuttentCharaterInfo = info;
		InitCardSlotInfo(info.netInfo.CharacterID);
		InitCardSlotRoots();
		if (arrCardSlotInfo[CuttentTargetSlot] != null)
		{
			InitCardInfo(arrCardSlotInfo[CuttentTargetSlot].CardSeqID);
		}
		else
		{
			InitCardInfo(-1);
		}
		InitButtonStatus();
		if (bIsSetDeploy)
		{
			PlayerStatus playerStatus = new PlayerStatus();
			if (CurrentDeployIndex > 0)
			{
				for (int i = 0; i < arrCardSlotInfo.Length; i++)
				{
					if (arrCardSlotInfo[i] != null && ManagedSingleton<PlayerNetManager>.Instance.dicCard.ContainsKey(arrCardSlotInfo[i].CardSeqID))
					{
						CardInfo cardInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCard[arrCardSlotInfo[i].CardSeqID];
						CARD_TABLE value = null;
						ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(cardInfo.netCardInfo.CardID, out value);
						if (value != null)
						{
							CurrentNetCardInfo = cardInfo.netCardInfo;
							CurrentExpTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCardExpTable(CurrentNetCardInfo.Exp);
							CurrentCardTable = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[CurrentNetCardInfo.CardID];
							playerStatus.nATK = (int)playerStatus.nATK + (int)((float)CurrentExpTable.n_CARD_ATK * CurrentCardTable.f_PARAM_ATK * (1f + (float)CurrentNetCardInfo.Star * CurrentCardTable.f_RANKUP));
							playerStatus.nHP = (int)playerStatus.nHP + (int)((float)CurrentExpTable.n_CARD_HP * CurrentCardTable.f_PARAM_HP * (1f + (float)CurrentNetCardInfo.Star * CurrentCardTable.f_RANKUP));
							playerStatus.nDEF = (int)playerStatus.nDEF + (int)((float)CurrentExpTable.n_CARD_DEF * CurrentCardTable.f_PARAM_DEF * (1f + (float)CurrentNetCardInfo.Star * CurrentCardTable.f_RANKUP));
						}
					}
				}
			}
			m_AllATKText.text = "+" + playerStatus.nATK.ToString();
			m_AllMHPText.text = "+" + playerStatus.nHP.ToString();
			m_AllDEFText.text = "+" + playerStatus.nDEF.ToString();
		}
		else
		{
			PlayerStatus cardSystemStatus = ManagedSingleton<StatusHelper>.Instance.GetCardSystemStatus(true, info.netInfo.CharacterID);
			m_AllATKText.text = "+" + cardSystemStatus.nATK.ToString();
			m_AllMHPText.text = "+" + cardSystemStatus.nHP.ToString();
			m_AllDEFText.text = "+" + cardSystemStatus.nDEF.ToString();
		}
		BtnRename.gameObject.SetActive(CurrentDeployIndex != -1);
		CurrentTargetDeployText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CHARA_CARD_SLOT");
		if (CurrentDeployIndex != -1)
		{
			string text = "------";
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployNameInfo.ContainsKey(CurrentDeployIndex))
			{
				string text2 = ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployNameInfo[CurrentDeployIndex];
				if (text2 != null && text2 != "")
				{
					text = ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployNameInfo[CurrentDeployIndex];
				}
			}
			CurrentTargetDeployText.text = text;
		}
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void InitCardSlotInfo(int CharacterID)
	{
		arrCardSlotInfo = new NetCharacterCardSlotInfo[3];
		if (bIsSetDeploy)
		{
			if (CurrentDeployIndex <= 0 || !ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo.ContainsKey(CurrentDeployIndex))
			{
				return;
			}
			List<NetCardDeployInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo[CurrentDeployIndex].Values.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				int num = list[i].CardSlot - 1;
				if (num >= 0 && num < arrCardSlotInfo.Length)
				{
					arrCardSlotInfo[num] = new NetCharacterCardSlotInfo();
					arrCardSlotInfo[num].CharacterID = list[i].DeployId;
					arrCardSlotInfo[num].CharacterCardSlot = list[i].CardSlot;
					if (ManagedSingleton<PlayerNetManager>.Instance.dicCard.ContainsKey(list[i].CardSeqID))
					{
						arrCardSlotInfo[num].CardSeqID = list[i].CardSeqID;
					}
					else
					{
						arrCardSlotInfo[num].CardSeqID = 0;
					}
				}
			}
		}
		else
		{
			if (!ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo.ContainsKey(CharacterID))
			{
				return;
			}
			List<NetCharacterCardSlotInfo> list2 = ManagedSingleton<PlayerNetManager>.Instance.dicCharacterCardSlotInfo[CharacterID].Values.ToList();
			for (int j = 0; j < list2.Count; j++)
			{
				int num2 = list2[j].CharacterCardSlot - 1;
				if (num2 >= 0 && num2 < arrCardSlotInfo.Length)
				{
					arrCardSlotInfo[num2] = list2[j];
				}
			}
		}
	}

	private void InitCardSlotRoots()
	{
		for (int i = 0; i < arrCardSlotInfo.Length; i++)
		{
			int childCount = m_CardSlotRoots[i].transform.childCount;
			for (int j = 0; j < childCount; j++)
			{
				UnityEngine.Object.Destroy(m_CardSlotRoots[i].transform.GetChild(j).gameObject);
			}
			if (arrCardSlotInfo[i] != null && ManagedSingleton<PlayerNetManager>.Instance.dicCard.ContainsKey(arrCardSlotInfo[i].CardSeqID))
			{
				CardInfo cardInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCard[arrCardSlotInfo[i].CardSeqID];
				bool whiteColor = true;
				CardIcon componentInChildren = UnityEngine.Object.Instantiate(refPrefabSmall, m_CardSlotRoots[i].transform).GetComponentInChildren<CardIcon>();
				componentInChildren.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				componentInChildren.gameObject.SetActive(true);
				m_CardSlotRoots[i].gameObject.SetActive(true);
				CARD_TABLE value = null;
				ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(cardInfo.netCardInfo.CardID, out value);
				if (value != null)
				{
					string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
					string s_ICON = value.s_ICON;
					componentInChildren.Setup(i, p_bundleName, s_ICON, OnClick, whiteColor);
					componentInChildren.CardSetup(cardInfo.netCardInfo.CardSeqID);
				}
			}
		}
	}

	private void InitButtonStatus()
	{
		if (CuttentCharaterInfo.netInfo.State != 1)
		{
			BtnEquip.interactable = false;
			BtnInfo.interactable = false;
			return;
		}
		int[] array = new int[3]
		{
			OrangeConst.CARD_EQUIPSLOT1_RANK,
			OrangeConst.CARD_EQUIPSLOT2_RANK,
			OrangeConst.CARD_EQUIPSLOT3_RANK
		};
		if (bIsSetDeploy)
		{
			BtnEquip.interactable = CurrentDeployIndex != -1;
		}
		else
		{
			BtnEquip.interactable = CuttentCharaterInfo.netInfo.Star >= array[CuttentTargetSlot];
		}
		BtnInfo.interactable = arrCardSlotInfo[CuttentTargetSlot] != null && arrCardSlotInfo[CuttentTargetSlot].CardSeqID > 0;
	}

	private void ResetTooltipSkillInfo()
	{
		int num = 0;
		for (num = 0; num < TooltipSkillConditionName.Length; num++)
		{
			TooltipSkillConditionRoots[num].SetActive(true);
			TooltipSkillConditionName[num].gameObject.SetActive(false);
		}
		for (num = 0; num < TooltipSkillColorRoot1.Length; num++)
		{
			TooltipSkillColorRoot1[num].SetImage(-1);
			TooltipSkillColorRoot2[num].SetImage(-1);
		}
	}

	public void InitCardInfo(int nTargetCardSeqID)
	{
		CardBaseRoot.gameObject.SetActive(false);
		CardSkillRoot1.SetActive(false);
		CardSkillRoot2.SetActive(false);
		m_ATKText.text = "+0";
		m_MHPText.text = "+0";
		m_DEFText.text = "+0";
		ResetTooltipSkillInfo();
		CardInfo value = null;
		ManagedSingleton<PlayerNetManager>.Instance.dicCard.TryGetValue(nTargetCardSeqID, out value);
		if (value == null)
		{
			return;
		}
		CurrentNetCardInfo = value.netCardInfo;
		CurrentExpTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCardExpTable(CurrentNetCardInfo.Exp);
		CurrentCardTable = ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT[CurrentNetCardInfo.CardID];
		m_ATKText.text = "+" + (int)((float)CurrentExpTable.n_CARD_ATK * CurrentCardTable.f_PARAM_ATK * (1f + (float)CurrentNetCardInfo.Star * CurrentCardTable.f_RANKUP));
		m_MHPText.text = "+" + (int)((float)CurrentExpTable.n_CARD_HP * CurrentCardTable.f_PARAM_HP * (1f + (float)CurrentNetCardInfo.Star * CurrentCardTable.f_RANKUP));
		m_DEFText.text = "+" + (int)((float)CurrentExpTable.n_CARD_DEF * CurrentCardTable.f_PARAM_DEF * (1f + (float)CurrentNetCardInfo.Star * CurrentCardTable.f_RANKUP));
		int[] array = new int[6] { CurrentCardTable.n_SKILL1_RANK0, CurrentCardTable.n_SKILL1_RANK1, CurrentCardTable.n_SKILL1_RANK2, CurrentCardTable.n_SKILL1_RANK3, CurrentCardTable.n_SKILL1_RANK4, CurrentCardTable.n_SKILL1_RANK5 };
		int[] array2 = new int[6] { CurrentCardTable.n_SKILL2_RANK0, CurrentCardTable.n_SKILL2_RANK1, CurrentCardTable.n_SKILL2_RANK2, CurrentCardTable.n_SKILL2_RANK3, CurrentCardTable.n_SKILL2_RANK4, CurrentCardTable.n_SKILL2_RANK5 };
		if (CurrentNetCardInfo.Star > 5 || CurrentNetCardInfo.Star < 0)
		{
			CurrentNetCardInfo.Star = 0;
		}
		int num = array[CurrentNetCardInfo.Star];
		if (num != 0)
		{
			CardSkillRoot1.SetActive(true);
			for (int i = 0; i < CardSkillColorRoot1.Length; i++)
			{
				CardSkillColorRoot1[i].SetImage(-1);
			}
			SKILL_TABLE skillTbl2 = null;
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(num, out skillTbl2);
			if (skillTbl2 != null)
			{
				SkillTooltipNameRoots[0].text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl2.w_NAME);
				SkillTooltipTipRoots[0].text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl2.w_TIP);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(skillTbl2.s_ICON), skillTbl2.s_ICON, delegate(Sprite asset)
				{
					if (asset != null)
					{
						CardSkillImage1.sprite = asset;
					}
					else
					{
						Debug.LogWarning("SkillButton.Setup: unable to load sprite " + skillTbl2.s_ICON);
					}
				});
			}
			bool flag = false;
			flag = ((!bIsSetDeploy) ? MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCharactertCardSkillActive(CuttentCharaterInfo.netInfo.CharacterID, num, value.netCardInfo.CardSeqID) : MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCharactertCardSkillActive(0, num, nTargetCardSeqID, arrCardSlotInfo.ToList()));
			SkillFrameRoots[0].SetActive(flag);
			if (CurrentCardTable.n_SKILL1_CHARAID != 0)
			{
				SkillMaskCharRoots[0].SetActive(!flag);
				SkillMaskBasicRoots[0].SetActive(false);
				CardSkillIconCharFrameRoots[0].gameObject.SetActive(true);
				CardSkillIconBasicFrameRoots[0].gameObject.SetActive(false);
				CHARACTER_TABLE value2 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(CurrentCardTable.n_SKILL1_CHARAID, out value2);
				if (value2 != null)
				{
					TooltipSkillConditionName[0].gameObject.SetActive(true);
					TooltipSkillConditionName[0].text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(value2.w_NAME);
				}
			}
			else if (CurrentCardTable.s_SKILL1_COMBINATION != "null")
			{
				string[] array3 = CurrentCardTable.s_SKILL1_COMBINATION.Split(',');
				for (int j = 0; j < array3.Length; j++)
				{
					int image = int.Parse(array3[j]);
					CardSkillColorRoot1[j].SetImage(image);
					TooltipSkillColorRoot1[j].SetImage(image);
				}
				SkillMaskCharRoots[0].SetActive(false);
				SkillMaskBasicRoots[0].SetActive(!flag);
				CardSkillIconCharFrameRoots[0].gameObject.SetActive(false);
				CardSkillIconBasicFrameRoots[0].gameObject.SetActive(true);
			}
			else
			{
				SkillMaskCharRoots[0].SetActive(false);
				SkillMaskBasicRoots[0].SetActive(!flag);
				CardSkillIconCharFrameRoots[0].gameObject.SetActive(false);
				CardSkillIconBasicFrameRoots[0].gameObject.SetActive(true);
				TooltipSkillConditionRoots[0].SetActive(false);
			}
		}
		num = array2[CurrentNetCardInfo.Star];
		if (num != 0)
		{
			CardSkillRoot2.SetActive(true);
			for (int k = 0; k < CardSkillColorRoot2.Length; k++)
			{
				CardSkillColorRoot2[k].SetImage(-1);
			}
			SKILL_TABLE skillTbl = null;
			ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(num, out skillTbl);
			if (skillTbl != null)
			{
				SkillTooltipNameRoots[1].text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl.w_NAME);
				SkillTooltipTipRoots[1].text = ManagedSingleton<OrangeTextDataManager>.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(skillTbl.w_TIP);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(skillTbl.s_ICON), skillTbl.s_ICON, delegate(Sprite asset)
				{
					if (asset != null)
					{
						CardSkillImage2.sprite = asset;
					}
					else
					{
						Debug.LogWarning("SkillButton.Setup: unable to load sprite " + skillTbl.s_ICON);
					}
				});
			}
			bool flag2 = false;
			flag2 = ((!bIsSetDeploy) ? MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCharactertCardSkillActive(CuttentCharaterInfo.netInfo.CharacterID, num, value.netCardInfo.CardSeqID) : MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCharactertCardSkillActive(0, num, nTargetCardSeqID, arrCardSlotInfo.ToList()));
			SkillFrameRoots[1].SetActive(flag2);
			if (CurrentCardTable.n_SKILL2_CHARAID != 0)
			{
				SkillMaskCharRoots[1].SetActive(!flag2);
				SkillMaskBasicRoots[1].SetActive(false);
				CardSkillIconCharFrameRoots[1].gameObject.SetActive(true);
				CardSkillIconBasicFrameRoots[1].gameObject.SetActive(false);
				CHARACTER_TABLE value3 = null;
				ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(CurrentCardTable.n_SKILL2_CHARAID, out value3);
				if (value3 != null)
				{
					TooltipSkillConditionName[1].gameObject.SetActive(true);
					TooltipSkillConditionName[1].text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(value3.w_NAME);
				}
			}
			else if (CurrentCardTable.s_SKILL2_COMBINATION != "null")
			{
				string[] array4 = CurrentCardTable.s_SKILL2_COMBINATION.Split(',');
				for (int l = 0; l < array4.Length; l++)
				{
					int image2 = int.Parse(array4[l]);
					CardSkillColorRoot2[l].SetImage(image2);
					TooltipSkillColorRoot2[l].SetImage(image2);
				}
				SkillMaskCharRoots[1].SetActive(false);
				SkillMaskBasicRoots[1].SetActive(!flag2);
				CardSkillIconCharFrameRoots[1].gameObject.SetActive(false);
				CardSkillIconBasicFrameRoots[1].gameObject.SetActive(true);
			}
			else
			{
				SkillMaskCharRoots[1].SetActive(false);
				SkillMaskBasicRoots[1].SetActive(!flag2);
				CardSkillIconCharFrameRoots[1].gameObject.SetActive(false);
				CardSkillIconBasicFrameRoots[1].gameObject.SetActive(true);
				TooltipSkillConditionRoots[1].SetActive(false);
			}
		}
		CardBaseRoot.gameObject.SetActive(true);
		int childCount = CardBaseRoot.transform.childCount;
		for (int m = 0; m < childCount; m++)
		{
			UnityEngine.Object.Destroy(CardBaseRoot.transform.GetChild(m).gameObject);
		}
		CardBase component = UnityEngine.Object.Instantiate(refPrefab, CardBaseRoot.transform).GetComponent<CardBase>();
		component.CardSetup(CurrentNetCardInfo);
		component.SetEquipCharImage(CurrentNetCardInfo.CardSeqID);
	}

	public void CancelConnectingMask()
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
	}

	public void OnClick(int p_param)
	{
		if (CuttentTargetSlot != p_param)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
			CuttentTargetSlot = p_param;
			for (int i = 0; i < m_CardSlotFrameRoots.Length; i++)
			{
				m_CardSlotFrameRoots[i].SetActive(CuttentTargetSlot == i);
			}
			if (arrCardSlotInfo[p_param] != null)
			{
				InitCardInfo(arrCardSlotInfo[p_param].CardSeqID);
				bRecheckCardSlot = CuttentTargetSlot;
			}
			InitButtonStatus();
		}
	}

	public void OnCardEmptyClick(int p_param)
	{
		if (CuttentTargetSlot != p_param)
		{
			PlayUISE("SystemSE", 29);
			CuttentTargetSlot = p_param;
			for (int i = 0; i < m_CardSlotFrameRoots.Length; i++)
			{
				m_CardSlotFrameRoots[i].SetActive(CuttentTargetSlot == i);
			}
			if (arrCardSlotInfo[p_param] == null || arrCardSlotInfo[p_param].CardSeqID == 0)
			{
				m_ATKText.text = "+0";
				m_MHPText.text = "+0";
				m_DEFText.text = "+0";
				CardSkillRoot1.SetActive(false);
				CardSkillRoot2.SetActive(false);
				CardBaseRoot.gameObject.SetActive(false);
				bRecheckCardSlot = CuttentTargetSlot;
			}
			else
			{
				InitCardInfo(arrCardSlotInfo[p_param].CardSeqID);
				bRecheckCardSlot = CuttentTargetSlot;
			}
			InitButtonStatus();
		}
	}

	private void OnDestroy()
	{
	}

	private bool IsCharacterUnlocked()
	{
		return CuttentCharaterInfo.netInfo.State == 1;
	}

	public void OnOpenCardEquip()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardEquip", delegate(CardEquipUI ui)
		{
			if (arrCardSlotInfo == null || CuttentCharaterInfo == null)
			{
				ui.OnClickCloseBtnFix();
			}
			else
			{
				int sid = 0;
				if (arrCardSlotInfo[CuttentTargetSlot] != null)
				{
					sid = arrCardSlotInfo[CuttentTargetSlot].CardSeqID;
				}
				PlayUISE("SystemSE", 36);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(CuttentCharaterInfo.netInfo.CharacterID, CuttentTargetSlot, sid, bIsSetDeploy, CurrentDeployIndex);
			}
		});
	}

	public void OnShowSkillTooltip(int idx)
	{
		PlayUISE("SystemSE", 36);
		SkillTooltipBG.SetActive(true);
		SkillTooltipRoots[idx].SetActive(true);
	}

	public void OnHideSkillTooltip()
	{
		PlayUISE("SystemSE", 37);
		SkillTooltipBG.SetActive(false);
		SkillTooltipRoots[0].SetActive(false);
		SkillTooltipRoots[1].SetActive(false);
	}

	public void OnOpenCardInfoUI()
	{
		if (arrCardSlotInfo[CuttentTargetSlot] != null)
		{
			PlayUISE("SystemSE", 36);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardInfo", delegate(CardInfoUI ui)
			{
				ui.listHasCards = ManagedSingleton<PlayerNetManager>.Instance.dicCard.Values.ToList();
				ui.bNeedInitList = true;
				ui.nTargetCardSeqID = arrCardSlotInfo[CuttentTargetSlot].CardSeqID;
			});
		}
	}

	public void OnCardInfoUIClose()
	{
		if (this != null)
		{
			Setup(CuttentCharaterInfo, bIsDeploy, bIsSetDeploy);
			CardMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CardMainUI>("UI_CardMain");
			if (uI != null)
			{
				uI.Setup();
			}
		}
	}

	public void OnClickRules()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonScrollMsg", delegate(CommonScrollMsgUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_GROUP_RULE"));
		});
	}

	private void TestDeployInit()
	{
		bool activeSelf = CardDeployMeunRoot.gameObject.activeSelf;
	}

	public void OnShowDeploy()
	{
		CardDeployMeunRoot.gameObject.SetActive(!CardDeployMeunRoot.gameObject.activeSelf);
		CardDeployMeunRoot.Setup();
		CurrentDeployIndex = -1;
		if (CardDeployMeunRoot.gameObject.activeSelf)
		{
			bIsDeploy = true;
			TestDeployInit();
		}
		else
		{
			bIsDeploy = false;
		}
		BtnRename.gameObject.SetActive(false);
	}

	public void ChangeDeployName(string name)
	{
		ManagedSingleton<PlayerNetManager>.Instance.SetCardDeployNameReq(CurrentDeployIndex, name, delegate
		{
			CardDeployMeunRoot.Setup(true);
			CurrentTargetDeployText.text = name;
		});
	}

	public void OnChangeDeployName()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_InputText", delegate(InputTextUI ui)
		{
			ui.SetupCardDeploy(ChangeDeployName, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_DEPLOY_NAME"), CurrentTargetDeployText.text, 6);
		});
	}

	public void ResetCurrentDeployIndex()
	{
		CurrentDeployIndex = -1;
	}

	public void OnEquipDeploy()
	{
		NetCharacterInfo info = CuttentCharaterInfo.netInfo;
		int[] CardSeqIDArray = new int[3];
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo.ContainsKey(CurrentDeployIndex))
		{
			List<NetCardDeployInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo[CurrentDeployIndex].Values.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				int num = list[i].CardSlot - 1;
				if (num >= 0 && num < arrCardSlotInfo.Length)
				{
					CardSeqIDArray[num] = list[i].CardSeqID;
				}
			}
		}
		if (info.State != 1)
		{
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.EquipCardReq(CuttentCharaterInfo.netInfo.CharacterID, 1, CardSeqIDArray[0], delegate
		{
			if (info.Star >= OrangeConst.CARD_EQUIPSLOT2_RANK)
			{
				ManagedSingleton<PlayerNetManager>.Instance.EquipCardReq(CuttentCharaterInfo.netInfo.CharacterID, 2, CardSeqIDArray[1], delegate
				{
					if (info.Star >= OrangeConst.CARD_EQUIPSLOT3_RANK)
					{
						ManagedSingleton<PlayerNetManager>.Instance.EquipCardReq(CuttentCharaterInfo.netInfo.CharacterID, 3, CardSeqIDArray[2], delegate
						{
							Setup(CuttentCharaterInfo, bIsDeploy, bIsSetDeploy);
						});
					}
					else
					{
						Setup(CuttentCharaterInfo, bIsDeploy, bIsSetDeploy);
					}
				});
			}
			else
			{
				Setup(CuttentCharaterInfo, bIsDeploy, bIsSetDeploy);
			}
		});
	}

	public void OnClickDeployBtn(int idx, SystemSE clickSE = SystemSE.CRI_SYSTEMSE_SYS_CURSOR01)
	{
		CurrentDeployIndex = idx;
		if (bIsDeploy && !bIsSetDeploy)
		{
			string msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_DEPLOY_SURE"), base.name);
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ARMSSKILL_SURE"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					OnEquipDeploy();
				});
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(ResetCurrentDeployIndex));
			});
		}
		else
		{
			PlayUISE(clickSE);
			CharacterInfo characterInfo = new CharacterInfo();
			characterInfo.netInfo = new NetCharacterInfo();
			characterInfo.netInfo.CharacterID = 1;
			characterInfo.netInfo.Star = 5;
			characterInfo.netInfo.State = 1;
			BtnRename.gameObject.SetActive(true);
			Setup(characterInfo, bIsDeploy, bIsSetDeploy);
			OnDeployBackup();
		}
	}

	public int OnGetCurrentDeployIndex()
	{
		return CurrentDeployIndex;
	}

	public void OnCurrentDeploySetCard(int TargetCardSeqID)
	{
		BtnDeploySave.interactable = true;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo.ContainsKey(CurrentDeployIndex))
		{
			foreach (KeyValuePair<int, NetCardDeployInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo[CurrentDeployIndex])
			{
				if (item.Value.CardSeqID == TargetCardSeqID)
				{
					item.Value.CardSeqID = 0;
				}
			}
		}
		int num = CuttentTargetSlot + 1;
		ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo.Value(CurrentDeployIndex).Value(num).DeployId = CurrentDeployIndex;
		ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo.Value(CurrentDeployIndex).Value(num).CardSlot = (sbyte)num;
		ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo.Value(CurrentDeployIndex).Value(num).CardSeqID = TargetCardSeqID;
		bCurrentDeploySlotDirty = true;
	}

	public void OnDeploySave()
	{
		BtnDeploySave.interactable = false;
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		if (!ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo.ContainsKey(CurrentDeployIndex))
		{
			return;
		}
		List<NetCardDeployInfo> list = ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo[CurrentDeployIndex].Values.ToList();
		int[] array = new int[3];
		for (int i = 0; i < list.Count; i++)
		{
			int num = list[i].CardSlot - 1;
			if (num >= 0 && num < arrCardSlotInfo.Length)
			{
				if (ManagedSingleton<PlayerNetManager>.Instance.dicCard.ContainsKey(list[i].CardSeqID))
				{
					array[num] = list[i].CardSeqID;
				}
				else
				{
					array[num] = 0;
				}
			}
		}
		ManagedSingleton<PlayerNetManager>.Instance.SetCardDeployReq(CurrentDeployIndex, array.ToList(), delegate
		{
			CharacterInfo characterInfo = new CharacterInfo();
			characterInfo.netInfo = new NetCharacterInfo();
			characterInfo.netInfo.CharacterID = 1;
			characterInfo.netInfo.Star = 5;
			characterInfo.netInfo.State = 1;
			BtnRename.gameObject.SetActive(true);
			Setup(characterInfo, bIsDeploy, bIsSetDeploy);
			OnDeployBackup();
			CardDeployMeunRoot.Setup(true);
		});
	}

	public void OnDeployCancel()
	{
		BtnDeploySave.interactable = false;
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
		OnDeployRevert();
		Setup(CuttentCharaterInfo, bIsDeploy, bIsSetDeploy);
	}

	public void OnDeployBackup()
	{
		CurrentCardDeploySeqIDBackup = new int[3];
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo.ContainsKey(CurrentDeployIndex))
		{
			foreach (KeyValuePair<int, NetCardDeployInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo[CurrentDeployIndex])
			{
				int num = item.Key - 1;
				if (num >= 0 && num < 3)
				{
					CurrentCardDeploySeqIDBackup[num] = item.Value.CardSeqID;
				}
			}
		}
		bCurrentDeploySlotDirty = false;
	}

	public void OnDeployRevert()
	{
		foreach (KeyValuePair<int, NetCardDeployInfo> item in ManagedSingleton<PlayerNetManager>.Instance.dicCardDeployInfo[CurrentDeployIndex])
		{
			int num = item.Key - 1;
			if (num >= 0 && num < 3)
			{
				if (CurrentCardDeploySeqIDBackup == null)
				{
					item.Value.CardSeqID = 0;
				}
				else
				{
					item.Value.CardSeqID = CurrentCardDeploySeqIDBackup[num];
				}
			}
		}
		OnDeployBackup();
		BtnDeploySave.interactable = false;
	}

	public void SetCloseBackgroundRoot(bool atv)
	{
		bCloseBackgroundRootFlag = true;
		CloseBackgroundRoot.SetActive(atv);
	}

	public override void OnClickCloseBtn()
	{
		if (bCurrentDeploySlotDirty)
		{
			string msg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_DEPLOY_WARN2");
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_DEPLOY_WARN4"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					OnDeployRevert();
					base.CloseSE = SystemSE.NONE;
					base.OnClickCloseBtn();
					base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				});
			});
		}
		else
		{
			base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			base.OnClickCloseBtn();
		}
	}

	public void OnClearCurrentAllSetCard()
	{
		BtnDeploySave.interactable = false;
		if (!bIsSetDeploy)
		{
			return;
		}
		string msg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIP_CARD_UNEQUIPALL");
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
			ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), msg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
			{
				int[] source = new int[3];
				ManagedSingleton<PlayerNetManager>.Instance.SetCardDeployReq(CurrentDeployIndex, source.ToList(), delegate
				{
					CharacterInfo characterInfo = new CharacterInfo();
					characterInfo.netInfo = new NetCharacterInfo();
					characterInfo.netInfo.CharacterID = 1;
					characterInfo.netInfo.Star = 5;
					characterInfo.netInfo.State = 1;
					BtnRename.gameObject.SetActive(true);
					Setup(characterInfo, bIsDeploy, bIsSetDeploy);
					OnDeployBackup();
					CardDeployMeunRoot.Setup(true);
				});
				base.CloseSE = SystemSE.NONE;
				base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL01;
			});
		});
	}
}
