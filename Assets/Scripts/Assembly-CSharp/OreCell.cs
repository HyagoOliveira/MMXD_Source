using UnityEngine;
using UnityEngine.UI;

public class OreCell : OreCellBase
{
	[SerializeField]
	private Text _textStageInfo;

	[SerializeField]
	private Button _buttonLevelUp;

	[SerializeField]
	private GameObject _goLevelUpRedDot;

	[SerializeField]
	private GameObject _goLevelUpMask;

	[SerializeField]
	private Button _buttonLevelMax;

	private OreListChildUI _parentUI;

	private OreInfoData _oreInfoData;

	private bool _hasNextLevel;

	private bool _canLevelUp;

	public override void ScrollCellIndex(int p_idx)
	{
		base.ScrollCellIndex(p_idx);
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<OreListChildUI>();
		}
		_oreInfoData = _parentUI.OreInfoDataList[_idx];
		InitOreInfo(_oreInfoData.ItemID, _oreInfoData.MainSkillAttrData);
		_textStageInfo.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(_oreInfoData.StageInfo);
		NetGuildInfo guildInfoCache = Singleton<GuildSystem>.Instance.GuildInfoCache;
		bool hasNextLevel;
		bool canLevelUp;
		_oreInfoData.CheckLevelUpState(guildInfoCache, out hasNextLevel, out canLevelUp);
		_hasNextLevel = hasNextLevel;
		_canLevelUp = canLevelUp;
		if (_hasNextLevel)
		{
			_buttonLevelUp.gameObject.SetActive(true);
			_buttonLevelMax.gameObject.SetActive(false);
			if (_canLevelUp)
			{
				_goLevelUpMask.SetActive(false);
				_goLevelUpRedDot.SetActive(guildInfoCache.Money >= _oreInfoData.LevelUpMoney);
			}
			else
			{
				_goLevelUpMask.SetActive(true);
				_goLevelUpRedDot.SetActive(false);
			}
		}
		else
		{
			_buttonLevelUp.gameObject.SetActive(false);
			_buttonLevelMax.gameObject.SetActive(true);
			_buttonLevelMax.interactable = false;
		}
	}

	public void OnClickLevelUpBtn()
	{
		if (_hasNextLevel)
		{
			if (_canLevelUp)
			{
				_parentUI.OnClickOneLevelUpBtn(_oreInfoData);
				return;
			}
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_POWERTOWER_WARN_3", _oreInfoData.NextLevelInfoData.OpenLimitValue));
		}
	}
}
