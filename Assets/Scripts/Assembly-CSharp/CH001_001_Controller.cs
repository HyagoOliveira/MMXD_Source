using System;
using CallbackDefs;
using UnityEngine;

public class CH001_001_Controller : RMXController, ILogicUpdate
{
	private int _Dissolver_amount = Shader.PropertyToID("_Dissolver_amount");

	private MaterialPropertyBlock mpb;

	private SkinnedMeshRenderer shaw;

	private bool isShawActive;

	private float shawTime;

	private float lastSetAmount;

	private int tweenUid = -1;

	private Callback ShawCb;

	private int uid = -1;

	public override void Start()
	{
		base.Start();
		mpb = null;
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "ShawlMesh_e", true);
		shaw = transform.GetComponent<SkinnedMeshRenderer>();
		mpb = new MaterialPropertyBlock();
		shaw.GetPropertyBlock(mpb);
		mpb.SetFloat(_Dissolver_amount, 0f);
		shaw.SetPropertyBlock(mpb);
		isShawActive = false;
	}

	protected void OnEnable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	protected void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public void LogicUpdate()
	{
		if (!_refEntity.Activate)
		{
			return;
		}
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.WALK:
		case OrangeCharacter.MainStatus.DASH:
		case OrangeCharacter.MainStatus.AIRDASH:
			if (!isShawActive)
			{
				LeanTween.cancel(base.gameObject, tweenUid);
				SetShawOpen();
			}
			break;
		case OrangeCharacter.MainStatus.IDLE:
			if (tweenUid == -1 && isShawActive)
			{
				SetShawClose();
			}
			break;
		}
	}

	private void SetShawOpen()
	{
		isShawActive = true;
		tweenUid = LeanTween.value(base.gameObject, 0f, 8f, 0.3f).setOnUpdate(delegate(float val)
		{
			mpb.SetFloat(_Dissolver_amount, val);
			shaw.SetPropertyBlock(mpb);
		}).setOnComplete((Action)delegate
		{
			tweenUid = -1;
		})
			.uniqueId;
	}

	private void SetShawClose()
	{
		isShawActive = false;
		tweenUid = LeanTween.value(base.gameObject, 8f, 0f, 0.3f).setOnUpdate(delegate(float val)
		{
			mpb.SetFloat(_Dissolver_amount, val);
			shaw.SetPropertyBlock(mpb);
		}).setOnComplete((Action)delegate
		{
			tweenUid = -1;
		})
			.uniqueId;
	}

	public override void ClearSkill()
	{
		base.ClearSkill();
		if (_refEntity.UsingVehicle)
		{
			SetShawClose();
		}
	}
}
