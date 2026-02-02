public class GuildEddieRewardListCell : ScrollIndexCallback
{
	private RewardListCellHelper _rewardListCellHelper;

	private GuildEddieRewardListUI _parentUI;

	public override void ScrollCellIndex(int p_idx)
	{
		if (_parentUI == null)
		{
			_parentUI = GetComponentInParent<GuildEddieRewardListUI>();
		}
		if (_rewardListCellHelper == null)
		{
			_rewardListCellHelper = GetComponent<RewardListCellHelper>();
		}
		BOXGACHACONTENT_TABLE bOXGACHACONTENT_TABLE = _parentUI.RewardListCache[p_idx];
		int n_REWARD_ID = bOXGACHACONTENT_TABLE.n_REWARD_ID;
		int n_AMOUNT_MIN = bOXGACHACONTENT_TABLE.n_AMOUNT_MIN;
		int n_AMOUNT_MAX = bOXGACHACONTENT_TABLE.n_AMOUNT_MAX;
		string itemSet = string.Format("{0}/{1}", bOXGACHACONTENT_TABLE.n_TOTAL, bOXGACHACONTENT_TABLE.n_TOTAL);
		_rewardListCellHelper.Setup(n_REWARD_ID, n_AMOUNT_MIN, n_AMOUNT_MAX, 1, 2, itemSet);
	}
}
