using UnityEngine;

public class Valentine2021NPCController : EnemyHumanSwordController
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

	public override void Unlock()
	{
		_unlockReady = true;
		if (InGame && (int)Hp > 0)
		{
			Activate = true;
		}
	}

	protected override void AwakeJob()
	{
		base.AwakeJob();
	}

	protected override void SetStatus(int mainStatus, int subStatus = 0)
	{
		base.SetStatus(mainStatus, subStatus);
		MainStatus mainStatus2 = _mainStatus;
		if (mainStatus2 == MainStatus.EventIdle && _subStatus == SubStatus.Phase0)
		{
			_velocity = VInt3.zero;
			CanMove = false;
			InfoBar = _transform.GetComponentInChildren<ObjInfoBar>();
			if ((bool)InfoBar)
			{
				InfoBar.gameObject.SetActive(false);
			}
			ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		}
	}

	protected override void UpdateAnimation()
	{
		base.UpdateAnimation();
		int num = 0;
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.EventIdle)
		{
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
		SetColliderEnable(false);
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
			WeaponName = new string[1] { "BowlMesh_c_L" };
			break;
		case AI_STATE.mob_003:
			WeaponName = new string[2] { "SaberMeshMain_m", "SaberMeshSub_g" };
			break;
		default:
			WeaponName = new string[0];
			break;
		}
	}
}
