using System;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomUI : OrangeUIBase
{
	public enum RoomType
	{
		DOUBLE = 0,
		COUNT = 1
	}

	private RoomType myRoomType;

	private bool isPublic = true;

	private int condition;

	private int capacity = 2;

	public Callback RoomRefreshCB;

	[SerializeField]
	private InputField inputRoomName;

	[SerializeField]
	private CreateRoomSelectionUnit selectionUnit;

	[SerializeField]
	private EnhanceScrollView enhanceType;

	[SerializeField]
	private EnhanceScrollView enhanceStage;

	[SerializeField]
	private EnhanceScrollView enhanceLvReq;

	[SerializeField]
	private Toggle togglePublic;

	[SerializeField]
	private Toggle togglePrivate;

	private List<STAGE_TABLE> listStageData;

	private List<int> listLvReq;

	private int selectStageIdx;

	private STAGE_TABLE nowStage;

	private Vector2[] unitSize = new Vector2[2]
	{
		new Vector2(380f, 111f),
		new Vector2(615f, 111f)
	};

	private bool bLockNet;

	private float fLockTime;

	private const float fLimitLockTime = 30f;

	private readonly int TARGET_COUNT = 7;

	public void SetupRoomTypeDouble(ref List<STAGE_TABLE> p_listStageData, int p_selectStageIdx)
	{
		listStageData = p_listStageData;
		selectStageIdx = p_selectStageIdx;
		nowStage = p_listStageData[selectStageIdx];
		listLvReq = GetLvReqList();
		SetDefaultRoomName();
		InitEnhanceType();
		InitEnhanceStage();
		InitEnhanceLvReq();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void InitEnhanceType()
	{
		List<CreateRoomSelectionUnit> list = new List<CreateRoomSelectionUnit>();
		string[] array = new string[1] { MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MULTIPLAYER_ROOM_TYPE_1") };
		for (int i = 0; i < 1; i++)
		{
			CreateRoomSelectionUnit createRoomSelectionUnit = UnityEngine.Object.Instantiate(selectionUnit, enhanceType.transform);
			createRoomSelectionUnit.GetComponent<RectTransform>().sizeDelta = unitSize[0];
			createRoomSelectionUnit.Init(array[i], i, SetRoomIdx);
			list.Add(createRoomSelectionUnit);
		}
		list.Reverse();
		enhanceType.startCenterIndex = list.Count - 1;
		enhanceType.listEnhanceItems = list.ConvertAll((Converter<CreateRoomSelectionUnit, EnhanceItem>)((CreateRoomSelectionUnit x) => x));
		enhanceType.Setup();
	}

	private void SetDefaultRoomName()
	{
		inputRoomName.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ROOM_TITLE"), ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname, string.Empty);
	}

	private void InitEnhanceStage()
	{
		List<CreateRoomSelectionUnit> list = new List<CreateRoomSelectionUnit>();
		STAGE_TABLE sTAGE_TABLE = null;
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		for (int i = 0; i < listStageData.Count; i++)
		{
			sTAGE_TABLE = listStageData[i];
			if (sTAGE_TABLE.n_RANK <= lV)
			{
				CreateRoomSelectionUnit createRoomSelectionUnit = UnityEngine.Object.Instantiate(selectionUnit, enhanceStage.transform);
				createRoomSelectionUnit.GetComponent<RectTransform>().sizeDelta = unitSize[1];
				createRoomSelectionUnit.Init(ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(sTAGE_TABLE.w_NAME), i, SetStageIdx);
				list.Add(createRoomSelectionUnit);
			}
		}
		list.Reverse();
		enhanceStage.startCenterIndex = list.Count - 1 - selectStageIdx;
		enhanceStage.listEnhanceItems = list.ConvertAll((Converter<CreateRoomSelectionUnit, EnhanceItem>)((CreateRoomSelectionUnit x) => x));
		enhanceStage.Setup();
	}

	private void InitEnhanceLvReq()
	{
		List<CreateRoomSelectionUnit> list = new List<CreateRoomSelectionUnit>();
		string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANK_REQUIRE");
		for (int i = 0; i < listLvReq.Count; i++)
		{
			CreateRoomSelectionUnit createRoomSelectionUnit = UnityEngine.Object.Instantiate(selectionUnit, enhanceLvReq.transform);
			createRoomSelectionUnit.GetComponent<RectTransform>().sizeDelta = unitSize[0];
			createRoomSelectionUnit.Init(str + listLvReq[i], i, SetLvIdx);
			list.Add(createRoomSelectionUnit);
		}
		list.Reverse();
		enhanceLvReq.startCenterIndex = list.Count - 1;
		enhanceLvReq.listEnhanceItems = list.ConvertAll((Converter<CreateRoomSelectionUnit, EnhanceItem>)((CreateRoomSelectionUnit x) => x));
		enhanceLvReq.Setup();
	}

	private List<int> GetLvReqList()
	{
		List<int> list = new List<int>();
		int expMaxLevel = ManagedSingleton<OrangeTableHelper>.Instance.GetExpMaxLevel();
		for (int i = nowStage.n_RANK; i <= expMaxLevel; i += 5)
		{
			list.Add(i);
		}
		return list;
	}

	private void SetRoomIdx(int idx)
	{
	}

	private void SetStageIdx(int idx)
	{
		STAGE_TABLE sTAGE_TABLE = listStageData[idx];
		if (sTAGE_TABLE.n_ID != nowStage.n_ID)
		{
			selectStageIdx = idx;
			nowStage = sTAGE_TABLE;
			listLvReq.Clear();
			listLvReq = GetLvReqList();
			int count = enhanceLvReq.listEnhanceItems.Count;
			for (int i = 0; i < count; i++)
			{
				UnityEngine.Object.Destroy(enhanceLvReq.listEnhanceItems[i].gameObject);
			}
			enhanceLvReq.listEnhanceItems.Clear();
			InitEnhanceLvReq();
		}
	}

	private void SetLvIdx(int idx)
	{
		condition = listLvReq[idx];
	}

	public void OnToggleSettingChangePublic()
	{
		if (togglePublic.isOn)
		{
			if (!isPublic)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			}
			isPublic = true;
		}
	}

	public void OnToggleSettingChangePrivate()
	{
		if (togglePrivate.isOn)
		{
			if (isPublic)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			}
			isPublic = false;
		}
	}

	public void OnClickBtnCreateRoom()
	{
		StageHelper.StageJoinCondition stageJoinCondition = StageHelper.StageJoinCondition.NONE;
		if (!ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(nowStage, ref stageJoinCondition))
		{
			ManagedSingleton<StageHelper>.Instance.DisplayConditionInfo(nowStage, stageJoinCondition);
			return;
		}
		if (inputRoomName.text.Length < 1)
		{
			SetDefaultRoomName();
		}
		CoopStageSelectUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CoopStageSelectUI>("UI_CoopStageSelectUI");
		RoomSelectUI uI2 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<RoomSelectUI>("UI_RoomSelect");
		if (null != uI)
		{
			uI.UpdateTab(selectStageIdx);
			uI.IgnoreFristSE = true;
		}
		if (null != uI2)
		{
			uI2.UpdateTab(RoomSelectUI.RoomSelectTab.NoramlPlay, selectStageIdx);
			OnCraeteRoom();
		}
		else if (!bLockNet || Time.realtimeSinceStartup - fLockTime > 30f)
		{
			bLockNet = true;
			fLockTime = Time.realtimeSinceStartup;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
				OnCraeteRoom();
				bLockNet = false;
			});
		}
		else if (bLockNet)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TIP_WAITFORMATCH"), 42);
		}
	}

	private void OnCraeteRoom()
	{
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.SingleMatch = false;
		string p_condition = string.Format("{0},{1}", condition, "");
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.CraeteCoopRoom(nowStage, isPublic, p_condition, inputRoomName.text.ToString(), 2, OnRSCreatePVEPrepareRoom);
	}

	private void OnRSCreatePVEPrepareRoom(object res)
	{
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.OnCreateRoomMainUI(res, nowStage, RoomRefreshCB);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		base.CloseSE = SystemSE.NONE;
		OnClickCloseBtn();
	}
}
