public class GuildInviteSearchPlayerCell : GuildInvitePlayerCellBase
{
	private GuildInviteSearchListUI _parentUI;

	private void Awake()
	{
		_parentUI = GetComponentInParent<GuildInviteSearchListUI>();
	}

	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
		_playerId = _parentUI.TargetPlayerId;
		GuildUIHelper.SetPlayerHUDData(_playerId, _playerName, _playerLevel, _playerIconRoot);
	}

	public void OnClickInviteBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GuildInviteConfirm", delegate(GuildInviteConfirmUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.OnInviteConfirmEvent += delegate
			{
				_parentUI.CloseSE = SystemSE.NONE;
			};
			ui.OnInviteConfirmEvent += _parentUI.OnClickCloseBtn;
			ui.Setup(_playerId);
		});
	}
}
