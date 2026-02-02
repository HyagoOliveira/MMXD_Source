#define RELEASE
using System;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS075_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum WallSide
	{
		Up = 1,
		Down = 2,
		Left = 3,
		Right = 4,
		None = 5
	}

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
		ANI_DEBUT_LOOP = 2,
		ANI_WALK = 3,
		ANI_SKILL0_START = 4,
		ANI_SKILL0_LOOP = 5,
		ANI_SKILL0_END = 6,
		ANI_SKILL1_START = 7,
		ANI_SKILL1_LOOP = 8,
		ANI_SKILL1_END = 9,
		ANI_SKILL2_START = 10,
		ANI_SKILL2_LOOP = 11,
		ANI_SKILL2_END = 12,
		ANI_SKILL3_START = 13,
		ANI_SKILL3_LOOP = 14,
		ANI_SKILL3_END = 15,
		ANI_HURT = 16,
		ANI_DEAD = 17,
		MAX_ANIMATION_ID = 18
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

	private int[] DefaultSkillWeight = new int[4] { 1, 1, 1, 1 };

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

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

	private float ShotAngle;

	[SerializeField]
	private CatchPlayerTool CatchTool;

	private int SpawnCount;

	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("磁力吸引")]
	[SerializeField]
	private float Skill0AtkTime = 5f;

	[SerializeField]
	private Vector2 Skill0GravityForce = Vector2.one;

	[SerializeField]
	private float Skill0Gravitational = 120f;

	[SerializeField]
	private ParticleSystem Skill0UseFx;

	[SerializeField]
	private int Skill0RecoverPercent = 20;

	private VirtualButton jumpbutton;

	private bool Skill0NeedRecover;

	[Header("磁鐵環繞")]
	[SerializeField]
	private float Skill1AtkInterrval = 4f;

	private int Skill1AtkFrame;

	[SerializeField]
	private int Skill1AtkTimes = 3;

	private EM196_Controller[] Skill1Tails = new EM196_Controller[4];

	[SerializeField]
	private Transform[] Skill1TailsObjs = new Transform[4];

	[SerializeField]
	private SkinnedMeshRenderer[] Skill1TailsMesh = new SkinnedMeshRenderer[5];

	private int[] Skill1TailAtkTimes = new int[4] { 3, 3, 3, 3 };

	private bool Skill1CanTailAtk;

	[Header("手裏劍")]
	[SerializeField]
	private int Skill2ShootTimes = 3;

	[SerializeField]
	private float Skill2ShootFrame = 0.83f;

	[Header("金蟬脫殼")]
	[SerializeField]
	private float Skill3HideTime = 0.3f;

	[SerializeField]
	private float Skill3ShowTime = 0.3f;

	[SerializeField]
	private float Skill3XFloat = 3f;

	[SerializeField]
	private ParticleSystem Skill3UseFx1;

	[SerializeField]
	private ParticleSystem Skill3UseFx2;

	[SerializeField]
	private SkinnedMeshRenderer BodyMesh;

	private WallSide _wallside = WallSide.Down;

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

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
		_animationHash = new int[18];
		_animationHash[0] = Animator.StringToHash("BS075@idl_loop");
		_animationHash[1] = Animator.StringToHash("BS075@debut");
		_animationHash[2] = Animator.StringToHash("BS075@debut_loop");
		_animationHash[3] = Animator.StringToHash("BS075@walk_loop");
		_animationHash[4] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL1_CASTING1");
		_animationHash[5] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL1_CASTLOOP1");
		_animationHash[6] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL1_CASTOUT1");
		_animationHash[7] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL2_CASTING1");
		_animationHash[8] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL2_CASTLOOP1");
		_animationHash[9] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL2_CASTOUT1");
		_animationHash[10] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL3_CASTING1");
		_animationHash[11] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL3_CASTLOOP1");
		_animationHash[12] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL3_CASTOUT1");
		_animationHash[13] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL4_CASTING1");
		_animationHash[14] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL4_CASTLOOP1");
		_animationHash[15] = Animator.StringToHash("BS075@MAGNET_CENTIPEDE_SKILL4_CASTOUT1");
		_animationHash[16] = Animator.StringToHash("BS075@hurt_loop");
		_animationHash[17] = Animator.StringToHash("BS075@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPos", true);
		}
		if (BodyMesh == null)
		{
			BodyMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS075_BodyMesh", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (Skill1TailsMesh[0] == null)
		{
			Skill1TailsMesh[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS075_Tail_Mesh05", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (Skill1TailsMesh[1] == null)
		{
			Skill1TailsMesh[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS075_Tail_Mesh04", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (Skill1TailsMesh[2] == null)
		{
			Skill1TailsMesh[2] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS075_Tail_Mesh03", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (Skill1TailsMesh[3] == null)
		{
			Skill1TailsMesh[3] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS075_Tail_Mesh02", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (Skill1TailsMesh[4] == null)
		{
			Skill1TailsMesh[4] = OrangeBattleUtility.FindChildRecursive(ref childs, "BS075_Tail_Mesh01", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (Skill0UseFx == null)
		{
			Skill0UseFx = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0UseFx", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill3UseFx1 == null)
		{
			Skill3UseFx1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill2UseFx1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill3UseFx2 == null)
		{
			Skill3UseFx2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill2UseFx2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1TailsObjs[0] == null)
		{
			Skill1TailsObjs[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "Bone_tail_01", true);
		}
		if (Skill1TailsObjs[1] == null)
		{
			Skill1TailsObjs[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "Bone_tail_02", true);
		}
		if (Skill1TailsObjs[2] == null)
		{
			Skill1TailsObjs[2] = OrangeBattleUtility.FindChildRecursive(ref childs, "Bone_tail_03", true);
		}
		if (Skill1TailsObjs[3] == null)
		{
			Skill1TailsObjs[3] = OrangeBattleUtility.FindChildRecursive(ref childs, "Bone_tail_04", true);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimPoint = new Vector3(0f, 0.8f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		FallDownSE = new string[2] { "BossSE05", "bs044_magne11" };
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
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 != 0 && subStatus2 == SubStatus.Phase2)
			{
				CheckRoomSize();
			}
			break;
		}
		case MainStatus.Idle:
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			if (IsInvincible)
			{
				IsInvincible = false;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				Skill0NeedRecover = false;
				PlayBossSE("BossSE05", "bs044_magne01_lp");
				bPlayLoopSE = true;
				break;
			case SubStatus.Phase1:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0AtkTime * 20f);
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				jumpbutton = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.JUMP);
				jumpbutton.SetBanMask(true);
				foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
				{
					runPlayer.RemoveSelfJumpCB();
				}
				break;
			case SubStatus.Phase2:
				PlayBossSE("BossSE05", "bs044_magne01_stop");
				bPlayLoopSE = false;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				CatchTool.ReleaseTarget();
				foreach (OrangeCharacter runPlayer2 in StageUpdate.runPlayers)
				{
					runPlayer2.PlayerResetPressJump();
					jumpbutton = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.JUMP);
					jumpbutton.SetBanMask(false);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
			{
				if (Skill1CanTailAtk)
				{
					break;
				}
				PlayBossSE("BossSE05", "bs044_magne06");
				Skill1Tails = new EM196_Controller[4];
				for (int j = 0; j < 4; j++)
				{
					Vector3 position2 = Skill1TailsObjs[j].position;
					position2.z = 0f;
					EM196_Controller eM196_Controller = SpawnEnemy(position2);
					if ((bool)eM196_Controller)
					{
						Skill1Tails[j] = eM196_Controller;
						Skill1TailAtkTimes[j] = Skill1AtkTimes;
					}
					else
					{
						Debug.LogError("BS075 第 " + j + " 個尾巴生成失敗");
						Skill1TailAtkTimes[j] = 0;
					}
				}
				SkinnedMeshRenderer[] skill1TailsMesh = Skill1TailsMesh;
				for (int i = 0; i < skill1TailsMesh.Length; i++)
				{
					skill1TailsMesh[i].enabled = false;
				}
				Skill1CanTailAtk = true;
				Skill1AtkFrame = GameLogicUpdateManager.GameFrame + (int)(Skill1AtkInterrval * 20f);
				int num2 = ((_wallside != WallSide.Up) ? 1 : (-1));
				if ((bool)Skill1Tails[0])
				{
					Skill1Tails[0].SetAtkPos(new Vector3(NowPos.x + 0.5f, NowPos.y + 4f * (float)num2, 0f), false);
				}
				if ((bool)Skill1Tails[1])
				{
					Skill1Tails[1].SetAtkPos(new Vector3(NowPos.x - 0.5f, NowPos.y + 4f * (float)num2, 0f), false);
				}
				if ((bool)Skill1Tails[2])
				{
					Skill1Tails[2].SetAtkPos(new Vector3(NowPos.x + 0.5f, NowPos.y + 5f * (float)num2, 0f), false);
				}
				if ((bool)Skill1Tails[3])
				{
					Skill1Tails[3].SetAtkPos(new Vector3(NowPos.x - 0.5f, NowPos.y + 5f * (float)num2, 0f), false);
				}
				break;
			}
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				HasActed = false;
				ActionAnimatorFrame = Skill2ShootFrame;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
			{
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3HideTime * 20f);
				Skill3UseFx1.Play();
				IsInvincible = true;
				BodyMesh.gameObject.SetActive(false);
				SkinnedMeshRenderer[] skill1TailsMesh = Skill1TailsMesh;
				for (int i = 0; i < skill1TailsMesh.Length; i++)
				{
					skill1TailsMesh[i].gameObject.SetActive(false);
				}
				SetColliderEnable(false);
				break;
			}
			case SubStatus.Phase2:
			{
				bool num = OrangeBattleUtility.Random(0, 20) / 10 == 0;
				bool flag = OrangeBattleUtility.Random(0, 20) / 10 == 0;
				Vector3 position = MinPos + Vector3.right;
				if (num)
				{
					position.x = MinPos.x + Skill3XFloat;
					UpdateDirection(1);
				}
				else
				{
					position.x = MaxPos.x - Skill3XFloat;
					UpdateDirection(-1);
				}
				if (flag)
				{
					position.y = MaxPos.y;
					SetWallSide(1);
				}
				else
				{
					position.y = MinPos.y;
					SetWallSide(2);
				}
				_transform.position = position;
				Controller.LogicPosition = new VInt3(NowPos);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill3ShowTime * 20f);
				Skill3UseFx2.Play();
				break;
			}
			case SubStatus.Phase3:
			{
				IsInvincible = false;
				BodyMesh.gameObject.SetActive(true);
				SkinnedMeshRenderer[] skill1TailsMesh = Skill1TailsMesh;
				for (int i = 0; i < skill1TailsMesh.Length; i++)
				{
					skill1TailsMesh[i].gameObject.SetActive(true);
				}
				SetColliderEnable(true);
				break;
			}
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (bPlayLoopSE)
				{
					PlayBossSE("BossSE05", "bs044_magne01_stop");
					bPlayLoopSE = false;
				}
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
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
			default:
				return;
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
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
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
				PlayBossSE("BossSE05", "bs044_magne09");
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP;
				break;
			case SubStatus.Phase3:
				PlayBossSE("BossSE05", "bs044_magne08");
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL3_END;
				break;
			case SubStatus.Phase2:
				return;
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
				mainStatus = (MainStatus)RandomWeight(2);
				if ((mainStatus == MainStatus.Skill1 || mainStatus == MainStatus.Skill0) && Skill1CanTailAtk)
				{
					mainStatus = MainStatus.Idle;
				}
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
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (bWaitNetStatus || IdleWaitFrame >= GameLogicUpdateManager.GameFrame)
			{
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!Target)
			{
				foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
				{
					if (runPlayer.IsAlive())
					{
						Target = runPlayer;
					}
				}
			}
			if ((bool)Target)
			{
				UpdateDirection();
				UpdateRandomState();
			}
			else
			{
				UpdateRandomState(MainStatus.Skill2);
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
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					if (Skill0NeedRecover)
					{
						HurtPassParam hurtPassParam = new HurtPassParam();
						int num = (int)MaxHp / (100 / Skill0RecoverPercent);
						if ((int)Hp + num > (int)MaxHp)
						{
							num = (int)MaxHp - (int)Hp;
						}
						hurtPassParam.dmg = -num;
						Hurt(hurtPassParam);
					}
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
					break;
				}
				if (!CatchTool.IsCatching)
				{
					Collider2D collider2D = Physics2D.OverlapBox(CatchTool.CatchTransform.position, Vector2.one * 1.5f, 0f, LayerMask.GetMask("Player"));
					if ((bool)collider2D && (int)Hp > 0)
					{
						OrangeCharacter component = collider2D.GetComponent<OrangeCharacter>();
						if (!component.IsUnBreakX())
						{
							CatchTool.CatchTarget(component);
						}
						Skill0NeedRecover = true;
						PlayBossSE("BossSE05", "bs044_magne02");
					}
				}
				if (CatchTool.IsCatching)
				{
					CatchTool.MoveTarget();
					break;
				}
				foreach (OrangeCharacter runPlayer2 in StageUpdate.runPlayers)
				{
					if ((int)runPlayer2.Hp > 0)
					{
						VInt3 vInt = new VInt3((CatchTool.CatchTransform.position - runPlayer2.GetTargetPoint()).normalized * Skill0GravityForce * Skill0Gravitational) / 1000f;
						runPlayer2.AddForce(new VInt3(vInt.x, vInt.y, 0));
						int num2 = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
						runPlayer2.AddForceFieldProxy(new VInt3(0, -num2 - 1, 0));
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
				if (_currentFrame > 0.1f && !Skill1TailsMesh[0].enabled)
				{
					Skill1TailsMesh[0].enabled = true;
				}
				else if (_currentFrame > 0.3f && !Skill1TailsMesh[1].enabled)
				{
					Skill1TailsMesh[1].enabled = true;
				}
				else if (_currentFrame > 0.5f && !Skill1TailsMesh[2].enabled)
				{
					Skill1TailsMesh[2].enabled = true;
				}
				else if (_currentFrame > 0.7f && !Skill1TailsMesh[3].enabled)
				{
					Skill1TailsMesh[3].enabled = true;
				}
				else if (_currentFrame > 0.9f && !Skill1TailsMesh[4].enabled)
				{
					Skill1TailsMesh[4].enabled = true;
				}
				else if (_currentFrame > 1f)
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					EndPos = GetTargetPos();
					UpdateDirection();
					Vector3 vector = EndPos - ShootPos.position;
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos.position, vector, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos.position, Quaternion.Euler(0f, 0f, 20f) * vector, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					switch (_wallside)
					{
					case WallSide.Up:
						BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos.position, Quaternion.Euler(0f, 0f, -20f) * vector, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						break;
					case WallSide.Down:
						BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos.position, Quaternion.Euler(0f, 0f, -40f) * vector, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						break;
					}
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 3f)
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
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
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
		}
		if (!Skill1CanTailAtk || GameLogicUpdateManager.GameFrame <= Skill1AtkFrame)
		{
			return;
		}
		int num3 = 0;
		int num4 = -1;
		for (int i = 0; i < Skill1TailAtkTimes.Length; i++)
		{
			if (Skill1Tails[i].hasHit)
			{
				Skill1TailAtkTimes[i] = 0;
			}
			else if (Skill1TailAtkTimes[i] > num3)
			{
				num4 = i;
				num3 = Skill1TailAtkTimes[i];
			}
		}
		if (num4 > -1)
		{
			Skill1Tails[num4].SetAtkPos(GetTargetPos());
			Skill1TailAtkTimes[num4]--;
			Skill1AtkFrame = GameLogicUpdateManager.GameFrame + (int)(Skill1AtkInterrval * 20f);
		}
		else
		{
			Skill1CanTailAtk = false;
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

	private int RandomWeight(int SkillStart)
	{
		int num = 0;
		int num2 = 0;
		int num3 = DefaultSkillWeight.Length;
		for (int i = 0; i < num3; i++)
		{
			num2 += DefaultSkillWeight[i];
		}
		int num4 = OrangeBattleUtility.Random(0, num2);
		for (int j = 0; j < num3; j++)
		{
			num += DefaultSkillWeight[j];
			if (num4 < num)
			{
				return j + SkillStart;
			}
		}
		return 0;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
		{
			runPlayer.PlayerResetPressJump();
		}
		Skill1CanTailAtk = false;
		EM196_Controller[] skill1Tails = Skill1Tails;
		foreach (EM196_Controller eM196_Controller in skill1Tails)
		{
			if (eM196_Controller != null)
			{
				eM196_Controller.SetSuicide();
			}
		}
		if (CatchTool.IsCatching)
		{
			CatchTool.ReleaseTarget();
		}
		StageUpdate.SlowStage();
		SetColliderEnable(false);
		SetStatus(MainStatus.Die);
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
		return NowPos + Vector3.up * 1f * base.direction;
	}

	private EM196_Controller SpawnEnemy(Vector3 SpawnPos)
	{
		int num = 3;
		MOB_TABLE enemy = GetEnemy((int)EnemyWeapons[num].BulletData.f_EFFECT_X);
		if (enemy == null)
		{
			Debug.LogError("要生成的怪物資料有誤，生怪技能ID " + num + " 怪物GroupID " + EnemyWeapons[num].BulletData.f_EFFECT_X);
			return null;
		}
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(enemy, sNetSerialID + SpawnCount);
		SpawnCount++;
		if ((bool)enemyControllerBase)
		{
			enemyControllerBase.SetPositionAndRotation(SpawnPos, base.direction == 1);
			enemyControllerBase.SetActive(true);
		}
		return enemyControllerBase as EM196_Controller;
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

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y + 1f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.up, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D4 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.down, 30f, layerMask, _transform);
		bool flag = false;
		if (!raycastHit2D)
		{
			flag = true;
			Debug.LogError("沒有偵測到左邊界，之後一些技能無法準確判斷位置");
		}
		if (!raycastHit2D2)
		{
			flag = true;
			Debug.LogError("沒有偵測到右邊界，之後一些技能無法準確判斷位置");
		}
		if (!raycastHit2D4)
		{
			flag = true;
			Debug.LogError("沒有偵測到地板，之後一些技能無法準確判斷位置");
		}
		if (!raycastHit2D3)
		{
			flag = true;
			Debug.LogError("沒有偵測到天花板，之後一些技能無法準確判斷位置");
		}
		if (flag)
		{
			MaxPos = new Vector3(NowPos.x + 3f, NowPos.y + 6f, 0f);
			MinPos = new Vector3(NowPos.x - 6f, NowPos.y, 0f);
		}
		else
		{
			MaxPos = new Vector3(raycastHit2D2.point.x, raycastHit2D3.point.y, 0f);
			MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D4.point.y, 0f);
		}
	}

	public void SetWallSide(int wallside = 0)
	{
		Vector3 localScale = Vector3.one;
		switch ((WallSide)wallside)
		{
		case WallSide.Up:
			localScale = new Vector3(ModelTransform.localScale.x, -1.2f, ModelTransform.localScale.z);
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(0f, -1f);
			base.AimPoint = new Vector3(0f, -0.8f, 0f);
			CatchTool.PosOffset = Vector3.down * 0.8f;
			break;
		case WallSide.Down:
			localScale = new Vector3(ModelTransform.localScale.x, 1.2f, ModelTransform.localScale.z);
			_wallside = (WallSide)wallside;
			Controller.Collider2D.offset = new Vector2(0f, 1f);
			base.AimPoint = new Vector3(0f, 0.8f, 0f);
			CatchTool.PosOffset = Vector3.down * 0.5f;
			break;
		case WallSide.Left:
		case WallSide.Right:
			Debug.Log("BS075 不該爬左右牆壁");
			break;
		}
		ModelTransform.localScale = localScale;
	}

	protected override void UpdateGravity()
	{
		if (IgnoreGravity)
		{
			return;
		}
		switch (_wallside)
		{
		case WallSide.Up:
		case WallSide.Down:
			if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
			{
				_velocity.y = 0;
			}
			break;
		case WallSide.Left:
		case WallSide.Right:
			if ((_velocity.x < 0 && Controller.Collisions.left) || (_velocity.x > 0 && Controller.Collisions.right))
			{
				_velocity.x = 0;
			}
			break;
		}
		int num = 0;
		num = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
		switch (_wallside)
		{
		case WallSide.Up:
			_velocity.y -= num;
			break;
		case WallSide.Down:
			_velocity.y += num;
			break;
		case WallSide.Left:
			_velocity.x += num;
			break;
		case WallSide.Right:
			_velocity.x -= num;
			break;
		}
	}
}
