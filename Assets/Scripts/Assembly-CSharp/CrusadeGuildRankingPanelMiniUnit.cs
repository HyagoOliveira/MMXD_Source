using UnityEngine;
using UnityEngine.UI;

public class CrusadeGuildRankingPanelMiniUnit : MonoBehaviour
{
	[SerializeField]
	private ImageSpriteSwitcher _imageRank;

	[SerializeField]
	private Text _textRank;

	[SerializeField]
	private Text _guildName;

	[SerializeField]
	private CommonGuildBadge _guildBadge;

	public void Setup(NetGuildInfo guildInfo, int ranking, long score)
	{
		if (ranking <= 3)
		{
			_imageRank.gameObject.SetActive(true);
			_imageRank.ChangeImage(ranking - 1);
			_textRank.gameObject.SetActive(false);
		}
		else
		{
			_imageRank.gameObject.SetActive(false);
			_textRank.gameObject.SetActive(true);
			_textRank.text = ranking.ToString();
		}
		if (guildInfo != null)
		{
			_guildName.text = guildInfo.GuildName;
			_guildBadge.gameObject.SetActive(true);
			_guildBadge.SetBadgeIndex(guildInfo.Badge);
			_guildBadge.SetBadgeColor((float)guildInfo.BadgeColor / 360f);
		}
		else
		{
			_guildName.text = "---";
			_guildBadge.gameObject.SetActive(false);
		}
	}
}
