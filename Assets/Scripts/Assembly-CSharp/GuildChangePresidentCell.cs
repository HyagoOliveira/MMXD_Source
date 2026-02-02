using UnityEngine;
using UnityEngine.UI;

public class GuildChangePresidentCell : ScrollIndexCallback
{
	[SerializeField]
	private Text _playerLevel;

	[SerializeField]
	private GameObject _playerIconRoot;

	[SerializeField]
	private Text _playerName;

	[SerializeField]
	private GameObject _buttonSelect;

	[SerializeField]
	private GameObject _imageSelected;

	[SerializeField]
	private GameObject _imageCurrentPresident;

	private GuildChangePresidentListUI _parentUI;

	private int _idx;

	private string _playerId;

	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<GuildChangePresidentListUI>();
			_parentUI.OnItemSelected += RefreshButtonStatus;
		}
		NetMemberInfo netMemberInfo = Singleton<GuildSystem>.Instance.MemberInfoListCache[_idx];
		_playerId = netMemberInfo.MemberId;
		GuildUIHelper.SetPlayerHUDData(_playerId, _playerName, _playerLevel, _playerIconRoot);
		RefreshButtonStatus();
	}

	public void OnClickSelectBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
		_parentUI.OnClickCellSelectBtn(_playerId);
	}

	private void RefreshButtonStatus()
	{
		SetButtonStatus(_parentUI.SelectedPlayerId == _playerId);
	}

	private void SetButtonStatus(bool isSelected)
	{
		bool flag = _playerId == Singleton<GuildSystem>.Instance.GuildInfoCache.LeaderPlayerID;
		_imageCurrentPresident.SetActive(flag);
		_buttonSelect.SetActive(!flag && !isSelected);
		_imageSelected.SetActive(!flag && isSelected);
	}
}
