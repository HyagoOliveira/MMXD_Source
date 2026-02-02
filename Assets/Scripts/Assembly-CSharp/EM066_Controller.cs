using CallbackDefs;
using StageLib;
using UnityEngine;

public class EM066_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		DetectAndShoot = 1,
		WaitForNextShoot = 2,
		IdleWaitNet = 3
	}

	[SerializeField]
	private Transform modelTransform;

	[SerializeField]
	private Transform shootPointTransform;

	[SerializeField]
	private Transform colliderTransform;

	[SerializeField]
	private float modelOffset;

	[SerializeField]
	private int shootCooldownTime;

	private MainStatus mainStatus;

	private BasicBullet shootBullet;

	private RaycastHit2D[] hitInfos;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		ModelTransform = modelTransform;
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		base.AimTransform = colliderTransform;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		mainStatus = MainStatus.Idle;
		IgnoreGravity = true;
		AiTimer.TimerStart();
	}

	public override void LogicUpdate()
	{
		if (Activate && (bool)_enemyAutoAimSystem)
		{
			BaseLogicUpdate();
			UpdateStatusLogic();
		}
	}

	public void UpdateFunc()
	{
	}

	public override void SetActive(bool _isActive)
	{
		base.SetActive(_isActive);
		shootBullet = null;
		if (_isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			StickOnWall();
			SetStatus(MainStatus.Idle);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	private void RegisterStatus(MainStatus _mainStatus)
	{
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)_mainStatus);
				mainStatus = MainStatus.IdleWaitNet;
			}
		}
		else
		{
			SetStatus(_mainStatus);
		}
	}

	public override void UpdateEnemyID(int _id)
	{
		base.UpdateEnemyID(_id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(EnemyWeapons[1].BulletData.f_DISTANCE);
	}

	public override void UpdateStatus(int _nSet, string _smsg, Callback _callback = null)
	{
		SetStatus((MainStatus)_nSet);
	}

	private void SetStatus(MainStatus _mainStatus)
	{
		mainStatus = _mainStatus;
		AiTimer.TimerStart();
	}

	private void StickOnWall()
	{
		if (hitInfos == null)
		{
			hitInfos = new RaycastHit2D[4];
		}
		Vector3 vector = _transform.position + new Vector3(999f, 999f, 999f);
		for (int i = 0; i < hitInfos.Length; i++)
		{
			hitInfos[i] = Physics2D.Raycast(_transform.position, Quaternion.Euler(0f, 0f, 90 * i) * Vector2.up, 100f, Controller.collisionMask);
			if (hitInfos[i].collider != null && Vector2.Distance(_transform.position, hitInfos[i].point) < Vector2.Distance(_transform.position, vector))
			{
				vector = hitInfos[i].point;
			}
		}
		modelTransform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.up, modelTransform.position - vector));
		vector += (_transform.position - vector).normalized * modelOffset;
		Controller.LogicPosition = new VInt3(vector);
		_transform.position = vector;
	}

	private void UpdateStatusLogic()
	{
		switch (mainStatus)
		{
		case MainStatus.Idle:
			RegisterStatus(MainStatus.DetectAndShoot);
			break;
		case MainStatus.DetectAndShoot:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (Target != null && shootBullet == null)
			{
				float z = -45f;
				if (Vector3.Dot(modelTransform.right, Target._transform.position - modelTransform.position) < 0f)
				{
					z = 45f;
				}
				Vector3 pDirection = Quaternion.Euler(0f, 0f, z) * modelTransform.up;
				shootBullet = (BasicBullet)BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, shootPointTransform, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				if (shootBullet != null)
				{
					shootBullet.FreeDISTANCE = (float)(shootBullet.GetBulletData.n_SPEED * shootCooldownTime) * 0.001f;
				}
				RegisterStatus(MainStatus.WaitForNextShoot);
			}
			break;
		case MainStatus.WaitForNextShoot:
			if (shootBullet != null)
			{
				if (AiTimer.GetMillisecond() >= shootCooldownTime)
				{
					shootBullet.BackToPool();
					shootBullet = null;
				}
			}
			else
			{
				RegisterStatus(MainStatus.DetectAndShoot);
			}
			break;
		}
	}
}
