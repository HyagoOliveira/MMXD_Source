#define RELEASE
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugDevAccount : Singleton<DebugDevAccount>
{
	public class DevAccount
	{
		public string PlayerId;

		public string Account;

		public string Password;
	}

	public bool RecordNextDevAccount;

	public void TryRecordDevAccount(string playerId)
	{
		if (RecordNextDevAccount)
		{
			Debug.LogWarning("[TryRecordDevAccount] PlayerId = " + playerId);
			AccountInfo accountInfo = ManagedSingleton<PlayerNetManager>.Instance.AccountInfo;
			DevAccount[] array = ReadDevAccounts();
			DevAccount devAccount2 = array.FirstOrDefault((DevAccount devAccount) => devAccount.PlayerId == playerId);
			if (devAccount2 != null)
			{
				devAccount2.Account = accountInfo.ID;
				devAccount2.Password = accountInfo.Secret;
			}
			else
			{
				devAccount2 = new DevAccount
				{
					PlayerId = playerId,
					Account = accountInfo.ID,
					Password = accountInfo.Secret
				};
				List<DevAccount> list = array.ToList();
				list.Add(devAccount2);
				array = list.ToArray();
			}
			WriteDevAccounts(array);
			RecordNextDevAccount = false;
		}
	}

	public DevAccount[] ReadDevAccounts()
	{
		Debug.Log("[ReadDevAccounts]");
		DevAccount[] obj;
		if (!JsonHelper.TryDeserialize<DevAccount[]>(PlayerPrefs.GetString("DevAccount", "[]"), out obj))
		{
			return new DevAccount[0];
		}
		return obj;
	}

	public void WriteDevAccounts(DevAccount[] accounts)
	{
		Debug.Log("[WriteDevAccounts]");
		foreach (DevAccount value in accounts)
		{
			Debug.Log("Account = " + JsonHelper.Serialize(value));
		}
		string value2 = JsonHelper.Serialize(accounts);
		PlayerPrefs.SetString("DevAccount", value2);
	}

	public void ClearDevAccounts()
	{
		Debug.LogWarning("[ClearDevAccounts]");
		PlayerPrefs.DeleteKey("DevAccount");
	}
}
