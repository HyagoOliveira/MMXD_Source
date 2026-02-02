using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class EM090_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Hurt = 1,
		IdleWaitNet = 2
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		MAX_SUBSTATUS = 6
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	[Header("電流特效")]
	[SerializeField]
	private ParticleSystem ThunderFx;

	[SerializeField]
	private float ThunderLength = 3f;

	[Header("受擊物件")]
	[SerializeField]
	private Transform LowerMesh;

	[SerializeField]
	private float LifeLength = 2.3f;

	[Header("事件")]
	[SerializeField]
	private int EventID;

	private bool HasCallEvent;

	private bool bPlayLP;

	private float FxLengthFix
	{
		get
		{
			return LifeLength * (((float)(int)Hp - 1f) / ((float)(int)MaxHp - 1f)) / ThunderLength;
		}
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	public new virtual void Unlock()
	{
		_unlockReady = true;
		base.AllowAutoAim = false;
		SetColliderEnable(true);
		if (InGame && (int)Hp > 0)
		{
			Activate = true;
		}
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		base.AllowAutoAim = false;
		base.AimPoint = Vector3.zero;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(8f);
		AiTimer.TimerStart();
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp > 0 && !HasCallEvent && (int)Hp == 1)
		{
			EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
			stageEventCall.nID = EventID;
			stageEventCall.tTransform = _transform;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			HasCallEvent = true;
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
	}

	public override void LogicUpdate()
	{
		selfBuffManager.UpdateBuffTime();
	}

	public void UpdateFunc()
	{
	}

	public override void SetActive(bool isActive)
	{
		IgnoreGravity = true;
		base.SetActive(isActive);
		if (isActive)
		{
			LowerMesh.localPosition = new Vector3(0f, 0.7f + LifeLength, 0f);
			ThunderFx.transform.localScale = new Vector3(FxLengthFix, 1f, 1f);
			ThunderFx.transform.localPosition = new Vector3(0f, LowerMesh.localPosition.y / 2f, 0f);
			ThunderFx.Play();
			if (!bPlayLP)
			{
				PlaySE("BattleSE", "bt_denkikan_lp");
				bPlayLP = true;
			}
			HasCallEvent = false;
			SetStatus(MainStatus.Idle);
		}
		else
		{
			if (ThunderFx.isPlaying && bPlayLP)
			{
				PlaySE("BattleSE", "bt_denkikan_stop");
				bPlayLP = false;
			}
			ThunderFx.Stop();
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		ModelTransform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * (float)base.direction);
		_transform.position = pos;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if ((int)Hp - (int)tHurtPassParam.dmg <= 0 && tHurtPassParam.IsPlayer)
		{
			tHurtPassParam.dmg = (int)Hp - 1;
		}
		ObscuredInt result = base.Hurt(tHurtPassParam);
		LowerMesh.localPosition = new Vector3(0f, 0.7f + LifeLength * (((float)(int)Hp - 1f) / ((float)(int)MaxHp - 1f)), 0f);
		if (FxLengthFix != 0f)
		{
			if (!ThunderFx.isPlaying)
			{
				ThunderFx.Play();
				if (!bPlayLP)
				{
					PlaySE("BattleSE", "bt_denkikan_lp");
					bPlayLP = true;
				}
			}
			ThunderFx.transform.localScale = new Vector3(FxLengthFix, 1f, 1f);
			ThunderFx.transform.localPosition = new Vector3(0f, LowerMesh.localPosition.y / 2f, 0f);
		}
		else
		{
			ThunderFx.Stop();
			if (bPlayLP)
			{
				PlaySE("BattleSE", "bt_denkikan_stop");
				bPlayLP = false;
			}
		}
		if (!HasCallEvent && (int)Hp == 1)
		{
			EventManager.StageEventCall p_param = new EventManager.StageEventCall
			{
				nID = EventID,
				tTransform = _transform
			};
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, p_param);
			HasCallEvent = true;
		}
		return result;
	}

	public override void SetEventCtrlID(int eventid)
	{
		EventID = eventid;
	}
}
