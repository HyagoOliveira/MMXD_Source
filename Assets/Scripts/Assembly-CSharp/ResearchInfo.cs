using System.Collections.Generic;

public class ResearchInfo
{
	public SortedDictionary<int, NetResearchInfo> dicResearch = new SortedDictionary<int, NetResearchInfo>();

	public List<NetFreeResearchInfo> listFreeResearch;

	public List<NetResearchRecord> listResearchRecord;
}
