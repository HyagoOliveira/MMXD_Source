using CallbackDefs;
using StageLib;
using UnityEngine;

public class Em098Controller : Em015Controller
{
	[SerializeField]
	protected ParticleSystem bombFx;

	protected override void Awake()
	{
		base.Awake();
		DeadCallback = OnDead;
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		string[] array = smsg.Split(',');
		int x = int.Parse(array[0]);
		int logicFrame = int.Parse(array[1]);
		TargetPos = Controller.LogicPosition;
		TargetPos.x = x;
		int num = Controller.LogicPosition.x - TargetPos.x;
		base.direction = ((num <= 0) ? 1 : (-1));
		if (_moveSpeedMultiplier != 1f)
		{
			_velocity.x = moveSpdX * base.direction * (int)(_moveSpeedMultiplier * 100f) / 100;
		}
		else
		{
			_velocity.x = moveSpdX * base.direction;
		}
		if (nSet == 9 && !bombFx.IsAlive())
		{
			bombFx.Play();
			_animator.Play(HASH_RUN, 0, 0f);
		}
		emState = (EmState)nSet;
		UpdateLogicToNext(logicFrame);
	}

	public override void LogicUpdate()
	{
		logicFrameNow = GameLogicUpdateManager.GameFrame;
		if (!Activate)
		{
			return;
		}
		BaseUpdate();
		switch (emState)
		{
		case EmState.INIT:
			if (SectorSensor.Look(_transform.localPosition, rayDirection, 60f, 3000f, rayMask, out hit))
			{
				_velocity.x = 0;
				_velocity.y = -moveSpdY.i;
				TargetPos = new VInt3(hit.point);
				TargetPos.y += bornAddHeight.i - bornAddHeightPlus.i;
				emState = EmState.BORN;
			}
			break;
		case EmState.BORN:
		{
			int num = IntMath.Max(_velocity.y, TargetPos.y - Controller.LogicPosition.y);
			if (num >= 0)
			{
				num = bornAddHeightPlus.i;
				emState = EmState.IDLE_ARMOR;
			}
			Controller.LogicPosition.y += num;
			break;
		}
		case EmState.IDLE_ARMOR:
			bombFx.Play();
			_animator.Play(HASH_RUN, 0, 0f);
			if (StageUpdate.gbIsNetGame)
			{
				UpdateTarget();
				break;
			}
			emState = EmState.RUN;
			UpdateLogicToNext(logicRun);
			break;
		case EmState.IDLE_NULL:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.IDLE_ARMOR;
			}
			break;
		case EmState.PRE_SKL_01:
			if (logicFrameNow > logicToNext)
			{
				Fire();
			}
			break;
		case EmState.SKL_01:
			if (logicFrameNow > logicToNext)
			{
				_animator.Play(HASH_IDLE_NULL, 0);
				emState = EmState.IDLE_NULL;
				UpdateLogicToNext(logicIdle);
			}
			break;
		case EmState.RUN_NET:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.IDLE_ARMOR;
			}
			else if (Mathf.Abs(Controller.LogicPosition.x - TargetPos.x) > moveSpdX.i)
			{
				Controller.LogicPosition.x += _velocity.x;
			}
			else if (Mathf.Abs(Controller.LogicPosition.x - TargetPos.x) > 0)
			{
				Controller.LogicPosition.x = TargetPos.x;
			}
			else
			{
				Fire();
			}
			break;
		case EmState.RUN:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.IDLE_ARMOR;
			}
			else
			{
				UpdateTarget();
				GeneratorRay();
			}
			Controller.LogicPosition.x += _velocity.x;
			break;
		}
		distanceDelta = Vector3.Distance(_transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
	}

	protected override void SetBulletData(BombBullet bullet)
	{
		bullet._transform.SetParent(bombBone);
		bullet.transform.localRotation = Quaternion.identity;
		bullet._transform.localPosition = Vector3.zero;
		bullet.animationLength[0] = aniSkl01_wait;
		bullet.animationLength[1] = aniSkl01_drop;
		bullet.RefMob = EnemyData;
		bullet.RefBuffStatus = selfBuffManager.sBuffStatus;
		bullet.UpdateBulletData(EnemyWeapons[0].BulletData);
		bullet.hitCB = delegate
		{
			bombFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		};
		bullet.Active(dropBottomPos, Vector3.down, (int)targetMask | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer));
	}

	protected void OnDead()
	{
		bombFx.Stop();
	}
}
