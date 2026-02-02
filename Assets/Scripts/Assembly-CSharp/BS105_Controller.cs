#define RELEASE
using System;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS105_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		Phase9 = 9,
		MAX_SUBSTATUS = 10
	}

	private enum SkillPattern
	{
		State1 = 1,
		State2 = 2,
		State3 = 3,
		State4 = 4,
		State5 = 5
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_IDLE2 = 1,
		ANI_DEBUT = 2,
		ANI_DEBUT_LOOP = 3,
		ANI_WALK = 4,
		ANI_SKILL0_START = 5,
		ANI_SKILL0_LOOP = 6,
		ANI_SKILL0_END = 7,
		ANI_SKILL1_START1 = 8,
		ANI_SKILL1_LOOP1 = 9,
		ANI_SKILL1_START2 = 10,
		ANI_SKILL1_LOOP2 = 11,
		ANI_SKILL1_END = 12,
		ANI_SKILL2_START1 = 13,
		ANI_SKILL2_LOOP1 = 14,
		ANI_SKILL2_START2 = 15,
		ANI_SKILL2_LOOP2 = 16,
		ANI_SKILL2_START3 = 17,
		ANI_SKILL2_LOOP3 = 18,
		ANI_SKILL2_END = 19,
		ANI_SKILL3_START = 20,
		ANI_SKILL3_LOOP = 21,
		ANI_SKILL3_END = 22,
		ANI_SKILL4_START = 23,
		ANI_SKILL4_LOOP = 24,
		ANI_SKILL4_END = 25,
		ANI_SKILL5_START1 = 26,
		ANI_SKILL5_LOOP1 = 27,
		ANI_SKILL5_START2 = 28,
		ANI_SKILL5_END1 = 29,
		ANI_SKILL5_START3 = 30,
		ANI_SKILL5_LOOP3 = 31,
		ANI_SKILL5_END3 = 32,
		ANI_HURT = 33,
		ANI_DEAD = 34,
		MAX_ANIMATION_ID = 35
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

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	private int SpawnCount;

	[SerializeField]
	private SkinnedMeshRenderer RHandMesh;

	[SerializeField]
	private SkinnedMeshRenderer LHandMesh;

	[SerializeField]
	private SkinnedMeshRenderer RGunMesh;

	[SerializeField]
	private SkinnedMeshRenderer LGunMesh;

	private CollideBulletHitSelf BuffCollide;

	[SerializeField]
	private string BuffCollideName = "BuffCollide";

	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("火炎彈")]
	[SerializeField]
	private int Skill0ShootTimes = 6;

	[SerializeField]
	private float Skill0ShootFrame = 0.05f;

	[Header("爆裂火炎彈")]
	[SerializeField]
	private int Skill1ShootTimes = 1;

	[SerializeField]
	private float Skill1ShootFrame = 0.05f;

	[SerializeField]
	private float Skill1ChargeTime = 1f;

	private EnergyBullet Skill1Bullet;

	[Header("跳躍火炎彈")]
	[SerializeField]
	private int Skill2ShootTimes = 3;

	[SerializeField]
	private float Skill2ShootFrame = 0.05f;

	[SerializeField]
	private float Skill2JumpTime = 0.5f;

	[SerializeField]
	private float Skill2WallDis = 0.5f;

	private int Gravity = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;

	[Header("投盾")]
	[SerializeField]
	private float Skill3ShootFrame = 0.05f;

	[SerializeField]
	private Transform ShieldTransform;

	[SerializeField]
	private SkinnedMeshRenderer ShieldMesh;

	private EM200_Controller Skill3Shield;

	[Header("收盾")]
	[SerializeField]
	private float Skill4ShootFrame = 0.05f;

	[Header("持盾衝刺")]
	[SerializeField]
	private Transform ShieldObj;

	[SerializeField]
	private CatchPlayerTool CatchTool;

	[SerializeField]
	private int Skill5AtkTimes = 2;

	[SerializeField]
	private int Skill5ThrowSpeed = 15000;

	[SerializeField]
	private float Skill5ThrowTime = 1f;

	[SerializeField]
	private ParticleSystem Skill5UseFx1;

	[SerializeField]
	private ParticleSystem Skill5UseFx2;

	[SerializeField]
	private ParticleSystem Skill5UseFx3;

	private int DashSpeed = Mathf.RoundToInt(OrangeBattleUtility.PlayerDashSpeed * OrangeBattleUtility.PPU * OrangeBattleUtility.FPS * 1000f);

	[SerializeField]
	private float DashSpeedMulti = 1.4f;

	[Header("AI控制")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SkillPattern skillpattern = SkillPattern.State1;

	private int Skill0UseTimes;

	private int Skill2UseTimes;

	private int Skill5UseTimes;

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
		_animationHash = new int[35];
		_animationHash[0] = Animator.StringToHash("BS105@IDLE_LOOP01");
		_animationHash[1] = Animator.StringToHash("BS105@IDLE_LOOP02");
		_animationHash[2] = Animator.StringToHash("BS105@DEBUT");
		_animationHash[3] = Animator.StringToHash("BS105@DEBUT_Loop");
		_animationHash[4] = Animator.StringToHash("BS105@MOVE_LOOP");
		_animationHash[5] = Animator.StringToHash("BS105@SIGMA_X3_SKILL1_CASTING1");
		_animationHash[6] = Animator.StringToHash("BS105@SIGMA_X3_SKILL1_CASTLOOP1");
		_animationHash[7] = Animator.StringToHash("BS105@SIGMA_X3_SKILL1_CASTOUT1");
		_animationHash[8] = Animator.StringToHash("BS105@SIGMA_X3_SKILL2_CASTING1");
		_animationHash[9] = Animator.StringToHash("BS105@SIGMA_X3_SKILL2_CASTLOOP1");
		_animationHash[10] = Animator.StringToHash("BS105@SIGMA_X3_SKILL2_CASTING2");
		_animationHash[11] = Animator.StringToHash("BS105@SIGMA_X3_SKILL2_CASTLOOP2");
		_animationHash[12] = Animator.StringToHash("BS105@SIGMA_X3_SKILL2_CASTOUT1");
		_animationHash[13] = Animator.StringToHash("BS105@SIGMA_X3_SKILL3_CASTING1");
		_animationHash[14] = Animator.StringToHash("BS105@SIGMA_X3_SKILL3_CASTLOOP1");
		_animationHash[15] = Animator.StringToHash("BS105@SIGMA_X3_SKILL3_CASTING2");
		_animationHash[16] = Animator.StringToHash("BS105@SIGMA_X3_SKILL3_CASTLOOP2");
		_animationHash[17] = Animator.StringToHash("BS105@SIGMA_X3_SKILL3_CASTING3");
		_animationHash[18] = Animator.StringToHash("BS105@SIGMA_X3_SKILL3_CASTLOOP3");
		_animationHash[19] = Animator.StringToHash("BS105@SIGMA_X3_SKILL3_CASTOUT1");
		_animationHash[20] = Animator.StringToHash("BS105@SIGMA_X3_SKILL4_CASTING1");
		_animationHash[21] = Animator.StringToHash("BS105@SIGMA_X3_SKILL4_CASTLOOP1");
		_animationHash[22] = Animator.StringToHash("BS105@SIGMA_X3_SKILL4_CASTOUT1");
		_animationHash[23] = Animator.StringToHash("BS105@SIGMA_X3_SKILL5_CASTING1");
		_animationHash[24] = Animator.StringToHash("BS105@SIGMA_X3_SKILL5_CASTLOOP1");
		_animationHash[25] = Animator.StringToHash("BS105@SIGMA_X3_SKILL5_CASTOUT1");
		_animationHash[26] = Animator.StringToHash("BS105@SIGMA_X3_SKILL6_CASTING1");
		_animationHash[27] = Animator.StringToHash("BS105@SIGMA_X3_SKILL6_CASTLOOP1");
		_animationHash[28] = Animator.StringToHash("BS105@SIGMA_X3_SKILL6_CASTING2");
		_animationHash[29] = Animator.StringToHash("BS105@SIGMA_X3_SKILL6_CASTOUT1");
		_animationHash[30] = Animator.StringToHash("BS105@SIGMA_X3_SKILL6_CASTING3");
		_animationHash[31] = Animator.StringToHash("BS105@SIGMA_X3_SKILL6_CASTLOOP2");
		_animationHash[32] = Animator.StringToHash("BS105@SIGMA_X3_SKILL6_CASTOUT2");
		_animationHash[33] = Animator.StringToHash("BS105@HURT_Loop");
		_animationHash[34] = Animator.StringToHash("BS105@DEAD");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		BuffCollide = OrangeBattleUtility.FindChildRecursive(ref childs, BuffCollideName, true).gameObject.AddOrGetComponent<CollideBulletHitSelf>();
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip R Hand", true);
		}
		if (ShieldTransform == null)
		{
			ShieldTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "p", true);
		}
		if (ShieldMesh == null)
		{
			ShieldMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS104_WeaponMesh_G", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (RHandMesh == null)
		{
			RHandMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS104_HandMesh_R_G", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (LHandMesh == null)
		{
			LHandMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS104_HandMesh_L_G", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (RGunMesh == null)
		{
			RGunMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS104_WeaponMesh_R_G", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (LGunMesh == null)
		{
			LGunMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS104_WeaponMesh_L_G", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (Skill5UseFx1 == null)
		{
			Skill5UseFx1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5UseFx1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill5UseFx2 == null)
		{
			Skill5UseFx2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5UseFx2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill5UseFx3 == null)
		{
			Skill5UseFx3 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5UseFx3", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (!ShieldObj)
		{
			ShieldObj = OrangeBattleUtility.FindChildRecursive(ref childs, "ShieldCollider", true);
		}
		if ((bool)ShieldObj)
		{
			ShieldObj.gameObject.AddOrGetComponent<StageObjParam>().nSubPartID = 1;
			GuardTransform.Add(1);
		}
		SwitchMesh(RGunMesh, false);
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
			if (netSyncData.nParam0 > 0 && netSyncData.nParam0 <= 5)
			{
				skillpattern = (SkillPattern)netSyncData.nParam0;
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
			switch (_subStatus)
			{
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
				_velocity = VInt3.zero;
				ActionTimes = Skill0ShootTimes;
				SwitchMesh(RHandMesh, false);
				SwitchMesh(RGunMesh, true);
				break;
			case SubStatus.Phase1:
				HasActed = false;
				ActionAnimatorFrame = Skill0ShootFrame;
				break;
			case SubStatus.Phase2:
				SwitchMesh(RHandMesh, true);
				SwitchMesh(RGunMesh, false);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				SwitchMesh(LHandMesh, false);
				SwitchMesh(LGunMesh, true);
				break;
			case SubStatus.Phase1:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill1ChargeTime * 20f);
				break;
			case SubStatus.Phase2:
				ActionTimes = Skill1ShootTimes;
				_animator.SetFloat(_HashAngle, 90f);
				break;
			case SubStatus.Phase3:
				HasActed = false;
				ActionAnimatorFrame = Skill1ShootFrame;
				break;
			case SubStatus.Phase4:
				SwitchMesh(LHandMesh, true);
				SwitchMesh(LGunMesh, false);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE06", "bs050_dopsig03");
				if (NowPos.x > CenterPos.x)
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				break;
			case SubStatus.Phase1:
			{
				int num = (int)(Skill2JumpTime * 20f);
				ActionFrame = GameLogicUpdateManager.GameFrame + num;
				_velocity = new VInt3(CalXSpeed(NowPos.x, CenterPos.x, Skill2JumpTime), Math.Abs(Gravity * num), 0);
				break;
			}
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				IgnoreGravity = true;
				ActionTimes = Skill2ShootTimes;
				SwitchMesh(RHandMesh, false);
				SwitchMesh(RGunMesh, true);
				break;
			case SubStatus.Phase3:
				HasActed = false;
				ActionAnimatorFrame = Skill2ShootFrame;
				break;
			case SubStatus.Phase4:
				if (base.direction == -1)
				{
					EndPos.x = MinPos.x + Skill2WallDis;
				}
				else
				{
					EndPos.x = MaxPos.x - Skill2WallDis;
				}
				_velocity.x = CalXSpeed(NowPos.x, EndPos.x, Skill2JumpTime);
				IgnoreGravity = false;
				SwitchMesh(RHandMesh, true);
				SwitchMesh(RGunMesh, false);
				break;
			case SubStatus.Phase6:
				_velocity = VInt3.zero;
				UpdateDirection(-base.direction);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				HasActed = false;
				ActionAnimatorFrame = Skill3ShootFrame;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				if ((bool)Skill3Shield)
				{
					Skill3Shield.SetSkill2(ShieldTransform.position);
					if (Skill3Shield._transform.position.x > NowPos.x)
					{
						UpdateDirection(1);
					}
					else
					{
						UpdateDirection(-1);
					}
				}
				break;
			case SubStatus.Phase1:
				HasActed = false;
				ActionAnimatorFrame = Skill4ShootFrame;
				break;
			case SubStatus.Phase2:
				BuffCollide.Active(friendMask);
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ActionTimes = Skill5AtkTimes;
				_velocity = VInt3.zero;
				_velocity.x = (int)((float)DashSpeed * DashSpeedMulti) * base.direction;
				SwitchFx(Skill5UseFx1, true);
				ShieldObj.gameObject.SetActive(true);
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE06", "bs050_dopsig07");
				_collideBullet.UpdateBulletData(EnemyWeapons[5].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				SwitchFx(Skill5UseFx1, false);
				SwitchFx(Skill5UseFx2, true);
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase3:
				PlaySE("BossSE06", "bs050_dopsig11");
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				_velocity = VInt3.zero;
				SwitchFx(Skill5UseFx2, false);
				UpdateDirection(-base.direction);
				ShieldObj.gameObject.SetActive(false);
				break;
			case SubStatus.Phase4:
				PlaySE("BossSE06", "bs050_dopsig08");
				PlaySE("BossSE06", "bs050_dopsig09");
				_velocity = VInt3.zero;
				SwitchFx(Skill5UseFx2, false);
				SwitchFx(Skill5UseFx3, true);
				break;
			case SubStatus.Phase5:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill5ThrowTime * 20f);
				break;
			case SubStatus.Phase6:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase7:
				PlaySE("BossSE06", "bs050_dopsig04");
				SwitchFx(Skill5UseFx1, true);
				_velocity.x = (int)((float)DashSpeed * DashSpeedMulti) * base.direction;
				break;
			case SubStatus.Phase8:
				SwitchFx(Skill5UseFx1, false);
				SwitchFx(Skill5UseFx2, true);
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				if (!Controller.Collisions.below)
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
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
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
				_currentAnimationId = AnimationID.ANI_SKILL2_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL2_END;
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
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL4_END;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL5_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL5_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL5_END1;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL5_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL5_END3;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL5_START1;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP1;
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
			{
				int num = 0;
				switch (skillpattern)
				{
				case SkillPattern.State1:
					switch (OrangeBattleUtility.Random(0, 300) / 100)
					{
					case 0:
						mainStatus = MainStatus.Skill0;
						Skill0UseTimes++;
						break;
					case 1:
						mainStatus = MainStatus.Skill2;
						Skill2UseTimes++;
						break;
					case 2:
						mainStatus = MainStatus.Skill5;
						Skill5UseTimes++;
						break;
					}
					skillpattern = SkillPattern.State2;
					break;
				case SkillPattern.State2:
					if (Skill0UseTimes < 2 && Skill2UseTimes < 2 && Skill5UseTimes < 2)
					{
						skillpattern = SkillPattern.State1;
						goto case SkillPattern.State1;
					}
					Skill0UseTimes = (Skill2UseTimes = (Skill5UseTimes = 0));
					mainStatus = MainStatus.Skill3;
					skillpattern = SkillPattern.State3;
					break;
				case SkillPattern.State3:
					mainStatus = MainStatus.Skill1;
					skillpattern = SkillPattern.State4;
					break;
				case SkillPattern.State4:
					switch (OrangeBattleUtility.Random(0, 200) / 100)
					{
					case 0:
						mainStatus = MainStatus.Skill0;
						Skill0UseTimes++;
						break;
					case 1:
						mainStatus = MainStatus.Skill2;
						Skill2UseTimes++;
						break;
					}
					skillpattern = SkillPattern.State5;
					break;
				case SkillPattern.State5:
					if (Skill0UseTimes < 2 && Skill2UseTimes < 2)
					{
						skillpattern = SkillPattern.State4;
						goto case SkillPattern.State4;
					}
					Skill0UseTimes = (Skill2UseTimes = 0);
					mainStatus = MainStatus.Skill4;
					skillpattern = SkillPattern.State1;
					break;
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
			UploadEnemyStatus((int)mainStatus, false, new object[1] { (int)skillpattern });
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
					PlaySE("BossSE06", "bs050_dopsig02");
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					PlaySE("BossSE06", "bs050_dopsig12");
					BS105Skill0Bullet obj = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos.position + Vector3.right * 0.4f * base.direction + Vector3.down * 0.1f, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS105Skill0Bullet;
					obj.BackCallback = ShootWalBullet;
					float num = ActionTimes % 3 - 1;
					obj.SetAmplitude(num * 0.8f * (float)base.direction);
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase2);
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
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					EndPos = GetTargetPos();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPos.position);
					_animator.SetFloat(_HashAngle, ShotAngle);
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (!HasActed)
				{
					EndPos = GetTargetPos();
					UpdateDirection();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPos.position);
					_animator.SetFloat(_HashAngle, ShotAngle);
				}
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					EndPos = CenterPos;
					TargetPos = new VInt3(EndPos);
					UpdateDirection();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPos.position);
					_animator.SetFloat(_HashAngle, ShotAngle);
					Vector3 pDirection = EndPos - ShootPos.position;
					Skill1Bullet = BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, ShootPos, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as EnergyBullet;
					Skill1Bullet.BackCallback = Skill1BulletBack;
					Skill1Bullet.FreeDISTANCE = Vector2.Distance(ShootPos.position, EndPos);
					Skill1Bullet.StartShoot();
				}
				if (_currentFrame > 1f)
				{
					Skill1Bullet = null;
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase4);
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
				if (GameLogicUpdateManager.GameFrame >= ActionFrame && _velocity.y <= 10)
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
					PlaySE("BossSE06", "bs050_dopsig13");
					HasActed = true;
					(BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos, Vector3.down, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BasicBullet).BackCallback = Skill2BulletBack;
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase4);
					}
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					if (Skill3Shield == null)
					{
						PlaySE("BossSE06", "bs050_dopsig14");
						Skill3Shield = SpawnEnemy(ShieldTransform.position);
						float x = ((NowPos.x > CenterPos.x) ? (MinPos.x + 1.5f) : (MaxPos.x - 1.5f));
						Skill3Shield.SetSkill0(new Vector3(x, MaxPos.y - 1.5f, 0f));
					}
					SwitchMesh(ShieldMesh, false);
					selfBuffManager.RemoveBuffByCONDITIONID(EnemyWeapons[7].BulletData.n_CONDITION_ID);
				}
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((bool)Skill3Shield && Skill3Shield.hasBack)
				{
					PlaySE("BossSE06", "bs050_dopsig16");
					Skill3Shield.SetSuicide();
					Skill3Shield = null;
					SwitchMesh(ShieldMesh, true);
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
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
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					UpdateDirection(-base.direction);
					_velocity.x = (int)((float)DashSpeed * DashSpeedMulti) * base.direction;
					SetStatus(MainStatus.Skill5, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					PlaySE("BossSE06", "bs050_dopsig10");
					SetStatus(MainStatus.Skill5, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (CatchTool.IsCatching && GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					CatchTool.ReleaseTarget();
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (CatchTool.IsCatching && GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					CatchTool.ReleaseTarget();
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase3);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase1:
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
	}

	public void UpdateFunc()
	{
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		MainStatus mainStatus = _mainStatus;
		if (mainStatus != MainStatus.Skill5)
		{
			return;
		}
		switch (_subStatus)
		{
		case SubStatus.Phase1:
		{
			Collider2D collider2D = Physics2D.OverlapBox(NowPos + Vector3.right * base.direction, Vector2.one, 0f, LayerMask.GetMask("Player"));
			if ((bool)collider2D && (int)Hp > 0)
			{
				OrangeCharacter component = collider2D.GetComponent<OrangeCharacter>();
				CatchTool.CatchTarget(component);
			}
			if (CatchTool.IsCatching)
			{
				ActionTimes = 0;
				SetStatus(MainStatus.Skill5, SubStatus.Phase4);
			}
			else if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
			{
				if (--ActionTimes > 0)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				else
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase3);
				}
			}
			break;
		}
		case SubStatus.Phase4:
			if (_currentFrame > 0.8f && CatchTool.IsCatching)
			{
				CatchTool.MoveTargetWithForce(new VInt3(0, Skill5ThrowSpeed, 0));
			}
			else if (CatchTool.IsCatching)
			{
				CatchTool.MoveTarget();
			}
			if ((bool)CatchTool.TargetOC && CatchTool.TargetOC.Controller.Collisions.above)
			{
				CatchTool.ReleaseTarget();
				(BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, CatchTool.TargetOC._transform.position, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
				CatchTool.ReleaseTarget();
			}
			break;
		case SubStatus.Phase5:
			if (CatchTool.IsCatching)
			{
				CatchTool.MoveTargetWithForce(new VInt3(0, Skill5ThrowSpeed, 0));
			}
			if ((bool)CatchTool.TargetOC && CatchTool.TargetOC.Controller.Collisions.above)
			{
				CollideBullet obj = BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, CatchTool.TargetOC._transform.position, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet;
				CatchTool.ReleaseTarget();
				obj.bNeedBackPoolModelName = true;
				CatchTool.ReleaseTarget();
			}
			break;
		case SubStatus.Phase2:
		case SubStatus.Phase3:
			break;
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
			BuffCollide.UpdateBulletData(EnemyWeapons[7].BulletData, "", base.gameObject.GetInstanceID());
			BuffCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			BuffCollide.Active(friendMask);
			ShieldObj.gameObject.SetActive(false);
			CheckRoomSize();
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
			if ((bool)BuffCollide)
			{
				BuffCollide.BackToPool();
			}
			SwitchFx(Skill5UseFx1, false);
			SwitchFx(Skill5UseFx2, false);
			SwitchFx(Skill5UseFx3, false);
			SwitchMesh(RHandMesh, true);
			SwitchMesh(RGunMesh, false);
			SwitchMesh(LHandMesh, true);
			SwitchMesh(LGunMesh, false);
			if (CatchTool.IsCatching)
			{
				CatchTool.ReleaseTarget();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_002)
			{
				base.SoundSource.PlaySE("BossSE06", "bs050_dopsig18", 0.5f);
				SetStatus(MainStatus.Die);
			}
			else
			{
				Debug.LogError("此Boss 在此模式不該被擊殺");
				SetStatus(MainStatus.Die);
			}
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

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x - 4f, _transform.position.y + 0.5f, 0f);
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

	private int CalXSpeed(float startx, float endx, float jumptime, float timeoffset = 1f)
	{
		int num = (int)((float)Math.Abs((int)(jumptime * 20f)) * timeoffset);
		return (int)((endx - startx) * 1000f * 20f / (float)num);
	}

	private void ShootWalBullet(object obj)
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
		else if (basicBullet.isHitBlock)
		{
			BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, basicBullet._transform.position, Vector3.up, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		}
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
		}
		else if (basicBullet.isHitBlock)
		{
			(BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, basicBullet._transform.position, Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS045_ThunderBullet).SetEndPos(MaxPos.x - 1f);
			(BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, basicBullet._transform.position, Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS045_ThunderBullet).SetEndPos(MinPos.x + 1f);
		}
	}

	private void Skill1BulletBack(object obj)
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
			BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, worldPos, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		}
	}

	private EM200_Controller SpawnEnemy(Vector3 SpawnPos)
	{
		int num = 4;
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
		return enemyControllerBase as EM200_Controller;
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

	private void SwitchMesh(SkinnedMeshRenderer Mesh, bool onoff)
	{
		if ((bool)Mesh)
		{
			Mesh.enabled = onoff;
			return;
		}
		Debug.Log(string.Concat("Mesh載入有誤，目前狀態是 ", _mainStatus, "的階段 ", _subStatus));
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

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
	}
}
