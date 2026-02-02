#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using Better;
using CallbackDefs;
using enums;

public class UILinkHelper : ManagedSingleton<UILinkHelper>
{
	private delegate void Callback(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb);

	public enum LINK
	{
		STORY = 1,
		EVENT = 2,
		CHALLENGE = 3,
		COOP = 4,
		MISSION = 5,
		SHOP = 6,
		GACHA = 7,
		RESEARCH = 8,
		PVP = 9,
		FRIEND = 10,
		QUALITING = 11,
		TOWER = 12,
		SPEED = 13,
		WORLD_BOSS = 14,
		GUILD_BOSS = 15,
		TOTAL_WAR = 16,
		DEEP_RECORD = 17,
		WANTED = 18
	}

	private System.Collections.Generic.Dictionary<int, Callback> dicLink;

	public override void Initialize()
	{
		dicLink = new Better.Dictionary<int, Callback>();
		dicLink.Add(1, LoadStoryUI);
		dicLink.Add(2, LoadEventUI);
		dicLink.Add(3, LoadBossChallengeUI);
		dicLink.Add(4, LoadCoopPlayUI);
		dicLink.Add(5, LoadMissionUI);
		dicLink.Add(6, LoadShopUI);
		dicLink.Add(7, LoadGachaUI);
		dicLink.Add(8, LoadResearchUI);
		dicLink.Add(9, LoadPVPRewardUI);
		dicLink.Add(10, LoadFriendUI);
		dicLink.Add(11, LoadQualitingUI);
		dicLink.Add(12, LoadTowerUI);
		dicLink.Add(13, LoadSpeedUI);
		dicLink.Add(14, LoadWorldBossUI);
		dicLink.Add(15, LoadGuildBossUI);
		dicLink.Add(16, LoadTotalWarUI);
		dicLink.Add(17, LoadDeepRecordUI);
		dicLink.Add(18, LoadWantedUI);
	}

	public override void Dispose()
	{
		dicLink.Clear();
		dicLink = null;
	}

	public void LoadUI(int p_link, CallbackDefs.Callback p_cb = null)
	{
		HOWTOGET_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.HOWTOGET_TABLE_DICT.TryGetValue(p_link, out value))
		{
			LoadUI(value, p_cb);
		}
	}

	public void LoadUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
		CallbackDefs.Callback closeCB = null;
		MonoBehaviourSingleton<UIManager>.Instance.UILinkPrepare(delegate
		{
			closeCB = p_cb;
			LeanTween.delayedCall(0.3f, (Action)delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
				dicLink[howToGetTable.n_UILINK](howToGetTable, closeCB);
			});
		});
	}

	private void LoadStoryUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		TUTORIAL_TABLE value = null;
		int currentTurtorialID = ManagedSingleton<PlayerHelper>.Instance.GetCurrentTurtorialID();
		if (ManagedSingleton<OrangeDataManager>.Instance.TUTORIAL_TABLE_DICT.TryGetValue(currentTurtorialID, out value) && value.s_TRIGGER.Contains("UI_StoryStageSelect"))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_StoryStageSelect", delegate(StoryStageSelectUI ui)
			{
				ui.closeCB = (CallbackDefs.Callback)Delegate.Combine(ui.closeCB, p_cb);
				ui.Setup();
			});
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_StoryStageSelect", delegate(StoryStageSelectUI ui)
		{
			if (howToGetTable.n_VALUE_X != 0)
			{
				ui.TargetArea = howToGetTable.n_VALUE_X;
			}
			if (howToGetTable.n_VALUE_Z != 0)
			{
				ui.TargetDifficulty = (StoryStageSelectUI.DIFFICULTY_TYPE)howToGetTable.n_VALUE_Z;
			}
			ui.Setup(false);
			ui.closeCB = p_cb;
			STAGE_TABLE targetStage = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.FirstOrDefault((STAGE_TABLE x) => x.n_MAIN == howToGetTable.n_VALUE_X && x.n_SUB == howToGetTable.n_VALUE_Y && x.n_DIFFICULTY == howToGetTable.n_VALUE_Z);
			if (targetStage != null)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChallengePopup", delegate(UI_ChallengePopup ui2)
				{
					ui2.Setup(targetStage);
				});
			}
		});
	}

	private void LoadEventUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		if (howToGetTable.n_VALUE_Y > 0)
		{
			STAGE_TABLE sTAGE_TABLE = null;
			sTAGE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.FirstOrDefault((STAGE_TABLE x) => x.n_MAIN == howToGetTable.n_VALUE_X);
			if (sTAGE_TABLE != null)
			{
				StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
				if (!ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(sTAGE_TABLE, ref condition) && condition != StageHelper.StageJoinCondition.AP && condition != StageHelper.StageJoinCondition.COUNT)
				{
					ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(sTAGE_TABLE, condition);
					p_cb.CheckTargetToInvoke();
					return;
				}
			}
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_EventStage", delegate(EventStageMain ui)
		{
			ui.Setup(howToGetTable.n_VALUE_X);
			ui.closeCB = p_cb;
		});
	}

	private void LoadBossChallengeUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bTowerBase = false;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentChallengeTab = 0;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BossChallenge", delegate(UI_Challenge ui)
		{
			ui.closeCB = p_cb;
			ui.NowPage = howToGetTable.n_VALUE_Z - 1;
			if (howToGetTable.n_VALUE_X != 0)
			{
				STAGE_TABLE targetStage = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.FirstOrDefault((STAGE_TABLE x) => x.n_MAIN == howToGetTable.n_VALUE_X && x.n_SUB == howToGetTable.n_VALUE_Y && x.n_DIFFICULTY == 1);
				if (targetStage != null)
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BossStand", delegate(BossStandUI bossStandUI)
					{
						bossStandUI.Setup(targetStage.w_BOSS_INTRO);
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChallengePopup", delegate(UI_ChallengePopup challengePopupUI)
						{
							challengePopupUI.Setup(targetStage);
							challengePopupUI.closeCB = delegate
							{
								bossStandUI.OnClickCloseBtn();
							};
						});
					});
				}
			}
		});
	}

	private void LoadCoopPlayUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CoopStageSelectUI", delegate(CoopStageSelectUI ui)
		{
			if (howToGetTable.n_VALUE_X != 0)
			{
				ui.LinkSelectStageMainId = howToGetTable.n_VALUE_X;
			}
			if (howToGetTable.n_VALUE_Y != 0)
			{
				ui.LinkSelectStageSubId = howToGetTable.n_VALUE_Y;
			}
			ui.Setup();
			ui.closeCB = p_cb;
		});
	}

	private void LoadMissionUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Mission", delegate(MissionUI ui)
		{
			ui.Setup((MissionType)howToGetTable.n_VALUE_X);
			ui.closeCB = p_cb;
		});
	}

	private void LoadShopUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShopTop", delegate(ShopTopUI ui)
		{
			if (howToGetTable.n_VALUE_Z > 0)
			{
				ui.DefaultSubIdx = howToGetTable.n_VALUE_Z;
			}
			ui.Setup((ShopTopUI.ShopSelectTab)howToGetTable.n_VALUE_X, (howToGetTable.n_VALUE_Y <= 0) ? ShopTopUI.ShopSubType.sub_1 : ((ShopTopUI.ShopSubType)howToGetTable.n_VALUE_Y));
			ui.closeCB = p_cb;
		});
	}

	private void LoadGachaUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Gacha", delegate(GachaUI ui)
			{
				ui.SetupByGachaGroupId(howToGetTable.n_VALUE_X);
				ui.closeCB = p_cb;
			});
		});
	}

	private void LoadResearchUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Research", delegate(ResearchUI ui)
		{
			ui.Setup(ResearchUI.ResearchPageType.NORMAL_RESEARCH, howToGetTable.n_VALUE_X);
			ui.closeCB = p_cb;
		});
	}

	private void LoadPVPRewardUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpRoomSelect", delegate(PvpRoomSelectUI ui)
		{
			ui.Setup();
			ui.OnCickRewardBtn();
			ui.closeCB = p_cb;
		});
	}

	private void LoadFriendUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendMain", delegate(FriendMainUI ui)
		{
			ui.Setup();
			ui.closeCB = p_cb;
		});
	}

	private void LoadQualitingUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Qualifing", delegate(QualifingUI ui)
		{
			ui.Setup();
			ui.closeCB = p_cb;
		});
	}

	private void LoadTowerUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bTowerBase = true;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentChallengeTab = 1;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BossChallenge", delegate(UI_Challenge ui)
		{
			ui.closeCB = p_cb;
		});
	}

	private void LoadSpeedUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bTowerBase = false;
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentChallengeTab = 2;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BossChallenge", delegate(UI_Challenge ui)
		{
			ui.closeCB = p_cb;
		});
	}

	private void LoadWorldBossUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WORLDBOSSEVENT", delegate(WorldBossEventUI ui)
		{
			ui.Setup();
			ui.closeCB = p_cb;
		});
	}

	private void LoadGuildBossUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		Singleton<GuildSystem>.Instance.OnGetCheckGuildStateOnceEvent += OnGetCheckGuildStateEventOfGuildBoss;
		Singleton<GuildSystem>.Instance.ReqCheckGuildState();
	}

	private void OnGetCheckGuildStateEventOfGuildBoss()
	{
		if (!Singleton<GuildSystem>.Instance.HasGuild)
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_BANNER_ERROR");
			return;
		}
		Singleton<CrusadeSystem>.Instance.OnRetrieveCrusadeInfoOnceEvent += OnRetrieveCrusadeInfoOnceEvent;
		Singleton<CrusadeSystem>.Instance.RetrieveCrusadeInfo();
	}

	private void OnRetrieveCrusadeInfoOnceEvent()
	{
		if (!Singleton<CrusadeSystem>.Instance.HasEvent)
		{
			Debug.LogError("No GuildBoss Event");
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildBossMainUI>("UI_GuildBossMain", OnGuildBossUILoaded);
		}
	}

	private void OnGuildBossUILoaded(GuildBossMainUI ui)
	{
		ui.Setup();
	}

	private void LoadTotalWarUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_TotalWar", delegate(TotalWarUI ui)
		{
			ui.Setup();
		});
	}

	private void LoadDeepRecordUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		ManagedSingleton<DeepRecordHelper>.Instance.RetrieveRecordGridInfoReq(false);
	}

	private void LoadWantedUI(HOWTOGET_TABLE howToGetTable, CallbackDefs.Callback p_cb = null)
	{
		Singleton<GuildSystem>.Instance.OnGetGuildInfoOnceEvent += LoadGuildSceneAndWantedUI;
		Singleton<GuildSystem>.Instance.ReqGetGuildInfo();
	}

	private void LoadGuildSceneAndWantedUI()
	{
		if (Singleton<GuildSystem>.Instance.GuildInfoCache != null)
		{
			MonoBehaviourSingleton<UIManager>.Instance.BackToHometop();
			Singleton<GuildSystem>.Instance.OpenGuildLobbyScene(OnGuildLobbySceneLoadedOfWanted);
		}
		else
		{
			CommonUIHelper.ShowCommonTipUI("GUILD_BANNER_ERROR");
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
		}
	}

	private void OnGuildLobbySceneLoadedOfWanted()
	{
		Singleton<GuildSystem>.Instance.MainSceneController.TriggerWantedBtnAction(true);
	}
}
