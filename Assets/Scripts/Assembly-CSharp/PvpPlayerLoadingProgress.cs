using System.Collections;
using System.Globalization;
using OrangeSocket;
using UnityEngine;
using cb;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(OrangeText))]
public class PvpPlayerLoadingProgress : MonoBehaviour
{
	public const string PVP_LOADING_PROGRESS_FORMAT = "progress#{0}#{1}";

	private Canvas canvas;

	private OrangeText textProgress;

	private string uid;

	private bool isSelf;

	private float progressResource;

	private float progressScene;

	private float totalProgress;

	private NumberFormatInfo formatInfo;

	private float[] rate = new float[2] { 0.5f, 0.5f };

	private void Awake()
	{
		formatInfo = new NumberFormatInfo
		{
			PercentDecimalDigits = 0,
			PercentPositivePattern = 1
		};
		canvas = GetComponent<Canvas>();
		textProgress = GetComponent<OrangeText>();
	}

	public void Setup(string p_uid)
	{
		canvas.enabled = true;
		uid = p_uid;
		totalProgress = 0f;
		textProgress.text = "0%";
		isSelf = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify == p_uid;
		if (!isSelf)
		{
			MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.NTBroadcastToRoom, OnUpdateProgress);
			return;
		}
		Singleton<GenericEventManager>.Instance.AttachEvent<float>(EventManager.ID.UPDATE_STAGE_RES_PROGRESS, UpdateResourceProgress);
		Singleton<GenericEventManager>.Instance.AttachEvent<float>(EventManager.ID.UPDATE_SCENE_PROGRESS, UpdateSceneProgress);
		StartCoroutine(OnStartUpdateProgress());
	}

	public void OnUpdateProgress(object obj)
	{
		if (totalProgress >= 1f)
		{
			return;
		}
		string action = ((NTBroadcastToRoom)obj).Action;
		if (action.StartsWith("progress"))
		{
			string[] array = action.Split('#');
			if (array[1].Equals(uid))
			{
				float.TryParse(array[2], out totalProgress);
				textProgress.text = totalProgress.ToString("p", formatInfo);
			}
		}
	}

	private void UpdateResourceProgress(float p_progress)
	{
		progressResource = Mathf.Max(progressResource, p_progress);
	}

	private void UpdateSceneProgress(float progress)
	{
		progressScene = Mathf.Max(progressScene, progress);
	}

	private IEnumerator OnStartUpdateProgress()
	{
		while (true)
		{
			float num = progressScene * rate[0] + progressResource * rate[1];
			if (totalProgress != num)
			{
				totalProgress = num;
				textProgress.text = totalProgress.ToString("p", formatInfo);
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoom(string.Format("progress#{0}#{1}", uid, totalProgress)));
			}
			yield return CoroutineDefine._0_3sec;
		}
	}

	private void OnDisable()
	{
		if (!isSelf)
		{
			MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.NTBroadcastToRoom, OnUpdateProgress);
			return;
		}
		Singleton<GenericEventManager>.Instance.DetachEvent<float>(EventManager.ID.UPDATE_STAGE_RES_PROGRESS, UpdateResourceProgress);
		Singleton<GenericEventManager>.Instance.DetachEvent<float>(EventManager.ID.UPDATE_SCENE_PROGRESS, UpdateSceneProgress);
	}
}
