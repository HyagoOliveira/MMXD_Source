using System.Collections.Generic;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;

internal class ServerSelectMainUI : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect m_gameScrollRect;

	[SerializeField]
	private LoopVerticalScrollRect m_zoneScrollRect;

	private GameServerInfo serverList;

	private List<GameServerZoneInfo> bestZoneList = new List<GameServerZoneInfo>();

	private int currentSelectedGameTabIndex = 1;

	private RetrieveServerStatusRes serverStatusRes;

	private HashSet<int> offlineServerIDHash = new HashSet<int>();

	private HashSet<int> overloadServerIDHash = new HashSet<int>();

	private GameServerZoneInfo zoneInfo;

	public string GetServerGameName(int index)
	{
		if (index >= serverList.Game.Count)
		{
			return string.Empty;
		}
		return MonoBehaviourSingleton<LocalizationManager>.Instance.GetNameFromGameServerNameInfo(serverList.Game[index].Name);
	}

	public GameServerZoneInfo GetZoneInfo(int zoneIndex)
	{
		if (currentSelectedGameTabIndex == 0)
		{
			return bestZoneList[zoneIndex];
		}
		return serverList.Game[currentSelectedGameTabIndex - 1].Zone[zoneIndex];
	}

	public void Setup(GameServerInfo inputServerList)
	{
		serverList = inputServerList;
		FindBestZone();
		List<GameServerGameInfo> game = serverList.Game;
		m_gameScrollRect.totalCount = game.Count + 1;
		if (ManagedSingleton<ServerStatusHelper>.Instance.IsUpdated())
		{
			m_gameScrollRect.RefillCells();
			UpdateServerGameButton(0);
		}
		else
		{
			ManagedSingleton<ServerStatusHelper>.Instance.UpdateServerStatus(delegate
			{
				m_gameScrollRect.RefillCells();
				UpdateServerGameButton(0);
			});
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void FindBestZone()
	{
		bestZoneList.Clear();
		if (serverList == null)
		{
			return;
		}
		int bestZoneID = ManagedSingleton<ServerStatusHelper>.Instance.GetBestZoneID();
		if (bestZoneID != 0)
		{
			FindBestZoneNew(bestZoneID);
			return;
		}
		for (int i = 0; i < serverList.Game.Count; i++)
		{
			for (int j = 0; j < serverList.Game[i].Zone.Count; j++)
			{
				if (serverList.Game[i].Zone[j].Best == 1)
				{
					bestZoneList.Add(serverList.Game[i].Zone[j]);
				}
			}
		}
	}

	private void FindBestZoneNew(int bestZoneID)
	{
		for (int i = 0; i < serverList.Game.Count; i++)
		{
			for (int j = 0; j < serverList.Game[i].Zone.Count; j++)
			{
				if (serverList.Game[i].Zone[j].ID == bestZoneID)
				{
					bestZoneList.Add(serverList.Game[i].Zone[j]);
					break;
				}
			}
		}
	}

	public int GetSelectedGameTabIndex()
	{
		return currentSelectedGameTabIndex;
	}

	private void UpdateServerGameButton(int index)
	{
		currentSelectedGameTabIndex = index;
		if (index == 0)
		{
			m_zoneScrollRect.totalCount = bestZoneList.Count;
			m_zoneScrollRect.RefillCells();
		}
		else
		{
			m_zoneScrollRect.totalCount = serverList.Game[currentSelectedGameTabIndex - 1].Zone.Count;
			m_zoneScrollRect.RefillCells();
		}
	}

	public void OnClickServerGameButton(int index)
	{
		if (currentSelectedGameTabIndex != index)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		}
		UpdateServerGameButton(index);
	}

	public void ZoneSelectedCallback(int zoneIndex)
	{
		zoneInfo = GetZoneInfo(zoneIndex);
		string host = zoneInfo.Host;
		int iD = zoneInfo.ID;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChoseServiceZoneID = iD;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		OnClickCloseBtn();
	}
}
