using System;
using System.Collections.Generic;
using OrangeSocket;
using UnityEngine;
using cc;

public class RankPanelMini : MonoBehaviour
{
	private struct RankInfo
	{
		public string playerID;

		public int rank;

		public int score;
	}

	[SerializeField]
	private Transform[] m_rankNameGroupArray;

	[SerializeField]
	private Transform m_scrollGroup;

	private List<RankInfo> m_rankInfoList = new List<RankInfo>();

	private int m_displayIndex;

	private int m_tweenId;

	private int m_tweenId2;

	private int EventID;

	private void OnDestroy()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CC.RSGetPlayerHUDList, OnCreateRSGetPlayerHUDListCallback);
	}

	public void OnCreateRSGetPlayerHUDListCallback(object res)
	{
		if (!(res is RSGetPlayerHUDList))
		{
			return;
		}
		RSGetPlayerHUDList rSGetPlayerHUDList = (RSGetPlayerHUDList)res;
		if (rSGetPlayerHUDList.Result != 70300)
		{
			return;
		}
		for (int i = 0; i < rSGetPlayerHUDList.PlayerHUDLength; i++)
		{
			SocketPlayerHUD socketPlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(rSGetPlayerHUDList.PlayerHUD(i));
			if (socketPlayerHUD != null)
			{
				ManagedSingleton<SocketHelper>.Instance.UpdateHUD(socketPlayerHUD.m_PlayerId, socketPlayerHUD);
			}
		}
	}

	public void Setup(List<EventRankingInfo> eventRankingInfoList, int eventID = 0)
	{
		EventID = eventID;
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetPlayerHUDList, OnCreateRSGetPlayerHUDListCallback);
		ClearRankInfo();
		if (eventRankingInfoList.Count == 0)
		{
			AddRankInfo("---", 1, 0);
			AddRankInfo("---", 2, 0);
			AddRankInfo("---", 3, 0);
		}
		else
		{
			string[] array = new string[eventRankingInfoList.Count];
			for (int i = 0; i < eventRankingInfoList.Count; i++)
			{
				array[i] = eventRankingInfoList[i].PlayerID;
			}
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerHUDList(array));
			foreach (EventRankingInfo eventRankingInfo in eventRankingInfoList)
			{
				AddRankInfo(eventRankingInfo.PlayerID, eventRankingInfo.Ranking, eventRankingInfo.Score);
			}
			if (eventRankingInfoList.Count < 3)
			{
				for (int j = eventRankingInfoList.Count + 1; j <= 3; j++)
				{
					AddRankInfo("---", j, 0);
				}
			}
		}
		StartScroll();
	}

	private void SetupPanel()
	{
		if (m_displayIndex > m_rankInfoList.Count - 1)
		{
			m_rankNameGroupArray[1].GetComponent<RankPanelMiniUnit>().Setup(m_rankInfoList[m_displayIndex - 1].playerID, m_rankInfoList[m_displayIndex - 1].rank, m_rankInfoList[m_displayIndex - 1].score);
			m_rankNameGroupArray[2].GetComponent<RankPanelMiniUnit>().Setup(m_rankInfoList[1].playerID, m_rankInfoList[1].rank, m_rankInfoList[1].score);
			m_displayIndex = 2;
		}
		else
		{
			m_rankNameGroupArray[1].GetComponent<RankPanelMiniUnit>().Setup(m_rankInfoList[m_displayIndex - 1].playerID, m_rankInfoList[m_displayIndex - 1].rank, m_rankInfoList[m_displayIndex - 1].score);
			m_rankNameGroupArray[2].GetComponent<RankPanelMiniUnit>().Setup(m_rankInfoList[m_displayIndex].playerID, m_rankInfoList[m_displayIndex].rank, m_rankInfoList[m_displayIndex].score);
			m_displayIndex++;
		}
	}

	private void StartScroll(float scrollSpeed = 1f, float scrollPause = 4f)
	{
		float scrollDist = 500f;
		m_displayIndex = 2;
		m_rankNameGroupArray[0].GetComponent<RankPanelMiniUnit>().Setup(m_rankInfoList[0].playerID, m_rankInfoList[0].rank, m_rankInfoList[0].score);
		SetupPanel();
		LeanTween.cancel(ref m_tweenId);
		LeanTween.cancel(ref m_tweenId2);
		m_scrollGroup.transform.localPosition = new Vector3(0f, 0f, 0f);
		m_tweenId = LeanTween.delayedCall(scrollPause, (Action)delegate
		{
			m_tweenId2 = LeanTween.value(m_scrollGroup.gameObject, 0f, 0f - scrollDist, scrollSpeed).setOnUpdate(delegate(float val)
			{
				m_scrollGroup.transform.localPosition = new Vector3(val, 0f, 0f);
			}).setOnComplete((Action)delegate
			{
				m_tweenId2 = -1;
				m_scrollGroup.transform.localPosition = new Vector3(0f, 0f, 0f);
				SetupPanel();
			})
				.setEase(LeanTweenType.easeOutCubic)
				.uniqueId;
		}).setRepeat(-1).uniqueId;
	}

	private void ClearRankInfo()
	{
		m_rankInfoList.Clear();
	}

	public void AddRankInfo(string playerID, int rank, int score)
	{
		RankInfo item = default(RankInfo);
		item.playerID = playerID;
		item.rank = rank;
		item.score = score;
		m_rankInfoList.Add(item);
	}

	public void OnClickRankingBtn()
	{
		if (EventID != 0 && MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RankingUIFlag)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RankingMain", delegate(RankingMainUI ui)
			{
				ui.Setup(EventID);
			});
		}
	}

	private void OnDisable()
	{
		LeanTween.cancel(ref m_tweenId);
		LeanTween.cancel(ref m_tweenId2);
	}
}
