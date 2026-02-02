using UnityEngine;
using UnityEngine.UI;

public class CommonPlayerInfoMiniUI : CommonPlayerFloatUIBase
{
	[SerializeField]
	private Text _textPlayerName;

	[SerializeField]
	private GameObject _playerIconRoot;

	public override void Setup(string playerId, Vector3 tarPos)
	{
		base.Setup(playerId, tarPos);
		SocketPlayerHUD playerHUD;
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerId, out playerHUD))
		{
			_textPlayerName.text = playerHUD.m_Name;
			CommonAssetHelper.LoadPlayerIcon(_playerIconRoot, delegate(PlayerIconBase playerIcon)
			{
				CommonUIHelper.SetPlayerIcon(playerIcon, playerHUD.m_IconNumber, 0.65f);
			});
		}
		else
		{
			_textPlayerName.text = "---";
		}
	}
}
