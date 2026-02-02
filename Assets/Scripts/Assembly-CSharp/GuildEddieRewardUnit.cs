using UnityEngine;

public class GuildEddieRewardUnit : MonoBehaviour
{
	[SerializeField]
	private ImageSpriteSwitcher _eddieIconBGSwitcher;

	[SerializeField]
	private ImageSpriteSwitcher _eddieIconFrameSwitcher;

	[SerializeField]
	private ImageSpriteSwitcher _eddieIconSwitcher;

	[SerializeField]
	private GameObject _rewardIconRoot;

	[SerializeField]
	private GameObject _playerIconRoot;

	private CommonIconBase _rewardIconBase;

	private PlayerIconBase _playerIconBase;

	private string _playerId;

	public void Setup(NetEddieBoxGachaRecord boxGachaRecord)
	{
		_playerId = boxGachaRecord.PlayerID;
		BOXGACHACONTENT_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.BOXGACHACONTENT_TABLE_DICT.TryGetValue(boxGachaRecord.BoxGachaID, out value))
		{
			SetRewardIcon(_playerId, value.n_REWARD_ID, boxGachaRecord.Count);
		}
		_eddieIconBGSwitcher.gameObject.SetActive(false);
	}

	public void Reset(int boxLevel)
	{
		_eddieIconBGSwitcher.gameObject.SetActive(true);
		_eddieIconBGSwitcher.ChangeImage(boxLevel);
		_eddieIconFrameSwitcher.ChangeImage(boxLevel);
		_eddieIconSwitcher.ChangeImage(boxLevel);
		_rewardIconRoot.SetActive(false);
		_playerIconRoot.SetActive(false);
		_playerId = string.Empty;
	}

	public void OnClickRewardUnitButton()
	{
		if (!(_playerId == string.Empty))
		{
			Vector3 tarPos = GetComponent<RectTransform>().position;
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonPlayerInfoMini", delegate(CommonPlayerInfoMiniUI ui)
			{
				ui.Setup(_playerId, tarPos);
			});
		}
	}

	private void SetRewardIcon(string playerId, int rewardId, int rewardCount)
	{
		if (_rewardIconBase == null)
		{
			CommonAssetHelper.LoadCommonIconSmall(_rewardIconRoot, delegate(CommonIconBase commonIcon)
			{
				_rewardIconBase = commonIcon;
				CommonUIHelper.SetCommonIcon(_rewardIconBase, rewardId, rewardCount);
			});
		}
		else
		{
			CommonUIHelper.SetCommonIcon(_rewardIconBase, rewardId, rewardCount);
		}
		SocketPlayerHUD value;
		if (_playerIconBase == null)
		{
			CommonAssetHelper.LoadPlayerIcon(_playerIconRoot, delegate(PlayerIconBase playerIcon)
			{
				_playerIconBase = playerIcon;
				SocketPlayerHUD value2;
				if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerId, out value2))
				{
					CommonUIHelper.SetPlayerIcon(_playerIconBase, value2.m_IconNumber, 0.55f);
				}
			});
		}
		else if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerId, out value))
		{
			CommonUIHelper.SetPlayerIcon(_playerIconBase, value.m_IconNumber, 0.55f);
		}
		_rewardIconRoot.SetActive(true);
		_playerIconRoot.SetActive(true);
	}
}
