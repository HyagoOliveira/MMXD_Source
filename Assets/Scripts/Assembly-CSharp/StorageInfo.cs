using System;
using System.Collections.Generic;
using CallbackDefs;

public class StorageInfo
{
	public string L10nKey;

	public bool IsNew;

	public StorageInfo[] Sub;
    [Obsolete]
    public CallbackObj ClickCb;

	public object[] Param;

	public Func<object[], bool> IsNewChecker;

	public Func<object[], bool> IsSuggestChecker;

	public Dictionary<string, LOCALIZATION_TABLE> Refl10nTable;

	public StorageInfo Parent { get; set; }

	public float AnimAddDelay { get; set; }

    [Obsolete]
    public StorageInfo(string L10nKey, bool IsNew, int SubCount, CallbackObj ClickCb = null, Func<object[], bool> isNewChecker = null, Func<object[], bool> isSuggestChecker = null)
	{
		this.L10nKey = L10nKey;
		this.IsNew = IsNew;
		Sub = new StorageInfo[SubCount];
		this.ClickCb = ClickCb;
		IsNewChecker = isNewChecker;
		IsSuggestChecker = isSuggestChecker;
	}

	public bool UpdateNew()
	{
		if (IsNewChecker != null)
		{
			return IsNewChecker(Param);
		}
		return false;
	}

	public bool UpdateSuggest()
	{
		if (IsSuggestChecker != null)
		{
			return IsSuggestChecker(Param);
		}
		return false;
	}
}
