#define RELEASE
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS079_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private class Skill0BallData
	{
		public readonly int LifeFrame;

		public int ShootTime;

		public int CDFrame;

		public Skill0BallData(int life, int times, int cd)
		{
			LifeFrame = life;
			ShootTime = times;
			CDFrame = cd;
		}

		public void ResetCD(int cd)
		{
			CDFrame = cd;
			ShootTime--;
		}
	}

	private enum Element
	{
		Normal = 0,
		Green = 1,
		Yellow = 2,
		Blue = 3,
		Red = 4
	}

	private enum SkillPattern
	{
		State1 = 1,
		State2 = 2,
		State3 = 3,
		State4 = 4,
		State5 = 5
	}

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
		MAX_SUBSTATUS = 6
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_DEBUT_LOOP = 2,
		ANI_MOVE = 3,
		ANI_SKILL0_START1 = 4,
		ANI_SKILL0_LOOP1 = 5,
		ANI_SKILL0_START2 = 6,
		ANI_SKILL0_LOOP2 = 7,
		ANI_SKILL0_END2 = 8,
		ANI_SKILL1_START1 = 9,
		ANI_SKILL1_LOOP1 = 10,
		ANI_SKILL1_START2 = 11,
		ANI_SKILL1_LOOP2 = 12,
		ANI_SKILL1_END2 = 13,
		ANI_SKILL2_START1 = 14,
		ANI_SKILL2_LOOP1 = 15,
		ANI_SKILL2_START2 = 16,
		ANI_SKILL2_LOOP2 = 17,
		ANI_SKILL2_END2 = 18,
		ANI_SKILL3_START = 19,
		ANI_SKILL3_LOOP = 20,
		ANI_SKILL3_END = 21,
		ANI_SKILL4_START = 22,
		ANI_SKILL4_END = 23,
		ANI_HURT = 24,
		ANI_DEAD_START = 25,
		ANI_DEAD_LOOP = 26,
		ANI_DEAD_END = 27,
		MAX_ANIMATION_ID = 28
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

	private int[] DefaultSkillCard = new int[6] { 0, 1, 2, 3, 4, 5 };

	private static int[] DefaultRangedSkillCard = new int[3] { 3, 4, 5 };

	private List<int> RangedSKC = new List<int>();

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	private float MoveDis;

	[SerializeField]
	private Transform ShootPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private Vector3 CenterPos;

	private float GroundYPos;

	private int ActionTimes;

	private float ActionAnimatorFrame;

	private int ActionFrame;

	private bool HasActed;

	private int SpawnCount;

	[SerializeField]
	private SkinnedMeshRenderer DebutMesh;

	[SerializeField]
	private SkinnedMeshRenderer BattleMesh;

	[SerializeField]
	private SkinnedMeshRenderer MantleMesh;

	[SerializeField]
	private SkinnedMeshRenderer GemMesh;

	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("噩夢能源球")]
	[SerializeField]
	private int Skill0ShootTimes = 2;

	[SerializeField]
	private float Skill0ShootFrame = 0.4f;

	[SerializeField]
	private CharacterMaterial GemMaterial;

	private Element UseElement;

	[SerializeField]
	private ParticleSystem[] Skilll0UseFX = new ParticleSystem[5];

	[Header("黑色球")]
	[SerializeField]
	private float BlackLifeTime = 5f;

	[SerializeField]
	private float BlackCDTime = 2.5f;

	[SerializeField]
	private int MaxEnemyNum = 6;

	private float BlackPosAngle = 140f;

	private float EM158PosAngle = 130f;

	private List<ValueTuple<LocateBullet, Skill0BallData>> BlackList = new List<ValueTuple<LocateBullet, Skill0BallData>>();

	[Header("黃色球")]
	[SerializeField]
	private float YellowLifeTime = 5f;

	[SerializeField]
	private float YellowCDTime = 1.25f;

	private List<ValueTuple<LocateBullet, Skill0BallData>> YellowList = new List<ValueTuple<LocateBullet, Skill0BallData>>();

	[Header("藍色球")]
	[SerializeField]
	private float BlueLifeTime = 5f;

	[SerializeField]
	private float gravitational = 100f;

	[SerializeField]
	private Vector2 GravityForce = new Vector2(3f, 0.8f);

	private List<ValueTuple<LocateBullet, Skill0BallData>> BlueList = new List<ValueTuple<LocateBullet, Skill0BallData>>();

	[Header("黑洞")]
	[SerializeField]
	private int Skill1ShootTimes = 1;

	[SerializeField]
	private float Skill1ShootFrame = 0.4f;

	[SerializeField]
	private ParticleSystem Skill1UseFX1;

	[SerializeField]
	private ParticleSystem Skill1UseFX2;

	[SerializeField]
	private ParticleSystem Skill1UseFX3;

	[Header("突進")]
	[SerializeField]
	private float Skill2ShootFrame = 0.4f;

	[SerializeField]
	private int Skill2DashSpeed = 12500;

	[Header("鏡射")]
	[SerializeField]
	private ParticleSystem Skill3UseFX;

	private bool hasCopy;

	[Header("瞬移")]
	[SerializeField]
	private ParticleSystem Skill4UseFX;

	[Header("飛行")]
	[SerializeField]
	private int FlySpeed = 7500;

	[Header("AI控制")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SkillPattern skillpattern = SkillPattern.State1;

	[SerializeField]
	private bool isEXMode;

	[SerializeField]
	private float AIXDis = 6f;

	private int Skill1UseTime;

	private int Skill2UseTime;

	private int Skill5UseTime;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

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
			case "Skill4":
				NextSkill = MainStatus.Skill4;
				break;
			case "Skill5":
				NextSkill = MainStatus.Skill5;
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
		_animationHash = new int[28];
		_animationHash[0] = Animator.StringToHash("BS079@idl_loop");
		_animationHash[1] = Animator.StringToHash("BS079@debut");
		_animationHash[2] = Animator.StringToHash("BS079@debut_loop");
		_animationHash[3] = Animator.StringToHash("BS079@fly_loop");
		_animationHash[4] = Animator.StringToHash("BS079@GATE_SKILL1_CASTING1");
		_animationHash[5] = Animator.StringToHash("BS079@GATE_SKILL1_CASTLOOP1");
		_animationHash[6] = Animator.StringToHash("BS079@GATE_SKILL1_CASTING2");
		_animationHash[7] = Animator.StringToHash("BS079@GATE_SKILL1_CASTLOOP2");
		_animationHash[8] = Animator.StringToHash("BS079@GATE_SKILL1_CASTOUT1");
		_animationHash[9] = Animator.StringToHash("BS079@GATE_SKILL2_CASTING1");
		_animationHash[10] = Animator.StringToHash("BS079@GATE_SKILL2_CASTLOOP1");
		_animationHash[11] = Animator.StringToHash("BS079@GATE_SKILL2_CASTING2");
		_animationHash[12] = Animator.StringToHash("BS079@GATE_SKILL2_CASTLOOP2");
		_animationHash[13] = Animator.StringToHash("BS079@GATE_SKILL2_CASTOUT1");
		_animationHash[14] = Animator.StringToHash("BS079@GATE_SKILL3_CASTING1");
		_animationHash[15] = Animator.StringToHash("BS079@GATE_SKILL3_CASTLOOP1");
		_animationHash[16] = Animator.StringToHash("BS079@GATE_SKILL3_CASTING2");
		_animationHash[17] = Animator.StringToHash("BS079@GATE_SKILL3_CASTLOOP2");
		_animationHash[18] = Animator.StringToHash("BS079@GATE_SKILL3_CASTOUT1");
		_animationHash[19] = Animator.StringToHash("BS079@GATE_SKILL4_CASTING1");
		_animationHash[20] = Animator.StringToHash("BS079@GATE_SKILL4_CASTLOOP1");
		_animationHash[21] = Animator.StringToHash("BS079@GATE_SKILL4_CASTOUT1");
		_animationHash[22] = Animator.StringToHash("BS079@GATE_SKILL5_CASTING1");
		_animationHash[23] = Animator.StringToHash("BS079@GATE_SKILL5_CASTLOOP1");
		_animationHash[24] = Animator.StringToHash("BS079@hurt");
		_animationHash[25] = Animator.StringToHash("BS079@dead_start");
		_animationHash[26] = Animator.StringToHash("BS079@dead_loop");
		_animationHash[27] = Animator.StringToHash("BS079@dead_end");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPoint_Head", true);
		}
		if (MantleMesh == null)
		{
			MantleMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "CH079_MeshB", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (GemMesh == null)
		{
			GemMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "GemMesh_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (DebutMesh == null)
		{
			DebutMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS079_MeshB", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (BattleMesh == null)
		{
			BattleMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS079_MeshA", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (GemMaterial == null)
		{
			GemMaterial = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPoint_Head", true).GetComponent<CharacterMaterial>();
		}
		for (int i = 0; i < Skilll0UseFX.Length; i++)
		{
			if (Skilll0UseFX[i] == null)
			{
				Skilll0UseFX[i] = OrangeBattleUtility.FindChildRecursive(ref childs, "Skilll0UseFX" + i, true).GetComponent<ParticleSystem>();
			}
		}
		if (Skill1UseFX1 == null)
		{
			Skill1UseFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1UseFX1", true).GetComponent<ParticleSystem>();
		}
		if (Skill1UseFX2 == null)
		{
			Skill1UseFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1UseFX2", true).GetComponent<ParticleSystem>();
		}
		if (Skill1UseFX3 == null)
		{
			Skill1UseFX3 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1UseFX3", true).GetComponent<ParticleSystem>();
		}
		if (Skill3UseFX == null)
		{
			Skill3UseFX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill3UseFX", true).GetComponent<ParticleSystem>();
		}
		if (Skill4UseFX == null)
		{
			Skill4UseFX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill4UseFX", true).GetComponent<ParticleSystem>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimPoint = new Vector3(0f, 1.2f, 0f);
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
			if (subStatus2 != 0 && subStatus2 == SubStatus.Phase1)
			{
				HasActed = false;
			}
			break;
		}
		case MainStatus.Idle:
			if (((int)Controller.collisionMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer)) > 0)
			{
				Controller2D controller = Controller;
				controller.collisionMask = (int)controller.collisionMask - (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
				IgnoreGravity = true;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
				UpdateDirection();
			}
			_velocity = VInt3.zero;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				if (ActionTimes < 1)
				{
					ActionTimes = Skill0ShootTimes;
				}
				if (BlueList.Count < 1)
				{
					UseElement = (Element)(OrangeBattleUtility.Random(0, 40) / 10);
				}
				else
				{
					UseElement = (Element)(OrangeBattleUtility.Random(0, 30) / 10);
				}
				GemMaterial.UpdateTex((int)((UseElement != Element.Blue) ? UseElement : Element.Normal));
				EndPos = GetTargetPos();
				UpdateDirection();
				break;
			case SubStatus.Phase3:
				SwitchFx(Skilll0UseFX[(int)UseElement], true);
				ActionAnimatorFrame = Skill0ShootFrame;
				HasActed = false;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				UpdateDirection();
				SwitchFx(Skill1UseFX1, true);
				break;
			case SubStatus.Phase2:
				SwitchFx(Skill1UseFX2, true);
				ActionAnimatorFrame = Skill1ShootFrame;
				HasActed = false;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				UpdateDirection();
				break;
			case SubStatus.Phase2:
				PlayBossSE("BossSE05", "bs048_gate11");
				StartPos = NowPos;
				EndPos = GetTargetPos();
				UpdateDirection();
				MoveDis = Vector2.Distance(GetTargetPoint(), EndPos);
				ActionAnimatorFrame = Skill2ShootFrame;
				HasActed = false;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				UpdateDirection((!(NowPos.x > CenterPos.x)) ? 1 : (-1));
				break;
			case SubStatus.Phase1:
				SwitchFx(Skill3UseFX, true);
				PlayBossSE("BossSE05", "bs048_gate10");
				break;
			case SubStatus.Phase2:
				SwitchFx(Skill3UseFX, false);
				if (!hasCopy)
				{
					EM203_Controller eM203_Controller = SpawnEnemy(NowPos + Vector3.right * 1.5f * base.direction, 10) as EM203_Controller;
					if ((bool)eM203_Controller)
					{
						hasCopy = true;
						eM203_Controller.SetRoomSize(MaxPos, MinPos);
					}
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE05", "bs048_gate05");
				_velocity = VInt3.zero;
				HasActed = false;
				switch (skillpattern)
				{
				case SkillPattern.State3:
					EndPos = GetTargetPos();
					EndPos = new Vector3((EndPos.x > CenterPos.x) ? (MinPos.x + 3f) : (MaxPos.x - 3f), (EndPos.y > CenterPos.y) ? (MinPos.y + 3f) : (MaxPos.y - 3f), 0f);
					break;
				case SkillPattern.State4:
					if (!Target)
					{
						EndPos = GetTargetPos();
					}
					if ((bool)Target)
					{
						EndPos = Target._transform.position + Vector3.left * 2f * Target.direction;
					}
					if (EndPos.x > MaxPos.x || EndPos.x < MinPos.x)
					{
						EndPos = Target._transform.position + Vector3.right * 2f * Target.direction;
					}
					break;
				default:
					Debug.LogError("瞬移不該進來這裡");
					break;
				}
				break;
			case SubStatus.Phase1:
				_transform.position = EndPos;
				Controller.LogicPosition = new VInt3(EndPos);
				break;
			}
			break;
		case MainStatus.Skill5:
			if (_subStatus == SubStatus.Phase0)
			{
				StartPos = NowPos;
				EndPos = GetTargetPos();
				UpdateDirection();
				MoveDis = Vector2.Distance(NowPos, EndPos);
				_velocity = new VInt3((EndPos - NowPos).normalized) * FlySpeed * 0.001f;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE05", "bs048_gate03");
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				EndPos = GetTargetPos();
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				StartCoroutine(BossDieFlow(_transform.position, "FX_BOSS_EXPLODE2", false, false));
				break;
			case SubStatus.Phase3:
				_velocity.x = 2500 * -base.direction;
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
				_currentAnimationId = AnimationID.ANI_SKILL1_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL1_END2;
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
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL4_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL4_END;
				break;
			}
			break;
		case MainStatus.Skill5:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_MOVE;
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
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEAD_END;
				break;
			case SubStatus.Phase1:
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateNextState(MainStatus status = MainStatus.Idle)
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
				if (DebugMode)
				{
					mainStatus = NextSkill;
					break;
				}
				switch (skillpattern)
				{
				case SkillPattern.State1:
					if (isEXMode)
					{
						hasCopy = false;
						for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
						{
							EM203_Controller eM203_Controller = StageUpdate.runEnemys[i].mEnemy as EM203_Controller;
							if ((bool)eM203_Controller && !eM203_Controller.hasHide)
							{
								hasCopy = true;
								break;
							}
						}
					}
					mainStatus = ((!isEXMode || hasCopy) ? MainStatus.Skill0 : MainStatus.Skill3);
					skillpattern = SkillPattern.State2;
					Skill5UseTime = (Skill2UseTime = (Skill1UseTime = 0));
					break;
				case SkillPattern.State2:
					if (Skill5UseTime <= 2 && Skill2UseTime <= 2 && Skill1UseTime <= 2)
					{
						EndPos = GetTargetPos();
						if (Vector2.Distance(EndPos, NowPos) < AIXDis)
						{
							mainStatus = MainStatus.Skill5;
							if (!isEXMode)
							{
								Skill5UseTime = 0;
								skillpattern = SkillPattern.State1;
							}
							Skill5UseTime++;
						}
						else if (OrangeBattleUtility.Random(0, 10) < 5)
						{
							mainStatus = MainStatus.Skill4;
							skillpattern = SkillPattern.State3;
							Skill2UseTime++;
						}
						else
						{
							mainStatus = MainStatus.Skill4;
							skillpattern = SkillPattern.State4;
							Skill1UseTime++;
						}
						break;
					}
					goto case SkillPattern.State1;
				case SkillPattern.State3:
					if (!isEXMode)
					{
						Skill2UseTime = 0;
						goto case SkillPattern.State1;
					}
					goto case SkillPattern.State2;
				case SkillPattern.State4:
					if (!isEXMode)
					{
						Skill1UseTime = 0;
						goto case SkillPattern.State1;
					}
					goto case SkillPattern.State2;
				}
				break;
			}
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
				if (!HasActed && _currentFrame > 0.4f)
				{
					HasActed = true;
					_characterMaterial.UpdateTex(0);
					SwitchMesh(DebutMesh, false);
					SwitchMesh(BattleMesh, true);
					SwitchMesh(MantleMesh, true);
				}
				if (MantleMesh != null && MantleMesh.enabled && _currentFrame > 0.478f)
				{
					SwitchMesh(MantleMesh, false);
				}
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
						UpdateNextState();
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
					UpdateNextState();
				}
				else
				{
					UpdateNextState(MainStatus.Skill2);
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
				if (_currentFrame > 1f)
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					SwitchFx(Skilll0UseFX[(int)UseElement], false);
					switch (UseElement)
					{
					case Element.Normal:
					{
						EndPos = CenterPos + Quaternion.Euler(0f, 0f, BlackPosAngle) * (Vector3.up * 2.5f);
						BlackPosAngle += BlackPosAngle;
						TargetPos = new VInt3(EndPos);
						UpdateDirection();
						Vector3 pDirection = EndPos - ShootPos.position;
						LocateBullet locateBullet2 = BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, ShootPos, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as LocateBullet;
						locateBullet2.FreeDISTANCE = Vector2.Distance(EndPos, ShootPos.position);
						locateBullet2.SetEndPos(EndPos);
						BlackList.Add(new ValueTuple<LocateBullet, Skill0BallData>(locateBullet2, new Skill0BallData(GameLogicUpdateManager.GameFrame + (int)(BlackLifeTime * 20f), (int)(BlackLifeTime / BlackCDTime), GameLogicUpdateManager.GameFrame + (int)(BlackCDTime * 20f))));
						break;
					}
					case Element.Green:
						(BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, ShootPos, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BasicBullet).BackCallback = GreenBallBoom;
						break;
					case Element.Yellow:
					{
						EndPos = GetTargetPos(true);
						UpdateDirection();
						Vector3 pDirection = EndPos - ShootPos.position;
						LocateBullet locateBullet3 = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as LocateBullet;
						locateBullet3.FreeDISTANCE = Vector2.Distance(EndPos, ShootPos.position);
						locateBullet3.SetEndPos(EndPos);
						YellowList.Add(new ValueTuple<LocateBullet, Skill0BallData>(locateBullet3, new Skill0BallData(GameLogicUpdateManager.GameFrame + (int)(YellowLifeTime * 20f), (int)(YellowLifeTime / YellowCDTime), GameLogicUpdateManager.GameFrame + (int)(YellowCDTime * 20f))));
						break;
					}
					case Element.Blue:
					{
						switch (OrangeBattleUtility.Random(0, 30) / 10)
						{
						case 0:
							EndPos = new Vector3(MinPos.x + 1.5f, MinPos.y + 1.5f, 0f);
							break;
						case 1:
							EndPos = new Vector3(MaxPos.x - 1.5f, MinPos.y + 1.5f, 0f);
							break;
						case 2:
							EndPos = new Vector3(CenterPos.x, MaxPos.y - 1.5f, 0f);
							break;
						}
						TargetPos = new VInt3(EndPos);
						UpdateDirection();
						Vector3 pDirection = EndPos - ShootPos.position;
						LocateBullet locateBullet = BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, ShootPos, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as LocateBullet;
						locateBullet.FreeDISTANCE = Vector2.Distance(EndPos, ShootPos.position);
						locateBullet.SetEndPos(EndPos);
						BlueList.Add(new ValueTuple<LocateBullet, Skill0BallData>(locateBullet, new Skill0BallData(GameLogicUpdateManager.GameFrame + (int)(BlueLifeTime * 20f), (int)(BlueLifeTime / BlueLifeTime), GameLogicUpdateManager.GameFrame + (int)(BlueLifeTime * 20f))));
						break;
					}
					}
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill0);
					}
					else
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase4);
					}
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					(BulletBase.TryShotBullet(EnemyWeapons[8].BulletData, Skill1UseFX3.transform.position, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					_velocity = new VInt3((EndPos - GetTargetPoint()).normalized) * Skill2DashSpeed * 0.001f;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Vector2.Distance(GetTargetPoint(), StartPos) > MoveDis)
				{
					_velocity = VInt3.zero;
					_transform.position = EndPos;
					Controller.LogicPosition = new VInt3(EndPos);
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
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
					SetStatus(MainStatus.Skill0);
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!HasActed && _currentFrame > 0.7f)
				{
					HasActed = true;
					SwitchFx(Skill4UseFX, true);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					switch (skillpattern)
					{
					case SkillPattern.State3:
						SetStatus(MainStatus.Skill2);
						break;
					case SkillPattern.State4:
						SetStatus(MainStatus.Skill1);
						break;
					default:
						SetStatus(MainStatus.Idle);
						break;
					}
				}
				break;
			}
			break;
		case MainStatus.Skill5:
			if (_subStatus == SubStatus.Phase0 && Vector2.Distance(StartPos, NowPos) > MoveDis)
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 5f)
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
				if (_currentFrame > 0.4f)
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
			}
			break;
		}
		CheckBlueList();
		CheckBlackList();
		CheckYellowList();
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
			_characterMaterial.UpdateTex(1);
			SwitchMesh(DebutMesh, true);
			SwitchMesh(BattleMesh, false);
			SwitchMesh(MantleMesh, false);
			CheckRoomSize();
			IgnoreGravity = true;
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
		}
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
		BlackList.Clear();
		YellowList.Clear();
		BlueList.Clear();
		ParticleSystem[] skilll0UseFX = Skilll0UseFX;
		foreach (ParticleSystem fx in skilll0UseFX)
		{
			SwitchFx(fx, false);
		}
		SwitchFx(Skill1UseFX1, false);
		SwitchFx(Skill1UseFX2, false);
		SwitchFx(Skill1UseFX3, false);
		SwitchFx(Skill3UseFX, false);
		SwitchFx(Skill4UseFX, false);
		List<EM203_Controller> list = new List<EM203_Controller>();
		for (int j = 0; j < StageUpdate.runEnemys.Count; j++)
		{
			EM203_Controller eM203_Controller = StageUpdate.runEnemys[j].mEnemy as EM203_Controller;
			if ((bool)eM203_Controller)
			{
				PlayBossSE("BossSE", "bs048_gate10");
				list.Add(eM203_Controller);
			}
		}
		foreach (EM203_Controller item in list)
		{
			item.SetDie();
		}
		List<EM158_Controller> list2 = new List<EM158_Controller>();
		for (int k = 0; k < StageUpdate.runEnemys.Count; k++)
		{
			EM158_Controller eM158_Controller = StageUpdate.runEnemys[k].mEnemy as EM158_Controller;
			if ((bool)eM158_Controller)
			{
				list2.Add(eM158_Controller);
			}
		}
		foreach (EM158_Controller item2 in list2)
		{
			item2.SetDie();
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
		return NowPos + Vector3.right * 3f * base.direction;
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x - 4f, _transform.position.y + 1f, 0f);
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
			CenterPos = (MaxPos + MinPos) / 2f;
		}
	}

	private EnemyControllerBase SpawnEnemy(Vector3 SpawnPos, int weaponid)
	{
		int num = weaponid;
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
			EM158_Controller eM158_Controller = enemyControllerBase as EM158_Controller;
			if ((bool)eM158_Controller)
			{
				eM158_Controller.SetNextPos(SpawnPos + Quaternion.Euler(0f, 0f, EM158PosAngle) * (Vector3.up * 1f));
				EM158PosAngle += EM158PosAngle;
			}
			enemyControllerBase.SetPositionAndRotation(SpawnPos, base.direction == -1);
			enemyControllerBase.SetActive(true);
		}
		return enemyControllerBase;
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

	private void SwitchFx(ParticleSystem Fx, bool onoff)
	{
		if ((bool)Fx)
		{
			if (onoff)
			{
				Fx.Play();
				return;
			}
			Fx.Stop();
			Fx.Clear();
		}
		else
		{
			Debug.Log(string.Concat("特效載入有誤，目前狀態是 ", _mainStatus, "的階段 ", _subStatus));
		}
	}

	private void CheckBlackList()
	{
		if (BlackList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < BlackList.Count; i++)
		{
			if (!BlackList[i].Item1.isHasArrived)
			{
				BlackList[i].Item2.ResetCD(GameLogicUpdateManager.GameFrame + 20);
			}
			else if (BlackList[i].Item1.bIsEnd)
			{
				BlackList[i].Item1.SetBackToPool();
				BlackList.Remove(BlackList[i]);
				i--;
			}
			else if (GameLogicUpdateManager.GameFrame > BlackList[i].Item2.CDFrame)
			{
				if (MaxEnemyNum > StageUpdate.runEnemys.Count)
				{
					SpawnEnemy(BlackList[i].Item1._transform.position, 4);
				}
				BlackList[i].Item2.ResetCD(GameLogicUpdateManager.GameFrame + (int)(BlackCDTime * 20f));
			}
		}
	}

	private void CheckYellowList()
	{
		if (YellowList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < YellowList.Count; i++)
		{
			if (!YellowList[i].Item1.isHasArrived)
			{
				YellowList[i].Item2.ResetCD(GameLogicUpdateManager.GameFrame + 20);
			}
			else if (YellowList[i].Item1.bIsEnd)
			{
				YellowList[i].Item1.SetBackToPool();
				YellowList.Remove(YellowList[i]);
				i--;
			}
			else if ((bool)Target && GameLogicUpdateManager.GameFrame > YellowList[i].Item2.CDFrame)
			{
				PlayBossSE("BossSE05", "bs048_gate13");
				Vector3 position = YellowList[i].Item1._transform.position;
				BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, position, Target._transform.position - position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				YellowList[i].Item2.ResetCD(GameLogicUpdateManager.GameFrame + (int)(YellowCDTime * 20f));
			}
		}
	}

	private void CheckBlueList()
	{
		if (BlueList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < BlueList.Count; i++)
		{
			if (!BlueList[i].Item1.isHasArrived)
			{
				continue;
			}
			if (BlueList[i].Item1.bIsEnd)
			{
				BlueList[i].Item1.SetBackToPool();
				BlueList.Remove(BlueList[i]);
				i--;
				continue;
			}
			foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
			{
				if ((int)runPlayer.Hp > 0)
				{
					VInt3 vInt = new VInt3((BlueList[i].Item1._transform.position - runPlayer._transform.position).normalized * GravityForce * gravitational) / 1000f;
					runPlayer.AddForce(new VInt3(vInt.x, 0, 0));
					runPlayer.AddForceFieldProxy(new VInt3(0, vInt.y, 0));
				}
			}
		}
	}

	private void GreenBallBoom(object obj)
	{
		BulletBase bulletBase = null;
		if (obj != null)
		{
			bulletBase = obj as BulletBase;
		}
		if (bulletBase == null)
		{
			Debug.LogError("子彈資料有誤。");
			return;
		}
		PlayBossSE("BossSE05", "bs048_gate07");
		for (int i = 1; i < 4; i++)
		{
			Vector3 pDirection = Quaternion.Euler(0f, 0f, i * 45) * Vector3.up;
			BulletBase.TryShotBullet(EnemyWeapons[7].BulletData, bulletBase._transform.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
			pDirection = Quaternion.Euler(0f, 0f, i * 45) * Vector3.up;
			pDirection = Quaternion.Euler(0f, 0f, -i * 45) * Vector3.up;
			BulletBase.TryShotBullet(EnemyWeapons[7].BulletData, bulletBase._transform.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		}
	}

	private void SwitchMesh(SkinnedMeshRenderer Mesh, bool onoff)
	{
		if ((bool)Mesh)
		{
			Mesh.enabled = onoff;
			return;
		}
		Debug.Log(string.Concat("Mesh載入有誤，目前狀態是 ", _mainStatus, "的階段 ", _subStatus));
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if (!isEXMode && (int)obscuredInt <= (int)MaxHp / 2)
		{
			skillpattern = SkillPattern.State1;
			isEXMode = true;
		}
		return obscuredInt;
	}
}
