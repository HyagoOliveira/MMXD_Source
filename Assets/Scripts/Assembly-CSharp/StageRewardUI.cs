using System;
using System.Collections;
using System.Collections.Generic;
using OrangeApi;
using StageLib;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class StageRewardUI : OrangeUIBase
{
	public class BtnClickCB
	{
		public int nBtnID;

		public bool bIsLock;

		public Action<int> action;

		public void OnClick()
		{
			if (!bIsLock)
			{
				action(nBtnID);
			}
		}
	}

	[Serializable]
	public class RewardButtonRef
	{
		public GameObject SelfRoot;

		public GameObject IconRoot;

		public GameObject CompaignMark;

		public GameObject RareRoot;

		[NonSerialized]
		public CommonIconBase tCommonIconBase;
	}

	private enum SHOW_TYPE
	{
		NORMAL = 0,
		NET_PVE = 1,
		ACTIVITY = 2
	}

	private class LTDescrCtrl
	{
		public LTDescr tLinkLTDescr;
	}

	[Header("Global")]
	public StageLoadIcon StageBg;

	public StageLoadIcon CharacterImg;

	public GameObject[] BgImage;

	public GameObject tCommonIconSmallBase;

	public RewardButtonRef[][] ItemButtons;

	public float fScaleValue = 1.4f;

	public Text explabel;

	public Image[] expbarsub;

	public Text explv;

	public GameObject[] ShowType;

	public CanvasGroup[] ExpMoneyProfRoot;

	public Text[] MoneyLabel;

	public Text[] ProfLabel;

	public Text[] ExpLabel;

	[Header("ShowType0")]
	public float fGoScaleValue = 7f;

	public Color okcolor = new Color(1f, 0.8f, 0f);

	public Color failcolor = new Color(0.7607843f, 0.7607843f, 0.7607843f);

	public Text[] GoText;

	public Image[] GoOkImg;

	public Image[] GoEndImg;

	public RewardButtonRef[] ItemButtons0;

	[Header("ShowType1")]
	public GameObject refCommonSmallIcon;

	public CanvasGroup[] PlayerObj;

	public GameObject[] PlayerMainWeapon;

	public GameObject[] PlayerSubWeapon;

	public Image[] PlayerImg;

	public Text[] PlayerName;

	public Text[] PlayerPower;

	public GameObject[] ImageMVP;

	public GameObject[] ImageNotMVP;

	public Text[] Score;

	public Text[] Go1Text;

	public Image[] Go1OkImg;

	public Image[] Go1EndImg;

	public RewardButtonRef[] ItemButtons1;

	[Header("ShowType2")]
	public Text PlayTime;

	public CanvasGroup[] CleanRank;

	public GameObject[] EffectRoot;

	public Image[] CleanRankScaleBG;

	public Text CleanText;

	public Text CleanScoreText;

	private int nCleanValue;

	public RewardButtonRef[] ItemButtons2;

	public GameObject ActivityRoot;

	public GameObject TimeRoot;

	public Text TimeMinuteTxt;

	public Text TimeSecondTxt;

	public Text TimeMSecondTxt;

	public Image NewRecordImg;

	public Text ScoreOutTxt;

	[Header("left")]
	public RawImage UserModel;

	private StageEndRes refStageEndRes;

	private Animator tShowModelAniCtrl;

	private float fStageCleanTime;

	private int nNowStageStart;

	[HideInInspector]
	public Text[][] GoTextAll;

	[HideInInspector]
	public Image[][] GoOkImgAll;

	[HideInInspector]
	public Image[][] GoEndImgAll;

	[HideInInspector]
	public StageUpdate refStageUpdate;

	private RenderTextureObj textureObj;

	private bool expupse;

	private int nShowType;

	private bool bIsGoHome;

	private float fGoHomeTime;

	private string sPlayTimeColorStart = "";

	private string sPlayTimeColorEnd = "";

	private bool isGo2HomeSEPlay;

	private List<LTDescrCtrl> listLTDescrCtrl = new List<LTDescrCtrl>();

	private bool bEndRightNow;

	private void Start()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<StageRewardCell.PerRewardCell, int>(EventManager.ID.UI_UPDATESTAGEREWARD, SetStageRewardIcon);
		MonoBehaviourSingleton<InputManager>.Instance.DestroyVirtualPad();
		MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null, 0f);
		GoTextAll = new Text[2][];
		GoOkImgAll = new Image[2][];
		GoEndImgAll = new Image[2][];
		GoTextAll[0] = GoText;
		GoOkImgAll[0] = GoOkImg;
		GoEndImgAll[0] = GoEndImg;
		GoTextAll[1] = Go1Text;
		GoOkImgAll[1] = Go1OkImg;
		GoEndImgAll[1] = Go1EndImg;
		ItemButtons = new RewardButtonRef[3][];
		ItemButtons[0] = ItemButtons0;
		ItemButtons[1] = ItemButtons1;
		ItemButtons[2] = ItemButtons2;
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM00", 7);
		expupse = false;
		if (refStageEndRes != null)
		{
			int num = ManagedSingleton<PlayerHelper>.Instance.GetExp() - refStageEndRes.DisplayExp;
			EXP_TABLE expTable = ManagedSingleton<OrangeTableHelper>.Instance.GetExpTable(num);
			explv.text = expTable.n_ID.ToString();
			int num2 = (num - (expTable.n_TOTAL_RANKEXP - expTable.n_RANKEXP)) * 100 / expTable.n_RANKEXP;
			explabel.text = num2 + "%";
			int i = 0;
			for (int num3 = 100 / expbarsub.Length; i < expbarsub.Length && i * num3 < num2; i++)
			{
				expbarsub[i].gameObject.SetActive(true);
			}
			for (; i < expbarsub.Length; i++)
			{
				expbarsub[i].gameObject.SetActive(false);
			}
			for (i = 0; i < ProfLabel.Length; i++)
			{
				ProfLabel[i].text = "+" + refStageEndRes.DisplayProf;
				MoneyLabel[i].text = "+" + refStageEndRes.DisplayMoney;
				ExpLabel[i].text = "+" + refStageEndRes.DisplayExp;
			}
		}
		else
		{
			int num4 = 0;
			EXP_TABLE expTable2 = ManagedSingleton<OrangeTableHelper>.Instance.GetExpTable(num4);
			explv.text = expTable2.n_ID.ToString();
			int num5 = 100 - (expTable2.n_TOTAL_RANKEXP - num4) * 100 / expTable2.n_RANKEXP;
			explabel.text = num5 + "%";
			int j = 0;
			for (int num6 = 100 / expbarsub.Length; j < expbarsub.Length && j * num6 < num5; j++)
			{
				expbarsub[j].gameObject.SetActive(true);
			}
			for (; j < expbarsub.Length; j++)
			{
				expbarsub[j].gameObject.SetActive(false);
			}
			for (j = 0; j < ProfLabel.Length; j++)
			{
				ProfLabel[j].text = "+999";
				MoneyLabel[j].text = "+999";
				ExpLabel[j].text = "+999";
			}
		}
		for (int k = 0; k < ExpMoneyProfRoot.Length; k++)
		{
			ExpMoneyProfRoot[k].alpha = 0f;
		}
		((RectTransform)BgImage[1].transform).anchoredPosition = new Vector2(-3000f, 0f);
		((RectTransform)BgImage[2].transform).anchoredPosition = new Vector2(3000f, 0f);
		if (!StageUpdate.gbIsNetGame)
		{
			if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0)
			{
				STAGE_TABLE sTAGE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[ManagedSingleton<StageHelper>.Instance.nLastStageID];
				switch ((StageType)(short)sTAGE_TABLE.n_TYPE)
				{
				case StageType.Scenario:
					nShowType = 0;
					break;
				case StageType.Activity:
					nShowType = 2;
					ActivityRoot.SetActive(true);
					TimeRoot.SetActive(false);
					if (sTAGE_TABLE.n_MAIN == 15001)
					{
						CleanText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACHIEVEMENT_TOTAL_SCORE");
						CleanScoreText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_POINT");
					}
					else if (sTAGE_TABLE.n_MAIN == 15011)
					{
						CleanText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("STAGE_CLEAR_TIME");
						CleanScoreText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIME_SECOND");
					}
					else if (sTAGE_TABLE.n_MAIN == 15021)
					{
						CleanText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACHIEVEMENT_TOTAL_SCORE");
						CleanScoreText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_POINT");
					}
					else if (sTAGE_TABLE.n_MAIN == 15031)
					{
						CleanText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ACHIEVEMENT_TOTAL_SCORE");
						CleanScoreText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("EVENT_STAGE_POINT");
					}
					break;
				case StageType.TimeAttack:
				case StageType.BossRush:
					nShowType = 2;
					ActivityRoot.SetActive(false);
					TimeRoot.SetActive(true);
					CleanText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("STAGE_CLEAR_TIME");
					break;
				case StageType.RaidBoss:
					nShowType = 2;
					ActivityRoot.SetActive(true);
					TimeRoot.SetActive(false);
					CleanText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_CONTRIBUTION");
					CleanScoreText.text = "";
					break;
				case StageType.Crusade:
					if (StageUpdate.StageMode == StageMode.Contribute)
					{
						nShowType = 2;
						ActivityRoot.SetActive(true);
						TimeRoot.SetActive(false);
						CleanText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_CONTRIBUTION");
						CleanScoreText.text = "";
					}
					else
					{
						nShowType = 0;
					}
					break;
				case StageType.TWSuppress:
				case StageType.TWLightning:
				case StageType.TWCrusade:
					nShowType = 2;
					ActivityRoot.SetActive(true);
					TimeRoot.SetActive(false);
					CleanText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TOTALWAR_RANK_POINT");
					CleanScoreText.text = "";
					break;
				default:
					nShowType = 0;
					break;
				}
			}
			else
			{
				nShowType = 0;
			}
		}
		else
		{
			nShowType = 1;
		}
		ShowType[0].SetActive(nShowType == 0);
		ShowType[1].SetActive(nShowType == 1);
		ShowType[2].SetActive(nShowType == 2);
		for (int l = 0; l < ItemButtons[nShowType].Length; l++)
		{
			SetStageRewardItemButton(ItemButtons[nShowType][l], l);
		}
		PlayTime.text = "";
		for (int m = 0; m < CleanRank.Length; m++)
		{
			CleanRank[m].alpha = 0f;
			EffectRoot[m].SetActive(false);
		}
		if (NewRecordImg != null)
		{
			NewRecordImg.gameObject.SetActive(false);
			RotateSelf[] componentsInChildren = NewRecordImg.transform.GetComponentsInChildren<RotateSelf>(true);
			for (int n = 0; n < componentsInChildren.Length; n++)
			{
				componentsInChildren[n].enabled = false;
			}
		}
		if (ScoreOutTxt != null)
		{
			ScoreOutTxt.gameObject.SetActive(false);
		}
		if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0)
		{
			nNowStageStart = refStageUpdate.GetNowStageStart();
			STAGE_TABLE p_stage = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[ManagedSingleton<StageHelper>.Instance.nLastStageID];
			if (!ManagedSingleton<PlayerNetManager>.Instance.dicStage.ContainsKey(ManagedSingleton<StageHelper>.Instance.nLastStageID))
			{
				return;
			}
			if (GoTextAll.Length > nShowType)
			{
				string[] stageClearMsg = ManagedSingleton<StageHelper>.Instance.GetStageClearMsg(p_stage);
				for (int num7 = 0; num7 < 3; num7++)
				{
					GoTextAll[nShowType][num7].text = stageClearMsg[num7];
					GoTextAll[nShowType][num7].color = failcolor;
					GoOkImgAll[nShowType][num7].gameObject.SetActive(false);
					GoEndImgAll[nShowType][num7].gameObject.SetActive(false);
				}
			}
			string text = ManagedSingleton<StageHelper>.Instance.nLastStageID.ToString();
			text = text.Substring(0, text.Length - 2);
			text = "bg_stage_" + int.Parse(text).ToString("000");
			StageBg.CheckLoadT<Sprite>("ui/background/" + text, text);
			string empty = string.Empty;
			CharacterImg.enabled = false;
			CharacterImg.transform.localRotation = Quaternion.identity;
			((RectTransform)CharacterImg.transform).anchoredPosition = ((RectTransform)CharacterImg.transform).anchoredPosition + new Vector2(-195f, 585f);
			StageHelper.StageCharacterStruct stageCharacterStruct = ManagedSingleton<StageHelper>.Instance.GetStageCharacterStruct();
			empty = ((stageCharacterStruct.Skin <= 0) ? ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(stageCharacterStruct.StandbyChara).s_ICON : ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[stageCharacterStruct.Skin].s_ICON);
			CharacterImg.CheckLoadPerfab(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, empty), empty + "_db", StartMoveBgImage);
		}
		else
		{
			for (int num8 = 0; num8 < 3; num8++)
			{
				GoTextAll[nShowType][num8].color = failcolor;
				GoOkImgAll[nShowType][num8].gameObject.SetActive(false);
				GoEndImgAll[nShowType][num8].gameObject.SetActive(false);
			}
			StageBg.CheckLoadT<Image>("ui/background/bg_stage_001", "bg_stage_001");
			CharacterImg.enabled = false;
			CharacterImg.transform.localRotation = Quaternion.identity;
			((RectTransform)CharacterImg.transform).anchoredPosition = ((RectTransform)CharacterImg.transform).anchoredPosition + new Vector2(-195f, 585f);
			CharacterImg.CheckLoadPerfab("texture/2d/stand/St_ch_006_000", "St_ch_006_000", StartMoveBgImage);
		}
		if (StageUpdate.gbGeneratePvePlayer)
		{
			for (int num9 = 0; num9 < 2; num9++)
			{
				PlayerObj[num9].gameObject.SetActive(false);
			}
			long num10 = 0L;
			int num11 = 0;
			for (int num12 = 0; num12 < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count && num12 < 2; num12++)
			{
				num10 += MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num12].nALLDMG;
			}
			for (int num13 = 0; num13 < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count && num13 < 2; num13++)
			{
				OrangeCharacter playerByID = StageUpdate.GetPlayerByID(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].PlayerId);
				if (playerByID == null)
				{
					continue;
				}
				PlayerObj[num11].alpha = 0f;
				PlayerObj[num11].gameObject.SetActive(true);
				Button component = PlayerObj[num11].gameObject.transform.Find("Button").GetComponent<Button>();
				component.onClick.RemoveAllListeners();
				float num14 = 0f;
				if (num11 == 0)
				{
					component.onClick.AddListener(AddFriendCall1);
				}
				else
				{
					component.onClick.AddListener(AddFriendCall2);
				}
				num14 = ((num10 == 0L) ? 0f : ((float)MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].nALLDMG / (float)num10 * 100f));
				if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].PlayerId == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
				{
					component.gameObject.SetActive(false);
				}
				else if (ManagedSingleton<FriendHelper>.Instance.IsFriend(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].PlayerId))
				{
					component.gameObject.SetActive(false);
				}
				else
				{
					component.gameObject.SetActive(true);
				}
				string text2 = "";
				text2 = ((MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.CharacterList[0].Skin <= 0) ? ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.CharacterList[0].CharacterID).s_ICON : ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.CharacterList[0].Skin].s_ICON);
				Image imgSt = PlayerImg[num11];
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter2("icon_" + text2), "icon_" + text2, delegate(Sprite spr)
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
				});
				PlayerName[num11].text = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].Nickname;
				PlayerPower[num11].text = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(playerByID.SetPBP.mainWStatus, playerByID.SetPBP.subWStatus, playerByID.SetPBP.tPlayerStatus + playerByID.SetPBP.chipStatus).ToString();
				Score[num11].text = (int)num14 + "%";
				if (num14 > 50f)
				{
					ImageMVP[num11].SetActive(true);
					ImageNotMVP[num11].SetActive(false);
				}
				else
				{
					ImageMVP[num11].SetActive(false);
					ImageNotMVP[num11].SetActive(true);
				}
				CommonIconBase component2 = UnityEngine.Object.Instantiate(refCommonSmallIcon, PlayerMainWeapon[num11].transform).GetComponent<CommonIconBase>();
				WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.MainWeaponInfo.WeaponID];
				int num15 = 0;
				foreach (NetWeaponExpertInfo weaponExpert in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.WeaponExpertList)
				{
					if (weaponExpert.WeaponID == MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.MainWeaponInfo.WeaponID)
					{
						num15 += weaponExpert.ExpertLevel;
					}
				}
				component2.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON);
				component2.SetOtherInfo(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.MainWeaponInfo, CommonIconBase.WeaponEquipType.UnEquip, true, num15, false);
				component2 = UnityEngine.Object.Instantiate(refCommonSmallIcon, PlayerSubWeapon[num11].transform).GetComponent<CommonIconBase>();
				if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.SubWeaponInfo.WeaponID != 0)
				{
					wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.SubWeaponInfo.WeaponID];
					num15 = 0;
					foreach (NetWeaponExpertInfo weaponExpert2 in MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.WeaponExpertList)
					{
						if (weaponExpert2.WeaponID == MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.SubWeaponInfo.WeaponID)
						{
							num15 += weaponExpert2.ExpertLevel;
						}
					}
					component2.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON);
					component2.SetOtherInfo(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[num13].netSealBattleSettingInfo.SubWeaponInfo, CommonIconBase.WeaponEquipType.UnEquip, true, num15, false);
				}
				else
				{
					component2.Setup(0, "", "");
					component2.SetOtherInfo(null, CommonIconBase.WeaponEquipType.UnEquip);
				}
				num11++;
			}
		}
		base._EscapeEvent = EscapeEvent.CUSTOM;
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<StageRewardCell.PerRewardCell, int>(EventManager.ID.UI_UPDATESTAGEREWARD, SetStageRewardIcon);
	}

	public void ShowCharacterPos()
	{
		if (tShowModelAniCtrl != null)
		{
			tShowModelAniCtrl.Play("showpos");
		}
	}

	protected override void DoCustomEscapeEvent()
	{
		GoToHome();
	}

	public void GoToHome()
	{
		if (listLTDescrCtrl.Count > 0)
		{
			for (int num = listLTDescrCtrl.Count - 1; num >= 0; num--)
			{
				int uniqueId = listLTDescrCtrl[num].tLinkLTDescr.id;
				LeanTween.cancel(ref uniqueId, true);
			}
			listLTDescrCtrl.Clear();
		}
		if (!bEndRightNow)
		{
			bEndRightNow = true;
			StartMoveBgImage();
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS04_STOP);
		}
		else if (!bIsGoHome || !(Time.realtimeSinceStartup - fGoHomeTime < 15f))
		{
			bIsGoHome = true;
			fGoHomeTime = Time.realtimeSinceStartup;
			if (!isGo2HomeSEPlay)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK02);
				isGo2HomeSEPlay = true;
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS04_STOP);
			}
			STAGE_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value) && value.n_ID == 111 && MonoBehaviourSingleton<PatchLoadHelper>.Instance.NeedLoadPatchData())
			{
				ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.LOGIN_BONUS;
			}
			else
			{
				StartCoroutine(SoundePlayGOCoroutine());
			}
		}
	}

	private IEnumerator SoundePlayGOCoroutine()
	{
		yield return new WaitForSeconds(1.5f);
		StageUpdate.go2home();
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.BattleServerLogout();
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("hometop", OrangeSceneManager.LoadingType.BLACK, delegate
		{
			base.OnClickCloseBtn();
		});
	}

	private void AddFriendCall1()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendInvite", delegate(FriendInviteUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[0].PlayerId);
		});
	}

	private void AddFriendCall2()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendInvite", delegate(FriendInviteUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[1].PlayerId);
		});
	}

	private void SetStageRewardIcon(StageRewardCell.PerRewardCell tPerRewardCell, int nID)
	{
		if (refStageEndRes.RewardEntities != null && nID < refStageEndRes.RewardEntities.RewardList.Count)
		{
			tPerRewardCell.refRoot.SetActive(true);
			OrangeRareText.Rare p_rare = OrangeRareText.Rare.DUMMY;
			switch ((RewardType)refStageEndRes.RewardEntities.RewardList[nID].RewardType)
			{
			case RewardType.Item:
			{
				ITEM_TABLE iTEM_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[refStageEndRes.RewardEntities.RewardList[nID].RewardID];
				p_rare = (OrangeRareText.Rare)iTEM_TABLE.n_RARE;
				tPerRewardCell.iconimg.CheckLoad(AssetBundleScriptableObject.Instance.GetIconItem(iTEM_TABLE.s_ICON), iTEM_TABLE.s_ICON);
				break;
			}
			case RewardType.Weapon:
			{
				WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[refStageEndRes.RewardEntities.RewardList[nID].RewardID];
				p_rare = (OrangeRareText.Rare)wEAPON_TABLE.n_RARITY;
				tPerRewardCell.iconimg.CheckLoad(AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON);
				break;
			}
			case RewardType.Character:
			{
				CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[refStageEndRes.RewardEntities.RewardList[nID].RewardID];
				p_rare = (OrangeRareText.Rare)cHARACTER_TABLE.n_RARITY;
				tPerRewardCell.iconimg.CheckLoad(AssetBundleScriptableObject.Instance.GetIconCharacter(cHARACTER_TABLE.s_ICON), cHARACTER_TABLE.s_ICON);
				break;
			}
			case RewardType.Equipment:
				p_rare = (OrangeRareText.Rare)ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT[refStageEndRes.RewardEntities.RewardList[nID].RewardID].n_RARE;
				break;
			}
			tPerRewardCell.iconbg.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareBgSmall((int)p_rare));
			tPerRewardCell.frm.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareFrameSmall((int)p_rare));
			tPerRewardCell.icontext.text = refStageEndRes.RewardEntities.RewardList[nID].Amount.ToString();
		}
		else
		{
			tPerRewardCell.refRoot.SetActive(false);
		}
	}

	private void SetStageRewardItemButton(RewardButtonRef tItemButton, int nIndex)
	{
		if (tItemButton == null)
		{
			return;
		}
		if (tItemButton.tCommonIconBase == null)
		{
			tItemButton.tCommonIconBase = UnityEngine.Object.Instantiate(tCommonIconSmallBase, tItemButton.IconRoot.transform).GetComponent<CommonIconBase>();
		}
		if (refStageEndRes != null && refStageEndRes.RewardEntities != null && nIndex < refStageEndRes.RewardEntities.RewardList.Count)
		{
			tItemButton.SelfRoot.gameObject.SetActive(false);
			OrangeRareText.Rare rare = OrangeRareText.Rare.DUMMY;
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			List<int> list = new List<int>();
			foreach (EVENT_TABLE item2 in ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_DROP, MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC))
			{
				list.Add(item2.n_DROP_ITEM);
			}
			switch ((RewardType)refStageEndRes.RewardEntities.RewardList[nIndex].RewardType)
			{
			case RewardType.Item:
				rare = (OrangeRareText.Rare)ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[refStageEndRes.RewardEntities.RewardList[nIndex].RewardID].n_RARE;
				tItemButton.tCommonIconBase.SetupItem(refStageEndRes.RewardEntities.RewardList[nIndex].RewardID, nIndex, OnItemBtnCB);
				tItemButton.tCommonIconBase.SetAmount(refStageEndRes.RewardEntities.RewardList[nIndex].Amount);
				break;
			case RewardType.Weapon:
				rare = (OrangeRareText.Rare)ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[refStageEndRes.RewardEntities.RewardList[nIndex].RewardID].n_RARITY;
				break;
			case RewardType.Character:
				rare = (OrangeRareText.Rare)ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[refStageEndRes.RewardEntities.RewardList[nIndex].RewardID].n_RARITY;
				break;
			case RewardType.Equipment:
			{
				EQUIP_TABLE eQUIP_TABLE = ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT[refStageEndRes.RewardEntities.RewardList[nIndex].RewardID];
				rare = (OrangeRareText.Rare)eQUIP_TABLE.n_RARE;
				tItemButton.tCommonIconBase.Setup(nIndex, AssetBundleScriptableObject.Instance.m_iconEquip, eQUIP_TABLE.s_ICON, OnItemBtnCB);
				NetEquipmentInfo netEquipmentInfo = new NetEquipmentInfo();
				netEquipmentInfo.EquipItemID = refStageEndRes.RewardEntities.RewardList[nIndex].RewardID;
				tItemButton.tCommonIconBase.SetEquipInfo(netEquipmentInfo);
				break;
			}
			case RewardType.Card:
			{
				STAGE_TABLE stage = null;
				if (!ManagedSingleton<OrangeTableHelper>.Instance.GetStage(refStageEndRes.StageInfo.StageID, out stage))
				{
					break;
				}
				List<GACHA_TABLE> listGachaByGroup = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(stage.n_FIRST_REWARD);
				for (int i = 0; i < listGachaByGroup.Count; i++)
				{
					ITEM_TABLE item = null;
					if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(listGachaByGroup[i].n_REWARD_ID, out item) && item.f_VALUE_Y == (float)refStageEndRes.RewardEntities.RewardList[nIndex].RewardID)
					{
						rare = (OrangeRareText.Rare)item.n_RARE;
						tItemButton.tCommonIconBase.SetItemWithAmountForCard(listGachaByGroup[i].n_REWARD_ID, refStageEndRes.RewardEntities.RewardList[nIndex].Amount, OnClickCardInfo);
						break;
					}
				}
				break;
			}
			}
			if (rare >= OrangeRareText.Rare.S)
			{
				tItemButton.RareRoot.SetActive(true);
				tItemButton.CompaignMark.SetActive(false);
			}
			else if (list.Contains(refStageEndRes.RewardEntities.RewardList[nIndex].RewardID))
			{
				tItemButton.RareRoot.SetActive(false);
				tItemButton.CompaignMark.SetActive(true);
			}
			else
			{
				tItemButton.RareRoot.SetActive(false);
				tItemButton.CompaignMark.SetActive(false);
			}
		}
		else
		{
			tItemButton.SelfRoot.gameObject.SetActive(false);
		}
	}

	public void SetStageEndRes(object tObj, float cleantime)
	{
		StageEndRes stageEndRes = tObj as StageEndRes;
		refStageEndRes = stageEndRes;
		fStageCleanTime = cleantime;
	}

	private void RemoveLTDescr(object obj)
	{
		LTDescrCtrl lTDescrCtrl = obj as LTDescrCtrl;
		if (lTDescrCtrl != null)
		{
			listLTDescrCtrl.Remove(lTDescrCtrl);
		}
	}

	public void StartMoveBgImage()
	{
		if (bEndRightNow)
		{
			((RectTransform)BgImage[1].transform).anchoredPosition = Vector2.zero;
			((RectTransform)BgImage[2].transform).anchoredPosition = Vector2.zero;
			StartExpUP();
			return;
		}
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, 3000f, 0f, 0.5f).setOnUpdate(delegate(float f)
		{
			((RectTransform)BgImage[1].transform).anchoredPosition = new Vector2(0f - f, 0f);
			((RectTransform)BgImage[2].transform).anchoredPosition = new Vector2(f, 0f);
		}).setOnComplete(delegate(object obj)
		{
			((RectTransform)BgImage[1].transform).anchoredPosition = Vector2.zero;
			((RectTransform)BgImage[2].transform).anchoredPosition = Vector2.zero;
			RemoveLTDescr(obj);
			StartExpUP();
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	public void StartExpUP()
	{
		int nExp = ManagedSingleton<PlayerHelper>.Instance.GetExp();
		int num = 0;
		if (refStageEndRes != null)
		{
			num = refStageEndRes.DisplayExp;
		}
		else
		{
			nExp = 300;
			num = 300;
		}
		if (bEndRightNow)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.DisplayLvPerform(delegate
			{
				EXP_TABLE expTable3 = ManagedSingleton<OrangeTableHelper>.Instance.GetExpTable(nExp);
				explv.text = expTable3.n_ID.ToString();
				int num7 = 100 - (expTable3.n_TOTAL_RANKEXP - nExp) * 100 / expTable3.n_RANKEXP;
				explabel.text = num7 + "%";
				int k = 0;
				for (int num8 = 100 / expbarsub.Length; k < expbarsub.Length && k * num8 < num7; k++)
				{
					expbarsub[k].gameObject.SetActive(true);
				}
				for (; k < expbarsub.Length; k++)
				{
					expbarsub[k].gameObject.SetActive(false);
				}
				StartShowExpMonetProfText();
			});
			return;
		}
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		if (!expupse)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS04_LP);
			expupse = true;
		}
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, num, 0f, 1f).setOnUpdate(delegate(float f)
		{
			int num4 = nExp - (int)f;
			EXP_TABLE expTable2 = ManagedSingleton<OrangeTableHelper>.Instance.GetExpTable(num4);
			explv.text = expTable2.n_ID.ToString();
			int num5 = (num4 - (expTable2.n_TOTAL_RANKEXP - expTable2.n_RANKEXP)) * 100 / expTable2.n_RANKEXP;
			explabel.text = num5 + "%";
			int j = 0;
			for (int num6 = 100 / expbarsub.Length; j < expbarsub.Length && j * num6 < num5; j++)
			{
				expbarsub[j].gameObject.SetActive(true);
			}
			for (; j < expbarsub.Length; j++)
			{
				expbarsub[j].gameObject.SetActive(false);
			}
		}).setOnComplete(delegate(object obj)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS04_STOP);
			MonoBehaviourSingleton<OrangeGameManager>.Instance.DisplayLvPerform(delegate
			{
				EXP_TABLE expTable = ManagedSingleton<OrangeTableHelper>.Instance.GetExpTable(nExp);
				explv.text = expTable.n_ID.ToString();
				int num2 = (nExp - (expTable.n_TOTAL_RANKEXP - expTable.n_RANKEXP)) * 100 / expTable.n_RANKEXP;
				explabel.text = num2 + "%";
				int i = 0;
				for (int num3 = 100 / expbarsub.Length; i < expbarsub.Length && i * num3 < num2; i++)
				{
					expbarsub[i].gameObject.SetActive(true);
				}
				for (; i < expbarsub.Length; i++)
				{
					expbarsub[i].gameObject.SetActive(false);
				}
				RemoveLTDescr(obj);
				StartShowExpMonetProfText();
			});
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	public void StartShowExpMonetProfText()
	{
		if (bEndRightNow)
		{
			for (int i = 0; i < ExpMoneyProfRoot.Length; i++)
			{
				ExpMoneyProfRoot[i].alpha = 1f;
			}
			StartShowItemButtons(0);
			return;
		}
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, 0f, 1f, 0.5f).setOnUpdate(delegate(float f)
		{
			for (int k = 0; k < ExpMoneyProfRoot.Length; k++)
			{
				ExpMoneyProfRoot[k].alpha = f;
			}
		}).setOnComplete(delegate(object obj)
		{
			for (int j = 0; j < ExpMoneyProfRoot.Length; j++)
			{
				ExpMoneyProfRoot[j].alpha = 1f;
			}
			RemoveLTDescr(obj);
			StartShowItemButtons(0);
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	public void OnItemBtnCB(int nBtnID)
	{
		ITEM_TABLE item = null;
		EQUIP_TABLE tEQUIP_TABLE = null;
		WEAPON_TABLE tWEAPON_TABLE = null;
		CHARACTER_TABLE tCHARACTER_TABLE = null;
		switch ((RewardType)refStageEndRes.RewardEntities.RewardList[nBtnID].RewardType)
		{
		case RewardType.Item:
			if (ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(refStageEndRes.RewardEntities.RewardList[nBtnID].RewardID, out item))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(item);
				});
			}
			break;
		case RewardType.Weapon:
			if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(refStageEndRes.RewardEntities.RewardList[nBtnID].RewardID, out tWEAPON_TABLE))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(tWEAPON_TABLE);
				});
			}
			break;
		case RewardType.Character:
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(refStageEndRes.RewardEntities.RewardList[nBtnID].RewardID, out tCHARACTER_TABLE))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(tCHARACTER_TABLE);
				});
			}
			break;
		case RewardType.Equipment:
			if (ManagedSingleton<OrangeDataManager>.Instance.EQUIP_TABLE_DICT.TryGetValue(refStageEndRes.RewardEntities.RewardList[nBtnID].RewardID, out tEQUIP_TABLE))
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
				{
					ui.CanShowHow2Get = false;
					ui.Setup(tEQUIP_TABLE);
				});
			}
			break;
		}
	}

	private void StartShowItemButton(int nIndex)
	{
		if (bEndRightNow)
		{
			ItemButtons[nShowType][nIndex].SelfRoot.SetActive(true);
			ItemButtons[nShowType][nIndex].SelfRoot.transform.localScale = new Vector3(0.97f, 0.97f, 1f);
			StartShowItemButtons(nIndex + 1);
			return;
		}
		float fScale = ItemButtons[nShowType][nIndex].SelfRoot.transform.localScale.x;
		ItemButtons[nShowType][nIndex].SelfRoot.transform.localScale = new Vector3(fScaleValue, fScaleValue, 1f);
		ItemButtons[nShowType][nIndex].SelfRoot.SetActive(true);
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STAMP01);
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, fScaleValue, fScale, 0.15f).setOnUpdate(delegate(float f)
		{
			ItemButtons[nShowType][nIndex].SelfRoot.transform.localScale = new Vector3(f, f, 1f);
		}).setOnComplete(delegate(object obj)
		{
			ItemButtons[nShowType][nIndex].SelfRoot.transform.localScale = new Vector3(fScale, fScale, 1f);
			RemoveLTDescr(obj);
			StartShowItemButtons(nIndex + 1);
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	public void StartShowItemButtons(int nIndex)
	{
		if (refStageEndRes != null && refStageEndRes.RewardEntities != null)
		{
			if (nIndex < refStageEndRes.RewardEntities.RewardList.Count)
			{
				StartShowItemButton(nIndex);
			}
			else
			{
				CheckGoToStageGosOrTime();
			}
		}
		else if (nIndex < ItemButtons[nShowType].Length)
		{
			StartShowItemButton(nIndex);
		}
		else
		{
			CheckGoToStageGosOrTime();
		}
	}

	private void CheckGoToStageGosOrTime()
	{
		switch (nShowType)
		{
		case 0:
			StartStageGos(0);
			break;
		case 1:
			StartStageGos(0);
			break;
		case 2:
		{
			if (ManagedSingleton<StageHelper>.Instance.nLastStageID == 0)
			{
				break;
			}
			STAGE_TABLE sTAGE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[ManagedSingleton<StageHelper>.Instance.nLastStageID];
			switch ((StageType)(short)sTAGE_TABLE.n_TYPE)
			{
			case StageType.Activity:
				ActivityRoot.SetActive(true);
				TimeRoot.SetActive(false);
				if (sTAGE_TABLE.n_MAIN == 15001)
				{
					nCleanValue = BattleInfoUI.Instance.nGetCampaignScore;
					PlayTime.text = "";
				}
				else if (sTAGE_TABLE.n_MAIN == 15011)
				{
					nCleanValue = Mathf.RoundToInt(BattleInfoUI.Instance.fCountDownTimerValue);
					PlayTime.text = "";
				}
				else if (sTAGE_TABLE.n_MAIN == 15021)
				{
					nCleanValue = Mathf.RoundToInt(BattleInfoUI.Instance.nGetnGetItemValue);
					PlayTime.text = "";
				}
				else if (sTAGE_TABLE.n_MAIN == 15031)
				{
					nCleanValue = Mathf.RoundToInt(BattleInfoUI.Instance.nGetnBattleScoreValue);
					PlayTime.text = "";
				}
				StartShowCleanTime();
				break;
			case StageType.TimeAttack:
				ActivityRoot.SetActive(false);
				TimeRoot.SetActive(true);
				nCleanValue = Mathf.RoundToInt(StageResManager.GetStageUpdate().fStageUseTime * 100f);
				StartShowCleanTime();
				break;
			case StageType.BossRush:
				ActivityRoot.SetActive(false);
				TimeRoot.SetActive(true);
				nCleanValue = Mathf.RoundToInt(StageResManager.GetStageUpdate().fStageUseTime * 100f);
				StartShowCleanTime();
				break;
			case StageType.RaidBoss:
				ActivityRoot.SetActive(true);
				TimeRoot.SetActive(false);
				nCleanValue = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerPower();
				if (nCleanValue >= OrangeConst.RAID_DAMAGE_LIMIT)
				{
					nCleanValue = OrangeConst.RAID_DAMAGE_LIMIT;
				}
				sPlayTimeColorStart = "<color=#4eff00>";
				sPlayTimeColorEnd = "</color>";
				PlayTime.text = "";
				StartShowCleanTime();
				break;
			case StageType.Crusade:
				ActivityRoot.SetActive(true);
				TimeRoot.SetActive(false);
				nCleanValue = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetMainPlayerPower();
				if (nCleanValue >= OrangeConst.GUILD_CRUSADE_LIMIT)
				{
					nCleanValue = OrangeConst.GUILD_CRUSADE_LIMIT;
				}
				sPlayTimeColorStart = "<color=#4eff00>";
				sPlayTimeColorEnd = "</color>";
				PlayTime.text = "";
				StartShowCleanTime();
				break;
			case StageType.TWSuppress:
			case StageType.TWLightning:
			case StageType.TWCrusade:
				ActivityRoot.SetActive(true);
				TimeRoot.SetActive(false);
				nCleanValue = GetTotalWarScore();
				sPlayTimeColorStart = "<color=#4eff00>";
				sPlayTimeColorEnd = "</color>";
				PlayTime.text = "";
				StartShowCleanTime();
				break;
			case StageType.BossChallenge:
			case StageType.Event:
			case StageType.TeamUp:
			case StageType.Tower:
				break;
			}
			break;
		}
		default:
			StartStageGos(0);
			break;
		}
	}

	private int GetTotalWarScore()
	{
		List<string> perGameSaveData = StageUpdate.GetPerGameSaveData();
		for (int num = perGameSaveData.Count - 1; num >= 0; num--)
		{
			if (perGameSaveData[num].StartsWith("FinalScore"))
			{
				return int.Parse(perGameSaveData[num].Substring("FinalScore".Length));
			}
		}
		return 0;
	}

	private void StartStageGo(int nIndex)
	{
		if (bEndRightNow)
		{
			GoOkImgAll[nShowType][nIndex].gameObject.SetActive(true);
			GoEndImgAll[nShowType][nIndex].gameObject.SetActive(true);
			GoOkImgAll[nShowType][nIndex].transform.localScale = new Vector3(1f, 1f, 1f);
			GoEndImgAll[nShowType][nIndex].transform.localScale = new Vector3(1f, 1f, 1f);
			GoEndImgAll[nShowType][nIndex].color = Color.white;
			GoEndImgAll[nShowType][nIndex].gameObject.SetActive(false);
			GoTextAll[nShowType][nIndex].color = okcolor;
			if (StageUpdate.gbGeneratePvePlayer)
			{
				if (nIndex < 5)
				{
					StartStageGos(nIndex + 1);
				}
				else
				{
					StartShowPlayer(0);
				}
			}
			else if (nIndex < 2)
			{
				StartStageGos(nIndex + 1);
			}
			return;
		}
		GoOkImgAll[nShowType][nIndex].gameObject.SetActive(true);
		GoEndImgAll[nShowType][nIndex].gameObject.SetActive(true);
		GoOkImgAll[nShowType][nIndex].transform.localScale = new Vector3(fGoScaleValue, fGoScaleValue, 1f);
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STAMP02);
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, fGoScaleValue, 1f, 0.3f).setOnUpdate(delegate(float f)
		{
			GoOkImgAll[nShowType][nIndex].transform.localScale = new Vector3(f, f, 1f);
			GoEndImgAll[nShowType][nIndex].transform.localScale = new Vector3(f, f, 1f);
			GoTextAll[nShowType][nIndex].color = okcolor * (7f - f) / 6f + failcolor * (f - 1f) / 6f;
		}).setOnComplete(delegate(object obj)
		{
			GoOkImgAll[nShowType][nIndex].transform.localScale = new Vector3(1f, 1f, 1f);
			GoEndImgAll[nShowType][nIndex].transform.localScale = new Vector3(1f, 1f, 1f);
			GoEndImgAll[nShowType][nIndex].color = Color.white;
			GoTextAll[nShowType][nIndex].color = okcolor;
			StartStageGoEnd(nIndex);
			RemoveLTDescr(obj);
			if (nIndex < 2)
			{
				StartStageGos(nIndex + 1);
			}
			else if (StageUpdate.gbGeneratePvePlayer)
			{
				StartShowPlayer(0);
			}
			else
			{
				bEndRightNow = true;
			}
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	private void StartStageGoEnd(int nIndex)
	{
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, 1f, fGoScaleValue, 0.8f).setOnUpdate(delegate(float f)
		{
			GoEndImgAll[nShowType][nIndex].transform.localScale = new Vector3(f, f, 1f);
			GoEndImgAll[nShowType][nIndex].color = new Color(1f, 1f, 1f, (7f - f) / 6f);
		}).setOnComplete(delegate(object obj)
		{
			GoEndImgAll[nShowType][nIndex].gameObject.SetActive(false);
			RemoveLTDescr(obj);
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	public void StartStageGos(int nIndex)
	{
		if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0)
		{
			STAGE_TABLE sTAGE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[ManagedSingleton<StageHelper>.Instance.nLastStageID];
			int[] array = new int[3] { sTAGE_TABLE.n_CLEAR1, sTAGE_TABLE.n_CLEAR2, sTAGE_TABLE.n_CLEAR3 };
			int[] array2 = new int[3] { sTAGE_TABLE.n_CLEAR_VALUE1, sTAGE_TABLE.n_CLEAR_VALUE2, sTAGE_TABLE.n_CLEAR_VALUE3 };
			if ((nNowStageStart & (1 << nIndex)) != 0)
			{
				StartStageGo(nIndex);
			}
			else if (nIndex < 2)
			{
				StartStageGos(nIndex + 1);
			}
			else if (StageUpdate.gbGeneratePvePlayer)
			{
				StartShowPlayer(0);
			}
			else
			{
				bEndRightNow = true;
			}
		}
		else
		{
			StartStageGo(nIndex);
		}
	}

	public void StartShowPlayer(int nIndex)
	{
		if (bEndRightNow)
		{
			PlayerObj[nIndex].gameObject.SetActive(true);
			PlayerObj[nIndex].alpha = 1f;
			if (nIndex < 1 && MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count > 1)
			{
				StartShowPlayer(nIndex + 1);
			}
			return;
		}
		PlayerObj[nIndex].gameObject.SetActive(true);
		PlayerObj[nIndex].alpha = 0f;
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, 0f, 1f, 0.3f).setOnUpdate(delegate(float f)
		{
			PlayerObj[nIndex].alpha = f;
		}).setOnComplete(delegate(object obj)
		{
			PlayerObj[nIndex].alpha = 1f;
			RemoveLTDescr(obj);
			int num = 0;
			for (int i = 0; i < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo.Count && i < 2; i++)
			{
				if (!(StageUpdate.GetPlayerByID(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo[i].PlayerId) == null))
				{
					num++;
				}
			}
			if (nIndex < 1 && nIndex + 2 <= num)
			{
				StartShowPlayer(nIndex + 1);
			}
			else
			{
				bEndRightNow = true;
			}
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	public void StartShowCleanTime()
	{
		if (bEndRightNow)
		{
			PlayTime.text = sPlayTimeColorStart + nCleanValue + sPlayTimeColorEnd;
			int num = nCleanValue % 100;
			int num2 = (nCleanValue - num) / 100 % 60;
			int num3 = ((nCleanValue - num) / 100 - num2) / 60;
			TimeMinuteTxt.text = num3.ToString("00");
			TimeSecondTxt.text = num2.ToString("00");
			TimeMSecondTxt.text = num.ToString("00");
			CheckCleanTime();
			return;
		}
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, 0f, 1f, 0.3f).setOnUpdate(delegate(float f)
		{
			int num7 = Mathf.RoundToInt(Mathf.Lerp(0f, nCleanValue, f));
			PlayTime.text = sPlayTimeColorStart + num7 + sPlayTimeColorEnd;
			int num8 = num7 % 100;
			int num9 = (num7 - num8) / 100 % 60;
			int num10 = ((num7 - num8) / 100 - num9) / 60;
			TimeMinuteTxt.text = num10.ToString("00");
			TimeSecondTxt.text = num9.ToString("00");
			TimeMSecondTxt.text = num8.ToString("00");
		}).setOnComplete(delegate(object obj)
		{
			PlayTime.text = sPlayTimeColorStart + nCleanValue + sPlayTimeColorEnd;
			int num4 = nCleanValue % 100;
			int num5 = (nCleanValue - num4) / 100 % 60;
			int num6 = ((nCleanValue - num4) / 100 - num5) / 60;
			TimeMinuteTxt.text = num6.ToString("00");
			TimeSecondTxt.text = num5.ToString("00");
			TimeMSecondTxt.text = num4.ToString("00");
			RemoveLTDescr(obj);
			CheckCleanTime();
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	private void CheckCleanTime()
	{
		switch (nShowType)
		{
		case 0:
			StartStageGos(0);
			break;
		case 1:
			StartStageGos(0);
			break;
		case 2:
			if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0)
			{
				switch ((StageType)(short)ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[ManagedSingleton<StageHelper>.Instance.nLastStageID].n_TYPE)
				{
				case StageType.Activity:
					StartShowCleanRank();
					break;
				case StageType.TimeAttack:
					StartShowCleanRank();
					break;
				case StageType.BossRush:
					StartShowCleanRank();
					break;
				case StageType.RaidBoss:
					StartShowCleanRank();
					break;
				case StageType.Crusade:
					StartShowCleanRank();
					break;
				case StageType.TWSuppress:
				case StageType.TWLightning:
				case StageType.TWCrusade:
					CheckNewRecordAndShow();
					break;
				case StageType.BossChallenge:
				case StageType.Event:
				case StageType.TeamUp:
				case StageType.Tower:
					break;
				}
			}
			break;
		default:
			StartStageGos(0);
			break;
		}
	}

	private void CheckNewRecordAndShow()
	{
		List<string> perGameSaveData = StageUpdate.GetPerGameSaveData();
		int num = 0;
		for (int num2 = perGameSaveData.Count - 1; num2 >= 0; num2--)
		{
			if (perGameSaveData[num2].StartsWith("RecordFlag"))
			{
				num = int.Parse(perGameSaveData[num2].Substring("RecordFlag".Length));
				break;
			}
		}
		switch (num)
		{
		case 2:
			StartShowNewRecord();
			break;
		case 3:
			StartShowScoreOut();
			break;
		}
	}

	public void StartShowScoreOut()
	{
		if (bEndRightNow)
		{
			ScoreOutTxt.gameObject.SetActive(true);
			ScoreOutTxt.transform.localScale = new Vector3(1f, 1f, 1f);
			return;
		}
		ScoreOutTxt.transform.localScale = new Vector3(11f, 11f, 1f);
		ScoreOutTxt.gameObject.SetActive(true);
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RESULT01);
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, 0f, 1f, 0.3f).setOnUpdate(delegate(float f)
		{
			ScoreOutTxt.transform.localScale = new Vector3(11f - 10f * f, 11f - 10f * f, 1f);
		}).setOnComplete(delegate(object obj)
		{
			ScoreOutTxt.transform.localScale = new Vector3(1f, 1f, 1f);
			RemoveLTDescr(obj);
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	private void InitNewRecordRotate()
	{
		RotateSelf[] componentsInChildren = NewRecordImg.transform.GetComponentsInChildren<RotateSelf>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}

	public void StartShowNewRecord()
	{
		if (bEndRightNow)
		{
			NewRecordImg.gameObject.SetActive(true);
			NewRecordImg.transform.localScale = new Vector3(1f, 1f, 1f);
			InitNewRecordRotate();
			return;
		}
		NewRecordImg.transform.localScale = new Vector3(11f, 11f, 1f);
		NewRecordImg.gameObject.SetActive(true);
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RESULT01);
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, 0f, 1f, 0.3f).setOnUpdate(delegate(float f)
		{
			NewRecordImg.transform.localScale = new Vector3(11f - 10f * f, 11f - 10f * f, 1f);
		}).setOnComplete(delegate(object obj)
		{
			NewRecordImg.transform.localScale = new Vector3(1f, 1f, 1f);
			InitNewRecordRotate();
			RemoveLTDescr(obj);
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	public void StartShowCleanRank()
	{
		int nIndex = 0;
		for (int i = 0; i < 3; i++)
		{
			if ((nNowStageStart & (1 << i)) != 0)
			{
				nIndex = 3 - i;
				break;
			}
		}
		if (bEndRightNow)
		{
			CleanRank[nIndex].alpha = 1f;
			CleanRank[nIndex].transform.localScale = new Vector3(1f, 1f, 1f);
			EffectRoot[nIndex].SetActive(true);
			CleanRankScaleBG[nIndex].gameObject.SetActive(false);
			return;
		}
		CleanRank[nIndex].alpha = 0f;
		CleanRank[nIndex].transform.localScale = new Vector3(11f, 11f, 1f);
		EffectRoot[nIndex].SetActive(false);
		CleanRankScaleBG[nIndex].gameObject.SetActive(true);
		CleanRankScaleBG[nIndex].transform.localScale = new Vector3(1f, 1f, 1f);
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RESULT01);
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, 0f, 1f, 0.3f).setOnUpdate(delegate(float f)
		{
			CleanRank[nIndex].alpha = f;
			CleanRank[nIndex].transform.localScale = new Vector3(11f - 10f * f, 11f - 10f * f, 1f);
		}).setOnComplete(delegate(object obj)
		{
			CleanRank[nIndex].alpha = 1f;
			CleanRank[nIndex].transform.localScale = new Vector3(1f, 1f, 1f);
			EffectRoot[nIndex].SetActive(true);
			RemoveLTDescr(obj);
			StartShowCleanRankBig();
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	public void StartShowCleanRankBig()
	{
		int nIndex = 0;
		for (int i = 0; i < 3; i++)
		{
			if ((nNowStageStart & (1 << i)) != 0)
			{
				nIndex = 3 - i;
				break;
			}
		}
		if (bEndRightNow)
		{
			CleanRankScaleBG[nIndex].gameObject.SetActive(false);
			return;
		}
		CleanRankScaleBG[nIndex].gameObject.SetActive(true);
		LTDescrCtrl lTDescrCtrl = new LTDescrCtrl();
		lTDescrCtrl.tLinkLTDescr = LeanTween.value(base.gameObject, 1f, 0f, 1f).setOnUpdate(delegate(float f)
		{
			Color color = CleanRankScaleBG[nIndex].color;
			color.a = f;
			CleanRankScaleBG[nIndex].color = color;
			CleanRankScaleBG[nIndex].transform.localScale = new Vector3(2f - f, 2f - f, 1f);
		}).setOnComplete(delegate(object obj)
		{
			CleanRankScaleBG[nIndex].gameObject.SetActive(false);
			RemoveLTDescr(obj);
			bEndRightNow = true;
		}, lTDescrCtrl);
		listLTDescrCtrl.Add(lTDescrCtrl);
	}

	private void OnClickCardInfo(int p_idx)
	{
		ITEM_TABLE item = null;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_idx, out item) || item.n_TYPE != 5 || item.n_TYPE_X != 1 || (int)item.f_VALUE_Y <= 0)
		{
			return;
		}
		CARD_TABLE card = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)item.f_VALUE_Y, out card))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(card, item);
			});
		}
	}
}
