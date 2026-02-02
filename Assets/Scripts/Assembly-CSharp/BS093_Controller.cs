#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS093_Controller : EnemyControllerBase, IManagedUpdateBehavior, IF_Master
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
		Skill5 = 7,
		Skill6 = 8,
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
		Phase7 = 7,
		Phase8 = 8,
		Phase9 = 9,
		Phase10 = 10,
		MAX_SUBSTATUS = 11
	}

	public enum AnimationID
	{
		ANI_IDLE1 = 0,
		ANI_IDLE2 = 1,
		ANI_DEBUT = 2,
		ANI_Skill0_START1 = 3,
		ANI_Skill0_LOOP1 = 4,
		ANI_Skill0_START2 = 5,
		ANI_Skill0_LOOP2 = 6,
		ANI_Skill0_END = 7,
		ANI_Skill1_START = 8,
		ANI_Skill1_LOOP = 9,
		ANI_Skill1_END = 10,
		ANI_Skill2_START1 = 11,
		ANI_Skill2_LOOP1 = 12,
		ANI_Skill2_START2 = 13,
		ANI_Skill2_END = 14,
		ANI_Skill3_START1 = 15,
		ANI_Skill3_LOOP1 = 16,
		ANI_Skill3_START2 = 17,
		ANI_Skill3_LOOP2 = 18,
		ANI_Skill3_END = 19,
		ANI_Skill4_START1 = 20,
		ANI_Skill4_LOOP1 = 21,
		ANI_Skill4_START2 = 22,
		ANI_Skill4_LOOP2 = 23,
		ANI_Skill4_START3 = 24,
		ANI_Skill4_END = 25,
		ANI_HURT = 26,
		ANI_DEAD = 27,
		ANI_DEAD_LOOP = 28,
		MAX_ANIMATION_ID = 29
	}

	private enum HandAIStatus
	{
		LAtk = 0,
		RAtk = 1,
		LCatch = 2,
		RCatch = 3
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

	private int[] _animationHash;

	private int[] SkillCard = new int[3] { 30, 30, 30 };

	private bool[] HasCombo = new bool[3];

	[SerializeField]
	private bool NeedCombo;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private float ShootFrame;

	private bool HasShot;

	private Vector3 ShotPos;

	private Vector3 EndPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private Vector3 CenterPos;

	private bool CanKeepOn = true;

	private int AtkWaitFrame;

	[SerializeField]
	private SkinnedMeshRenderer LHandMesh;

	[SerializeField]
	private SkinnedMeshRenderer RHandMesh;

	[SerializeField]
	private Transform LHandModel;

	[SerializeField]
	private Transform RHandModel;

	[SerializeField]
	private ParticleSystem LBoosterFX;

	[SerializeField]
	private ParticleSystem RBoosterFX;

	[Header("手參數")]
	private bool haveSpawn;

	private EM159_Controller EMLHand;

	private EM159_Controller EMRHand;

	private BossCorpsTool LHandCorp;

	private BossCorpsTool RHandCorp;

	[SerializeField]
	[Tooltip("攻擊前等待秒數")]
	private float PunchWaitTime = 2f;

	[SerializeField]
	[Tooltip("抓取前等待秒數")]
	private float CatchWaitTime = 1f;

	private int HandAICase;

	private int HandAIStep;

	private int SpawnCount;

	[Header("待機參數")]
	[SerializeField]
	private float IdleWaitTime = 1f;

	private int IdleWaitFrame;

	[SerializeField]
	private int FloatSpeed = 1600;

	[SerializeField]
	private int Acceleration = 200;

	private int UpDown = 1;

	[Header("身體衝撞參數")]
	[SerializeField]
	private int RamSpeed = 6000;

	[SerializeField]
	private int PressSpeed = -12000;

	[Header("鎖定雷射參數")]
	[SerializeField]
	[Tooltip("持續N輪")]
	private int DefaultShootRound = 2;

	[SerializeField]
	[Tooltip("一輪N發")]
	private int DefaultShootTime = 4;

	[SerializeField]
	private bool UpStart = true;

	private int ShootRound;

	private int ShootTime;

	private bool leftSide;

	[Header("錘擊參數")]
	private OrangeCharacter targetOC;

	private bool isCatching;

	private Transform CatchPos;

	[SerializeField]
	[Tooltip("搥擊力道")]
	private int FistPower = 9000;

	private CollideBullet FistCollide;

	private CollideBullet HitWallCollide;

	[Header("組合攻擊參數")]
	private float ShotAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[SerializeField]
	private float ShootHandTime = 0.25f;

	[SerializeField]
	private int RHandStopAnimatorFrame = 5;

	[SerializeField]
	private int LHandStopAnimatorFrame = 26;

	[SerializeField]
	private int[] LineAngleUp = new int[2] { 15, 80 };

	[SerializeField]
	private int[] LineAngleDown = new int[2] { 85, 20 };

	private int[] UseAngle;

	[SerializeField]
	private GameObject Skill4_LHandUseFx1;

	[SerializeField]
	private GameObject Skill4_RHandUseFx1;

	[SerializeField]
	private ParticleSystem Skill4_LHandUseFx2;

	[SerializeField]
	private ParticleSystem Skill4_RHandUseFx2;

	private int StopFrame;

	private CollideBullet LSkill4Collide;

	private CollideBullet RSkill4Collide;

	[SerializeField]
	private ParticleSystem Skill6_LHandUseFx;

	[SerializeField]
	private ParticleSystem Skill6_RHandUseFx;

	private int nDeadCount;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	[Tooltip("上左右")]
	private bool bPlayUpLR;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	[Tooltip("下")]
	private bool bPlayDown;

	private bool bPlaySE13;

	private bool bPlaySE14;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private VInt3 _velocityOld;

	private MainStatus[] AICircuit2 = new MainStatus[2]
	{
		MainStatus.Idle,
		MainStatus.Skill5
	};

	private HandAIStatus[,] HandAICircuit = new HandAIStatus[2, 4]
	{
		{
			HandAIStatus.RAtk,
			HandAIStatus.LAtk,
			HandAIStatus.RAtk,
			HandAIStatus.LAtk
		},
		{
			HandAIStatus.RAtk,
			HandAIStatus.LCatch,
			HandAIStatus.RAtk,
			HandAIStatus.LCatch
		}
	};

	private void PlayUpLR_SE(bool onoff)
	{
		if (bPlayUpLR)
		{
			if (!onoff)
			{
				PlaySE("BossSE04", "bs031_general03_stop");
				bPlayUpLR = false;
			}
		}
		else if (onoff)
		{
			PlaySE("BossSE04", "bs031_general03_lp");
			bPlayUpLR = true;
		}
	}

	private void PlayDown_SE(bool onoff)
	{
		if (bPlayDown)
		{
			if (!onoff)
			{
				PlaySE("BossSE04", "bs031_general02_stop");
				bPlayDown = false;
			}
		}
		else if (onoff)
		{
			PlaySE("BossSE04", "bs031_general02_lp");
			bPlayDown = true;
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
			case "Skill4":
				NextSkill = MainStatus.Skill4;
				break;
			case "Skill5":
				NextSkill = MainStatus.Skill5;
				break;
			case "Skill6":
				NextSkill = MainStatus.Skill6;
				break;
			case "Skill8":
				HandAICase = 0;
				HandAIStep = 0;
				HandAICircuit = new HandAIStatus[2, 4]
				{
					{
						HandAIStatus.RAtk,
						HandAIStatus.LAtk,
						HandAIStatus.RAtk,
						HandAIStatus.LAtk
					},
					{
						HandAIStatus.RAtk,
						HandAIStatus.LCatch,
						HandAIStatus.RAtk,
						HandAIStatus.LCatch
					}
				};
				break;
			case "Skill9":
				HandAICircuit = new HandAIStatus[2, 4]
				{
					{
						HandAIStatus.LCatch,
						HandAIStatus.LCatch,
						HandAIStatus.LCatch,
						HandAIStatus.LCatch
					},
					{
						HandAIStatus.LCatch,
						HandAIStatus.LCatch,
						HandAIStatus.LCatch,
						HandAIStatus.LCatch
					}
				};
				break;
			}
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[29];
		_animationHash[0] = Animator.StringToHash("BS093@idle_stand_loop");
		_animationHash[1] = Animator.StringToHash("BS093@idle_air_loop");
		_animationHash[2] = Animator.StringToHash("BS093@debut");
		_animationHash[3] = Animator.StringToHash("BS093@skill_01_step1_start");
		_animationHash[4] = Animator.StringToHash("BS093@skill_01_step1_loop");
		_animationHash[5] = Animator.StringToHash("BS093@skill_01_step2_start");
		_animationHash[6] = Animator.StringToHash("BS093@skill_01_step2_loop");
		_animationHash[7] = Animator.StringToHash("BS093@skill_01_step2_end");
		_animationHash[8] = Animator.StringToHash("BS093@skill_02_start");
		_animationHash[9] = Animator.StringToHash("BS093@skill_02_loop");
		_animationHash[10] = Animator.StringToHash("BS093@skill_02_end");
		_animationHash[11] = Animator.StringToHash("BS093@skill_03_step1_start");
		_animationHash[12] = Animator.StringToHash("BS093@skill_03_step1_loop");
		_animationHash[13] = Animator.StringToHash("BS093@skill_03_step2_start");
		_animationHash[14] = Animator.StringToHash("BS093@skill_03_step2_end");
		_animationHash[15] = Animator.StringToHash("BS093@skill_05_step1_start");
		_animationHash[16] = Animator.StringToHash("BS093@skill_05_step1_loop");
		_animationHash[17] = Animator.StringToHash("BS093@skill_05_step2_start");
		_animationHash[18] = Animator.StringToHash("BS093@skill_05_step2_loop");
		_animationHash[19] = Animator.StringToHash("BS093@skill_05_step2_end");
		_animationHash[20] = Animator.StringToHash("BS093@skill_06_step1_start");
		_animationHash[21] = Animator.StringToHash("BS093@skill_06_step1_loop");
		_animationHash[22] = Animator.StringToHash("BS093@skill_06_step2_start");
		_animationHash[23] = Animator.StringToHash("BS093@skill_06_step2_loop");
		_animationHash[24] = Animator.StringToHash("BS093@skill_06_step3_start");
		_animationHash[25] = Animator.StringToHash("BS093@skill_06_step3_end");
		_animationHash[26] = Animator.StringToHash("BS093@hurt_loop");
		_animationHash[27] = Animator.StringToHash("BS093@dead");
		_animationHash[28] = Animator.StringToHash("BS093@dead_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "EnemyCollider", true);
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (LHandMesh == null)
		{
			LHandMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS093_HandMesh_L", true).GetComponent<SkinnedMeshRenderer>();
		}
		if (RHandMesh == null)
		{
			RHandMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS093_HandMesh_R", true).GetComponent<SkinnedMeshRenderer>();
		}
		if (LHandModel == null)
		{
			LHandModel = OrangeBattleUtility.FindChildRecursive(ref childs, "HandPos_L", true);
		}
		if (RHandModel == null)
		{
			RHandModel = OrangeBattleUtility.FindChildRecursive(ref childs, "HandPos_R", true);
		}
		if (LBoosterFX == null)
		{
			LBoosterFX = OrangeBattleUtility.FindChildRecursive(ref childs, "LBoosterFX", true).GetComponent<ParticleSystem>();
		}
		if (RBoosterFX == null)
		{
			RBoosterFX = OrangeBattleUtility.FindChildRecursive(ref childs, "RBoosterFX", true).GetComponent<ParticleSystem>();
		}
		if (Skill4_LHandUseFx1 == null)
		{
			Skill4_LHandUseFx1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4_LHandUseFx1", true).gameObject;
		}
		if (Skill4_RHandUseFx1 == null)
		{
			Skill4_RHandUseFx1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4_RHandUseFx1", true).gameObject;
		}
		if (Skill4_LHandUseFx2 == null)
		{
			Skill4_LHandUseFx2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4_LHandUseFx2", true).GetComponent<ParticleSystem>();
		}
		if (Skill4_RHandUseFx2 == null)
		{
			Skill4_RHandUseFx2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4_RHandUseFx2", true).GetComponent<ParticleSystem>();
		}
		if (Skill6_LHandUseFx == null)
		{
			Skill6_LHandUseFx = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill6_LHandUseFx", true).GetComponent<ParticleSystem>();
		}
		if (Skill6_RHandUseFx == null)
		{
			Skill6_RHandUseFx = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill6_RHandUseFx", true).GetComponent<ParticleSystem>();
		}
		if (FistCollide == null)
		{
			FistCollide = OrangeBattleUtility.FindChildRecursive(ref childs, "FistCollide", true).gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (HitWallCollide == null)
		{
			HitWallCollide = OrangeBattleUtility.FindChildRecursive(ref childs, "HitWallCollide", true).gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (LSkill4Collide == null)
		{
			LSkill4Collide = OrangeBattleUtility.FindChildRecursive(ref childs, "LSkill4Collide", true).gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (RSkill4Collide == null)
		{
			RSkill4Collide = OrangeBattleUtility.FindChildRecursive(ref childs, "RSkill4Collide", true).gameObject.AddOrGetComponent<CollideBullet>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		HashAnimation();
		LoadParts(ref childs);
		base.AimPoint = new Vector3(0f, 0f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(30f);
		AiTimer.TimerStart();
		_velocityOld = VInt3.zero;
	}

	protected override void Start()
	{
		base.Start();
		if ((bool)_characterMaterial)
		{
			_characterMaterial.HurtColor = new Color(0.43f, 0.43f, 0.43f, 0.65f);
		}
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Math.Abs(ModelTransform.localScale.z) * (float)base.direction);
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
				CheckRoomSize();
				break;
			case SubStatus.Phase1:
				_velocity.y = FloatSpeed;
				PlayUpLR_SE(true);
				SwitchBoosterFX(true);
				break;
			}
			break;
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayUpLR_SE(false);
				PlayDown_SE(false);
				StopMove();
				_velocity.y = FloatSpeed * UpDown;
				IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
				if ((float)(int)Hp < (float)(int)MaxHp * 0.75f && !HasCombo[0])
				{
					HasCombo[0] = true;
					NeedCombo = true;
				}
				if ((float)(int)Hp < (float)(int)MaxHp * 0.5f && !HasCombo[1])
				{
					HasCombo[1] = true;
					NeedCombo = true;
				}
				if ((float)(int)Hp < (float)(int)MaxHp * 0.25f && !HasCombo[2])
				{
					HasCombo[2] = true;
					NeedCombo = true;
				}
				break;
			case SubStatus.Phase1:
				UpDown *= -1;
				break;
			case SubStatus.Phase3:
				_velocity.y = FloatSpeed;
				if (!haveSpawn)
				{
					SwitchBoosterFX(true);
				}
				break;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateDirection();
				StopMove();
				break;
			case SubStatus.Phase1:
				_velocity.y = FloatSpeed;
				PlayUpLR_SE(true);
				break;
			case SubStatus.Phase2:
				StopMove();
				_velocity.x = RamSpeed * base.direction;
				break;
			case SubStatus.Phase3:
				StopMove();
				PlayUpLR_SE(false);
				PlayDown_SE(true);
				HasShot = false;
				ShootFrame = 0.4f;
				break;
			case SubStatus.Phase4:
				PlayDown_SE(false);
				StopMove();
				break;
			case SubStatus.Phase5:
				_velocity.y = -PressSpeed / 3;
				PlayUpLR_SE(true);
				break;
			case SubStatus.Phase7:
				StopMove();
				if (Mathf.Abs(_transform.position.x - MinPos.x) < Mathf.Abs(_transform.position.x - MaxPos.x))
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				_velocity.x = RamSpeed * base.direction;
				break;
			case SubStatus.Phase8:
				StopMove();
				_velocity.y = -FloatSpeed;
				PlayDown_SE(true);
				PlayUpLR_SE(false);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateDirection();
				StopMove();
				ShootRound = DefaultShootRound;
				ShootTime = DefaultShootTime;
				break;
			case SubStatus.Phase1:
				ShootFrame = 0.8f;
				HasShot = false;
				break;
			case SubStatus.Phase2:
				PlayUpLR_SE(false);
				PlayDown_SE(false);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				StopMove();
				UpdateDirection();
				break;
			case SubStatus.Phase2:
				ShootFrame = 0.5f;
				HasShot = false;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_collideBullet.BackToPool();
				StopMove();
				if (Skill6_LHandUseFx != null)
				{
					Skill6_LHandUseFx.Play();
				}
				if (Skill6_RHandUseFx != null)
				{
					Skill6_RHandUseFx.Play();
				}
				SendMission();
				_animator.speed = 0f;
				base.SoundSource.PlaySE("BossSE04", "bs031_general14_lp");
				break;
			case SubStatus.Phase1:
				HandComeBack();
				ShowHand(true);
				haveSpawn = false;
				SwitchBoosterFX(true);
				if (Skill6_LHandUseFx != null)
				{
					Skill6_LHandUseFx.Stop();
				}
				if (Skill6_RHandUseFx != null)
				{
					Skill6_RHandUseFx.Stop();
				}
				_animator.speed = 1f;
				base.SoundSource.PlaySE("BossSE04", "bs031_general14_stop");
				break;
			case SubStatus.Phase2:
				if ((_transform.position.x - MinPos.x < 4f && base.direction == -1) || (MaxPos.x - _transform.position.x < 4f && base.direction == 1))
				{
					UpdateDirection(-base.direction);
				}
				break;
			case SubStatus.Phase5:
				HasShot = false;
				ShootFrame = 0.13f;
				break;
			case SubStatus.Phase6:
				PlayUpLR_SE(true);
				SwitchCollide(FistCollide, false);
				SwitchCollide(_collideBullet);
				StopMove();
				if (Mathf.Abs(_transform.position.x - MinPos.x) < Mathf.Abs(_transform.position.x - MaxPos.x))
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				_velocity.x = RamSpeed * base.direction;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				StopMove();
				_velocity.y = FloatSpeed * 3;
				PlayUpLR_SE(true);
				leftSide = true;
				ShootTime = 2;
				if (UpStart)
				{
					UseAngle = LineAngleUp;
				}
				else
				{
					UseAngle = LineAngleDown;
				}
				break;
			case SubStatus.Phase1:
				PlayUpLR_SE(false);
				StopMove();
				HasShot = false;
				AtkWaitFrame = GameLogicUpdateManager.GameFrame + (int)(ShootHandTime * 20f);
				break;
			case SubStatus.Phase2:
				PlayDown_SE(true);
				break;
			case SubStatus.Phase3:
				_velocity.y = PressSpeed;
				break;
			case SubStatus.Phase4:
				PlayDown_SE(false);
				break;
			case SubStatus.Phase5:
				_animator.SetFloat(_HashAngle, UseAngle[0]);
				break;
			case SubStatus.Phase6:
				if ((bool)Skill4_LHandUseFx2 && (bool)Skill4_RHandUseFx2)
				{
					bPlaySE13 = true;
					PlaySE("BossSE04", "bs031_general13_lp");
					Skill4_LHandUseFx2.Play();
					Skill4_RHandUseFx2.Play();
				}
				SwitchCollide(LSkill4Collide);
				SwitchCollide(RSkill4Collide);
				break;
			case SubStatus.Phase7:
				if ((bool)Skill4_LHandUseFx1 && (bool)Skill4_RHandUseFx1)
				{
					Skill4_LHandUseFx1.SetActive(true);
					Skill4_RHandUseFx1.SetActive(true);
				}
				ShotAngle = UseAngle[0];
				_animator.SetFloat(_HashAngle, ShotAngle);
				break;
			case SubStatus.Phase8:
				bPlaySE13 = false;
				PlaySE("BossSE04", "bs031_general13_stop");
				SwitchCollide(LSkill4Collide, false);
				SwitchCollide(RSkill4Collide, false);
				StopFrame = GameLogicUpdateManager.GameFrame + RHandStopAnimatorFrame;
				SendMission();
				break;
			case SubStatus.Phase9:
				StopFrame = GameLogicUpdateManager.GameFrame + LHandStopAnimatorFrame;
				SendMission();
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				StopMove();
				break;
			case SubStatus.Phase1:
				ShootFrame = 0.1f;
				HasShot = false;
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				StopMove();
				SendMission();
				break;
			case SubStatus.Phase1:
				if (Skill6_LHandUseFx != null)
				{
					Skill6_LHandUseFx.Play();
				}
				if (Skill6_RHandUseFx != null)
				{
					Skill6_RHandUseFx.Play();
				}
				SendMission();
				bPlaySE14 = true;
				base.SoundSource.PlaySE("BossSE04", "bs031_general14_lp");
				break;
			case SubStatus.Phase2:
				HandComeBack();
				ShowHand(true);
				SwitchBoosterFX(true);
				if (Skill6_LHandUseFx != null)
				{
					Skill6_LHandUseFx.Stop();
				}
				if (Skill6_RHandUseFx != null)
				{
					Skill6_RHandUseFx.Stop();
				}
				haveSpawn = false;
				bPlaySE14 = false;
				base.SoundSource.PlaySE("BossSE04", "bs031_general14_stop");
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				base.AllowAutoAim = false;
				StopMove();
				SendMission();
				OrangeBattleUtility.LockPlayer();
				STAGE_TABLE value;
				ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value);
				bool flag = false;
				if (value != null)
				{
					Dictionary<int, StageInfo>.Enumerator enumerator = ManagedSingleton<PlayerNetManager>.Instance.dicStage.GetEnumerator();
					while (enumerator.MoveNext())
					{
						STAGE_TABLE value2;
						if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(enumerator.Current.Value.netStageInfo.StageID, out value2) && value2.n_MAIN == value.n_MAIN)
						{
							flag = true;
						}
					}
				}
				if (!flag)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase3);
				}
				else if (_transform.position.y - MinPos.y < 0.1f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				else
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			}
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				StartCoroutine(BossDieFlow(GetTargetPoint()));
				break;
			case SubStatus.Phase3:
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				break;
			case SubStatus.Phase4:
				UpdateDirection(-1);
				HandComeBack();
				haveSpawn = false;
				ShowHand(true);
				nDeadCount = 0;
				break;
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
		_velocityOld = _velocity;
	}

	private void SendMission()
	{
		CanKeepOn = false;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Skill0:
		case MainStatus.Skill1:
		case MainStatus.Skill2:
			switch (HandAICircuit[HandAICase, HandAIStep])
			{
			case HandAIStatus.LAtk:
				if (LHandCorp != null)
				{
					LHandCorp.SendMission(0);
				}
				break;
			case HandAIStatus.RAtk:
				if (RHandCorp != null)
				{
					RHandCorp.SendMission(0);
				}
				break;
			case HandAIStatus.LCatch:
				if (LHandCorp != null)
				{
					LHandCorp.SendMission(1);
				}
				break;
			case HandAIStatus.RCatch:
				if (RHandCorp != null)
				{
					RHandCorp.SendMission(1);
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			if (LHandCorp != null)
			{
				LHandCorp.StopMission();
				LHandCorp.SendMission(3);
			}
			if (RHandCorp != null)
			{
				RHandCorp.StopMission();
				RHandCorp.SendMission(3);
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (LHandCorp != null)
				{
					LHandCorp.StopMission();
				}
				if (RHandCorp != null)
				{
					RHandCorp.StopMission();
				}
				break;
			case SubStatus.Phase1:
				if (leftSide)
				{
					if (LHandCorp != null)
					{
						LHandCorp.SendMission(4);
					}
				}
				else if (RHandCorp != null)
				{
					RHandCorp.SendMission(4);
				}
				break;
			case SubStatus.Phase8:
				if (RHandCorp != null)
				{
					RHandCorp.SendMission(3, true);
				}
				break;
			case SubStatus.Phase9:
				if (LHandCorp != null)
				{
					LHandCorp.SendMission(3, true);
				}
				break;
			}
			leftSide = !leftSide;
			break;
		case MainStatus.Skill5:
			if (LHandCorp != null)
			{
				LHandCorp.StopMission();
				LHandCorp.SendMission(2, false, true);
			}
			if (RHandCorp != null)
			{
				RHandCorp.StopMission();
				RHandCorp.SendMission(2, false, true);
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (LHandCorp != null)
				{
					LHandCorp.StopMission();
				}
				if (RHandCorp != null)
				{
					RHandCorp.StopMission();
				}
				break;
			case SubStatus.Phase1:
				if (LHandCorp != null)
				{
					LHandCorp.SendMission(3, true);
				}
				if (RHandCorp != null)
				{
					RHandCorp.SendMission(3, true);
				}
				break;
			}
			break;
		case MainStatus.Die:
			if (LHandCorp != null)
			{
				LHandCorp.StopMission();
			}
			if (RHandCorp != null)
			{
				RHandCorp.StopMission();
			}
			if (EMLHand != null)
			{
				EMLHand.SetDead();
			}
			if (EMRHand != null)
			{
				EMRHand.SetDead();
			}
			break;
		case MainStatus.Debut:
			break;
		}
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		default:
			return;
		case MainStatus.Debut:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_IDLE2;
				break;
			}
			break;
		case MainStatus.Idle:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE2;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_IDLE2;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_IDLE2;
				break;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill0_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill0_LOOP1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill0_START2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill0_LOOP2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_Skill0_END;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_IDLE2;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_IDLE2;
				break;
			case SubStatus.Phase2:
				return;
			}
			break;
		case MainStatus.Skill1:
		case MainStatus.Skill5:
		case MainStatus.Skill6:
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
				_currentAnimationId = AnimationID.ANI_Skill3_START1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill3_LOOP1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill3_START2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill3_LOOP2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_Skill3_END;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_IDLE2;
				break;
			case SubStatus.Phase1:
				return;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_IDLE2;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill4_START1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill4_LOOP1;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill4_START2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_Skill4_LOOP2;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_Skill4_START3;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_Skill4_END;
				break;
			case SubStatus.Phase7:
				return;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase4:
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_DEAD_LOOP;
				break;
			case SubStatus.Phase3:
				return;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateRandomState(MainStatus status = MainStatus.Idle)
	{
		MainStatus mainStatus = status;
		if (!haveSpawn)
		{
			mainStatus = MainStatus.Skill5;
		}
		if (!haveSpawn && NeedCombo)
		{
			mainStatus = MainStatus.Skill4;
		}
		if (haveSpawn && NeedCombo)
		{
			mainStatus = MainStatus.Skill6;
		}
		if (haveSpawn && isCatching)
		{
			mainStatus = MainStatus.Skill3;
		}
		if (mainStatus == MainStatus.Idle)
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
		if ((NeedCombo && mainStatus == MainStatus.Skill6) || mainStatus == MainStatus.Skill4)
		{
			UpStart = OrangeBattleUtility.Random(0, 100) < 50;
			NeedCombo = false;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
	}

	private int RandomCard(int StartPos)
	{
		int num = 0;
		int num2 = 0;
		int num3 = SkillCard.Length;
		for (int i = 0; i < num3; i++)
		{
			num2 += SkillCard[i];
		}
		int num4 = OrangeBattleUtility.Random(0, num2);
		for (int j = 0; j < num3; j++)
		{
			num += SkillCard[j];
			if (num4 < num)
			{
				return j + StartPos;
			}
		}
		return 0;
	}

	public override void LogicUpdate()
	{
		if (_mainStatus == MainStatus.Debut)
		{
			BaseUpdate();
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
				if (IntroCallBack != null)
				{
					IntroCallBack();
					base.SoundSource.PlaySE("BossSE04", "bs031_general00");
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_velocity.y <= 0)
				{
					_velocity.y = FloatSpeed;
				}
				if (Controller.GetRealCenterPos().y > MinPos.y + 3f)
				{
					StopMove();
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_introReady && !bWaitNetStatus)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.y += -UpDown * Acceleration;
				if (_velocity.y * UpDown <= 0)
				{
					SetStatus(MainStatus.Idle, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				_velocity.y += UpDown * Acceleration;
				if (Math.Abs(_velocity.y) < FloatSpeed)
				{
					break;
				}
				StopMove();
				if (bWaitNetStatus)
				{
					break;
				}
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					if (DebugMode)
					{
						if (NextSkill == MainStatus.Skill3)
						{
							if (haveSpawn && isCatching)
							{
								SetStatus(MainStatus.Skill3);
							}
							else if (!haveSpawn)
							{
								SetStatus(MainStatus.Skill5);
							}
							else
							{
								SetStatus(MainStatus.Idle);
							}
						}
						else
						{
							SetStatus(NextSkill);
						}
					}
					else
					{
						TargetPos = Target.Controller.LogicPosition;
						UpdateRandomState();
					}
				}
				else
				{
					SetStatus(MainStatus.Idle, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.GetRealCenterPos().y > MinPos.y + 3f)
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (isCatching && CheckMissionOverAll())
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase8);
				}
				else if (Controller.GetRealCenterPos().y > MinPos.y + 4.5f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (isCatching && CheckMissionOverAll())
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase8);
					break;
				}
				EndPos = GetTargetPos();
				if ((_transform.position.x - EndPos.x) * (float)base.direction > 0f || Controller.Collisions.right || Controller.Collisions.left)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					_velocity.y = PressSpeed;
				}
				if (Controller.Collisions.below)
				{
					PlaySE("BossSE04", "bs031_general01");
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
					for (int j = 0; j < 3; j++)
					{
						EndPos = new Vector3(_transform.position.x, MinPos.y - 0.5f, 0f);
						BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, EndPos + Vector3.right * ((float)j + 0.6f), Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, EndPos + Vector3.left * ((float)j + 0.6f), Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (Controller.GetRealCenterPos().y > MinPos.y + 3f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (isCatching && CheckMissionOverAll())
				{
					SetStatus(MainStatus.Skill3);
				}
				else if (Controller.Collisions.right || Controller.Collisions.left)
				{
					UpdateDirection(-base.direction);
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase8:
				if (Controller.GetRealCenterPos().y < MinPos.y + 3f)
				{
					PlayDown_SE(false);
					SetStatus(MainStatus.Skill3);
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
				if (isCatching && CheckMissionOverAll())
				{
					SetStatus(MainStatus.Skill3);
					break;
				}
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					EndPos = GetTargetPos();
					if (leftSide)
					{
						BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, LHandModel, EndPos - LHandModel.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
					else
					{
						BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, RHandModel, EndPos - RHandModel.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
					leftSide = !leftSide;
				}
				if (HasShot)
				{
					if (--ShootTime > 0)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase1);
					}
					else if (--ShootRound > 0)
					{
						ShootTime = DefaultShootTime;
						SetStatus(MainStatus.Skill1, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase2:
				SetStatus(MainStatus.Idle);
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
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					PlaySE("BossSE04", "bs031_general05");
					for (int i = 1; i < 4; i++)
					{
						ShotPos = Controller.GetRealCenterPos() + Vector3.up * (1.2f * (float)i + 1f) + Vector3.right * 0.5f * i * base.direction;
						BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, ShotPos, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						ShotPos = Controller.GetRealCenterPos() + Vector3.down * (1.2f * (float)i + 1f) + Vector3.right * 0.5f * i * base.direction;
						BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, ShotPos, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
				}
				if (HasShot && _currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
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
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (CheckMissionOverAll())
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
					SetStatus(MainStatus.Skill3, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					SwitchCollide(FistCollide);
					HasShot = true;
					PlaySE("BossSE04", "bs031_general09");
				}
				if (targetOC == null)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (Controller.Collisions.right || Controller.Collisions.left)
				{
					PlayUpLR_SE(false);
					UpdateDirection(-base.direction);
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_transform.position.y > MaxPos.y + 8f)
				{
					EndPos = GetTargetPos();
					if (EndPos.x < CenterPos.x)
					{
						UpdateDirection(-1);
						_transform.position = new Vector3(MaxPos.x - Controller.Collider2D.size.x / 2f - 0.5f, _transform.position.y, _transform.position.z);
					}
					else
					{
						UpdateDirection(1);
						_transform.position = new Vector3(MinPos.x + Controller.Collider2D.size.x / 2f + 0.5f, _transform.position.y, _transform.position.z);
					}
					Controller.LogicPosition = new VInt3(_transform.position);
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (!HasShot && GameLogicUpdateManager.GameFrame > AtkWaitFrame)
				{
					HasShot = true;
					if (!haveSpawn)
					{
						SpawnHand();
					}
					if ((bool)EMLHand && (bool)EMRHand)
					{
						SendMission();
					}
					ShowHand(false);
					SwitchBoosterFX(false);
					if (--ShootTime > 0)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase1);
					}
				}
				if (HasShot && _currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					PlaySE("BossSE04", "bs031_general11");
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
					SetStatus(MainStatus.Skill4, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if ((UpStart && ShotAngle > (float)UseAngle[1]) || (!UpStart && ShotAngle < (float)UseAngle[1]))
				{
					if ((bool)Skill4_LHandUseFx1 && (bool)Skill4_RHandUseFx1)
					{
						Skill4_LHandUseFx1.SetActive(false);
						Skill4_RHandUseFx1.SetActive(false);
					}
					SetStatus(MainStatus.Skill4, SubStatus.Phase8);
				}
				else
				{
					if (UpStart)
					{
						ShotAngle += 1f;
					}
					else
					{
						ShotAngle -= 1f;
					}
					_animator.SetFloat(_HashAngle, ShotAngle);
				}
				break;
			case SubStatus.Phase8:
				if (GameLogicUpdateManager.GameFrame >= StopFrame || _currentFrame > 0.1f)
				{
					_animator.speed = 0f;
				}
				if (CheckMissionOverAll())
				{
					_animator.speed = 1f;
					ShowHand(true, 2);
					SwitchBoosterFX(true, 2);
					HandComeBack(2);
					SetStatus(MainStatus.Skill4, SubStatus.Phase9);
				}
				break;
			case SubStatus.Phase9:
				if (GameLogicUpdateManager.GameFrame >= StopFrame || _currentFrame > 0.495f)
				{
					_animator.speed = 0f;
				}
				if (CheckMissionOverAll())
				{
					_animator.speed = 1f;
					ShowHand(true, 1);
					SwitchBoosterFX(true, 1);
					HandComeBack(1);
					haveSpawn = false;
					SetStatus(MainStatus.Skill4, SubStatus.Phase10);
				}
				break;
			case SubStatus.Phase10:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!haveSpawn)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase1);
				}
				else
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					SpawnHand();
					ShowHand(false);
					SwitchBoosterFX(false);
					PlayUpLR_SE(false);
					if ((bool)EMLHand && (bool)EMRHand)
					{
						SendMission();
					}
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (CheckMissionOverAll())
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f && CheckMissionOverAll())
				{
					if (!haveSpawn)
					{
						SetStatus(MainStatus.Skill6, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill6, SubStatus.Phase1);
					}
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && CheckMissionOverAll())
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4);
				}
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.6f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase3:
				if (nDeadCount > 40)
				{
					_transform.position = new Vector3(MaxPos.x - Controller.Collider2D.size.x / 2f - 1f, MinPos.y, 0f);
					Controller.LogicPosition = new VInt3(_transform.position);
					SetStatus(MainStatus.Die, SubStatus.Phase4);
				}
				else
				{
					nDeadCount++;
				}
				break;
			case SubStatus.Phase4:
				if (nDeadCount > 1)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase5);
				}
				else
				{
					nDeadCount++;
				}
				break;
			}
			break;
		}
		if (CanKeepOn && haveSpawn && !isCatching && !NeedCombo && GameLogicUpdateManager.GameFrame > AtkWaitFrame && (bool)EMLHand && (bool)EMRHand)
		{
			SendMission();
		}
		if ((bool)targetOC && (int)targetOC.Hp <= 0)
		{
			HitWallCollide.HitCallback = null;
			SwitchCollide(HitWallCollide, false);
			ReleasePlayer();
		}
	}

	public void UpdateFunc()
	{
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.Skill3)
		{
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((bool)targetOC)
				{
					EndPos = Vector3.right * EMLHand.direction + Vector3.down * (targetOC.Controller.Collider2D.size.y / 2f);
				}
				else
				{
					EndPos = Vector3.right * EMLHand.direction + Vector3.down * 0.5f;
				}
				if ((bool)targetOC)
				{
					targetOC._transform.position = EMLHand._transform.position + EndPos;
					targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
				}
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase2:
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				if ((bool)targetOC)
				{
					EndPos = Vector3.right * base.direction + Vector3.down * (targetOC.Controller.Collider2D.size.y / 2f);
				}
				else
				{
					EndPos = Vector3.right * base.direction + Vector3.down * 0.5f;
				}
				if ((bool)targetOC)
				{
					targetOC._transform.position = LHandModel.position + EndPos;
					targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
				}
				break;
			case SubStatus.Phase5:
				if (HasShot)
				{
					if (HasShot && (bool)targetOC)
					{
						if (targetOC.Controller.Collisions.left || Target.Controller.Collisions.right)
						{
							HitWallCollide.transform.position = targetOC._transform.position;
							SwitchCollide(HitWallCollide, true, false);
							HitWallCollide.HitCallback = CloseHitWallCollide;
							ReleasePlayer();
						}
						else
						{
							targetOC.AddForce(new VInt3(Vector3.right * base.direction) * FistPower * 0.001f);
						}
					}
					break;
				}
				goto case SubStatus.Phase1;
			}
		}
		else if (targetOC != null)
		{
			if ((bool)targetOC)
			{
				EndPos = Vector3.right * EMLHand.direction + Vector3.down * (targetOC.Controller.Collider2D.size.y / 2f);
			}
			else
			{
				EndPos = Vector3.right * EMLHand.direction + Vector3.down * 0.5f;
			}
			targetOC._transform.position = EMLHand._transform.position + EndPos;
			targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
		}
	}

	public override void SetActive(bool isActive)
	{
		IgnoreGravity = true;
		base.SetActive(isActive);
		if (isActive)
		{
			ModelTransform.localScale = new Vector3(1.7f, 1.7f, 1.7f * (float)base.direction);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			FistCollide.UpdateBulletData(EnemyWeapons[5].BulletData);
			FistCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			HitWallCollide.UpdateBulletData(EnemyWeapons[5].BulletData);
			HitWallCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			LSkill4Collide.UpdateBulletData(EnemyWeapons[6].BulletData);
			LSkill4Collide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			RSkill4Collide.UpdateBulletData(EnemyWeapons[6].BulletData);
			RSkill4Collide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			base.AllowAutoAim = false;
			SetStatus(MainStatus.Debut);
			CloseAllFx();
			base.SoundSource.ForcePlaySE("BossSE04", "bs031_general11");
		}
		else
		{
			CloseAllFx();
			SwitchCollide(_collideBullet, false);
			SwitchCollide(FistCollide, false);
			SwitchCollide(HitWallCollide, false);
			SwitchCollide(LSkill4Collide, false);
			SwitchCollide(RSkill4Collide, false);
			HandComeBack();
			base.SoundSource.f_vol = 0f;
			ShowHand(true);
		}
	}

	private void SpawnHand()
	{
		if (haveSpawn)
		{
			return;
		}
		MOB_TABLE tMOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[1].BulletData.f_EFFECT_X];
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(tMOB_TABLE, sNetSerialID + SpawnCount);
		SpawnCount++;
		if ((bool)enemyControllerBase)
		{
			EMLHand = enemyControllerBase.gameObject.GetComponent<EM159_Controller>();
			if ((bool)EMLHand)
			{
				Vector3 position = LHandModel.position;
				EMLHand.SetPositionAndRotation(position, base.direction == -1);
				EMLHand.gameObject.name = "Left_Hand";
				EMLHand.SetLRHand(true);
				MainStatus mainStatus = _mainStatus;
				if (mainStatus == MainStatus.Skill5)
				{
					EMLHand.ModelTransform.rotation = Quaternion.Euler(new Vector3(-0.101f, -35.664f, 8.721001f));
				}
				else
				{
					EMLHand.ModelTransform.rotation = LHandModel.rotation;
				}
				EMLHand.SetActive(true);
				LHandCorp = new BossCorpsTool(EMLHand);
				LHandCorp.Master = this;
				EMLHand.JoinCorps(LHandCorp);
			}
		}
		EnemyControllerBase enemyControllerBase2 = StageUpdate.StageSpawnEnemyByMob(tMOB_TABLE, sNetSerialID + SpawnCount);
		SpawnCount++;
		if ((bool)enemyControllerBase2)
		{
			EMRHand = enemyControllerBase2.gameObject.GetComponent<EM159_Controller>();
			if ((bool)EMRHand)
			{
				Vector3 position2 = RHandModel.position;
				EMRHand.SetPositionAndRotation(position2, base.direction == -1);
				EMRHand.gameObject.name = "Right_Hand";
				EMRHand.SetLRHand(false);
				MainStatus mainStatus = _mainStatus;
				if (mainStatus == MainStatus.Skill5)
				{
					EMRHand.ModelTransform.rotation = Quaternion.Euler(new Vector3(2.083f, -54.18f, 3.102f));
				}
				else
				{
					EMRHand.ModelTransform.rotation = RHandModel.rotation;
				}
				EMRHand.SetActive(true);
				RHandCorp = new BossCorpsTool(EMRHand);
				RHandCorp.Master = this;
				EMRHand.JoinCorps(RHandCorp);
			}
		}
		haveSpawn = true;
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y + 5f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.down, 30f, layerMask, _transform);
		float y = raycastHit2D3.point.y + 10f;
		if (!raycastHit2D3)
		{
			Debug.LogError("沒有偵測到地板，之後一些技能無法準確判斷位置");
			return;
		}
		MaxPos = new Vector3(raycastHit2D2.point.x, y, 0f);
		MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D3.point.y, 0f);
		CenterPos = (MaxPos + MinPos) / 2f + Vector3.up * 0.5f;
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, ModelTransform.localScale.z * (float)base.direction);
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

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			_animator.speed = 1f;
			ReleasePlayer();
			CloseAllFx();
			SwitchCollide(_collideBullet, false);
			SwitchCollide(FistCollide, false);
			SwitchCollide(HitWallCollide, false);
			SwitchCollide(LSkill4Collide, false);
			SwitchCollide(RSkill4Collide, false);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CLOSE_FX);
			if (bPlaySE13)
			{
				PlaySE("BossSE04", "bs031_general13_stop");
			}
			if (bPlaySE14)
			{
				base.SoundSource.PlaySE("BossSE04", "bs031_general14_stop");
			}
			PlayUpLR_SE(false);
			PlayDown_SE(false);
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

	private void ReleasePlayer()
	{
		if ((bool)targetOC)
		{
			if (targetOC.IsStun)
			{
				targetOC.SetStun(false);
			}
			targetOC = null;
			isCatching = false;
		}
	}

	public void ReportObjects(object[] values)
	{
		switch ((int)values[0])
		{
		case 0:
		{
			CanKeepOn = (bool)values[1];
			MainStatus mainStatus = _mainStatus;
			if (mainStatus == MainStatus.Skill5)
			{
				HandAICase = 0;
				HandAIStep = 0;
			}
			else
			{
				HandAIStep = (HandAIStep + 1) % 4;
				if (HandAIStep == 0)
				{
					HandAICase = (HandAICase + 1) % 2;
				}
			}
			switch (HandAICircuit[HandAICase, HandAIStep])
			{
			case HandAIStatus.LAtk:
			case HandAIStatus.RAtk:
				AtkWaitFrame = GameLogicUpdateManager.GameFrame + (int)(PunchWaitTime * 20f);
				break;
			case HandAIStatus.LCatch:
			case HandAIStatus.RCatch:
				AtkWaitFrame = GameLogicUpdateManager.GameFrame + (int)(CatchWaitTime * 20f);
				break;
			}
			break;
		}
		case 1:
		{
			if ((int)Hp <= 0 || NeedCombo || _mainStatus == MainStatus.Skill4)
			{
				break;
			}
			string text = (string)values[1];
			if (values[2] != null)
			{
				CatchPos = (Transform)values[2];
			}
			for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
			{
				if (StageUpdate.runPlayers[i].sNetSerialID == text)
				{
					targetOC = StageUpdate.runPlayers[i];
				}
			}
			if ((bool)targetOC && !targetOC.IsStun)
			{
				targetOC._transform.position = EMLHand._transform.position;
				targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
				targetOC.SetStun(true);
				isCatching = true;
				PlaySE("BossSE04", "bs031_general08");
			}
			break;
		}
		default:
			Debug.LogError("雙手回傳參數類型有誤");
			break;
		}
	}

	public object[] GetValues(object[] param)
	{
		switch ((int)param[0])
		{
		case 0:
		{
			int num3 = (int)param[1];
			if (num3 == EMLHand.gameObject.GetInstanceID())
			{
				return new object[2] { LHandModel, base.direction };
			}
			if (num3 == EMRHand.gameObject.GetInstanceID())
			{
				return new object[2] { RHandModel, base.direction };
			}
			break;
		}
		case 1:
		{
			int num2 = (int)param[1];
			if (num2 == EMLHand.gameObject.GetInstanceID())
			{
				return new object[1] { EMLHand._transform.position + Vector3.right * base.direction * 2f + Vector3.up * 0.5f };
			}
			if (num2 == EMRHand.gameObject.GetInstanceID())
			{
				return new object[1] { EMRHand._transform.position + Vector3.right * base.direction * 2f + Vector3.down * 0.5f };
			}
			break;
		}
		case 2:
		{
			int num = (int)param[1];
			EndPos = Vector3.right * base.direction + Vector3.up * 2f;
			if (base.direction == -1)
			{
				if (num == EMLHand.gameObject.GetInstanceID())
				{
					return new object[1] { new Vector3(MinPos.x + (MaxPos.x - MinPos.x) * 0.3f, MinPos.y, 0f) + EndPos };
				}
				if (num == EMRHand.gameObject.GetInstanceID())
				{
					return new object[1] { new Vector3(MinPos.x + (MaxPos.x - MinPos.x) * 0.6f, MinPos.y, 0f) + EndPos };
				}
			}
			else if (base.direction == 1)
			{
				if (num == EMLHand.gameObject.GetInstanceID())
				{
					return new object[1] { new Vector3(MaxPos.x - (MaxPos.x - MinPos.x) * 0.3f, MinPos.y, 0f) + EndPos };
				}
				if (num == EMRHand.gameObject.GetInstanceID())
				{
					return new object[1] { new Vector3(MaxPos.x - (MaxPos.x - MinPos.x) * 0.6f, MinPos.y, 0f) + EndPos };
				}
			}
			break;
		}
		default:
			return null;
		}
		return null;
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

	private void ShowHand(bool onoff, int hand = 0)
	{
		string cueName = ((!onoff) ? "bs031_general06" : "bs031_general07");
		switch (hand)
		{
		case 0:
			LHandMesh.enabled = onoff;
			RHandMesh.enabled = onoff;
			break;
		case 1:
			LHandMesh.enabled = onoff;
			break;
		case 2:
			RHandMesh.enabled = onoff;
			break;
		}
		PlaySE("BossSE04", cueName);
	}

	private void CloseAllFx()
	{
		if (Skill4_LHandUseFx1 != null)
		{
			Skill4_LHandUseFx1.SetActive(false);
		}
		if (Skill4_RHandUseFx1 != null)
		{
			Skill4_RHandUseFx1.SetActive(false);
		}
		if (Skill4_LHandUseFx2 != null)
		{
			Skill4_LHandUseFx2.Stop();
		}
		if (Skill4_RHandUseFx2 != null)
		{
			Skill4_RHandUseFx2.Stop();
		}
		if (Skill6_LHandUseFx != null)
		{
			Skill6_LHandUseFx.Stop();
		}
		if (Skill6_RHandUseFx != null)
		{
			Skill6_RHandUseFx.Stop();
		}
		if (LBoosterFX != null)
		{
			LBoosterFX.Stop();
		}
		if (RBoosterFX != null)
		{
			RBoosterFX.Stop();
		}
	}

	private void SwitchBoosterFX(bool onoff, int hand = 0)
	{
		if (onoff)
		{
			switch (hand)
			{
			case 0:
				if (LBoosterFX != null)
				{
					LBoosterFX.Play();
				}
				if (RBoosterFX != null)
				{
					RBoosterFX.Play();
				}
				break;
			case 1:
				LBoosterFX.Play();
				break;
			case 2:
				RBoosterFX.Play();
				break;
			}
			return;
		}
		switch (hand)
		{
		case 0:
			if (LBoosterFX != null)
			{
				LBoosterFX.Stop();
			}
			if (RBoosterFX != null)
			{
				RBoosterFX.Stop();
			}
			break;
		case 1:
			LBoosterFX.Stop();
			break;
		case 2:
			RBoosterFX.Stop();
			break;
		}
	}

	private void HandComeBack(int hand = 0)
	{
		switch (hand)
		{
		case 0:
			if (LHandCorp != null)
			{
				LHandCorp.ComeBack();
				LHandCorp = null;
			}
			if (RHandCorp != null)
			{
				RHandCorp.ComeBack();
				RHandCorp = null;
			}
			break;
		case 1:
			if (LHandCorp != null)
			{
				LHandCorp.ComeBack();
				LHandCorp = null;
			}
			break;
		case 2:
			if (RHandCorp != null)
			{
				RHandCorp.ComeBack();
				RHandCorp = null;
			}
			break;
		}
	}

	private void SwitchCollide(CollideBullet collidebullet = null, bool onoff = true, bool follow = true)
	{
		if (collidebullet == null)
		{
			return;
		}
		if (onoff)
		{
			if (follow)
			{
				collidebullet.Active(collidebullet._transform.parent, Quaternion.identity, targetMask, true);
			}
			else
			{
				collidebullet.Active(targetMask);
			}
		}
		else
		{
			collidebullet.BackToPool();
		}
	}

	private void CloseHitWallCollide(object obj)
	{
		SwitchCollide(HitWallCollide, false);
	}

	public override void BackToPool()
	{
		HandComeBack();
		ShowHand(true);
		base.BackToPool();
	}

	private void StopMove()
	{
		_velocity = VInt3.zero;
	}
}
