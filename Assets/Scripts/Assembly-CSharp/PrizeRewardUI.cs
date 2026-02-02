using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrizeRewardUI : OrangeUIBase
{
	[Serializable]
	private class PrizeRewadInfo
	{
		public string LocalizationKey;

		public Color Color;

		public Sprite BgSprite1;

		public Sprite BgSprite2;
	}

	[SerializeField]
	private PrizeRewadInfo[] prizeRewadInfo;

	[SerializeField]
	private PrizeRewardUIUnit prizeRewardUnit;

	[SerializeField]
	private ScrollRect scrollRect;

	private RectTransform content;

	protected override void Awake()
	{
		base.Awake();
		content = scrollRect.content;
	}

	public void Setup(ref List<GACHA_TABLE> listReward)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		if (this.prizeRewadInfo.Length == listReward.Count)
		{
			for (int i = 0; i < this.prizeRewadInfo.Length; i++)
			{
				PrizeRewardUIUnit prizeRewardUIUnit = UnityEngine.Object.Instantiate(prizeRewardUnit, content, false);
				PrizeRewadInfo prizeRewadInfo = this.prizeRewadInfo[i];
				GACHA_TABLE gACHA_TABLE = listReward[i];
				prizeRewardUIUnit.Setup(rewardInfo: new NetRewardInfo
				{
					RewardType = (sbyte)gACHA_TABLE.n_TYPE,
					RewardID = gACHA_TABLE.n_REWARD_ID,
					Amount = gACHA_TABLE.n_AMOUNT_MIN
				}, sprite0: prizeRewadInfo.BgSprite1, sprite1: prizeRewadInfo.BgSprite2, titleKey: prizeRewadInfo.LocalizationKey, colorTitle: prizeRewadInfo.Color);
			}
		}
	}
}
