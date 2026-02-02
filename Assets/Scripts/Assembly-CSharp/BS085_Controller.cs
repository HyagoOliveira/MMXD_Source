#define RELEASE
using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS085_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		IdleEntity = 1,
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
		MAX_SUBSTATUS = 6
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_IDLE_ENTITY = 1,
		ANI_DEBUT = 2,
		ANI_DEBUT_LOOP = 3,
		ANI_WALK = 4,
		ANI_WALK_ENTITY = 5,
		ANI_SKILL0_START1 = 6,
		ANI_SKILL0_LOOP1 = 7,
		ANI_SKILL0_START2 = 8,
		ANI_SKILL0_LOOP2 = 9,
		ANI_SKILL0_END2 = 10,
		ANI_SKILL1_START1 = 11,
		ANI_SKILL1_LOOP1 = 12,
		ANI_SKILL1_LOOP2 = 13,
		ANI_SKILL1_END2 = 14,
		ANI_SKILL2_START1 = 15,
		ANI_SKILL2_LOOP1 = 16,
		ANI_SKILL2_END1 = 17,
		ANI_SKILL3_START1 = 18,
		ANI_SKILL3_LOOP1 = 19,
		ANI_SKILL3_START2 = 20,
		ANI_SKILL3_LOOP2 = 21,
		ANI_SKILL3_START3 = 22,
		ANI_SKILL3_LOOP3 = 23,
		ANI_HURT = 24,
		ANI_DEAD = 25,
		MAX_ANIMATION_ID = 26
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

	private float GroundYPos;

	private int ActionTimes;

	private float ActionAnimatorFrame;

	private int ActionFrame;

	private bool HasActed;

	private float ShotAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[Header("切換主體用")]
	[SerializeField]
	private Animator[] animators = new Animator[2];

	[SerializeField]
	private Transform[] modelTransforms = new Transform[2];

	[SerializeField]
	private Vector2[] boxColliderSize = new Vector2[2]
	{
		new Vector2(2f, 4f),
		Vector2.one
	};

	[SerializeField]
	private Vector2[] boxColliderOffset = new Vector2[2]
	{
		new Vector2(0f, 2f),
		new Vector2(0f, 0.5f)
	};

	[SerializeField]
	private Vector3[] AimPoints = new Vector3[2]
	{
		new Vector3(0f, 2f, 0f),
		new Vector3(0f, 0.5f, 0f)
	};

	[SerializeField]
	private SkinnedMeshRenderer[] ShellMesh = new SkinnedMeshRenderer[4];

	[SerializeField]
	private Transform[] ShellObj = new Transform[4];

	[SerializeField]
	private int MaxShellPercentHp = 300;

	private int ShellHp = 10000;

	private bool HasShellBroken;

	[Header("廢鐵Jump")]
	[SerializeField]
	private float fSkill0JumpTime = 0.5f;

	private int nGravity = OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;

	[Header("廢鐵Throw")]
	[SerializeField]
	private float fSkill1ActFrame = 0.4f;

	[SerializeField]
	private int nSkill1ActTimes;

	[Header("廢鐵Back")]
	[SerializeField]
	private ParticleSystem psSkill2UseFX;

	[SerializeField]
	private float fSkill2ActFrame = 0.4f;

	[Header("蜻蜓衝刺")]
	[SerializeField]
	private ParticleSystem psSkill3UseFX;

	[SerializeField]
	private float fSkill3JumpTime = 0.5f;

	[SerializeField]
	private float fSkill3ActTime = 1f;

	[SerializeField]
	private int nSkill3RushSpd = 15000;

	[SerializeField]
	private int nSkill3MaxUseTimes = 3;

	private int nSkill3UseTimes;

	[Header("蜻蜓現身")]
	[SerializeField]
	private ParticleSystem psSkill4UseFX;

	[SerializeField]
	private string sPartsUsseFX = "FX_MOB_EXPLODE0";

	[SerializeField]
	private float fSkill4ActTime = 1f;

	[Header("廢鐵Reborn")]
	[SerializeField]
	private ParticleSystem psSkill5UseFX;

	[SerializeField]
	private float fSkill5ActTime = 1f;

	[Header("待機等待Frame")]
	[SerializeField]
	private float IdleWaitTime = 1f;

	private int IdleWaitFrame;

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private bool playFly;

	private bool playSprint;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

	private void FlyStatu(bool fly, bool sprint)
	{
		if (fly)
		{
			if (!playFly)
			{
				PlaySE("BossSE06", "bs114_oldrobot10_lp");
				playFly = true;
			}
		}
		else if (playFly)
		{
			PlaySE("BossSE06", "bs114_oldrobot10_stop");
			playFly = false;
		}
		if (sprint)
		{
			if (playFly)
			{
				PlaySE("BossSE06", "bs114_oldrobot10_stop");
			}
			if (!playSprint)
			{
				PlaySE("BossSE06", "bs114_oldrobot11_lp");
				playSprint = true;
			}
		}
		else
		{
			if (playSprint)
			{
				PlaySE("BossSE06", "bs114_oldrobot11_stop");
				playSprint = false;
			}
			if (playFly)
			{
				PlaySE("BossSE06", "bs114_oldrobot10_lp");
			}
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
		_animationHash = new int[26];
		_animationHash[0] = Animator.StringToHash("BS085@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS085_Enemy@idl_loop");
		_animationHash[2] = Animator.StringToHash("BS085@debut");
		_animationHash[3] = Animator.StringToHash("BS085@debut_loop");
		_animationHash[4] = Animator.StringToHash("BS085@walk_loop");
		_animationHash[5] = Animator.StringToHash("BS085_Enemy@walk_loop");
		_animationHash[6] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL1_CASTING1");
		_animationHash[7] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL1_CASTLOOP1");
		_animationHash[8] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL1_CASTING2");
		_animationHash[9] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL1_CASTLOOP2");
		_animationHash[10] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL1_CASTOUT1");
		_animationHash[11] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL3_CASTING1");
		_animationHash[12] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL3_CASTLOOP1");
		_animationHash[13] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL3_CASTLOOP2");
		_animationHash[14] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL3_CASTOUT1");
		_animationHash[15] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL4_CASTING1");
		_animationHash[16] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL4_CASTLOOP1");
		_animationHash[17] = Animator.StringToHash("BS085@OLD_ROBOT_SKILL4_CASTOUT1");
		_animationHash[18] = Animator.StringToHash("BS085_Enemy@OLD_ROBOT_SKILL2_CASTING1");
		_animationHash[19] = Animator.StringToHash("BS085_Enemy@OLD_ROBOT_SKILL2_CASTLOOP1");
		_animationHash[20] = Animator.StringToHash("BS085_Enemy@OLD_ROBOT_SKILL2_CASTING2");
		_animationHash[21] = Animator.StringToHash("BS085_Enemy@OLD_ROBOT_SKILL2_CASTLOOP2");
		_animationHash[22] = Animator.StringToHash("BS085_Enemy@OLD_ROBOT_SKILL2_CASTING3");
		_animationHash[23] = Animator.StringToHash("BS085_Enemy@OLD_ROBOT_SKILL2_CASTLOOP3");
		_animationHash[24] = Animator.StringToHash("BS085@hurt_loop");
		_animationHash[25] = Animator.StringToHash("BS085@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 Neck", true);
		}
		if (ShellObj[0] == null)
		{
			ShellObj[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 HeadNub", true);
		}
		if (ShellObj[1] == null)
		{
			ShellObj[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 R Forearm", true);
		}
		if (ShellObj[2] == null)
		{
			ShellObj[2] = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 R Hand", true);
		}
		if (ShellObj[3] == null)
		{
			ShellObj[3] = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 L Hand", true);
		}
		if (psSkill2UseFX == null)
		{
			psSkill2UseFX = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill2UseFX", true).GetComponent<ParticleSystem>();
		}
		if (psSkill3UseFX == null)
		{
			psSkill3UseFX = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill3UseFX", true).GetComponent<ParticleSystem>();
		}
		if (psSkill4UseFX == null)
		{
			psSkill4UseFX = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill4UseFX", true).GetComponent<ParticleSystem>();
		}
		if (psSkill5UseFX == null)
		{
			psSkill5UseFX = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill5UseFX", true).GetComponent<ParticleSystem>();
		}
		if (ShellMesh[0] == null)
		{
			ShellMesh[0] = OrangeBattleUtility.FindChildRecursive(ref childs, "Head_Mesh", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (ShellMesh[1] == null)
		{
			ShellMesh[1] = OrangeBattleUtility.FindChildRecursive(ref childs, "Forearm_Mesh_R", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (ShellMesh[2] == null)
		{
			ShellMesh[2] = OrangeBattleUtility.FindChildRecursive(ref childs, "Hand_Mesh_R", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
		if (ShellMesh[3] == null)
		{
			ShellMesh[3] = OrangeBattleUtility.FindChildRecursive(ref childs, "Hand_Mesh_L", true).gameObject.AddOrGetComponent<SkinnedMeshRenderer>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimPoint = new Vector3(0f, 2f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sPartsUsseFX, 10);
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
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				PlaySE("BossSE06", "bs114_oldrobot02");
				SwitchFx(psSkill5UseFX, true);
				break;
			case SubStatus.Phase2:
				SwitchFx(psSkill5UseFX, false);
				break;
			}
			break;
		case MainStatus.Idle:
		case MainStatus.IdleEntity:
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				PlaySE("BossSE06", "bs114_oldrobot03");
				SwitchCollideBullet(_collideBullet, true, 1);
				EndPos = GetTargetPos();
				if (NowPos.x > EndPos.x)
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				int num2 = (int)(fSkill0JumpTime * 20f);
				ActionFrame = GameLogicUpdateManager.GameFrame + num2;
				_velocity = new VInt3(CalXSpeed(NowPos.x, (NowPos.x + EndPos.x) / 2f, fSkill0JumpTime), Mathf.Abs(nGravity * num2), 0);
				break;
			}
			case SubStatus.Phase2:
				_velocity.x = CalXSpeed(NowPos.x, EndPos.x, fSkill0JumpTime);
				IgnoreGravity = false;
				break;
			case SubStatus.Phase4:
				PlaySE("BossSE06", "bs114_oldrobot04");
				SwitchCollideBullet(_collideBullet, true, 0);
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				ActionTimes = nSkill1ActTimes;
				break;
			case SubStatus.Phase2:
				HasActed = false;
				EndPos = GetTargetPos();
				UpdateDirection();
				ActionAnimatorFrame = fSkill1ActFrame;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				HasActed = false;
				ActionAnimatorFrame = fSkill2ActFrame;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				if (!playFly)
				{
					PlaySE("BossSE06", "bs114_oldrobot03");
				}
				_velocity = VInt3.zero;
				EndPos = GetTargetPos();
				UpdateDirection();
				EndPos = NowPos;
				int num = (int)(fSkill3JumpTime * 20f);
				ActionFrame = GameLogicUpdateManager.GameFrame + num;
				_velocity = new VInt3(CalXSpeed(NowPos.x, (NowPos.x + EndPos.x) / 2f, fSkill3JumpTime), Mathf.Abs(nGravity * num), 0);
				break;
			}
			case SubStatus.Phase2:
				IgnoreGravity = true;
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill3ActTime * 20f);
				break;
			case SubStatus.Phase4:
				if (playFly)
				{
					FlyStatu(true, true);
				}
				EndPos = GetTargetPos();
				UpdateDirection();
				SwitchFx(psSkill3UseFX, true);
				_velocity = new VInt3((EndPos - NowPos).normalized) * nSkill3RushSpd * 0.001f;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetColliderEnable(false);
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				HasActed = false;
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE06", "bs114_oldrobot09");
				SwitchModel(1);
				SwitchCollideBullet(_collideBullet, true, 2);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill4ActTime * 20f);
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				PlaySE("BossSE06", "bs114_oldrobot12");
				FlyStatu(false, false);
				SetColliderEnable(false);
				SwitchModel();
				SkinnedMeshRenderer[] shellMesh = ShellMesh;
				for (int i = 0; i < shellMesh.Length; i++)
				{
					shellMesh[i].enabled = true;
				}
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				SwitchFx(psSkill5UseFX, true);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill5ActTime * 20f);
				ModelTransform.localPosition = Vector3.down * 2f;
				PlaySE("BossSE06", "bs114_oldrobot01");
				LeanTween.move(ModelTransform.gameObject, ModelTransform.position + Vector3.up * 2f, 0.8f).setOnUpdateVector3(delegate(Vector3 pos)
				{
					ModelTransform.position = pos;
				}).setOnComplete((Action)delegate
				{
					LeanTween.cancel(ModelTransform.gameObject);
				});
				break;
			}
			case SubStatus.Phase1:
				SwitchCollideBullet(_collideBullet, true, 0);
				HasShellBroken = false;
				SwitchFx(psSkill5UseFX, false);
				ShellHp = (int)((float)(int)MaxHp * ((float)MaxShellPercentHp / 100f));
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				IgnoreGravity = true;
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				break;
			case SubStatus.Phase1:
				StartCoroutine(BossDieFlow(base.AimTransform, "FX_BOSS_EXPLODE2", false, false));
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
		case MainStatus.IdleEntity:
			_currentAnimationId = AnimationID.ANI_IDLE_ENTITY;
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
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP2;
				break;
			case SubStatus.Phase3:
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
				_currentAnimationId = AnimationID.ANI_SKILL2_END1;
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
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_IDLE_ENTITY;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT_LOOP;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_SKILL3_START3;
				break;
			}
			return;
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
			case MainStatus.IdleEntity:
			case MainStatus.Skill4:
			case MainStatus.Skill5:
				if (HasShellBroken)
				{
					if (nSkill3UseTimes >= nSkill3MaxUseTimes)
					{
						nSkill3UseTimes = 0;
						mainStatus = MainStatus.Skill5;
					}
					else
					{
						mainStatus = MainStatus.Skill3;
						nSkill3UseTimes++;
					}
				}
				else if (CheckShellFull())
				{
					mainStatus = ((OrangeBattleUtility.Random(0, 100) >= 50) ? MainStatus.Skill1 : MainStatus.Skill0);
				}
				else
				{
					int num = OrangeBattleUtility.Random(0, 150);
					mainStatus = ((num >= 50) ? ((num >= 100) ? MainStatus.Skill2 : MainStatus.Skill1) : MainStatus.Skill0);
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

	private bool CheckShellFull()
	{
		SkinnedMeshRenderer[] shellMesh = ShellMesh;
		for (int i = 0; i < shellMesh.Length; i++)
		{
			if (!shellMesh[i].enabled)
			{
				return false;
			}
		}
		return true;
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
				if (_currentFrame > 1f)
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
		case MainStatus.IdleEntity:
			if (bWaitNetStatus || IdleWaitFrame >= GameLogicUpdateManager.GameFrame)
			{
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!Target)
			{
				for (int k = 0; k < StageUpdate.runPlayers.Count; k++)
				{
					if ((int)StageUpdate.runPlayers[k].Hp > 0)
					{
						Target = StageUpdate.runPlayers[k];
						break;
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
				UpdateRandomState();
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
				if (GameLogicUpdateManager.GameFrame >= ActionFrame && _velocity.y <= 10)
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
				if (Controller.Collisions.below)
				{
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, ShootPos, (EndPos - ShootPos.position).normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase3);
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
			{
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					PlaySE("BossSE06", "bs114_oldrobot06");
					for (int i = 0; i < 5; i++)
					{
						StartPos = NowPos + Vector3.right * (1f * (float)(i - 2));
						(BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, StartPos, ShootPos.position - StartPos, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BasicBullet).FreeDISTANCE = Vector2.Distance(StartPos, ShootPos.position);
					}
				}
				if (!HasActed || !(_currentFrame > 1f))
				{
					break;
				}
				ShellHp += (int)((float)(int)MaxHp * ((float)(MaxShellPercentHp / ShellMesh.Length) / 100f));
				if (ShellHp > (int)((float)(int)MaxHp * ((float)MaxShellPercentHp / 100f)))
				{
					ShellHp = (int)((float)(int)MaxHp * ((float)MaxShellPercentHp / 100f));
				}
				SkinnedMeshRenderer[] shellMesh = ShellMesh;
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in shellMesh)
				{
					if (!skinnedMeshRenderer.enabled)
					{
						skinnedMeshRenderer.enabled = true;
						break;
					}
				}
				SetStatus(MainStatus.Skill2, SubStatus.Phase2);
				break;
			}
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
				if (GameLogicUpdateManager.GameFrame >= ActionFrame && _velocity.y <= 10)
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
				if (Controller.Collisions.left || Controller.Collisions.right || Controller.Collisions.above || Controller.Collisions.below || _velocity.Equals(VInt3.zero))
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 0.5f, false);
					IgnoreGravity = false;
					_velocity = VInt3.zero;
					SwitchFx(psSkill3UseFX, false);
				}
				if (Controller.Collisions.below)
				{
					if (!playFly)
					{
						PlaySE("BossSE06", "bs114_oldrobot04");
					}
					else
					{
						FlyStatu(true, false);
					}
					SetStatus(MainStatus.IdleEntity);
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!HasActed && _currentFrame > 0.9f)
				{
					HasActed = true;
					StageFXParam stageFXParam = new StageFXParam();
					stageFXParam.bMute = true;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxstory_explode_000", psSkill4UseFX.transform, Quaternion.identity, new Vector3(1f, 1f, 1f), new object[1] { stageFXParam });
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					FlyStatu(true, false);
					SetColliderEnable(true);
					base.AllowAutoAim = true;
					UpdateRandomState();
				}
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					PlaySE("BossSE06", "bs114_oldrobot02");
					SetStatus(MainStatus.Skill5, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetColliderEnable(true);
					base.AllowAutoAim = true;
					UpdateRandomState();
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
					SetStatus(MainStatus.Die, SubStatus.Phase2);
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
			SwitchCollideBullet(_collideBullet, true, 0);
			ShellHp = (int)((float)(int)MaxHp * ((float)MaxShellPercentHp / 100f));
			SwitchModel(1);
			SwitchModel();
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
			if (playFly)
			{
				PlaySE("BossSE06", "bs114_oldrobot10_stop");
				playFly = false;
			}
			if (playSprint)
			{
				PlaySE("BossSE06", "bs114_oldrobot11_stop");
				playSprint = false;
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
				TargetPos = new VInt3(Target.GetTargetPoint() + Vector3.up * 0.15f);
			}
			return TargetPos.vec3;
		}
		return NowPos + Vector3.right * 3f * base.direction;
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

	private void SwitchModel(int model = 0)
	{
		if (model >= modelTransforms.Length || model < 0)
		{
			Debug.LogError("找不到該 Model");
			return;
		}
		if (model == 0)
		{
			_animator.Play(_animationHash[1], 0, 0f);
		}
		else
		{
			_animator.Play(_animationHash[2], 0, 0f);
		}
		ModelTransform.gameObject.SetActive(false);
		ModelTransform = modelTransforms[model];
		ModelTransform.gameObject.SetActive(true);
		_animator.StopPlayback();
		_animator = animators[model];
		base.AimPoint = AimPoints[model];
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (!HasShellBroken)
		{
			if (ShellHp > (int)tHurtPassParam.dmg)
			{
				ShellHp -= tHurtPassParam.dmg;
				tHurtPassParam.dmg = 0;
			}
			else if (ShellHp > 0)
			{
				HasShellBroken = true;
				ShellHp = 0;
				tHurtPassParam.dmg = 0;
				UploadEnemyStatus(7, false, new object[1] { ShellHp });
			}
			for (int num = ShellMesh.Length - 1; num >= 0; num--)
			{
				if (ShellMesh[num].enabled && ShellHp <= (int)((float)(int)MaxHp * ((float)(MaxShellPercentHp * num / ShellMesh.Length) / 100f)))
				{
					PlaySE("BossSE06", "bs114_oldrobot07");
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sPartsUsseFX, ShellObj[num].position, Quaternion.Euler(0f, 0f, 0f), new object[1]
					{
						new Vector3(0.8f, 0.8f, 0.8f)
					});
					ShellMesh[num].enabled = false;
				}
			}
		}
		return base.Hurt(tHurtPassParam);
	}

	private int CalXSpeed(float startx, float endx, float jumptime, float timeoffset = 1f)
	{
		int num = (int)((float)Mathf.Abs((int)(jumptime * 20f)) * timeoffset);
		return (int)((endx - startx) * 1000f * 20f / (float)num);
	}
}
