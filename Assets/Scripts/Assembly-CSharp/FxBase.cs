using StageLib;
using UnityEngine;

public class FxBase : PoolBaseObject
{
	protected int tweenUid = -1;

	[SerializeField]
	protected float timeBackToPool = 1f;

	[SerializeField]
	public bool useSE;

	[SerializeField]
	public bool useEndSE;

	[SerializeField]
	[Tooltip("不做遠近檢查，存在即撥放")]
	public bool forcePlay;

	[SerializeField]
	public string[] SEName;

	[SerializeField]
	public string[] EndSEName;

	protected ParticleSystem ps;

	public OrangeCriSource SoundSource;

	private bool hasPLoop;

	private StageFXParam tStageFXParam;

	protected Vector3 OriScale = Vector3.one;

	public ParticleSystem pPS
	{
		get
		{
			return ps;
		}
	}

	public bool IsEnd { get; set; } = true;


	protected virtual void Awake()
	{
		base.gameObject.layer = ManagedSingleton<OrangeLayerManager>.Instance.FxLayer;
		ps = GetComponentInChildren<ParticleSystem>(true);
		base.gameObject.SetActive(false);
		OriScale = base.transform.localScale;
		if (useSE)
		{
			SoundSource = base.gameObject.AddOrGetComponent<OrangeCriSource>();
			SoundSource.Initial(OrangeSSType.HIT);
		}
	}

	public virtual void Active(params object[] p_params)
	{
		IsEnd = false;
		base.gameObject.SetActive(true);
		ps.Play(true);
		float fPlayTime = timeBackToPool;
		if (p_params != null && p_params.Length != 0)
		{
			tStageFXParam = p_params[0] as StageFXParam;
			if (tStageFXParam != null && tStageFXParam.fPlayTime > 0f)
			{
				fPlayTime = tStageFXParam.fPlayTime;
			}
		}
		StartCoroutine(StageResManager.TweenFloatCoroutine(0f, 1f, fPlayTime, null, delegate
		{
			BackToPool();
		}));
		if (!useSE)
		{
			return;
		}
		for (int i = 0; i < SEName.Length; i += 2)
		{
			string[] array = SEName[i + 1].Split(',');
			float num = 0f;
			float delay = 0f;
			if (array.Length > 1)
			{
				delay = float.Parse(array[1]);
			}
			if (array.Length > 2)
			{
				hasPLoop = true;
				num = float.Parse(array[2]);
				SoundSource.AddLoopSE(SEName[i], array[0], num, delay);
			}
			else
			{
				PlaySE(SEName[i], array[0], delay);
			}
		}
	}

	public void StopEmittingBackToPool(float fWaitTime = 5f)
	{
		StartCoroutine(StageResManager.TweenFloatCoroutine(1f, 0f, fWaitTime, delegate(float f)
		{
			ParticleSystem.EmissionModule emission = ps.emission;
			emission.rateOverTimeMultiplier = f;
		}, delegate
		{
			if (ps != null)
			{
				ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
			}
		}));
		StartCoroutine(StageResManager.TweenFloatCoroutine(0f, 1f, fWaitTime * 2f, null, delegate
		{
			BackToPool();
		}));
	}

	public override void BackToPool()
	{
		IsEnd = true;
		if (ps != null)
		{
			ps.Stop(true);
		}
		base.gameObject.SetActive(false);
		LeanTween.cancel(base.gameObject);
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
		base.transform.localScale = OriScale;
		MonoBehaviourSingleton<FxManager>.Instance.UnRegisterFxBase(this);
		if (hasPLoop)
		{
			SoundSource.StopAll();
		}
		if (useEndSE)
		{
			for (int i = 0; i < EndSEName.Length; i += 2)
			{
				string[] array = EndSEName[i + 1].Split(',');
				float delay = 0f;
				if (array.Length > 1)
				{
					delay = float.Parse(array[1]);
				}
				PlaySE(EndSEName[i], array[0], delay);
			}
		}
		if (tStageFXParam != null)
		{
			tStageFXParam = null;
		}
	}

	private void PlaySE(string acb, string cue, float delay = 0f)
	{
		if (tStageFXParam == null || !tStageFXParam.bMute)
		{
			if (forcePlay)
			{
				SoundSource.ForcePlaySE(acb, cue, delay);
			}
			else
			{
				SoundSource.PlaySE(acb, cue, delay);
			}
		}
	}

	private void OnDestroy()
	{
		LeanTween.cancel(base.gameObject, false);
	}
}
