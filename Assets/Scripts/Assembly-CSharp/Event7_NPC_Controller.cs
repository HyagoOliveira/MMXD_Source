using UnityEngine;

public class Event7_NPC_Controller : EnemyHumanSwordController
{
	private ObjInfoBar InfoBar;

	private Transform SpecObj;

	private string[] WeaponName;

	[SerializeField]
	private int[] EventID = new int[10];

	protected override void AwakeJob()
	{
		base.AwakeJob();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fx_bs086_teleport_out", 2);
	}

	protected override void SetStatus(int mainStatus, int subStatus = 0)
	{
		base.SetStatus(mainStatus, subStatus);
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			SetStatus(MainStatus.EventIdle);
			break;
		case MainStatus.Fall:
			SetStatus(MainStatus.EventFall);
			break;
		case MainStatus.EventIdle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				_velocity = VInt3.zero;
				string[] weaponName = WeaponName;
				foreach (string key2 in weaponName)
				{
					SpecObj = OrangeBattleUtility.FindChildRecursive(_transform, key2);
					if ((bool)SpecObj)
					{
						SpecObj.gameObject.SetActive(false);
					}
				}
				break;
			}
			case SubStatus.Phase1:
			{
				base.SoundSource.PlaySE("BossSE06", "bs049_ferham02");
				string[] weaponName = WeaponName;
				foreach (string key in weaponName)
				{
					SpecObj = OrangeBattleUtility.FindChildRecursive(_transform, key);
					if ((bool)SpecObj)
					{
						SpecObj.gameObject.SetActive(true);
					}
				}
				break;
			}
			case SubStatus.Phase2:
				base.SoundSource.PlaySE("BossSE06", "bs049_ferham01");
				break;
			case SubStatus.Phase5:
				BackToPool();
				break;
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				break;
			}
			break;
		}
	}

	protected override void UpdateAnimation()
	{
		int stateNameHash = 0;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				stateNameHash = EnemyHumanController._animationHash[0][_isShoot];
				break;
			case SubStatus.Phase1:
				stateNameHash = EnemyHumanController._animationHash[10][_isShoot];
				break;
			case SubStatus.Phase2:
				stateNameHash = _swordAnimationHash[SlashCount];
				break;
			case SubStatus.Phase3:
				stateNameHash = _swordAnimationHash[5 + SlashCount - 1];
				break;
			}
			break;
		case MainStatus.Fall:
			stateNameHash = EnemyHumanController._animationHash[9][_isShoot];
			break;
		case MainStatus.Hurt:
			if (IsStun)
			{
				stateNameHash = EnemyHumanController._animationHash[24][_isShoot];
				_isShoot = 0;
			}
			else
			{
				stateNameHash = EnemyHumanController._animationHash[23][_isShoot];
			}
			break;
		case MainStatus.Walk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				stateNameHash = EnemyHumanController._animationHash[22][_isShoot];
				break;
			case SubStatus.Phase1:
				stateNameHash = EnemyHumanController._animationHash[2][_isShoot];
				break;
			}
			break;
		case MainStatus.EventIdle:
			stateNameHash = ((_subStatus != 0) ? Animator.StringToHash("skillclip" + (int)(_subStatus - 1)) : Animator.StringToHash("skillclip2"));
			break;
		case MainStatus.Dead:
			stateNameHash = EnemyHumanController._animationHash[0][_isShoot];
			break;
		}
		_animator.Play(stateNameHash, 0, 0f);
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.EventIdle && _subStatus != 0 && _currentFrame > 1f)
		{
			SetStatus(MainStatus.EventIdle);
		}
	}

	public override void SetActiveReal(bool isActive)
	{
		base.SetActiveReal(isActive);
		if (isActive)
		{
			base.AllowAutoAim = false;
			SetColliderEnable(true);
			DisableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, EventCall);
			SetStatus(MainStatus.EventIdle);
			string[] weaponName = WeaponName;
			foreach (string key in weaponName)
			{
				SpecObj = OrangeBattleUtility.FindChildRecursive(_transform, key);
				if ((bool)SpecObj)
				{
					SpecObj.gameObject.SetActive(false);
				}
			}
			if (_enemyCollider.Length != 0)
			{
				for (int j = 0; j < _enemyCollider.Length; j++)
				{
					_enemyCollider[j].gameObject.SetLayer(ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer);
				}
			}
			targetMask = ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask;
			friendMask = ManagedSingleton<OrangeLayerManager>.Instance.EnemyUseMask;
			neutralMask = (int)ManagedSingleton<OrangeLayerManager>.Instance.EnemyUseMask | (int)ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask;
			if (AiState == AI_STATE.mob_001)
			{
				_transform.localScale = Vector3.one * 1.5f;
				UpdateDirection(base.direction);
			}
		}
		else
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, EventCall);
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		base.SetPositionAndRotation(pos, bBack);
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
		IgnoreGravity = true;
		WeaponName = new string[3] { "WhipMesh_Main_c", "WhipMesh_Sub_e", "skill_effect" };
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (DeadCallback != null)
		{
			DeadCallback();
		}
		AI_STATE aiState = AiState;
		base.DeadPlayCompleted = true;
		string[] weaponName = WeaponName;
		foreach (string key in weaponName)
		{
			SpecObj = OrangeBattleUtility.FindChildRecursive(_transform, key);
			if ((bool)SpecObj)
			{
				SpecObj.gameObject.SetActive(false);
			}
		}
		if (_mainStatus != MainStatus.Dead)
		{
			SetColliderEnable(false);
			SetStatus(MainStatus.Dead);
		}
	}

	public override void BackToPool()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fx_bs086_teleport_out", Controller.GetRealCenterPos(), Quaternion.identity, new object[1] { Vector3.one });
		base.BackToPool();
	}

	public void EventCall(EventManager.StageEventCall tStageEventCall)
	{
		int nID = tStageEventCall.nID;
		for (int i = 0; i < EventID.Length; i++)
		{
			if (EventID[i] != 0 && nID == EventID[i])
			{
				SetStatus(MainStatus.EventIdle, (SubStatus)i);
				break;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, EventCall);
	}
}
