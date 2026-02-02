public class FinalStrikeInfo
{
	private NetFinalStrikeInfo _netFinalStrikeInfo;

	public bool bIsDirty;

	public static bool bIsAnyDirty;

	public static bool bIsItmeUpdateDirty;

	public bool bIsCanStarUp;

	public NetFinalStrikeInfo netFinalStrikeInfo
	{
		get
		{
			return _netFinalStrikeInfo;
		}
		set
		{
			_netFinalStrikeInfo = value;
			if (_netFinalStrikeInfo.FinalStrikeID != 0)
			{
				bIsDirty = true;
				bIsAnyDirty = true;
			}
		}
	}
}
