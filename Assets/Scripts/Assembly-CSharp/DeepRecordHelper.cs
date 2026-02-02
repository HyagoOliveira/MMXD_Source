using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using OrangeApi;
using UnityEngine;
using enums;

public class DeepRecordHelper : ManagedSingleton<DeepRecordHelper>
{
	private enum AbilityType
	{
		Battle = 1,
		Explore = 2,
		Action = 3
	}

	private enum HoldValType
	{
		Explore = 1,
		Battle = 2,
		Total = 3
	}

	[Flags]
	public enum GridRefreashEvent
	{
		None = 0,
		Battle = 1,
		Ability = 2,
		Random = 4,
		All = 7
	}

	public static string RichTextRed = "<color=#FF0000>{0}</color>";

	public static string RichTextGreen = "<color=#00FF00>{0}</color>";

	public const int SET_LIMIT = 10;

	public const int JOIN_PLAYER_LIMIT = 4;

	public List<NetCommonCoordinateInfo> ListMovePoint = new List<NetCommonCoordinateInfo>();

	public List<NetRecordGridBattleLogInfo> ListBattleLog = new List<NetRecordGridBattleLogInfo>();

	public List<NetRecordGridAbilityLogInfo> ListAbilityLog = new List<NetRecordGridAbilityLogInfo>();

	public List<NetRecordGridRandomLogInfo> ListRandomLog = new List<NetRecordGridRandomLogInfo>();

	public List<NetRecordGridMultiMoveLogInfo> ListMultiMoveLog = new List<NetRecordGridMultiMoveLogInfo>();

	public DateTime ApiRefreashTime;

	private int mapWidth = -1;

	private int mapHeight = -1;

	private GridRefreashEvent RefreashFlag = GridRefreashEvent.All;

	public NetRecordGridPlayerInfo PlayerInfo { get; set; }

	public List<NetRecordGridOtherPlayerInfo> OtherPlayerList { get; set; }

	public NetRecordGridMapInfo MapInfo { get; set; }

	public bool[,] FinishPoints { get; private set; }

	public int PlayerMoveCount { get; private set; }

	public DeepRecordTeamSetUI.Status Status { get; set; }

	public bool IsExpired
	{
		get
		{
			if (MapInfo != null)
			{
				return Convert.ToInt32(MapInfo.ResetTime - MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC) <= 0;
			}
			return true;
		}
	}

	public DeepRecordMainUI MainUI { get; set; }

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public override void Reset()
	{
		if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene == "hometop")
		{
			ApiRefreashTime = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerTimeNowUTC;
		}
		else
		{
			ForceReset();
		}
	}

	public void ForceReset()
	{
		PlayerInfo = null;
		OtherPlayerList = null;
		MapInfo = null;
		FinishPoints = null;
		Status = DeepRecordTeamSetUI.Status.Unknow;
		MainUI = null;
		ClearLog();
	}

	public void ClearLog()
	{
		RefreashFlag = GridRefreashEvent.All;
		ListMovePoint = new List<NetCommonCoordinateInfo>();
		ListBattleLog = new List<NetRecordGridBattleLogInfo>();
		ListAbilityLog = new List<NetRecordGridAbilityLogInfo>();
		ListRandomLog = new List<NetRecordGridRandomLogInfo>();
	}

	public static int GetCharacterRecordVal(CharacterHelper.SortType sortType, CharacterInfo characterInfo)
	{
		CHARACTER_TABLE characterTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(characterInfo.netInfo.CharacterID);
		int num = 0;
		if (characterTable != null)
		{
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				num = characterTable.n_RECORD_BATTLE;
				break;
			case CharacterHelper.SortType.ACTION:
				num = characterTable.n_RECORD_ACTION;
				break;
			case CharacterHelper.SortType.EXPLORE:
				num = characterTable.n_RECORD_EXPLORE;
				break;
			case CharacterHelper.SortType.TOTAL:
				return GetCharacterRecordVal(CharacterHelper.SortType.BATTLE, characterInfo) + GetCharacterRecordVal(CharacterHelper.SortType.ACTION, characterInfo) + GetCharacterRecordVal(CharacterHelper.SortType.EXPLORE, characterInfo);
			}
			return Mathf.FloorToInt((float)(num * GetCharacterRecordWeightedByStar(sortType, characterInfo.netInfo.Star)) / 100f);
		}
		return 0;
	}

	public static int GetCharacterRecordWeightedByStar(CharacterHelper.SortType sortType, sbyte star)
	{
		int result = 100;
		switch (star)
		{
		case 1:
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				result = OrangeConst.CHARACTER_RECORD_BATTLE_1;
				break;
			case CharacterHelper.SortType.ACTION:
				result = OrangeConst.CHARACTER_RECORD_ACTION_1;
				break;
			case CharacterHelper.SortType.EXPLORE:
				result = OrangeConst.CHARACTER_RECORD_EXPLORE_1;
				break;
			}
			break;
		case 2:
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				result = OrangeConst.CHARACTER_RECORD_BATTLE_2;
				break;
			case CharacterHelper.SortType.ACTION:
				result = OrangeConst.CHARACTER_RECORD_ACTION_2;
				break;
			case CharacterHelper.SortType.EXPLORE:
				result = OrangeConst.CHARACTER_RECORD_EXPLORE_2;
				break;
			}
			break;
		case 3:
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				result = OrangeConst.CHARACTER_RECORD_BATTLE_3;
				break;
			case CharacterHelper.SortType.ACTION:
				result = OrangeConst.CHARACTER_RECORD_ACTION_3;
				break;
			case CharacterHelper.SortType.EXPLORE:
				result = OrangeConst.CHARACTER_RECORD_EXPLORE_3;
				break;
			}
			break;
		case 4:
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				result = OrangeConst.CHARACTER_RECORD_BATTLE_4;
				break;
			case CharacterHelper.SortType.ACTION:
				result = OrangeConst.CHARACTER_RECORD_ACTION_4;
				break;
			case CharacterHelper.SortType.EXPLORE:
				result = OrangeConst.CHARACTER_RECORD_EXPLORE_4;
				break;
			}
			break;
		case 5:
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				result = OrangeConst.CHARACTER_RECORD_BATTLE_5;
				break;
			case CharacterHelper.SortType.ACTION:
				result = OrangeConst.CHARACTER_RECORD_ACTION_5;
				break;
			case CharacterHelper.SortType.EXPLORE:
				result = OrangeConst.CHARACTER_RECORD_EXPLORE_5;
				break;
			}
			break;
		}
		return result;
	}

	public static int GetWeaponRecordVal(CharacterHelper.SortType sortType, WeaponInfo weaponInfo)
	{
		WEAPON_TABLE weaponTable = ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponTable(weaponInfo.netInfo.WeaponID);
		int num = 0;
		if (weaponTable != null)
		{
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				num = weaponTable.n_RECORD_BATTLE;
				break;
			case CharacterHelper.SortType.ACTION:
				num = weaponTable.n_RECORD_ACTION;
				break;
			case CharacterHelper.SortType.EXPLORE:
				num = weaponTable.n_RECORD_EXPLORE;
				break;
			case CharacterHelper.SortType.TOTAL:
				return GetWeaponRecordVal(CharacterHelper.SortType.BATTLE, weaponInfo) + GetWeaponRecordVal(CharacterHelper.SortType.ACTION, weaponInfo) + GetWeaponRecordVal(CharacterHelper.SortType.EXPLORE, weaponInfo);
			}
			return Mathf.FloorToInt((float)(num * GetWeaponRecordWeightedByStar(sortType, weaponInfo.netInfo.Star)) / 100f);
		}
		return 0;
	}

	public static int GetWeaponRecordWeightedByStar(CharacterHelper.SortType sortType, byte star)
	{
		int result = 100;
		switch (star)
		{
		case 1:
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				result = OrangeConst.WEAPON_RECORD_BATTLE_1;
				break;
			case CharacterHelper.SortType.ACTION:
				result = OrangeConst.WEAPON_RECORD_ACTION_1;
				break;
			case CharacterHelper.SortType.EXPLORE:
				result = OrangeConst.WEAPON_RECORD_EXPLORE_1;
				break;
			}
			break;
		case 2:
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				result = OrangeConst.WEAPON_RECORD_BATTLE_2;
				break;
			case CharacterHelper.SortType.ACTION:
				result = OrangeConst.WEAPON_RECORD_ACTION_2;
				break;
			case CharacterHelper.SortType.EXPLORE:
				result = OrangeConst.WEAPON_RECORD_EXPLORE_2;
				break;
			}
			break;
		case 3:
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				result = OrangeConst.WEAPON_RECORD_BATTLE_3;
				break;
			case CharacterHelper.SortType.ACTION:
				result = OrangeConst.WEAPON_RECORD_ACTION_3;
				break;
			case CharacterHelper.SortType.EXPLORE:
				result = OrangeConst.WEAPON_RECORD_EXPLORE_3;
				break;
			}
			break;
		case 4:
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				result = OrangeConst.WEAPON_RECORD_BATTLE_4;
				break;
			case CharacterHelper.SortType.ACTION:
				result = OrangeConst.WEAPON_RECORD_ACTION_4;
				break;
			case CharacterHelper.SortType.EXPLORE:
				result = OrangeConst.WEAPON_RECORD_EXPLORE_4;
				break;
			}
			break;
		case 5:
			switch (sortType)
			{
			case CharacterHelper.SortType.BATTLE:
				result = OrangeConst.WEAPON_RECORD_BATTLE_5;
				break;
			case CharacterHelper.SortType.ACTION:
				result = OrangeConst.WEAPON_RECORD_ACTION_5;
				break;
			case CharacterHelper.SortType.EXPLORE:
				result = OrangeConst.WEAPON_RECORD_EXPLORE_5;
				break;
			}
			break;
		}
		return result;
	}

	public static CharacterHelper.SortType GetLastSortType()
	{
		return (CharacterHelper.SortType)MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastRecordSortType;
	}

	public static void SetLastSortType(CharacterHelper.SortType p_sortType)
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastRecordSortType = (int)p_sortType;
	}

	public static string GetSpritesNameByLatticeType(RecordGridLatticeType recordGridLatticeType)
	{
		switch (recordGridLatticeType)
		{
		case RecordGridLatticeType.Battle:
			return "UI_RECORD_icon_02";
		case RecordGridLatticeType.Explore:
			return "UI_RECORD_icon_03";
		case RecordGridLatticeType.Ability:
			return "UI_RECORD_icon_05";
		case RecordGridLatticeType.Random:
			return "UI_RECORD_icon_01";
		case RecordGridLatticeType.Hold:
			return "UI_RECORD_icon_04";
		default:
			return string.Empty;
		}
	}

	public static string GetCellNameByLatticeType(RecordGridLatticeType recordGridLatticeType)
	{
		switch (recordGridLatticeType)
		{
		case RecordGridLatticeType.Battle:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_BATTLE_LATTICE");
		case RecordGridLatticeType.Explore:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_EXPLORE_LATTICE");
		case RecordGridLatticeType.Ability:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_ABILITY_LATTICE");
		case RecordGridLatticeType.Random:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_RANDOM_LATTICE");
		case RecordGridLatticeType.Hold:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_BOSS_LATTICE");
		default:
			return string.Empty;
		}
	}

	public static string SuccessRate(int SuggestVal, int NowVal, int extra = 0)
	{
		float num = (float)OrangeConst.RECORD_DATUM_VALUE / 100f;
		float num2 = (float)OrangeConst.RECORD_PROPORTION_VALUE / 100f;
		float num3 = (float)OrangeConst.RECORD_CORRECTION_VALUE / 100f;
		int value = Mathf.FloorToInt(100f - num * (float)SuggestVal + num2 * ((float)NowVal * num3 - (float)SuggestVal)) + extra;
		return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_SCREEN_SUCCESS", Mathf.Clamp(value, 0, 100).ToString());
	}

	public static int GetTeamVal(RECORDGRID_TABLE recordGridTable)
	{
		if (recordGridTable != null)
		{
			switch ((RecordGridLatticeType)(short)recordGridTable.n_TYPE)
			{
			default:
				return 0;
			case RecordGridLatticeType.Battle:
				return ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.BattlePoint;
			case RecordGridLatticeType.Explore:
				return ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.ExplorePoint;
			case RecordGridLatticeType.Hold:
				break;
			}
			switch ((HoldValType)recordGridTable.n_VALUE_X)
			{
			case HoldValType.Explore:
				return ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.ExplorePoint;
			case HoldValType.Battle:
				return ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.BattlePoint;
			case HoldValType.Total:
				return Mathf.FloorToInt((float)(ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.ExplorePoint + ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.BattlePoint) / 2f);
			}
		}
		return 0;
	}

	public static string GetAbilityTip(RECORDGRID_TABLE table)
	{
		switch ((RecordGridAbilityType)(short)table.n_VALUE_X)
		{
		case RecordGridAbilityType.Battle:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_SCREEN_BATTLE") + table.n_VALUE_Z.ToString("+#;-#;0");
		case RecordGridAbilityType.Explore:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_SCREEN_EXPLORE") + table.n_VALUE_Z.ToString("+#;-#;0");
		case RecordGridAbilityType.Action:
			return MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_SCREEN_ACTION") + table.n_VALUE_Z.ToString("+#;-#;0");
		default:
			return string.Empty;
		}
	}

	public static string GetRandomLatticeMsg(int id)
	{
		string result = string.Empty;
		RANDOMLATTICE_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.RANDOMLATTICE_TABLE_DICT.TryGetValue(id, out value))
		{
			switch (value.n_TYPE)
			{
			case 1:
				result = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(value.s_RANDOM_TIP), value.n_VALUE_X);
				break;
			case 2:
				result = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(value.s_RANDOM_TIP), value.n_VALUE_X);
				break;
			case 3:
				result = ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(value.s_RANDOM_TIP);
				break;
			case 4:
				result = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(value.s_RANDOM_TIP), value.n_VALUE_X.ToString("+#;-#;0"), value.n_VALUE_Y.ToString("+#;-#;0"));
				break;
			case 5:
				result = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(value.s_RANDOM_TIP), value.n_VALUE_X, value.n_VALUE_Y.ToString("+#;-#;0"));
				break;
			case 6:
				result = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(value.s_RANDOM_TIP), value.n_VALUE_X.ToString("+#;-#;0"), value.n_VALUE_Y.ToString("+#;-#;0"));
				break;
			case 7:
				result = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(value.s_RANDOM_TIP), value.n_VALUE_X.ToString("+#;-#;0"), value.n_VALUE_Y.ToString("+#;-#;0"));
				break;
			case 8:
				result = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(value.s_RANDOM_TIP), value.n_VALUE_X.ToString("+#;-#;0"), value.n_VALUE_Y.ToString("+#;-#;0"));
				break;
			case 9:
				result = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(value.s_RANDOM_TIP), value.n_VALUE_X.ToString("+#;-#;0"), value.n_VALUE_Y.ToString("+#;-#;0"));
				break;
			case 10:
				result = string.Format(ManagedSingleton<OrangeTextDataManager>.Instance.RECORDTIP_TABLE_DICT.GetL10nValue(value.s_RANDOM_TIP), value.n_VALUE_X.ToString("+#;-#;0"), value.n_VALUE_Y.ToString("+#;-#;0"));
				break;
			}
		}
		return result;
	}

	public void RetrieveRecordGridInfoReq(bool playSE = true)
	{
		if (playSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
		}
		if (Status != 0 && !IsExpired && !IsNeedToRefreash())
		{
			OpenDeepRecordMainUI();
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveRecordGridInfoReq(delegate(RetrieveRecordGridInfoRes res)
		{
			Code code = (Code)res.Code;
			switch (code)
			{
			default:
				ForceReset();
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(code, false);
				break;
			case Code.RECORDGRID_PLAYER_STATUS_NOT_FOUND:
				Status = DeepRecordTeamSetUI.Status.Edit;
				PlayerInfo = res.PlayerInfo;
				OtherPlayerList = res.OtherPlayerList;
				MapInfo = res.MapInfo;
				OpenDeepRecordTeamSetUI();
				UpateApiRefreashTime();
				break;
			case Code.RECORDGRID_PLAYER_STATUS_RESET:
				Status = DeepRecordTeamSetUI.Status.Edit;
				ListMovePoint.Clear();
				PlayerInfo = res.PlayerInfo;
				OtherPlayerList = res.OtherPlayerList;
				MapInfo = res.MapInfo;
				OpenDeepRecordTeamSetUI();
				UpateApiRefreashTime();
				if ((bool)MainUI)
				{
					if (TurtorialUI.IsTutorialing())
					{
						TurtorialUI.ForceCloseTutorial();
						MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("EVENT_OUTDATE", delegate
						{
							MonoBehaviourSingleton<UIManager>.Instance.BackToHometop(true, true, delegate
							{
								ForceReset();
							});
						});
					}
					else
					{
						ClearLog();
						MainUI.OnClickCloseBtn();
					}
				}
				break;
			case Code.RECORDGRID_PLAYER_STATUS_ACTIVITY:
				Status = DeepRecordTeamSetUI.Status.View;
				PlayerInfo = res.PlayerInfo;
				OtherPlayerList = res.OtherPlayerList;
				MapInfo = res.MapInfo;
				OpenDeepRecordMainUI(true);
				UpateApiRefreashTime();
				break;
			}
		});
	}

	public void AddMovementInfo(int pX, int pY)
	{
		NetCommonCoordinateInfo netCommonCoordinateInfo = new NetCommonCoordinateInfo();
		netCommonCoordinateInfo.X = pX;
		netCommonCoordinateInfo.Y = pY;
		int num = ListMovePoint.FindIndex((NetCommonCoordinateInfo x) => x.X == pX && x.Y == pY);
		if (-1 != num)
		{
			ListMovePoint.RemoveRange(num, ListMovePoint.Count - num);
		}
		ListMovePoint.Add(netCommonCoordinateInfo);
	}

	public void SaveMovement()
	{
		if (ListMovePoint.Count <= 0)
		{
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.ChallengeRecordGridReq(ListMovePoint, delegate(ChallengeRecordGridRes res)
		{
			Code code = (Code)res.Code;
			if (ChallengeRecordGridReqValid(code))
			{
				ListMovePoint.Clear();
				NetRecordGridPlayerInfo playerInfo = res.PlayerInfo;
				PlayerInfo.CurrentPositionInfo = playerInfo.CurrentPositionInfo;
				for (int i = 0; i < res.OtherPlayerList.Count; i++)
				{
					NetRecordGridOtherPlayerInfo newOtherPlayerInfo = res.OtherPlayerList[i];
					NetRecordGridOtherPlayerInfo netRecordGridOtherPlayerInfo = OtherPlayerList.FirstOrDefault((NetRecordGridOtherPlayerInfo x) => x.PlayerID == newOtherPlayerInfo.PlayerID);
					if (netRecordGridOtherPlayerInfo != null)
					{
						netRecordGridOtherPlayerInfo.CharacterID = newOtherPlayerInfo.CharacterID;
						netRecordGridOtherPlayerInfo.CurrentPositionInfo = newOtherPlayerInfo.CurrentPositionInfo;
						netRecordGridOtherPlayerInfo.NickName = newOtherPlayerInfo.NickName;
						netRecordGridOtherPlayerInfo.Rank = newOtherPlayerInfo.Rank;
					}
					else
					{
						OtherPlayerList.Add(newOtherPlayerInfo);
					}
				}
			}
		});
	}

	public void ChallengeRecordGridReq(Callback<ChallengeRecordGridRes> p_cb)
	{
		ManagedSingleton<PlayerNetManager>.Instance.ChallengeRecordGridReq(ListMovePoint, delegate(ChallengeRecordGridRes res)
		{
			Code code = (Code)res.Code;
			if (!ChallengeRecordGridReqValid(code))
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(code);
			}
			else
			{
				ListMovePoint.Clear();
				NetRecordGridPlayerInfo playerInfo = res.PlayerInfo;
				PlayerInfo.CurrentPositionInfo = playerInfo.CurrentPositionInfo;
				PlayerInfo.ActionPoint = playerInfo.ActionPoint;
				PlayerInfo.BattlePoint = playerInfo.BattlePoint;
				PlayerInfo.ExplorePoint = playerInfo.ExplorePoint;
				PlayerInfo.ExtraSuccessRate = playerInfo.ExtraSuccessRate;
				PlayerInfo.Rank = playerInfo.Rank;
				foreach (NetCommonCoordinateInfo finishPosition in playerInfo.FinishPositionList)
				{
					PlayerInfo.FinishPositionList.Add(finishPosition);
					RefreashFinishPoint(finishPosition.X, finishPosition.Y);
				}
				for (int i = 0; i < res.OtherPlayerList.Count; i++)
				{
					NetRecordGridOtherPlayerInfo newOtherPlayerInfo = res.OtherPlayerList[i];
					NetRecordGridOtherPlayerInfo netRecordGridOtherPlayerInfo = OtherPlayerList.FirstOrDefault((NetRecordGridOtherPlayerInfo x) => x.PlayerID == newOtherPlayerInfo.PlayerID);
					if (netRecordGridOtherPlayerInfo != null)
					{
						netRecordGridOtherPlayerInfo.CharacterID = newOtherPlayerInfo.CharacterID;
						netRecordGridOtherPlayerInfo.CurrentPositionInfo = newOtherPlayerInfo.CurrentPositionInfo;
						netRecordGridOtherPlayerInfo.NickName = newOtherPlayerInfo.NickName;
						netRecordGridOtherPlayerInfo.Rank = newOtherPlayerInfo.Rank;
					}
					else
					{
						OtherPlayerList.Add(newOtherPlayerInfo);
					}
				}
				if (res.MapInfo.EventList.Count > 0)
				{
					foreach (NetRecordGridMapEventInfo e in res.MapInfo.EventList)
					{
						NetRecordGridMapEventInfo netRecordGridMapEventInfo = MapInfo.EventList.FirstOrDefault((NetRecordGridMapEventInfo x) => x.X == e.X && x.Y == e.Y);
						if (netRecordGridMapEventInfo != null)
						{
							netRecordGridMapEventInfo.PlayerID = e.PlayerID;
							netRecordGridMapEventInfo.Value = e.Value;
						}
						else
						{
							MapInfo.EventList.Add(e);
						}
					}
				}
				foreach (NetItemInfo item in res.ItemList)
				{
					ManagedSingleton<PlayerNetManager>.Instance.dicItem.Value(item.ItemID).netItemInfo = item;
				}
				PlayerMoveCount += 1;
				p_cb(res);
			}
		});
	}

	public void ChallengeMultiRecordGridReq(Callback<ChallengeMultiRecordGridRes> p_cb)
	{
		ManagedSingleton<PlayerNetManager>.Instance.ChallengeMultiRecordGridReq(ListMovePoint, delegate(ChallengeMultiRecordGridRes res)
		{
			Code code = (Code)res.Code;
			if (!ChallengeMultiRecordGridReqValid(code))
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(code);
			}
			else
			{
				NetRecordGridPlayerInfo playerInfo = res.PlayerInfo;
				PlayerInfo.CurrentPositionInfo = playerInfo.CurrentPositionInfo;
				PlayerInfo.ActionPoint = playerInfo.ActionPoint;
				PlayerInfo.BattlePoint = playerInfo.BattlePoint;
				PlayerInfo.ExplorePoint = playerInfo.ExplorePoint;
				PlayerInfo.ExtraSuccessRate = playerInfo.ExtraSuccessRate;
				PlayerInfo.Rank = playerInfo.Rank;
				foreach (NetCommonCoordinateInfo finishPosition in playerInfo.FinishPositionList)
				{
					PlayerInfo.FinishPositionList.Add(finishPosition);
					RefreashFinishPoint(finishPosition.X, finishPosition.Y);
				}
				for (int i = 0; i < res.OtherPlayerList.Count; i++)
				{
					NetRecordGridOtherPlayerInfo newOtherPlayerInfo = res.OtherPlayerList[i];
					NetRecordGridOtherPlayerInfo netRecordGridOtherPlayerInfo = OtherPlayerList.FirstOrDefault((NetRecordGridOtherPlayerInfo x) => x.PlayerID == newOtherPlayerInfo.PlayerID);
					if (netRecordGridOtherPlayerInfo != null)
					{
						netRecordGridOtherPlayerInfo.CharacterID = newOtherPlayerInfo.CharacterID;
						netRecordGridOtherPlayerInfo.CurrentPositionInfo = newOtherPlayerInfo.CurrentPositionInfo;
						netRecordGridOtherPlayerInfo.NickName = newOtherPlayerInfo.NickName;
						netRecordGridOtherPlayerInfo.Rank = newOtherPlayerInfo.Rank;
					}
					else
					{
						OtherPlayerList.Add(newOtherPlayerInfo);
					}
				}
				if (res.MapInfo.EventList.Count > 0)
				{
					foreach (NetRecordGridMapEventInfo e in res.MapInfo.EventList)
					{
						NetRecordGridMapEventInfo netRecordGridMapEventInfo = MapInfo.EventList.FirstOrDefault((NetRecordGridMapEventInfo x) => x.X == e.X && x.Y == e.Y);
						if (netRecordGridMapEventInfo != null)
						{
							netRecordGridMapEventInfo.PlayerID = e.PlayerID;
							netRecordGridMapEventInfo.Value = e.Value;
						}
						else
						{
							MapInfo.EventList.Add(e);
						}
					}
				}
				ListMultiMoveLog.Clear();
				if (res.RecordGridLog.Count > 0)
				{
					for (int num = res.RecordGridLog.Count - 1; num >= 0; num--)
					{
						ListMultiMoveLog.Insert(0, res.RecordGridLog[num]);
					}
				}
				PlayerMoveCount += ListMultiMoveLog.Count;
				RefreashFlag = GridRefreashEvent.All;
				foreach (NetItemInfo item in res.ItemList)
				{
					ManagedSingleton<PlayerNetManager>.Instance.dicItem.Value(item.ItemID).netItemInfo = item;
				}
				p_cb(res);
			}
		});
	}

	public void RetrieveRecordGridBattleLogReq(Callback p_cb)
	{
		if (!RefreashFlag.HasFlag(GridRefreashEvent.Battle))
		{
			p_cb.CheckTargetToInvoke();
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveRecordGridBattleLogReq(ListBattleLog.Count, delegate(RetrieveRecordGridBattleLogRes res)
		{
			RefreashFlag &= ~GridRefreashEvent.Battle;
			if (res.RecordGridLog.Count > 0)
			{
				for (int num = res.RecordGridLog.Count - 1; num >= 0; num--)
				{
					ListBattleLog.Insert(0, res.RecordGridLog[num]);
				}
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void UpateApiRefreashTime()
	{
		ApiRefreashTime = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerTimeNowUTC.AddSeconds(600.0);
	}

	public void RetrieveRecordGridAbilityLogReq(Callback p_cb)
	{
		if (!RefreashFlag.HasFlag(GridRefreashEvent.Ability))
		{
			p_cb.CheckTargetToInvoke();
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveRecordGridAbilityLogReq(ListAbilityLog.Count, delegate(RetrieveRecordGridAbilityLogRes res)
		{
			RefreashFlag &= ~GridRefreashEvent.Ability;
			if (res.RecordGridLog.Count > 0)
			{
				for (int num = res.RecordGridLog.Count - 1; num >= 0; num--)
				{
					ListAbilityLog.Insert(0, res.RecordGridLog[num]);
				}
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrieveRecordGridRandomLogReq(Callback p_cb)
	{
		if (!RefreashFlag.HasFlag(GridRefreashEvent.Random))
		{
			RefreashRandomLogCount();
			p_cb.CheckTargetToInvoke();
			return;
		}
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveRecordGridRandomLogReq(ListRandomLog.Count, delegate(RetrieveRecordGridRandomLogRes res)
		{
			RefreashRandomLogCount();
			RefreashFlag &= ~GridRefreashEvent.Random;
			if (res.RecordGridLog.Count > 0)
			{
				for (int num = res.RecordGridLog.Count - 1; num >= 0; num--)
				{
					ListRandomLog.Insert(0, res.RecordGridLog[num]);
				}
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	private void RefreashRandomLogCount()
	{
		if (PlayerMoveCount == 0)
		{
			return;
		}
		foreach (NetRecordGridRandomLogInfo item in ListRandomLog)
		{
			item.Count = Mathf.Clamp(item.Count - PlayerMoveCount, 0, int.MaxValue);
		}
		PlayerMoveCount = 0;
	}

	private bool ChallengeRecordGridReqValid(Code code)
	{
		if ((uint)(code - 110200) <= 2u)
		{
			return true;
		}
		return false;
	}

	private bool ChallengeMultiRecordGridReqValid(Code code)
	{
		switch ((int)code)
		{
		case 200:
			return true;
		case 110204:
			return false;
		default:
			return false;
		}
	}

	public NetRecordGridCoordinateInfo GetCoordinteInfo(int x, int z)
	{
		return MapInfo.GridPositionList.FirstOrDefault((NetRecordGridCoordinateInfo p) => p.X == x && p.Y == z);
	}

	public void InitFinishPoint(int x, int y)
	{
		if (mapWidth == x && mapHeight == y && FinishPoints != null)
		{
			return;
		}
		mapWidth = x;
		mapHeight = y;
		FinishPoints = new bool[x, y];
		for (int i = 0; i < x; i++)
		{
			for (int j = 0; j < y; j++)
			{
				FinishPoints[i, j] = false;
			}
		}
		foreach (NetCommonCoordinateInfo finishPosition in PlayerInfo.FinishPositionList)
		{
			RefreashFinishPoint(finishPosition.X, finishPosition.Y);
		}
	}

	public void RefreashFinishPoint(int x, int y)
	{
		FinishPoints[x, y] = true;
	}

	public bool IsPointFinished(int x, int y)
	{
		if (FinishPoints == null)
		{
			return false;
		}
		return FinishPoints[x, y];
	}

	public bool IsNeedToRefreash()
	{
		return ApiRefreashTime < MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerTimeNowUTC;
	}

	public void AddLogFlag(GridRefreashEvent flag)
	{
		RefreashFlag |= flag;
	}

	public void OpenDeepRecordMainUI(bool refreash = false)
	{
		if ((bool)MainUI)
		{
			if (refreash)
			{
				MainUI.Refreash();
			}
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DeepRecordMain", delegate(DeepRecordMainUI ui)
			{
				MainUI = ui;
				ui.Setup();
			});
		}
	}

	public void OpenDeepRecordTeamSetUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DeepRecordTeamSet", delegate(DeepRecordTeamSetUI ui)
		{
			ui.Setup();
		});
	}

	public void OpenRewardUI()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_EventReward", delegate(EventRewardDialog ui)
		{
			ui.DeepRecordSetup();
		});
	}

	public void OpenRuleUI()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonScrollMsg", delegate(CommonScrollMsgUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_TIP"));
		});
	}
}
