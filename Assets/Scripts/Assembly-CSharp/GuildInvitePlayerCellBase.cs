using UnityEngine;
using UnityEngine.UI;

public class GuildInvitePlayerCellBase : ScrollIndexCallback
{
	[SerializeField]
	protected Text _playerLevel;

	[SerializeField]
	protected GameObject _playerIconRoot;

	[SerializeField]
	protected Text _playerName;

	[SerializeField]
	protected GameObject _onlineIcon;

	[SerializeField]
	protected GameObject _offlineIcon;

	[SerializeField]
	protected Text _status;

	protected int _idx;

	[HideInInspector]
	protected string _playerId;

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
}
