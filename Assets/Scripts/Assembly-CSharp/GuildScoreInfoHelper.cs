using UnityEngine;
using UnityEngine.UI;

public class GuildScoreInfoHelper : MonoBehaviour
{
	[SerializeField]
	private Image _imageGuildScore;

	[SerializeField]
	private Text _textGuildScore;

	public void Setup(NetGuildInfo guildInfo, GuildSetting guildSetting)
	{
		_imageGuildScore.fillAmount = ((guildSetting.RankupScore > 0) ? ((float)guildInfo.Score / (float)guildSetting.RankupScore) : 0f);
		_textGuildScore.text = ((guildSetting.RankupScore > 0) ? string.Format("{0}/{1}", guildInfo.Score, guildSetting.RankupScore) : "--/--");
	}
}
