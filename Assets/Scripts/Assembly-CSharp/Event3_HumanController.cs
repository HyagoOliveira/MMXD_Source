using UnityEngine;

public class Event3_HumanController : EnemyHumanController
{
	private SkinnedMeshRenderer BodyMesh;

	private SkinnedMeshRenderer LHandMesh;

	private SkinnedMeshRenderer RHandMesh;

	[SerializeField]
	[Tooltip("登場時間")]
	private float ShowTime = 1f;

	private int ShowFrame;

	private Vector3 ShowPos;

	private bool HasShow;

	private bool CanMove;

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch984_Gate_000", 4);
	}

	public override void OnToggleCharacterMaterial(bool appear)
	{
		IgnoreGravity = false;
		CanMove = true;
		base.AllowAutoAim = true;
		SetColliderEnable(true);
	}

	public override void LogicUpdate()
	{
		if (!Activate || !BuildDone || PlayerWeapons == null || PlayerWeapons[0].BulletData == null)
		{
			return;
		}
		if (!HasShow && GameLogicUpdateManager.GameFrame > ShowFrame)
		{
			if ((bool)BodyMesh && (bool)LHandMesh && (bool)RHandMesh)
			{
				BodyMesh.enabled = true;
				LHandMesh.enabled = true;
				RHandMesh.enabled = true;
				_transform.position = ShowPos;
				Controller.LogicPosition = new VInt3(_transform.position);
			}
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Appear(delegate
				{
					OnToggleCharacterMaterial(true);
				});
			}
			EnableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			HasShow = true;
		}
		if (CanMove)
		{
			base.LogicUpdate();
		}
	}

	public override void SetActiveReal(bool isActive)
	{
		if (IsStun)
		{
			SetStun(false);
		}
		Controller.enabled = isActive;
		SetColliderEnable(isActive);
		if (isActive)
		{
			_transform.SetParent(null);
			Controller.LogicPosition = new VInt3(_transform.localPosition);
			_velocityExtra = VInt3.zero;
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
			MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, true);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch984_Gate_000", ShowPos, Quaternion.identity, new object[1] { Vector3.one });
			_animator.SetFloat(hashSpeedMultiplier, 1f);
			SetStatus(MainStatus.Idle);
			ShowFrame = GameLogicUpdateManager.GameFrame + (int)(ShowTime * 20f);
			CanMove = false;
			HasShow = false;
			base.AllowAutoAim = false;
			SetColliderEnable(false);
			IgnoreGravity = true;
			BodyMesh = OrangeBattleUtility.FindChildRecursive(base.transform, "BodyMesh_m").GetComponent<SkinnedMeshRenderer>();
			LHandMesh = OrangeBattleUtility.FindChildRecursive(base.transform, "HandMesh_L_m").GetComponent<SkinnedMeshRenderer>();
			RHandMesh = OrangeBattleUtility.FindChildRecursive(base.transform, "HandMesh_R_m").GetComponent<SkinnedMeshRenderer>();
			if ((bool)BodyMesh && (bool)LHandMesh && (bool)RHandMesh)
			{
				BodyMesh.enabled = false;
				LHandMesh.enabled = false;
				RHandMesh.enabled = false;
			}
		}
		else
		{
			CanMove = false;
			HasShow = false;
			BuildDone = false;
			UpdateHurtAction();
			AiTimer.TimerStop();
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
			MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, false);
			selfBuffManager.StopLoopSE();
			DisableWeaponMesh(ref PlayerWeapons[WeaponCurrent]);
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Disappear(delegate
				{
					Transform[] target = base.transform.GetComponentsInChildren<Transform>(true);
					Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "AimIcon2");
					if ((bool)transform)
					{
						transform.SetParent(null);
					}
					if ((bool)CurrentEnemyHumanModel)
					{
						CurrentEnemyHumanModel.transform.SetParentNull();
						CurrentEnemyHumanModel.BackToPool();
						for (int i = 0; i < CurrentEnemyHumanWeapon.Length; i++)
						{
							if (CurrentEnemyHumanWeapon[i] != null)
							{
								CurrentEnemyHumanWeapon[i].transform.SetParentNull();
								CurrentEnemyHumanWeapon[i].BackToPool();
							}
						}
						ModelTransform = null;
					}
					MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
				});
			}
		}
		Activate = ManagedSingleton<StageHelper>.Instance.bEnemyActive && isActive;
		if (!isActive)
		{
			bNeedDead = false;
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		base.direction = ((!bBack) ? 1 : (-1));
		ShowPos = pos;
	}
}
