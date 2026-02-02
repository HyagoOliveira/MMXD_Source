using UnityEngine;

public class WantedRewardInfoHelper : OrangeChildUIBase
{
	[SerializeField]
	private WantedRewardInfoUnitHelper[] _rewardInfoUnitHelpers;

	public void Setup(WANTED_TABLE wantedAttrData, bool isReceived = false)
	{
		_rewardInfoUnitHelpers[0].Setup(wantedAttrData.n_REWRD_1, wantedAttrData.n_NUMBER_1, isReceived);
		_rewardInfoUnitHelpers[1].Setup(wantedAttrData.n_REWRD_2, wantedAttrData.n_NUMBER_2, isReceived);
		_rewardInfoUnitHelpers[2].Setup(wantedAttrData.n_REWRD_3, wantedAttrData.n_NUMBER_3, isReceived);
	}
}
