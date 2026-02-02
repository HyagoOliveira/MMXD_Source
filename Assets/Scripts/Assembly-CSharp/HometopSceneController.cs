using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using UnityEngine;
using enums;

public class HometopSceneController : OrangeSceneController
{
	private readonly string PATH_BG = "prefab/hometop/bg/";

	private readonly string PATH_BG_BASE = "MAINBG_BASE_001";

	[SerializeField]
	private GameObject[] arrayDisable;

	[SerializeField]
	private Transform CharacterPos;

	[SerializeField]
	private Transform bgPos;

	[SerializeField]
	private Camera characterCamera;

	[SerializeField]
	private OrangeCriSource SoundSource;

	private float[] characterOffset = new float[7] { 0.36f, 0.49f, 0.34f, 0.6f, 0.36f, 0.41f, 0.25f };

	private float[] cameraHeight = new float[7] { 1.28f, 1.44f, 1.3f, 1.69f, 0.85f, 1.35f, 1.35f };

	private float[] mmkCameraHeight = new float[2] { 0.85f, 1.2f };

	private string useBg = string.Empty;

	private StageHelper.STAGE_END_GO dayChangeKeep;

	private bool showNotice;

	private readonly string[] modelEndWith = new string[3] { "_U.prefab", "_U_S.prefab", "_U_S_S.prefab" };

	private string modelName = string.Empty;

	protected override void Awake()
	{
		base.Awake();
		float designFov = characterCamera.fieldOfView;
		if (OrangeGameUtility.SetNewFov(ref designFov))
		{
			characterCamera.fieldOfView = designFov;
		}
		showNotice = PlayerPrefs.GetInt(CtcWebViewNotice.FLAG_NOTICE_NEVER_SHOWING_TODAY, 0) == 0;
		SoundSource = base.gameObject.AddOrGetComponent<OrangeCriSource>();
		SoundSource.Initial(OrangeSSType.SYSTEM);
		SoundSource.f_vol = 1f;
		SoundSource._currentDis = 0f;
		SoundSource.IsVisiable = true;
		Singleton<GenericEventManager>.Instance.AttachEvent<CHARACTER_TABLE, WEAPON_TABLE, SKIN_TABLE>(EventManager.ID.UPDATE_RENDER_CHARACTER, UpdateCharacter);
		Singleton<GenericEventManager>.Instance.AttachEvent<bool>(EventManager.ID.UPDATE_HOMETOP_RENDER, UpdateHometopRender);
		MonoBehaviourSingleton<SteamManager>.Instance.AchievedAchievement(SteamAchievement.ST_ACHIEVEMENT_01);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		HometopUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<HometopUI>("UI_Hometop");
		if (null != uI)
		{
			uI.OnClickCloseBtn();
		}
		SoundSource.IsVisiable = false;
		Singleton<GenericEventManager>.Instance.DetachEvent<CHARACTER_TABLE, WEAPON_TABLE, SKIN_TABLE>(EventManager.ID.UPDATE_RENDER_CHARACTER, UpdateCharacter);
		Singleton<GenericEventManager>.Instance.DetachEvent<bool>(EventManager.ID.UPDATE_HOMETOP_RENDER, UpdateHometopRender);
	}

	protected override void SceneInit()
	{
		if (!MonoBehaviourSingleton<OrangeSceneManager>.Instance.IsActiveScene("hometop"))
		{
			return;
		}
		MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.LoadRequestPath();
		useBg = PATH_BG_BASE;
		EVENT_TABLE[] array = ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT.Values.Where((EVENT_TABLE x) => x.n_TYPE == 14).ToArray();
		if (array != null)
		{
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			foreach (EVENT_TABLE eVENT_TABLE in array)
			{
				if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(eVENT_TABLE.s_IMG) && ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(eVENT_TABLE.s_BEGIN_TIME, eVENT_TABLE.s_END_TIME, serverUnixTimeNowUTC))
				{
					useBg = eVENT_TABLE.s_IMG;
					break;
				}
			}
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(PATH_BG + useBg, useBg, delegate(GameObject obj)
		{
			if (obj != null)
			{
				DestroyOldItem(bgPos);
				UnityEngine.Object.Instantiate(obj, bgPos, true);
				AnimatorSoundHelper[] componentsInChildren = bgPos.GetComponentsInChildren<AnimatorSoundHelper>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].SoundSource = SoundSource;
				}
			}
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckReturnLastNetGame(delegate
			{
				TurtorialUI.LoadTurtorialUIByAB(delegate
				{
					MonoBehaviourSingleton<UIManager>.Instance.GetOrLoadUI("UI_Hometop", delegate(HometopUI hometopUI)
					{
						hometopUI.Setup(this, MonoBehaviourSingleton<GameServerService>.Instance.DayChange);
						CharacterInfo characterInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara];
						CHARACTER_TABLE character = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara];
						WEAPON_TABLE value = null;
						SKIN_TABLE value2 = null;
						ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.MainWeaponID, out value);
						ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(characterInfo.netInfo.Skin, out value2);
						UpdateCharacter(character, value, value2);
						dayChangeKeep = StageHelper.STAGE_END_GO.HOMETOP;
						if (MonoBehaviourSingleton<GameServerService>.Instance.DayChange && !TurtorialUI.IsTutorialing())
						{
							MonoBehaviourSingleton<GameServerService>.Instance.DayChange = false;
							if (ManagedSingleton<StageHelper>.Instance.nStageEndGoUI != StageHelper.STAGE_END_GO.LOGIN_BONUS)
							{
								dayChangeKeep = StageHelper.STAGE_END_GO.HOMETOP;
							}
							ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.LOGIN_BONUS;
						}
						UpdateHometopState(hometopUI);
					});
				});
			});
		});
	}

	private void UpdateHometopState(HometopUI hometopUI)
	{
		switch (ManagedSingleton<StageHelper>.Instance.nStageEndGoUI)
		{
		default:
			UpdateHometopRender(true);
			hometopUI.OnUpdateHometopData();
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			MonoBehaviourSingleton<UIManager>.Instance.ForcePowerUPSE = true;
			break;
		case StageHelper.STAGE_END_GO.STORYSTAGESELECT:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_StoryStageSelect", delegate(StoryStageSelectUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
				ui.Setup();
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			});
			break;
		case StageHelper.STAGE_END_GO.COOPSTAGESELECT:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CoopStageSelectUI", delegate(CoopStageSelectUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
				ui.Setup();
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			});
			break;
		case StageHelper.STAGE_END_GO.PVPROOMSELECT:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpRoomSelect", delegate(PvpRoomSelectUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
				ui.Setup();
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			});
			break;
		case StageHelper.STAGE_END_GO.SEASON:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Qualifing", delegate(QualifingUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
				ui.Setup();
			});
			break;
		case StageHelper.STAGE_END_GO.BOSSCHALLENGE:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_BossChallenge", delegate(UI_Challenge ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			});
			break;
		case StageHelper.STAGE_END_GO.GACHA:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Gacha", delegate(GachaUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
				ui.Setup(delegate
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui2)
					{
						ui2.Setup(ManagedSingleton<StageHelper>.Instance.nStageEndParam[0] as NetRewardsEntity, (bool)ManagedSingleton<StageHelper>.Instance.nStageEndParam[1]);
					});
				});
			});
			break;
		case StageHelper.STAGE_END_GO.LOGIN_BONUS:
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.PVP_REWARD);
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			List<EVENT_TABLE> p_listSpecLogin = ManagedSingleton<ExtendDataHelper>.Instance.GetEventTableByType(enums.EventType.EVENT_LOGIN, serverUnixTimeNowUTC);
			if (p_listSpecLogin != null)
			{
				CheckLoginBonus(0, ref p_listSpecLogin);
				break;
			}
			UpdateHometopRender(true);
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			MonoBehaviourSingleton<OrangeIAP>.Instance.CheckClientNewReceipt(0, MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicNewReceipt.Values.ToList(), new List<NetRewardInfo>(), CheckDayChangeKeep);
			MonoBehaviourSingleton<UIManager>.Instance.ForcePowerUPSE = true;
			break;
		}
		case StageHelper.STAGE_END_GO.PVPRANDOMMATCHING:
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = MonoBehaviourSingleton<OrangeMatchManager>.Instance.LastRqPvpMatchType;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpMatch", delegate(PvpMatchUI ui)
				{
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
					ui.Init();
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_HOMETOP_HINT, IconHintChk.HINT_TYPE.PVP_REWARD);
					MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
				});
			});
			break;
		case StageHelper.STAGE_END_GO.SEASONRANDOMMATCHING:
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = MonoBehaviourSingleton<OrangeMatchManager>.Instance.LastRqPvpMatchType;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
			{
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.OnInitSeasonCharaterMaxHPList();
				ManagedSingleton<PlayerNetManager>.Instance.SealBattleSettingReq(MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList, delegate(string setting)
				{
					MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting = setting;
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpMatch", delegate(PvpMatchUI ui)
					{
						ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
						ui.Init();
						MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
					});
				}, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nMainWeaponID, MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.nSubWeaponID);
			}, false);
			break;
		case StageHelper.STAGE_END_GO.GUIDE:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Guide", delegate(GuideUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
				ui.Setup();
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			});
			break;
		case StageHelper.STAGE_END_GO.ACTIVITYEVENT:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_EventStage", delegate(EventStageMain ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
				ui.Setup(ManagedSingleton<StageHelper>.Instance.activityEventStageMainID);
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			});
			break;
		case StageHelper.STAGE_END_GO.WOLRDBOSS:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_WORLDBOSSEVENT", delegate(WorldBossEventUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
				ui.Setup();
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			});
			break;
		case StageHelper.STAGE_END_GO.TOTALWAR:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_TotalWar", delegate(TotalWarUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
				ui.Setup();
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			});
			break;
		case StageHelper.STAGE_END_GO.CRUSADE:
			Singleton<GuildSystem>.Instance.OnGetGuildInfoOnceEvent += LoadGuildSceneAndCrusadeUI;
			Singleton<GuildSystem>.Instance.ReqGetGuildInfo();
			break;
		case StageHelper.STAGE_END_GO.FRIENDPVPCREATEPRIVATEROOM:
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpGameType = PVPGameType.OneVSOneBattle;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = PVPMatchType.FriendOneVSOne;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpRoomSelect", delegate(PvpRoomSelectUI ui)
				{
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
					ui.Setup();
					ManagedSingleton<PlayerNetManager>.Instance.SealBattleSettingReq(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, delegate(string setting)
					{
						MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting = setting;
						MonoBehaviourSingleton<FriendPVPHelper>.Instance.CreatePrivateRoomAndWaitForGuest();
						MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
					});
				});
			});
			break;
		case StageHelper.STAGE_END_GO.FRIENDPVPJOINPRIVATEROOM:
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpGameType = PVPGameType.OneVSOneBattle;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = PVPMatchType.FriendOneVSOne;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpRoomSelect", delegate(PvpRoomSelectUI ui)
				{
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(hometopUI.OnUpdateHometopData));
					ui.Setup();
					ManagedSingleton<PlayerNetManager>.Instance.SealBattleSettingReq(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, delegate(string setting)
					{
						MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting = setting;
						MonoBehaviourSingleton<FriendPVPHelper>.Instance.StartReconnectingToHost();
						MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
					});
				});
			});
			break;
		}
		ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.HOMETOP;
	}

	private void LoadGuildSceneAndCrusadeUI()
	{
		if (Singleton<GuildSystem>.Instance.GuildInfoCache != null)
		{
			Singleton<GuildSystem>.Instance.OpenGuildLobbyScene(OnGuildLobbySceneLoaded);
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
		}
	}

	private void OnGuildLobbySceneLoaded()
	{
		Singleton<GuildSystem>.Instance.MainSceneController.TriggerGuildBossBtnAction(true);
	}

	private bool CheckLoginBonus(int p_nowIdx, ref List<EVENT_TABLE> p_listSpecLogin)
	{
		if (p_nowIdx >= p_listSpecLogin.Count)
		{
			if (p_nowIdx > 0)
			{
				UpdateHometopRender(true);
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
				MonoBehaviourSingleton<OrangeIAP>.Instance.CheckClientNewReceipt(0, MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicNewReceipt.Values.ToList(), new List<NetRewardInfo>(), CheckDayChangeKeep);
				MonoBehaviourSingleton<UIManager>.Instance.ForcePowerUPSE = true;
			}
			return true;
		}
		int nowIdx = p_nowIdx;
		List<EVENT_TABLE> listSpecLogin = p_listSpecLogin;
		List<MISSION_TABLE> listBonus = ManagedSingleton<MissionHelper>.Instance.GetMissionByTypeAndSubType(MissionType.LoginBouns, listSpecLogin[p_nowIdx].n_TYPE_X);
		int n_COUNTER = listBonus[0].n_COUNTER;
		int netCounter = ManagedSingleton<MissionHelper>.Instance.GetMissionCounter(n_COUNTER);
		if (netCounter == 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			MonoBehaviourSingleton<UIManager>.Instance.ForcePowerUPSE = true;
			return true;
		}
		bool flag = IsRewriteLocalCounter(n_COUNTER, netCounter);
		if (listSpecLogin[p_nowIdx].n_TYPE_X == 0)
		{
			if (flag)
			{
				showNotice = true;
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_LoginBonusLoop", delegate(LoginBonusLoopUI ui)
				{
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
					{
						if (CheckLoginBonus(nowIdx + 1, ref listSpecLogin))
						{
							UpdateHometopRender(true);
							MonoBehaviourSingleton<OrangeIAP>.Instance.CheckClientNewReceipt(0, MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicNewReceipt.Values.ToList(), new List<NetRewardInfo>(), CheckDayChangeKeep);
						}
					});
					ui.CloseSE = SystemSE.NONE;
					ui.Setup(netCounter - 1, listBonus);
				});
				return false;
			}
			return CheckLoginBonus(nowIdx + 1, ref listSpecLogin);
		}
		if (flag)
		{
			showNotice = true;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_LoginBonusSpecial", delegate(LoginBonusSpecialUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					if (CheckLoginBonus(nowIdx + 1, ref listSpecLogin))
					{
						UpdateHometopRender(true);
						MonoBehaviourSingleton<OrangeIAP>.Instance.CheckClientNewReceipt(0, MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicNewReceipt.Values.ToList(), new List<NetRewardInfo>(), CheckDayChangeKeep);
					}
				});
				ui.CloseSE = SystemSE.NONE;
				ui.Setup(netCounter - 1, listSpecLogin[p_nowIdx], listBonus);
			});
			return false;
		}
		return CheckLoginBonus(nowIdx + 1, ref listSpecLogin);
	}

	private bool IsRewriteLocalCounter(int counterKey, int netCounter)
	{
		int value = 0;
		if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicLoginBonusCounter.TryGetValue(counterKey, out value))
		{
			if (value != netCounter)
			{
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicLoginBonusCounter[counterKey] = netCounter;
				return true;
			}
			return false;
		}
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicLoginBonusCounter.Add(counterKey, netCounter);
		return true;
	}

	private void CheckDayChangeKeep()
	{
		if (dayChangeKeep == StageHelper.STAGE_END_GO.HOMETOP)
		{
			ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = dayChangeKeep;
			CheckNotice();
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
			return;
		}
		ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = dayChangeKeep;
		dayChangeKeep = StageHelper.STAGE_END_GO.HOMETOP;
		MonoBehaviourSingleton<UIManager>.Instance.GetOrLoadUI("UI_Hometop", delegate(HometopUI hometopUI)
		{
			hometopUI.Setup(this);
			UpdateHometopState(hometopUI);
		});
	}

	private void CheckNotice()
	{
		if (showNotice && !TurtorialUI.IsTutorialing())
		{
			showNotice = false;
			HometopUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<HometopUI>("UI_Hometop");
			if (uI != null)
			{
				uI.OnOpenNoticePopup();
			}
		}
	}

	private void UpdateCharacter(CHARACTER_TABLE character, WEAPON_TABLE equipWeapon, SKIN_TABLE skinTable)
	{
		if (skinTable != null)
		{
			if (modelName == skinTable.s_MODEL)
			{
				return;
			}
			modelName = skinTable.s_MODEL;
		}
		else
		{
			if (modelName == character.s_MODEL)
			{
				return;
			}
			modelName = character.s_MODEL;
		}
		int num = 0;
		if (character.n_SPECIAL_SHOWPOSE > 0)
		{
			num = 1;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/character/" + modelName, modelName + modelEndWith[num], delegate(UnityEngine.Object obj)
		{
			if (!(obj == null))
			{
				GameObject go = null;
				Animator animator = null;
				RuntimeAnimatorController tRuntimeAnimatorController = null;
				if (equipWeapon != null && character.n_SPECIAL_SHOWPOSE > 0)
				{
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/animator/empty", "empty2", delegate(RuntimeAnimatorController obj2)
					{
						tRuntimeAnimatorController = obj2;
						string[] bnudles = new string[2]
						{
							string.Empty,
							AssetBundleScriptableObject.Instance.m_newmodel_weapon + equipWeapon.s_MODEL
						};
						string[] clips = OrangeAnimatonHelper.GetUniqueDebutName(modelName, out bnudles[0]);
						MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(bnudles, delegate
						{
							DestroyOldItem(CharacterPos);
							AnimatorOverrideController runtimeAnimatorController = OrangeAnimatonHelper.OverrideRuntimeAnimClip(ref tRuntimeAnimatorController, ref bnudles[0], ref clips);
							go = UnityEngine.Object.Instantiate(obj, CharacterPos, false) as GameObject;
							go.transform.localScale = new Vector3(character.f_MODELSIZE, character.f_MODELSIZE, character.f_MODELSIZE);
							animator = go.GetComponent<Animator>();
							float[] characterTableExtraSize = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTableExtraSize(character.s_ModelExtraSize);
							if (characterTableExtraSize.Length > 3)
							{
								go.transform.localPosition = new Vector3(0f, characterTableExtraSize[3], 0f);
							}
							else
							{
								go.transform.localPosition = new Vector3(0f, 0f, 0f);
							}
							animator.runtimeAnimatorController = runtimeAnimatorController;
							animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
							animator.Play("2");
							CharacterAnimatorStandBy characterAnimatorStandBy2 = go.AddOrGetComponent<CharacterAnimatorStandBy>();
							characterAnimatorStandBy2.IsSpecialPos = true;
							characterAnimatorStandBy2.Init(character.s_MODEL, character.s_ANIMATOR, tRuntimeAnimatorController, animator, character.n_WEAPON_MOTION, characterTableExtraSize);
							characterAnimatorStandBy2.SetWeapon(MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(bnudles[1], equipWeapon.s_MODEL + "_U.prefab"), equipWeapon);
							SetCameraHeightAndCharacterOffset(animator.avatar.name);
						}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
					});
				}
				else
				{
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("model/animator/empty", "empty", delegate(UnityEngine.Object obj2)
					{
						tRuntimeAnimatorController = (RuntimeAnimatorController)obj2;
						if (equipWeapon != null)
						{
							DestroyOldItem(CharacterPos);
							go = UnityEngine.Object.Instantiate(obj, CharacterPos, false) as GameObject;
							animator = go.GetComponent<Animator>();
							go.AddOrGetComponent<CharacterAnimatonRandWink>().Setup(animator);
							CharacterAnimatorStandBy characterAnimatorStandBy = go.AddOrGetComponent<CharacterAnimatorStandBy>();
							characterAnimatorStandBy.Init(character.s_MODEL, character.s_ANIMATOR, tRuntimeAnimatorController, animator, character.n_WEAPON_MOTION, ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTableExtraSize(character.s_ModelExtraSize));
							characterAnimatorStandBy.UpdateWeapon(equipWeapon.n_ID, true, null);
							SetCameraHeightAndCharacterOffset(animator.avatar.name);
						}
					});
				}
			}
		});
	}

	private void UpdateHometopRender(bool active)
	{
		GameObject[] array = arrayDisable;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.activeSelf != active)
			{
				gameObject.SetActive(active);
			}
		}
		if (active)
		{
			StartCoroutine(ManagedSingleton<CharacterHelper>.Instance.CheckCharacterUpgrades());
		}
	}

	private void SetCameraHeightAndCharacterOffset(string useAnimator)
	{
		float num = cameraHeight[0];
		switch (useAnimator)
		{
		default:
			num = cameraHeight[2];
			CharacterPos.localPosition = new Vector3(0f, characterOffset[2], 0f);
			break;
		case "ORU_RIGAvatar":
			num = cameraHeight[0];
			CharacterPos.localPosition = new Vector3(0f, characterOffset[0], 0f);
			break;
		case "MSU_RIGAvatar":
			num = cameraHeight[1];
			CharacterPos.localPosition = new Vector3(0f, characterOffset[1], 0f);
			break;
		case "MMU_RIGAvatar":
		case "FMU_RIGAvatar":
			num = cameraHeight[2];
			CharacterPos.localPosition = new Vector3(0f, characterOffset[2], 0f);
			break;
		case "MLU_RIGAvatar":
			num = cameraHeight[3];
			CharacterPos.localPosition = new Vector3(0f, characterOffset[3], 0f);
			break;
		case "MMK_RIGAvatar":
			cameraHeight[4] = (useBg.Equals(PATH_BG_BASE) ? mmkCameraHeight[0] : mmkCameraHeight[1]);
			num = cameraHeight[4];
			CharacterPos.localPosition = new Vector3(0f, characterOffset[4], 0f);
			break;
		case "FLU_RIGAvatar":
			num = cameraHeight[5];
			CharacterPos.localPosition = new Vector3(0f, characterOffset[5], 0f);
			break;
		case "FSU_RIGAvatar":
			num = cameraHeight[6];
			CharacterPos.localPosition = new Vector3(0f, characterOffset[6], 0f);
			break;
		}
		if (num != characterCamera.transform.position.y)
		{
			LeanTween.moveLocalY(characterCamera.gameObject, num, 0.2f);
		}
	}

	private void DestroyOldItem(Transform parent)
	{
		for (int num = parent.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(parent.GetChild(num).gameObject);
		}
	}

	public Transform GetCharacterPos()
	{
		return CharacterPos;
	}
}
