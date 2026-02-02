using CallbackDefs;
using StageLib;
using UnityEngine;

public class EM039_Controller : EnemyLoopBase, IManagedUpdateBehavior
{
	private enum EmState
	{
		INIT = 0,
		BORN = 1,
		IDLE = 2,
		HURT = 3,
		SKILL0 = 4,
		RUN = 5,
		WAIT_NET = 6,
		RUN_NET = 7
	}

	private readonly int HASH_IDLE = Animator.StringToHash("EM039@idle_loop");

	private readonly int HASH_HURT = Animator.StringToHash("EM039@hurt_loop");

	private readonly int HASH_SKILL0 = Animator.StringToHash("EM039@skill_01");

	private readonly int HASH_RUN = Animator.StringToHash("EM039@run_loop");

	private Transform modelTransform;

	private Transform shootPointTransform;

	private EmState emState;

	private VInt2 moveSpd = new VInt2(new Vector2(0.32f, 0.2f));

	private RaycastHit2D hit;

	private Vector2 rayDirection = Vector2.down;

	private float rayLength = 100f;

	private int rayMask;

	private VInt bornAddHeight = new VInt(0.25f);

	private VInt bornAddHeightPlus = new VInt(0.3f);

	private VInt3 shiftPos = new VInt3(new Vector3(2f, -1f, 0f));

	private VInt3 targetPos = VInt3.zero;

	private int logicFrameNow;

	private int logicToNext;

	private int logicToShoot;

	private int magazine;

	private int targetIdx = -1;

	[SerializeField]
	private int logicIdle = 50;

	[SerializeField]
	private int logicSkl01 = 20;

	[SerializeField]
	private int logicRun = 40;

	[SerializeField]
	private int logicShoot = 14;

	protected override void Awake()
	{
		base.Awake();
		rayMask = 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer;
		_animator = GetComponentInChildren<Animator>();
		IgnoreGravity = true;
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		modelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		base.AimTransform = modelTransform;
		shootPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint", true);
	}

	protected override void SetStunStatus(bool enable)
	{
		IsStunStatus = true;
		if (enable)
		{
			emState = EmState.HURT;
			_animator.Play(HASH_HURT, 0, 0f);
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
			emState = EmState.INIT;
			MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
			_animator.enabled = true;
			_animator.Play(HASH_IDLE, 0, 0f);
		}
		else
		{
			MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
			_animator.Play(HASH_IDLE, 0, 0f);
			_animator.enabled = false;
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		string[] array = smsg.Split(',');
		int x = int.Parse(array[0]);
		int y = int.Parse(array[1]);
		targetPos.x = x;
		targetPos.y = y;
		VInt3 vInt = Controller.LogicPosition - targetPos;
		base.direction = ((vInt.x <= 0) ? 1 : (-1));
		if (vInt.x > 0)
		{
			if (vInt.x - shiftPos.x > moveSpd.x)
			{
				_velocity.x = -moveSpd.x;
			}
			else
			{
				_velocity.x = -(vInt.x - shiftPos.x);
			}
		}
		else if (IntMath.Abs(vInt.x + shiftPos.x) > moveSpd.x)
		{
			_velocity.x = moveSpd.x;
		}
		else
		{
			_velocity.x = -(vInt.x + shiftPos.x);
		}
		if (IntMath.Abs(vInt.y + shiftPos.y) > moveSpd.y)
		{
			_velocity.y = IntMath.Min(moveSpd.y * -IntMath.Sign(vInt.y + shiftPos.y), moveSpd.y);
		}
		else
		{
			_velocity.y = -(vInt.y + shiftPos.y);
		}
		modelTransform.localScale = new Vector3(modelTransform.localScale.x, modelTransform.localScale.y, Mathf.Abs(modelTransform.localScale.z) * (float)base.direction);
		emState = (EmState)nSet;
		UpdateLogicToNext(logicRun);
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
		if (_velocityExtra != VInt3.zero)
		{
			_velocityExtra *= 0.1f;
		}
		BaseLogicUpdate(true);
		bool flag = false;
		bool flag2 = false;
		switch (emState)
		{
		case EmState.INIT:
			if (SectorSensor.Look(_transform.localPosition, rayDirection, 60f, 30f, rayMask, out hit))
			{
				logicFrameNow = GameLogicUpdateManager.GameFrame;
				_velocity.x = 0;
				_velocity.y = -moveSpd.y;
				targetPos = new VInt3(hit.point);
				targetPos.y += bornAddHeight.i - bornAddHeightPlus.i;
				emState = EmState.BORN;
			}
			break;
		case EmState.BORN:
		{
			int num = IntMath.Max(_velocity.y, targetPos.y - Controller.LogicPosition.y);
			if (num >= 0)
			{
				num = bornAddHeightPlus.i;
				emState = EmState.IDLE;
			}
			Controller.LogicPosition.y += num;
			break;
		}
		case EmState.IDLE:
			if (StageUpdate.gbIsNetGame)
			{
				if (StageUpdate.bIsHost)
				{
					UpdateTarget();
					emState = EmState.WAIT_NET;
					StageUpdate.RegisterSendAndRun(sNetSerialID, 7, targetPos.x + "," + targetPos.y);
					_velocity.x = 0;
					_velocity.y = 0;
				}
			}
			else
			{
				UpdateLogicToNext(logicRun);
				_animator.Play(HASH_RUN, 0, 0f);
				emState = EmState.RUN;
			}
			break;
		case EmState.SKILL0:
			if (logicFrameNow > logicToNext)
			{
				_animator.Play(HASH_IDLE, 0);
				emState = EmState.IDLE;
				UpdateLogicToNext(logicIdle);
			}
			if (logicFrameNow > logicToShoot && magazine > 0)
			{
				BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, shootPointTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				magazine--;
			}
			break;
		case EmState.RUN_NET:
		{
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.IDLE;
				break;
			}
			flag = false;
			flag2 = false;
			if (IntMath.Abs(Controller.LogicPosition.x - targetPos.x) <= shiftPos.x)
			{
				flag = true;
			}
			if (IntMath.Abs(Controller.LogicPosition.y - targetPos.y) <= IntMath.Abs(shiftPos.y))
			{
				flag2 = true;
			}
			if (flag && flag2)
			{
				UseSkill0();
				break;
			}
			VInt3 vInt = Controller.LogicPosition - targetPos;
			if (base.direction == -1)
			{
				if (vInt.x > 0)
				{
					if (vInt.x - shiftPos.x > moveSpd.x)
					{
						_velocity.x = -moveSpd.x;
					}
					else
					{
						_velocity.x = -(vInt.x - shiftPos.x);
					}
				}
				else
				{
					_velocity.x = 0;
				}
			}
			else if (vInt.x < 0)
			{
				if (IntMath.Abs(vInt.x + shiftPos.x) > moveSpd.x)
				{
					_velocity.x = moveSpd.x;
				}
				else
				{
					_velocity.x = -(vInt.x + shiftPos.x);
				}
			}
			else
			{
				_velocity.x = 0;
			}
			if (IntMath.Abs(vInt.y + shiftPos.y) > moveSpd.y)
			{
				_velocity.y = IntMath.Min(moveSpd.y * -IntMath.Sign(vInt.y + shiftPos.y), moveSpd.y);
			}
			else
			{
				_velocity.y = -(vInt.y + shiftPos.y);
			}
			break;
		}
		case EmState.RUN:
			if (logicFrameNow > logicToNext)
			{
				emState = EmState.IDLE;
			}
			else
			{
				GeneratorRay();
			}
			break;
		}
		if (!StageUpdate.gbIsNetGame)
		{
			UpdateTarget();
		}
		distanceDelta = Vector3.Distance(_transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
	}

	private void UpdateTarget()
	{
		if (emState == EmState.SKILL0)
		{
			return;
		}
		targetIdx = -1;
		int num = int.MaxValue;
		VInt3 vInt = VInt3.zero;
		if (OrangeBattleUtility.ListPlayer != null)
		{
			for (int i = 0; i < OrangeBattleUtility.ListPlayer.Count; i++)
			{
				if ((int)OrangeBattleUtility.ListPlayer[i].Hp > 0)
				{
					int num2 = IntMath.Abs(Controller.LogicPosition.x - OrangeBattleUtility.ListPlayer[i].Controller.LogicPosition.x);
					if (num2 < num)
					{
						targetIdx = i;
						num = num2;
					}
				}
			}
			if (targetIdx != -1)
			{
				OrangeCharacter orangeCharacter = OrangeBattleUtility.ListPlayer[targetIdx];
				targetPos = new VInt3(orangeCharacter.AimTransform.position + Vector3.down * 1.3f);
				vInt = Controller.LogicPosition - targetPos;
				base.direction = ((vInt.x <= 0) ? 1 : (-1));
			}
			else
			{
				targetPos = Controller.LogicPosition;
			}
		}
		else
		{
			targetPos = Controller.LogicPosition;
		}
		if (IntMath.Abs(vInt.x + shiftPos.x) > moveSpd.x)
		{
			_velocity.x = moveSpd.x * -IntMath.Sign(vInt.x + shiftPos.x);
		}
		else
		{
			_velocity.x = -(vInt.x + shiftPos.x);
		}
		if (IntMath.Abs(vInt.y + shiftPos.y) > moveSpd.y)
		{
			_velocity.y = IntMath.Min(moveSpd.y * -IntMath.Sign(vInt.y + shiftPos.y), moveSpd.y);
		}
		else
		{
			_velocity.y = -(vInt.y + shiftPos.y);
		}
		modelTransform.localScale = new Vector3(modelTransform.localScale.x, modelTransform.localScale.y, Mathf.Abs(modelTransform.localScale.z) * (float)base.direction);
	}

	private void GeneratorRay()
	{
		SKILL_TABLE bulletData = EnemyWeapons[0].BulletData;
		hit = Physics2D.Raycast(shootPointTransform.position, Vector2.right * base.direction, bulletData.f_DISTANCE, targetMask);
		if ((bool)hit)
		{
			UseSkill0();
		}
	}

	private void UseSkill0()
	{
		_velocity.x = 0;
		_velocity.y = 0;
		UpdateLogicToNext(logicSkl01);
		logicToShoot = logicFrameNow + logicShoot;
		magazine = 3;
		_animator.Play(HASH_SKILL0, 0, 0f);
		emState = EmState.SKILL0;
	}

	private void UpdateLogicToNext(int logicFrame)
	{
		logicToNext = logicFrameNow + logicFrame;
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

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		modelTransform.localScale = new Vector3(modelTransform.localScale.x, modelTransform.localScale.y, Mathf.Abs(modelTransform.localScale.z) * (float)base.direction);
		base.transform.position = pos;
	}
}
