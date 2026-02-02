using System;
using System.Collections;
using StageLib;
using UnityEngine;

public class BS034_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Walk = 2,
		ShootClaw = 3,
		Jump = 4,
		PushAttack = 5,
		IdleWaitNet = 6,
		PullClaw = 7,
		Dead = 8
	}

	protected enum SubStatus
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
		ANI_WALK = 2,
		ANI_HURT = 3,
		ANI_JUMP_START = 4,
		ANI_JUMP_LOOP = 5,
		ANI_LAND = 6,
		ANI_SKILL0_START = 7,
		ANI_SKILL0_LOOP = 8,
		ANI_SKILL0_END = 9,
		ANI_SKILL1_START = 10,
		ANI_SKILL1_LOOP = 11,
		ANI_SKILL1_END = 12,
		ANI_SKILL2_START = 13,
		ANI_SKILL2_LOOP = 14,
		ANI_SKILL2_END = 15,
		ANI_DIE = 16,
		MAX_ANIMATION_ID = 17
	}

	protected bool IsBigBoss = true;

	protected readonly Vector3 PlayerCatchVec2 = new Vector3(0.45f, -0.73f, -0.57f);

	[Header("Serialize OBJ")]
	[SerializeField]
	public Transform LeftHandBone;

	[SerializeField]
	public Transform LeftHandClaw;

	[SerializeField]
	public Transform LeftCatchPoint;

	[SerializeField]
	public Transform RightCatchPoint;

	[SerializeField]
	public Transform Model;

	[SerializeField]
	public GameObject[] RenderModes;

	[Header("Catch Param")]
	[SerializeField]
	public float CanCatchTime = 0.07f;

	[SerializeField]
	public float CatchEndFixBlend = 0.25f;

	protected MainStatus _mainStatus;

	protected SubStatus _subStatus;

	protected AnimationID _currentAnimationId;

	protected float _currentFrame;

	protected int[] _animationHash;

	protected CollideBullet _rightPunchCollideBullet;

	protected CollideBullet _leftPunchCollideBullet;

	protected readonly int _hashVspd = Animator.StringToHash("fVspd");

	public int WalkSpeed = 1500;

	public int MoveSpeed = 8000;

	public int DashSpeed = 15000;

	public int JumpSpeed = 22000;

	public int jumpDeadZone = 5000;

	protected OrangeCharacter targetOC;

	protected bool IsCatch;

	public bool IsBlendMotion;

	private bool PlayWeakEffect;

	private float StageLeftPosX;

	private float StageRightPosX;

	protected bool _bDeadCallResult = true;

	protected Quaternion _originRotation;

	protected int jumpDistance;

	protected void OnEnable()
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
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = Model;
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "CollideBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		OrangeBattleUtility.FindChildRecursive(ref target, "RightHandCollider", true).gameObject.AddOrGetComponent<StageObjParam>().nSubPartID = 1;
		OrangeBattleUtility.FindChildRecursive(ref target, "LeftHandCollider", true).gameObject.AddOrGetComponent<StageObjParam>().nSubPartID = 2;
		GuardTransform.Add(1);
		GuardTransform.Add(2);
		_leftPunchCollideBullet = LeftCatchPoint.gameObject.AddOrGetComponent<CollideBullet>();
		_rightPunchCollideBullet = RightCatchPoint.gameObject.AddOrGetComponent<CollideBullet>();
		_animationHash = new int[17];
		_animationHash[0] = Animator.StringToHash("BS034@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS034@debut");
		_animationHash[2] = Animator.StringToHash("BS034@run_loop");
		_animationHash[3] = Animator.StringToHash("BS034@hurt_loop");
		_animationHash[4] = Animator.StringToHash("BS034@skill_03_jump_start");
		_animationHash[5] = Animator.StringToHash("BS034@skill_03_jump_loop");
		_animationHash[6] = Animator.StringToHash("BS034@skill_03_landing");
		_animationHash[7] = Animator.StringToHash("BS034@skill_01_start");
		_animationHash[8] = Animator.StringToHash("BS034@skill_01_loop");
		_animationHash[9] = Animator.StringToHash("BS034@skill_01_end");
		_animationHash[10] = Animator.StringToHash("BS034@skill_02_start");
		_animationHash[11] = Animator.StringToHash("BS034@skill_02_loop");
		_animationHash[12] = Animator.StringToHash("BS034@skill_02_end");
		_animationHash[13] = Animator.StringToHash("BS034@skill_04_start");
		_animationHash[14] = Animator.StringToHash("BS034@skill_04_loop");
		_animationHash[15] = Animator.StringToHash("BS034@skill_04_end");
		_mainStatus = MainStatus.Debut;
		_subStatus = SubStatus.Phase0;
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "body_dn_bone", true);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(18f);
		_bDeadPlayCompleted = false;
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (StageUpdate.gbIsNetGame)
		{
			if (Target.Controller.LogicPosition.x > Controller.LogicPosition.x)
			{
				base.direction = 1;
			}
			else
			{
				base.direction = -1;
			}
		}
		else if (Target != null && Target.transform.position.x > _transform.position.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)(-base.direction));
	}

	protected void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			break;
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase3:
				if (IntroCallBack != null)
				{
					IntroCallBack();
				}
				break;
			}
			break;
		case MainStatus.Walk:
			_velocity.x = base.direction * WalkSpeed;
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = 0;
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * jumpDistance;
				_velocity.y = JumpSpeed;
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.ShootClaw:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("bs101_rt55j02", 0.4f);
				_leftPunchCollideBullet.Active(targetMask);
				_velocity.x = 0;
				break;
			case SubStatus.Phase1:
				_velocity.x = 0;
				break;
			case SubStatus.Phase2:
				PlayBossSE("bs101_rt55j03", 0.2f);
				_velocity.x = 0;
				break;
			}
			break;
		case MainStatus.PullClaw:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (IsCatch)
				{
					PlayBossSE("bs101_rt55j04", 0.4f);
				}
				if (targetOC != null)
				{
					StartCoroutine(targetOC.CatchCheckWall(base.direction));
				}
				_velocity.x = 0;
				break;
			case SubStatus.Phase1:
				_leftPunchCollideBullet.Active(targetMask);
				_velocity.x = 0;
				break;
			case SubStatus.Phase2:
				PlayBossSE("bs101_rt55j03", 0.2f);
				_velocity.x = 0;
				break;
			}
			break;
		case MainStatus.PushAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = 0;
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * WalkSpeed;
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Dead:
		{
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_004)
			{
				base.DeadPlayCompleted = true;
			}
			if (!Controller.Collisions.below)
			{
				IgnoreGravity = true;
			}
			_velocity.x = 0;
			if (IsCatch && targetOC != null && (int)targetOC.Hp > 1)
			{
				releaseOC();
			}
			if (IsBigBoss)
			{
				OrangeBattleUtility.LockPlayer();
			}
			PlayBossSE("HitSE", 103);
			PlayBossSE("HitSE", 104);
			StartCoroutine(MBossExplosionSE());
			if (base.AimTransform != null)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_BOSS_EXPLODE2", base.AimTransform, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_BOSS_EXPLODE2", new Vector3(base.transform.position.x, base.transform.position.y + 1f, base.transform.position.z), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			}
			if (_bDeadCallResult)
			{
				BattleInfoUI.Instance.ShowExplodeBG(base.gameObject);
			}
			else
			{
				BattleInfoUI.Instance.ShowExplodeBG(base.gameObject, false, false);
			}
			break;
		}
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
		case MainStatus.Debut:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			}
			break;
		case MainStatus.Walk:
			_currentAnimationId = AnimationID.ANI_WALK;
			break;
		case MainStatus.ShootClaw:
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
			}
			break;
		case MainStatus.PullClaw:
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
			}
			break;
		case MainStatus.PushAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL2_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_END;
				break;
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_JUMP_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			}
			break;
		case MainStatus.Dead:
			_currentAnimationId = ((!Controller.Collisions.below) ? AnimationID.ANI_HURT : AnimationID.ANI_IDLE);
			break;
		}
		if (!IsBlendMotion)
		{
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
		}
		else if (_currentFrame > 0.7f)
		{
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 1f - _currentFrame);
		}
		else
		{
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 1f - _currentFrame - CatchEndFixBlend);
		}
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
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				UpdateDirection();
				jumpDistance = IntMath.Abs((int)(_transform.position.x - Target._transform.position.x) * 1000);
				UpdateRandomState();
			}
			break;
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					PlayBossSE("bs101_rt55j06");
					PlayBossSE("bs101_rt55j00");
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
					Vector3 vector = new Vector3(base.transform.position.x, base.transform.position.y + 2f, 0f);
					RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.left, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
					RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector, Vector2.right, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
					if (raycastHit2D.collider != null)
					{
						StageLeftPosX = raycastHit2D.point.x;
					}
					else
					{
						StageLeftPosX = base.transform.position.x - 20f;
					}
					if (raycastHit2D2.collider != null)
					{
						StageRightPosX = raycastHit2D2.point.x;
					}
					else
					{
						StageRightPosX = base.transform.position.x + 20f;
					}
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.ShootClaw:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > CanCatchTime)
				{
					_leftPunchCollideBullet.HitCallback = HitCB;
				}
				if (IsCatch)
				{
					if (targetOC != null && (int)targetOC.Hp > 1)
					{
						IsBlendMotion = true;
						SetStatus(_mainStatus, SubStatus.Phase2);
						_leftPunchCollideBullet.HitCallback = null;
					}
					else
					{
						releaseOC();
					}
				}
				else if (_currentFrame > 1f)
				{
					IsBlendMotion = false;
					SetStatus(_mainStatus, SubStatus.Phase1);
					_leftPunchCollideBullet.HitCallback = null;
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f || IsCatch)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					IsBlendMotion = false;
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.PullClaw:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (targetOC != null && (bool)OrangeBattleUtility.RaycastIgnoreSelf(LeftCatchPoint.position, Vector2.right * base.direction, 2f, Controller.collisionMask, _transform) && IsCatch)
				{
					releaseOC();
				}
				if (_currentFrame > 0.6f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (IsCatch)
				{
					releaseOC();
				}
				if (_currentFrame > 0.3f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.PushAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else if (_currentFrame > 0.35f)
				{
					_velocity.x = base.direction * WalkSpeed;
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 5f)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Walk:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				UpdateDirection();
				_velocity.x = base.direction * WalkSpeed;
			}
			else
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		}
		if ((int)Hp < (int)MaxHp / 2 && !PlayWeakEffect)
		{
			PlayWeakEffect = true;
			StartCoroutine(playWeakVFX());
		}
		if ((bool)targetOC && (int)targetOC.Hp <= 0)
		{
			releaseOC();
		}
	}

	public void UpdateFunc()
	{
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		_animator.SetFloat(_hashVspd, (float)_velocity.y * 0.001f);
		if (!IsCatch || !targetOC || (int)targetOC.Hp <= 0)
		{
			return;
		}
		MainStatus mainStatus = _mainStatus;
		if (mainStatus != 0)
		{
			if (mainStatus != MainStatus.ShootClaw)
			{
				if (mainStatus != MainStatus.PullClaw)
				{
					return;
				}
			}
			else if (_subStatus == SubStatus.Phase2 && !(_currentFrame < 0.5f))
			{
				if (_subStatus == SubStatus.Phase2 && _currentFrame > 0.5f && _currentFrame < 0.75f)
				{
					targetOC._transform.position = LeftCatchPoint.position + new Vector3((float)base.direction * 0.3f, 0.2f, 0f);
					targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
					targetOC.ModelTransform.rotation = LeftCatchPoint.rotation;
					return;
				}
				goto IL_01f2;
			}
			targetOC._transform.position = LeftCatchPoint.position + new Vector3((float)base.direction * 0.9f, -0.4f, 0f);
			targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
			targetOC.ModelTransform.rotation = LeftCatchPoint.rotation;
			return;
		}
		goto IL_01f2;
		IL_01f2:
		targetOC._transform.position = LeftCatchPoint.position + new Vector3((float)base.direction * 0.3f, -0.8f, 0f);
		targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
		targetOC.ModelTransform.rotation = LeftCatchPoint.rotation;
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_animator.enabled = isActive;
		if (isActive)
		{
			_rightPunchCollideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			_rightPunchCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_rightPunchCollideBullet.Active(targetMask);
			_leftPunchCollideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			_leftPunchCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_leftPunchCollideBullet.Active(targetMask);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
			_leftPunchCollideBullet.BackToPool();
			_rightPunchCollideBullet.BackToPool();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_002:
			_bDeadCallResult = false;
			BattleInfoUI.Instance.IsBossAppear = true;
			break;
		case AI_STATE.mob_003:
		case AI_STATE.mob_004:
		case AI_STATE.mob_005:
			_bDeadCallResult = false;
			break;
		default:
			_bDeadCallResult = true;
			break;
		}
	}

	protected virtual void UpdateRandomState()
	{
		MainStatus mainStatus;
		if (IsCatch)
		{
			mainStatus = MainStatus.PullClaw;
		}
		else if (AiState == AI_STATE.mob_003)
		{
			mainStatus = MainStatus.Jump;
			if (OrangeBattleUtility.Random(0, 100) < 40)
			{
				mainStatus = MainStatus.PushAttack;
			}
		}
		else
		{
			int num = 3;
			if (!CheckCanShootClaw())
			{
				num++;
			}
			mainStatus = (MainStatus)((!(Target != null) || !(Math.Abs(Target._transform.position.x - _transform.position.x) < 2f)) ? OrangeBattleUtility.Random(num, 5) : OrangeBattleUtility.Random(num, 6));
		}
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus);
				SetStatus(MainStatus.IdleWaitNet);
			}
		}
		else
		{
			SetStatus(mainStatus);
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

	public override void BossIntro(Action cb)
	{
		if (_mainStatus == MainStatus.Debut)
		{
			_introReady = true;
			IntroCallBack = cb;
		}
	}

	private void HitCB(object obj)
	{
		if (IsCatch || (bool)targetOC)
		{
			return;
		}
		targetOC = OrangeBattleUtility.GetHitTargetOrangeCharacter(_leftPunchCollideBullet.HitTarget);
		if (targetOC != null && (int)targetOC.Hp > 0 && (int)Hp > 0)
		{
			if ((bool)targetOC.IsUnBreakX() || targetOC.IsStun)
			{
				targetOC = null;
				return;
			}
			targetOC.SetStun(true);
			_originRotation = targetOC.ModelTransform.rotation;
			IsCatch = true;
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(RenderModes, ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer);
			PlayBossSE("bs101_rt55j08");
		}
	}

	protected virtual void releaseOC()
	{
		if (IsCatch && (bool)targetOC)
		{
			targetOC.transform.position = new Vector3(targetOC.transform.position.x, targetOC.transform.position.y, 0f);
			targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
			targetOC.ModelTransform.rotation = _originRotation;
			if ((int)targetOC.Hp > 0)
			{
				targetOC.SetStun(false);
			}
			playPullWallVFX();
		}
		MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(RenderModes, ManagedSingleton<OrangeLayerManager>.Instance.RenderEnemy);
		targetOC = null;
		IsCatch = false;
	}

	protected bool CheckCanShootClaw()
	{
		float x = Controller.LogicPosition.vec3.x;
		if ((Math.Abs(x - StageLeftPosX) < 1f && base.direction == -1) || (Math.Abs(x - StageRightPosX) < 1f && base.direction == 1))
		{
			return false;
		}
		if (Mathf.Abs(x - StageRightPosX) < 2f || Mathf.Abs(x - StageLeftPosX) < 2f)
		{
			return false;
		}
		return true;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		if ((bool)_leftPunchCollideBullet)
		{
			_leftPunchCollideBullet.BackToPool();
		}
		if ((bool)_rightPunchCollideBullet)
		{
			_rightPunchCollideBullet.BackToPool();
		}
		SetStatus(MainStatus.Dead);
	}

	protected void playPullWallVFX()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_common_000", targetOC.transform.position, targetOC.transform.rotation, Array.Empty<object>());
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_common_000", LeftHandClaw.position, LeftHandClaw.rotation, Array.Empty<object>());
	}

	private IEnumerator playWeakVFX()
	{
		PlayWeakEffect = true;
		int mode = 1;
		while ((int)Hp > 0)
		{
			if (mode == 1)
			{
				Vector3 p_worldPos = new Vector3(LeftCatchPoint.position.x, LeftCatchPoint.position.y + 1.5f, LeftCatchPoint.position.z);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_smoke_000", p_worldPos, LeftCatchPoint.rotation, Array.Empty<object>());
				mode = 2;
			}
			else
			{
				Vector3 p_worldPos2 = new Vector3(RightCatchPoint.position.x, RightCatchPoint.position.y + 1.5f, RightCatchPoint.position.z);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_smoke_000", p_worldPos2, RightCatchPoint.rotation, Array.Empty<object>());
				mode = 1;
			}
			yield return new WaitForSeconds(1.2f);
		}
		yield return true;
	}
}
