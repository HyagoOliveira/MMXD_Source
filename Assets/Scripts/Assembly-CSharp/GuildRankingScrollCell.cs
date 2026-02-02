using UnityEngine;
using UnityEngine.UI;

public class GuildRankingScrollCell : ScrollIndexCallback
{
	[SerializeField]
	private ImageSpriteSwitcher _bgSwitcher;

	[SerializeField]
	private ImageSpriteSwitcher _rankSwitcher;

	[SerializeField]
	private Text _textRank;

	[SerializeField]
	private CommonGuildBadge _guildBadge;

	[SerializeField]
	private Text _textGuildName;

	[SerializeField]
	private Text _textScore;

	[SerializeField]
	private Image _imageSelection;

	private bool _isSelected;

	private RankingMainUI _parentUI;

	private bool _isScrollCell;

	public NetGuildInfo GuildInfoCache { get; private set; }

	public int Rank { get; private set; }

	public int Index { get; private set; }

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			_imageSelection.enabled = _isSelected;
		}
	}

	public override void ScrollCellIndex(int idx)
	{
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<RankingMainUI>();
		}
		_isScrollCell = true;
		base.gameObject.name = string.Format("GuildRank{0}", idx);
		RankingMainUI.GuildRankingData guildRankingData = _parentUI.GuildRankingDataCache[idx];
		Setup(guildRankingData.GuildInfo, idx, guildRankingData.Rank, guildRankingData.Score, _parentUI.CurrectTouchIndex == idx);
	}

	public void Reset()
	{
		Setup(null, -1, -1, 0L);
	}

	public void Setup(NetGuildInfo guildInfo, int idx, int rank = -1, long score = 0L, bool isSelected = false)
	{
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<RankingMainUI>();
		}
		Index = idx;
		GuildInfoCache = guildInfo;
		Rank = rank;
		IsSelected = isSelected;
		if (rank < 1)
		{
			_bgSwitcher.ChangeImage(3);
			_rankSwitcher.gameObject.SetActive(false);
			_textRank.gameObject.SetActive(true);
			_textRank.text = "----";
		}
		else if (rank >= 4)
		{
			_bgSwitcher.ChangeImage(3);
			_rankSwitcher.gameObject.SetActive(false);
			_textRank.gameObject.SetActive(true);
			_textRank.text = rank.ToString();
		}
		else
		{
			_bgSwitcher.ChangeImage(rank - 1);
			_textRank.gameObject.SetActive(false);
			_rankSwitcher.gameObject.SetActive(true);
			_rankSwitcher.ChangeImage(rank - 1);
		}
		if (guildInfo != null)
		{
			_guildBadge.gameObject.SetActive(true);
			_guildBadge.SetBadgeIndex(guildInfo.Badge);
			_textGuildName.text = guildInfo.GuildName;
		}
		else
		{
			_guildBadge.gameObject.SetActive(false);
			_textGuildName.text = "----";
		}
		if (score >= 0)
		{
			_textScore.text = score.ToString();
		}
		else
		{
			_textScore.text = "---";
		}
	}

	public void OnClickCell()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
		_parentUI.SelectGuildRankingCell(Index, _isScrollCell);
	}

	public void SelectGuildRankingCell()
	{
		_parentUI.SelectGuildRankingCell(Index, _isScrollCell);
	}
}
