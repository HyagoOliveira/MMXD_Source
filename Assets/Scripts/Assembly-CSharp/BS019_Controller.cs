using System;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using StageLib;
using UnityEngine;

public class BS019_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Run = 2,
		Jump = 3,
		Spin = 4,
		Punch = 5,
		Laser = 6,
		Dead = 7,
		Hurt = 8,
		IdleWaitNet = 9
	}

	protected enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		MAX_SUBSTATUS = 4
	}

	public enum DieStatus
	{
		Left = 0,
		Right = 1,
		Mid = 2,
		End = 3,
		MAX_SUBSTATUS = 4
	}

	public enum AnimationID
	{
		ANI_DEBUT = 0,
		ANI_DEBUT_END = 1,
		ANI_IDLE = 2,
		ANI_HURT = 3,
		ANI_DEAD = 4,
		ANI_JUMP_START = 5,
		ANI_JUMP_LOOP = 6,
		ANI_JUMP_END = 7,
		ANI_RUN_START = 8,
		ANI_RUN_LOOP = 9,
		ANI_RUN_END = 10,
		ANI_SPIN_START = 11,
		ANI_SPIN_LOOP = 12,
		ANI_SPIN_END = 13,
		ANI_PUNCH_START = 14,
		ANI_PUNCH_LOOP = 15,
		ANI_PUNCH_END = 16,
		ANI_LASER_START = 17,
		ANI_LASER_LOOP = 18,
		ANI_LASER_END = 19,
		MAX_ANIMATION_ID = 20
	}

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	protected MainStatus _mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	protected SubStatus _subStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	protected AnimationID _currentAnimationId;

	public int MoveSpeed = 10;

	public int JumpSpeed = 80;

	public int RunSpeed = 5;

	protected readonly int _hashVspd = Animator.StringToHash("fVspd");

	protected readonly int _hashWalkSpeed = Animator.StringToHash("fWalkSpeed");

	protected readonly int _hashMirror = Animator.StringToHash("bMirror");

	protected bool _debutEnd;

	protected CollideBullet _LHandCollideBullet;

	protected CollideBullet _RHandCollideBullet;

	protected CollideBullet _LFootCollideBullet;

	protected CollideBullet _RFootCollideBullet;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	protected float _currentFrame;

	protected int[] _animationHash;

	protected int AILevel = 1;

	protected Transform eye_Light;

	protected Transform Left_eye_Light;

	protected Transform Right_eye_Light;

	protected Transform Left_Laser_bullet;

	protected FX_Layzer_01 Left_LinePoint_Object;

	protected Transform _LFoot_Point;

	protected Transform _RFoot_Point;

	protected int[] nearAi = new int[4] { 2, 5, 4, 3 };

	protected int[] nearRaund = new int[5] { 0, 100, 350, 650, 1000 };

	protected int[] FarAi = new int[3] { 2, 4, 3 };

	protected int[] FarRaund = new int[4] { 0, 150, 500, 1000 };

	protected DieStatus _DieStatus;

	protected string FX_BOSS_EXPLODE1 = "FX_BOSS_EXPLODE1";

	protected string FX_BOSS_EXPLODE2 = "FX_BOSS_EXPLODE2";

	protected string FX_SKILL1 = "fxuse_bs19_skill1";

	protected string FX_FIRE_EXPLOSION = "Fire_Explosion_000";

	protected string FX_FIRE_FLASH = "Fire_Flash_000";

	protected string FX_USE_TARGET = "fxuseTarget";

	protected string FX_DURING_CF0 = "fxduring_CF_0_000";

	protected float Fire_Start_X = 37f;

	protected Transform L_Explode1;

	protected Transform R_Explode1;

	protected Transform H_Explode;

	private Vector3 defaultModelRotation = new Vector3(0f, 90f, 0f);

	protected bool IsChipInfoAnim;

	private bool _bDeadCallResult = true;

	[SerializeField]
	protected bool IsBossVer;

	protected int nDeadCount;

	protected void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		PlaySE("BossSE", 39);
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Start()
	{
		base.Start();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		base.AllowAutoAim = false;
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "AimPoint");
		_animator = GetComponentInChildren<Animator>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_LHandCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "L_hand_collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		_RHandCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "R_hand_collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		_LFootCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "L_foot_collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		_RFootCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "R_foot_collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (AILevel == 1)
		{
			if (null == _enemyAutoAimSystem)
			{
				OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
				_enemyAutoAimSystem.UpdateAimRange(100f);
			}
			eye_Light = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_CF_0_000", true);
			Left_eye_Light = OrangeBattleUtility.FindChildRecursive(eye_Light, "FX_Ray_010_A", true);
			Right_eye_Light = OrangeBattleUtility.FindChildRecursive(eye_Light, "FX_Ray_010_B", true);
			_LFoot_Point = OrangeBattleUtility.FindChildRecursive(ref target, "L_foot_collider", true);
			_RFoot_Point = OrangeBattleUtility.FindChildRecursive(ref target, "R_foot_collider", true);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_FIRE_EXPLOSION, 2);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_FIRE_FLASH, 2);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_SKILL1);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_USE_TARGET, 10);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_BOSS_EXPLODE1, 2);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_BOSS_EXPLODE2);
			L_Explode1 = OrangeBattleUtility.FindChildRecursive(ref target, "L_Explode1", true);
			R_Explode1 = OrangeBattleUtility.FindChildRecursive(ref target, "R_Explode1", true);
			H_Explode = OrangeBattleUtility.FindChildRecursive(ref target, "H_Explode", true);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/" + FX_DURING_CF0, FX_DURING_CF0, delegate(GameObject obj)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(obj);
				Left_Laser_bullet = gameObject.transform;
				Left_Laser_bullet.gameObject.SetActive(false);
				Left_LinePoint_Object = Left_Laser_bullet.GetComponentInChildren<FX_Layzer_01>();
			});
		}
		_animationHash = new int[20];
		_animationHash[0] = Animator.StringToHash("Debut");
		_animationHash[1] = Animator.StringToHash("DebutEnd");
		_animationHash[2] = Animator.StringToHash("Idle");
		_animationHash[4] = Animator.StringToHash("Dead");
		_animationHash[3] = Animator.StringToHash("Hurt");
		_animationHash[8] = Animator.StringToHash("RunStart");
		_animationHash[9] = Animator.StringToHash("RunLoop");
		_animationHash[10] = Animator.StringToHash("RunEnd");
		_animationHash[5] = Animator.StringToHash("JumpStart");
		_animationHash[6] = Animator.StringToHash("JumpLoop");
		_animationHash[7] = Animator.StringToHash("JumpEnd");
		_animationHash[14] = Animator.StringToHash("PunchStart");
		_animationHash[15] = Animator.StringToHash("PunchLoop");
		_animationHash[16] = Animator.StringToHash("PunchEnd");
		_animationHash[11] = Animator.StringToHash("SpinStart");
		_animationHash[12] = Animator.StringToHash("SpinLoop");
		_animationHash[13] = Animator.StringToHash("SpinEnd");
		_animationHash[17] = Animator.StringToHash("LaserStart");
		_animationHash[18] = Animator.StringToHash("LaserLoop");
		_animationHash[19] = Animator.StringToHash("LaserEnd");
		if (IsBossVer)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		base.direction = 1;
		if (IsChipInfoAnim)
		{
			SetStatus(MainStatus.Idle);
		}
		else
		{
			SetStatus(MainStatus.Debut);
		}
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
		_bDeadPlayCompleted = false;
	}

	public override void SetChipInfoAnim()
	{
		IsChipInfoAnim = true;
	}

	protected void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Dead:
		case MainStatus.IdleWaitNet:
			base.DeadPlayCompleted = true;
			UpdateDirection(1);
			ModelTransform.localEulerAngles = defaultModelRotation;
			_velocity.x = 0;
			break;
		case MainStatus.Run:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateDirection();
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				break;
			default:
				throw new ArgumentOutOfRangeException("subStatus", subStatus, null);
			case SubStatus.Phase1:
				break;
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * MoveSpeed * 1000;
				_velocity.y = JumpSpeed * 1000;
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				break;
			default:
				throw new ArgumentOutOfRangeException("subStatus", subStatus, null);
			}
			break;
		case MainStatus.Spin:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * MoveSpeed * 1000;
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				break;
			default:
				throw new ArgumentOutOfRangeException("subStatus", subStatus, null);
			}
			break;
		case MainStatus.Punch:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateDirection();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase1:
			case SubStatus.Phase2:
			case SubStatus.MAX_SUBSTATUS:
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.Debut:
		case MainStatus.Laser:
		case MainStatus.Hurt:
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
		UpdateCollider();
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		_velocityShift.z = 0;
		BaseLogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (AILevel == 1)
			{
				if (Controller.Collisions.below)
				{
					UpdateRandomState();
				}
			}
			else if (_currentFrame > 1f && Controller.Collisions.below)
			{
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
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_debutEnd)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Run:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 3.0 || (base.direction == -1 && Controller.Collisions.left) || (base.direction == 1 && Controller.Collisions.right))
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
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_FIRE_FLASH, new Vector3(_LFoot_Point.position.x, _LFoot_Point.position.y - 1f, _LFoot_Point.position.z - 2f), Quaternion.identity, Array.Empty<object>());
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_FIRE_FLASH, new Vector3(_RFoot_Point.position.x, _RFoot_Point.position.y - 1f, _RFoot_Point.position.z - 2f), Quaternion.identity, Array.Empty<object>());
					if (AiState == AI_STATE.mob_002)
					{
						MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
					}
				}
				break;
			case SubStatus.Phase1:
			{
				if (!Controller.Collisions.below)
				{
					break;
				}
				SetStatus(_mainStatus, SubStatus.Phase2);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_FIRE_EXPLOSION, new Vector3(_LFoot_Point.position.x, _LFoot_Point.position.y - 1f, _LFoot_Point.position.z - 2f), Quaternion.identity, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_FIRE_EXPLOSION, new Vector3(_RFoot_Point.position.x, _RFoot_Point.position.y - 1f, _RFoot_Point.position.z - 2f), Quaternion.identity, Array.Empty<object>());
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1.25f, false);
				int num = OrangeBattleUtility.Random(5, 11);
				float num2 = 30 / num;
				for (int i = 0; i < num; i++)
				{
					BasicBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BasicBullet>(EnemyWeapons[3].BulletData.s_MODEL);
					if ((bool)poolObj)
					{
						poolObj.isForceSE = true;
						poolObj.needPlayEndSE = true;
						Vector3 vector = new Vector3(Fire_Start_X + (float)i * num2, OrangeBattleUtility.Random(6, 12), 0f);
						RaycastHit2D[] array = Physics2D.RaycastAll(vector, Vector3.down, 200f, LayerMask.GetMask("SemiBlock"));
						EnemyWeapons[3].BulletData.f_DISTANCE = 500f;
						poolObj.UpdateBulletData(EnemyWeapons[3].BulletData);
						poolObj.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
						if (array.Length != 0)
						{
							int num3 = OrangeBattleUtility.Random(0, array.Length);
							poolObj.FreeDISTANCE = Vector3.Distance(vector, array[num3].transform.position) - 1f;
							Vector3 vector2 = new Vector3(vector.x, vector.y, vector.z);
							Vector3 vector3 = new Vector3(array[num3].transform.position.x, array[num3].transform.position.y, array[num3].transform.position.z);
							float distance = Mathf.Abs(Vector3.Distance(vector2, vector3));
							Vector3 vector4 = (vector2.xy() - vector3.xy()).normalized;
							MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>(FX_USE_TARGET, vector2, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, -vector4)), Array.Empty<object>()).SetEffect(distance, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 2f);
						}
						poolObj.needPlayEndSE = true;
						poolObj.Active(vector, Vector3.down, targetMask);
						poolObj.UpdateFx();
					}
				}
				PlaySE("BossSE", 38);
				break;
			}
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
					PlaySE("BossSE", 39);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Spin:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_SKILL1, base.transform, Quaternion.identity, Array.Empty<object>());
					_LHandCollideBullet.Active(targetMask);
					_RHandCollideBullet.Active(targetMask);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 3.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					if (_RHandCollideBullet.IsActivate)
					{
						_RHandCollideBullet.IsDestroy = true;
					}
					if (_LHandCollideBullet.IsActivate)
					{
						_LHandCollideBullet.IsDestroy = true;
					}
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Punch:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					_LHandCollideBullet.Active(targetMask);
					_RHandCollideBullet.Active(targetMask);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					if (_RHandCollideBullet.IsActivate)
					{
						_RHandCollideBullet.IsDestroy = true;
					}
					if (_LHandCollideBullet.IsActivate)
					{
						_LHandCollideBullet.IsDestroy = true;
					}
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.01f)
				{
					if (nDeadCount > 10)
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase1:
				switch (_DieStatus)
				{
				case DieStatus.Left:
					if (_currentFrame > 0.05f)
					{
						OrangeBattleUtility.LockPlayer();
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_BOSS_EXPLODE1, L_Explode1, Quaternion.identity, Array.Empty<object>());
						_DieStatus = DieStatus.Right;
						PlaySE("HitSE", 102);
						PlaySE("BossSE", 39);
					}
					break;
				case DieStatus.Right:
					if (_currentFrame > 0.35f)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_BOSS_EXPLODE1, R_Explode1, Quaternion.identity, Array.Empty<object>());
						_DieStatus = DieStatus.Mid;
						PlaySE("HitSE", 103);
					}
					break;
				case DieStatus.Mid:
					if (_currentFrame > 0.8f)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_BOSS_EXPLODE2, H_Explode, Quaternion.identity, Array.Empty<object>());
						_DieStatus = DieStatus.End;
						PlaySE("HitSE", 104);
						StartCoroutine(MBossExplosionSE());
						if (_bDeadCallResult)
						{
							BattleInfoUI.Instance.ShowExplodeBG(base.gameObject, false);
						}
						else
						{
							BattleInfoUI.Instance.ShowExplodeBG(base.gameObject, false, false);
						}
					}
					break;
				case DieStatus.End:
					break;
				}
				break;
			}
			break;
		case MainStatus.Laser:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
					Left_Laser_bullet.gameObject.SetActive(true);
					Left_LinePoint_Object.lineStart.transform.position = Left_eye_Light.transform.position;
				}
				break;
			case SubStatus.Phase1:
				Left_LinePoint_Object.lineStart.transform.position = new Vector3(Left_eye_Light.transform.position.x, Left_eye_Light.transform.position.y, 0f);
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				Left_LinePoint_Object.lineStart.transform.position = new Vector3(Left_eye_Light.transform.position.x, Left_eye_Light.transform.position.y, 0f);
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
					eye_Light.gameObject.SetActive(false);
					Left_Laser_bullet.gameObject.SetActive(false);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.Hurt:
		case MainStatus.IdleWaitNet:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_animator.SetFloat(_hashVspd, _velocity.vec3.y);
			_animator.SetFloat(_hashWalkSpeed, (float)RunSpeed / 10f);
			_velocityShift += new VInt3(ModelTransform.localPosition);
			ModelTransform.localPosition = Vector3.zero;
		}
	}

	protected void UpdateRandomState()
	{
		MainStatus mainStatus = (MainStatus)OrangeBattleUtility.Random(2, 6);
		if (AILevel == 1)
		{
			Target = StageUpdate.GetNearestPlayerByVintPos(Controller.LogicPosition);
			if (Target == null)
			{
				mainStatus = MainStatus.Idle;
			}
			else if (base.transform.position.x > Target.transform.position.x + 12.76f || base.transform.position.x < Target.transform.position.x - 12.76f || Target.transform.position.y < -14f)
			{
				int num = OrangeBattleUtility.Random(0, 1000);
				for (int i = 0; i < FarAi.Length; i++)
				{
					if (num > FarRaund[i] && num < FarRaund[i + 1])
					{
						mainStatus = (MainStatus)FarAi[i];
					}
				}
			}
			else
			{
				int num2 = OrangeBattleUtility.Random(0, 1000);
				for (int j = 0; j < nearAi.Length; j++)
				{
					if (num2 > nearRaund[j] && num2 < nearRaund[j + 1])
					{
						mainStatus = (MainStatus)nearAi[j];
					}
				}
			}
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

	private void UpdateCollider()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Dead:
		case MainStatus.Hurt:
		case MainStatus.IdleWaitNet:
			if (_RHandCollideBullet.IsActivate)
			{
				_RHandCollideBullet.IsDestroy = true;
			}
			if (_LHandCollideBullet.IsActivate)
			{
				_LHandCollideBullet.IsDestroy = true;
			}
			break;
		case MainStatus.Debut:
		case MainStatus.Run:
		case MainStatus.Jump:
		case MainStatus.Spin:
		case MainStatus.Punch:
		case MainStatus.Laser:
			break;
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
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_JUMP_END;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEBUT_END;
				break;
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
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_RUN_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_RUN_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_RUN_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
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
				_currentAnimationId = AnimationID.ANI_JUMP_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Spin:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SPIN_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SPIN_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SPIN_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Punch:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_PUNCH_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_PUNCH_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_PUNCH_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Laser:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_LASER_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_LASER_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_LASER_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	protected virtual void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (OrangeBattleUtility.CurrentCharacter != null && OrangeBattleUtility.CurrentCharacter.transform.position.x > _transform.position.x)
		{
			base.direction = 1;
		}
		else
		{
			base.direction = -1;
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
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
		pos.z = 6f;
		base.transform.position = pos;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Debut && _subStatus < SubStatus.Phase3)
		{
			return Hp;
		}
		return base.Hurt(tHurtPassParam);
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Dead)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if (IsBossVer)
			{
				StageUpdate.SlowStage();
			}
			SetStatus(MainStatus.Dead);
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		_LHandCollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
		_RHandCollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
		Left_Laser_bullet.GetComponent<CollideBullet>().UpdateBulletData(EnemyWeapons[2].BulletData);
		_LHandCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		_RHandCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/bullet/" + EnemyWeapons[3].BulletData.s_MODEL, EnemyWeapons[3].BulletData.s_MODEL, delegate(GameObject obj)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(obj);
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BasicBullet>(gameObject.GetComponent<BasicBullet>(), EnemyWeapons[3].BulletData.s_MODEL);
		});
		AI_STATE aI_STATE = AI_STATE.mob_001;
		if (EnemyData.s_AI != "null")
		{
			aI_STATE = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
		}
		AiState = aI_STATE;
		if (aI_STATE == AI_STATE.mob_002)
		{
			_bImmunityDeadArea = true;
			_bDeadCallResult = false;
		}
		else
		{
			_bDeadCallResult = true;
		}
	}

	public override void Unlock()
	{
		base.Unlock();
		_debutEnd = true;
	}
}
