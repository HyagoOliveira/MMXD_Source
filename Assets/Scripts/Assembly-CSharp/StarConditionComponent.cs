#define RELEASE
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class StarConditionComponent : MonoBehaviour
{
	private string conditionFormat = "{0}/{1}";

	private int[,] iconIdx = new int[5, 5]
	{
		{ 1, 1, 1, 1, 1 },
		{ 1, 2, 1, 1, 1 },
		{ 1, 3, 5, 1, 1 },
		{ 2, 3, 4, 5, 1 },
		{ 1, 2, 3, 4, 5 }
	};

	private float progressDefaultX = 14f;

	private float progressDefaultY = 40.1f;

	private float progressSpacingX = 191.5f;

	[SerializeField]
	private Text conditionTitle;

	[SerializeField]
	private Text conditionValueNow;

	[SerializeField]
	private StarConditionComponentUnit unit;

	[SerializeField]
	private RectTransform unitParent;

	[SerializeField]
	private Image imgProgressBg;

	[SerializeField]
	private Image imgProgressFg;

	[SerializeField]
	private Image imgMainStyle;

	private List<MISSION_TABLE> listMissionTable;

	private List<StarConditionComponentUnit> listUnits = new List<StarConditionComponentUnit>();

	private int conditionMax;

	private int conditionValue;

	private int fillterID;

	private Callback retrievedRewardCB;

	private int condition;

	private int fillter;

	private int difficulty;

	private bool isCallingApi;

	public void GetStyleChangeByConditionType(int conditionType, int difficulty, int fillterID, out string title, out int value, out string srcPath)
	{
		title = (srcPath = string.Empty);
		value = 0;
		MissionCondition missionCondition = (MissionCondition)conditionType;
		if (missionCondition != MissionCondition.ActivityReached)
		{
			MissionCondition missionCondition2 = missionCondition - 1001;
			int num = 1;
			value = GetStageConditionValue(fillterID, difficulty);
			srcPath = "UI_SubCommon_star_00";
		}
		else
		{
			title = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("MISSION_ACTIVITY_NAME");
			value = ManagedSingleton<MissionHelper>.Instance.GetActivityValue(MissionType.Daily);
			srcPath = "UI_SubCommon_Dailymission_icon";
		}
	}

	public void Setup(int p_condition, int p_fillter, int p_difficulty = 0, Callback p_cb = null)
	{
		condition = p_condition;
		fillter = p_fillter;
		difficulty = p_difficulty;
		retrievedRewardCB = p_cb;
		foreach (StarConditionComponentUnit listUnit in listUnits)
		{
			Object.Destroy(listUnit.gameObject);
		}
		listUnits.Clear();
		if (p_difficulty == 0)
		{
			listMissionTable = ManagedSingleton<OrangeTableHelper>.Instance.GetListMissionByCondition(p_condition, p_fillter);
		}
		else
		{
			listMissionTable = ManagedSingleton<OrangeTableHelper>.Instance.GetListMissionByConditionDifficulty(p_condition, p_fillter, p_difficulty);
		}
		int count = listMissionTable.Count;
		if (listMissionTable != null && count > 0)
		{
			base.gameObject.SetActive(true);
			string title;
			string srcPath;
			GetStyleChangeByConditionType(p_condition, p_difficulty, p_fillter, out title, out conditionValue, out srcPath);
			SetUnit();
			conditionTitle.text = title;
			conditionValueNow.text = string.Format(conditionFormat, conditionValue, conditionMax);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_sub_common, srcPath, delegate(Sprite obj)
			{
				imgMainStyle.sprite = obj;
				imgMainStyle.color = Color.white;
			});
			imgProgressBg.rectTransform.sizeDelta = new Vector2(progressDefaultX + progressSpacingX * (float)count, progressDefaultY);
			imgProgressFg.fillAmount = Mathf.Clamp01((float)conditionValue / (float)conditionMax);
		}
		else
		{
			Debug.LogError("Can't find listMissionTable from ID:" + p_fillter);
			base.gameObject.SetActive(false);
		}
	}

	private int GetStageConditionValue(int p_fillter, int p_difficulty)
	{
		List<NetStageInfo> listNetStageByMainId = ManagedSingleton<StageHelper>.Instance.GetListNetStageByMainId(p_fillter);
		int num = 0;
		if (listNetStageByMainId == null)
		{
			return num;
		}
		foreach (NetStageInfo item in listNetStageByMainId)
		{
			STAGE_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(item.StageID, out value))
			{
				if (p_difficulty == 0)
				{
					num += ManagedSingleton<StageHelper>.Instance.GetStarAmount(item.Star);
				}
				else if (value.n_DIFFICULTY == p_difficulty)
				{
					num += ManagedSingleton<StageHelper>.Instance.GetStarAmount(item.Star);
				}
			}
		}
		return num;
	}

    [System.Obsolete]
    private void SetUnit()
	{
		listUnits = new List<StarConditionComponentUnit>();
		conditionMax = 0;
		int num = listMissionTable.Count - 1;
		MISSION_TABLE mISSION_TABLE = null;
		MissionInfo value = null;
		StarConditionComponentUnit.RewardState rewardState = StarConditionComponentUnit.RewardState.NOT_REACHED;
		for (int i = 0; i < listMissionTable.Count; i++)
		{
			StarConditionComponentUnit starConditionComponentUnit = Object.Instantiate(unit, unitParent);
			listUnits.Add(starConditionComponentUnit);
			mISSION_TABLE = listMissionTable[i];
			if (conditionMax < mISSION_TABLE.n_CONDITION_Y)
			{
				conditionMax = mISSION_TABLE.n_CONDITION_Y;
			}
			rewardState = StarConditionComponentUnit.RewardState.NOT_REACHED;
			CallbackIdx p_cb = OnClickRewardReq;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicMission.TryGetValue(mISSION_TABLE.n_ID, out value))
			{
				if (value.netMissionInfo.Received != 0)
				{
					rewardState = StarConditionComponentUnit.RewardState.RECEIVED;
					p_cb = OnClickRewardPopup;
				}
			}
			else if (conditionValue >= mISSION_TABLE.n_CONDITION_Y)
			{
				rewardState = StarConditionComponentUnit.RewardState.REACH;
				p_cb = OnClickRewardReq;
			}
			else
			{
				p_cb = OnClickRewardPopup;
			}
			starConditionComponentUnit.Setup(i, iconIdx[num, i], mISSION_TABLE.n_CONDITION_Y, rewardState, p_cb);
		}
	}

	private void OnClickRewardPopup(int idx)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_StarConditionPopup", delegate(StarConditionPopupUI ui)
		{
			ui.Setup(listMissionTable[idx], listUnits[idx].rewardState == StarConditionComponentUnit.RewardState.RECEIVED);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		});
	}

	private void OnClickRewardReq(int idx)
	{
		if (isCallingApi)
		{
			return;
		}
		isCallingApi = true;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ReceiveMissionRewardReq(listMissionTable[idx].n_ID, delegate(object p_param)
		{
			List<NetRewardInfo> rewardList = p_param as List<NetRewardInfo>;
			if (rewardList != null && rewardList.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
				{
					ui.Setup(rewardList, 0.3f);
				});
			}
			retrievedRewardCB.CheckTargetToInvoke();
			listUnits[idx].UpdateRewardState(StarConditionComponentUnit.RewardState.RECEIVED);
			Setup(condition, fillter, difficulty, retrievedRewardCB);
			isCallingApi = false;
		});
	}
}
