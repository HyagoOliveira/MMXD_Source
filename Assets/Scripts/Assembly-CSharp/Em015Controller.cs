using System;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class Em015Controller : EnemyLoopBase, IManagedUpdateBehavior
{
	protected enum EmState
	{
		INIT = 0,
		BORN = 1,
		IDLE_ARMOR = 2,
		IDLE_NULL = 3,
		HURT = 4,
		PRE_SKL_01 = 5,
		SKL_01 = 6,
		RUN = 7,
		WAIT_NET = 8,
		RUN_NET = 9
	}

	protected readonly string STR_SKL_01 = "EM015@skl01";

	protected readonly int HASH_IDLE_ARMOR = Animator.StringToHash("EM015@idle1_loop");

	protected readonly int HASH_IDLE_NULL = Animator.StringToHash("EM015@idle2_loop");

	protected readonly int HASH_HURT = Animator.StringToHash("EM015@hurt_loop");

	protected readonly int HASH_SKL_01 = Animator.StringToHash("EM015@skl01");

	protected readonly int HASH_RUN = Animator.StringToHash("EM015@run_loop");

	protected EmState emState;

	protected EmState oldemState;

	protected VInt moveSpdX = new VInt(0.32f);

	protected VInt moveSpdY = new VInt(1f);

	protected RaycastHit2D hit;

	protected Vector2 rayDirection = Vector2.down;

	private float rayLength = 100f;

	protected int rayMask;

	protected VInt bornAddHeight = new VInt(5.5f);

	protected VInt bornAddHeightPlus = new VInt(0.3f);

	protected int logicFrameNow;

	protected int logicToNext;

	protected float aniSkl01_wait = 71f / (226f * (float)Math.PI);

	protected float aniSkl01_drop = 0.6333334f;

	[SerializeField]
	protected Transform dropBottomPos;

	[SerializeField]
	protected int logicIdle = 50;

	[SerializeField]
	protected int logicSkl01 = 20;

	[SerializeField]
	protected int logicRun = 40;

	[SerializeField]
	protected int logicWaitSkl = 10;

	[SerializeField]
	protected SkinnedMeshRenderer bombMesh;

	[SerializeField]
	protected Transform bombBone;

	protected override void Awake()
	{
		base.Awake();
		rayMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.SemiBlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer);
		bombBone = OrangeBattleUtility.FindChildRecursive(base.transform, "bomb_bone", true);
	}

	protected override void SetStunStatus(bool enable)
	{
		IsStunStatus = true;
		if (enable)
		{
			emState = EmState.HURT;
			_animator.Play(HASH_HURT, 0, 0f);
			oldemState = emState;
		}
		else if (oldemState == EmState.IDLE_ARMOR || oldemState == EmState.IDLE_NULL)
		{
			emState = oldemState;
		}
		else
		{
			emState = EmState.IDLE_NULL;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			emState = EmState.INIT;
			MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
			_animator.enabled = true;
			_animator.Play(HASH_IDLE_ARMOR, 0, 0f);
		}
		else
		{
			MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
			_animator.Play(HASH_IDLE_ARMOR, 0, 0f);
			_animator.enabled = false;
		}
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
		if (nSet == 9 && !bombMesh.enabled)
		{
			bombMesh.enabled = true;
			_animator.Play(HASH_RUN, 0, 0f);
		}
		emState = (EmState)nSet;
		UpdateLogicToNext(logicFrame);
	}

	public override void BackToPool()
	{
		base.BackToPool();
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
			bombMesh.enabled = true;
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

	protected void UpdateTarget()
	{
		if (emState == EmState.SKL_01)
		{
			return;
		}
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			TargetPos = VInt3.zero;
			if (OrangeBattleUtility.ListPlayer != null)
			{
				OrangeCharacter nearestPlayerByVintPos = StageUpdate.GetNearestPlayerByVintPos(Controller.LogicPosition, (int)fAIWorkRange);
				if (nearestPlayerByVintPos != null)
				{
					if (nearestPlayerByVintPos.UsingVehicle && nearestPlayerByVintPos.refRideBaseObj != null)
					{
						TargetPos = nearestPlayerByVintPos.refRideBaseObj.Controller.LogicPosition;
					}
					else
					{
						TargetPos = nearestPlayerByVintPos.Controller.LogicPosition;
					}
					int num = Controller.LogicPosition.x - nearestPlayerByVintPos.Controller.LogicPosition.x;
					base.direction = ((num <= 0) ? 1 : (-1));
				}
				else
				{
					TargetPos = Controller.LogicPosition;
				}
				bWaitNetStatus = true;
				StageUpdate.RegisterSendAndRun(sNetSerialID, 9, TargetPos.x.ToString() + "," + logicRun);
			}
			else
			{
				TargetPos = Controller.LogicPosition;
			}
		}
		else
		{
			bWaitNetStatus = false;
		}
	}

	protected void GeneratorRay()
	{
		hit = Physics2D.Raycast(Controller.LogicPosition.vec3, rayDirection, rayLength, targetMask);
		if ((bool)hit)
		{
			Stop();
		}
	}

	private void Stop()
	{
		_velocity.x = 0;
		UpdateLogicToNext(logicWaitSkl);
		_animator.Play(HASH_IDLE_ARMOR, 0, 0f);
		emState = EmState.PRE_SKL_01;
	}

	protected void Fire()
	{
		_velocity.x = 0;
		UpdateLogicToNext(logicSkl01);
		_animator.Play(HASH_SKL_01, 0, 0f);
		emState = EmState.SKL_01;
		SKILL_TABLE skillTable = EnemyWeapons[0].BulletData;
		skillTable.s_MODEL = "BombBullet";
		BombBullet bullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BombBullet>(skillTable.s_MODEL);
		bullet.isForceSE = true;
		if ((bool)bullet)
		{
			SetBulletData(bullet);
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/bullet/" + skillTable.s_MODEL, skillTable.s_MODEL, delegate(GameObject obj)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(obj);
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BombBullet>(gameObject.GetComponent<BombBullet>(), skillTable.s_MODEL);
			bullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BombBullet>(skillTable.s_MODEL);
			if ((bool)bullet)
			{
				SetBulletData(bullet);
			}
		});
	}

	protected virtual void SetBulletData(BombBullet bullet)
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
			bombMesh.enabled = false;
		};
		bullet.Active(dropBottomPos, Vector3.down, (int)targetMask | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer));
	}

	protected void UpdateLogicToNext(int logicFrame)
	{
		logicToNext = GameLogicUpdateManager.GameFrame + logicFrame;
	}

	public override void UpdateFunc()
	{
		base.UpdateFunc();
		_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}
}
