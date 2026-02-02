public class ItemInfo
{
	private NetItemInfo _netItemInfo;

	public NetItemInfo netItemInfo
	{
		get
		{
			return _netItemInfo;
		}
		set
		{
			_netItemInfo = value;
			FinalStrikeInfo.bIsAnyDirty = true;
			FinalStrikeInfo.bIsItmeUpdateDirty = true;
		}
	}
}
