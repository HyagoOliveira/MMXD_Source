using UnityEngine;
using UnityEngine.UI;

public class GuildInvitePlayerCell : GuildInvitePlayerCellBase
{
	[SerializeField]
	private Text _textInviteMsg;

	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
		NetPlayerJoinMessageInfo netPlayerJoinMessageInfo = Singleton<GuildSystem>.Instance.InvitePlayerListCache[p_idx];
		_playerId = netPlayerJoinMessageInfo.PlayerID;
		GuildUIHelper.SetPlayerHUDData(_playerId, _playerName, _playerLevel, _playerIconRoot);
		_textInviteMsg.text = netPlayerJoinMessageInfo.Message;
	}

	public void OnClickCancelInviteBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CANCEL);
		Singleton<GuildSystem>.Instance.ReqCancelInvitePlayer(_playerId);
	}
}
