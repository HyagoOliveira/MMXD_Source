using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS037_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Hurt = 2,
		Dead = 3,
		Run = 4,
		Boomerang = 5,
		Dash = 6,
		Teleport = 7,
		Throw = 8,
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
		ANI_DEBUT_FALL = 1,
		ANI_DEBUT = 2,
		ANI_HURT = 3,
		ANI_RUN = 4,
		ANI_DEAD = 5,
		ANI_SKILL0_START = 6,
		ANI_SKILL0_LOOP = 7,
		ANI_SKILL0_END = 8,
		ANI_SKILL1_START = 9,
		ANI_SKILL1_LOOP = 10,
		ANI_SKILL1_END = 11,
		ANI_SKILL2_START = 12,
		ANI_SKILL2_LOOP = 13,
		ANI_SKILL2_END = 14,
		ANI_SKILL3_START = 15,
		ANI_SKILL3_END = 16,
		MAX_ANIMATION_ID = 17
	}

	private AnimationID _currentAnimationId;

	public int RunSpeed = 45000;

	private Transform _headShootTransform;

	private int[] _animatorHash;

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private float _currentFrame;

	private SkinnedMeshRenderer _modelMeshRenderer;

	private bool _haveBoomerang = true;

	private bool _isStealth;

	private bool IsChipInfoAnim;

	private CollideBullet _collideBulletSide;

	private CollideBullet _collideBulletWall;

	private ParticleSystem mfxuse_skill1;

	private ParticleSystem mFX_Booster_L;

	private ParticleSystem mFX_Booster_R;

	private int ReleaseFrame;

	private bool hasRelease;

	private readonly Vector3 _collideBulletSidePosition = new Vector3(0f, 0f, 0.5f);

	private readonly Vector3 _collideBulletSideRotation = new Vector3(0f, 90f, 0f);

	private float StageLeftPosX;

	private float StageRightPosX;

	private int nRandom0;

	private int nRandom1;

	private bool DeadCallResult = true;

	private bool CanSummon;

	[SerializeField]
	private float SummonTime = 20f;

	private int SummonFrame;

	public int ThrowForce = 120000;

	private VInt3 _targetPos = VInt3.zero;

	private Vector3 distance;

	private int useWeapon;

	private bool IsCatch;

	private OrangeCharacter targetOC;

	protected readonly Vector3 PlayerCatchVec2 = new Vector3(0.22f, -0.73f, -0.57f);

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
		_modelMeshRenderer = OrangeBattleUtility.FindChildRecursive(ref target, "BS037_Mesh_U").GetComponent<SkinnedMeshRenderer>();
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "BodyCollider");
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "BodyCollider").gameObject.AddOrGetComponent<CollideBullet>();
		_headShootTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Head");
		_collideBulletSide = OrangeBattleUtility.FindChildRecursive(ref target, "SideCollider").gameObject.AddOrGetComponent<CollideBullet>();
		_collideBulletWall = OrangeBattleUtility.FindChildRecursive(ref target, "WallCollider").gameObject.AddOrGetComponent<CollideBullet>();
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_skill", true);
		mFX_Booster_L = OrangeBattleUtility.FindChildRecursive(ref target, "FX_Booster_L", true).GetComponent<ParticleSystem>();
		mFX_Booster_R = OrangeBattleUtility.FindChildRecursive(ref target, "FX_Booster_R", true).GetComponent<ParticleSystem>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		if (transform != null)
		{
			mfxuse_skill1 = transform.GetComponent<ParticleSystem>();
		}
		_animator = GetComponentInChildren<Animator>();
		_animatorHash = new int[17];
		for (int i = 0; i < 17; i++)
		{
			_animatorHash[i] = Animator.StringToHash("idle");
		}
		_animatorHash[0] = Animator.StringToHash("BS037@idle_loop");
		_animatorHash[1] = Animator.StringToHash("BS037@debut_fall_loop");
		_animatorHash[2] = Animator.StringToHash("BS037@debut");
		_animatorHash[3] = Animator.StringToHash("BS037@hurt_loop");
		_animatorHash[5] = Animator.StringToHash("BS037@dead");
		_animatorHash[4] = Animator.StringToHash("BS037@run_forward_loop");
		_animatorHash[6] = Animator.StringToHash("BS037@skill_01_start");
		_animatorHash[7] = Animator.StringToHash("BS037@skill_01_loop");
		_animatorHash[8] = Animator.StringToHash("BS037@skill_01_end");
		_animatorHash[9] = Animator.StringToHash("BS037@skill_02_start");
		_animatorHash[10] = Animator.StringToHash("BS037@skill_02_loop");
		_animatorHash[11] = Animator.StringToHash("BS037@skill_02_end");
		_animatorHash[12] = Animator.StringToHash("BS037@skill_03_start");
		_animatorHash[13] = Animator.StringToHash("BS037@skill_03_loop");
		_animatorHash[14] = Animator.StringToHash("BS037@skill_03_end");
		_animatorHash[15] = Animator.StringToHash("BS037@skill_04_start");
		_animatorHash[16] = Animator.StringToHash("BS037@skill_04_end");
		SetStatus(MainStatus.Debut);
	}

	public override void SetChipInfoAnim()
	{
		IsChipInfoAnim = true;
		SetStatus(MainStatus.Idle);
		UpdateAnimation();
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			DeadCallResult = false;
			CanSummon = true;
			base.DeadPlayCompleted = false;
		}
		else
		{
			DeadCallResult = true;
			base.DeadPlayCompleted = true;
			CanSummon = false;
		}
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(50f);
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			if (CanSummon)
			{
				SummonFrame = GameLogicUpdateManager.GameFrame + (int)(SummonTime * 20f);
			}
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_collideBulletSide.UpdateBulletData(EnemyWeapons[3].BulletData);
			_collideBulletSide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBulletWall.UpdateBulletData(EnemyWeapons[4].BulletData);
			_collideBulletWall.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBulletWall.HitCallback = OnPlayerCollideWallCB;
		}
		else
		{
			_collideBullet.BackToPool();
			_collideBulletSide.BackToPool();
			_collideBulletWall.BackToPool();
		}
	}

	public void UpdateFunc()
	{
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.Teleport)
		{
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_modelMeshRenderer.enabled = true;
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase3:
				_modelMeshRenderer.enabled = AiTimer.GetTicks(1) % 2 == 0;
				break;
			case SubStatus.Phase2:
				_modelMeshRenderer.enabled = false;
				break;
			}
		}
		else if (!_modelMeshRenderer.enabled)
		{
			_modelMeshRenderer.enabled = true;
		}
		if (IsCatch && (bool)targetOC && (int)targetOC.Hp > 0)
		{
			targetOC._transform.position = _headShootTransform.position + (targetOC._transform.position - targetOC.Controller.GetRealCenterPos());
			targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
		}
		else if (IsCatch && (int)targetOC.Hp <= 0)
		{
			releaseOC();
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		if (IsChipInfoAnim)
		{
			return;
		}
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
			_velocity.x = base.direction * RunSpeed;
			break;
		case MainStatus.Boomerang:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = 0;
				base.SoundSource.PlaySE("BossSE", "bs003_boomel01");
				break;
			case SubStatus.Phase1:
				_velocity.x = 0;
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				UpdateMagazine(1, true);
				break;
			}
			break;
		case MainStatus.Dash:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				_velocity.x = 0;
				_targetPos = Controller.LogicPosition;
				int value2 = Controller.LogicPosition.x - TargetPos.x;
				_targetPos.x = TargetPos.x + Math.Sign(value2) * 1500;
				break;
			}
			case SubStatus.Phase1:
				_velocity.x = base.direction * RunSpeed;
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				break;
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!Controller.Collisions.below)
				{
					IgnoreGravity = true;
				}
				if (IsCatch)
				{
					releaseOC();
				}
				if ((bool)targetOC)
				{
					targetOC.SetStun(false);
					targetOC.Controller.HitWallCallbackAbove = null;
					targetOC = null;
				}
				_collideBullet.BackToPool();
				base.AllowAutoAim = false;
				_velocity.x = 0;
				OrangeBattleUtility.LockPlayer();
				break;
			case SubStatus.Phase1:
				if (DeadCallResult)
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
		case MainStatus.Teleport:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = 0;
				SetStealth(true);
				break;
			case SubStatus.Phase3:
			{
				_velocity.x = 0;
				_targetPos = Controller.LogicPosition;
				if (nRandom0 > 50)
				{
					int value = Controller.LogicPosition.x - TargetPos.x;
					_targetPos.x = TargetPos.x + Math.Sign(value) * 1500;
				}
				else
				{
					_targetPos.x = (int)((StageLeftPosX + (StageRightPosX - StageLeftPosX) * (float)nRandom1 / 100f) * 1000f);
				}
				if (_targetPos.vec3.x > StageRightPosX)
				{
					_targetPos.x -= 2000;
				}
				if (_targetPos.vec3.x < StageLeftPosX)
				{
					_targetPos.x += 2000;
				}
				RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(_targetPos.vec3, Vector2.right, 2f, Controller.collisionMask, _transform);
				RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(_targetPos.vec3, Vector2.left, 2f, Controller.collisionMask, _transform);
				if ((bool)raycastHit2D)
				{
					_targetPos.x -= 2000;
				}
				if ((bool)raycastHit2D2)
				{
					_targetPos.x += 2000;
				}
				Controller.SetLogicPosition(_targetPos);
				break;
			}
			}
			break;
		case MainStatus.Throw:
		{
			SubStatus subStatus2 = _subStatus;
			break;
		}
		}
		switch (_mainStatus)
		{
		case MainStatus.Dash:
			_collideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
			break;
		default:
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			break;
		case MainStatus.Debut:
			break;
		}
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_collideBulletSide.Active(targetMask);
			_collideBulletSide.transform.localPosition = _collideBulletSidePosition;
			_collideBulletSide.transform.localEulerAngles = _collideBulletSideRotation;
			_collideBulletSide.HitCallback = TriggerThrowCB;
			break;
		case MainStatus.Throw:
			if (_subStatus == SubStatus.Phase0)
			{
				_collideBulletSide.HitCallback = null;
			}
			else if (_collideBulletSide.IsActivate)
			{
				_collideBulletSide.BackToPool();
				_collideBulletSide.HitCallback = null;
			}
			break;
		default:
			if (_collideBulletSide.IsActivate)
			{
				_collideBulletSide.BackToPool();
				_collideBulletSide.HitCallback = null;
			}
			break;
		}
		UpdateAnimation();
		AiTimer.TimerStart();
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
					Vector3 vector = new Vector3(base.transform.position.x, base.transform.position.y + 2f, 0f);
					RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.left, 20f, Controller.collisionMask);
					RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector, Vector2.right, 20f, Controller.collisionMask);
					if (raycastHit2D.collider != null)
					{
						StageLeftPosX = raycastHit2D.point.x + 2f;
					}
					else
					{
						StageLeftPosX = base.transform.position.x - 20f;
					}
					if (raycastHit2D2.collider != null)
					{
						StageRightPosX = raycastHit2D2.point.x - 2f;
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
		case MainStatus.Boomerang:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
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
				else if (_currentFrame > 0.1f && EnemyWeapons[useWeapon].MagazineRemain > 0f)
				{
					useWeapon = 1;
					Transform headShootTransform = _headShootTransform;
					BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, headShootTransform, (TargetPos.vec3 - headShootTransform.position).normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).BackCallback = OnBoomerangBackCB;
					EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
					EnemyWeapons[useWeapon].MagazineRemain -= 1f;
					_haveBoomerang = false;
				}
				break;
			}
			break;
		case MainStatus.Dash:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					if (!mFX_Booster_L.isPlaying)
					{
						mFX_Booster_L.Play();
						mFX_Booster_R.Play();
					}
					else
					{
						mFX_Booster_L.Stop();
						mFX_Booster_R.Stop();
						mFX_Booster_L.Play();
						mFX_Booster_R.Play();
					}
					if (mfxuse_skill1 != null)
					{
						mfxuse_skill1.Play();
						PlayBossSE("BossSE", 157);
					}
				}
				break;
			case SubStatus.Phase1:
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					_targetPos.x = Target.Controller.LogicPosition.x;
					if (base.direction != Math.Sign(_targetPos.x - Controller.LogicPosition.x) || Controller.Collisions.right || Controller.Collisions.left)
					{
						SetStatus(_mainStatus, SubStatus.Phase2);
						if (mfxuse_skill1 != null)
						{
							mfxuse_skill1.Stop();
						}
						PlayBossSE("BossSE", 158);
					}
				}
				else
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					if (mfxuse_skill1 != null)
					{
						mfxuse_skill1.Stop();
					}
					PlayBossSE("BossSE", 158);
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
		case MainStatus.Teleport:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					PlayBossSE("BossSE", 19);
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (AiTimer.GetMillisecond() > 333)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (AiTimer.GetMillisecond() > 267)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
					PlayBossSE("BossSE", 20);
				}
				break;
			case SubStatus.Phase3:
				if ((double)_currentFrame > 1.0)
				{
					SetStealth(false);
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (IsCatch)
			{
				releaseOC();
			}
			UpdateRandomState();
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 0.4)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			}
			break;
		case MainStatus.Throw:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				else if (_currentFrame > 0.45f && _collideBulletSide.HitCallback == null)
				{
					_collideBulletSide.ForceClearList();
					_collideBulletSide.HitCallback = HitCB;
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 1.0)
				{
					if (IsCatch)
					{
						UpdateRandomState(2);
					}
					else
					{
						UpdateRandomState(1);
					}
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				else if (_currentFrame > 0.25f && IsCatch)
				{
					releaseOC();
					PlayBossSE("BossSE", 22);
				}
				if (hasRelease && GameLogicUpdateManager.GameFrame > ReleaseFrame)
				{
					hasRelease = false;
					if ((bool)targetOC)
					{
						targetOC.Controller.HitWallCallbackAbove = null;
						OnPlayerCollideWall();
					}
				}
				break;
			}
			break;
		default:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.Hurt:
		case MainStatus.Run:
			break;
		}
		if (CanSummon && SummonFrame < GameLogicUpdateManager.GameFrame)
		{
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
			SummonFrame = GameLogicUpdateManager.GameFrame + (int)(SummonTime * 20f);
		}
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT_FALL;
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
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Dead:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = ((!Controller.Collisions.below) ? AnimationID.ANI_HURT : AnimationID.ANI_DEAD);
				break;
			}
			return;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		case MainStatus.Run:
			_currentAnimationId = AnimationID.ANI_RUN;
			break;
		case MainStatus.Boomerang:
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
		case MainStatus.Dash:
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
			}
			break;
		case MainStatus.Throw:
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
		case MainStatus.Teleport:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL3_START;
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				return;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL3_END;
				break;
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
		CanSummon = false;
		mFX_Booster_L.Stop();
		mFX_Booster_R.Stop();
		mfxuse_skill1.Stop();
		StageUpdate.SlowStage();
		PlaySE(ExplodeSE[0], ExplodeSE[1]);
		SetStatus(MainStatus.Dead);
	}

	protected virtual void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (TargetPos.vec3.x > _transform.position.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	protected void UpdateRandomState(int nMode = 0)
	{
		MainStatus nSetKey = MainStatus.Idle;
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			if (nMode == 2 && targetOC != null)
			{
				nSetKey = MainStatus.Throw;
				TargetPos = new VInt3(targetOC.GetTargetPoint());
			}
			else if (nMode == 1)
			{
				nSetKey = MainStatus.Throw;
				if (Target != null)
				{
					TargetPos = new VInt3(Target.GetTargetPoint());
				}
			}
			else if (nMode == 3 && targetOC != null)
			{
				nSetKey = MainStatus.Throw;
				TargetPos = new VInt3(targetOC.GetTargetPoint());
			}
			else if (_haveBoomerang)
			{
				nSetKey = (MainStatus)OrangeBattleUtility.Random(5, 8);
				if (Target == null)
				{
					return;
				}
				TargetPos = new VInt3(Target.GetTargetPoint());
				UpdateDirection();
			}
			else
			{
				nSetKey = (MainStatus)OrangeBattleUtility.Random(6, 8);
				if (Target == null)
				{
					return;
				}
				TargetPos = new VInt3(Target.GetTargetPoint());
				UpdateDirection();
			}
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
		}
		if (StageUpdate.bIsHost)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.TargetPosX = TargetPos.x;
			netSyncData.TargetPosY = TargetPos.y;
			netSyncData.TargetPosZ = TargetPos.z;
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			netSyncData.nParam0 = nMode;
			netSyncData.sParam0 = OrangeBattleUtility.Random(0, 100) + "," + OrangeBattleUtility.Random(0, 100);
			if (nMode == 2)
			{
				netSyncData.sParam0 = netSyncData.sParam0 + "," + targetOC.sNetSerialID;
			}
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, (int)nSetKey, JsonConvert.SerializeObject(netSyncData));
		}
	}

	private void SetStealth(bool activate)
	{
		if (_isStealth != activate)
		{
			_isStealth = activate;
			base.AllowAutoAim = !activate;
			SetColliderEnable(!activate);
			if (activate)
			{
				_collideBullet.BackToPool();
			}
			else
			{
				_collideBullet.Active(targetMask);
			}
		}
	}

	private void TriggerThrowCB(object obj)
	{
		if (!IsCatch)
		{
			Collider2D collider2D = obj as Collider2D;
			if (collider2D != null)
			{
				targetOC = OrangeBattleUtility.GetHitTargetOrangeCharacter(collider2D);
			}
			UpdateRandomState(3);
		}
	}

	private void HitCB(object obj)
	{
		if (!StageUpdate.bIsHost || IsCatch)
		{
			return;
		}
		targetOC = OrangeBattleUtility.GetHitTargetOrangeCharacter(_collideBulletSide.HitTarget);
		if (!(targetOC != null) || (int)targetOC.Hp <= 0 || (int)Hp <= 0)
		{
			return;
		}
		if ((bool)targetOC.IsUnBreakX())
		{
			targetOC = null;
			return;
		}
		IsCatch = true;
		targetOC.SetStun(true);
		if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right * base.direction, 2f, Controller.collisionMask, _transform))
		{
			UpdateDirection(-base.direction);
		}
		PlayBossSE("BossSE", 21);
	}

	private void releaseOC()
	{
		if (IsCatch && (bool)targetOC)
		{
			if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right * base.direction, 2f, Controller.collisionMask, _transform))
			{
				UpdateDirection(-base.direction);
			}
			targetOC.AddForce(new VInt3(15000 * base.direction, ThrowForce, 0));
			targetOC.Controller.HitWallCallbackAbove = OnPlayerCollideWall;
			hasRelease = true;
			ReleaseFrame = GameLogicUpdateManager.GameFrame + 5;
		}
		IsCatch = false;
	}

	private void OnPlayerCollideWall()
	{
		if ((bool)targetOC)
		{
			targetOC.SetStun(false);
			_collideBulletWall.transform.position = targetOC.transform.position;
			_collideBulletWall.Active(targetMask);
			targetOC = null;
		}
	}

	private void OnPlayerCollideWallCB(object obj)
	{
		_collideBulletWall.BackToPool();
	}

	private void OnBoomerangBackCB(object obj)
	{
		_haveBoomerang = true;
	}

	public override void UpdateStatus(int nSet, string sMsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		int subStatus = 0;
		if (sMsg != null && sMsg != "")
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(sMsg);
			Controller.LogicPosition.x = netSyncData.SelfPosX;
			Controller.LogicPosition.y = netSyncData.SelfPosY;
			Controller.LogicPosition.z = netSyncData.SelfPosZ;
			TargetPos.x = netSyncData.TargetPosX;
			TargetPos.y = netSyncData.TargetPosY;
			TargetPos.z = netSyncData.TargetPosZ;
			string[] array = netSyncData.sParam0.Split(',');
			nRandom0 = int.Parse(array[0]);
			nRandom1 = int.Parse(array[1]);
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
			if (netSyncData.nParam0 == 1)
			{
				subStatus = 2;
			}
			else if (netSyncData.nParam0 == 2)
			{
				subStatus = 2;
				targetOC = StageUpdate.GetPlayerByID(array[2]);
				if (targetOC != null && !targetOC.IsJacking)
				{
					IsCatch = true;
					if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right * base.direction, 2f, Controller.collisionMask, _transform))
					{
						UpdateDirection(-base.direction);
					}
				}
			}
			else
			{
				UpdateDirection();
				if (IsCatch)
				{
					releaseOC();
				}
			}
		}
		SetStatus((MainStatus)nSet, (SubStatus)subStatus);
	}
}
