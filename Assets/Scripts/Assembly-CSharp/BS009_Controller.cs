using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using StageLib;
using UnityEngine;

public class BS009_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Run = 2,
		Jump = 3,
		Punch = 4,
		Shoot = 5,
		Plasma = 6,
		Dead = 7,
		Hurt = 8,
		IdleWaitNet = 9
	}

	private enum SubStatus
	{
		Prepare = 0,
		Normal = 1,
		End = 2,
		Wait = 3,
		MAX_SUBSTATUS = 4
	}

	public enum AnimationID
	{
		ANI_DEBUT = 0,
		ANI_IDLE = 1,
		ANI_HURT = 2,
		ANI_DEAD = 3,
		ANI_RUN_START = 4,
		ANI_RUN_LOOP = 5,
		ANI_JUMP_START = 6,
		ANI_JUMP_LOOP = 7,
		ANI_JUMP_END = 8,
		ANI_PLASMA_START = 9,
		ANI_PLASMA_LOOP = 10,
		ANI_PLASMA_END = 11,
		ANI_PUNCH_START = 12,
		ANI_PUNCH_END = 13,
		ANI_SHOOT_START = 14,
		ANI_SHOOT_END = 15,
		MAX_ANIMATION_ID = 16
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	public int MoveSpeed = 10;

	public int JumpSpeed = 80;

	public int RunSpeed = 5;

	public int AILevel = 1;

	private Vector3 lock_Pos = new Vector3(0f, 0f, 0f);

	private Vector3 source_Pos = new Vector3(0f, 0f, 0f);

	private int[] nearAi = new int[4] { 3, 4, 5, 6 };

	private int[] nearRaund = new int[5] { 0, 100, 500, 800, 1000 };

	private int[] FarAi = new int[4] { 3, 2, 5, 6 };

	private int[] FarRaund = new int[5] { 0, 100, 500, 800, 1000 };

	private Transform LeftAtk;

	private Transform RightAtk;

	private int PunchStatus;

	private bool DebutUpdateDirection;

	private Action IntoBack;

	private readonly int _hashVspd = Animator.StringToHash("fVspd");

	private readonly int _hashWalkSpeed = Animator.StringToHash("fWalkSpeed");

	private int _shootNum;

	private CollideBullet ClawCollideBullet;

	private CollideBullet Jump_Run_CollideBullet;

	private float _currentFrame;

	private int[] _animationHash;

	private Transform _shootPointTransform;

	private VInt3 targetPos;

	private Vector3 defaultModelRotation = new Vector3(0f, 90f, 0f);

	private bool IsChipInfoAnim;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Start()
	{
		base.Start();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "AimPoint");
		_animator = GetComponentInChildren<Animator>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_shootPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint", true);
		ClawCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "ColliderBullet", true).gameObject.AddOrGetComponent<CollideBullet>();
		Jump_Run_CollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "ColliderBullet_Jump_Run", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (AILevel == 1)
		{
			if (null == _enemyAutoAimSystem)
			{
				OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
				_enemyAutoAimSystem.UpdateAimRange(100f);
			}
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_bs09_run", 10);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_maoh_000", 2);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
			LeftAtk = OrangeBattleUtility.FindChildRecursive(ref target, "L_hand_bone4", true);
			RightAtk = OrangeBattleUtility.FindChildRecursive(ref target, "R_hand_bone4", true);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget", 2);
		}
		_animationHash = new int[16];
		_animationHash[0] = Animator.StringToHash("BS009@debut");
		_animationHash[1] = Animator.StringToHash("BS009@idle_loop");
		_animationHash[3] = Animator.StringToHash("BS009@dead");
		_animationHash[2] = Animator.StringToHash("BS009@hurt");
		_animationHash[4] = Animator.StringToHash("BS009@run_start");
		_animationHash[5] = Animator.StringToHash("BS009@run_loop");
		_animationHash[6] = Animator.StringToHash("BS009@skill_02_jump_start");
		_animationHash[7] = Animator.StringToHash("BS009@skill_02_jump_loop");
		_animationHash[8] = Animator.StringToHash("BS009@skill_02_jump_end");
		_animationHash[12] = Animator.StringToHash("BS009@skill_03_start");
		_animationHash[13] = Animator.StringToHash("BS009@skill_03_end");
		_animationHash[9] = Animator.StringToHash("BS009@skill_04_start");
		_animationHash[10] = Animator.StringToHash("BS009@skill_04_loop");
		_animationHash[11] = Animator.StringToHash("BS009@skill_04_end");
		_animationHash[14] = Animator.StringToHash("BS009@skill_01_start");
		_animationHash[15] = Animator.StringToHash("BS009@skill_01_end");
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
		if (AILevel == 1)
		{
			DebutUpdateDirection = false;
		}
	}

	public override void SetChipInfoAnim()
	{
		IsChipInfoAnim = true;
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Prepare)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Dead:
		case MainStatus.IdleWaitNet:
			UpdateDirection();
			ModelTransform.localEulerAngles = defaultModelRotation;
			_velocity.x = 0;
			break;
		case MainStatus.Run:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				UpdateDirection();
				break;
			case SubStatus.End:
				_velocity = VInt3.zero;
				break;
			default:
				throw new ArgumentOutOfRangeException("subStatus", subStatus, null);
			case SubStatus.Normal:
				break;
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				UpdateDirection();
				break;
			case SubStatus.Normal:
				_velocity.x = base.direction * MoveSpeed * 1000;
				_velocity.y = JumpSpeed * 1000;
				break;
			case SubStatus.End:
				_velocity = VInt3.zero;
				break;
			default:
				throw new ArgumentOutOfRangeException("subStatus", subStatus, null);
			}
			break;
		case MainStatus.Plasma:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				UpdateDirection();
				break;
			default:
				throw new ArgumentOutOfRangeException("subStatus", subStatus, null);
			case SubStatus.Normal:
			case SubStatus.End:
				break;
			}
			break;
		case MainStatus.Punch:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				UpdateDirection();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Normal:
			case SubStatus.End:
			case SubStatus.MAX_SUBSTATUS:
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.Debut:
		case MainStatus.Shoot:
		case MainStatus.Hurt:
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
		UpdateCollider();
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		string[] array = smsg.Split(',');
		int x = int.Parse(array[0]);
		int y = int.Parse(array[1]);
		targetPos.x = x;
		targetPos.y = y;
		SetStatus((MainStatus)nSet);
	}

	public override void LogicUpdate()
	{
		if (!DebutUpdateDirection)
		{
			UpdateDirection();
			DebutUpdateDirection = true;
		}
		_velocityExtra.z = 0;
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (Activate)
			{
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
			}
			else if (_currentFrame > 1f)
			{
				SetStatus(_mainStatus, SubStatus.Normal);
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				if (_currentFrame > 1f)
				{
					SetStatus(_mainStatus, SubStatus.End);
				}
				break;
			case SubStatus.End:
				if (AiTimer.GetMillisecond() > 1300)
				{
					AiTimer.TimerStop();
					if (StageUpdate.gbIsNetGame)
					{
						BattleInfoUI.Instance.ShowExplodeBG(base.gameObject);
					}
					else if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.ContainsKey(ManagedSingleton<StageHelper>.Instance.nLastStageID))
					{
						BattleInfoUI.Instance.ShowExplodeBG(base.gameObject);
					}
				}
				break;
			}
			break;
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				if (Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Normal);
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, true);
				}
				break;
			case SubStatus.Normal:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Wait);
				}
				break;
			case SubStatus.Wait:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Wait);
				}
				break;
			case SubStatus.End:
				if ((double)_currentFrame > 0.9)
				{
					SetStatus(MainStatus.Idle);
					if (IntoBack != null)
					{
						IntoBack();
					}
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Run:
			if (AILevel == 1)
			{
				switch (_subStatus)
				{
				case SubStatus.Prepare:
					if (_currentFrame > 1f)
					{
						SetStatus(_mainStatus, SubStatus.Normal);
						Jump_Run_CollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
						Jump_Run_CollideBullet.Active(targetMask);
					}
					break;
				case SubStatus.Normal:
				{
					if (base.transform.position.x > lock_Pos.x)
					{
						Vector2 vector = new Vector2(base.transform.position.x, base.transform.position.y);
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bs09_run", vector, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
						_velocity.x = -20000;
						if (base.transform.position.x <= lock_Pos.x + 3f)
						{
							SetStatus(MainStatus.Punch);
							_velocity.x = 0;
							PunchStatus = 0;
							if (Jump_Run_CollideBullet.IsActivate)
							{
								Jump_Run_CollideBullet.IsDestroy = true;
							}
						}
						break;
					}
					Vector2 vector2 = new Vector2(base.transform.position.x, base.transform.position.y);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bs09_run", vector2, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
					_velocity.x = 20000;
					if (base.transform.position.x >= lock_Pos.x - 3f)
					{
						SetStatus(MainStatus.Punch);
						_velocity.x = 0;
						PunchStatus = 0;
						if (Jump_Run_CollideBullet.IsActivate)
						{
							Jump_Run_CollideBullet.IsDestroy = true;
						}
					}
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				SetStatus(MainStatus.Punch);
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Normal);
				}
				break;
			case SubStatus.Normal:
				if (Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.End);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bs09_jup_down", base.transform.position, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
					Jump_Run_CollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
					Jump_Run_CollideBullet.Active(targetMask);
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
				}
				break;
			case SubStatus.End:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				else if (Jump_Run_CollideBullet.IsActivate)
				{
					Jump_Run_CollideBullet.IsDestroy = true;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Plasma:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Normal);
					_shootNum = 0;
				}
				break;
			case SubStatus.Normal:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.End);
				}
				break;
			case SubStatus.End:
				if ((double)_currentFrame > 0.192 && _shootNum == 0)
				{
					ShootBullet(3);
				}
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
			case SubStatus.Prepare:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Normal);
				}
				break;
			case SubStatus.Normal:
				if (AILevel == 1)
				{
					switch (PunchStatus)
					{
					case 0:
						if ((double)_currentFrame > 0.13)
						{
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_maoh_000", new Vector3(RightAtk.position.x, base.transform.position.y, RightAtk.position.z), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
							PunchStatus = 1;
							ClawCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
							ClawCollideBullet.Active(targetMask);
						}
						break;
					case 1:
						if ((double)_currentFrame > 0.27)
						{
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_maoh_000", new Vector3(LeftAtk.position.x, base.transform.position.y, LeftAtk.position.z), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
							PunchStatus = 2;
							ClawCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
							ClawCollideBullet.Active(targetMask);
						}
						else if (ClawCollideBullet.IsActivate)
						{
							ClawCollideBullet.IsDestroy = true;
						}
						break;
					case 2:
						if (_currentFrame > 1f)
						{
							SetStatus(MainStatus.Idle);
							PunchStatus = 0;
						}
						else if (ClawCollideBullet.IsActivate)
						{
							ClawCollideBullet.IsDestroy = true;
						}
						break;
					}
				}
				else if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Shoot:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				if ((double)_currentFrame > 1.0)
				{
					_shootNum = 0;
					SetStatus(_mainStatus, SubStatus.Normal);
				}
				break;
			case SubStatus.Normal:
				if (_currentFrame > 0f && _shootNum == 0)
				{
					UpdateDirection();
					ShootBullet(2);
				}
				if ((double)_currentFrame > 0.25 && _shootNum == 1)
				{
					UpdateDirection();
					ShootBullet(2);
				}
				if ((double)_currentFrame > 0.53 && _shootNum == 2)
				{
					UpdateDirection();
					ShootBullet(2);
				}
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
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
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_animator.SetFloat(_hashVspd, _velocity.vec3.y);
			_velocityExtra += new VInt3(ModelTransform.localPosition);
			ModelTransform.localPosition = Vector3.zero;
		}
	}

	private void NetGoToIdel()
	{
		StageUpdate.RegisterSendAndRun(sNetSerialID, 0);
		SetStatus(MainStatus.IdleWaitNet);
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = (MainStatus)OrangeBattleUtility.Random(2, 7);
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				int num = int.MaxValue;
				VInt3 zero = VInt3.zero;
				if (OrangeBattleUtility.ListPlayer != null)
				{
					int num2 = -1;
					for (int i = 0; i < OrangeBattleUtility.ListPlayer.Count; i++)
					{
						int num3 = IntMath.Abs(Controller.LogicPosition.x - OrangeBattleUtility.ListPlayer[i].Controller.LogicPosition.x);
						if (num3 < num)
						{
							num2 = i;
							num = num3;
						}
					}
					if (num2 != -1)
					{
						OrangeCharacter orangeCharacter = OrangeBattleUtility.ListPlayer[num2];
						targetPos = orangeCharacter.Controller.LogicPosition;
						base.direction = (((Controller.LogicPosition - orangeCharacter.Controller.LogicPosition).x <= 0) ? 1 : (-1));
						int num4 = OrangeBattleUtility.Random(0, 1000);
						for (int j = 0; j < FarAi.Length; j++)
						{
							if (num4 > FarRaund[j] && num4 < FarRaund[j + 1])
							{
								mainStatus = (MainStatus)FarAi[j];
							}
						}
						if (mainStatus == MainStatus.Run)
						{
							source_Pos = new Vector3(base.transform.position.x, base.transform.position.y + 2f, base.transform.position.z);
							lock_Pos = new Vector3(targetPos.vec3.x, source_Pos.y, source_Pos.z);
							float distance = Vector2.Distance(source_Pos, lock_Pos);
							Vector3 vector = (source_Pos.xy() - lock_Pos.xy()).normalized;
							MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", source_Pos, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, -vector)), Array.Empty<object>()).SetEffect(distance, new Color(1f, 1f, 0f, 0.6f), new Color(1f, 0.54f, 0f), 1f, 3f);
						}
						PunchStatus = 0;
						StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus, targetPos.x + "," + targetPos.y);
						SetStatus(MainStatus.IdleWaitNet);
					}
					else
					{
						NetGoToIdel();
					}
				}
				else
				{
					NetGoToIdel();
				}
			}
			else
			{
				SetStatus(MainStatus.IdleWaitNet);
			}
			return;
		}
		if (AILevel == 1)
		{
			if (Target == null)
			{
				mainStatus = MainStatus.Idle;
			}
			else if (base.transform.position.x > Target.transform.position.x + 5f || base.transform.position.x < Target.transform.position.x - 5f)
			{
				int num5 = OrangeBattleUtility.Random(0, 1000);
				for (int k = 0; k < FarAi.Length; k++)
				{
					if (num5 > FarRaund[k] && num5 < FarRaund[k + 1])
					{
						mainStatus = (MainStatus)FarAi[k];
					}
				}
				if (mainStatus == MainStatus.Run && (bool)Target)
				{
					source_Pos = new Vector3(base.transform.position.x, base.transform.position.y + 2f, base.transform.position.z);
					lock_Pos = new Vector3(Target.transform.position.x, source_Pos.y, source_Pos.z);
					float distance2 = Vector2.Distance(source_Pos, lock_Pos);
					Vector3 vector2 = (source_Pos.xy() - lock_Pos.xy()).normalized;
					MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", source_Pos, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, -vector2)), Array.Empty<object>()).SetEffect(distance2, new Color(1f, 1f, 0f, 0.7f), new Color(1f, 0.54f, 0f), 1f, 3f);
				}
				PunchStatus = 0;
			}
			else
			{
				int num6 = OrangeBattleUtility.Random(0, 1000);
				for (int l = 0; l < nearAi.Length; l++)
				{
					if (num6 > nearRaund[l] && num6 < nearRaund[l + 1])
					{
						mainStatus = (MainStatus)nearAi[l];
					}
				}
			}
		}
		else
		{
			mainStatus = (MainStatus)OrangeBattleUtility.Random(2, 7);
		}
		SetStatus(mainStatus);
	}

	private void UpdateCollider()
	{
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.Idle || (uint)(mainStatus - 7) <= 2u)
		{
			if (ClawCollideBullet.IsActivate)
			{
				ClawCollideBullet.IsDestroy = true;
			}
			if (Jump_Run_CollideBullet.IsActivate)
			{
				Jump_Run_CollideBullet.IsDestroy = true;
			}
		}
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Normal:
				_currentAnimationId = AnimationID.ANI_JUMP_END;
				break;
			case SubStatus.Wait:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.End:
				_currentAnimationId = AnimationID.ANI_DEBUT;
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
			if (_subStatus != SubStatus.End)
			{
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			}
			return;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		case MainStatus.Run:
			if (AILevel == 1)
			{
				switch (_subStatus)
				{
				case SubStatus.Prepare:
					_currentAnimationId = AnimationID.ANI_RUN_START;
					break;
				case SubStatus.Normal:
					_currentAnimationId = AnimationID.ANI_RUN_LOOP;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				_currentAnimationId = AnimationID.ANI_RUN_LOOP;
			}
			break;
		case MainStatus.Jump:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				_currentAnimationId = AnimationID.ANI_JUMP_START;
				break;
			case SubStatus.Normal:
				_currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.End:
				_currentAnimationId = AnimationID.ANI_JUMP_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Plasma:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				_currentAnimationId = AnimationID.ANI_PLASMA_START;
				break;
			case SubStatus.Normal:
				_currentAnimationId = AnimationID.ANI_PLASMA_LOOP;
				break;
			case SubStatus.End:
				_currentAnimationId = AnimationID.ANI_PLASMA_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Punch:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				_currentAnimationId = AnimationID.ANI_PUNCH_START;
				break;
			case SubStatus.Normal:
				_currentAnimationId = AnimationID.ANI_PUNCH_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Shoot:
			switch (_subStatus)
			{
			case SubStatus.Prepare:
				_currentAnimationId = AnimationID.ANI_SHOOT_START;
				break;
			case SubStatus.Normal:
				_currentAnimationId = AnimationID.ANI_SHOOT_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (AILevel == 1)
		{
			if (_mainStatus == MainStatus.Run && _subStatus == SubStatus.Normal)
			{
				_animator.speed = 5f;
			}
			else
			{
				_animator.speed = 1f;
			}
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
		}
		else
		{
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
		}
	}

	private void ShootBullet(int id)
	{
		Vector3 pDirection = Vector3.zero;
		switch (id)
		{
		case 2:
			pDirection = ((!(null != Target) || AILevel != 1) ? _shootPointTransform.right : (-(_shootPointTransform.position - Target.transform.position).normalized));
			break;
		case 3:
			pDirection = new Vector3((float)base.direction * Mathf.Cos((float)Math.PI * 5f / 12f), Mathf.Sin((float)Math.PI * 5f / 12f), 0f);
			break;
		}
		BulletBase.TryShotBullet(EnemyWeapons[id].BulletData, _shootPointTransform, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		_shootNum++;
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (StageUpdate.gbIsNetGame)
		{
			if (targetPos.x > Controller.LogicPosition.x)
			{
				base.direction = 1;
			}
			else
			{
				base.direction = -1;
			}
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
		base.transform.position = pos;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg, base.UpdateHurtAction);
		if ((int)Hp > 0)
		{
			_characterMaterial.Hurt();
		}
		else if (_mainStatus != MainStatus.Dead)
		{
			StageUpdate.SlowStage();
			Explosion();
			SetStatus(MainStatus.Dead);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_BOSS_EXPLODE2", new Vector3(base.transform.position.x, base.transform.position.y + 1f, base.transform.position.z), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
		}
		return Hp;
	}

	public override void BossIntro(Action cb)
	{
		if (_mainStatus == MainStatus.Debut)
		{
			SetStatus(_mainStatus, SubStatus.End);
			IntoBack = cb;
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		ClawCollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
		Jump_Run_CollideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
	}
}
