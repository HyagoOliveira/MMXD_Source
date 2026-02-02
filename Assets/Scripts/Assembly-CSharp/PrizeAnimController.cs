#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;

public class PrizeAnimController : MonoBehaviour
{
	private class PrizeTweenInfo
	{
		public float from;

		public float to;

		public float time;

		public int loopCount;

		public PrizeTweenInfo(float from, float to, float time, int loopCount = 1)
		{
			this.from = from;
			this.to = to;
			this.time = time;
			this.loopCount = loopCount;
		}
	}

	[SerializeField]
	public float SEDis = 10f;

	private Queue<PrizeTweenInfo> queueEnd = new Queue<PrizeTweenInfo>();

	private LTDescr lastTween;

	public bool CanSkip { get; private set; }

	private void Awake()
	{
		CanSkip = false;
	}

	public void StartAnim(float endDegree)
	{
		Debug.Log("[PrizeAnim] Start.");
		LeanTween.cancel(base.gameObject, false);
		base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		queueEnd.Clear();
		CanSkip = false;
		float num = endDegree;
		float time = UnityEngine.Random.Range(0.9f, 1.2f);
		num += UnityEngine.Random.Range(10f, 30f);
		Queue<PrizeTweenInfo> queue = new Queue<PrizeTweenInfo>();
		queue.Enqueue(new PrizeTweenInfo(0f, -180f, 0.35f));
		queue.Enqueue(new PrizeTweenInfo(-180f, -360f, 0.3f));
		queue.Enqueue(new PrizeTweenInfo(0f, -360f, 0.25f));
		queue.Enqueue(new PrizeTweenInfo(0f, -360f, 0.2f, -1));
		queueEnd = new Queue<PrizeTweenInfo>();
		queueEnd.Enqueue(new PrizeTweenInfo(0f, -360f, 0.5f));
		queueEnd.Enqueue(new PrizeTweenInfo(0f, -360f, 0.75f));
		queueEnd.Enqueue(new PrizeTweenInfo(0f, num, 0.9f));
		queueEnd.Enqueue(new PrizeTweenInfo(num, endDegree, time));
		PlayQueueAnim(queue);
	}

	public void StopAnim(Callback p_cb)
	{
		if (!CanSkip)
		{
			return;
		}
		CanSkip = false;
		Debug.Log("[PrizeAnim] Stop.");
		if (lastTween != null)
		{
			lastTween.setOnComplete((Action)delegate
			{
				PlayQueueAnim(queueEnd, p_cb);
			});
			lastTween.loopType = LeanTweenType.once;
			lastTween.loopCount = 0;
		}
	}

	private void PlayQueueAnim(Queue<PrizeTweenInfo> p_queue, Callback p_cb = null)
	{
		if (p_queue.Count > 0)
		{
			PrizeTweenInfo prizeTweenInfo = p_queue.Dequeue();
			float seCount = 0f;
			CanSkip = prizeTweenInfo.loopCount == -1;
			lastTween = LeanTween.value(base.gameObject, prizeTweenInfo.from, prizeTweenInfo.to, prizeTweenInfo.time).setOnUpdate(delegate(float val)
			{
				if (Mathf.Abs(val - seCount) > SEDis)
				{
					seCount = val;
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_SPIN);
				}
				base.transform.localRotation = Quaternion.Euler(0f, 0f, val);
			}).setOnComplete((Action)delegate
			{
				lastTween = null;
				PlayQueueAnim(p_queue, p_cb);
			})
				.setLoopCount(prizeTweenInfo.loopCount);
		}
		else
		{
			p_cb.CheckTargetToInvoke();
		}
	}
}
