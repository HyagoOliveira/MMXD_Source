using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class GuildRankInfoHelper : MonoBehaviour
{
	[SerializeField]
	private RectTransform _rectPanel;

	public float BG_HEIGHT_WITH_RANKUP = 342f;

	public float BG_HEIGHT_WITHOUT_RANKUP = 262f;

	[SerializeField]
	private ImageSpriteSwitcher _rankImage;

	[SerializeField]
	private GuildScoreInfoHelper _scoreInfoHelper;

	[SerializeField]
	private Button _buttonRankup;

	[SerializeField]
	private UIShiny _buttonRankupShiny;

	[SerializeField]
	private GameObject _goRankupRedDot;

	[SerializeField]
	private GameObject _goRankupMask;

	private NetGuildInfo _guildInfo;

	private GuildSetting _guildSetting;

	private GuildSetting _guildSettingNext;

	public void Setup(NetGuildInfo guildInfo, GuildSetting guildSetting, bool hasRankupButton)
	{
		_guildInfo = guildInfo;
		_guildSetting = guildSetting;
		_rankImage.ChangeImage(guildInfo.Rank - 1);
		_scoreInfoHelper.Setup(guildInfo, guildSetting);
		_buttonRankup.gameObject.SetActive(hasRankupButton);
		_rectPanel.sizeDelta = new Vector2(_rectPanel.sizeDelta.x, hasRankupButton ? BG_HEIGHT_WITH_RANKUP : BG_HEIGHT_WITHOUT_RANKUP);
		if (hasRankupButton)
		{
			GuildSetting guildSetting2;
			if (GuildSetting.TryGetSettingByGuildRank(guildInfo.Rank + 1, out guildSetting2))
			{
				_guildSettingNext = guildSetting2;
				_buttonRankup.interactable = true;
				_buttonRankupShiny.enabled = true;
				_goRankupRedDot.SetActive(guildInfo.Score >= guildSetting.RankupScore && guildInfo.Money >= guildSetting.RankupMoney);
				_goRankupMask.SetActive(false);
			}
			else
			{
				_guildSettingNext = null;
				_buttonRankup.interactable = false;
				_buttonRankupShiny.enabled = false;
				_goRankupRedDot.SetActive(false);
				_goRankupMask.SetActive(true);
			}
		}
	}

	public void OnClickRankupButton()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GuildRankupConfirmUI>("UI_GuildRankupConfirm", OnRankupConfirmUILoaded);
	}

	private void OnRankupConfirmUILoaded(GuildRankupConfirmUI ui)
	{
		string guildRankString = Singleton<GuildSystem>.Instance.GetGuildRankString(_guildInfo.Rank + 1);
		ui.CloseSE = SystemSE.NONE;
		ui.Setup(_guildInfo.Rank, _guildInfo.Rank + 1, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GUILD_LEVELUP_CAPTION", guildRankString), _guildInfo.Score, _guildInfo.Money, _guildSetting.RankupScore, _guildSetting.RankupMoney, OnConfirmRankup);
	}

	private void OnConfirmRankup()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		Singleton<GuildSystem>.Instance.ReqRankupGuild();
	}
}
