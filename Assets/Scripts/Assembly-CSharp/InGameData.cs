using System.Collections.Generic;

public class InGameData
{
	public int BattlePowerMax;

	public int LastStageID = 1;

	public int LastStageDifficulty = 1;

	public int HowToGetStageID;

	public int HowToGetStageDifficulty;

	public int CharacterSortType;

	public int CharacterSortStatus = 2;

	public bool CharacterSortDescend = true;

	public int WeaponSortType = 255;

	public int WeaponSortKey = 1;

	public int WeaponSortStatus = 4;

	public bool WeaponSortDescend = true;

	public int ChipSortType = 255;

	public int ChipSortKey = 1;

	public int ChipGetKey = 4;

	public bool ChipSortDescend = true;

	public int RecordSortType = 3;

	public bool RecordMoveChk = true;

	public bool RepeatScenario;

	public int CardSortStatus = 255;

	public int CardSortType = 255;

	public int CardMainSortType = 255;

	public int CardInfoSortType = 255;

	public int CardSortKey = 1;

	public int CardDeploySortKey = 1;

	public int CardMainSortKey = 1;

	public int CardInfoSortKey = 1;

	public bool CardSortDescend = true;

	public bool CardDeploySortDescend = true;

	public bool CardMainSortDescend = true;

	public bool CardInfoSortDescend = true;

	public int CardResetSortType = 255;

	public int CardResetSortKey = 1;

	public HashSet<int> hashSignShowNewHint = new HashSet<int>();

	public int SignSortStatus = 4;

	public int SignSortKey = 4;

	public bool SignSortDescend = true;

	public string TutorialSave = string.Empty;

	public string RecoveryNetGameData = string.Empty;

	public Setting Setting = new Setting();

	public Dictionary<int, int> DicLoginBonusCounter = new Dictionary<int, int>();

	public Dictionary<string, IAPReceipt> DicNewReceipt = new Dictionary<string, IAPReceipt>();

	public Dictionary<int, int> DicFriendDisplay = new Dictionary<int, int>
	{
		{ 0, 0 },
		{ 1, 0 },
		{ 2, 0 }
	};

	public int FriendChatShowHintTime;

	public Dictionary<int, int> DicSeasonPrepareCharacterIDs = new Dictionary<int, int>
	{
		{ 0, 1 },
		{ 1, 0 },
		{ 2, 0 }
	};

	public Dictionary<int, int> DicEventStageDifficulties = new Dictionary<int, int>();

	public HashSet<int> HashBossSweepSelection = new HashSet<int>();

	public InGameData()
	{
		BattlePowerMax = 0;
		Setting = new Setting();
		DicLoginBonusCounter = new Dictionary<int, int>();
	}
}
