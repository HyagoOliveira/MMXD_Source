using UnityEngine;
using UnityEngine.UI;

public class GuildApplyPlayerCell : ScrollIndexCallback
{
	[SerializeField]
	private Text _playerLevel;

	[SerializeField]
	private GameObject _playerIconRoot;

	[SerializeField]
	private Text _playerName;

	[SerializeField]
	private Text _playerApplyMsg;

	[SerializeField]
	private OnlineStatusHelper _onlineStatus;

	private int _idx;

	private string _playerId;

	private GuildApplyPlayerListChildUI _parentUI;

	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<GuildApplyPlayerListChildUI>();
		}
		NetPlayerJoinMessageInfo netPlayerJoinMessageInfo = Singleton<GuildSystem>.Instance.ApplyPlayerListCache[p_idx];
		_playerId = netPlayerJoinMessageInfo.PlayerID;
		GuildUIHelper.SetPlayerHUDData(_playerId, _playerName, _playerLevel, _playerIconRoot);
		_playerApplyMsg.text = netPlayerJoinMessageInfo.Message;
		GuildUIHelper.SetOnlineStatus(_playerId, _onlineStatus);
	}

	public void OnClickPlayerIconBtn()
	{
		if (!string.IsNullOrEmpty(_playerId))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PlayerInfoMain", delegate(PlayerInfoMainUI ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(_playerId);
			});
		}
	}

	public void OnClickAgreeBtn()
	{
		GuildApplyPlayerListChildUI parentUI = _parentUI;
		if ((object)parentUI != null)
		{
			parentUI.OnClickOneAgreeBtn(_playerId);
		}
	}

	public void OnClickRefuseBtn()
	{
		GuildApplyPlayerListChildUI parentUI = _parentUI;
		if ((object)parentUI != null)
		{
			parentUI.OnClickOneRefuseBtn(_playerId);
		}
	}
}
