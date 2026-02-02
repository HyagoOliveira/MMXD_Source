using System;
using UnityEngine;
using UnityEngine.UI;

internal class CoopStageSelectUIUnit : ScrollIndexCallback
{
	[SerializeField]
	private CoopStageSelectUI parent;

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

	private Vector2[] sizeDelta = new Vector2[2]
	{
		new Vector2(225f, 131f),
		new Vector2(425f, 248f)
	};

	[HideInInspector]
	public int NowIdx = -1;

	private bool isClear;

	private Color clear = Color.clear;

	private Color white = Color.white;

	public STAGE_TABLE StageData { get; private set; }

	public StageInfo NetStageInfo { get; private set; }

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		parent.SetData(this, ref NowIdx);
	}

	public void SetMainUnitData()
	{
		SetUnitActive(true, 1);
		SetUnitActive(false, 0);
		base.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
		LeanTween.cancel(base.gameObject);
		LeanTween.scale(base.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setOnComplete((Action)delegate
		{
			parent.Rebuild(this);
			LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
		});
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
		int activeStar = 0;
		if (StageData == null)
		{
			imgStage[idx].color = clear;
			StarClearComponent[idx].gameObject.SetActive(false);
		}
		else
		{
			StarClearComponent[idx].gameObject.SetActive(true);
			if (NetStageInfo != null)
			{
				activeStar = ManagedSingleton<StageHelper>.Instance.GetStarAmount(NetStageInfo.netStageInfo.Star);
			}
			StarClearComponent[idx].SetActiveStar(activeStar);
			imgStage[idx].sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_iconStageBg, StageData.s_ICON);
			imgStage[idx].color = white;
		}
		rect.sizeDelta = sizeDelta[idx];
		SubMainObj[idx].gameObject.SetActive(active);
	}

	public void OnClickUnit()
	{
		parent.OnClickUnit(rect);
	}
}
