#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS065_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Skill2 = 4,
		Skill3 = 5,
		Die = 6
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		MAX_SUBSTATUS = 6
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_WALK = 2,
		ANI_SKILL0_START1 = 3,
		ANI_SKILL0_LOOP1 = 4,
		ANI_SKILL0_START2 = 5,
		ANI_SKILL0_LOOP2 = 6,
		ANI_SKILL0_END2 = 7,
		ANI_SKILL1_START = 8,
		ANI_SKILL1_LOOP = 9,
		ANI_SKILL1_END = 10,
		ANI_SKILL2_START1 = 11,
		ANI_SKILL2_LOOP1 = 12,
		ANI_SKILL2_START2 = 13,
		ANI_SKILL2_LOOP2 = 14,
		ANI_SKILL2_END2 = 15,
		ANI_SKILL3_START = 16,
		ANI_SKILL3_END = 17,
		ANI_HURT = 18,
		ANI_DEAD = 19,
		MAX_ANIMATION_ID = 20
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

	private int[] DefaultSkillCard = new int[4] { 0, 1, 2, 3 };

	private static int[] DefaultRangedSkillCard = new int[3] { 3, 4, 5 };

	private List<int> RangedSKC = new List<int>();

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	[SerializeField]
	private Transform ShootPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private float GroundYPos;

	private int ActionTimes;

	private float ActionAnimatorFrame;

	private int ActionFrame;

	private bool HasActed;

	private int SpawnCount;

	[Header("身體撞擊")]
	[SerializeField]
	private float Skill0P1ActionFrame = 2f;

	[SerializeField]
	private float Skill0P2ActionFrame = 0.6f;

	[SerializeField]
	private int Skill0P2JumpSpd = 6000;

	[SerializeField]
	private int Skill0P2MoveSpd = 10000;

	[Header("冰椎")]
	[SerializeField]
	private int Skill1P1ShootTimes = 6;

	[SerializeField]
	private int Skill1P2ShootTimes = 12;

	[SerializeField]
	private float Skill1P1ShootFrame = 5f;

	[SerializeField]
	private float Skill1P2ShootTime = 0.2f;

	[SerializeField]
	private Transform Skill1ShootPosL;

	[SerializeField]
	private Transform Skill1ShootPosR;

	[SerializeField]
	private SkinnedMeshRenderer LShoulderMesh;

	[SerializeField]
	private SkinnedMeshRenderer RShoulderMesh;

	private bool Skill1CanShoot;

	private int Skill1ShootFrame;

	private int Skill1ActionTimes;

	[Header("大冰椎")]
	[SerializeField]
	private float Skill2ChargeAnimaFrame = 5f;

	[SerializeField]
	private float Skill2P1ShootFrame = 0.28f;

	[SerializeField]
	private ParticleSystem Skill2UseFx;

	private EnergyBullet Skill2Bullet;

	[Header("冰霜晶塔")]
	[SerializeField]
	private float Skill3ActionFrame = 0.7f;

	[Header("待機等待Frame")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	private bool bPlayLoopSE;

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
		_animationHash = new int[20];
		_animationHash[0] = Animator.StringToHash("BS065@idl_loop");
		_animationHash[1] = Animator.StringToHash("BS065@debut");
		_animationHash[2] = Animator.StringToHash("BS065@walk_loop");
		_animationHash[3] = Animator.StringToHash("BS065@Frost-Walrus_SKILL1_CASTING1");
		_animationHash[4] = Animator.StringToHash("BS065@Frost-Walrus_SKILL1_CASTLOOP1");
		_animationHash[5] = Animator.StringToHash("BS065@Frost-Walrus_SKILL1_CASTING2");
		_animationHash[6] = Animator.StringToHash("BS065@Frost-Walrus_SKILL1_CASTLOOP2");
		_animationHash[7] = Animator.StringToHash("BS065@Frost-Walrus_SKILL1_CASTOUT1");
		_animationHash[8] = Animator.StringToHash("BS065@Frost-Walrus_SKILL2_CASTING1");
		_animationHash[9] = Animator.StringToHash("BS065@Frost-Walrus_SKILL2_CASTLOOP1");
		_animationHash[10] = Animator.StringToHash("BS065@Frost-Walrus_SKILL2_CASTOUT1");
		_animationHash[11] = Animator.StringToHash("BS065@Frost-Walrus_SKILL3_CASTING1");
		_animationHash[12] = Animator.StringToHash("BS065@Frost-Walrus_SKILL3_CASTLOOP1");
		_animationHash[13] = Animator.StringToHash("BS065@Frost-Walrus_SKILL3_CASTING2");
		_animationHash[14] = Animator.StringToHash("BS065@Frost-Walrus_SKILL3_CASTLOOP2");
		_animationHash[15] = Animator.StringToHash("BS065@Frost-Walrus_SKILL3_CASTOUT1");
		_animationHash[16] = Animator.StringToHash("BS065@Frost-Walrus_SKILL4_CASTING1");
		_animationHash[17] = Animator.StringToHash("BS065@Frost-Walrus_SKILL4_CASTOUT1");
		_animationHash[18] = Animator.StringToHash("BS065@hurt");
		_animationHash[19] = Animator.StringToHash("BS065@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (Skill1ShootPosL == null)
		{
			Skill1ShootPosL = OrangeBattleUtility.FindChildRecursive(ref childs, "Bone005", true);
		}
		if (Skill1ShootPosR == null)
		{
			Skill1ShootPosR = OrangeBattleUtility.FindChildRecursive(ref childs, "Bone004", true);
		}
		if (Skill2UseFx == null)
		{
			Skill2UseFx = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill2UseFx", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (LShoulderMesh == null)
		{
			LShoulderMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS065_ShoulderMesh_L", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (RShoulderMesh == null)
		{
			RShoulderMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS065_ShoulderMesh_R", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimPoint = new Vector3(0f, 0.6f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		FallDownSE = new string[2] { "BossSE05", "bs045_frost02" };
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
		if (back)
		{
			base.direction = -base.direction;
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
				PlaySE("BossSE05", "bs045_frost01", true);
				break;
			}
			break;
		case MainStatus.Idle:
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				EndPos = GetTargetPos();
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				ActionAnimatorFrame = Skill0P1ActionFrame;
				break;
			case SubStatus.Phase2:
				PlayBossSE("BossSE05", "bs045_frost05_lg");
				bPlayLoopSE = true;
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				HasActed = false;
				ActionAnimatorFrame = Skill0P2ActionFrame;
				break;
			case SubStatus.Phase4:
				PlayBossSE("BossSE05", "bs045_frost05_stop");
				bPlayLoopSE = false;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ActionTimes = Skill1P1ShootTimes;
				break;
			case SubStatus.Phase1:
				HasActed = false;
				ActionAnimatorFrame = Skill1P1ShootFrame;
				break;
			case SubStatus.Phase2:
				LShoulderMesh.enabled = true;
				RShoulderMesh.enabled = true;
				Skill1ActionTimes = Skill1P2ShootTimes;
				Skill1ShootFrame = GameLogicUpdateManager.GameFrame + (int)(Skill1P2ShootTime * 20f);
				Skill1CanShoot = true;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE05", "bs045_frost07");
				break;
			case SubStatus.Phase1:
				PlayBossSE("BossSE05", "bs045_frost08_lg");
				bPlayLoopSE = true;
				Skill2UseFx.Play();
				Skill2Bullet = BulletBase.TryShotBullet<EnergyBullet>(EnemyWeapons[4].BulletData, ShootPos.position, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				if ((bool)Skill2Bullet)
				{
					base.SoundSource.ForcePlaySE("BossSE05", "bs045_frost10", 0.8f);
					Skill2Bullet.BackCallback = Skill2BulletBack;
				}
				break;
			case SubStatus.Phase2:
				PlayBossSE("BossSE05", "bs045_frost08_stop");
				bPlayLoopSE = false;
				Skill2UseFx.Stop();
				break;
			case SubStatus.Phase4:
				HasActed = false;
				ActionAnimatorFrame = Skill2P1ShootFrame;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				PlayBossSE("BossSE05", "bs045_frost08_lg");
				bPlayLoopSE = true;
				Skill2UseFx.Play();
				Skill2Bullet = BulletBase.TryShotBullet<EnergyBullet>(EnemyWeapons[4].BulletData, ShootPos.position, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				if ((bool)Skill2Bullet)
				{
					base.SoundSource.ForcePlaySE("BossSE05", "bs045_frost10", 0.8f);
					Skill2Bullet.BackCallback = Skill2BulletBack;
				}
				break;
			case SubStatus.Phase2:
				PlayBossSE("BossSE05", "bs045_frost08_stop");
				bPlayLoopSE = false;
				Skill2UseFx.Stop();
				HasActed = false;
				ActionAnimatorFrame = Skill3ActionFrame;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Skill2UseFx.isPlaying || bPlayLoopSE)
				{
					bPlayLoopSE = false;
					PlayBossSE("BossSE05", "bs045_frost08_stop");
					Skill2UseFx.Stop();
				}
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				nDeadCount = 0;
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				StartCoroutine(BossDieFlow(_transform.position));
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
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
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			break;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
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
				_currentAnimationId = AnimationID.ANI_SKILL0_END2;
				break;
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
				_currentAnimationId = AnimationID.ANI_SKILL2_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_END2;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL2_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL3_START;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL3_END;
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
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
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
				break;
			case MainStatus.Idle:
				mainStatus = (MainStatus)RandomCard(2);
				break;
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
				if (IntroCallBack != null)
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
			if (!bWaitNetStatus && IdleWaitFrame < GameLogicUpdateManager.GameFrame)
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
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > ActionAnimatorFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					_velocity.x = Skill0P2MoveSpd * base.direction;
					_velocity.y = Skill0P2JumpSpd;
				}
				if (HasActed && Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.left || Controller.Collisions.right)
				{
					PlayBossSE("BossSE05", "bs045_frost06");
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
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
				if (_currentFrame > ActionAnimatorFrame)
				{
					if (LShoulderMesh.enabled)
					{
						LShoulderMesh.enabled = false;
					}
					if (RShoulderMesh.enabled)
					{
						RShoulderMesh.enabled = false;
					}
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, Skill1ShootPosL.position, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, Skill1ShootPosR.position, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill1, _subStatus);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					}
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
				if (_currentFrame > Skill2ChargeAnimaFrame)
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					PlayBossSE("BossSE05", "bs045_frost11");
					HasActed = true;
					if ((bool)Skill2Bullet)
					{
						Skill2Bullet.StartShoot();
						Skill2Bullet = null;
					}
				}
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
				if (_currentFrame > Skill2ChargeAnimaFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					if ((bool)Skill2Bullet)
					{
						PlayBossSE("BossSE05", "bs045_frost11");
						Skill2Bullet.GoBack();
						Skill2Bullet = null;
					}
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
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
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 4f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			}
			break;
		}
		if (!Skill1CanShoot || GameLogicUpdateManager.GameFrame <= Skill1ShootFrame)
		{
			return;
		}
		for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
		{
			Vector3 position = StageUpdate.runPlayers[i]._transform.position;
			position.y = _transform.position.y + 15f;
			BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, position, Vector3.down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
			Skill1ShootFrame = GameLogicUpdateManager.GameFrame + (int)(Skill1P2ShootTime * 20f);
			Skill1ActionTimes--;
			if (Skill1ActionTimes <= 0)
			{
				Skill1CanShoot = false;
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
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
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
			Skill1CanShoot = false;
			if ((bool)Skill2Bullet)
			{
				Skill2Bullet.BackCallback = null;
				Skill2Bullet.GoBack();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	private Vector3 GetTargetPos(bool realcenter = false)
	{
		if (!Target)
		{
			Target = _enemyAutoAimSystem.GetClosetPlayer();
		}
		if ((bool)Target)
		{
			if (realcenter)
			{
				TargetPos = new VInt3(Target.Controller.GetRealCenterPos());
			}
			else
			{
				TargetPos = new VInt3(Target.GetTargetPoint() + Vector3.up * 0.15f);
			}
			return TargetPos.vec3;
		}
		return NowPos + Vector3.right * 3f * base.direction;
	}

	private void Skill2BulletBack(object obj)
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
		for (int i = 0; i < 8; i++)
		{
			Vector3 pDirection = Quaternion.Euler(0f, 0f, i * 45) * Vector3.up;
			Vector3 worldPos = basicBullet._transform.position + Vector3.down * 0.5f;
			BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, worldPos, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).BackCallback = SpawnIcethorn;
		}
	}

	private void SpawnIcethorn(object obj)
	{
		BasicBullet basicBullet = null;
		if (obj != null)
		{
			basicBullet = obj as BasicBullet;
		}
		if (basicBullet == null)
		{
			Debug.LogError("子彈資料有誤。");
		}
		else
		{
			if (!basicBullet.isHitBlock)
			{
				return;
			}
			int num = 6;
			Vector3 position = basicBullet._transform.position;
			MOB_TABLE enemy = GetEnemy((int)EnemyWeapons[num].BulletData.f_EFFECT_X);
			if (enemy == null)
			{
				Debug.LogError("要生成的怪物資料有誤，生怪技能ID " + num + " 怪物GroupID " + EnemyWeapons[num].BulletData.f_EFFECT_X);
				return;
			}
			EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(enemy, sNetSerialID + SpawnCount);
			SpawnCount++;
			if ((bool)enemyControllerBase)
			{
				enemyControllerBase.SetPositionAndRotation(position, base.direction == 1);
				enemyControllerBase.SetActive(true);
			}
		}
	}

	private MOB_TABLE GetEnemy(int nGroupID)
	{
		MOB_TABLE[] mobArrayFromGroup = ManagedSingleton<OrangeTableHelper>.Instance.GetMobArrayFromGroup(nGroupID);
		for (int i = 0; i < mobArrayFromGroup.Length; i++)
		{
			if (mobArrayFromGroup[i].n_DIFFICULTY == StageUpdate.gDifficulty)
			{
				return mobArrayFromGroup[i];
			}
		}
		return null;
	}
}
