using Coffee.UIExtensions;
using UnityEngine.UI;

public class KillActivityUI : ScoreUIBase
{
	public Image KillScoreBar;

	public Text KillScoreText;

	public UIShiny KillActivityEft;

	private float fPercent;

	public override void Init()
	{
		KillActivityEft.Play();
		KillScoreBar.fillAmount = 0f;
		KillScoreText.text = "0%";
	}

	public virtual void LateUpdate()
	{
		if (BattleInfoUI.Instance != null)
		{
			fPercent = (float)BattleInfoUI.Instance.nGetCampaignScore / (float)BattleInfoUI.Instance.nCampaignTotalScore;
			if (fPercent > 1f)
			{
				fPercent = 1f;
			}
			KillScoreBar.fillAmount = fPercent;
			KillScoreText.text = (fPercent * 100f).ToString("0.0") + "%";
		}
	}
}
