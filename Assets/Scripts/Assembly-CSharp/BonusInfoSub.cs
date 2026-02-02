using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using enums;

public class BonusInfoSub : CommonSubMenu
{
	public class InfoLable
	{
		public int bonusType;

		public int stageType;

		public int sValue;

		public long remainTime;

		public EVENT_TABLE eventTable;
	}

	[SerializeField]
	private GameObject m_sample;

	[SerializeField]
	private OrangeText m_bonusTitle;

	[SerializeField]
	private OrangeText m_bonusTimeLimit;

	public List<InfoLable> dicBonusEvent = new List<InfoLable>();

	public List<EVENT_TABLE> eventTblList = new List<EVENT_TABLE>();

	private void Start()
	{
	}

	public void SetActive(bool act)
	{
		if (content.transform.childCount > 0)
		{
			base.transform.gameObject.SetActive(act);
		}
		else
		{
			base.transform.gameObject.SetActive(false);
		}
	}

	public void Clear()
	{
		dicBonusEvent.Clear();
		while (content.transform.childCount != 0)
		{
			Object.DestroyImmediate(content.transform.GetChild(0).gameObject);
		}
	}

	public void ReCreateInfo(int stageID)
	{
		Clear();
		STAGE_TABLE stageTbl;
		if (!ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(stageID, out stageTbl))
		{
			return;
		}
		(from p in ManagedSingleton<OrangeDataManager>.Instance.EVENT_TABLE_DICT
			where p.Value.n_TYPE == 4
			select p into a
			select a.Value).ToList().ForEach(delegate(EVENT_TABLE tbl)
		{
			long serverUnixTimeNowUTC = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
			if (ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(tbl.s_BEGIN_TIME, tbl.s_END_TIME, serverUnixTimeNowUTC) && tbl.n_TYPE_X == stageTbl.n_TYPE && (tbl.n_TYPE_Y == stageTbl.n_MAIN || tbl.n_TYPE_Y == 0))
			{
				InfoLable item2 = new InfoLable
				{
					bonusType = tbl.n_BONUS_TYPE,
					stageType = stageTbl.n_TYPE,
					sValue = ((tbl.n_BONUS_TYPE == 4) ? tbl.n_BONUS_RATE : (tbl.n_BONUS_RATE - 100)),
					remainTime = CapUtility.DateToUnixTime(ManagedSingleton<OrangeTableHelper>.Instance.ParseDate(tbl.s_END_TIME).AddHours(-ManagedSingleton<OrangeTableHelper>.Instance.ServerTimeZone)),
					eventTable = tbl
				};
				dicBonusEvent.Add(item2);
			}
		});
		ManagedSingleton<PlayerNetManager>.Instance.dicService.Select((KeyValuePair<int, ServiceInfo> p) => p.Value.netServiceInfo).ToList().ForEach(delegate(NetPlayerServiceInfo info)
		{
			SERVICE_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.SERVICE_TABLE_DICT.TryGetValue(info.ServiceID, out value) && value.n_TYPE == 4 && value.n_TYPE_1 == stageTbl.n_TYPE && info.ExpireTime >= MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC)
			{
				InfoLable item = new InfoLable
				{
					bonusType = value.n_TYPE_3,
					stageType = value.n_TYPE_1,
					sValue = ((value.n_TYPE_3 == 4) ? value.n_TYPE_4 : (value.n_TYPE_4 - 100)),
					remainTime = info.ExpireTime,
					eventTable = null
				};
				dicBonusEvent.Add(item);
			}
		});
	}

	public void SetCommonIcon(CommonIconBase icon, int equID)
	{
		int addCnt = 0;
		icon.SetBonusInfoEnabled(false);
		if (dicBonusEvent.Count != 0)
		{
			dicBonusEvent.ForEach(delegate(InfoLable lab)
			{
				if (lab.eventTable != null && lab.eventTable.n_SP_TYPE != 0 && lab.eventTable.n_SP_ID == equID)
				{
					icon.AddBonusInfo((BonusType)lab.bonusType, lab.sValue);
					addCnt++;
				}
			});
		}
		if (addCnt != 0)
		{
			icon.SetBonusInfoEnabled(true);
		}
	}

	public void SetupInfo(int stageID)
	{
		ReCreateInfo(stageID);
		AddLabel(dicBonusEvent);
	}

	public void AddLabel(List<InfoLable> dicInfo, bool bUseSPID = false)
	{
		dicInfo.ForEach(delegate(InfoLable lab)
		{
			string text = ManagedSingleton<OrangeTableHelper>.Instance.dicBonusIcon[(BonusType)lab.bonusType].m_localization;
			if (!string.IsNullOrEmpty(text))
			{
				text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(text), lab.sValue);
			}
			m_bonusTitle.text = text;
			m_bonusTimeLimit.text = OrangeGameUtility.GetRemainTimeText(lab.remainTime);
			Object.Instantiate(m_sample, content.transform, false).SetActive(true);
		});
	}
}
