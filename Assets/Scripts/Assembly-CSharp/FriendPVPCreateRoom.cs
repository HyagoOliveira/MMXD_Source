using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using cm;
using enums;

public class FriendPVPCreateRoom : OrangeUIBase
{
	[SerializeField]
	private InputField inputRoomName;

	[SerializeField]
	private Toggle togglePublic;

	[SerializeField]
	private Toggle togglePrivate;

	[SerializeField]
	private Button buttonOK;

	[SerializeField]
	private GameObject controlBlock;

	public Callback createRoomCB;

	private STAGE_TABLE _stageTable;

	private bool bMute;

	private Toggle _currentToggle;

	protected override void Awake()
	{
		base.Awake();
		SetDefaultRoomName();
		buttonOK.interactable = false;
	}

	private void ToggleSE(Toggle nowToggle)
	{
		if (_currentToggle != nowToggle && !bMute)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		bMute = false;
		_currentToggle = nowToggle;
	}

	public void Setup(int stageID)
	{
		ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(stageID, out _stageTable);
		togglePublic.onValueChanged.AddListener(delegate
		{
			ToggleSE(togglePublic);
		});
		togglePrivate.onValueChanged.AddListener(delegate
		{
			ToggleSE(togglePrivate);
		});
		buttonOK.interactable = true;
		_currentToggle = togglePublic;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void OnClickBtnCreateRoom()
	{
		buttonOK.interactable = false;
		if ((bool)controlBlock)
		{
			controlBlock.SetActive(true);
		}
		if (inputRoomName.text.Length < 1)
		{
			SetDefaultRoomName();
		}
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		base.CloseSE = SystemSE.NONE;
		CreateRoom();
	}

	public static string CreateDefaultRoomName()
	{
		return string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ROOM_TITLE"), ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname, string.Empty);
	}

	private void SetDefaultRoomName()
	{
		inputRoomName.text = CreateDefaultRoomName();
	}

	private void CreateRoom()
	{
		int tier = 0;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpGameType = PVPGameType.OneVSOneBattle;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = PVPMatchType.FriendOneVSOne;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.CreatePVPPrepareRoom(MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType, MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID, tier, togglePublic.isOn, inputRoomName.text, false, OnRSCreatePVPPrepareRoom);
		});
	}

	private void OnRSCreatePVPPrepareRoom(object res)
	{
		if (!(res is RSCreatePVPPrepareRoom))
		{
			return;
		}
		RSCreatePVPPrepareRoom rs = (RSCreatePVPPrepareRoom)res;
		if (rs.Result != 61000)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)rs.Result, false);
			OnClickCloseBtn();
			return;
		}
		NetSealBattleSettingInfo netSealBattleSettingInfo = null;
		if (!ManagedSingleton<PlayerHelper>.Instance.ParserUnsealedBattleSetting(rs.Unsealedbattlesetting, out netSealBattleSettingInfo))
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(Code.MATCH_CREATEROOM_FAIL, false);
			OnClickCloseBtn();
			return;
		}
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = rs.Ip;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = rs.Port;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.SingleMatch = false;
		createRoomCB.CheckTargetToInvoke();
		createRoomCB = null;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_FriendPVPRoomMain", delegate(FriendPVPRoomMain friendPVPRoomMain)
		{
			friendPVPRoomMain.CloseSE = SystemSE.NONE;
			friendPVPRoomMain.Setup(true, rs.Roomid, rs.Invitecode, _stageTable);
			OnClickCloseBtn();
			GoCheckUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<GoCheckUI>("UI_GoCheck");
			if ((bool)uI)
			{
				uI.OnClickCloseBtn();
			}
		});
	}
}
