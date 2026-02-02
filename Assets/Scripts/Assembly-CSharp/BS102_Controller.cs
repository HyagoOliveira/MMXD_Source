using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS102_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Skill2 = 4,
		Skill3 = 5,
		Skill4 = 6,
		Die = 7
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		Phase6 = 6,
		MAX_SUBSTATUS = 7
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT_FALL_LOOP = 1,
		ANI_DEBUT_FALL_END = 2,
		ANI_DEBUT_END = 3,
		ANI_Skill0_START = 4,
		ANI_Skill0_LOOP = 5,
		ANI_Skill0_END = 6,
		ANI_Skill1_START = 7,
		ANI_Skill1_LOOP = 8,
		ANI_Skill1_END = 9,
		ANI_Skill2_START1 = 10,
		ANI_Skill2_LOOP1 = 11,
		ANI_Skill2_START2 = 12,
		ANI_Skill2_LOOP2 = 13,
		ANI_Skill2_END = 14,
		ANI_Skill3_START = 15,
		ANI_Skill3_LOOP = 16,
		ANI_Skill3_END = 17,
		ANI_HURT = 18,
		ANI_DEAD = 19,
		MAX_ANIMATION_ID = 20
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill = MainStatus.Skill0;

	private int AIStep;

	private float RoomXSize = 18f;

	private int nDeadCount;

	private int ShootTime;

	[SerializeField]
	private Transform NorsePoint;

	private CollideBullet WaterCollide;

	[SerializeField]
	private ParticleSystem WaterSplashFX;

	private Transform[] TargetList = new Transform[3];

	private readonly int _HashAngle = Animator.StringToHash("angle");

	[SerializeField]
	private Transform ShootPoint;

	private int SpawnCount;

	private int JumpForce = 18000;

	private CollideBullet PressCollide;

	private float NextFrame;

	private bool _isCatching;

	private bool _isKnockout;

	private new VInt3 AddForce;

	private VInt3 KnockOutForce = new VInt3(12000, 15000, 0);

	private OrangeCharacter targetOC;

	[SerializeField]
	private Vector3 Displacement = new Vector3(0f, -0.8f, 0f);

	private bool CanSummon;

	private bool DeadCallResult = true;

	private MainStatus[] AICircuit = new MainStatus[4]
	{
		MainStatus.Idle,
		MainStatus.Skill1,
		MainStatus.Skill2,
		MainStatus.Skill0
	};

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected void ChangeDebugMode()
	{
		DebugMode = !DebugMode;
	}

	protected void ChangeSetSkill(object[] param)
	{
		string text = param[0] as string;
		if (!(text == string.Empty))
		{
			switch (text)
			{
			case "Idle":
				NextSkill = MainStatus.Idle;
				break;
			case "Skill0":
				NextSkill = MainStatus.Skill0;
				break;
			case "Skill1":
				NextSkill = MainStatus.Skill1;
				break;
			case "Skill2":
				NextSkill = MainStatus.Skill2;
				break;
			case "Skill3":
				NextSkill = MainStatus.Skill3;
				break;
			case "Skill4":
				NextSkill = MainStatus.Skill4;
				break;
			}
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash[0] = Animator.StringToHash("BS051@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS051@skill_03_fall_loop");
		_animationHash[2] = Animator.StringToHash("BS051@skill_03_landing");
		_animationHash[3] = Animator.StringToHash("BS051@debut");
		_animationHash[4] = Animator.StringToHash("BS051@skill_01_start");
		_animationHash[5] = Animator.StringToHash("BS102@skill_01_loop");
		_animationHash[6] = Animator.StringToHash("BS051@skill_01_end");
		_animationHash[7] = Animator.StringToHash("BS051@skill_02_start");
		_animationHash[8] = Animator.StringToHash("BS051@skill_02_loop");
		_animationHash[9] = Animator.StringToHash("BS051@skill_02_end");
		_animationHash[10] = Animator.StringToHash("BS051@skill_03_jump_start");
		_animationHash[11] = Animator.StringToHash("BS051@skill_03_jump_loop");
		_animationHash[12] = Animator.StringToHash("BS051@skill_03_fall_start");
		_animationHash[13] = Animator.StringToHash("BS051@skill_03_fall_loop");
		_animationHash[14] = Animator.StringToHash("BS051@skill_03_landing");
		_animationHash[15] = Animator.StringToHash("BS051@skill_04_start");
		_animationHash[16] = Animator.StringToHash("BS051@skill_04_loop");
		_animationHash[17] = Animator.StringToHash("BS051@skill_04_end");
		_animationHash[18] = Animator.StringToHash("BS051@hurt_loop");
		_animationHash[19] = Animator.StringToHash("BS051@dead");
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] componentsInChildren = _transform.GetComponentsInChildren<Transform>(true);
		_animationHash = new int[20];
		HashAnimation();
		LoadParts(componentsInChildren);
		base.AimPoint = new Vector3(0f, 1.2f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
		FallDownSE = new string[2] { "BossSE04", "bs036_mrmammo04" };
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
		SetStatus((MainStatus)nSet);
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
		ModelTransform.localEulerAngles = new Vector3(0f, -90 + base.direction * 25, 0f);
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)(-base.direction));
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
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ShootTime = 1;
				if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right * base.direction, 1f, Controller.collisionMask, _transform))
				{
					UpdateDirection(-base.direction);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ShootTime = 2;
				_animator.SetFloat(_HashAngle, 0f);
				break;
			case SubStatus.Phase1:
			{
				if ((bool)Target)
				{
					Transform transform2 = Target._transform;
				}
				Vector2 vector3 = ShootPoint.position;
				BS102_WaterBullet obj = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, vector3, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, false, _isCatching) as BS102_WaterBullet;
				obj.HitBlockCallback = SpawnWater;
				obj.SetSpeedMultiplier((float)(ShootTime - 1) * 1f + 1f);
				break;
			}
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
					if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Target._transform.position, Vector2.right * base.direction, 1.5f, Controller.collisionMask, _transform))
					{
						TargetPos.x -= 1200 * base.direction;
					}
					int num = Math.Abs(TargetPos.x - Controller.LogicPosition.x);
					int num2 = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
					int num3 = -(JumpForce * 2) / num2;
					int num4 = num * 20 / num3;
					_velocity = new VInt3(num4 * base.direction, JumpForce, 0);
				}
				break;
			case SubStatus.Phase2:
				_collideBullet.BackToPool();
				PressCollide.Active(targetMask);
				break;
			case SubStatus.Phase4:
				_collideBullet.Active(targetMask);
				PressCollide.BackToPool();
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				NextFrame = 0f;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				NextFrame = 0f;
				ShootTime = 3;
				_animator.SetFloat(_HashAngle, -45f);
				break;
			case SubStatus.Phase1:
			{
				ShootTime--;
				_animator.SetFloat(_HashAngle, 25f);
				Vector2 vector = ShootPoint.position;
				Vector2 vector2 = Vector3.right * base.direction * 2.5f + Vector3.up + ShootPoint.position;
				BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, vector, vector2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, false, true);
				if (ShootTime <= 0 && _isCatching)
				{
					_isCatching = false;
					if ((bool)targetOC)
					{
						_isKnockout = true;
						AddForce = new VInt3(KnockOutForce.x * base.direction, KnockOutForce.y, 0);
					}
				}
				break;
			}
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				nDeadCount = 0;
				if (Controller.Collisions.below)
				{
					base.SoundSource.PlaySE("BossSE04", "bs036_mrmammo04", 1.4f);
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				else
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				IgnoreGravity = true;
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase3:
				if (DeadCallResult)
				{
					StartCoroutine(BossDieFlow(GetTargetPoint()));
				}
				else
				{
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
				}
				break;
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT_FALL_LOOP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT_FALL_END;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEBUT_END;
				break;
			}
			break;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill0_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill0_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill0_END;
				break;
			}
			break;
		case MainStatus.Skill1:
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill1_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill1_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill1_END;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill2_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill2_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill2_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill2_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill2_END;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill3_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill3_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill3_END;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
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
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_introReady)
				{
					base.SoundSource.PlaySE("BossSE04", "bs036_mrmammo01", 0.5f);
					SetStatus(MainStatus.Debut, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f && IntroCallBack != null)
				{
					IntroCallBack();
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					UpdateNextState();
				}
				else
				{
					UpdateNextState(2);
				}
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0f && ShootTime > 0)
				{
					ShootTime--;
					WaterCollide.Active(targetMask);
					WaterSplashFX.Play();
				}
				else if (_currentFrame > 0.6f && WaterCollide.IsActivate)
				{
					if (WaterCollide.IsActivate)
					{
						WaterCollide.BackToPool();
					}
					WaterSplashFX.Stop();
				}
				else if (_currentFrame > 1f)
				{
					WaterSplashFX.Stop();
					if (ShootTime <= 0)
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase1);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f && !bWaitNetStatus)
				{
					UpdateNextState();
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					if (--ShootTime > 0)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f && !bWaitNetStatus)
				{
					UpdateNextState();
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((float)_velocity.y < 0f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
			{
				if (!Controller.Collisions.below)
				{
					break;
				}
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
				if (AiState == AI_STATE.mob_002 && CanSummon)
				{
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
				}
				for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
				{
					EM174_Controller eM174_Controller = StageUpdate.runEnemys[i].mEnemy as EM174_Controller;
					if ((bool)eM174_Controller)
					{
						eM174_Controller.StartBurn();
					}
				}
				SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				break;
			}
			case SubStatus.Phase4:
				if (_currentFrame > 1f && !bWaitNetStatus)
				{
					if ((bool)Target)
					{
						UpdateNextState();
					}
					else
					{
						UpdateNextState(2);
					}
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.63f && _currentFrame > NextFrame)
				{
					NextFrame = 2f;
					Vector2 start = NorsePoint.position;
					Vector3 end = new Vector3(9 * base.direction, 0f, 0f) + _transform.position;
					ShootAndSetWater(start, end);
				}
				else if (_currentFrame > 0.15f && _currentFrame > NextFrame)
				{
					NextFrame = 0.63f;
					Vector2 start2 = NorsePoint.position;
					Vector3 end2 = new Vector3(5 * base.direction, 0f, 0f) + _transform.position;
					ShootAndSetWater(start2, end2);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f && !bWaitNetStatus)
				{
					UpdateNextState();
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			if (_isCatching && (bool)targetOC)
			{
				targetOC.transform.position = Displacement + ShootPoint.position;
				targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.5f && !targetOC && !bWaitNetStatus)
				{
					TryCatchPlayer();
					_animator.SetFloat(_HashAngle, 90f * (_currentFrame - 1f));
				}
				if (!(_currentFrame > 1f) || bWaitNetStatus)
				{
					break;
				}
				if (!targetOC)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					Vector3 vector = new Vector3(8 * base.direction, 0f, 0f) + ShootPoint.position;
					if ((bool)Target)
					{
						vector = Target.transform.position;
					}
					float num = Math.Abs(vector.x - _transform.position.x);
					if (AIStep == 7 || num > RoomXSize / 2f)
					{
						UpdateNextState();
					}
					else
					{
						UpdateNextState(5);
					}
				}
				else
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					if (ShootTime <= 0)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase1);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					UpdateNextState();
				}
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					if (nDeadCount > 10)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase3);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 0.58f)
				{
					if (nDeadCount == 0)
					{
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
					}
					if (nDeadCount > 10)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase3);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			}
			break;
		}
		if (_isKnockout && (bool)targetOC)
		{
			VInt vInt = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			AddForce.y += vInt * (VInt)1f / 1000;
			targetOC.AddForce(AddForce);
			if (AddForce.y <= 1000 || (int)targetOC.Hp <= 0)
			{
				targetOC.SetStun(false);
				targetOC = null;
				_isKnockout = false;
			}
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
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
			PressCollide.UpdateBulletData(EnemyWeapons[3].BulletData);
			PressCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			WaterCollide.UpdateBulletData(EnemyWeapons[4].BulletData);
			WaterCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
			PressCollide.BackToPool();
		}
	}

	private void ResetSkillParam()
	{
		ShootTime = 0;
		_velocity = VInt3.zero;
	}

	private void UploadStatus(MainStatus status)
	{
		ResetSkillParam();
		if (status != 0)
		{
			if (CheckHost())
			{
				UploadEnemyStatus((int)status);
			}
		}
		else
		{
			SetStatus(MainStatus.Idle);
		}
	}

	private void UpdateNextState(int Step = -1)
	{
		if (bWaitNetStatus)
		{
			return;
		}
		if (DebugMode)
		{
			UploadStatus(NextSkill);
			return;
		}
		if (Step == -1)
		{
			AIStep = (AIStep + 1) % AICircuit.Length;
		}
		else
		{
			AIStep = Step % AICircuit.Length;
		}
		UploadStatus(AICircuit[AIStep]);
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
		IntroCallBack = cb;
		_introReady = true;
	}

	public override void Unlock()
	{
		_unlockReady = true;
		base.AllowAutoAim = true;
		if ((int)Hp > 0)
		{
			SetColliderEnable(true);
		}
		if (InGame)
		{
			Activate = true;
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)targetOC)
			{
				targetOC.SetStun(false);
				targetOC = null;
				_isKnockout = false;
			}
			if ((bool)WaterSplashFX)
			{
				WaterSplashFX.Stop();
			}
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)PressCollide)
			{
				PressCollide.BackToPool();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
			CanSummon = false;
		}
	}

	private void LoadParts(Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "BodyCollider", true).gameObject.AddOrGetComponent<CollideBullet>();
		PressCollide = OrangeBattleUtility.FindChildRecursive(ref childs, "PressCollider", true).gameObject.AddOrGetComponent<CollideBullet>();
		WaterCollide = OrangeBattleUtility.FindChildRecursive(ref childs, "WaterCollide", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (WaterSplashFX == null)
		{
			WaterSplashFX = OrangeBattleUtility.FindChildRecursive(ref childs, "WaterSplashFX").gameObject.GetComponent<ParticleSystem>();
		}
		if (!NorsePoint)
		{
			NorsePoint = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 Ponytail1Nub", true);
		}
		if (!ShootPoint)
		{
			ShootPoint = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 L Hand", true);
		}
	}

	private void ShootAndSetWater(Vector2 start, Vector3 end)
	{
		BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, start, end, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
	}

	private void SpawnWater(object obj)
	{
		if (obj == null)
		{
			return;
		}
		Vector3 vector = (Vector3)obj;
		EM174_Controller eM174_Controller = SpawnWater();
		if ((bool)eM174_Controller)
		{
			eM174_Controller.SetParent(this);
			Vector3 vector2 = new Vector3(0f, 0.8f, 0f) + vector;
			RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector2, Vector2.down, 5f, Controller.collisionMask, _transform);
			Vector3 pos = vector2;
			if ((bool)raycastHit2D)
			{
				pos = raycastHit2D.point + Vector2.up * 0.01f;
			}
			eM174_Controller.SetPositionAndRotation(pos, base.direction == -1);
			eM174_Controller.SetActive(true);
		}
	}

	private EM174_Controller SpawnWater()
	{
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[2].BulletData.f_EFFECT_X], sNetSerialID + SpawnCount, 16);
		if ((bool)enemyControllerBase)
		{
			SpawnCount++;
			PlaySE("BossSE04", "bs036_mrmammo03");
			return enemyControllerBase.gameObject.GetComponent<EM174_Controller>();
		}
		return null;
	}

	private void FireOnHit(object obj)
	{
		if (obj == null)
		{
			return;
		}
		StageObjParam component = (obj as Collider2D).gameObject.GetComponent<StageObjParam>();
		if (!(component == null))
		{
			EM174_Controller eM174_Controller = component.tLinkSOB as EM174_Controller;
			if (!(eM174_Controller == null) && (!eM174_Controller._readyBurn || !eM174_Controller._isBurning))
			{
				eM174_Controller.StartBurn();
			}
		}
	}

	private void TryCatchPlayer()
	{
		Vector2 point = new Vector3(0.25f * (float)base.direction, 0f, 0f) + ShootPoint.position;
		Vector2 size = new Vector2(1f, 1f);
		Collider2D collider2D = Physics2D.OverlapBox(point, size, 0f, LayerMask.GetMask("Player"));
		if (!collider2D)
		{
			return;
		}
		targetOC = OrangeBattleUtility.GetHitTargetOrangeCharacter(collider2D);
		if (!targetOC || targetOC.IsStun)
		{
			return;
		}
		if ((bool)targetOC.IsUnBreakX())
		{
			targetOC = null;
			return;
		}
		targetOC.transform.position = Displacement + ShootPoint.position;
		targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
		targetOC.transform.rotation = Quaternion.Euler(Vector3.zero);
		targetOC.SetStun(true);
		_isCatching = true;
		PlaySE("BossSE03", "bs023_mammo09");
		if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right * base.direction, 2f, Controller.collisionMask, _transform))
		{
			UpdateDirection(-base.direction);
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_001:
			base.DeadPlayCompleted = true;
			break;
		case AI_STATE.mob_002:
			CanSummon = true;
			break;
		case AI_STATE.mob_003:
			DeadCallResult = false;
			break;
		}
	}
}
