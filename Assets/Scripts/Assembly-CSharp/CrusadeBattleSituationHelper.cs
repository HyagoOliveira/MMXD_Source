#define RELEASE
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrusadeBattleSituationHelper : MonoBehaviour
{
	private class TimeInfoData
	{
		public string TimeInfo;

		public string AttackTime;

		public string PlayerID;

		public int PortraitID;
	}

	private const float TIMEINFO_MOVE_SPEED = 0.5f;

	private const float TIMEINFO_MOVE_TIME = 3f;

	[SerializeField]
	private List<CrusadeTimeInfo> _timeInfoHelpers;

	private int _lastChargedTimeInfoDataCount;

	private List<TimeInfoData> _timeInfoDataList = new List<TimeInfoData>();

	private Coroutine _timeInfoMoveCoroutine;

	private void OnDestroy()
	{
		if (_timeInfoMoveCoroutine != null)
		{
			StopCoroutine(_timeInfoMoveCoroutine);
		}
	}

	public void ChargeTimeInfoData(List<NetCrusadeOnTimeRecord> newOnTimeRecordList)
	{
		_timeInfoDataList.AddRange(newOnTimeRecordList.Select((NetCrusadeOnTimeRecord record) => new TimeInfoData
		{
			TimeInfo = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RAID_DAMAGE_INFO"), record.NickName, record.Score),
			AttackTime = DateTimeHelper.FromEpochLocalTime(record.BattleTime).ToTimeString(),
			PlayerID = record.PlayerID,
			PortraitID = record.PortraitID
		}));
		Debug.Log(string.Format("[{0}] ChargeCount = {1}, TotalCount = {2}", "ChargeTimeInfoData", newOnTimeRecordList.Count, _timeInfoDataList.Count));
		_lastChargedTimeInfoDataCount = newOnTimeRecordList.Count;
		if (_timeInfoMoveCoroutine == null)
		{
			_timeInfoMoveCoroutine = StartCoroutine(TimeCoroutine());
		}
	}

	private void ResetTimeInfoData()
	{
		for (int i = 0; i < _timeInfoHelpers.Count; i++)
		{
			SetTimeAttackData(i, (i < _timeInfoDataList.Count) ? _timeInfoDataList[i] : new TimeInfoData());
		}
	}

	private void SetTimeAttackData(int index, TimeInfoData timeInfoData)
	{
		CrusadeTimeInfo crusadeTimeInfo = _timeInfoHelpers[index];
		crusadeTimeInfo.TimeInfoText.text = ((!string.IsNullOrEmpty(timeInfoData.TimeInfo)) ? timeInfoData.TimeInfo : "-----");
		crusadeTimeInfo.AttackTimeText.text = timeInfoData.AttackTime;
		if (!string.IsNullOrEmpty(timeInfoData.TimeInfo))
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.SetPlayerIcon(crusadeTimeInfo.PlayerIconRoot, timeInfoData.PortraitID, Vector3.one, false);
		}
	}

	private IEnumerator TimeCoroutine()
	{
		yield return CoroutineDefine._waitForEndOfFrame;
		Vector3 posFirst = _timeInfoHelpers.First().transform.position;
		if (_timeInfoDataList.Count == 0)
		{
			base.gameObject.SetActive(false);
			yield break;
		}
		base.gameObject.SetActive(true);
		ResetTimeInfoData();
		if (_timeInfoDataList.Count <= 2)
		{
			yield break;
		}
		float waitTime = 3f;
		bool needResetPos = false;
		while (true)
		{
			if (waitTime > 0f)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				waitTime -= Time.deltaTime;
				continue;
			}
			for (int i = 0; i < _timeInfoHelpers.Count; i++)
			{
				CrusadeTimeInfo crusadeTimeInfo = _timeInfoHelpers[i];
				crusadeTimeInfo.transform.position -= new Vector3(0.5f, 0f, 0f);
				if (i == 1 && crusadeTimeInfo.transform.position.x <= posFirst.x)
				{
					needResetPos = true;
				}
			}
			if (needResetPos)
			{
				needResetPos = false;
				waitTime = 3f;
				Vector3 vector = posFirst - _timeInfoHelpers.First().transform.position;
				for (int j = 0; j < _timeInfoHelpers.Count; j++)
				{
					_timeInfoHelpers[j].transform.position += vector;
				}
				TimeInfoData item = _timeInfoDataList[0];
				_timeInfoDataList.RemoveAt(0);
				if (_timeInfoDataList.Count <= _lastChargedTimeInfoDataCount)
				{
					_timeInfoDataList.Add(item);
				}
				ResetTimeInfoData();
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}
}
