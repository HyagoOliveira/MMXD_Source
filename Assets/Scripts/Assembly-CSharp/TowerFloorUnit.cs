using UnityEngine;
using UnityEngine.UI;

internal class TowerFloorUnit : ScrollIndexCallback
{
	[SerializeField]
	private UI_Challenge parent;

	[SerializeField]
	private RectTransform rect;

	[SerializeField]
	private GameObject[] SubMainObj;

	[SerializeField]
	private Image[] imgStage;

	[SerializeField]
	private StarClearComponent[] StarClearComponent;

	[SerializeField]
	private Text textStageName;

	[SerializeField]
	private Text FloorText;

	[SerializeField]
	private Image LockImage;

	[SerializeField]
	private Image ClearImage;

	[SerializeField]
	private Image ClearFrameImage;

	[SerializeField]
	private Image[] TowerFrameImage;

	[SerializeField]
	private Image[] TowerBobyImage;

	[SerializeField]
	private Image[] TowerBobyLineImage;

	[SerializeField]
	private Image[] TowerHeadImage;

	[SerializeField]
	private Image[] TowerHeadLineImage;

	[SerializeField]
	private Text[] TowerHeadTextImage;

	[SerializeField]
	private Image[] TowerBaseImage;

	[SerializeField]
	private Image StageImage;

	private Vector2[] sizeDelta = new Vector2[2]
	{
		new Vector2(225f, 200f),
		new Vector2(425f, 200f)
	};

	[HideInInspector]
	public int NowIdx = -1;

	public bool isClear;

	private Color clear = Color.clear;

	private Color white = Color.white;

	private Color32[] colors = new Color32[2]
	{
		new Color32(144, 238, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, 195, 249, byte.MaxValue)
	};

	public STAGE_TABLE StageData { get; private set; }

	public StageInfo NetStageInfo { get; private set; }

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		parent = GetComponentInParent<UI_Challenge>();
		parent.SetData(base.gameObject, ref NowIdx);
	}

	public void SetMainUnitData()
	{
		SetUnitActive(true, 1);
		SetUnitActive(false, 0);
		parent.Rebuild(base.gameObject);
	}

	public void SetSubUnitData(STAGE_TABLE p_stageData, StageInfo p_netStageInfo)
	{
		StageData = p_stageData;
		NetStageInfo = p_netStageInfo;
		SetUnitActive(true, 0);
		SetUnitActive(false, 1);
	}

	public void SetUnitActive(bool active, int idx)
	{
		if (!active)
		{
			SubMainObj[idx].SetActive(active);
			return;
		}
		int num = 0;
		if (StageData == null)
		{
			imgStage[idx].color = clear;
			LockImage.color = white;
		}
		else
		{
			if (NetStageInfo != null)
			{
				num = ManagedSingleton<StageHelper>.Instance.GetStarAmount(NetStageInfo.netStageInfo.Star);
			}
			StageImage.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_iconStageBg, StageData.s_ICON);
			StageImage.color = white;
			ClearImage.color = clear;
			ClearFrameImage.color = clear;
			if (num <= 0)
			{
				isClear = false;
				StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
				if (!ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(StageData, ref condition))
				{
					LockImage.color = white;
				}
				else
				{
					LockImage.color = clear;
				}
			}
			else
			{
				isClear = true;
				LockImage.color = clear;
				ClearImage.color = white;
				ClearFrameImage.color = white;
			}
			FloorText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("TOWER_LEVEL_SHORT"), StageData.n_SUB);
		}
		rect.sizeDelta = sizeDelta[idx];
		SubMainObj[idx].gameObject.SetActive(active);
		int difficultyType = parent.GetDifficultyType();
		int roofStatus = parent.GetRoofStatus();
		for (int i = 0; i < TowerBobyImage.Length; i++)
		{
			if (difficultyType == i)
			{
				TowerBobyImage[i].color = white;
				TowerBobyLineImage[i].color = white;
				TowerFrameImage[i].color = white;
				if (StageData.n_SUB == 1)
				{
					TowerBaseImage[i].color = white;
					TowerHeadImage[i].color = clear;
					TowerHeadLineImage[i].color = clear;
					TowerHeadTextImage[i].color = clear;
				}
				else if (StageData.n_SUB == roofStatus)
				{
					TowerHeadImage[i].color = white;
					TowerHeadLineImage[i].color = white;
					TowerHeadTextImage[i].color = white;
					TowerBaseImage[i].color = clear;
				}
				else
				{
					TowerBaseImage[i].color = clear;
					TowerHeadImage[i].color = clear;
					TowerHeadLineImage[i].color = clear;
					TowerHeadTextImage[i].color = clear;
				}
			}
			else
			{
				TowerBobyImage[i].color = clear;
				TowerBobyLineImage[i].color = clear;
				TowerFrameImage[i].color = clear;
				TowerBaseImage[i].color = clear;
				TowerHeadImage[i].color = clear;
				TowerHeadLineImage[i].color = clear;
				TowerHeadTextImage[i].color = clear;
			}
		}
	}

	public void OnClickUnit()
	{
		parent.OnClickUnit(rect);
	}
}
