using UnityEngine;

public class Event6_NPC_Controller : EnemyHumanSwordController
{
	private ObjInfoBar InfoBar;

	private Transform SpecObj;

	private string[] WeaponName;

	[SerializeField]
	private int[] EventID = new int[3];

	[SerializeField]
	private bool CanMove;

	[SerializeField]
	private float SendEventInterval = 10f;

	private int SendEventFrame;

	[SerializeField]
	private int ScreenMaxNum = 3;

	private int ScreenNum;

	protected override void AwakeJob()
	{
		base.AwakeJob();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fx_bs086_teleport_out", 2);
	}

	protected override void SetStatus(int mainStatus, int subStatus = 0)
	{
		if (!CanMove && mainStatus == 1000 && subStatus == 1)
		{
			return;
		}
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
				_velocity = VInt3.zero;
				ScreenNum = 0;
				InfoBar = _transform.GetComponentInChildren<ObjInfoBar>();
				if ((bool)InfoBar)
				{
					InfoBar.SetPlayBar(MaxHp, Hp, "", 10 * EnemyData.n_DIFFICULTY);
				}
				break;
			case SubStatus.Phase1:
			{
				CallEvent(EventID[1]);
				ScreenNum++;
				SendEventFrame = GameLogicUpdateManager.GameFrame + (int)(SendEventInterval * 20f);
				string[] weaponName = WeaponName;
				foreach (string key3 in weaponName)
				{
					SpecObj = OrangeBattleUtility.FindChildRecursive(_transform, key3);
					if ((bool)SpecObj)
					{
						SpecObj.gameObject.SetActive(true);
					}
				}
				break;
			}
			case SubStatus.Phase2:
			{
				SendEventFrame = GameLogicUpdateManager.GameFrame + (int)(SendEventInterval * 20f);
				string[] weaponName = WeaponName;
				foreach (string key4 in weaponName)
				{
					SpecObj = OrangeBattleUtility.FindChildRecursive(_transform, key4);
					if ((bool)SpecObj)
					{
						SpecObj.gameObject.SetActive(true);
					}
				}
				break;
			}
			case SubStatus.Phase3:
			{
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
			case SubStatus.Phase4:
			{
				string[] weaponName = WeaponName;
				foreach (string key in weaponName)
				{
					SpecObj = OrangeBattleUtility.FindChildRecursive(_transform, key);
					if ((bool)SpecObj)
					{
						SpecObj.gameObject.SetActive(false);
					}
				}
				break;
			}
			case SubStatus.Phase5:
				BackToPool();
				break;
			}
			break;
		}
	}

	protected override void UpdateAnimation()
	{
		int num = 0;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				num = EnemyHumanController._animationHash[0][_isShoot];
				break;
			case SubStatus.Phase1:
				num = EnemyHumanController._animationHash[10][_isShoot];
				break;
			case SubStatus.Phase2:
				num = _swordAnimationHash[SlashCount];
				break;
			case SubStatus.Phase3:
				num = _swordAnimationHash[5 + SlashCount - 1];
				break;
			}
			break;
		case MainStatus.Fall:
			num = EnemyHumanController._animationHash[9][_isShoot];
			break;
		case MainStatus.Hurt:
			if (IsStun)
			{
				num = EnemyHumanController._animationHash[24][_isShoot];
				_isShoot = 0;
			}
			else
			{
				num = EnemyHumanController._animationHash[23][_isShoot];
			}
			break;
		case MainStatus.Walk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				num = EnemyHumanController._animationHash[22][_isShoot];
				break;
			case SubStatus.Phase1:
				num = EnemyHumanController._animationHash[2][_isShoot];
				break;
			}
			break;
		case MainStatus.EventIdle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			case SubStatus.Phase3:
			case SubStatus.Phase4:
			{
				AI_STATE aiState = AiState;
				num = EnemyHumanController._animationHash[0][0];
				_animator.Play(num, 0, 0f);
				break;
			}
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				num = Animator.StringToHash("skillclip0");
				_animator.Play(num, 0, 0f);
				break;
			}
			break;
		case MainStatus.Dead:
			num = Animator.StringToHash("skillclip1");
			_animator.Play(num, 0, 0f);
			break;
		}
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		MainStatus mainStatus = _mainStatus;
		if (mainStatus != MainStatus.EventIdle)
		{
			return;
		}
		switch (_subStatus)
		{
		case SubStatus.Phase1:
			if (_currentFrame > 1f)
			{
				SetStatus(MainStatus.EventIdle, SubStatus.Phase3);
			}
			break;
		case SubStatus.Phase2:
			if (_currentFrame > 1f)
			{
				SetStatus(MainStatus.EventIdle, SubStatus.Phase3);
			}
			break;
		case SubStatus.Phase3:
			if (GameLogicUpdateManager.GameFrame >= SendEventFrame)
			{
				if (ScreenNum < ScreenMaxNum)
				{
					CallEvent(EventID[1]);
					ScreenNum++;
					SetStatus(MainStatus.EventIdle, SubStatus.Phase2);
				}
				else
				{
					SetStatus(MainStatus.EventIdle, SubStatus.Phase4);
				}
			}
			break;
		case SubStatus.Phase0:
			break;
		}
	}

	public override void SetActiveReal(bool isActive)
	{
		base.SetActiveReal(isActive);
		if (isActive)
		{
			base.AllowAutoAim = false;
			CanMove = false;
			SetColliderEnable(true);
			DisableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
			ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
			UpdateDirection(base.direction);
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
		WeaponName = new string[1] { "TabletMesh_c" };
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
		if (nID == EventID[0])
		{
			CanMove = true;
			SetStatus(MainStatus.EventIdle, SubStatus.Phase1);
		}
		else if (nID == EventID[2])
		{
			SetStatus(MainStatus.EventIdle, SubStatus.Phase5);
		}
	}

	private void CallEvent(int EventID)
	{
		EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
		stageEventCall.nID = EventID;
		stageEventCall.tTransform = _transform;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, EventCall);
	}
}
