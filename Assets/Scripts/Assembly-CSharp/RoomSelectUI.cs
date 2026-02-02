#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using cm;

public class RoomSelectUI : OrangeUIBase
{
	public enum RoomSelectTab
	{
		NoramlPlay = 0
	}

	private RoomSelectTab roomSelectTab;

	private List<StorageInfo> listStorage = new List<StorageInfo>();

	[SerializeField]
	private ScrollRect storageRect;

	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private RoomSelectUIUnit roomSelectUIUnit;

	[SerializeField]
	private OrangeText textRewardCount;

	[SerializeField]
	private GameObject channelObj;

	[SerializeField]
	private OrangeText textChannel;

	[SerializeField]
	private Button btnRefreshAll;

	[SerializeField]
	private Canvas canvasNoResultMsg;

	public List<RoomData> listRoomData = new List<RoomData>();

	private List<STAGE_TABLE> listStageData;

	private int nowSelectIdx;

	private OrangeBgExt m_bgExt;

	public STAGE_TABLE GetSelectStage
	{
		get
		{
			return listStageData[nowSelectIdx];
		}
	}

	public bool IsPublic { get; set; }

	public string Condition { get; set; }

	public string Capacity { get; set; }

	protected override void Awake()
	{
		base.Awake();
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVEPrepareRoomList, OnRSPVEPrepareRoomList);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CM.RSPVEPrepareRoomListV2, OnRSPVEPrepareRoomList);
		backToHometopCB = (Callback)Delegate.Combine(backToHometopCB, new Callback(Clear));
		if ((bool)btnRefreshAll)
		{
			btnRefreshAll.onClick.AddListener(OnClickFindAll);
		}
	}

	public void Setup(List<STAGE_TABLE> p_listStageData, int p_selectIdx)
	{
		m_bgExt = Background as OrangeBgExt;
		listStageData = p_listStageData;
		nowSelectIdx = p_selectIdx;
		textRewardCount.text = string.Format("{0}/{1}", ManagedSingleton<StageHelper>.Instance.GetCoopRewardCount(), OrangeConst.CORP_REWARD_COUNT);
		StorageInfo storageInfo = new StorageInfo("STAGE_CORP_NORAMAL", false, listStageData.Count, OnClickTab);
		for (int i = 0; i < listStageData.Count; i++)
		{
			StorageInfo storageInfo2 = new StorageInfo(listStageData[i].w_NAME, false, 0, OnClickTab);
			storageInfo2.Refl10nTable = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT;
			storageInfo.Sub[i] = storageInfo2;
			storageInfo.Sub[i].Param = new object[2]
			{
				RoomSelectTab.NoramlPlay,
				i
			};
		}
		listStorage.Add(storageInfo);
		StorageGenerator.Load("StorageComp00", listStorage, 0, nowSelectIdx, storageRect.transform, delegate(GameObject storage)
		{
			storageRect.content = storage.GetComponent<RectTransform>();
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
			float y = storageRect.content.anchoredPosition.y - base.transform.InverseTransformPoint(storageRect.content.GetChild(1).GetChild(1).GetChild(nowSelectIdx + 1)
				.position).y;
				storageRect.content.anchoredPosition = new Vector2(storageRect.content.anchoredPosition.x, y);
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
				{
					MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
					OnClickBtnRefresh();
				});
			});
			channelObj.SetActive(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.Community);
		}

		private void OnClickTab(object p_param)
		{
			StorageInfo storageInfo = (StorageInfo)p_param;
			RoomSelectTab roomSelectTab = (RoomSelectTab)storageInfo.Param[0];
			nowSelectIdx = (int)storageInfo.Param[1];
			OnClickBtnRefresh();
		}

		private void OnClickFindAll()
		{
			btnRefreshAll.interactable = false;
			Invoke("ReleaseRefreshBtn", 5f);
			List<short> list = new List<short>();
			List<int> list2 = new List<int>();
			foreach (STAGE_TABLE listStageDatum in listStageData)
			{
				list.Add(Convert.ToInt16(listStageDatum.n_TYPE));
				list2.Add(listStageDatum.n_ID);
			}
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.SearchCoopRoomListV2(list, list2);
		}

		private void ReleaseRefreshBtn()
		{
			if ((bool)btnRefreshAll)
			{
				btnRefreshAll.interactable = true;
			}
		}

		public void UpdateTab(RoomSelectTab roomSelectTab, int nowIdx)
		{
			if (listStorage.Count > 0)
			{
				StorageInfo storageInfo = listStorage[0];
				if (nowIdx < storageInfo.Sub.Length)
				{
					StorageInfo storageInfo2 = storageInfo.Sub[nowIdx];
					storageInfo2.ClickCb.CheckTargetToInvoke(storageInfo2);
				}
			}
		}

		public void OnClickBtnRefresh()
		{
			STAGE_TABLE sTAGE_TABLE = listStageData[nowSelectIdx];
			m_bgExt.ChangeBackground(sTAGE_TABLE.s_BG);
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.SearchCoopRoomList(sTAGE_TABLE, OnRSPVEPrepareRoomList);
		}

		public void OnClickBtnCreateRoom()
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CreateRoom", delegate(CreateRoomUI ui)
			{
				base.IsVisible = false;
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.RoomRefreshCB = OnClickBtnRefresh;
				ui.SetupRoomTypeDouble(ref listStageData, nowSelectIdx);
			});
		}

		private void Clear()
		{
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSPVEPrepareRoomList, OnRSPVEPrepareRoomList);
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CM.RSPVEPrepareRoomListV2, OnRSPVEPrepareRoomList);
			if ((bool)btnRefreshAll)
			{
				btnRefreshAll.onClick.RemoveListener(OnClickFindAll);
			}
			CancelInvoke("ReleaseRefreshBtn");
		}

		public override void OnClickCloseBtn()
		{
			Clear();
			base.OnClickCloseBtn();
		}

		private void OnRSPVEPrepareRoomList(object res)
		{
			scrollRect.ClearCells();
			listRoomData.Clear();
			if (res is RSPVEPrepareRoomList)
			{
				RSPVEPrepareRoomList rSPVEPrepareRoomList = (RSPVEPrepareRoomList)res;
				for (int i = 0; i < rSPVEPrepareRoomList.Roomcount; i++)
				{
					RoomData item = new RoomData(rSPVEPrepareRoomList.Roomid(i), rSPVEPrepareRoomList.Leader(i), rSPVEPrepareRoomList.Condition(i), rSPVEPrepareRoomList.Roomname(i), rSPVEPrepareRoomList.Capacity(i), rSPVEPrepareRoomList.Current(i), rSPVEPrepareRoomList.Ip(i), rSPVEPrepareRoomList.Port(i));
					listRoomData.Add(item);
				}
				scrollRect.OrangeInit(roomSelectUIUnit, 5, listRoomData.Count);
				if ((bool)canvasNoResultMsg)
				{
					canvasNoResultMsg.enabled = listRoomData.Count == 0;
				}
			}
		}

		public void OnClickBtnTestCreateRoom()
		{
		}

		private void OnRSCreatePVEPrepareRoom(object res)
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnCreateRoomMainUI(res, listStageData[nowSelectIdx], OnClickBtnRefresh);
		}

		public void OnClickChannel()
		{
			Debug.Log("OnClickChannel");
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Channel", delegate(ChannelUI ui)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup();
			});
		}
	}
