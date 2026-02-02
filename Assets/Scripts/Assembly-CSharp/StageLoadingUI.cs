using UnityEngine;

public class StageLoadingUI : MonoBehaviour, ILoadingState
{
	[SerializeField]
	private OrangeBgExt bgExt;

	[SerializeField]
	private OrangeText textProgess;

	private int nowProgress = -1;

	public bool IsComplete { get; set; }

	public object[] Params { get; set; }

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<float>(EventManager.ID.UPDATE_LOADING_PROGRESS, UpdateLoadingProgress);
	}

	private void Awake()
	{
		IsComplete = true;
	}

	private void Start()
	{
		OrangeBgExt orangeBgExt = Object.Instantiate(bgExt, base.transform, false);
		orangeBgExt.rt.SetParent(base.transform, false);
		orangeBgExt.ApplyEft = true;
		orangeBgExt.ChangeBackground(ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT[ManagedSingleton<StageHelper>.Instance.nLastStageID].s_BG);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<float>(EventManager.ID.UPDATE_LOADING_PROGRESS, UpdateLoadingProgress);
	}

	private void UpdateLoadingProgress(float p_progress)
	{
		int num = (int)p_progress;
		if (nowProgress != num)
		{
			nowProgress = num;
			textProgess.text = num.ToString();
			if (nowProgress >= 1)
			{
				Singleton<GenericEventManager>.Instance.DetachEvent<float>(EventManager.ID.UPDATE_LOADING_PROGRESS, UpdateLoadingProgress);
			}
		}
	}
}
