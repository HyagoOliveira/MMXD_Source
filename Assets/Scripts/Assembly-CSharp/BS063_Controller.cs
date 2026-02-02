#define RELEASE
using System;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS063_Controller : EnemyControllerBase, IManagedUpdateBehavior, IF_Master
{
	private enum MainStatus
	{
		Idle = 0,
		FFIdle = 1,
		Debut = 2,
		Skill0 = 3,
		Skill1 = 4,
		Skill2 = 5,
		Skill3 = 6,
		Skill4 = 7,
		Skill5 = 8,
		Die = 9
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
		FFANI_IDLE = 1,
		ANI_DEBUT = 2,
		ANI_DEBUT_LOOP = 3,
		ANI_SKILL0_LOOP = 4,
		ANI_SKILL1_START = 5,
		ANI_SKILL1_LOOP = 6,
		ANI_SKILL1_END = 7,
		ANI_SKILL2_START1 = 8,
		ANI_SKILL2_LOOP1 = 9,
		ANI_SKILL2_START2 = 10,
		ANI_SKILL2_LOOP2 = 11,
		ANI_SKILL2_END2 = 12,
		ANI_SKILL3_START1 = 13,
		ANI_SKILL3_LOOP1 = 14,
		ANI_SKILL3_START2 = 15,
		ANI_SKILL3_LOOP2 = 16,
		ANI_SKILL3_START3 = 17,
		ANI_SKILL3_END3 = 18,
		ANI_SKILL4_START = 19,
		ANI_SKILL4_LOOP = 20,
		ANI_SKILL4_END = 21,
		ANI_SKILL5_START = 22,
		ANI_SKILL5_LOOP = 23,
		ANI_SKILL5_END = 24,
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

	private MainStatus _FFmainStatus;

	private SubStatus _FFsubStatus;

	private float _currentFFFrame;

	private int nDeadCount;

	private int[] _animationHash;

	private int[] _FFanimaionHash;

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[Header("待機")]
	[SerializeField]
	private float IdleWaitTime = 3f;

	private int IdleWaitFrame;

	[SerializeField]
	private float FFIdleWaitTime = 1f;

	private int FFIdleWaitFrame;

	[SerializeField]
	[Tooltip("判斷X距離")]
	private float JudgeXDis = 8f;

	[Header("Mesh列")]
	[SerializeField]
	private GameObject HandMeshL;

	[SerializeField]
	private GameObject HandMeshR;

	[SerializeField]
	private GameObject SwordMesh;

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	private float MaxXPos;

	private float MinXPos;

	private float CenterXPos;

	private int ActionTimes;

	private float ActionAnimatorFrame;

	private int ActionFrame;

	private bool HasActed;

	[Header("走路")]
	private int WalkTimes;

	private int TotalWalkTImes;

	[SerializeField]
	private int WalkSpeed = 2000;

	[SerializeField]
	private float WalkTime = 1f;

	[Header("揮擊")]
	[SerializeField]
	private float Skill1ActionFrame = 0.8f;

	[Header("後跳")]
	[SerializeField]
	private bool NeedSkill2;

	[SerializeField]
	private int BackSpeed = 15000;

	private bool FFIsIdle;

	private CollideBullet _FFcollideBullet;

	[Header("火箭飛拳")]
	[SerializeField]
	private Transform LHandModel;

	[SerializeField]
	private Transform RHandModel;

	private EM181_Controller EMLHand;

	private EM181_Controller EMRHand;

	private BossCorpsTool LHandCorp;

	private BossCorpsTool RHandCorp;

	private int SpawnCount;

	[Header("劍氣衝擊波")]
	[SerializeField]
	private ParticleSystem SwordFX;

	[SerializeField]
	private Transform SwordPos;

	[Header("追蹤彈")]
	[SerializeField]
	private int Skill5ShootTimes = 6;

	[SerializeField]
	private Transform LHandPos;

	[SerializeField]
	private Transform RHandPos;

	private bool isLeft;

	private int AIStep;

	private MainStatus[] AICircuit = new MainStatus[3]
	{
		MainStatus.Skill5,
		MainStatus.Skill4,
		MainStatus.Skill3
	};

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	[SerializeField]
	private MainStatus FFNextSkill;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

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
				FFNextSkill = MainStatus.FFIdle;
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
				FFNextSkill = MainStatus.Skill3;
				break;
			case "Skill4":
				FFNextSkill = MainStatus.Skill4;
				break;
			case "Skill5":
				FFNextSkill = MainStatus.Skill5;
				break;
			}
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

	protected virtual void HashAnimation()
	{
		_animationHash = new int[27];
		_animationHash[0] = Animator.StringToHash("BS063@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS063@idle_loop");
		_animationHash[2] = Animator.StringToHash("BS063@debut");
		_animationHash[3] = Animator.StringToHash("BS063@debut_standby_loop");
		_animationHash[4] = Animator.StringToHash("BS063@walk_loop");
		_animationHash[5] = Animator.StringToHash("BS063@skill_4_ver2_start");
		_animationHash[6] = Animator.StringToHash("BS063@skill_4_ver2_loop");
		_animationHash[7] = Animator.StringToHash("BS063@skill_4_ver2_end");
		_animationHash[8] = Animator.StringToHash("BS063@skill_5_step1_start");
		_animationHash[9] = Animator.StringToHash("BS063@skill_5_step1_loop");
		_animationHash[10] = Animator.StringToHash("BS063@skill_5_step2_start");
		_animationHash[11] = Animator.StringToHash("BS063@skill_5_step2_loop");
		_animationHash[12] = Animator.StringToHash("BS063@skill_5_step2_end");
		_animationHash[13] = Animator.StringToHash("BS063@skill_1_step1_start");
		_animationHash[14] = Animator.StringToHash("BS063@skill_1_step1_loop");
		_animationHash[15] = Animator.StringToHash("BS063@skill_1_step2_start");
		_animationHash[16] = Animator.StringToHash("BS063@skill_1_step2_loop");
		_animationHash[17] = Animator.StringToHash("BS063@skill_1_step3_start");
		_animationHash[18] = Animator.StringToHash("BS063@skill_1_step3_end");
		_animationHash[19] = Animator.StringToHash("BS063@skill_2_start");
		_animationHash[20] = Animator.StringToHash("BS063@skill_2_loop");
		_animationHash[21] = Animator.StringToHash("BS063@skill_2_end");
		_animationHash[22] = Animator.StringToHash("BS063@skill_3_start");
		_animationHash[23] = Animator.StringToHash("BS063@skill_3_loop");
		_animationHash[24] = Animator.StringToHash("BS063@skill_3_end");
		_animationHash[25] = Animator.StringToHash("BS063@hurt_loop");
		_animationHash[26] = Animator.StringToHash("BS063@death");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		_FFcollideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "FFCollider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (HandMeshL == null)
		{
			HandMeshL = OrangeBattleUtility.FindChildRecursive(ref childs, "BS063_HandMesh_L", true).gameObject;
		}
		if (HandMeshR == null)
		{
			HandMeshR = OrangeBattleUtility.FindChildRecursive(ref childs, "BS063_HandMesh_R", true).gameObject;
		}
		if (SwordMesh == null)
		{
			SwordMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS063_SaberMesh", true).gameObject;
		}
		if (SwordFX == null)
		{
			SwordFX = OrangeBattleUtility.FindChildRecursive(ref childs, "SwordFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (SwordPos == null)
		{
			SwordPos = OrangeBattleUtility.FindChildRecursive(ref childs, "SwordPos", true);
		}
		if (LHandPos == null)
		{
			LHandPos = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 L Hand", true);
		}
		if (RHandPos == null)
		{
			RHandPos = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 R Hand", true);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimTransform = _enemyCollider[0].transform;
		base.AimPoint = new Vector3(0f, 0f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
		base.Start();
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

	private void UpdateDirection(int forceDirection = 0, bool back = false)
	{
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		switch (mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Debut:
		case MainStatus.Skill0:
		case MainStatus.Skill1:
		case MainStatus.Skill2:
		case MainStatus.Die:
			_mainStatus = mainStatus;
			_subStatus = subStatus;
			switch (_mainStatus)
			{
			case MainStatus.Idle:
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				_velocity.x = 0;
				IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
				break;
			case MainStatus.Skill0:
				if (_subStatus == SubStatus.Phase0)
				{
					_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
					_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
					_collideBullet.Active(targetMask);
					ActionFrame = GameLogicUpdateManager.GameFrame + (int)(WalkTime * 20f);
					_velocity.x = base.direction * WalkSpeed;
				}
				break;
			case MainStatus.Skill1:
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
					_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
					_collideBullet.Active(targetMask);
					_velocity = VInt3.zero;
					break;
				case SubStatus.Phase2:
					(BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, NowPos + Vector3.right * 3f * base.direction, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
					break;
				}
				break;
			case MainStatus.Skill2:
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					_velocity = VInt3.zero;
					_collideBullet.UpdateBulletData(EnemyWeapons[7].BulletData);
					_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
					_collideBullet.Active(targetMask);
					break;
				case SubStatus.Phase3:
					HasActed = false;
					ActionAnimatorFrame = 0.8f;
					break;
				case SubStatus.Phase5:
					_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
					_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
					_collideBullet.Active(targetMask);
					_velocity = VInt3.zero;
					break;
				}
				break;
			case MainStatus.Die:
				_mainStatus = mainStatus;
				_subStatus = subStatus;
				_FFmainStatus = mainStatus;
				_FFsubStatus = subStatus;
				switch (_subStatus)
				{
				case SubStatus.Phase0:
					base.AllowAutoAim = false;
					_velocity = VInt3.zero;
					nDeadCount = 0;
					if (!Controller.Collisions.below)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase2);
					}
					break;
				case SubStatus.Phase1:
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
					break;
				}
				break;
			}
			break;
		case MainStatus.FFIdle:
		case MainStatus.Skill3:
		case MainStatus.Skill4:
		case MainStatus.Skill5:
			_FFmainStatus = mainStatus;
			_FFsubStatus = subStatus;
			switch (_FFmainStatus)
			{
			case MainStatus.FFIdle:
				FFIsIdle = true;
				FFIdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(FFIdleWaitTime * 20f);
				break;
			case MainStatus.Skill3:
				switch (_FFsubStatus)
				{
				case SubStatus.Phase1:
					SpawnHand(false);
					SwitchMesh(false, 2);
					if ((bool)EMRHand && RHandCorp != null)
					{
						RHandCorp.SendMission(0);
					}
					break;
				case SubStatus.Phase3:
					SpawnHand();
					SwitchMesh(false, 1);
					if ((bool)EMLHand && LHandCorp != null)
					{
						LHandCorp.SendMission(0);
					}
					break;
				case SubStatus.Phase4:
					if ((bool)EMRHand && RHandCorp != null)
					{
						RHandCorp.SendMission(1);
					}
					if ((bool)EMLHand && LHandCorp != null)
					{
						LHandCorp.SendMission(1);
					}
					break;
				case SubStatus.Phase5:
					SwitchMesh(true);
					break;
				}
				break;
			case MainStatus.Skill4:
				switch (_FFsubStatus)
				{
				case SubStatus.Phase0:
					_mainStatus = MainStatus.Idle;
					SwitchMesh(true, 3);
					break;
				case SubStatus.Phase1:
					ActionAnimatorFrame = 0.17f;
					HasActed = false;
					break;
				case SubStatus.Phase2:
					SwitchMesh(false, 3);
					break;
				}
				break;
			case MainStatus.Skill5:
				switch (_FFsubStatus)
				{
				case SubStatus.Phase0:
					isLeft = false;
					ActionTimes = Skill5ShootTimes;
					break;
				case SubStatus.Phase1:
					isLeft = !isLeft;
					ActionAnimatorFrame = 0.5f;
					HasActed = false;
					break;
				}
				break;
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation(mainStatus);
	}

	private void UpdateAnimation(MainStatus mainstatus)
	{
		switch (mainstatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT_LOOP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			break;
		case MainStatus.Idle:
		case MainStatus.FFIdle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
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
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_START1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_START2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_END2;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_FFsubStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL3_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL3_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP2;
				break;
			case SubStatus.Phase4:
				return;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL3_START3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL3_END3;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_FFsubStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL4_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL4_END;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_FFsubStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL5_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL5_END;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			}
			break;
		}
		switch (mainstatus)
		{
		case MainStatus.Debut:
		case MainStatus.Skill2:
		case MainStatus.Die:
			_animator.SetLayerWeight(1, 0f);
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Idle:
		case MainStatus.Skill0:
		case MainStatus.Skill1:
			_animator.SetLayerWeight(1, 1f);
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.FFIdle:
		case MainStatus.Skill3:
		case MainStatus.Skill5:
			_animator.SetLayerWeight(1, 1f);
			_animator.Play(_animationHash[(int)_currentAnimationId], 1, 0f);
			break;
		case MainStatus.Skill4:
			_animator.SetLayerWeight(1, 0f);
			_animator.Play(_animationHash[(int)_currentAnimationId], 1, 0f);
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		}
	}

	private void UpdateRandomState(MainStatus status = MainStatus.Idle)
	{
		MainStatus mainStatus = status;
		if (status == MainStatus.Idle)
		{
			switch (_mainStatus)
			{
			case MainStatus.Debut:
				SetStatus(MainStatus.Idle);
				SetStatus(MainStatus.FFIdle);
				break;
			case MainStatus.Idle:
			{
				if (_FFmainStatus == MainStatus.Skill4)
				{
					return;
				}
				if (NeedSkill2)
				{
					WalkTimes = 0;
					TotalWalkTImes = 0;
					NeedSkill2 = false;
					mainStatus = MainStatus.Skill2;
					break;
				}
				float num = 0f;
				if ((bool)Target)
				{
					num = _transform.position.x - Target._transform.position.x;
				}
				if ((bool)Target && num < 2.5f && num > 0f && WalkTimes > 0)
				{
					WalkTimes = 0;
					mainStatus = MainStatus.Skill1;
				}
				else
				{
					WalkTimes++;
					TotalWalkTImes++;
					mainStatus = MainStatus.Skill0;
				}
				break;
			}
			}
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

	private void FFUpdateNextState()
	{
		if (NeedSkill2 || _mainStatus == MainStatus.Debut || _mainStatus == MainStatus.Skill2 || _mainStatus == MainStatus.Die || bWaitNetStatus)
		{
			return;
		}
		if (DebugMode)
		{
			FFUploadStatus(FFNextSkill);
			return;
		}
		if ((bool)Target && Target._transform.position.x - _transform.position.x > 1.5f && TotalWalkTImes > 0)
		{
			NeedSkill2 = true;
			AIStep = -1;
			return;
		}
		AIStep = (AIStep + 1) % AICircuit.Length;
		if (AICircuit[AIStep] == MainStatus.Skill4 && _mainStatus != 0)
		{
			AIStep--;
		}
		else
		{
			FFUploadStatus(AICircuit[AIStep]);
		}
	}

	private void FFUploadStatus(MainStatus status)
	{
		if (status != MainStatus.FFIdle)
		{
			if (CheckHost())
			{
				UploadEnemyStatus((int)status);
			}
		}
		else
		{
			SetStatus(MainStatus.FFIdle);
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
		_currentFFFrame = _animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
		switch (_FFmainStatus)
		{
		case MainStatus.FFIdle:
			if (!bWaitNetStatus && FFIdleWaitFrame < GameLogicUpdateManager.GameFrame)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					UpdateDirection();
					FFUpdateNextState();
				}
			}
			break;
		case MainStatus.Skill3:
			switch (_FFsubStatus)
			{
			case SubStatus.Phase0:
				if (_currentFFFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFFFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFFFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (CheckMissionOverAll())
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (CheckMissionOverAll())
				{
					if (LHandCorp != null)
					{
						LHandCorp.ComeBack();
						LHandCorp = null;
						EMLHand = null;
					}
					if (RHandCorp != null)
					{
						RHandCorp.ComeBack();
						RHandCorp = null;
						EMRHand = null;
					}
					SetStatus(MainStatus.Skill3, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFFFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFFFrame > 1f)
				{
					SetStatus(MainStatus.FFIdle);
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_FFsubStatus)
			{
			case SubStatus.Phase0:
				if (_currentFFFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (!HasActed && _currentFFFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, SwordPos.position, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).BackCallback = ShootSubBullet;
				}
				if (_currentFFFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFFFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
					SetStatus(MainStatus.FFIdle);
				}
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_FFsubStatus)
			{
			case SubStatus.Phase0:
				if (_currentFFFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (!HasActed && _currentFFFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					EndPos = GetTargetPos();
					if (isLeft)
					{
						Vector3 pDirection = EndPos - LHandPos.position;
						BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, LHandPos.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
					else
					{
						Vector3 pDirection2 = EndPos - RHandPos.position;
						BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, RHandPos.position, pDirection2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
				}
				if (_currentFFFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill5, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill5, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFFFrame > 1f)
				{
					SetStatus(MainStatus.FFIdle);
				}
				break;
			}
			break;
		}
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_introReady)
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
				if (IntroCallBack != null)
				{
					IntroCallBack();
					if (!bWaitNetStatus)
					{
						UpdateRandomState();
					}
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus && (IdleWaitFrame < GameLogicUpdateManager.GameFrame || NeedSkill2))
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					UpdateDirection();
					UpdateRandomState();
				}
				else
				{
					UpdateRandomState(MainStatus.Skill2);
				}
			}
			break;
		case MainStatus.Skill0:
			if (_subStatus == SubStatus.Phase0 && GameLogicUpdateManager.GameFrame > ActionFrame)
			{
				_velocity = VInt3.zero;
				SetStatus(MainStatus.Idle);
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
				if (FFIsIdle)
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
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					_velocity.x = -base.direction * BackSpeed;
					_velocity.y = 8000;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (Controller.Collisions.right)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
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
			case SubStatus.Phase0:
				if (_currentFrame > 0.4f)
				{
					if (nDeadCount > 10)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase1);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 3f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			}
			break;
		case MainStatus.FFIdle:
		case MainStatus.Skill3:
		case MainStatus.Skill4:
		case MainStatus.Skill5:
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
			WalkTimes = 0;
			TotalWalkTImes = 0;
			NeedSkill2 = false;
			AIStep = -1;
			_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_FFcollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_FFcollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_FFcollideBullet.Active(targetMask);
			SwitchMesh(false, 3);
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)_FFcollideBullet)
			{
				_FFcollideBullet.BackToPool();
			}
			if (LHandCorp != null)
			{
				LHandCorp.ComeBack();
				LHandCorp = null;
				EMLHand = null;
			}
			if (RHandCorp != null)
			{
				RHandCorp.ComeBack();
				RHandCorp = null;
				EMRHand = null;
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	private bool CheckMissionOverAll()
	{
		if (LHandCorp != null && !LHandCorp.CheckMissionProgress())
		{
			return false;
		}
		if (RHandCorp != null && !RHandCorp.CheckMissionProgress())
		{
			return false;
		}
		return true;
	}

	private void SpawnHand(bool lhand = true)
	{
		MOB_TABLE tMOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[2].BulletData.f_EFFECT_X];
		if (lhand)
		{
			EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(tMOB_TABLE, sNetSerialID + SpawnCount);
			SpawnCount++;
			if ((bool)enemyControllerBase)
			{
				EMLHand = enemyControllerBase.gameObject.GetComponent<EM181_Controller>();
				if ((bool)EMLHand)
				{
					Vector3 position = LHandModel.position;
					EMLHand.SetPositionAndRotation(position, base.direction == -1);
					EMLHand.gameObject.name = "Left_Hand";
					EMLHand.SetLRHand(true);
					LHandCorp = new BossCorpsTool(EMLHand);
					LHandCorp.Master = this;
					EMLHand.JoinCorps(LHandCorp);
					EMLHand.SetActive(true);
				}
			}
			return;
		}
		EnemyControllerBase enemyControllerBase2 = StageUpdate.StageSpawnEnemyByMob(tMOB_TABLE, sNetSerialID + SpawnCount);
		SpawnCount++;
		if ((bool)enemyControllerBase2)
		{
			EMRHand = enemyControllerBase2.gameObject.GetComponent<EM181_Controller>();
			if ((bool)EMRHand)
			{
				Vector3 position2 = RHandModel.position;
				EMRHand.SetPositionAndRotation(position2, base.direction == -1);
				EMRHand.gameObject.name = "Right_Hand";
				EMRHand.SetLRHand(false);
				RHandCorp = new BossCorpsTool(EMRHand);
				RHandCorp.Master = this;
				EMRHand.JoinCorps(RHandCorp);
				EMRHand.SetActive(true);
			}
		}
	}

	private Vector3 GetTargetPos()
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if ((bool)Target)
		{
			TargetPos = Target.Controller.LogicPosition;
			return TargetPos.vec3;
		}
		return _transform.position + Vector3.right * 3f * base.direction;
	}

	public void ReportObjects(object[] values)
	{
	}

	public object[] GetValues(object[] param)
	{
		int num = (int)param[1];
		if (num == EMLHand.gameObject.GetInstanceID())
		{
			return new object[2] { LHandModel, base.direction };
		}
		if (num == EMRHand.gameObject.GetInstanceID())
		{
			return new object[2] { RHandModel, base.direction };
		}
		return null;
	}

	private void SwitchMesh(bool onoff, int mesh = 0)
	{
		switch (mesh)
		{
		case 1:
			HandMeshL.SetActive(onoff);
			break;
		case 2:
			HandMeshR.SetActive(onoff);
			break;
		case 3:
			SwordFX.gameObject.SetActive(onoff);
			if (onoff)
			{
				PlaySE("BossSE04", "bs037_inary08");
				SwordFX.Play();
			}
			else
			{
				SwordFX.Stop();
			}
			SwordMesh.SetActive(onoff);
			break;
		default:
			HandMeshL.SetActive(onoff);
			HandMeshR.SetActive(onoff);
			break;
		}
	}

	private void ShootSubBullet(object obj)
	{
		BasicBullet basicBullet = null;
		if (obj != null)
		{
			basicBullet = obj as BasicBullet;
		}
		if (basicBullet == null)
		{
			Debug.LogError("子彈資料有誤。");
			return;
		}
		BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, basicBullet._transform.position + Vector3.right * 0.5f, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, basicBullet._transform.position + Vector3.right * 0.5f, Vector3.down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
	}
}
