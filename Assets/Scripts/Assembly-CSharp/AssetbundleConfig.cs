using System.Collections.Generic;

public class AssetbundleConfig
{
	public long date;

	public List<AssetbundleId> ListAssetbundleId = new List<AssetbundleId>();

	public AssetbundleConfig(long date, List<AssetbundleId> ListAssetbundleId)
	{
		this.date = date;
		this.ListAssetbundleId = ListAssetbundleId;
	}
}
