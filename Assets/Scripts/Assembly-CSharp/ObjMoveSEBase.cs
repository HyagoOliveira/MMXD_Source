using NaughtyAttributes;
using StageLib;
using UnityEngine;

public class ObjMoveSEBase : MonoBehaviour
{
	[BoxGroup("Sound")]
	[SerializeField]
	protected BattleSE StartSE;

	[BoxGroup("Sound")]
	[SerializeField]
	protected BattleSE LoopSE;

	[BoxGroup("Sound")]
	[SerializeField]
	protected BattleSE LoopStopSE;

	[BoxGroup("Sound")]
	[SerializeField]
	protected BattleSE StopSE;

	[BoxGroup("acbSound")]
	[SerializeField]
	protected string ACB_Str;

	[BoxGroup("acbSound")]
	[SerializeField]
	protected string StartSEStr;

	[BoxGroup("acbSound")]
	[SerializeField]
	protected string LoopSEStr;

	[BoxGroup("acbSound")]
	[SerializeField]
	protected string LoopStopSEStr;

	[BoxGroup("acbSound")]
	[SerializeField]
	protected string StopSEStr;

	[SerializeField]
	private bool bForcePlay;

	protected MapObjEvent MapObjEvent;

	protected bool visible;

	protected bool isForseSE;

	protected bool useForseCloseSE;

	private Vector3 preVec3;

	private bool isSEPlayed;

	private bool waitMeshRender2PlaySE;

	private Vector3 lastPosition = new Vector3(0f, 0f, 0f);

	[SerializeField]
	public OrangeCriSource SoundSource;

	private void Awake()
	{
	}

	protected virtual void Start()
	{
		MapObjEvent = base.transform.parent.GetComponent<MapObjEvent>();
		if ((bool)MapObjEvent)
		{
			SoundSource = MapObjEvent.SoundSource;
			if (bForcePlay)
			{
				SoundSource.Initial(OrangeSSType.SYSTEM);
			}
			MapObjEvent.MoveObjStart = PlayStartSE;
			MapObjEvent.MoveObjEnd = PlayEndSE;
			lastPosition = MapObjEvent.transform.position;
		}
	}

	public virtual void PlayStartSE()
	{
		if (isSEPlayed)
		{
			return;
		}
		if (visible || isForseSE)
		{
			if (string.IsNullOrEmpty(ACB_Str))
			{
				PlayBattleSECheck(LoopSE, isForseSE);
			}
			else
			{
				PlayStrSECheck(isForseSE);
			}
			isSEPlayed = true;
		}
		else
		{
			waitMeshRender2PlaySE = true;
		}
	}

	public virtual void PlayEndSE()
	{
		if (isSEPlayed)
		{
			if (string.IsNullOrEmpty(ACB_Str))
			{
				PlayBattleSE(LoopStopSE, isForseSE);
			}
			else
			{
				PlayStrSEEnd(isForseSE);
			}
			waitMeshRender2PlaySE = (isSEPlayed = false);
		}
	}

	protected virtual void Update()
	{
		if (waitMeshRender2PlaySE)
		{
			PlayStartSE();
		}
	}

	protected void OnDestroy()
	{
		PlayEndSE();
	}

	protected void PlayBattleSECheck(BattleSE cueId, bool ForceTrigger = false)
	{
		if (!MonoBehaviourSingleton<UIManager>.Instance.IsLoading && (visible || ForceTrigger))
		{
			useForseCloseSE = true;
			SoundSource.PlaySE("BattleSE", (int)cueId);
		}
	}

	protected void PlayStrSECheck(bool ForceTrigger = false)
	{
		if (MonoBehaviourSingleton<UIManager>.Instance.IsLoading || (!visible && !ForceTrigger))
		{
			return;
		}
		useForseCloseSE = true;
		if (ForceTrigger)
		{
			if (!string.IsNullOrEmpty(StartSEStr))
			{
				SoundSource.ForcePlaySE(ACB_Str, StartSEStr);
			}
			if (!string.IsNullOrEmpty(LoopSEStr))
			{
				SoundSource.ForcePlaySE(ACB_Str, LoopSEStr);
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(StartSEStr))
			{
				SoundSource.PlaySE(ACB_Str, StartSEStr);
			}
			if (!string.IsNullOrEmpty(LoopSEStr))
			{
				SoundSource.PlaySE(ACB_Str, LoopSEStr);
			}
		}
	}

	protected void PlayBattleSE(BattleSE cueId, bool ForceTrigger = false)
	{
		if (!MonoBehaviourSingleton<UIManager>.Instance.IsLoading && (visible || ForceTrigger || useForseCloseSE))
		{
			SoundSource.PlaySE("BattleSE", (int)cueId);
			useForseCloseSE = false;
		}
	}

	protected void PlayStrSEEnd(bool ForceTrigger = false)
	{
		if (MonoBehaviourSingleton<UIManager>.Instance.IsLoading || (!visible && !ForceTrigger && !useForseCloseSE))
		{
			return;
		}
		if (ForceTrigger || useForseCloseSE)
		{
			if (!string.IsNullOrEmpty(StopSEStr))
			{
				SoundSource.ForcePlaySE(ACB_Str, StopSEStr);
			}
			if (!string.IsNullOrEmpty(LoopStopSEStr))
			{
				SoundSource.ForcePlaySE(ACB_Str, LoopStopSEStr);
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(StopSEStr))
			{
				SoundSource.PlaySE(ACB_Str, StopSEStr);
			}
			if (!string.IsNullOrEmpty(LoopStopSEStr))
			{
				SoundSource.PlaySE(ACB_Str, LoopStopSEStr);
			}
		}
		useForseCloseSE = false;
	}

	private void OnBecameVisible()
	{
		visible = true;
	}

	private void OnBecameInvisible()
	{
		visible = false;
	}
}
