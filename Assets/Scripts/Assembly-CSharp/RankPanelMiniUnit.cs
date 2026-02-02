using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RankPanelMiniUnit : MonoBehaviour
{
	[SerializeField]
	private Transform m_rankDiamond;

	[SerializeField]
	private OrangeText m_playerName;

	[SerializeField]
	private Transform m_playerIcon;

	[SerializeField]
	private OrangeText m_rankText;

	private string m_bundleName = "texture/2d/ui/ui_ranking";

	private string m_playerID;

	private int m_ranking;

	private int m_score;

	public void Setup(string playerID, int ranking, int score)
	{
		m_playerID = playerID;
		m_ranking = ranking;
		m_score = score;
		foreach (Transform item in m_playerIcon)
		{
			Object.Destroy(item.gameObject);
		}
		string assetName = "";
		switch (ranking)
		{
		case 1:
			assetName = "UI_Ranking_Top1Icon";
			break;
		case 2:
			assetName = "UI_Ranking_Top2Icon";
			break;
		case 3:
			assetName = "UI_Ranking_Top3Icon";
			break;
		}
		m_rankDiamond.gameObject.SetActive(false);
		if (m_rankText != null)
		{
			m_rankText.gameObject.SetActive(false);
		}
		if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			if (ranking <= 3)
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(m_bundleName, assetName, delegate(Sprite icon)
				{
					m_rankDiamond.gameObject.SetActive(true);
					m_rankDiamond.GetComponent<Image>().gameObject.SetActive(true);
					m_rankDiamond.GetComponent<Image>().sprite = icon;
				});
			}
			else if (m_rankText != null)
			{
				m_rankText.gameObject.SetActive(true);
				m_rankText.text = ranking.ToString();
			}
		}
		if (string.Compare(m_playerID, "---") != 0)
		{
			if (base.isActiveAndEnabled)
			{
				StartCoroutine(AssignPlayerName(m_playerID));
			}
		}
		else
		{
			m_playerName.text = "---";
			m_playerIcon.gameObject.SetActive(false);
		}
	}

	private IEnumerator AssignPlayerName(string playerID)
	{
		while (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.ContainsKey(playerID))
		{
			yield return new WaitForSeconds(0.3f);
		}
		SocketPlayerHUD value;
		if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(playerID, out value))
		{
			SetPlayerHUD(value);
		}
	}

	private void SetPlayerHUD(SocketPlayerHUD playerHUD)
	{
		m_playerName.text = playerHUD.m_Name;
		m_playerIcon.gameObject.SetActive(true);
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(m_playerIcon, playerHUD.m_IconNumber, new Vector3(1f, 1f, 1f), false);
	}
}
