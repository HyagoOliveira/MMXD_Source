using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class EM068_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		SK_1 = 1,
		SK_2 = 2,
		IdleWaitNet = 3
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		MAX_SUBSTATUS = 4
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_SK_1 = 1,
		ANI_SK_2 = 2,
		ANI_HURT = 3,
		MAX_ANIMATION_ID = 4
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	public int WalkSpeed;

	private readonly int[] AttackCDFrame = new int[2]
	{
		(int)(0.75f / GameLogicUpdateManager.m_fFrameLen),
		(int)(4f / GameLogicUpdateManager.m_fFrameLen)
	};

	protected readonly float[] SkillDistance = new float[2] { 16f, 20f };

	private int[] CDOverFrame = new int[2];

	private int logicFrameNow;

	private bool _isAction;

	private bool _isUp = true;

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private Transform[] KannonGun = new Transform[2];

	private Transform[] MissileGun = new Transform[2];

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
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		KannonGun[0] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Up", true);
		KannonGun[1] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Down", true);
		MissileGun[0] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Right", true);
		MissileGun[1] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Left", true);
		_animationHash = new int[4];
		_animationHash[0] = Animator.StringToHash("EM068@idle_loop");
		_animationHash[1] = Animator.StringToHash("EM068@skill_01");
		_animationHash[2] = Animator.StringToHash("EM068@skill_02");
		_animationHash[3] = Animator.StringToHash("EM068@hurt_loop");
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		base.AimPoint = new Vector3(0f, 1.2f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(20f, 10f);
		FallDownSE = new string[2] { "EnemySE02", "em024_cndriver03" };
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (smsg != null && smsg != "")
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
			Controller.LogicPosition.x = netSyncData.SelfPosX;
			Controller.LogicPosition.y = netSyncData.SelfPosY;
			Controller.LogicPosition.z = netSyncData.SelfPosZ;
			TargetPos.x = netSyncData.TargetPosX;
			TargetPos.y = netSyncData.TargetPosY;
			TargetPos.z = netSyncData.TargetPosZ;
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
			UpdateDirection();
		}
		if (AiState == AI_STATE.mob_002)
		{
			SetStatus(MainStatus.Idle);
		}
		else
		{
			SetStatus((MainStatus)nSet);
		}
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (TargetPos.x > Controller.LogicPosition.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			UpdateDirection();
			_velocity.x = 0;
			break;
		case MainStatus.SK_1:
			_isUp = true;
			UpdateDirection();
			break;
		case MainStatus.SK_2:
			PlaySE("EnemySE02", "em024_cndriver02");
			UpdateDirection();
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.SK_1:
			_currentAnimationId = AnimationID.ANI_SK_1;
			break;
		case MainStatus.SK_2:
			_currentAnimationId = AnimationID.ANI_SK_2;
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (AiState != AI_STATE.mob_002)
			{
				logicFrameNow = GameLogicUpdateManager.GameFrame;
				if (TrySK_2() && logicFrameNow > CDOverFrame[1])
				{
					mainStatus = MainStatus.SK_2;
				}
				else if (TrySK_1() && logicFrameNow > CDOverFrame[0])
				{
					mainStatus = MainStatus.SK_1;
				}
			}
			break;
		case MainStatus.SK_1:
			UpdateDirection();
			logicFrameNow = GameLogicUpdateManager.GameFrame;
			if (_currentFrame > 1f && _isAction)
			{
				_isAction = false;
				SetStatus(MainStatus.Idle);
			}
			else if (logicFrameNow > CDOverFrame[0] && !_isAction)
			{
				ShootSkill_1(LastTargetPos);
				CDOverFrame[0] = logicFrameNow + AttackCDFrame[0];
			}
			break;
		case MainStatus.SK_2:
			if (_currentFrame > 1f)
			{
				CDOverFrame[1] = logicFrameNow + AttackCDFrame[1];
				SetStatus(MainStatus.Idle);
				_isAction = false;
			}
			else if (_currentFrame > 0.3f && !_isAction)
			{
				ShootSkill_2();
				_isAction = true;
			}
			break;
		}
		if (AiState == AI_STATE.mob_002)
		{
			mainStatus = MainStatus.Idle;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
	}

	protected virtual bool TrySK_1()
	{
		Target = _enemyAutoAimSystem.GetClosetPlayer();
		if ((bool)Target)
		{
			return true;
		}
		return false;
	}

	protected virtual bool TrySK_2()
	{
		Target = _enemyAutoAimSystem.GetClosetPlayer();
		if ((bool)Target)
		{
			return true;
		}
		return false;
	}

	protected virtual void ShootSkill_1(Vector3 EndPoint)
	{
		if (_isUp)
		{
			BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, KannonGun[0].transform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
			_isUp = false;
		}
		else
		{
			BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, KannonGun[1].transform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
			_isUp = true;
			_isAction = true;
		}
	}

	protected virtual void ShootSkill_2()
	{
		PlaySE("EnemySE02", "em024_cndriver01");
		BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, MissileGun[0].transform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, MissileGun[1].transform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_animator.enabled = isActive;
		UpdateAIState();
		if (isActive)
		{
			SkillDistance[0] = EnemyWeapons[1].BulletData.f_DISTANCE;
			SkillDistance[1] = EnemyWeapons[2].BulletData.f_DISTANCE;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			if (AiState == AI_STATE.mob_002)
			{
				_collideBullet.BackToPool();
			}
		}
		else
		{
			_collideBullet.BackToPool();
		}
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
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * -1f);
		base.transform.position = pos;
	}
}
