using System;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class Em008Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum EmState
	{
		INIT = 0,
		BORN = 1,
		IDLE = 2,
		HURT = 3,
		TRY_SKL_001 = 4,
		SKL_001 = 5,
		TRY_SKL_002 = 6,
		SKL_002 = 7,
		DIE = 8,
		TRY_SKL_001_NET = 9,
		TRY_SKL_002_NET = 10
	}

	private readonly int HASH_IDLE_LOOP = Animator.StringToHash("EM008@idle_loop");

	private readonly int HASH_HURT_LOOP = Animator.StringToHash("EM008@hurt_loop");

	private readonly int HASH_SKL01 = Animator.StringToHash("EM008@skl01");

	private readonly int HASH_SKL02 = Animator.StringToHash("EM008@skl02");

	private readonly int logicHurt = (int)(0.367f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int logicSkl01 = (int)(2.133f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int logicSkl02 = (int)(1.2f / GameLogicUpdateManager.m_fFrameLen);

	private EmState emState;

	[SerializeField]
	private SplashBullet splashBullet;

	[SerializeField]
	private int logicIdleLoop = 30;

	[SerializeField]
	private Transform shootTransform1;

	[SerializeField]
	private Transform shootTransform2;

	[SerializeField]
	private GameObject weaponRender;

	private int logicFrameNow;

	private int logicToNext;

	private int[] logicReloadFrame;

	private int[] NextReloadFrame;

	private RaycastHit2D hit;

	private float sensorDistance1 = 3.5f;

	private Vector2 skl01pos;

	[SerializeField]
	private int logicTrySkl001 = 10;

	[SerializeField]
	private int logicTrySkl002 = 5;

	[SerializeField]
	private float sensorAngle = 70f;

	[SerializeField]
	private int sensorDistance2 = 8;

	private Vector3 rayExtra = new Vector3(0f, 1f, 0f);

	protected override void Awake()
	{
		base.Awake();
		CreateSplashPoolObj();
		base.AimPoint = new Vector3(0f, 0.9f, 0f);
		skl01pos = new Vector2(Mathf.Abs(shootTransform1.localPosition.x - base.transform.localPosition.x) / 2f, 1f);
	}

	protected override void SetStunStatus(bool enable)
	{
		IsStunStatus = true;
		if (enable)
		{
			emState = EmState.HURT;
			_animator.Play(HASH_HURT_LOOP, 0, 0f);
		}
		else
		{
			emState = EmState.IDLE;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_velocity.x = 0;
			_velocity.y = 0;
			emState = EmState.INIT;
			MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
			_animator.enabled = true;
			emState = EmState.INIT;
		}
		else
		{
			_velocity.x = 0;
			_velocity.y = 0;
			MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
			_animator.Play(HASH_IDLE_LOOP, 0, 0f);
			_animator.enabled = false;
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		emState = (EmState)nSet;
		string[] array = smsg.Split(',');
		int num = int.Parse(array[0]);
		if (base.direction != num)
		{
			ReverseDirection();
		}
		if (array.Length >= 3)
		{
			hit = default(RaycastHit2D);
			float x = float.Parse(array[1]);
			float y = float.Parse(array[2]);
			hit.point = new Vector2(x, y);
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		logicReloadFrame = new int[EquippedWeaponNum];
		NextReloadFrame = new int[EquippedWeaponNum];
		for (int i = 0; i < EquippedWeaponNum; i++)
		{
			logicReloadFrame[i] = EnemyWeapons[i].BulletData.n_RELOAD / GameLogicUpdateManager.g_fixFrameLenFP.i;
			NextReloadFrame[i] = 0;
		}
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		logicFrameNow = GameLogicUpdateManager.GameFrame;
		switch (emState)
		{
		case EmState.INIT:
			hit = Physics2D.Raycast(_transform.localPosition, Vector2.down, float.MaxValue, (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.SemiBlockLayer));
			if ((bool)hit)
			{
				emState = EmState.BORN;
			}
			break;
		case EmState.BORN:
			if (Controller.Collisions.below)
			{
				Idle();
			}
			break;
		case EmState.IDLE:
			if (StageUpdate.bIsHost)
			{
				if (!Skill_01())
				{
					Skill_02();
				}
			}
			else if (bWaitNetStatus)
			{
				bWaitNetStatus = false;
			}
			break;
		case EmState.TRY_SKL_001_NET:
			NextReloadFrame[0] = logicFrameNow + logicReloadFrame[0];
			emState = EmState.TRY_SKL_001;
			_animator.Play(HASH_SKL01, 0, 0f);
			UpdateLogicToNext(logicTrySkl001);
			break;
		case EmState.TRY_SKL_002_NET:
			NextReloadFrame[1] = logicFrameNow + logicReloadFrame[1];
			emState = EmState.TRY_SKL_002;
			_animator.Play(HASH_SKL02, 0, 0f);
			UpdateLogicToNext(logicTrySkl002);
			break;
		case EmState.TRY_SKL_001:
			if (logicFrameNow > logicToNext)
			{
				UpdateLogicToNext(logicSkl01);
				emState = EmState.SKL_001;
				SetSplash();
			}
			break;
		case EmState.TRY_SKL_002:
			if (logicFrameNow > logicToNext)
			{
				weaponRender.SetActive(false);
				UpdateLogicToNext(logicSkl02);
				emState = EmState.SKL_002;
				Shoot_Skill_02();
			}
			break;
		case EmState.SKL_001:
		case EmState.SKL_002:
			if (logicFrameNow > logicToNext)
			{
				Idle();
			}
			break;
		}
		distanceDelta = Vector3.Distance(_transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
	}

	private void Idle()
	{
		_velocity.x = 0;
		_velocity.y = 0;
		emState = EmState.IDLE;
		_animator.Play(HASH_IDLE_LOOP, 0, 0f);
		weaponRender.SetActive(true);
	}

	private bool Skill_01()
	{
		if (logicFrameNow > NextReloadFrame[0])
		{
			if (Use_Skill_01(base.direction))
			{
				return true;
			}
			if (Use_Skill_01(-base.direction))
			{
				ReverseDirection();
				return true;
			}
			return false;
		}
		return false;
	}

	private bool Use_Skill_01(int direction)
	{
		if (SectorSensor.Look(Controller.LogicPosition.vec3 + rayExtra, direction, sensorAngle, sensorDistance1, targetMask, out hit))
		{
			if (bWaitNetStatus)
			{
				return true;
			}
			NextReloadFrame[0] = logicFrameNow + logicReloadFrame[0];
			string sOther = direction.ToString();
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, 9, sOther);
			return true;
		}
		return false;
	}

	private bool Skill_02()
	{
		if (logicFrameNow > NextReloadFrame[1])
		{
			if (Use_Skill_02(base.direction))
			{
				return true;
			}
			if (Use_Skill_02(-base.direction))
			{
				ReverseDirection();
				return true;
			}
			return false;
		}
		return false;
	}

	private bool Use_Skill_02(int direction)
	{
		if (SectorSensor.Look(Controller.LogicPosition.vec3 + rayExtra, direction, sensorAngle, sensorDistance2, targetMask, out hit))
		{
			if (bWaitNetStatus)
			{
				return true;
			}
			NextReloadFrame[1] = logicFrameNow + logicReloadFrame[1];
			string sOther = direction + "," + hit.point.x + "," + hit.point.y;
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, 10, sOther);
			return true;
		}
		return false;
	}

	private void Shoot_Skill_02()
	{
		SKILL_TABLE skillTable = EnemyWeapons[1].BulletData;
		CrossBullet bullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CrossBullet>(skillTable.s_MODEL);
		if ((bool)bullet)
		{
			string owner = EnemyData.n_ID.ToString();
			bullet.UpdateBulletData(skillTable, owner);
			bullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			bullet.transform.SetPositionAndRotation(shootTransform2.transform.position, Quaternion.identity);
			bullet.Active(hit.point, Vector3.right * base.direction, targetMask, null);
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/bullet/" + skillTable.s_MODEL, skillTable.s_MODEL, delegate(GameObject obj)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(obj);
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CrossBullet>(gameObject.GetComponent<CrossBullet>(), skillTable.s_MODEL);
			bullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CrossBullet>(skillTable.s_MODEL);
			if ((bool)bullet)
			{
				string owner2 = EnemyData.n_ID.ToString();
				bullet.UpdateBulletData(skillTable, owner2);
				bullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				bullet.transform.position = shootTransform2.transform.position;
				bullet.Active(hit.point, Vector3.right * base.direction, targetMask, null);
			}
		});
	}

	private void UpdateLogicToNext(int logicFrame)
	{
		logicToNext = logicFrameNow + logicFrame;
	}

	private void ReverseDirection()
	{
		base.direction *= -1;
		_transform.Rotate(new Vector3Int(0, 180, 0));
		_transform.localScale = new Vector3(1f, 1f, base.direction);
	}

	private void CreateSplashPoolObj()
	{
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<SplashBullet>(UnityEngine.Object.Instantiate(splashBullet), splashBullet.gameObject.name, 1);
	}

	private void SetSplash()
	{
		SplashBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<SplashBullet>(splashBullet.gameObject.name);
		poolObj._transform.SetParent(_transform);
		poolObj.UpdateBulletData(EnemyWeapons[0].BulletData);
		poolObj.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		poolObj._transform.localPosition = new Vector3(1f, 1f, 0f);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(EnemyWeapons[0].BulletData.s_HIT_FX, shootTransform1, Quaternion.identity, Array.Empty<object>());
		PlaySE(poolObj._HitGuardSE[0], poolObj._HitGuardSE[1], true);
		poolObj.isForceSE = true;
		poolObj.Active(targetMask, false);
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			base.direction = -1;
			base.transform.SetPositionAndRotation(pos, new Quaternion(0f, 180f, 0f, 0f));
		}
		else
		{
			base.direction = 1;
			base.transform.SetPositionAndRotation(pos, Quaternion.identity);
		}
		_transform.localScale = new Vector3(1f, 1f, base.direction);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}
}
