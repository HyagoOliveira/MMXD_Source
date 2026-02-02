using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using NaughtyAttributes;
using StageLib;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class UI_ChallengePopup : OrangeUIBase
{
	private readonly string infinity = "∞";

	private readonly int maxMultiSweepCount = 10;

	[SerializeField]
	private OrangeText textTitle;

	[SerializeField]
	private Image imgIconStageFrame;

	[SerializeField]
	private Image imgIconStage;

	[SerializeField]
	private Image[] imgClear;

	[SerializeField]
	private OrangeText[] textClear;

	[SerializeField]
	private OrangeText textAmountExp;

	[SerializeField]
	private OrangeText textAmountZ;

	[SerializeField]
	private OrangeText textAmountSP;

	[SerializeField]
	private OrangeText textCost;

	[SerializeField]
	private OrangeText textCount;

	[SerializeField]
	private ChallengePopupRewardUnit rewardIcon;

	[SerializeField]
	private ScrollRect rewardScrollRect;

	[SerializeField]
	private Sprite[] sprImgClear;

	[SerializeField]
	private OrangeText textSweepOnce;

	[SerializeField]
	private OrangeText textSweepMulti;

	[SerializeField]
	private Button btnBossIntro;

	[SerializeField]
	private Button btnSecret;

	[SerializeField]
	private OrangeText textSecret;

	[SerializeField]
	private BonusInfoSub bonusInfoSubMenu;

	[SerializeField]
	private Color[] bonusColor;

	[SerializeField]
	private BonusInfoTag bonusTag;

	[SerializeField]
	private Canvas canvasCountInfo;

	[SerializeField]
	private Canvas canvasScenarioInfo;

	[SerializeField]
	private Image imgScenarioOn;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_clickSecretSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_closeSecretSE;

	private STAGE_TABLE stageTable;

	private StageInfo stageInfo;

	private bool hasClearData;

	private List<IconBase> listReward = new List<IconBase>();

	private int multiSweepCount;

	private int Current_stageRewardId;

	private bool repeatScenario;

	private Color[] clearTextColor = new Color[2]
	{
		new Color(0.95686275f, 0.654902f, 1f / 85f),
		new Color(13f / 15f, 13f / 15f, 0.8862745f)
	};

	private int totalSecretCount;

	private int findSecretCount;

	public void Start()
	{
	}

	public void Setup(STAGE_TABLE p_stageTable)
	{
		backToHometopCB = (Callback)Delegate.Combine(backToHometopCB, new Callback(Clear));
		repeatScenario = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastRepeatScenario;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP01);
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL01;
		stageTable = p_stageTable;
		stageInfo = null;
		hasClearData = ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(stageTable.n_ID, out stageInfo);
		textTitle.text = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(stageTable.w_NAME);
		if (stageTable.n_TYPE == 1)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_story, "UI_story_icon03_00", delegate(Sprite obj)
			{
				imgIconStageFrame.sprite = obj;
				if (imgIconStageFrame.sprite != null)
				{
					imgIconStageFrame.color = Color.white;
				}
			});
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_iconStageBg, stageTable.s_ICON, delegate(Sprite obj)
			{
				imgIconStage.sprite = obj;
				if (imgIconStage.sprite != null)
				{
					imgIconStage.color = new Color(1f, 1f, 1f, 0.55f);
				}
			});
		}
		else
		{
			imgIconStageFrame.color = Color.clear;
			imgIconStage.color = Color.clear;
		}
		UpdateScenarioInfo();
		string[] stageClearMsg = ManagedSingleton<StageHelper>.Instance.GetStageClearMsg(stageTable);
		for (int i = 0; i < imgClear.Length; i++)
		{
			textClear[i].text = stageClearMsg[i];
			if (hasClearData)
			{
				if ((stageInfo.netStageInfo.Star & (1 << i)) != 0)
				{
					imgClear[i].sprite = sprImgClear[1];
					textClear[i].color = clearTextColor[0];
				}
				else
				{
					imgClear[i].sprite = sprImgClear[0];
					textClear[i].color = clearTextColor[1];
				}
			}
			else
			{
				imgClear[i].sprite = sprImgClear[0];
				textClear[i].color = clearTextColor[1];
			}
		}
		SetTextInfoByClear(hasClearData);
		SetSecretData();
		if (ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(stageTable.w_BOSS_INTRO))
		{
			btnBossIntro.gameObject.SetActive(false);
		}
		else
		{
			btnBossIntro.onClick.AddListener(OnClickBossIntroBtn);
		}
		bool flag = true;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.ContainsKey(stageTable.n_MAIN))
		{
			flag = ManagedSingleton<PlayerNetManager>.Instance.dicStage[stageTable.n_MAIN].StageSecretList.Count < 1;
		}
		bonusInfoSubMenu.SetupInfo(stageTable.n_ID);
		bonusTag.Setup(bonusInfoSubMenu.dicBonusEvent);
		if (bonusTag.SetActive(true))
		{
			bonusTag.StartRolling();
		}
		if (bonusInfoSubMenu.dicBonusEvent.Any((BonusInfoSub.InfoLable p) => p.bonusType == 1) && !flag)
		{
			textAmountExp.color = bonusColor[0];
		}
		else
		{
			textAmountExp.color = bonusColor[1];
		}
		if (bonusInfoSubMenu.dicBonusEvent.Any((BonusInfoSub.InfoLable p) => p.bonusType == 2) && !flag)
		{
			textAmountZ.color = bonusColor[0];
		}
		else
		{
			textAmountZ.color = bonusColor[1];
		}
		if (bonusInfoSubMenu.dicBonusEvent.Any((BonusInfoSub.InfoLable p) => p.bonusType == 3) && !flag)
		{
			textAmountSP.color = bonusColor[0];
		}
		else
		{
			textAmountSP.color = bonusColor[1];
		}
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void SetTextInfoByClear(bool alreadyClear)
	{
		string text = "x";
		string format = "{0}/{1}";
		if (alreadyClear)
		{
			textAmountExp.text = text + stageTable.n_GET_EXP;
			textAmountZ.text = text + stageTable.n_GET_MONEY;
			SetRewardList(stageTable.n_GET_REWARD);
		}
		else
		{
			textAmountExp.text = text + stageTable.n_FIRST_EXP;
			textAmountZ.text = text + stageTable.n_FIRST_MONEY;
			SetRewardList(stageTable.n_FIRST_REWARD);
		}
		int num = stageTable.n_PLAY_COUNT;
		if (stageInfo != null)
		{
			num -= stageInfo.netStageInfo.ClearCount;
		}
		textCount.text = ((stageTable.n_PLAY_COUNT == -1) ? infinity : string.Format(format, num, stageTable.n_PLAY_COUNT));
		textAmountSP.text = text + stageTable.n_PROF;
		textCost.text = stageTable.n_AP.ToString();
		textSweepOnce.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNTION_SWEEP");
		if (stageTable.n_PLAY_COUNT == -1 || num >= maxMultiSweepCount)
		{
			textSweepMulti.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNTION_MULTI_SWEEP"), maxMultiSweepCount);
			multiSweepCount = maxMultiSweepCount;
		}
		else
		{
			textSweepMulti.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNTION_MULTI_SWEEP"), num);
			multiSweepCount = num;
		}
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		GACHA_TABLE gACHA_TABLE = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(Current_stageRewardId)[p_idx];
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(gACHA_TABLE.n_REWARD_ID, out item))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			});
		}
	}

	private void SetRewardList(int p_stageRewardId)
	{
		foreach (IconBase item in listReward)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		listReward.Clear();
		List<GACHA_TABLE> listGachaByGroup = ManagedSingleton<ExtendDataHelper>.Instance.GetListGachaByGroup(p_stageRewardId);
		int count = listGachaByGroup.Count;
		Current_stageRewardId = p_stageRewardId;
		Transform content = rewardScrollRect.content;
		for (int i = 0; i < count; i++)
		{
			ChallengePopupRewardUnit challengePopupRewardUnit = UnityEngine.Object.Instantiate(rewardIcon, content);
			GACHA_TABLE gACHA_TABLE = listGachaByGroup[i];
			NetRewardInfo netGachaRewardInfo = new NetRewardInfo
			{
				RewardType = (sbyte)gACHA_TABLE.n_REWARD_TYPE,
				RewardID = gACHA_TABLE.n_REWARD_ID,
				Amount = gACHA_TABLE.n_AMOUNT_MAX
			};
			string bundlePath = string.Empty;
			string assetPath = string.Empty;
			int rare = 0;
			int[] rewardSpritePath = MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo, ref bundlePath, ref assetPath, ref rare);
			challengePopupRewardUnit.Setup(i, bundlePath, assetPath, OnClickItem);
			challengePopupRewardUnit.SetRare(rare);
			challengePopupRewardUnit.SetPieceActive(rewardSpritePath[0] == 1 && rewardSpritePath[1] == 4);
			listReward.Add(challengePopupRewardUnit);
		}
	}

	private void OnClickBossIntroBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BossIntro", delegate(BossIntroUI ui)
		{
			ui.Setup(stageTable.n_ID);
		});
	}

	public void OnClickGoCheck()
	{
		StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
		if (!ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(stageTable, ref condition))
		{
			ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(stageTable, condition, delegate
			{
				SetTextInfoByClear(hasClearData);
			});
			return;
		}
		ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = 0;
		if (stageTable.n_STAGE_RULE > 0)
		{
			STAGE_RULE_TABLE rule = null;
			if (ManagedSingleton<OrangeTableHelper>.Instance.GetStageRuleUI(stageTable.n_STAGE_RULE, out rule))
			{
				string ruleMsg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(rule.s_TIP);
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), ruleMsg, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_STARTER"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
					{
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK07);
						ManagedSingleton<StageHelper>.Instance.nLastStageID = stageTable.n_ID;
						ManagedSingleton<StageHelper>.Instance.nLastStageRuleID = rule.n_ID;
						ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = rule.n_ID;
						SetStageEndGoUI();
						ManagedSingleton<PlayerNetManager>.Instance.StageStartReq(ManagedSingleton<StageHelper>.Instance.nLastStageID, stageTable.s_STAGE, ManagedSingleton<StageHelper>.Instance.GetStageCrc(ManagedSingleton<StageHelper>.Instance.nLastStageID), delegate(string S_STAGE)
						{
							StageUpdate.SetStageName(S_STAGE, stageTable.n_DIFFICULTY);
							MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("StageTest", OrangeSceneManager.LoadingType.STAGE, null, false);
						});
					});
				});
				return;
			}
			ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status = stageTable.n_STAGE_RULE;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS02_STOP);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
		{
			ui.Setup(stageTable);
			SetStageEndGoUI();
		});
	}

	private void SetStageEndGoUI()
	{
		switch ((StageType)(short)stageTable.n_TYPE)
		{
		case StageType.Scenario:
			ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.STORYSTAGESELECT;
			break;
		case StageType.BossChallenge:
			ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.BOSSCHALLENGE;
			break;
		}
	}

	public void OnClickSweep(int count)
	{
		if (ManagedSingleton<EquipHelper>.Instance.ShowEquipmentLimitReachedDialog() || MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckCardCountMax())
		{
			return;
		}
		if (count > multiSweepCount)
		{
			count = multiSweepCount;
		}
		if (ManagedSingleton<PlayerHelper>.Instance.GetStamina() > stageTable.n_AP && ManagedSingleton<PlayerHelper>.Instance.GetStamina() < stageTable.n_AP * count)
		{
			count = ManagedSingleton<PlayerHelper>.Instance.GetStamina() / stageTable.n_AP;
		}
		bool flag = false;
		if (stageInfo != null && ManagedSingleton<StageHelper>.Instance.GetStarAmount(stageInfo.netStageInfo.Star) >= 3)
		{
			flag = true;
		}
		if (!flag)
		{
			string errorMsg = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_SWEEP_STAR");
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
			{
				tipUI.Setup(errorMsg, true);
			});
			return;
		}
		StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
		if (!ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(stageTable, ref condition, count))
		{
			ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(stageTable, condition, delegate
			{
				SetTextInfoByClear(hasClearData);
			});
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		MonoBehaviourSingleton<OrangeGameManager>.Instance.StageSweepReq(stageTable.n_ID, count, delegate(object res)
		{
			NetRewardsEntity reward = res as NetRewardsEntity;
			if (reward.RewardList.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsLvUp)
					{
						ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
						{
							MonoBehaviourSingleton<OrangeGameManager>.Instance.DisplayLvPerform();
						});
					}
					ui.Setup(reward.RewardList);
				});
			}
			else
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.DisplayLvPerform();
			}
			SetTextInfoByClear(hasClearData);
		});
	}

	private void SetSecretData()
	{
		totalSecretCount = stageTable.n_SECRET;
		if (totalSecretCount < 1)
		{
			textSecret.text = string.Empty;
			btnSecret.interactable = false;
			return;
		}
		btnSecret.interactable = true;
		if (hasClearData)
		{
			findSecretCount = stageInfo.StageSecretList.Count;
		}
		textSecret.text = findSecretCount + "/" + totalSecretCount;
	}

	public void UpdateScenarioInfo()
	{
		if (stageTable.n_PLAY_COUNT == -1)
		{
			canvasCountInfo.enabled = false;
			canvasScenarioInfo.enabled = true;
			imgScenarioOn.enabled = repeatScenario;
			if ((short)stageTable.n_TYPE == 1 && stageTable.n_DIFFICULTY > 1)
			{
				canvasScenarioInfo.enabled = false;
			}
		}
		else
		{
			canvasCountInfo.enabled = true;
			canvasScenarioInfo.enabled = false;
		}
	}

	public void OnClickRepeatScenarioBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		repeatScenario = !repeatScenario;
		imgScenarioOn.enabled = repeatScenario;
	}

	public void OnClickSecretBtn()
	{
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("SECRET_ELEMENT_TIP"), findSecretCount, totalSecretCount), (int)m_clickSecretSE, m_closeSecretSE);
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SWITCH_SCENE, Clear);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.SWITCH_SCENE, Clear);
	}

	private void Clear()
	{
		OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		Save();
		base.OnClickCloseBtn();
	}

	private void Save()
	{
		if (!canvasScenarioInfo.enabled)
		{
			StageUpdate.AllStageCtrlEvent = false;
			return;
		}
		StageUpdate.AllStageCtrlEvent = repeatScenario;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastRepeatScenario = repeatScenario;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
	}
}
