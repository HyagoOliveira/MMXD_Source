#define RELEASE
using System.Collections.Generic;
using UnityEngine;

public class CrusadeGuildRankingPanelMini : MonoBehaviour
{
	private class RankingInfoData
	{
		public NetGuildInfo GuildInfo;

		public int Rank;

		public long Score;
	}

	[SerializeField]
	private CrusadeGuildRankingPanelMiniUnit[] _rankingUnitArray;

	[SerializeField]
	private Transform _scrollGroup;

	private List<RankingInfoData> _rankingInfoDataList = new List<RankingInfoData>();

	private int _tweenId;

	private int _tweenId2;

	private int _eventID;

	public void Setup(List<CrusadeEventRankingInfo> rankingInfoList, int eventID = 0)
	{
		_eventID = eventID;
		ClearRankInfoData();
		SetupRankInfoData(rankingInfoList);
		StartScroll();
	}

	private void OnDestroy()
	{
		ClearTween();
	}

	private void SetupRankInfoData(List<CrusadeEventRankingInfo> rankingInfoList)
	{
		for (int i = 0; i < _rankingUnitArray.Length && i < rankingInfoList.Count; i++)
		{
			CrusadeEventRankingInfo crusadeEventRankingInfo = rankingInfoList[i];
			AddRankInfo(crusadeEventRankingInfo.GuildInfo, crusadeEventRankingInfo.Ranking, crusadeEventRankingInfo.Score);
		}
		if (rankingInfoList.Count < _rankingUnitArray.Length)
		{
			for (int j = rankingInfoList.Count; j < _rankingUnitArray.Length; j++)
			{
				AddRankInfo(null, j + 1, 0L);
			}
		}
	}

	private void ShiftRankInfoData()
	{
		RankingInfoData item = _rankingInfoDataList[1];
		_rankingInfoDataList.RemoveAt(1);
		_rankingInfoDataList.Add(item);
	}

	private void UpdateRankingUnit()
	{
		for (int i = 0; i < _rankingUnitArray.Length && i < _rankingInfoDataList.Count; i++)
		{
			RankingInfoData rankingInfoData = _rankingInfoDataList[i];
			_rankingUnitArray[i].Setup(rankingInfoData.GuildInfo, rankingInfoData.Rank, rankingInfoData.Score);
		}
	}

	private void StartScroll(float scrollPause = 4f)
	{
		UpdateRankingUnit();
		ClearTween();
		_scrollGroup.transform.localPosition = Vector3.zero;
		_tweenId = LeanTween.delayedCall(scrollPause, OnTween1Action).setRepeat(-1).uniqueId;
	}

	private void OnTween1Action()
	{
		float time = 1f;
		float num = 500f;
		_tweenId2 = LeanTween.value(_scrollGroup.gameObject, 0f, 0f - num, time).setOnUpdate(OnTween2Update).setOnComplete(OnTween2Complete)
			.setEase(LeanTweenType.easeOutCubic)
			.uniqueId;
	}

	private void OnTween2Update(float val)
	{
		_scrollGroup.transform.localPosition = new Vector3(val, 0f, 0f);
	}

	private void OnTween2Complete()
	{
		_scrollGroup.transform.localPosition = Vector3.zero;
		ShiftRankInfoData();
		UpdateRankingUnit();
	}

	private void ClearRankInfoData()
	{
		_rankingInfoDataList.Clear();
	}

	public void AddRankInfo(NetGuildInfo guildInfo, int rank, long score)
	{
		RankingInfoData item = new RankingInfoData
		{
			GuildInfo = guildInfo,
			Rank = rank,
			Score = score
		};
		_rankingInfoDataList.Add(item);
	}

	public void OnClickRankingBtn()
	{
		if (_eventID != Singleton<CrusadeSystem>.Instance.EventID)
		{
			Debug.LogError(string.Format("EventId mismatch : {0} != {1}", _eventID, Singleton<CrusadeSystem>.Instance.EventID));
		}
		else if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RankingUIFlag)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<RankingMainUI>("UI_RankingMain", OnRankingMainUILoaded);
		}
	}

	private void OnRankingMainUILoaded(RankingMainUI ui)
	{
		ui.Setup(_eventID);
	}

	private void ClearTween()
	{
		LeanTween.cancel(ref _tweenId);
		LeanTween.cancel(ref _tweenId2);
	}
}
