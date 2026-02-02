using System.Collections.Generic;

public class StageInfo
{
	public NetStageInfo netStageInfo;

	public List<int> StageSecretList = new List<int>();

	public List<int> TowerBossInfoList = new List<int>();

	public void AddStageSecretToList(int nStageSecretID)
	{
		if (!StageSecretList.Contains(nStageSecretID))
		{
			StageSecretList.Add(nStageSecretID);
		}
	}
}
