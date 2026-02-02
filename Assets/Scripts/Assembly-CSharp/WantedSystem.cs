#define RELEASE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OrangeApi;
using OrangeSocket;
using cc;
using enums;

public class WantedSystem : Singleton<WantedSystem>
{
	private class WantedRecommendedResult
	{
		public List<int> PickingSequence;

		public List<NetCharacterInfo> CandidateCharacterList;

		public List<NetCharacterInfo> CharacterInfoList;

		public WantedConditionFlag ConditionFlag;

		public int ConditionLevel;
	}

	private enum PickingType
	{
		ByCharacterId = 0,
		ByCharacterCount = 1,
		ByTotalStarCount = 2,
		ByGoCharacterCount = 3
	}

	private bool _loadUIAfterGetWantedInfo;

	private string[] _retrieveFriendList;

	private List<WantedMemberInfo> _characterInfoList = new List<WantedMemberInfo>();

	private List<WantedMemberInfo> _friendInfoList = new List<WantedMemberInfo>();

	public CharacterHelper.SortType SortType;

	public bool IsSortDescend;

	private readonly List<int> PickingTypes;

	private readonly List<List<int>> PickingSequenceList;

	private List<NetCharacterInfo> _sortedCandidateCharacterList;

	public List<WantedTargetInfo> WantedTargetInfoCacheList { get; private set; }

	public List<int> DeparturedCharacterCacheList { get; private set; }

	public List<NetWantedHelpInfo> WantedHelpInfoCacheList { get; private set; }

	public int UsedHelpCount { get; private set; }

	public List<WantedMemberInfo> SortedCharacterInfoList { get; private set; } = new List<WantedMemberInfo>();


	public List<WantedMemberInfo> SortedFriendInfoList { get; private set; } = new List<WantedMemberInfo>();


	public event Action<Code> OnRetrieveWantedInfoEvent;

	public event Action<Code> OnWantedStartEvent;

	public event Action<Code, List<NetRewardInfo>, WantedSuccessType> OnReceiveWantedEvent;

	public event Action<Code> OnRefreshWantedSlotEvent;

	public WantedSystem()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSFriendGetList, OnRefreshFriendList);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CC.RSGetPlayerCharacterInfoList, OnGetPlayerCharacterInfoList);
		PickingTypes = Enum.GetValues(typeof(PickingType)).Cast<int>().ToList();
		PickingSequenceList = MathUtility.GetPermutations(PickingTypes, PickingTypes.Count);
	}

	private void TriggerWantedInfoEvent()
	{
		Debug.Log("[TriggerWantedInfoEvent]");
		_loadUIAfterGetWantedInfo = false;
		Action<Code> onRetrieveWantedInfoEvent = this.OnRetrieveWantedInfoEvent;
		if (onRetrieveWantedInfoEvent != null)
		{
			onRetrieveWantedInfoEvent(Code.WANTED_GET_INFO_SUCCESS);
		}
	}

	public void ReqRetrieveWantedInfo(bool loadUI = false)
	{
		_loadUIAfterGetWantedInfo = loadUI;
		ManagedSingleton<PlayerNetManager>.Instance.WantedReqRetrieveWantedInfo(OnRetrieveWantedInfoRes);
	}

	public void ReqWantedStart(sbyte slot, List<int> characterIdList, NetWantedHelpInfo helpInfo)
	{
		ManagedSingleton<PlayerNetManager>.Instance.WantedReqStart(slot, characterIdList, helpInfo, OnWantedStartRes);
	}

	public void ReqReceiveWanted(sbyte slot, bool isBoost = false)
	{
		ManagedSingleton<PlayerNetManager>.Instance.WantedReqReceiveWanted(slot, (sbyte)(isBoost ? 1 : 0), OnReceiveWantedRes);
	}

	public void ReqRefreshWantedSlot(sbyte slot)
	{
		ManagedSingleton<PlayerNetManager>.Instance.WantedReqRefreshWantedSlot(slot, OnRefreshWantedSlotRes);
	}

	private void OnRetrieveWantedInfoRes(RetrieveWantedInfoRes resp)
	{
		Debug.Log("[OnRetrieveWantedInfoRes]");
		try
		{
			Code code = (Code)resp.Code;
			if (code == Code.WANTED_GET_INFO_SUCCESS)
			{
				RefreshWantedInfo(resp.WantedInfoList);
				UsedHelpCount = resp.HelpCount;
				if (_loadUIAfterGetWantedInfo)
				{
					MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
					_characterInfoList = (from characterInfo in ManagedSingleton<CharacterHelper>.Instance.SortCharacterListNoFragmentNoSave()
						select new WantedMemberInfo
						{
							CharacterInfo = characterInfo.netInfo,
							PlayerHUD = null,
							HelpInfo = null
						}).ToList();
					SortCharacterList(SortType, IsSortDescend);
					MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQFriendGetList());
				}
				else
				{
					Action<Code> onRetrieveWantedInfoEvent = this.OnRetrieveWantedInfoEvent;
					if (onRetrieveWantedInfoEvent != null)
					{
						onRetrieveWantedInfoEvent(code);
					}
				}
			}
			else
			{
				HandleErrorCode("OnRetrieveWantedInfoRes", code);
				Action<Code> onRetrieveWantedInfoEvent2 = this.OnRetrieveWantedInfoEvent;
				if (onRetrieveWantedInfoEvent2 != null)
				{
					onRetrieveWantedInfoEvent2(code);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[OnRetrieveWantedInfoRes] Excetpion : " + ex.Message);
			Debug.LogError("[OnRetrieveWantedInfoRes] StackTrace : " + ex.StackTrace);
			GuildUIHelper.ForceUnlockState();
		}
	}

	private void OnWantedStartRes(WantedStartRes resp)
	{
		Debug.Log("[OnWantedStartRes]");
		Code code = (Code)resp.Code;
		if (code == Code.WANTED_START_SUCCESS)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
			WantedMainUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<WantedMainUI>("UI_WantedMain");
			if ((bool)uI)
			{
				uI.bFirstMute = true;
			}
			RefreshWantedInfo(resp.WantedInfo);
			UsedHelpCount = resp.HelpCount;
			Action<Code> onWantedStartEvent = this.OnWantedStartEvent;
			if (onWantedStartEvent != null)
			{
				onWantedStartEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnWantedStartRes", code);
			Action<Code> onWantedStartEvent2 = this.OnWantedStartEvent;
			if (onWantedStartEvent2 != null)
			{
				onWantedStartEvent2(code);
			}
		}
	}

	private void OnReceiveWantedRes(ReceiveWantedRes resp)
	{
		Debug.Log("[OnReceiveWantedRes]");
		Code code = (Code)resp.Code;
		if (code == Code.WANTED_RECEIVE_SUCCESS)
		{
			RefreshWantedInfo(resp.WantedInfo);
			Singleton<GuildSystem>.Instance.GuildInfoCache.Score = resp.GuildScore;
			Action<Code, List<NetRewardInfo>, WantedSuccessType> onReceiveWantedEvent = this.OnReceiveWantedEvent;
			if (onReceiveWantedEvent != null)
			{
				onReceiveWantedEvent(code, resp.RewardEntities.RewardList, (WantedSuccessType)resp.SuccessType);
			}
		}
		else
		{
			HandleErrorCode("OnReceiveWantedRes", code);
			Action<Code, List<NetRewardInfo>, WantedSuccessType> onReceiveWantedEvent2 = this.OnReceiveWantedEvent;
			if (onReceiveWantedEvent2 != null)
			{
				onReceiveWantedEvent2(code, null, WantedSuccessType.NormalSuccess);
			}
		}
	}

	private void OnRefreshWantedSlotRes(RefreshWantedSlotRes resp)
	{
		Debug.Log("[OnRefreshWantedSlotRes]");
		Code code = (Code)resp.Code;
		if (code == Code.WANTED_REFRESH_SLOT_SUCCESS)
		{
			RefreshWantedInfo(resp.WantedInfo);
			Action<Code> onRefreshWantedSlotEvent = this.OnRefreshWantedSlotEvent;
			if (onRefreshWantedSlotEvent != null)
			{
				onRefreshWantedSlotEvent(code);
			}
		}
		else
		{
			HandleErrorCode("OnRefreshWantedSlotRes", code);
			Action<Code> onRefreshWantedSlotEvent2 = this.OnRefreshWantedSlotEvent;
			if (onRefreshWantedSlotEvent2 != null)
			{
				onRefreshWantedSlotEvent2(code);
			}
		}
	}

	private void OnRefreshFriendList(object obj)
	{
		Debug.Log("[OnRefreshFriendList]");
		if (!_loadUIAfterGetWantedInfo)
		{
			return;
		}
		try
		{
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
			if (!(obj is RSFriendGetList))
			{
				Debug.LogError("obj is not RSFriendGetList");
				TriggerWantedInfoEvent();
				return;
			}
			Code result = (Code)((RSFriendGetList)obj).Result;
			if (result == Code.COMMUNITY_FRIEND_GET_LIST_SUCCESS)
			{
				_friendInfoList.Clear();
				SortedFriendInfoList.Clear();
				_retrieveFriendList = MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.Keys.ToArray();
				if (_retrieveFriendList.Length != 0)
				{
					MonoBehaviourSingleton<UIManager>.Instance.Connecting(true);
					MonoBehaviourSingleton<CCSocketClient>.Instance.SendProtocol(FlatBufferCCHelper.CreateRQGetPlayerCharacterInfoList(_retrieveFriendList));
				}
				else
				{
					TriggerWantedInfoEvent();
				}
			}
			else
			{
				Debug.LogError(string.Format("[{0}] Code = {1}", "OnRefreshFriendList", result));
				TriggerWantedInfoEvent();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[OnRefreshFriendList] Excetpion : " + ex.Message);
			Debug.LogError("[OnRefreshFriendList] StackTrace : " + ex.StackTrace);
			GuildUIHelper.ForceUnlockState();
		}
	}

	private void OnGetPlayerCharacterInfoList(object obj)
	{
		Debug.Log("[OnGetPlayerCharacterInfoList]");
		try
		{
			MonoBehaviourSingleton<UIManager>.Instance.Connecting(false);
			if (!(obj is RSGetPlayerCharacterInfoList))
			{
				Debug.LogError("obj is not RSGetPlayerCharacterInfoList");
				TriggerWantedInfoEvent();
				return;
			}
			RSGetPlayerCharacterInfoList rSGetPlayerCharacterInfoList = (RSGetPlayerCharacterInfoList)obj;
			Code result = (Code)rSGetPlayerCharacterInfoList.Result;
			if (result == Code.COMMUNITY_GET_PLAYER_CHARACTER_INFO_LIST_SUCCESS)
			{
				for (int i = 0; i < rSGetPlayerCharacterInfoList.CharacterInfoJSONListLength; i++)
				{
					string text = _retrieveFriendList[i];
					SocketFriendInfo value;
					if (!MonoBehaviourSingleton<OrangeCommunityManager>.Instance.dicFriend.TryGetValue(text, out value))
					{
						Debug.LogError("No FriendInfo of PlayerId : " + text + " now");
						continue;
					}
					byte[] bytes = LZ4Helper.DecodeWithHeader(Convert.FromBase64String(rSGetPlayerCharacterInfoList.CharacterInfoJSONList(i)));
					Dictionary<int, CharacterInfo> dictionary = JsonConvert.DeserializeObject<Dictionary<int, CharacterInfo>>(Encoding.UTF8.GetString(bytes));
					SocketPlayerHUD socketPlayerHUD = JsonHelper.Deserialize<SocketPlayerHUD>(value.FriendPlayerHUD);
					CharacterInfo value2;
					if (dictionary.TryGetValue(socketPlayerHUD.m_StandbyCharID, out value2))
					{
						WantedMemberInfo item = new WantedMemberInfo
						{
							CharacterInfo = value2.netInfo,
							PlayerHUD = socketPlayerHUD,
							HelpInfo = new NetWantedHelpInfo
							{
								PlayerID = socketPlayerHUD.m_PlayerId,
								CharacterID = socketPlayerHUD.m_StandbyCharID,
								CharacterStar = value2.netInfo.Star
							}
						};
						_friendInfoList.Add(item);
					}
					else
					{
						Debug.LogError(string.Format("No CharacterInfo of StandByCharID : {0}, PlayerId : {1}", socketPlayerHUD.m_StandbyCharID, socketPlayerHUD.m_PlayerId));
					}
				}
				TriggerWantedInfoEvent();
			}
			else
			{
				Debug.LogError(string.Format("[{0}] Code = {1}", "OnGetPlayerCharacterInfoList", result));
				TriggerWantedInfoEvent();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[OnGetPlayerCharacterInfoList] Excetpion : " + ex.Message);
			Debug.LogError("[OnGetPlayerCharacterInfoList] StackTrace : " + ex.StackTrace);
			GuildUIHelper.ForceUnlockState();
		}
	}

	public void RefreshSortedCharacterList()
	{
		_sortedCandidateCharacterList = (from info in SortedCharacterInfoList
			select info.CharacterInfo into characterInfo
			orderby characterInfo.Star, characterInfo.CharacterID
			select characterInfo).ToList();
	}

	public bool TryGetRecommendedMemberList(WANTED_TABLE wantedAttrData, out List<NetCharacterInfo> characterInfoList, out WantedConditionFlag conditionFlag, out int conditionLevel)
	{
		characterInfoList = new List<NetCharacterInfo>();
		List<NetCharacterInfo> candidateCharacterList = _sortedCandidateCharacterList.ToList();
		candidateCharacterList.RemoveAll((NetCharacterInfo characterInfo) => DeparturedCharacterCacheList.Contains(characterInfo.CharacterID));
		if (candidateCharacterList.Count <= 0)
		{
			conditionFlag = WantedConditionFlag.None;
			conditionLevel = 0;
			return false;
		}
		List<WantedAttrDataParam> attrDataParams = new List<WantedAttrDataParam>
		{
			new WantedAttrDataParam(wantedAttrData.n_BASIS_EFFECT, wantedAttrData.n_EFFECT_X, wantedAttrData.n_EFFECT_Y),
			new WantedAttrDataParam(wantedAttrData.n_EXTRA_1, wantedAttrData.n_EXTRAX_1, wantedAttrData.n_EXTRAY_1),
			new WantedAttrDataParam(wantedAttrData.n_EXTRA_2, wantedAttrData.n_EXTRAX_2, wantedAttrData.n_EXTRAY_2),
			new WantedAttrDataParam(wantedAttrData.n_EXTRA_3, wantedAttrData.n_EXTRAX_3, wantedAttrData.n_EXTRAY_3)
		};
		List<WantedRecommendedResult> list = PickingSequenceList.Select((List<int> pickupSequence) => new WantedRecommendedResult
		{
			PickingSequence = pickupSequence.ToList(),
			CandidateCharacterList = candidateCharacterList.ToList(),
			CharacterInfoList = new List<NetCharacterInfo>()
		}).ToList();
		foreach (WantedRecommendedResult item in list)
		{
			PickMemberBySequence(item, attrDataParams);
			CheckConditionFlag(wantedAttrData, item.CharacterInfoList, out item.ConditionFlag, out item.ConditionLevel);
		}
		list.RemoveAll((WantedRecommendedResult recommendedResult) => !recommendedResult.ConditionFlag.HasFlag(WantedConditionFlag.BasicCondition));
		if (list.Count == 0)
		{
			conditionFlag = WantedConditionFlag.None;
			conditionLevel = 0;
			return false;
		}
		int maxConditionLevel = list.Max((WantedRecommendedResult recommendedResult) => recommendedResult.ConditionLevel);
		list.RemoveAll((WantedRecommendedResult recommendedResult) => recommendedResult.ConditionLevel < maxConditionLevel);
		list = list.OrderBy((WantedRecommendedResult recommendedResult) => recommendedResult.CharacterInfoList.Max((NetCharacterInfo characterInfo) => characterInfo.Star)).ToList();
		if (list.Count > 0)
		{
			sbyte minMaxCharacterStar = list[0].CharacterInfoList.Max((NetCharacterInfo characterInfo) => characterInfo.Star);
			list.RemoveAll((WantedRecommendedResult recommendedResult) => recommendedResult.CharacterInfoList.Max((NetCharacterInfo characterInfo) => characterInfo.Star) > minMaxCharacterStar);
		}
		list = list.OrderBy((WantedRecommendedResult recommendedResult) => recommendedResult.CharacterInfoList.Count).ToList();
		if (list.Count > 0)
		{
			int minCharacterCount = list[0].CharacterInfoList.Count;
			list.RemoveAll((WantedRecommendedResult recommendedResult) => recommendedResult.CharacterInfoList.Count > minCharacterCount);
		}
		if (list.Count == 0)
		{
			conditionFlag = WantedConditionFlag.None;
			conditionLevel = 0;
			return false;
		}
		int index = OrangeBattleUtility.Random(0, list.Count);
		WantedRecommendedResult wantedRecommendedResult = list[index];
		characterInfoList.AddRange(wantedRecommendedResult.CharacterInfoList);
		conditionFlag = wantedRecommendedResult.ConditionFlag;
		conditionLevel = wantedRecommendedResult.ConditionLevel;
		return true;
	}

	private void PickMemberBySequence(WantedRecommendedResult recommendedResult, List<WantedAttrDataParam> attrDataParams)
	{
		List<int> pickingSequence = recommendedResult.PickingSequence;
		while (pickingSequence.Count > 0)
		{
			int num = pickingSequence[0];
			pickingSequence.RemoveAt(0);
			switch ((PickingType)num)
			{
			case PickingType.ByCharacterId:
				foreach (WantedAttrDataParam attrDataParam in attrDataParams)
				{
					PickMemberByNStarByCharacterId(recommendedResult.CandidateCharacterList, recommendedResult.CharacterInfoList, attrDataParam.Condition, attrDataParam.ParamX, attrDataParam.ParamY);
				}
				break;
			case PickingType.ByCharacterCount:
				attrDataParams = (from attrDataParam in attrDataParams
					orderby attrDataParam.ParamX descending, attrDataParam.ParamY descending
					select attrDataParam).ToList();
				foreach (WantedAttrDataParam attrDataParam2 in attrDataParams)
				{
					PickMemberByNStarCharacterCount(recommendedResult.CandidateCharacterList, recommendedResult.CharacterInfoList, attrDataParam2.Condition, attrDataParam2.ParamX, attrDataParam2.ParamY);
				}
				break;
			case PickingType.ByTotalStarCount:
				attrDataParams = attrDataParams.OrderByDescending((WantedAttrDataParam attrDataParam) => attrDataParam.ParamX).ToList();
				foreach (WantedAttrDataParam attrDataParam3 in attrDataParams)
				{
					PickMemberByTotalStarCount(recommendedResult.CandidateCharacterList, recommendedResult.CharacterInfoList, attrDataParam3.Condition, attrDataParam3.ParamX, attrDataParam3.ParamY);
				}
				break;
			case PickingType.ByGoCharacterCount:
				attrDataParams = attrDataParams.OrderByDescending((WantedAttrDataParam attrDataParam) => attrDataParam.ParamX).ToList();
				foreach (WantedAttrDataParam attrDataParam4 in attrDataParams)
				{
					PickMemberByGoCharacterCount(recommendedResult.CandidateCharacterList, recommendedResult.CharacterInfoList, attrDataParam4.Condition, attrDataParam4.ParamX, attrDataParam4.ParamY);
				}
				break;
			}
		}
	}

	private void PickMemberByNStarByCharacterId(List<NetCharacterInfo> characterInfoList, List<NetCharacterInfo> pickedCharacterList, int condition, int paramX, int paramY)
	{
		if (condition != 3)
		{
			return;
		}
		int num = 1 - pickedCharacterList.Count((NetCharacterInfo characterInfo) => characterInfo.CharacterID == paramY && characterInfo.Star >= paramX);
		if (num == 0 || pickedCharacterList.Count + num > 4)
		{
			return;
		}
		List<NetCharacterInfo> resultList = characterInfoList.Where((NetCharacterInfo characterInfo) => characterInfo.CharacterID == paramY && characterInfo.Star >= paramX).ToList();
		if (resultList.Count >= num)
		{
			resultList = resultList.GetRange(0, num);
			pickedCharacterList.AddRange(resultList);
			characterInfoList.RemoveAll((NetCharacterInfo characterInfo) => resultList.Contains(characterInfo));
		}
	}

	private void PickMemberByNStarCharacterCount(List<NetCharacterInfo> characterInfoList, List<NetCharacterInfo> pickedCharacterList, int condition, int paramX, int paramY)
	{
		if (condition != 1)
		{
			return;
		}
		int num = paramY - pickedCharacterList.Count((NetCharacterInfo characterInfo) => characterInfo.Star >= paramX);
		if (num <= 0 || pickedCharacterList.Count + num > 4)
		{
			return;
		}
		List<NetCharacterInfo> resultList = characterInfoList.Where((NetCharacterInfo characterInfo) => characterInfo.Star >= paramX).ToList();
		if (resultList.Count >= num)
		{
			resultList = resultList.GetRange(0, num);
			pickedCharacterList.AddRange(resultList);
			characterInfoList.RemoveAll((NetCharacterInfo characterInfo) => resultList.Contains(characterInfo));
		}
	}

	private void PickMemberByTotalStarCount(List<NetCharacterInfo> characterInfoList, List<NetCharacterInfo> pickedCharacterList, int condition, int paramX, int paramY)
	{
		if (condition != 2)
		{
			return;
		}
		int num = 4 - pickedCharacterList.Count;
		if (num == 0)
		{
			return;
		}
		int num2 = paramX - pickedCharacterList.Sum((NetCharacterInfo characterInfo) => characterInfo.Star);
		if (num2 <= 0 || characterInfoList.Sum((NetCharacterInfo characterInfo) => characterInfo.Star) < num2)
		{
			return;
		}
		List<NetCharacterInfo> list = characterInfoList.OrderByDescending((NetCharacterInfo c) => c.Star).ToList();
		int num3 = 0;
		List<NetCharacterInfo> resultList = new List<NetCharacterInfo>();
		foreach (NetCharacterInfo item in list)
		{
			num3 += item.Star;
			resultList.Add(item);
			if (resultList.Count >= num || num3 >= num2)
			{
				break;
			}
		}
		if (num3 >= num2)
		{
			pickedCharacterList.AddRange(resultList);
			characterInfoList.RemoveAll((NetCharacterInfo characterInfo) => resultList.Contains(characterInfo));
		}
	}

	private void PickMemberByGoCharacterCount(List<NetCharacterInfo> characterInfoList, List<NetCharacterInfo> pickedCharacterList, int condition, int paramX, int paramY)
	{
		if (condition != 4 || paramX > 4)
		{
			return;
		}
		int num = paramX - pickedCharacterList.Count;
		if (num > 0 && characterInfoList.Count >= num)
		{
			List<NetCharacterInfo> resultList = characterInfoList.GetRange(0, num);
			pickedCharacterList.AddRange(resultList);
			characterInfoList.RemoveAll((NetCharacterInfo characterInfo) => resultList.Contains(characterInfo));
		}
	}

	public void CheckConditionFlag(WANTED_TABLE wantedAttrData, List<NetCharacterInfo> characterInfoList, out WantedConditionFlag conditionFlag, out int conditionLevel)
	{
		conditionLevel = 0;
		conditionFlag = WantedConditionFlag.None;
		if (IsCoditionMatched(wantedAttrData.n_BASIS_EFFECT, wantedAttrData.n_EFFECT_X, wantedAttrData.n_EFFECT_Y, characterInfoList))
		{
			conditionFlag |= WantedConditionFlag.BasicCondition;
			conditionLevel++;
		}
		if (IsCoditionMatched(wantedAttrData.n_EXTRA_1, wantedAttrData.n_EXTRAX_1, wantedAttrData.n_EXTRAY_1, characterInfoList))
		{
			conditionFlag |= WantedConditionFlag.BonusCondition1;
			conditionLevel++;
		}
		if (IsCoditionMatched(wantedAttrData.n_EXTRA_2, wantedAttrData.n_EXTRAX_2, wantedAttrData.n_EXTRAY_2, characterInfoList))
		{
			conditionFlag |= WantedConditionFlag.BonusCondition2;
			conditionLevel++;
		}
		if (IsCoditionMatched(wantedAttrData.n_EXTRA_3, wantedAttrData.n_EXTRAX_3, wantedAttrData.n_EXTRAY_3, characterInfoList))
		{
			conditionFlag |= WantedConditionFlag.BonusCondition3;
			conditionLevel++;
		}
	}

	private bool IsCoditionMatched(int condition, int paramX, int paramY, List<NetCharacterInfo> characterInfoList)
	{
		switch ((WantedGoCondition)(short)condition)
		{
		case WantedGoCondition.NStarCharacterCount:
			return characterInfoList.Count((NetCharacterInfo characterInfo) => characterInfo.Star >= paramX) >= paramY;
		case WantedGoCondition.TotalStarCount:
			return characterInfoList.Sum((NetCharacterInfo characterInfo) => characterInfo.Star) >= paramX;
		case WantedGoCondition.NStarByCharacterId:
		{
			List<NetCharacterInfo> list = characterInfoList.Where((NetCharacterInfo characterInfo) => characterInfo.CharacterID == paramY).ToList();
			if (list.Count > 0)
			{
				return list.Max((NetCharacterInfo characterInfo) => characterInfo.Star) >= paramX;
			}
			return false;
		}
		case WantedGoCondition.GoCharacterCount:
			return characterInfoList.Count >= paramX;
		default:
			return false;
		}
	}

	public void SortCharacterList(CharacterHelper.SortType sortType, bool isDescend)
	{
		switch (sortType)
		{
		case CharacterHelper.SortType.RARITY:
			if (isDescend)
			{
				SortedCharacterInfoList = (from info in _characterInfoList
					orderby ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[info.CharacterInfo.CharacterID].n_RARITY descending, info.CharacterInfo.CharacterID
					select info).ToList();
			}
			else
			{
				SortedCharacterInfoList = (from info in _characterInfoList
					orderby ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[info.CharacterInfo.CharacterID].n_RARITY, info.CharacterInfo.CharacterID descending
					select info).ToList();
			}
			break;
		case CharacterHelper.SortType.STAR:
			if (isDescend)
			{
				SortedCharacterInfoList = (from info in _characterInfoList
					orderby info.CharacterInfo.Star descending, info.CharacterInfo.CharacterID
					select info).ToList();
			}
			else
			{
				SortedCharacterInfoList = (from info in _characterInfoList
					orderby info.CharacterInfo.Star, info.CharacterInfo.CharacterID descending
					select info).ToList();
			}
			break;
		}
	}

	public void SortFriendCharacterList(CharacterHelper.SortType sortType, bool isDescend)
	{
		switch (sortType)
		{
		case CharacterHelper.SortType.RARITY:
			if (isDescend)
			{
				SortedFriendInfoList = (from info in _friendInfoList
					orderby ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[info.CharacterInfo.CharacterID].n_RARITY descending, info.CharacterInfo.CharacterID
					select info).ToList();
			}
			else
			{
				SortedFriendInfoList = (from info in _friendInfoList
					orderby ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[info.CharacterInfo.CharacterID].n_RARITY, info.CharacterInfo.CharacterID descending
					select info).ToList();
			}
			break;
		case CharacterHelper.SortType.STAR:
			if (isDescend)
			{
				SortedFriendInfoList = (from info in _friendInfoList
					orderby info.CharacterInfo.Star descending, info.CharacterInfo.CharacterID
					select info).ToList();
			}
			else
			{
				SortedFriendInfoList = (from info in _friendInfoList
					orderby info.CharacterInfo.Star, info.CharacterInfo.CharacterID descending
					select info).ToList();
			}
			break;
		}
	}

	private void RefreshWantedInfo(NetWantedInfo wantedInfo)
	{
		List<NetWantedInfo> list = WantedTargetInfoCacheList.Select((WantedTargetInfo info) => info.WantedInfo).ToList();
		int num = list.FindIndex((NetWantedInfo info) => info.Slot == wantedInfo.Slot);
		if (num >= 0)
		{
			list.RemoveAt(num);
			list.Insert(num, wantedInfo);
			RefreshWantedInfo(list);
		}
	}

	private void RefreshWantedInfo(List<NetWantedInfo> wantedInfoList)
	{
		List<WantedTargetInfo> list = new List<WantedTargetInfo>();
		for (int i = 0; i < wantedInfoList.Count; i++)
		{
			NetWantedInfo netWantedInfo = wantedInfoList[i];
			WANTED_TABLE value;
			if (!ManagedSingleton<OrangeDataManager>.Instance.WANTED_TABLE_DICT.TryGetValue(netWantedInfo.WantedID, out value))
			{
				Debug.LogError(string.Format("Invalid WantedID {0} of {1}", netWantedInfo.WantedID, "WANTED_TABLE_DICT"));
				continue;
			}
			WANTED_SUCCESS_TABLE value2;
			if (!ManagedSingleton<OrangeDataManager>.Instance.WANTED_SUCCESS_TABLE_DICT.TryGetValue(value.n_WANTED_SUCCESS, out value2))
			{
				Debug.LogError(string.Format("Invalid WantedSuccessID {0} of {1}", value.n_WANTED_SUCCESS, "WANTED_SUCCESS_TABLE_DICT"));
				continue;
			}
			list.Add(new WantedTargetInfo
			{
				WantedInfo = netWantedInfo,
				WantedAttrData = value,
				SuccessAttrData = value2
			});
		}
		WantedTargetInfoCacheList = list;
		DeparturedCharacterCacheList = wantedInfoList.SelectMany((NetWantedInfo wantedInfo) => wantedInfo.CharacterIDs).ToList();
		WantedHelpInfoCacheList = (from wantedInfo in wantedInfoList
			where wantedInfo.WantedHelpInfo != null
			select wantedInfo.WantedHelpInfo).ToList();
	}

	private void HandleErrorCode(string method, int ackCode)
	{
		HandleErrorCode(method, (Code)ackCode);
	}

	private void HandleErrorCode(string method, Code ackCode)
	{
		switch (ackCode)
		{
		case Code.WANTED_CHARACTER_USED_FAIL:
		case Code.WANTED_HELP_CHARACTER_USED_FAIL:
		case Code.WANTED_GO_CONDITION_1_FAIL:
		case Code.WANTED_GO_CONDITION_2_FAIL:
		case Code.WANTED_GO_CONDITION_3_FAIL:
		case Code.WANTED_GO_CONDITION_4_FAIL:
		case Code.WANTED_RECEIVED_FAIL:
		case Code.WANTED_STARTED_FAIL:
		case Code.WANTED_HELP_CHARACTER_STAR_COUNT_FAIL:
		case Code.WANTED_START_STAR_NOT_ENOUGH_FAIL:
		case Code.WANTED_START_HELP_COUNT_NOT_ENOUGH_FAIL:
		case Code.WANTED_START_PLAYERAP_NOT_ENOUGH:
		case Code.WANTED_FINISH_TIME_ERROR:
			Debug.LogError(string.Format("[{0}] Unhandled Error Code : {1}", method, ackCode));
			CommonUIHelper.ShowCommonTipUI(string.Format("ERROR : {0}", (int)ackCode), false);
			break;
		case Code.GUILD_MEMBER_MAX:
			Debug.Log(string.Format("[{0}] Customs Handling Error Code : {1}", method, ackCode));
			break;
		default:
			Debug.LogError(string.Format("[{0}] Unhandled Error Code : {1}", method, ackCode));
			break;
		}
	}
}
