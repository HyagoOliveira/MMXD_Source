using System;
using StageLib;
using UnityEngine;

public class EM153_Controller : batController
{
	[SerializeField]
	[Tooltip("登場時間")]
	private float ShowTime = 1f;

	private int ShowFrame;

	private Vector3 ShowPos;

	private bool HasShow;

	private bool CanMove;

	[SerializeField]
	private SkinnedMeshRenderer BodyMesh;

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch984_Gate_000", 4);
	}

	public override void OnToggleCharacterMaterial(bool appear)
	{
		CanMove = true;
		base.AllowAutoAim = true;
		SetColliderEnable(true);
		if ((bool)BodyMesh)
		{
			BodyMesh.enabled = appear;
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		if (!HasShow && GameLogicUpdateManager.GameFrame > ShowFrame)
		{
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Appear(delegate
				{
					OnToggleCharacterMaterial(true);
				});
			}
			_transform.SetPositionAndRotation(ShowPos, Quaternion.identity);
			Controller.LogicPosition = new VInt3(_transform.position);
			HasShow = true;
			base.AllowAutoAim = true;
			SetColliderEnable(true);
		}
		if (CanMove)
		{
			base.LogicUpdate();
		}
	}

	public override void SetActive(bool isActive)
	{
		if (IsStun)
		{
			SetStun(false);
		}
		try
		{
			if (StageUpdate.gStageName == "stage04_1401_e1" || StageUpdate.gStageName == "stage04_3301_e1")
			{
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_mob_explode_000", 5);
			}
		}
		catch (Exception)
		{
		}
		InGame = isActive;
		Controller.enabled = isActive;
		SetColliderEnable(isActive);
		MonoBehaviourSingleton<FxManager>.Instance.UpdateFx(FxArray, isActive);
		if (isActive)
		{
			AiTimer.TimerStart();
			_transform.SetParent(null);
			Controller.LogicPosition = new VInt3(_transform.localPosition);
			_velocityExtra = VInt3.zero;
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch984_Gate_000", ShowPos, Quaternion.identity, new object[1] { Vector3.one });
			ShowFrame = GameLogicUpdateManager.GameFrame + (int)(ShowTime * 20f);
			CanMove = false;
			HasShow = false;
			base.AllowAutoAim = false;
			SetColliderEnable(false);
			if ((bool)BodyMesh)
			{
				BodyMesh.enabled = false;
			}
		}
		else
		{
			Hp = 0;
			UpdateHurtAction();
			AiTimer.TimerStop();
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
			selfBuffManager.StopLoopSE();
			base.SoundSource.StopAll();
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Disappear(delegate
				{
					OnToggleCharacterMaterial(false);
					if (!InGame)
					{
						MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
					}
				});
			}
			else
			{
				OnToggleCharacterMaterial(false);
				MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, EnemyData.s_MODEL);
			}
			if (KeepSpawnedMob)
			{
				SpawnedMobList.Clear();
			}
			else
			{
				DestroyAllSpawnedMob();
			}
		}
		Activate = ManagedSingleton<StageHelper>.Instance.bEnemyActive && isActive;
		if (!isActive)
		{
			bNeedDead = false;
		}
		if (isActive)
		{
			ModelTransform.eulerAngles = ((_direction.x < 0f) ? new Vector3(0f, 180f, 0f) : new Vector3(0f, 135f, 0f));
			bWaitNetStatus = false;
			_animator.enabled = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.isForceSE = true;
			_collideBullet.Active(targetMask);
			AiTimer.TimerStart();
			AiTimer.SetMillisecondsOffset(EnemyData.n_AI_TIMER + 1);
		}
		else
		{
			TargetPos = VInt3.zero;
			_direction = TargetPos.vec3;
			_velocity = new VInt3(_direction * FlySpeed);
			_collideBullet.BackToPool();
			_animator.enabled = false;
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		ShowPos = pos;
		_direction = (bBack ? Vector3.left : Vector3.right);
		_velocity = new VInt3(_direction * FlySpeed);
		ModelTransform.eulerAngles = ((_direction.x < 0f) ? new Vector3(0f, 180f, 0f) : new Vector3(0f, 135f, 0f));
	}
}
