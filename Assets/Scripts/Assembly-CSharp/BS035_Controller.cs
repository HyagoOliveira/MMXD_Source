using System;
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class BS035_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Hurt = 2,
		Dead = 3,
		Run = 4,
		RunBack = 5,
		Skill0 = 6,
		Skill1 = 7,
		IdleWaitNet = 8,
		MAX_STATUS = 9
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
		ANI_DEBUT = 1,
		ANI_HURT = 2,
		ANI_RUN = 3,
		ANI_RUNBACK = 4,
		ANI_DEAD = 5,
		ANI_SKILL0_START = 6,
		ANI_SKILL0_LOOP = 7,
		ANI_SKILL0_END = 8,
		ANI_SKILL1_START = 9,
		ANI_SKILL1_LOOP = 10,
		ANI_SKILL1_END = 11,
		MAX_ANIMATION_ID = 12
	}

	private AnimationID _currentAnimationId;

	public int RunSpeed = 1700;

	public int RunBackSpeed = 1700;

	private Transform _mouthShootTransform;

	private Transform _backLeftShootTransform;

	private Transform _backRightShootTransform;

	private int[] _animatorHash;

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private float _currentFrame;

	private bool _bDeadCallResult = true;

	private bool _jumpFlag;

	public bool DebugMissile = true;

	private Vector3 distance;

	private int _prevWalkDirection = 1;

	private int useWeapon;

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
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "BodyCollider");
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "BodyCollider").gameObject.AddOrGetComponent<CollideBullet>();
		_mouthShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Mouth");
		_backLeftShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Back_Left");
		_backRightShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Back_Right");
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[12];
		for (int i = 0; i < 12; i++)
		{
			_animatorHash[i] = Animator.StringToHash("idle");
		}
		_animatorHash[0] = Animator.StringToHash("BS035@idle_loop");
		_animatorHash[1] = Animator.StringToHash("BS035@debut");
		_animatorHash[2] = Animator.StringToHash("BS035@hurt_loop");
		_animatorHash[5] = Animator.StringToHash("BS035@dead");
		_animatorHash[3] = Animator.StringToHash("BS035@run_forward_loop");
		_animatorHash[4] = Animator.StringToHash("BS035@run_backward_loop");
		_animatorHash[6] = Animator.StringToHash("BS035@skill_01_start");
		_animatorHash[7] = Animator.StringToHash("BS035@skill_01_loop");
		_animatorHash[8] = Animator.StringToHash("BS035@skill_01_end");
		_animatorHash[9] = Animator.StringToHash("BS035@skill_02_start");
		_animatorHash[10] = Animator.StringToHash("BS035@skill_02_loop");
		_animatorHash[11] = Animator.StringToHash("BS035@skill_02_end");
		SetStatus(MainStatus.Debut);
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(15f);
		AI_STATE aiState = AI_STATE.mob_001;
		if (EnemyData.s_AI != "null")
		{
			aiState = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
		}
		AiState = aiState;
		AI_STATE aiState2 = AiState;
		if (aiState2 == AI_STATE.mob_002)
		{
			_bDeadCallResult = false;
		}
		else
		{
			_bDeadCallResult = true;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase3 && IntroCallBack != null)
			{
				IntroCallBack();
			}
			break;
		}
		case MainStatus.Idle:
			_velocity.x = 0;
			break;
		case MainStatus.Run:
			_prevWalkDirection = 1;
			_velocity.x = base.direction * RunSpeed;
			break;
		case MainStatus.RunBack:
			_prevWalkDirection = -1;
			_velocity.x = -base.direction * RunBackSpeed;
			break;
		case MainStatus.Skill0:
			UpdateMagazine(1, true);
			_velocity.x = 0;
			break;
		case MainStatus.Skill1:
			UpdateMagazine(2, true);
			_velocity.x = 0;
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity.x = 0;
				OrangeBattleUtility.LockPlayer();
				break;
			case SubStatus.Phase1:
				if (_bDeadCallResult)
				{
					StartCoroutine(BossDieFlow(base.AimTransform));
				}
				else
				{
					StartCoroutine(BossDieFlow(base.AimTransform, "FX_BOSS_EXPLODE2", false, false));
				}
				break;
			}
			break;
		}
		UpdateAnimation();
	}

	public override void LogicUpdate()
	{
		if (_mainStatus == MainStatus.Debut)
		{
			BaseUpdate();
			UpdateGravity();
			Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
		if ((!Activate || !_enemyAutoAimSystem) && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		UpdateMagazine();
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_introReady)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_unlockReady)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				useWeapon = 1;
				if (EnemyWeapons[useWeapon].MagazineRemain > 0f && DebugMissile && (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED))
				{
					Transform mouthShootTransform = _mouthShootTransform;
					BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, mouthShootTransform, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
					EnemyWeapons[useWeapon].MagazineRemain -= 1f;
				}
				if (EnemyWeapons[useWeapon].MagazineRemain == 0f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					AiTimer.TimerStart();
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (AiTimer.GetMillisecond() >= EnemyData.n_AI_TIMER)
			{
				SetStatus((_prevWalkDirection == 1) ? MainStatus.RunBack : MainStatus.Run);
			}
			break;
		case MainStatus.Run:
			if ((bool)Physics2D.Raycast(Controller.GetBounds().center, Vector2.right * base.direction, 6f, Controller.collisionMask))
			{
				SetStatus(MainStatus.Skill0);
			}
			useWeapon = 2;
			if (EnemyWeapons[useWeapon].MagazineRemain > 0f && DebugMissile && (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED))
			{
				int num = (int)EnemyWeapons[useWeapon].MagazineRemain % 2;
				BulletBase.TryShotBullet(pTransform: (num != 0 && num == 1) ? _backRightShootTransform : _backLeftShootTransform, tSkillTable: EnemyWeapons[useWeapon].BulletData, pDirection: new Vector2(-base.direction, 1f), weaponStatus: null, tBuffStatus: selfBuffManager.sBuffStatus, refMOB_TABLE: EnemyData, pTargetMask: targetMask);
				EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
				EnemyWeapons[useWeapon].MagazineRemain -= 1f;
			}
			break;
		case MainStatus.RunBack:
			if ((bool)Physics2D.Raycast(Controller.GetBounds().center, Vector2.right * -base.direction, 5f, Controller.collisionMask))
			{
				SetStatus(MainStatus.Skill0);
			}
			useWeapon = 2;
			if (EnemyWeapons[useWeapon].MagazineRemain > 0f && (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED))
			{
				int num = (int)EnemyWeapons[useWeapon].MagazineRemain % 2;
				BulletBase.TryShotBullet(pTransform: (num != 0 && num == 1) ? _backRightShootTransform : _backLeftShootTransform, tSkillTable: EnemyWeapons[useWeapon].BulletData, pDirection: new Vector2(-base.direction, 1f), weaponStatus: null, tBuffStatus: selfBuffManager.sBuffStatus, refMOB_TABLE: EnemyData, pTargetMask: targetMask);
				EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
				EnemyWeapons[useWeapon].MagazineRemain -= 1f;
			}
			break;
		case MainStatus.Dead:
			if (_subStatus == SubStatus.Phase0 && (double)_currentFrame > 0.4)
			{
				SetStatus(_mainStatus, SubStatus.Phase1);
			}
			break;
		default:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.IdleWaitNet:
			break;
		}
	}

	public override void UpdateStatus(int nSet, string sMsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (!string.IsNullOrEmpty(sMsg))
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(sMsg);
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
		}
		SetStatus((MainStatus)nSet);
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase2:
			case SubStatus.Phase3:
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Idle:
		case MainStatus.IdleWaitNet:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Dead:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			}
			return;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		case MainStatus.Run:
			_currentAnimationId = AnimationID.ANI_RUN;
			break;
		case MainStatus.RunBack:
			_currentAnimationId = AnimationID.ANI_RUNBACK;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		_animator.Play(_animatorHash[(int)_currentAnimationId], 0, 0f);
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		base.transform.position = pos;
	}

	public override void BossIntro(Action cb)
	{
		if (_mainStatus == MainStatus.Debut)
		{
			_introReady = true;
			IntroCallBack = cb;
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		SetStatus(MainStatus.Dead);
	}
}
