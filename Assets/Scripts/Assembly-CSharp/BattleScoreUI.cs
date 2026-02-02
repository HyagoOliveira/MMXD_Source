using System;
using UnityEngine;
using UnityEngine.UI;

public class BattleScoreUI : ScoreUIBase
{
	public Text ScoreText;

	public bool bAddBattleScore;

	public bool bAddCampaignScore;

	[Tooltip("是否鎖在最大值不因減分變小.")]
	public bool bLockMaxValue;

	[NonSerialized]
	private int tmpTotal;

	[NonSerialized]
	private int nowScore;

	[NonSerialized]
	private int nBaseScore;

	public override void Init()
	{
		ScoreText.text = "0";
		tmpTotal = (nowScore = (nBaseScore = 0));
	}

	public override void SetNParam0(int nSet)
	{
		nBaseScore = nSet;
		nowScore = nSet;
	}

	public virtual void LateUpdate()
	{
		if (!(BattleInfoUI.Instance != null))
		{
			return;
		}
		tmpTotal = 0;
		if (bAddBattleScore)
		{
			tmpTotal += BattleInfoUI.Instance.nBattleScore;
		}
		if (bAddCampaignScore)
		{
			tmpTotal += BattleInfoUI.Instance.nGetCampaignScore;
		}
		if (nowScore < tmpTotal)
		{
			if (tmpTotal - nowScore >= 10)
			{
				nowScore += (int)((float)(tmpTotal - nowScore) * 0.1f);
			}
			else
			{
				nowScore = tmpTotal;
			}
			ScoreText.text = (nowScore - nBaseScore).ToString();
		}
		else if (!bLockMaxValue && nowScore > tmpTotal)
		{
			if (nowScore - tmpTotal >= 10)
			{
				nowScore += (int)((float)(tmpTotal - nowScore) * 0.1f);
			}
			else
			{
				nowScore = tmpTotal;
			}
			ScoreText.text = (nowScore - nBaseScore).ToString();
		}
	}
}
