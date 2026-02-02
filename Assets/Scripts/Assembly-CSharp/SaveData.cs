using System.Collections.Generic;
using Newtonsoft.Json;

public class SaveData
{
	public string SelectWorld = string.Empty;

	public string Locate = string.Empty;

	public string Birth = string.Empty;

	public int PatchVer;

	public string UID = string.Empty;

	public string CurrentPlayerID = OrangePlayerLocalData.DefaultPlayerID;

	public int LastChoseServiceZoneID = -1;

	public long AssetsDate;

	public string InheritCode = string.Empty;

	public bool IAPInProgress;

	public Dictionary<string, InGameData> DictInGameData = new Dictionary<string, InGameData>();

	[JsonIgnore]
	public Setting DefaultSetting = new Setting();

	public long RatingExpiredTime;

	public int BattlePowerMax
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 0;
			}
			return inGameData.BattlePowerMax;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).BattlePowerMax = value;
		}
	}

	public int LastSelectedStageID
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 1;
			}
			return inGameData.LastStageID;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).LastStageID = value;
		}
	}

	public int LastSelectedStageDifficulty
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 1;
			}
			return inGameData.LastStageDifficulty;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).LastStageDifficulty = value;
		}
	}

	public int HowToGetStageID
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 0;
			}
			return inGameData.HowToGetStageID;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).HowToGetStageID = value;
		}
	}

	public int HowToGetStageDifficulty
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 0;
			}
			return inGameData.HowToGetStageDifficulty;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).HowToGetStageDifficulty = value;
		}
	}

	public int LastCharacterUISortType
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 0;
			}
			return inGameData.CharacterSortType;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CharacterSortType = value;
		}
	}

	public int LastCharacterUISortStatus
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 2;
			}
			return inGameData.CharacterSortStatus;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CharacterSortStatus = value;
		}
	}

	public bool LastCharacterUISortDescend
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return true;
			}
			return inGameData.CharacterSortDescend;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CharacterSortDescend = value;
		}
	}

	public int LastWeaponSortType
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 255;
			}
			return inGameData.WeaponSortType;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).WeaponSortType = value;
		}
	}

	public int LastWeaponSortKey
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 1;
			}
			return inGameData.WeaponSortKey;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).WeaponSortKey = value;
		}
	}

	public int LastWeaponSortStatus
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 4;
			}
			return inGameData.WeaponSortStatus;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).WeaponSortStatus = value;
		}
	}

	public bool LastWeaponSortDescend
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return true;
			}
			return inGameData.WeaponSortDescend;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).WeaponSortDescend = value;
		}
	}

	public int LastChipSortType
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 255;
			}
			return inGameData.ChipSortType;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).ChipSortType = value;
		}
	}

	public int LastChipSortKey
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 1;
			}
			return inGameData.ChipSortKey;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).ChipSortKey = value;
		}
	}

	public int LastChipGetKey
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 4;
			}
			return inGameData.ChipGetKey;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).ChipGetKey = value;
		}
	}

	public bool LastChipSortDescend
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return true;
			}
			return inGameData.ChipSortDescend;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).ChipSortDescend = value;
		}
	}

	public int LastRecordSortType
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 3;
			}
			return inGameData.RecordSortType;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).RecordSortType = value;
		}
	}

	public bool LastRecordMoveChk
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return true;
			}
			return inGameData.RecordMoveChk;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).RecordMoveChk = value;
		}
	}

	public bool LastRepeatScenario
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return false;
			}
			return inGameData.RepeatScenario;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).RepeatScenario = value;
		}
	}

	public int LastCardSortType
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 255;
			}
			return inGameData.CardSortType;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardSortType = value;
		}
	}

	public int LastCardMainSortType
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 255;
			}
			return inGameData.CardMainSortType;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardMainSortType = value;
		}
	}

	public int LastCardInfoSortType
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 255;
			}
			return inGameData.CardMainSortType;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardInfoSortType = value;
		}
	}

	public int LastCardSortKey
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 1;
			}
			return inGameData.CardSortKey;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardSortKey = value;
		}
	}

	public int LastCardDeploySortKey
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 1;
			}
			return inGameData.CardDeploySortKey;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardDeploySortKey = value;
		}
	}

	public int LastCardMainSortKey
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 1;
			}
			return inGameData.CardMainSortKey;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardMainSortKey = value;
		}
	}

	public int LastCardInfoSortKey
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 1;
			}
			return inGameData.CardInfoSortKey;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardInfoSortKey = value;
		}
	}

	public bool LastCardSortDescend
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return true;
			}
			return inGameData.CardSortDescend;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardSortDescend = value;
		}
	}

	public bool LastCardDeploySortDescend
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return true;
			}
			return inGameData.CardDeploySortDescend;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardDeploySortDescend = value;
		}
	}

	public bool LastCardMainSortDescend
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return true;
			}
			return inGameData.CardMainSortDescend;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardMainSortDescend = value;
		}
	}

	public bool LastCardInfoSortDescend
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return true;
			}
			return inGameData.CardInfoSortDescend;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardInfoSortDescend = value;
		}
	}

	public int LastCardResetSortType
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 255;
			}
			return inGameData.CardResetSortType;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardResetSortType = value;
		}
	}

	public int LastCardResetSortKey
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 1;
			}
			return inGameData.CardResetSortKey;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).CardResetSortKey = value;
		}
	}

	public HashSet<int> HashSignShowNewHint
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return new HashSet<int>();
			}
			return inGameData.hashSignShowNewHint;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).hashSignShowNewHint = value;
		}
	}

	public int LastSignSortKey
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 4;
			}
			return inGameData.SignSortKey;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).SignSortKey = value;
		}
	}

	public int LastSignSortStatus
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 4;
			}
			return inGameData.SignSortStatus;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).SignSortStatus = value;
		}
	}

	public bool LastSignSortDescend
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return true;
			}
			return inGameData.SignSortDescend;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).SignSortDescend = value;
		}
	}

	public Setting Setting
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return DefaultSetting;
			}
			return inGameData.Setting;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).Setting = value;
		}
	}

	public Dictionary<int, int> DicLoginBonusCounter
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return null;
			}
			return inGameData.DicLoginBonusCounter;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).DicLoginBonusCounter = value;
		}
	}

	public Dictionary<string, IAPReceipt> DicNewReceipt
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return null;
			}
			return inGameData.DicNewReceipt;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).DicNewReceipt = value;
		}
	}

	public Dictionary<int, int> DicFriendDisplay
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return null;
			}
			return inGameData.DicFriendDisplay;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).DicFriendDisplay = value;
		}
	}

	public int FriendChatShowHintTime
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return 0;
			}
			return inGameData.FriendChatShowHintTime;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).FriendChatShowHintTime = value;
		}
	}

	public Dictionary<int, int> DicSeasonPrepareCharacterIDs
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return null;
			}
			return inGameData.DicSeasonPrepareCharacterIDs;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).DicSeasonPrepareCharacterIDs = value;
		}
	}

	public Dictionary<int, int> DicEventStageDifficulties
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return null;
			}
			return inGameData.DicEventStageDifficulties;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).DicEventStageDifficulties = value;
		}
	}

	public HashSet<int> HashBossSweepSelection
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return new HashSet<int>();
			}
			return inGameData.HashBossSweepSelection;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).HashBossSweepSelection = value;
		}
	}

	public string RecoveryNetGameData
	{
		get
		{
			InGameData inGameData = TryGetInGameData(CurrentPlayerID);
			if (inGameData == null)
			{
				return string.Empty;
			}
			return inGameData.RecoveryNetGameData;
		}
		set
		{
			DictInGameData.Value(CurrentPlayerID).RecoveryNetGameData = value;
		}
	}

	private InGameData TryGetInGameData(string PlayerID)
	{
		if (string.IsNullOrEmpty(PlayerID))
		{
			return null;
		}
		InGameData value = null;
		if (!DictInGameData.TryGetValue(PlayerID, out value))
		{
			value = new InGameData();
			DictInGameData.Add(PlayerID, value);
		}
		return value;
	}

	public bool IsNewAccount()
	{
		if (CurrentPlayerID == OrangePlayerLocalData.DefaultPlayerID && LastChoseServiceZoneID == -1)
		{
			return string.IsNullOrEmpty(Locate);
		}
		return false;
	}

	public string DisplayPlayerId()
	{
		if (CurrentPlayerID == OrangePlayerLocalData.DefaultPlayerID)
		{
			return string.Empty;
		}
		return "ID:" + CurrentPlayerID;
	}
}
