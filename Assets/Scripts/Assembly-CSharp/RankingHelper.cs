using System.Collections.Generic;
using OrangeSocket;
using cc;
using enums;

public class RankingHelper : ManagedSingleton<RankingHelper>
{
	public int CurrentRankType;

	public int CurrentServerID;

	public int CurrentFriendRankType;

	public int CurrentPersonalRankType;

	public bool FriendFlag;

	private RankType LAST_RANK_TYPE = RankType.TopPVETop;

	private Dictionary<RankType, List<SocketRankingInfo>> dicRankingList = new Dictionary<RankType, List<SocketRankingInfo>>();

	private Dictionary<RankType, List<SocketRankingInfo>> dicFriendRankingList = new Dictionary<RankType, List<SocketRankingInfo>>();

	private Dictionary<RankType, List<SocketRankingInfo>> dicPersonalRankingList = new Dictionary<RankType, List<SocketRankingInfo>>();

	private Dictionary<RankType, SocketRankingTypeInfo> dicPlayerRank = new Dictionary<RankType, SocketRankingTypeInfo>();

	private Dictionary<RankType, SocketRankingTypeInfo> dicPlayerFriendRank = new Dictionary<RankType, SocketRankingTypeInfo>();

	public override void Initialize()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSTopRankGetList, OnCreateRSTopRankGetListCallback);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSTopRankMyNearGetList, OnCreateRSTopRankMyNearGetListCallback);
	}

	public override void Dispose()
	{
	}

	public void RetrieveRankingList()
	{
		CurrentServerID = MonoBehaviourSingleton<GameServerService>.Instance.ServiceZoneID;
		dicRankingList.Clear();
		dicPlayerRank.Clear();
		CurrentRankType = 0;
		FriendFlag = false;
		OnRetrieveRankingListLoop();
	}

	public void OnRetrieveRankingListLoop()
	{
		CurrentRankType++;
		if (CurrentRankType <= (int)LAST_RANK_TYPE)
		{
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQTopRankGetList(CurrentServerID, CurrentRankType, 0, 99));
		}
		else
		{
			RetrieveFriendRankingList();
		}
	}

	public void OnCreateRSTopRankGetListCallback(object res)
	{
		if (!(res is RSTopRankGetList))
		{
			return;
		}
		RSTopRankGetList rSTopRankGetList = (RSTopRankGetList)res;
		if (rSTopRankGetList.Result != 73000)
		{
			return;
		}
		if (FriendFlag)
		{
			SocketRankingTypeInfo socketRankingTypeInfo = new SocketRankingTypeInfo();
			socketRankingTypeInfo.m_RankType = (RankType)rSTopRankGetList.RankType;
			socketRankingTypeInfo.m_Rank = rSTopRankGetList.PlayerRank;
			socketRankingTypeInfo.m_Score = rSTopRankGetList.PlayerScore;
			socketRankingTypeInfo.m_TotalCount = rSTopRankGetList.RankTotalCount;
			socketRankingTypeInfo.m_ServerID = rSTopRankGetList.ServerID;
			dicPlayerFriendRank.Add((RankType)rSTopRankGetList.RankType, socketRankingTypeInfo);
			dicFriendRankingList[(RankType)rSTopRankGetList.RankType] = new List<SocketRankingInfo>();
		}
		else
		{
			SocketRankingTypeInfo socketRankingTypeInfo2 = new SocketRankingTypeInfo();
			socketRankingTypeInfo2.m_RankType = (RankType)rSTopRankGetList.RankType;
			socketRankingTypeInfo2.m_Rank = rSTopRankGetList.PlayerRank;
			socketRankingTypeInfo2.m_Score = rSTopRankGetList.PlayerScore;
			socketRankingTypeInfo2.m_TotalCount = rSTopRankGetList.RankTotalCount;
			socketRankingTypeInfo2.m_ServerID = rSTopRankGetList.ServerID;
			dicPlayerRank.Add((RankType)rSTopRankGetList.RankType, socketRankingTypeInfo2);
			dicRankingList[(RankType)rSTopRankGetList.RankType] = new List<SocketRankingInfo>();
		}
		for (int i = 0; i < rSTopRankGetList.PlayerIDLength; i++)
		{
			string text = rSTopRankGetList.PlayerID(i);
			ManagedSingleton<SocketHelper>.Instance.UpdateHUD(text, rSTopRankGetList.PlayerHUD(i));
			SocketPlayerHUD value;
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(text, out value))
			{
				SocketRankingInfo item = new SocketRankingInfo
				{
					m_Index = i,
					m_Rank = i,
					m_PlayerId = text,
					m_PlayerName = rSTopRankGetList.PlayerName(i),
					m_Score = rSTopRankGetList.ScoreList(i),
					m_BestWeaponModelID = rSTopRankGetList.ValueList(i),
					m_PlayerModelID = value.m_StandbyCharID,
					m_PlayerModelSkin = value.m_StandbyCharSkin,
					m_MainWeaponModelID = value.m_MainWeaponID,
					m_MainWeaponModelSkin = value.m_MainWeaponSkin
				};
				if (FriendFlag)
				{
					dicFriendRankingList[(RankType)rSTopRankGetList.RankType].Add(item);
				}
				else
				{
					dicRankingList[(RankType)rSTopRankGetList.RankType].Add(item);
				}
			}
		}
		OnRetrieveRankingListLoop();
	}

	public void RetrieveFriendRankingList()
	{
		if (FriendFlag)
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicRankingList = new Dictionary<RankType, List<SocketRankingInfo>>(dicRankingList);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicPlayerRank = new Dictionary<RankType, SocketRankingTypeInfo>(dicPlayerRank);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriendRankingList = new Dictionary<RankType, List<SocketRankingInfo>>(dicFriendRankingList);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicPlayerFriendRank = new Dictionary<RankType, SocketRankingTypeInfo>(dicPlayerFriendRank);
			MonoBehaviourSingleton<CCSocketClient>.Instance.SetSyncNetworkFrequency(1000);
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.RankingUIFlag = true;
		}
		else
		{
			FriendFlag = true;
			CurrentRankType = 0;
			CurrentServerID = 0;
			dicFriendRankingList.Clear();
			dicPlayerFriendRank.Clear();
			OnRetrieveRankingListLoop();
		}
	}

	public void RetrievePersonalRankingList()
	{
		dicPersonalRankingList.Clear();
		CurrentPersonalRankType = 0;
		OnRetrievePersonalRankingListLoop();
	}

	public void OnRetrievePersonalRankingListLoop()
	{
		CurrentPersonalRankType++;
		if (CurrentPersonalRankType <= (int)LAST_RANK_TYPE)
		{
			MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQTopRankMyNearGetList(CurrentServerID, CurrentPersonalRankType));
		}
		else
		{
			MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicPersonalRankingList = new Dictionary<RankType, List<SocketRankingInfo>>(dicPersonalRankingList);
		}
	}

	public void OnCreateRSTopRankMyNearGetListCallback(object res)
	{
		if (!(res is RSTopRankMyNearGetList))
		{
			return;
		}
		RSTopRankMyNearGetList rSTopRankMyNearGetList = (RSTopRankMyNearGetList)res;
		if (rSTopRankMyNearGetList.Result != 73000)
		{
			return;
		}
		dicPersonalRankingList[(RankType)rSTopRankMyNearGetList.RankType] = new List<SocketRankingInfo>();
		for (int i = 0; i < rSTopRankMyNearGetList.PlayerIDLength; i++)
		{
			string text = rSTopRankMyNearGetList.PlayerID(i);
			ManagedSingleton<SocketHelper>.Instance.UpdateHUD(text, rSTopRankMyNearGetList.PlayerHUD(i));
			SocketPlayerHUD value;
			if (MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicHUD.TryGetValue(text, out value))
			{
				SocketRankingInfo item = new SocketRankingInfo
				{
					m_Index = i,
					m_Rank = rSTopRankMyNearGetList.Start + i,
					m_PlayerId = text,
					m_PlayerName = rSTopRankMyNearGetList.PlayerName(i),
					m_Score = rSTopRankMyNearGetList.ScoreList(i),
					m_BestWeaponModelID = rSTopRankMyNearGetList.ValueList(i),
					m_PlayerModelID = value.m_StandbyCharID,
					m_PlayerModelSkin = value.m_StandbyCharSkin,
					m_MainWeaponModelID = value.m_MainWeaponID,
					m_MainWeaponModelSkin = value.m_MainWeaponSkin
				};
				dicPersonalRankingList[(RankType)rSTopRankMyNearGetList.RankType].Add(item);
			}
		}
		OnRetrievePersonalRankingListLoop();
	}
}
