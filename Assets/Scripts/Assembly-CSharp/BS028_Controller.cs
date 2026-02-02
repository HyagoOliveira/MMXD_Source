#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS028_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		ANI_DEBUT = 1,
		ANI_DEBUT_LOOP = 2,
		ANI_Skill0_START = 3,
		ANI_Skill0_LOOP = 4,
		ANI_Skill0_END = 5,
		ANI_Skill1_START1 = 6,
		ANI_Skill1_LOOP1 = 7,
		ANI_Skill1_START2 = 8,
		ANI_Skill1_LOOP2 = 9,
		ANI_Skill1_END1 = 10,
		ANI_Skill1_END2 = 11,
		ANI_Skill2_START = 12,
		ANI_Skill2_LOOP = 13,
		ANI_Skill2_END = 14,
		ANI_HURT1 = 15,
		ANI_HURT2 = 16,
		ANI_DEAD1 = 17,
		ANI_DEAD2 = 18,
		MAX_ANIMATION_ID = 19
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

	private int[] DefaultSkillCard = new int[4] { 0, 0, 1, 2 };

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[SerializeField]
	public GameObject[] RenderModes;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private int ShootTimes;

	private float ShootFrame;

	private Vector3 ShotPos;

	private Vector3 EndPos;

	[SerializeField]
	private Vector3 MaxPos;

	[SerializeField]
	private Vector3 MinPos;

	[SerializeField]
	private Vector3 CenterPos;

	[SerializeField]
	private float GroundYPos;

	private bool HasGetRoomSize;

	[SerializeField]
	private int AIStep;

	[Header("生怪")]
	[SerializeField]
	private float SummonWaitTime = 10f;

	private int SummonWaitFrame;

	private bool CanSummon;

	[Header("待機")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("雷射")]
	[SerializeField]
	private int Skill0ShootTimes = 3;

	[SerializeField]
	private ParticleSystem Skill0FX1;

	[SerializeField]
	private float Skill0Fx1Time = 1.5f;

	[SerializeField]
	private float Skill0ShootInterval = 0.5f;

	[SerializeField]
	private Transform Skill0ShotPos;

	private int Skill0UseFrame;

	[Header("搜索雷射")]
	[SerializeField]
	private float MinAngle = 30f;

	[SerializeField]
	private float MaxAngle = 150f;

	[SerializeField]
	private float MoveAngle = 4f;

	[SerializeField]
	private ParticleSystem BeamFx;

	[SerializeField]
	private float BeamLength = 8f;

	[SerializeField]
	private ParticleSystem BeamHitFX;

	[SerializeField]
	private ParticleSystem EyeBeamFX;

	[SerializeField]
	private Vector3 BeamAngle = new Vector3(0f, 90f, 150f);

	private float BeamFXAngle;

	private float BeamLengthFix;

	private float ShotAngle;

	private bool CanSummonSercher;

	[SerializeField]
	private GameObject HeadObj;

	private List<EM163_Controller> EM163s = new List<EM163_Controller>();

	[Header("牆召喚")]
	[SerializeField]
	private Transform WallModelL;

	[SerializeField]
	private Transform WallModelR;

	[SerializeField]
	private CharacterMaterial WallMeshL;

	[SerializeField]
	private CharacterMaterial WallMeshR;

	[SerializeField]
	private GameObject CoreMeshL;

	[SerializeField]
	private GameObject CoreMeshR;

	private int WallHp;

	private bool HasShowWall;

	private bool HasShowWallL;

	private int HpPercent = 17;

	private int[,] WallPattern = new int[0, 0];

	private int WallAtkPattern;

	[SerializeField]
	private float WallAtkTime = 10f;

	[SerializeField]
	private int WallAtkFrame;

	[SerializeField]
	private ParticleSystem[] HurtThunderFX = new ParticleSystem[3];

	private int LocalRecordHpStep;

	private bool StartMove;

	private bool HasDead;

	private bool lpSE02;

	private bool lpSE03;

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

	protected virtual void HashAnimation()
	{
		_animationHash = new int[19];
		_animationHash[0] = Animator.StringToHash("BS028@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS028@debut");
		_animationHash[2] = Animator.StringToHash("BS028@debut_standby");
		_animationHash[3] = Animator.StringToHash("BS028@skill_01_start");
		_animationHash[4] = Animator.StringToHash("BS028@skill_01_loop");
		_animationHash[5] = Animator.StringToHash("BS028@skill_01_end");
		_animationHash[6] = Animator.StringToHash("BS028@skill_02_step1_start");
		_animationHash[7] = Animator.StringToHash("BS028@skill_02_step1_loop");
		_animationHash[8] = Animator.StringToHash("BS028@skill_02_step2_start");
		_animationHash[9] = Animator.StringToHash("BS028@skill_02_step2_loop");
		_animationHash[10] = Animator.StringToHash("BS028@skill_02_step3_end1");
		_animationHash[11] = Animator.StringToHash("BS028@skill_02_step3_end2");
		_animationHash[12] = Animator.StringToHash("BS028@skill_03_start");
		_animationHash[13] = Animator.StringToHash("BS028@skill_03_loop");
		_animationHash[14] = Animator.StringToHash("BS028@skill_03_end");
		_animationHash[15] = Animator.StringToHash("BS028@hurt_loop");
		_animationHash[17] = Animator.StringToHash("BS028@dead");
		_animationHash[16] = Animator.StringToHash("BS028@hurt_head_separate_loop");
		_animationHash[18] = Animator.StringToHash("BS028@dead_head_separate");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (Skill0FX1 == null)
		{
			Skill0FX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_Big_Illumina_000_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill0ShotPos == null)
		{
			Skill0ShotPos = OrangeBattleUtility.FindChildRecursive(ref childs, "Eye_ShootPoint", true);
		}
		if (BeamFx == null)
		{
			BeamFx = OrangeBattleUtility.FindChildRecursive(ref childs, "fxduring_Big_Illumina_000_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (BeamHitFX == null)
		{
			BeamHitFX = OrangeBattleUtility.FindChildRecursive(ref childs, "fxhit_Big_Illumina_000_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (EyeBeamFX == null)
		{
			EyeBeamFX = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_Big_Illumina_002_(work)", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (WallModelL == null)
		{
			WallModelL = OrangeBattleUtility.FindChildRecursive(ref childs, "WallModel2", true);
		}
		if (WallModelR == null)
		{
			WallModelR = OrangeBattleUtility.FindChildRecursive(ref childs, "WallModel1", true);
		}
		if (WallMeshL == null)
		{
			WallMeshL = OrangeBattleUtility.FindChildRecursive(ref childs, "WallModel2", true).gameObject.AddOrGetComponent<CharacterMaterial>();
		}
		if (WallMeshR == null)
		{
			WallMeshR = OrangeBattleUtility.FindChildRecursive(ref childs, "WallModel1", true).gameObject.AddOrGetComponent<CharacterMaterial>();
		}
		if (CoreMeshL == null)
		{
			CoreMeshL = OrangeBattleUtility.FindChildRecursive(ref childs, "CoreL", true).gameObject;
		}
		if (CoreMeshR == null)
		{
			CoreMeshR = OrangeBattleUtility.FindChildRecursive(ref childs, "CoreR", true).gameObject;
		}
		if (HurtThunderFX[0] == null)
		{
			HurtThunderFX[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_Big_Illumina_003_(work)1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (HurtThunderFX[1] == null)
		{
			HurtThunderFX[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_Big_Illumina_003_(work)2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (HurtThunderFX[2] == null)
		{
			HurtThunderFX[2] = OrangeBattleUtility.FindChildRecursive(ref childs, "fxuse_Big_Illumina_003_(work)3", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimTransform = _enemyCollider[0].transform;
		base.AimPoint = new Vector3(0f, -0.4f, 0.6f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		FallDownSE = new string[2] { "BattleSE", "bt_ridearmor02" };
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
		if ((bool)WallMeshL)
		{
			WallMeshL.Disappear();
		}
		if ((bool)WallMeshR)
		{
			WallMeshR.Disappear();
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_explode_000", 10);
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (!HasGetRoomSize)
		{
			CheckRoomSize();
		}
		if (!StartMove)
		{
			StartMove = true;
		}
		if (!CanSummon)
		{
			CanSummon = true;
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
			if (netSyncData.nParam0 < 0)
			{
				netSyncData.nParam0 = 0;
			}
			WallHp = netSyncData.nParam0;
			if (netSyncData.sParam0 != string.Empty || netSyncData.sParam0.Length > 0)
			{
				string[] array = netSyncData.sParam0.Split(',');
				if ((nSet == 3 || nSet == 5) && array.Length > 1)
				{
					try
					{
						WallAtkPattern = int.Parse(array[1]);
						if (WallAtkPattern > WallPattern.GetLength(0) - 1)
						{
							WallAtkPattern = 0;
							Debug.LogError("牆壁設線模式有錯，收到的模式是 " + array[1]);
						}
						ShootWallLaser(WallAtkPattern);
						WallAtkFrame = GameLogicUpdateManager.GameFrame + (int)(20f * WallAtkTime);
						return;
					}
					catch
					{
						WallAtkPattern = 0;
						Debug.LogError("牆壁設線模式有錯，收到的模式是 " + array[1]);
						return;
					}
				}
				if (array.Length != 0 && array[0] != string.Empty)
				{
					try
					{
						AIStep = int.Parse(array[0]);
						AIStep = AIStep % 4 + 1;
					}
					catch
					{
						AIStep = AIStep % 4 + 1;
						Debug.LogError("AI階段有錯，收到的AI階段是 " + array[0]);
					}
				}
			}
		}
		if ((nSet == 2 && _mainStatus == MainStatus.Skill0) || (nSet == 3 && _mainStatus == MainStatus.Skill1))
		{
			return;
		}
		for (int i = 0; i < HurtThunderFX.Length; i++)
		{
			if ((bool)HurtThunderFX[i])
			{
				HurtThunderFX[i].Stop();
			}
		}
		if ((bool)BeamFx && BeamFx.isPlaying)
		{
			BeamFx.Stop();
		}
		if ((bool)BeamHitFX && BeamHitFX.isPlaying)
		{
			BeamHitFX.Stop();
		}
		if ((bool)EyeBeamFX && EyeBeamFX.isPlaying)
		{
			EyeBeamFX.Stop();
		}
		if ((bool)Skill0FX1 && Skill0FX1.isPlaying)
		{
			Skill0FX1.Stop();
		}
		if (nSet != 0)
		{
			SetStatus((MainStatus)nSet);
		}
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, ModelTransform.localScale.z);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		if (!StartMove && !HasDead && mainStatus == MainStatus.Die)
		{
			return;
		}
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			}
			break;
		case MainStatus.Idle:
		{
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			EM163s.Clear();
			for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
			{
				EM163_Controller eM163_Controller = StageUpdate.runEnemys[i].mEnemy as EM163_Controller;
				if ((bool)eM163_Controller)
				{
					EM163s.Add(eM163_Controller);
				}
			}
			for (int j = 0; j < EM163s.Count; j++)
			{
				EM163s[j].SetDie();
			}
			break;
		}
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetColliderEnable(true);
				if (HasShowWallL)
				{
					CloseWall();
				}
				if (!HasShowWall)
				{
					ForceShowWall();
				}
				if (HasShowWall)
				{
					base.AllowAutoAim = true;
				}
				EndPos = GetTargetPos();
				ShootTimes = Skill0ShootTimes;
				break;
			case SubStatus.Phase1:
				Skill0FX1.Play();
				Skill0UseFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0Fx1Time * 20f);
				break;
			case SubStatus.Phase2:
				Skill0UseFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0ShootInterval * 20f);
				EndPos = GetTargetPos();
				BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, Skill0ShotPos.position.xy() + Vector2.down * 3f, EndPos.xy() - (Skill0ShotPos.position.xy() + Vector2.down * 3f), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetColliderEnable(true);
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform, 998);
				if (!HasShowWall || !HasShowWallL)
				{
					ForceShowWall();
					ForceShowWall(1);
				}
				if (HasShowWall)
				{
					base.AllowAutoAim = true;
				}
				break;
			case SubStatus.Phase1:
				if (base.direction == -1)
				{
					ShotAngle = MaxAngle;
				}
				else
				{
					ShotAngle = MinAngle;
				}
				break;
			case SubStatus.Phase2:
				PlaySE("BossSE04", "bs112_Illumina02_lp");
				lpSE02 = true;
				EyeBeamFX.Play();
				BeamFx.Play();
				break;
			case SubStatus.Phase3:
				base.direction = 1;
				break;
			case SubStatus.Phase4:
				BeamFx.Stop();
				BeamHitFX.Stop();
				EyeBeamFX.Stop();
				if (lpSE02)
				{
					PlaySE("BossSE04", "bs112_Illumina02_stop");
					lpSE02 = false;
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetColliderEnable(false);
				WallModelR.position = new Vector3(MaxPos.x + 1.5f, MaxPos.y + 4f, 0f);
				WallMeshR.Appear();
				break;
			case SubStatus.Phase2:
				WallHp = (int)MaxHp * HpPercent / 100;
				if (WallHp > (int)Hp)
				{
					WallHp = Hp;
				}
				HasShowWall = true;
				base.VanishStatus = false;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetColliderEnable(false);
				WallModelR.position = new Vector3(MaxPos.x + 1.5f, MaxPos.y + 4f, 0f);
				WallModelL.position = new Vector3(MinPos.x - 1.5f, MaxPos.y + 4f, 0f);
				WallMeshR.Appear();
				WallMeshL.Appear();
				break;
			case SubStatus.Phase2:
				WallHp = (int)MaxHp * HpPercent / 100;
				if (WallHp > (int)Hp)
				{
					WallHp = Hp;
				}
				HasShowWall = true;
				HasShowWallL = true;
				base.VanishStatus = false;
				WallAtkFrame = GameLogicUpdateManager.GameFrame + (int)(20f * WallAtkTime);
				break;
			}
			break;
		case MainStatus.Skill4:
		{
			SubStatus subStatus2 = _subStatus;
			if ((uint)subStatus2 <= 1u)
			{
				if ((bool)BeamFx && BeamFx.isPlaying)
				{
					BeamFx.Stop();
				}
				if ((bool)BeamHitFX && BeamHitFX.isPlaying)
				{
					BeamHitFX.Stop();
				}
				if ((bool)EyeBeamFX && EyeBeamFX.isPlaying)
				{
					EyeBeamFX.Stop();
				}
				if ((bool)Skill0FX1 && Skill0FX1.isPlaying)
				{
					Skill0FX1.Stop();
				}
				CloseWall();
				if (lpSE02)
				{
					lpSE02 = false;
					PlaySE("BossSE04", "bs112_Illumina02_stop");
				}
			}
			break;
		}
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			case SubStatus.Phase1:
				base.AllowAutoAim = false;
				base.DeadPlayCompleted = true;
				break;
			case SubStatus.Phase2:
				StartCoroutine(BossDieFlow(_transform.position));
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
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT_LOOP;
				break;
			case SubStatus.Phase1:
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
				_currentAnimationId = AnimationID.ANI_Skill0_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill0_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill0_END;
				break;
			case SubStatus.Phase2:
				return;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill1_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill1_START2;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill1_LOOP2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill1_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill1_END1;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_Skill1_END2;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill2_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill2_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill2_END;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill2_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill2_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill2_END;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_HURT1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_HURT2;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill1_END1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill1_END2;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEAD1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEAD2;
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateNextState(MainStatus status = MainStatus.Idle)
	{
		MainStatus mainStatus = status;
		if (mainStatus == MainStatus.Idle)
		{
			MainStatus mainStatus2 = _mainStatus;
			if (mainStatus2 == MainStatus.Debut)
			{
				SetStatus(MainStatus.Idle);
				return;
			}
			switch (AIStep)
			{
			case 0:
				AIStep = 1;
				mainStatus = MainStatus.Skill2;
				break;
			case 1:
				mainStatus = MainStatus.Skill2;
				break;
			case 2:
				mainStatus = MainStatus.Skill0;
				break;
			case 3:
				mainStatus = MainStatus.Skill3;
				break;
			case 4:
				mainStatus = MainStatus.Skill1;
				break;
			}
		}
		switch (mainStatus)
		{
		case MainStatus.Skill0:
			AIStep = 2;
			break;
		case MainStatus.Skill1:
			AIStep = 4;
			break;
		case MainStatus.Skill2:
			AIStep = 1;
			break;
		case MainStatus.Skill3:
			AIStep = 3;
			break;
		}
		if (DebugMode)
		{
			mainStatus = NextSkill;
		}
		if (mainStatus != 0)
		{
			if (CheckHost())
			{
				UploadEnemyStatus((int)mainStatus, false, new object[1] { WallHp }, new object[1] { AIStep.ToString() });
			}
		}
		else
		{
			SetStatus(MainStatus.Idle);
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
		if (_unlockReady && StartMove)
		{
			if ((int)Hp <= 0 && _mainStatus != MainStatus.Die)
			{
				SetStatus(MainStatus.Die);
			}
			if (HasShowWall && _mainStatus != MainStatus.Die)
			{
				CalWallHpShouldBe();
			}
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
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
				if (IntroCallBack == null)
				{
					break;
				}
				IntroCallBack();
				if (!bWaitNetStatus)
				{
					if (!StartMove)
					{
						StartMove = true;
					}
					UpdateNextState();
				}
				else
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus && IdleWaitFrame < GameLogicUpdateManager.GameFrame)
			{
				UpdateNextState();
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
				if (GameLogicUpdateManager.GameFrame > Skill0UseFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame > Skill0UseFrame && !bWaitNetStatus)
				{
					if (--ShootTimes > 0)
					{
						UpdateNextState(MainStatus.Skill0);
						SetStatus(MainStatus.Skill0, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase3);
					}
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
				ShotAngle -= MoveAngle * (float)(-base.direction);
				if (BeamFx != null)
				{
					Vector3 vector = Quaternion.Euler(0f, 0f, 0f - ShotAngle) * Vector3.right;
					Debug.DrawRay(BeamFx.transform.position, vector * BeamLength, Color.blue);
					RaycastHit2D[] array = Physics2D.RaycastAll(BeamFx.transform.position.xy(), vector, 50f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
					if (array.Length != 0)
					{
						RaycastHit2D raycastHit2D = array[0];
						BeamFx.transform.LookAt(raycastHit2D.point);
						if (raycastHit2D.point.y <= GroundYPos + 0.1f)
						{
							BeamHitFX.transform.position = raycastHit2D.point;
						}
						if (!BeamHitFX.isPlaying)
						{
							BeamHitFX.Play();
						}
						BeamLengthFix = Vector3.Distance(raycastHit2D.point, BeamFx.transform.position) / BeamLength;
						BeamFx.transform.localScale = new Vector3(1f, 1f, BeamLengthFix);
					}
					else
					{
						BeamHitFX.Stop();
					}
					array = Physics2D.RaycastAll(BeamFx.transform.position, vector, 20f, (1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer));
					if (array.Length != 0)
					{
						RaycastHit2D raycastHit2D = array[0];
						OrangeCharacter component = raycastHit2D.collider.GetComponent<OrangeCharacter>();
						if ((bool)component)
						{
							array = Physics2D.RaycastAll(BeamFx.transform.position, vector, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
							if (array.Length != 0)
							{
								raycastHit2D = array[0];
								BeamFx.transform.LookAt(raycastHit2D.point);
								if (raycastHit2D.point.y <= GroundYPos + 0.1f)
								{
									BeamHitFX.transform.position = raycastHit2D.point;
								}
								if (!BeamHitFX.isPlaying)
								{
									BeamHitFX.Play();
								}
								BeamLengthFix = Vector3.Distance(raycastHit2D.point, BeamFx.transform.position) / BeamLength;
								BeamFx.transform.localScale = new Vector3(1f, 1f, BeamLengthFix);
								Debug.DrawRay(BeamFx.transform.position, vector * BeamLength, Color.black);
							}
							else
							{
								BeamHitFX.Stop();
							}
							Target = component;
							for (int k = 0; k < StageUpdate.runEnemys.Count; k++)
							{
								EM163_Controller eM163_Controller = StageUpdate.runEnemys[k].mEnemy as EM163_Controller;
								if ((bool)eM163_Controller)
								{
									eM163_Controller.SetAtkPos(Target._transform.position);
								}
								SetStatus(MainStatus.Skill1, SubStatus.Phase3);
							}
						}
					}
				}
				if ((base.direction == 1 && ShotAngle >= MaxAngle) || (base.direction == -1 && ShotAngle <= MinAngle))
				{
					base.direction *= -1;
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase3:
				if (BeamFx.isPlaying && _currentFrame > 1f)
				{
					PlaySE("BossSE04", "bs112_Illumina02_stop");
					lpSE02 = false;
					BeamFx.Stop();
					BeamHitFX.Stop();
					Vector3 vector2 = Quaternion.Euler(0f, 0f, 0f - ShotAngle) * Vector3.right;
					Debug.DrawRay(BeamFx.transform.position, vector2 * BeamLength, Color.blue);
					RaycastHit2D[] array2 = Physics2D.RaycastAll(BeamFx.transform.position.xy(), vector2, 50f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
					if (array2.Length != 0)
					{
						RaycastHit2D raycastHit2D2 = array2[0];
						BeamFx.transform.LookAt(raycastHit2D2.point);
						BeamLengthFix = Vector3.Distance(raycastHit2D2.point, BeamFx.transform.position) / BeamLength;
						BeamFx.transform.localScale = new Vector3(1f, 1f, BeamLengthFix);
					}
				}
				else if (_currentFrame > 3f)
				{
					if (base.direction == -1)
					{
						ShotAngle = MaxAngle;
					}
					else
					{
						ShotAngle = MinAngle;
					}
					if (!bWaitNetStatus)
					{
						UpdateNextState(MainStatus.Skill1);
						SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase5);
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
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
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
			{
				if (!(_currentFrame > 10f))
				{
					break;
				}
				for (int j = 0; j < HurtThunderFX.Length; j++)
				{
					if ((bool)HurtThunderFX[j])
					{
						HurtThunderFX[j].Stop();
					}
				}
				if (lpSE03)
				{
					PlaySE("BossSE04", "bs112_Illumina03_stop");
					lpSE03 = false;
				}
				SetStatus(MainStatus.Idle);
				break;
			}
			case SubStatus.Phase1:
				if (_currentFrame > 10f)
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
			{
				if (!(_currentFrame > 1f))
				{
					break;
				}
				for (int i = 0; i < HurtThunderFX.Length; i++)
				{
					if ((bool)HurtThunderFX[i])
					{
						HurtThunderFX[i].Stop();
					}
				}
				if (lpSE03)
				{
					PlaySE("BossSE04", "bs112_Illumina03_stop");
					lpSE03 = false;
				}
				SetStatus(MainStatus.Idle);
				break;
			}
			}
			break;
		case MainStatus.Die:
		{
			SubStatus subStatus = _subStatus;
			if ((uint)subStatus <= 1u && _currentFrame > 0.5f)
			{
				SetStatus(MainStatus.Die, SubStatus.Phase2);
			}
			break;
		}
		}
		if (CanSummon && GameLogicUpdateManager.GameFrame > SummonWaitFrame)
		{
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
			SummonWaitFrame = GameLogicUpdateManager.GameFrame + (int)(SummonWaitTime * 20f);
		}
		if (HasShowWallL && GameLogicUpdateManager.GameFrame > WallAtkFrame && !bWaitNetStatus)
		{
			WallAtkPattern = OrangeBattleUtility.Random(0, WallPattern.GetLength(0));
			UploadEnemyStatus((int)_mainStatus, false, new object[1] { WallHp }, new object[1] { AIStep + "," + WallAtkPattern + "," });
		}
	}

	public void UpdateFunc()
	{
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		switch (_mainStatus)
		{
		case MainStatus.Skill2:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase1)
			{
				if (WallModelR.position.y > GroundYPos + 1f)
				{
					WallModelR.position += Vector3.down * 0.35f;
					break;
				}
				PlaySE("BossSE04", "bs112_Illumina09");
				WallModelR.position = new Vector3(MaxPos.x + 1.5f, GroundYPos, 0f);
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
				SetStatus(MainStatus.Skill2, SubStatus.Phase2);
			}
			break;
		}
		case MainStatus.Skill3:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase1)
			{
				if (WallModelR.position.y > GroundYPos + 1f && WallModelL.position.y > GroundYPos + 1f)
				{
					WallModelR.position += Vector3.down * 0.35f;
					WallModelL.position += Vector3.down * 0.35f;
					break;
				}
				PlaySE("BossSE04", "bs112_Illumina09");
				WallModelR.position = new Vector3(MaxPos.x + 1.5f, GroundYPos, 0f);
				WallModelL.position = new Vector3(MinPos.x - 1.5f, GroundYPos, 0f);
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
				SetStatus(MainStatus.Skill2, SubStatus.Phase2);
			}
			break;
		}
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		IgnoreGravity = true;
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			HasGetRoomSize = false;
			CheckRoomSize();
			base.AllowAutoAim = false;
			CanSummon = false;
			CanSummonSercher = true;
			LocalRecordHpStep = 1;
			StartMove = false;
			HasDead = false;
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, ModelTransform.localScale.z);
		_transform.position = pos;
	}

	public override void BossIntro(Action cb)
	{
		SetStatus(MainStatus.Debut);
		IntroCallBack = cb;
		_introReady = true;
	}

	public override void Unlock()
	{
		_unlockReady = true;
		CanSummon = true;
		SummonWaitFrame = GameLogicUpdateManager.GameFrame + (int)(SummonWaitTime * 20f);
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
		CanSummon = false;
		CanSummonSercher = false;
		HasDead = true;
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		if (HasShowWall || HasShowWallL)
		{
			CloseWall();
		}
		if (lpSE02 || lpSE03)
		{
			base.SoundSource.StopAll();
			lpSE02 = false;
			lpSE03 = false;
		}
		for (int i = 0; i < HurtThunderFX.Length; i++)
		{
			if ((bool)HurtThunderFX[i])
			{
				HurtThunderFX[i].Stop();
			}
		}
		if ((bool)BeamFx && BeamFx.isPlaying)
		{
			BeamFx.Stop();
		}
		if ((bool)BeamHitFX && BeamHitFX.isPlaying)
		{
			BeamHitFX.Stop();
		}
		if ((bool)EyeBeamFX && EyeBeamFX.isPlaying)
		{
			EyeBeamFX.Stop();
		}
		if ((bool)Skill0FX1 && Skill0FX1.isPlaying)
		{
			Skill0FX1.Stop();
		}
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		StageUpdate.SlowStage();
		SetColliderEnable(false);
		if (_mainStatus == MainStatus.Skill1 && (_subStatus == SubStatus.Phase2 || _subStatus == SubStatus.Phase3 || _subStatus == SubStatus.Phase4))
		{
			SetStatus(MainStatus.Die, SubStatus.Phase1);
		}
		else
		{
			SetStatus(MainStatus.Die);
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		if (AiState == AI_STATE.mob_001)
		{
			WallPattern = new int[4, 2]
			{
				{ 1, 2 },
				{ 2, 3 },
				{ 1, 3 },
				{ 2, 4 }
			};
		}
	}

	private void CheckRoomSize()
	{
	}

	private Vector3 GetTargetPos(bool realcenter = true)
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
				TargetPos = new VInt3(Target.GetTargetPoint());
			}
			return TargetPos.vec3;
		}
		return _transform.position + Vector3.down * 3f;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (!HasShowWall)
		{
			tHurtPassParam.dmg = 0;
			return base.Hurt(tHurtPassParam);
		}
		if ((int)tHurtPassParam.dmg > WallHp)
		{
			tHurtPassParam.dmg = WallHp;
		}
		WallHp -= tHurtPassParam.dmg;
		if (WallHp <= 0)
		{
			base.VanishStatus = true;
			if ((int)Hp - (int)tHurtPassParam.dmg > 0)
			{
				if (_mainStatus == MainStatus.Skill0 || AIStep == 3)
				{
					SetStatus(MainStatus.Skill4);
				}
				else if (_mainStatus == MainStatus.Skill1 || AIStep == 4)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
			}
		}
		return base.Hurt(tHurtPassParam);
	}

	private void ForceShowWall(int WallNum = 0)
	{
		switch (WallNum)
		{
		case 0:
			WallModelR.position = new Vector3(MaxPos.x + 1.5f, GroundYPos, 0f);
			WallMeshR.Appear();
			if (WallHp > (int)Hp)
			{
				WallHp = Hp;
			}
			else if (WallHp == 0)
			{
				WallHp = (int)MaxHp * HpPercent / 100;
			}
			HasShowWall = true;
			base.VanishStatus = false;
			break;
		case 1:
			WallModelL.position = new Vector3(MinPos.x - 1.5f, GroundYPos, 0f);
			WallMeshL.Appear();
			HasShowWallL = true;
			break;
		}
	}

	private void CloseWall()
	{
		for (int i = 0; i < HurtThunderFX.Length; i++)
		{
			if ((bool)HurtThunderFX[i])
			{
				HurtThunderFX[i].Play();
			}
		}
		if (!lpSE03)
		{
			PlaySE("BossSE04", "bs112_Illumina03_lp");
			lpSE03 = true;
		}
		if (HasShowWall)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_explode_000", WallModelR.position + Vector3.up + Vector3.left * 2f, Quaternion.identity, Array.Empty<object>());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_explode_000", WallModelR.position + Vector3.up * 3f + Vector3.left * 2.8f, Quaternion.identity, Array.Empty<object>());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_explode_000", WallModelR.position + Vector3.up * 5f + Vector3.left * 1.3f, Quaternion.identity, Array.Empty<object>());
			HasShowWall = false;
			base.AllowAutoAim = false;
		}
		if (HasShowWallL)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_explode_000", WallModelL.position + Vector3.up + Vector3.right * 2f, Quaternion.identity, Array.Empty<object>());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_explode_000", WallModelL.position + Vector3.up * 3f + Vector3.right * 1.3f, Quaternion.identity, Array.Empty<object>());
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_explode_000", WallModelL.position + Vector3.up * 5f + Vector3.right * 2.8f, Quaternion.identity, Array.Empty<object>());
			HasShowWallL = false;
			EM163s.Clear();
			for (int j = 0; j < StageUpdate.runEnemys.Count; j++)
			{
				EM163_Controller eM163_Controller = StageUpdate.runEnemys[j].mEnemy as EM163_Controller;
				if ((bool)eM163_Controller)
				{
					EM163s.Add(eM163_Controller);
				}
			}
			for (int k = 0; k < EM163s.Count; k++)
			{
				EM163s[k].SetDie();
			}
		}
		if ((bool)WallMeshL)
		{
			WallMeshL.Disappear();
		}
		if ((bool)WallMeshR)
		{
			WallMeshR.Disappear();
		}
	}

	private void ShootWallLaser(int wallatkpattern = 0)
	{
		for (int i = 0; i < WallPattern.GetLength(1); i++)
		{
			ShotPos = WallModelL.position + Vector3.right * 1.5f + Vector3.up * 1.8f * WallPattern[wallatkpattern, i];
			BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShotPos, Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		}
	}

	private void CalWallHpShouldBe()
	{
		int num = (int)MaxHp * (HpPercent * LocalRecordHpStep) / 100;
		if (num > (int)MaxHp)
		{
			num = MaxHp;
		}
		while ((int)MaxHp - num > (int)Hp)
		{
			LocalRecordHpStep++;
			num = (int)MaxHp * (HpPercent * LocalRecordHpStep) / 100;
			if (num > (int)MaxHp)
			{
				num = MaxHp;
			}
		}
		if ((int)MaxHp - num > (int)Hp)
		{
			Debug.LogError("不該進來這裡1");
		}
		else if ((int)MaxHp - num < (int)Hp)
		{
			WallHp = (int)Hp - ((int)MaxHp - num);
		}
		else if ((int)MaxHp - num == (int)Hp)
		{
			WallHp = 0;
			if (LocalRecordHpStep % 2 == 1 && _mainStatus == MainStatus.Skill0)
			{
				SetStatus(MainStatus.Skill4);
			}
			else if (LocalRecordHpStep % 2 == 0 && _mainStatus == MainStatus.Skill1)
			{
				SetStatus(MainStatus.Skill4, SubStatus.Phase1);
			}
			else if (_mainStatus != MainStatus.Skill2 && _mainStatus != MainStatus.Skill3)
			{
				Debug.LogError(string.Concat("不該進來這裡2，目前狀態是 MainStatus：", _mainStatus, "  LocalRecordHpStep：", LocalRecordHpStep, "  Hp：", Hp, "  MaxHp：", MaxHp, "  wallhptmp：", num));
			}
			LocalRecordHpStep++;
		}
		else
		{
			Debug.LogError("不該進來這裡3");
		}
	}
}
