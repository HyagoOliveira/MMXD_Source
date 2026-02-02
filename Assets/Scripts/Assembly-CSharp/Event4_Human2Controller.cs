using UnityEngine;

public class Event4_Human2Controller : EnemyHumanSwordController
{
	[SerializeField]
	private bool CanMove;

	private int EventID = -1;

	private ObjInfoBar InfoBar;

	private Transform SpecObj;

	private string[] WeaponName;

	public override string[] GetHumanDependAnimations()
	{
		return new string[0];
	}

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
			if (_subStatus == SubStatus.Phase0)
			{
				_velocity = VInt3.zero;
				CanMove = false;
				InfoBar = _transform.GetComponentInChildren<ObjInfoBar>();
				if ((bool)InfoBar)
				{
					InfoBar.SetPlayBar(MaxHp, Hp, "", 10 * EnemyData.n_DIFFICULTY);
				}
				ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
			}
			break;
		}
	}

	protected override void UpdateAnimation()
	{
		base.UpdateAnimation();
		int num = 0;
		switch (_mainStatus)
		{
		case MainStatus.EventIdle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				AI_STATE aiState = AiState;
				num = EnemyHumanController._animationHash[0][_isShoot];
				_animator.Play(num, 0, 0f);
				break;
			}
			case SubStatus.Phase1:
				num = EnemyHumanController._animationHash[10][_isShoot];
				_animator.Play(num, 0, 0f);
				break;
			}
			break;
		case MainStatus.Dead:
			num = Animator.StringToHash("skillclip0");
			_animator.Play(num, 0, 0f);
			break;
		}
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.EventIdle)
		{
			SubStatus subStatus = _subStatus;
			if (subStatus != 0 && subStatus == SubStatus.Phase1 && Controller.Collisions.below)
			{
				SetStatus(MainStatus.EventIdle);
			}
		}
	}

	public override void SetActiveReal(bool isActive)
	{
		base.SetActiveReal(isActive);
		CanMove = false;
		if (!isActive)
		{
			return;
		}
		base.AllowAutoAim = false;
		SetColliderEnable(true);
		DisableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
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

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		base.SetPositionAndRotation(pos, bBack);
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_002:
			WeaponName = new string[3] { "SaberMesh_main_m", "SaberMesh_sub_g", "FlowerMesh_c" };
			break;
		case AI_STATE.mob_003:
			WeaponName = new string[0];
			break;
		default:
			WeaponName = new string[0];
			break;
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (DeadCallback != null)
		{
			DeadCallback();
		}
		AI_STATE aiState = AiState;
		base.DeadPlayCompleted = true;
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
}
