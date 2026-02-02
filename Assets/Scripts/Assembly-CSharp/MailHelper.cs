using System.Collections.Generic;
using enums;

public class MailHelper : ManagedSingleton<MailHelper>
{
	public enum MailType
	{
		NEW = 0,
		HISTORY = 1
	}

	private List<MailInfo> listMail = new List<MailInfo>();

	public int PreparedMailCount
	{
		get
		{
			return listMail.Count;
		}
		private set
		{
		}
	}

	public bool DisplayHint
	{
		get
		{
			if (ManagedSingleton<PlayerNetManager>.Instance.dicMail.Count <= 0)
			{
				return false;
			}
			return true;
		}
	}

	public override void Initialize()
	{
	}

	public override void Reset()
	{
		base.Reset();
		listMail.Clear();
	}

	public override void Dispose()
	{
		listMail.Clear();
	}

	public void PrepareMailList(MailType type)
	{
		if (type == MailType.NEW)
		{
			listMail = new List<MailInfo>();
			foreach (MailInfo value in ManagedSingleton<PlayerNetManager>.Instance.dicMail.Values)
			{
				if (value.netMailInfo.RecycleTime > MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC)
				{
					listMail.Add(value);
				}
			}
		}
		else
		{
			listMail = new List<MailInfo>(ManagedSingleton<PlayerNetManager>.Instance.dicReservedMail.Values);
		}
		listMail.Reverse();
	}

	public List<int> CollectRetrieveAllMailIDList(out bool epExcluded)
	{
		epExcluded = false;
		List<int> list = new List<int>();
		List<NetMailInfo> list2 = new List<NetMailInfo>();
		int num = ManagedSingleton<PlayerHelper>.Instance.GetEventStamina();
		foreach (MailInfo item in listMail)
		{
			if (item.netMailInfo.ReservedTime != 0)
			{
				continue;
			}
			if (item.netMailInfo.AttachmentAmount1 > 0 || item.netMailInfo.AttachmentAmount2 > 0 || item.netMailInfo.AttachmentAmount3 > 0)
			{
				int amount;
				if (!ContainsSpecificItem(item.netMailInfo, RewardType.Item, OrangeConst.ITEMID_EVENTAP, out amount))
				{
					list.Add(item.netMailInfo.MailID);
				}
				else if (num + amount > OrangeConst.EP_MAX)
				{
					list2.Add(item.netMailInfo);
					epExcluded = true;
				}
				else
				{
					list.Add(item.netMailInfo.MailID);
					num += amount;
				}
			}
			if (list.Count >= OrangeConst.MAIL_ONECOUNT)
			{
				break;
			}
		}
		return list;
	}

	public bool ContainsSpecificItem(NetMailInfo mail, RewardType type, int itemID, out int amount)
	{
		amount = 0;
		if (mail.AttachmentType1 == (sbyte)type && mail.AttachmentID1 == itemID)
		{
			amount += mail.AttachmentAmount1;
		}
		if (mail.AttachmentType2 == (sbyte)type && mail.AttachmentID2 == itemID)
		{
			amount += mail.AttachmentAmount2;
		}
		if (mail.AttachmentType3 == (sbyte)type && mail.AttachmentID3 == itemID)
		{
			amount += mail.AttachmentAmount3;
		}
		return amount > 0;
	}

	public MailInfo GetMailInfo(int idx)
	{
		if (listMail[idx] != null)
		{
			return listMail[idx];
		}
		return null;
	}
}
