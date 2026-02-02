using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS048_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		ANI_DEBUT_FALL = 1,
		ANI_DEBUT_LAND = 2,
		ANI_DEBUT = 3,
		ANI_SKILL0_START1 = 4,
		ANI_SKILL0_LOOP1 = 5,
		ANI_SKILL0_START2 = 6,
		ANI_SKILL0_LOOP2 = 7,
		ANI_SKILL0_START3 = 8,
		ANI_SKILL0_LOOP3 = 9,
		ANI_SKILL0_END = 10,
		ANI_SKILL1_START = 11,
		ANI_SKILL1_LOOP = 12,
		ANI_SKILL1_END = 13,
		ANI_SKILL2_START = 14,
		ANI_SKILL2_LOOP = 15,
		ANI_SKILL2_END = 16,
		ANI_SKILL3_START = 17,
		ANI_SKILL3_LOOP = 18,
		ANI_SKILL3_END = 19,
		ANI_SKILL4_START1 = 20,
		ANI_SKILL4_LOOP1 = 21,
		ANI_SKILL4_START2 = 22,
		ANI_SKILL4_LOOP2 = 23,
		ANI_SKILL4_END = 24,
		ANI_HURT = 25,
		ANI_DEAD = 26,
		MAX_ANIMATION_ID = 27
	}

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus _mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus _subStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private AnimationID _currentAnimationId;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private float _currentFrame;

	private int nDeadCount;

	private int[] _animationHash;

	private int[] DefaultSkillCard = new int[5] { 0, 1, 2, 3, 4 };

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private int IdleWaitFrame;

	[SerializeField]
	private float IdleWaitTime;

	[SerializeField]
	private float SK0JumpTime = 0.9f;

	private VInt3 JumpForce = VInt3.zero;

	private float EndPosX;

	[SerializeField]
	private float ShootAngle = 32f;

	[SerializeField]
	private Transform MouthPos;

	[SerializeField]
	private float JumpDis = 2f;

	[SerializeField]
	private ParticleSystem JetFx;

	[SerializeField]
	private ParticleSystem MouthFx;

	private int ShootTimes = -1;

	[SerializeField]
	private Transform HandPos;

	[SerializeField]
	private ParticleSystem ShieldFx;

	[SerializeField]
	private float SK4JumpTime = 1.2f;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

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
		_animationHash[0] = Animator.StringToHash("BS048@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS048@skill_01_step3_loop");
		_animationHash[2] = Animator.StringToHash("BS048@skill_01_step3_end");
		_animationHash[3] = Animator.StringToHash("BS048@debut");
		_animationHash[4] = Animator.StringToHash("BS048@skill_01_step1_start");
		_animationHash[5] = Animator.StringToHash("BS048@skill_01_step1_loop");
		_animationHash[6] = Animator.StringToHash("BS048@skill_01_step2_start");
		_animationHash[7] = Animator.StringToHash("BS048@skill_01_step2_loop");
		_animationHash[8] = Animator.StringToHash("BS048@skill_01_step3_start");
		_animationHash[9] = Animator.StringToHash("BS048@skill_01_step3_loop");
		_animationHash[10] = Animator.StringToHash("BS048@skill_01_step3_end");
		_animationHash[11] = Animator.StringToHash("BS048@skill_02_start");
		_animationHash[12] = Animator.StringToHash("BS048@skill_02_loop");
		_animationHash[13] = Animator.StringToHash("BS048@skill_02_end");
		_animationHash[14] = Animator.StringToHash("BS048@skill_03_start");
		_animationHash[15] = Animator.StringToHash("BS048@skill_03_loop");
		_animationHash[16] = Animator.StringToHash("BS048@skill_03_end");
		_animationHash[17] = Animator.StringToHash("BS048@skill_04_start");
		_animationHash[18] = Animator.StringToHash("BS048@skill_04_loop");
		_animationHash[19] = Animator.StringToHash("BS048@skill_04_end");
		_animationHash[20] = Animator.StringToHash("BS048@skill_05_step1_start");
		_animationHash[21] = Animator.StringToHash("BS048@skill_05_step1_loop");
		_animationHash[22] = Animator.StringToHash("BS048@skill_05_step2_start");
		_animationHash[23] = Animator.StringToHash("BS048@skill_05_step2_loop");
		_animationHash[24] = Animator.StringToHash("BS048@skill_05_step2_end");
		_animationHash[26] = Animator.StringToHash("BS048@death");
		_animationHash[25] = Animator.StringToHash("BS048@hurt_loop");
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] componentsInChildren = _transform.GetComponentsInChildren<Transform>(true);
		_animationHash = new int[27];
		HashAnimation();
		LoadParts(componentsInChildren);
		base.AimPoint = new Vector3(0f, 1.5f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
		FallDownSE = new string[2] { "BossSE03", "bs110_raider06" };
		AiTimer.TimerStart();
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
					UpdateDirection();
				}
				break;
			case SubStatus.Phase1:
				PlayBossSE("BossSE03", "bs110_raider06");
				break;
			case SubStatus.Phase4:
				if (IntroCallBack != null)
				{
					IntroCallBack();
				}
				break;
			}
			break;
		case MainStatus.Idle:
			IdleWaitFrame = (int)(IdleWaitTime * 20f) + GameLogicUpdateManager.GameFrame;
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				TargetPos = new VInt3(Target._transform.position);
			}
			UpdateDirection();
			_velocity = VInt3.zero;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
					UpdateDirection();
				}
				JetFx.Play();
				break;
			case SubStatus.Phase1:
			{
				EndPosX = _transform.position.x + JumpDis * (float)base.direction;
				int num3 = (int)(SK0JumpTime * 20f);
				float num4 = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
				JumpForce.y = (int)((0f - num4) * (float)num3 / 2f);
				JumpForce.x = (int)((EndPosX - _transform.position.x) / (float)num3 * 1000f * 20f);
				_velocity = JumpForce;
				PlaySE("BossSE03", "bs110_raider05");
				break;
			}
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				IgnoreGravity = true;
				JetFx.Stop();
				MouthFx.Play();
				break;
			case SubStatus.Phase3:
			{
				MouthFx.Stop();
				Vector3 down = Vector3.down;
				Vector3 position = MouthFx.transform.position;
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, position, down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				down = Quaternion.Euler(0f, 0f, ShootAngle) * Vector3.down;
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, position, down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				down = Quaternion.Euler(0f, 0f, 0f - ShootAngle) * Vector3.down;
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, position, down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				PlaySE("BossSE03", "bs110_raider01");
				break;
			}
			case SubStatus.Phase4:
				IgnoreGravity = false;
				_velocity.x = JumpForce.x;
				break;
			case SubStatus.Phase5:
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill1:
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				ShootTimes--;
				BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, HandPos.position, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				PlaySE("BossSE03", "bs110_raider03");
				ShieldFx.Play();
				break;
			case SubStatus.Phase2:
			{
				EM130_Controller eM130_Controller = null;
				EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[3].BulletData.f_EFFECT_X], sNetSerialID + GameLogicUpdateManager.GameFrame, 16);
				if ((bool)enemyControllerBase)
				{
					eM130_Controller = enemyControllerBase.gameObject.GetComponent<EM130_Controller>();
				}
				if ((bool)eM130_Controller)
				{
					eM130_Controller.SetPositionAndRotation(MouthPos.position, base.direction);
					eM130_Controller.SetActive(true);
				}
				break;
			}
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				if ((bool)Target)
				{
					EndPosX = Target._transform.position.x;
				}
				else
				{
					EndPosX = _transform.position.x + JumpDis * (float)base.direction;
				}
				int num = (int)(SK4JumpTime * 20f);
				float num2 = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
				JumpForce.y = (int)((0f - num2) * (float)num / 2f);
				JumpForce.x = (int)((EndPosX - _transform.position.x) / (float)num * 1000f * 20f);
				break;
			}
			case SubStatus.Phase1:
				PlaySE("BossSE03", "bs110_raider05");
				JetFx.Play();
				_velocity = JumpForce;
				break;
			case SubStatus.Phase2:
				JetFx.Stop();
				IgnoreGravity = true;
				_velocity = VInt3.zero;
				_collideBullet.UpdateBulletData(EnemyWeapons[4].BulletData);
				_collideBullet.Active(targetMask);
				break;
			case SubStatus.Phase3:
				IgnoreGravity = false;
				_velocity.x = JumpForce.x;
				break;
			case SubStatus.Phase4:
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
				_velocity = VInt3.zero;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.Active(targetMask);
				break;
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
			{
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_003)
				{
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
				}
				else
				{
					StartCoroutine(BossDieFlow(GetTargetPoint()));
				}
				break;
			}
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
				_currentAnimationId = AnimationID.ANI_DEBUT_FALL;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT_LAND;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEBUT;
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
				_currentAnimationId = AnimationID.ANI_SKILL0_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL0_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
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
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
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
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL3_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL3_END;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL4_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL4_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL4_END;
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

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.Idle:
			mainStatus = ((ShootTimes < 0) ? ((!Target || !(Target._transform.position.y - 2.5f > _transform.position.y)) ? ((MainStatus)RandomCard(2)) : MainStatus.Skill4) : ((MainStatus)(OrangeBattleUtility.Random(30, 50) / 10)));
			break;
		}
		if ((mainStatus == MainStatus.Skill1 || mainStatus == MainStatus.Skill2) && ShootTimes < 0)
		{
			ShootTimes = 2;
		}
		if (DebugMode)
		{
			mainStatus = NextSkill;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
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
					SetStatus(MainStatus.Debut, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f && IntroCallBack != null)
				{
					IntroCallBack();
					if (!bWaitNetStatus)
					{
						UpdateRandomState();
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus && GameLogicUpdateManager.GameFrame > IdleWaitFrame)
			{
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
					UpdateDirection();
					UpdateRandomState();
				}
				else
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					SetStatus(MainStatus.Skill0);
				}
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.9f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y <= 0)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
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
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.8f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y <= 0)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 0.8f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
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
			ShootTimes = -1;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			SetStatus(MainStatus.Debut);
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		_transform.position = pos;
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

	private int RandomCard(int StartPos)
	{
		if (SkillCard.ToArray().Length < 1)
		{
			SkillCard = new List<int>(DefaultSkillCard);
		}
		int num = SkillCard[OrangeBattleUtility.Random(0, SkillCard.ToArray().Length)];
		SkillCard.Remove(num);
		return num + StartPos;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)ShieldFx)
			{
				ShieldFx.Stop();
			}
			if ((bool)JetFx)
			{
				JetFx.Stop();
			}
			if ((bool)MouthFx)
			{
				MouthFx.Stop();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	private void LoadParts(Transform[] childs)
	{
		if (!ModelTransform)
		{
			ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		}
		if ((bool)ModelTransform)
		{
			_animator = ModelTransform.GetComponent<Animator>();
		}
		if (!_collideBullet)
		{
			_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "BodyCollider").gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (!MouthPos)
		{
			MouthPos = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPoint_Mouth", true);
		}
		if (!MouthFx)
		{
			MouthFx = OrangeBattleUtility.FindChildRecursive(ref childs, "MouthFx", true).GetComponent<ParticleSystem>();
		}
		if (!ShieldFx)
		{
			ShieldFx = OrangeBattleUtility.FindChildRecursive(ref childs, "ShieldFx", true).GetComponent<ParticleSystem>();
		}
		if (!JetFx)
		{
			JetFx = OrangeBattleUtility.FindChildRecursive(ref childs, "JetFx", true).GetComponent<ParticleSystem>();
		}
		if (!HandPos)
		{
			HandPos = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPoint_Hand_R", true);
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_003)
		{
			base.DeadPlayCompleted = true;
		}
	}
}
