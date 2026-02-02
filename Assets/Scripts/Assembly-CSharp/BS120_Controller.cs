#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS120_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
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
		IdleNoTarget = 7,
		Die = 8
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
		MAX_SUBSTATUS = 9
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_IDLE_SKY = 1,
		ANI_DEBUT = 2,
		ANI_DEBUT_LOOP = 3,
		ANI_RUN = 4,
		ANI_CALL = 5,
		ANI_FALL = 6,
		ANI_LAND = 7,
		ANI_SKILL0_START = 8,
		ANI_SKILL0_LOOP = 9,
		ANI_SKILL0_END = 10,
		ANI_SKILL1_START = 11,
		ANI_SKILL1_LOOP = 12,
		ANI_SKILL1_END = 13,
		ANI_SKILL2_START = 14,
		ANI_SKILL2_LOOP = 15,
		ANI_SKILL2_END = 16,
		ANI_SKILL3_START1 = 17,
		ANI_SKILL3_LOOP1 = 18,
		ANI_SKILL3_START2 = 19,
		ANI_SKILL3_LOOP2 = 20,
		ANI_SKILL3_START3 = 21,
		ANI_SKILL3_LOOP3 = 22,
		ANI_SKILL3_END3 = 23,
		ANI_SKILL4_START = 24,
		ANI_SKILL4_LOOP = 25,
		ANI_SKILL4_END = 26,
		ANI_SKILL4_END2 = 27,
		ANI_HURT = 28,
		ANI_DEAD = 29,
		MAX_ANIMATION_ID = 30
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

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	private float fMoveDis;

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

	private float ShotAngle;

	private float NextAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[Header("空氣爆破機關槍")]
	[SerializeField]
	private string sSk0UseFX;

	[SerializeField]
	private int nSk0AtkTimes = 10;

	[SerializeField]
	private float fSk0ActTime1 = 1f;

	[SerializeField]
	private float fSk0ActTime2 = 0.1f;

	[SerializeField]
	private float fSk0AtkSpacing = 1f;

	[SerializeField]
	private ParticleSystem psSk0UseFX1;

	[SerializeField]
	private ParticleSystem psSk0UseFX2;

	[Header("爆發")]
	[SerializeField]
	private float fSk1ActFrame = 0.8f;

	[SerializeField]
	private int nSk1AtkTimes = 3;

	[SerializeField]
	private ParticleSystem psSk1UseFX;

	private List<BulletBase> Sk1Bullets = new List<BulletBase>();

	[Header("光束刃")]
	[SerializeField]
	private float fSk2ActFrame = 0.7f;

	[SerializeField]
	private float fSk2ActTime = 1f;

	[SerializeField]
	private int nSk2AtkTimes = 2;

	[SerializeField]
	private ParticleSystem psSk2UseFX1;

	[SerializeField]
	private ParticleSystem psSk2UseFX2;

	private CollideBullet SwordCollider;

	[SerializeField]
	private string sSwordColliderObj = "SwordCollider";

	[SerializeField]
	private ParticleSystem SwordFX;

	[SerializeField]
	private SkinnedMeshRenderer SwordMesh;

	[SerializeField]
	private SkinnedMeshRenderer BodyMesh;

	[SerializeField]
	private SkinnedMeshRenderer GunMeshL;

	[SerializeField]
	private SkinnedMeshRenderer GunMeshR;

	[SerializeField]
	private SkinnedMeshRenderer CloakMesh;

	[SerializeField]
	private SkinnedMeshRenderer HandMeshL;

	[SerializeField]
	private SkinnedMeshRenderer HandMeshR;

	[Header("爆發衝擊")]
	[SerializeField]
	private int nSk3ActTimes = 3;

	[SerializeField]
	private float fSk3Dis = 20f;

	[SerializeField]
	private float fSk3ActTime = 1f;

	[SerializeField]
	private float fSk3FloatTime = 1f;

	[SerializeField]
	private int nSk3floatSpd = 5000;

	[SerializeField]
	private int nSk3Spd = 15000;

	[SerializeField]
	private ParticleSystem psSk3UseFX;

	private CollideBulletHitSelf BuffCollide;

	[Header("衝刺")]
	[SerializeField]
	private int nSk4Spd = 15000;

	[SerializeField]
	private string sSk4UseFX;

	[Header("待機等待Frame")]
	[SerializeField]
	private float IdleWaitTime = 0.5f;

	private int IdleWaitFrame;

	[Header("AI控制")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SkillPattern skillPattern = SkillPattern.State1;

	private SkillPattern nextState = SkillPattern.State1;

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private const float fBias = 0.01f;

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

	private void PlayBoss6SE(string cue)
	{
		PlaySE("BossSE06", cue);
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
		_animationHash = new int[30];
		_animationHash[0] = Animator.StringToHash("BS120@IDLE");
		_animationHash[1] = Animator.StringToHash("BS120@IDLE_SKY");
		_animationHash[2] = Animator.StringToHash("BS120@LOGIN");
		_animationHash[3] = Animator.StringToHash("BS120@LOGIN_LOOP");
		_animationHash[4] = Animator.StringToHash("BS120@RUN");
		_animationHash[5] = Animator.StringToHash("BS120@CALL");
		_animationHash[6] = Animator.StringToHash("saber_fall_loop");
		_animationHash[7] = Animator.StringToHash("saber_landing");
		_animationHash[8] = Animator.StringToHash("BS120@FORTE-EXE_SKILL1_CASTING");
		_animationHash[9] = Animator.StringToHash("BS120@FORTE-EXE_SKILL1_CASTLOOP");
		_animationHash[10] = Animator.StringToHash("BS120@FORTE-EXE_SKILL1_CASTOUT");
		_animationHash[11] = Animator.StringToHash("BS120@FORTE-EXE_SKILL2_CASTING1");
		_animationHash[12] = Animator.StringToHash("BS120@FORTE-EXE_SKILL2_CASTLOOP1");
		_animationHash[13] = Animator.StringToHash("BS120@FORTE-EXE_SKILL2_CASTOUT2");
		_animationHash[14] = Animator.StringToHash("BS120@FORTE-EXE_SKILL3_CASTING1");
		_animationHash[15] = Animator.StringToHash("BS120@FORTE-EXE_SKILL3_CASTLOOP1");
		_animationHash[16] = Animator.StringToHash("BS120@FORTE-EXE_SKILL3_CASTOUT1");
		_animationHash[17] = Animator.StringToHash("BS120@FORTE-EXE_SKILL4_CASTING1");
		_animationHash[18] = Animator.StringToHash("BS120@FORTE-EXE_SKILL4_CASTLOOP1");
		_animationHash[19] = Animator.StringToHash("BS120@FORTE-EXE_SKILL4_CASTING2");
		_animationHash[20] = Animator.StringToHash("BS120@FORTE-EXE_SKILL4_CASTLOOP2");
		_animationHash[21] = Animator.StringToHash("BS120@FORTE-EXE_SKILL4_CASTING3");
		_animationHash[22] = Animator.StringToHash("BS120@FORTE-EXE_SKILL4_CASTLOOP3");
		_animationHash[23] = Animator.StringToHash("BS120@FORTE-EXE_SKILL4_CASTOUT2");
		_animationHash[24] = Animator.StringToHash("BS120@SPRINT_START");
		_animationHash[25] = Animator.StringToHash("BS120@SPRINT_LOOP");
		_animationHash[26] = Animator.StringToHash("BS120@SPRINT_END");
		_animationHash[27] = Animator.StringToHash("BS120@SPRINT_END_SKY");
		_animationHash[28] = Animator.StringToHash("hurt_loop");
		_animationHash[29] = Animator.StringToHash("BS120@DEAD");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		SwordCollider = OrangeBattleUtility.FindChildRecursive(ref childs, sSwordColliderObj, true).gameObject.AddOrGetComponent<CollideBullet>();
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 Neck", true);
		}
		if (psSk0UseFX1 == null)
		{
			psSk0UseFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSk0UseFX1", true).GetComponent<ParticleSystem>();
		}
		if (psSk0UseFX2 == null)
		{
			psSk0UseFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSk0UseFX2", true).GetComponent<ParticleSystem>();
		}
		if (psSk1UseFX == null)
		{
			psSk1UseFX = OrangeBattleUtility.FindChildRecursive(ref childs, "psSk1UseFX", true).GetComponent<ParticleSystem>();
		}
		if (psSk2UseFX1 == null)
		{
			psSk2UseFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSk2UseFX1", true).GetComponent<ParticleSystem>();
		}
		if (psSk2UseFX2 == null)
		{
			psSk2UseFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSk2UseFX2", true).GetComponent<ParticleSystem>();
		}
		if (psSk3UseFX == null)
		{
			psSk3UseFX = OrangeBattleUtility.FindChildRecursive(ref childs, "psSk3UseFX", true).GetComponent<ParticleSystem>();
		}
		if (SwordFX == null)
		{
			SwordFX = OrangeBattleUtility.FindChildRecursive(ref childs, "SwordFX", true).GetComponent<ParticleSystem>();
		}
		if (BodyMesh == null)
		{
			BodyMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BodyMesh_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (GunMeshL == null)
		{
			GunMeshL = OrangeBattleUtility.FindChildRecursive(ref childs, "BusterMesh_L_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (GunMeshR == null)
		{
			GunMeshR = OrangeBattleUtility.FindChildRecursive(ref childs, "BusterMesh_R_m", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (CloakMesh == null)
		{
			CloakMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "CloakMesh_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (HandMeshL == null)
		{
			HandMeshL = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_L_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (HandMeshR == null)
		{
			HandMeshR = OrangeBattleUtility.FindChildRecursive(ref childs, "HandMesh_R_c", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (SwordMesh == null)
		{
			SwordMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "SaberMesh", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
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
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget", 3);
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
			if (netSyncData.nParam0 != 0)
			{
				skillPattern = (SkillPattern)netSyncData.nParam0;
			}
			IsInvincible = false;
		}
		SwitchFx(psSk1UseFX, false);
		SwitchFx(psSk2UseFX1, false);
		SwitchFx(psSk3UseFX, false);
		SwitchFx(SwordFX, false);
		SwitchMesh(BodyMesh, true);
		SwitchMesh(CloakMesh, true);
		SwitchMesh(GunMeshL, false);
		SwitchMesh(GunMeshR, false);
		SwitchMesh(HandMeshL, true);
		SwitchMesh(HandMeshR, true);
		SwitchMesh(SwordMesh, false);
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
			}
			break;
		case MainStatus.Idle:
		case MainStatus.IdleNoTarget:
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			IgnoreGravity = false;
			SwitchCollideBullet(_collideBullet, false);
			SwitchCollideBullet(_collideBullet, true, 0);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBoss6SE("bs052_forteexe05");
				_velocity = VInt3.zero;
				ActionTimes = nSk0AtkTimes;
				SwitchMesh(GunMeshL, true);
				SwitchMesh(GunMeshR, true);
				SwitchMesh(HandMeshL, false);
				SwitchMesh(HandMeshR, false);
				EndPos = GetTargetPos();
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSk0ActTime1 * 20f);
				break;
			case SubStatus.Phase2:
				HasActed = false;
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSk0ActTime2 * 20f);
				break;
			case SubStatus.Phase3:
				SwitchFx(psSk0UseFX1, false);
				SwitchFx(psSk0UseFX2, false);
				SwitchMesh(GunMeshL, false);
				SwitchMesh(GunMeshR, false);
				SwitchMesh(HandMeshL, true);
				SwitchMesh(HandMeshR, true);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				ActionTimes = nSk1AtkTimes;
				Sk1Bullets.Clear();
				break;
			case SubStatus.Phase1:
				if (ActionTimes == 3)
				{
					PlayBoss6SE("bs052_forteexe07");
				}
				else
				{
					PlayBoss6SE("bs052_forteexe08");
				}
				HasActed = false;
				ActionAnimatorFrame = fSk1ActFrame;
				SwitchFx(psSk1UseFX, true);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				ActionTimes = nSk2AtkTimes;
				IgnoreGravity = true;
				HasActed = false;
				ActionAnimatorFrame = fSk2ActFrame;
				PlayBoss6SE("bs052_forteexe11");
				SwitchFx(psSk2UseFX1, true);
				break;
			case SubStatus.Phase2:
				PlayBoss6SE("bs052_forteexe12");
				HasActed = false;
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSk2ActTime * 10f);
				break;
			case SubStatus.Phase3:
			{
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSk2ActTime * 10f);
				EndPos = GetTargetPos(true);
				int num = CheckBlock(EndPos);
				if (num != 0)
				{
					_transform.position = Target._transform.position - Vector3.right * num;
					Controller.LogicPosition = new VInt3(_transform.position);
					UpdateDirection(num);
				}
				else
				{
					_transform.position = Target._transform.position - Vector3.right * Target.direction;
					Controller.LogicPosition = new VInt3(_transform.position);
					UpdateDirection(Target.direction);
				}
				break;
			}
			case SubStatus.Phase4:
				ShowHideMesh();
				SwitchCollideBullet(SwordCollider, true);
				SwordCollider.HitCallback = SwordHit;
				break;
			case SubStatus.Phase5:
				IgnoreGravity = false;
				SwitchCollideBullet(SwordCollider, false);
				SwitchMesh(SwordMesh, false);
				SwitchFx(SwordFX, false);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBoss6SE("bs052_forteexe14");
				_velocity = VInt3.zero;
				_velocity = VInt3.up * ((float)nSk3floatSpd * 0.001f);
				IgnoreGravity = true;
				ActionTimes = nSk3ActTimes;
				break;
			case SubStatus.Phase1:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSk3FloatTime * 20f);
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase3:
				Controller.collisionMask = 0;
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSk3ActTime * 10f);
				IsInvincible = true;
				SwitchCollideBullet(_collideBullet, false);
				SwitchCollideBullet(_collideBullet, true, 5, 1);
				SwitchFx(psSk3UseFX, true);
				break;
			case SubStatus.Phase4:
				PlayBoss6SE("bs052_forteexe15");
				SwitchCollideBullet(_collideBullet, false);
				SwitchCollideBullet(_collideBullet, true, 4);
				switch (ActionTimes)
				{
				case 2:
					_transform.position = new Vector3(CenterPos.x + 5f * (float)base.direction, MaxPos.y + 4f, 0f);
					Controller.LogicPosition = new VInt3(NowPos);
					break;
				case 1:
					_transform.position = new Vector3(CenterPos.x - 5f * (float)base.direction, MinPos.y - 4f, 0f);
					Controller.LogicPosition = new VInt3(NowPos);
					break;
				}
				if (ActionTimes > 0)
				{
					fMoveDis = fSk3Dis;
				}
				else
				{
					fMoveDis = Vector2.Distance(StartPos, EndPos);
				}
				EndPos = GetTargetPos();
				StartPos = NowPos;
				RushTarget();
				break;
			case SubStatus.Phase6:
				PlayBoss6SE("bs052_forteexe02");
				ModelTransform.localRotation = Quaternion.Euler(0f, 90f, 0f);
				Controller.collisionMask = 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer;
				SwitchCollideBullet(_collideBullet, false);
				SwitchCollideBullet(_collideBullet, true, 0);
				SwitchFx(psSk3UseFX, false);
				_velocity = VInt3.zero;
				IsInvincible = false;
				break;
			case SubStatus.Phase7:
				_velocity.y = -nSk3floatSpd;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBoss6SE("bs052_forteexe03");
				switch (skillPattern)
				{
				case SkillPattern.State3:
					EndPos = new Vector3(CenterPos.x, MinPos.y, 0f);
					if (NowPos.x > EndPos.x)
					{
						UpdateDirection(-1);
					}
					else
					{
						UpdateDirection(1);
					}
					_velocity.x = nSk4Spd * base.direction;
					StartPos = NowPos;
					fMoveDis = Mathf.Abs(NowPos.x - EndPos.x);
					break;
				default:
					EndPos = GetTargetPos();
					if (EndPos.x > CenterPos.x)
					{
						UpdateDirection(-1);
					}
					else
					{
						UpdateDirection(1);
					}
					_velocity.x = nSk4Spd * base.direction;
					StartPos = NowPos;
					fMoveDis = Mathf.Abs(NowPos.x - EndPos.x);
					break;
				}
				break;
			case SubStatus.Phase2:
				PlayBoss6SE("bs052_forteexe04");
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (MinPos.x > NowPos.x || MinPos.y - 0.01f > NowPos.y || MaxPos.x < NowPos.x || MaxPos.y - Controller.Collider2D.size.y - 0.01f < NowPos.y)
				{
					_transform.position = CenterPos;
					Controller.LogicPosition = new VInt3(CenterPos);
					IgnoreGravity = true;
				}
				SwitchFx(psSk1UseFX, false);
				SwitchFx(psSk2UseFX1, false);
				SwitchFx(psSk3UseFX, false);
				SwitchFx(SwordFX, false);
				SwitchMesh(BodyMesh, true);
				SwitchMesh(CloakMesh, true);
				SwitchMesh(GunMeshL, false);
				SwitchMesh(GunMeshR, false);
				SwitchMesh(HandMeshL, true);
				SwitchMesh(HandMeshR, true);
				SwitchMesh(SwordMesh, false);
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				nDeadCount = 0;
				if (IgnoreGravity || !Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
					return;
				}
				break;
			case SubStatus.Phase1:
				StartCoroutine(BossDieFlow(_transform.position, "FX_BOSS_EXPLODE2", false, false));
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
		case MainStatus.IdleNoTarget:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
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
				_currentAnimationId = AnimationID.ANI_CALL;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_START;
				break;
			case SubStatus.Phase2:
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_END;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_FALL;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_LAND;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
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
				_currentAnimationId = AnimationID.ANI_SKILL3_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL3_END3;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_IDLE_SKY;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_IDLE;
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
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL4_END;
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
			case MainStatus.IdleNoTarget:
				if (DebugMode)
				{
					mainStatus = NextSkill;
					break;
				}
				skillPattern = nextState;
				switch (skillPattern)
				{
				case SkillPattern.State1:
					mainStatus = MainStatus.Skill1;
					break;
				case SkillPattern.State2:
					mainStatus = MainStatus.Skill2;
					nextState = SkillPattern.State4;
					break;
				case SkillPattern.State3:
					mainStatus = MainStatus.Skill4;
					break;
				case SkillPattern.State4:
					mainStatus = MainStatus.Skill4;
					break;
				case SkillPattern.State5:
					mainStatus = MainStatus.Skill4;
					break;
				default:
					mainStatus = MainStatus.Skill0;
					break;
				}
				break;
			}
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus, false, new object[1] { (int)skillPattern });
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
		case MainStatus.IdleNoTarget:
			if (bWaitNetStatus || IdleWaitFrame >= GameLogicUpdateManager.GameFrame)
			{
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!Target)
			{
				for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
				{
					if ((int)StageUpdate.runPlayers[i].Hp > 0)
					{
						Target = StageUpdate.runPlayers[i];
						break;
					}
				}
			}
			if ((bool)Target)
			{
				UpdateDirection();
				UpdateNextState();
			}
			else
			{
				UpdateNextState(MainStatus.IdleNoTarget);
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
				EndPos = GetTargetPos();
				UpdateDirection();
				NextAngle = Vector3.Angle((EndPos - NowPos).normalized, Vector3.up);
				ShotAngle = Mathf.Lerp(ShotAngle, NextAngle, 0.3f);
				_animator.SetFloat(_HashAngle, ShotAngle);
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					PlayBoss6SE("bs052_forteexe06");
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					SwitchFx(psSk0UseFX1, true);
					SwitchFx(psSk0UseFX2, true);
					Vector3 worldPos = Vector3.up + NowPos + (EndPos - NowPos).normalized * ((float)(nSk0AtkTimes - ActionTimes + 1) * fSk0AtkSpacing);
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, worldPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					if (--ActionTimes > 0)
					{
						HasActed = false;
						ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSk0ActTime2 * 20f);
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
					nextState = SkillPattern.State5;
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					EndPos = new Vector3(CenterPos.x + (float)(ActionTimes - 2) * 4f, MaxPos.y - 3f, 0f);
					BulletBase bulletBase = BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos, EndPos, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					if (bulletBase != null)
					{
						Sk1Bullets.Add(bulletBase);
					}
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
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
				if (_currentFrame > 1f)
				{
					nextState = SkillPattern.State2;
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
					SwitchMesh(SwordMesh, true);
					SwitchFx(SwordFX, true);
				}
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
				if (!HasActed && GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					HasActed = true;
					ShowHideMesh(false);
					SwitchFx(psSk2UseFX2, true);
					ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSk2ActTime * 10f);
				}
				if (HasActed && GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					PlayBoss6SE("bs052_forteexe13");
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (!(_currentFrame > 1f))
				{
					break;
				}
				if (NowPos.y < MinPos.y + 0.1f)
				{
					IgnoreGravity = false;
					SwitchCollideBullet(SwordCollider, false);
					SwitchMesh(SwordMesh, false);
					SwitchFx(SwordFX, false);
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				else
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_velocity == VInt3.zero)
				{
					_velocity = VInt3.up * ((float)nSk3floatSpd * 0.001f);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
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
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
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
				if (Vector2.Distance(NowPos, StartPos) >= fMoveDis)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase4);
					}
					else if (ActionTimes > -1)
					{
						PlayBoss6SE("bs052_forteexe15");
						EndPos = CenterPos;
						StartPos = NowPos;
						fMoveDis = Vector2.Distance(StartPos, EndPos);
						RushTarget();
					}
					else
					{
						_transform.position = EndPos;
						Controller.LogicPosition = new VInt3(EndPos);
						SetStatus(MainStatus.Skill3, SubStatus.Phase6);
					}
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (Controller.Collisions.below)
				{
					IgnoreGravity = false;
					nextState = SkillPattern.State5;
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase8:
				if (_currentFrame > 1f)
				{
					nextState = SkillPattern.State5;
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				switch (skillPattern)
				{
				case SkillPattern.State3:
					if (Vector2.Distance(NowPos, StartPos) >= fMoveDis)
					{
						_velocity = VInt3.zero;
						_transform.position = new Vector3(CenterPos.x, NowPos.y, 0f);
						Controller.LogicPosition = new VInt3(NowPos);
						SetStatus(MainStatus.Skill4, SubStatus.Phase2);
					}
					break;
				default:
					if (Controller.Collisions.right || Controller.Collisions.left)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase2);
					}
					break;
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					switch (skillPattern)
					{
					case SkillPattern.State3:
						SetStatus(MainStatus.Skill0);
						break;
					case SkillPattern.State4:
						UpdateDirection(-base.direction);
						SetStatus(MainStatus.Skill3);
						break;
					default:
						nextState = SkillPattern.State1;
						UpdateDirection(-base.direction);
						SetStatus(MainStatus.Idle);
						break;
					}
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
					if (nDeadCount > 10 || !Controller.Collisions.below)
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
			CheckRoomSize();
			SwitchMesh(SwordMesh, false);
			SwitchCollideBullet(_collideBullet, true, 0);
			SwitchCollideBullet(SwordCollider, false, 3);
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
		base.SoundSource.PlaySE("BossSE06", "bs052_forteexe16", 0.5f);
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		foreach (BulletBase sk1Bullet in Sk1Bullets)
		{
			sk1Bullet.BackToPool();
		}
		IgnoreGravity = false;
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
		if (!Target)
		{
			for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
			{
				if ((bool)StageUpdate.runPlayers[i])
				{
					Target = StageUpdate.runPlayers[i];
					break;
				}
			}
		}
		if ((bool)Target)
		{
			if (realcenter)
			{
				TargetPos = new VInt3(Target.Controller.GetRealCenterPos());
			}
			else
			{
				TargetPos = Target.Controller.LogicPosition;
			}
			return TargetPos.vec3;
		}
		return NowPos + Vector3.up * 1f;
	}

	private void SwitchCollideBullet(CollideBullet collide, bool onoff, int weaponid = -1, int targetlayer = 0)
	{
		if (!collide)
		{
			return;
		}
		if (weaponid != -1)
		{
			collide.UpdateBulletData(EnemyWeapons[weaponid].BulletData);
			collide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
		}
		if (onoff)
		{
			if (!collide.IsActivate)
			{
				switch (targetlayer)
				{
				case 1:
					collide.Active(friendMask);
					break;
				case 2:
					collide.Active(neutralMask);
					break;
				default:
					collide.Active(targetMask);
					break;
				}
			}
		}
		else if (collide.IsActivate)
		{
			collide.BackToPool();
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

	private void ShowHideMesh(bool show = true)
	{
		base.AllowAutoAim = show;
		SwitchMesh(BodyMesh, show);
		SwitchMesh(CloakMesh, show);
		SwitchMesh(GunMeshL, show);
		SwitchMesh(GunMeshR, show);
		SwitchMesh(HandMeshL, show);
		SwitchMesh(HandMeshR, show);
		SwitchMesh(SwordMesh, show);
		SwitchFx(SwordFX, show);
		SetColliderEnable(show);
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

	private int CheckBlock(Vector3 RayOrigin)
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(RayOrigin, Vector3.left, 2f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(RayOrigin, Vector3.right, 2f, layerMask, _transform);
		if ((bool)raycastHit2D)
		{
			return -1;
		}
		if ((bool)raycastHit2D2)
		{
			return 1;
		}
		return 0;
	}

	private void SwordHit(object obj)
	{
		if (obj != null)
		{
			Collider2D collider2D = obj as Collider2D;
			if (collider2D != null && OrangeBattleUtility.GetHitTargetOrangeCharacter(collider2D) != null)
			{
				ActionTimes = 0;
				nextState = SkillPattern.State3;
			}
		}
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
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
			CenterPos = (MaxPos + MinPos) / 2f;
		}
	}

	private void RushTarget()
	{
		UpdateDirection();
		Vector3 vector = EndPos - Controller.LogicPosition.vec3;
		vector.z = 0f;
		_velocity = new VInt3(vector.normalized * nSk3Spd) * 0.001f;
		ShotAngle = Vector2.Angle(Vector2.right * base.direction, vector);
		if (_velocity.y > 0)
		{
			ShotAngle = 0f - ShotAngle;
		}
		ShotAngle *= base.direction;
		ModelTransform.localRotation = Quaternion.Euler(ShotAngle, 90f, 0f);
		MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", NowPos, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, vector)), Array.Empty<object>()).SetEffect(fMoveDis, new Color(1f, 0.2f, 0.8f, 0.7f), new Color(1f, 0.2f, 0.8f), 1f);
	}
}
