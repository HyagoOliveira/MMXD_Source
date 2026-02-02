public class WantedMemberSelectCellHelper : WantedMemberSelectCellHelperBase
{
	private WantedMemberSelectUI _parentUI;

	private int _idx;

	public override void ScrollCellIndex(int idx)
	{
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<WantedMemberSelectUI>();
		}
		_idx = idx;
		_playerIcon.gameObject.SetActive(false);
		RefreshCell();
	}

	public override void RefreshCell()
	{
		NetCharacterInfo characterInfo = Singleton<WantedSystem>.Instance.SortedCharacterInfoList[_idx].CharacterInfo;
		int num = _parentUI.SelectedCharacterListCache.FindIndex((NetCharacterInfo info) => info.CharacterID == characterInfo.CharacterID);
		if (num >= 0)
		{
			_goSelectBorder.SetActive(true);
			_imageSelectIndex.gameObject.SetActive(true);
			_imageSelectIndex.ChangeImage(num + 1);
		}
		else
		{
			_goSelectBorder.SetActive(false);
			_imageSelectIndex.gameObject.SetActive(false);
		}
		bool flag = Singleton<WantedSystem>.Instance.DeparturedCharacterCacheList.Contains(characterInfo.CharacterID);
		_goTipDepartured.SetActive(flag);
		_commonIcon.SetupWanted(_idx, characterInfo, OnClickIcon, !flag && (_parentUI.SelectedCharacterListCache.Count < _parentUI.MemberSelectLimit || num >= 0));
	}

	private void OnClickIcon(int idx)
	{
		if (!_commonIcon.IsEnabled)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_WANTED_WARN_6"));
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		NetCharacterInfo characterInfo = Singleton<WantedSystem>.Instance.SortedCharacterInfoList[idx].CharacterInfo;
		_parentUI.OnClickCellButton(characterInfo);
	}
}
