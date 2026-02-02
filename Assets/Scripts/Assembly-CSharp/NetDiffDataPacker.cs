using System.Collections.Generic;
using JsonFx.Json;

public class NetDiffDataPacker
{
	[JsonName("_")]
	public List<NetDiffData> Vec = new List<NetDiffData>();

	public void Add(NetDiffData diff)
	{
		Vec.Add(diff);
	}
}
