using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuildBuffFloatInfoUI : CommonFloatUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect _scrollRect;

	[SerializeField]
	private GuildBuffFloatInfoCell _scrollCell;

	public List<PowerPillarInfoData> PowerPillarInfoDataList { get; private set; }

	public void Setup(List<PowerPillarInfoData> powerPillarInfoDataList, Vector3 tarPos)
	{
		base.Setup(tarPos);
		RefreshGuildBuffList(powerPillarInfoDataList);
	}

	public void RefreshGuildBuffList(List<PowerPillarInfoData> powerPillarInfoDataList)
	{
		PowerPillarInfoDataList = powerPillarInfoDataList;
		if (PowerPillarInfoDataList.Count > 0)
		{
			_scrollRect.OrangeInit(_scrollCell, 3, PowerPillarInfoDataList.Count);
		}
		else
		{
			OnClickCloseBtn();
		}
	}
}
