#define RELEASE
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS096_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		IdleStand = 7,
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
		MAX_SUBSTATUS = 6
	}

	private enum SkillPattern
	{
		State1 = 0,
		State2 = 1,
		State3 = 2,
		State4 = 3,
		State5 = 4
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_WALK = 2,
		ANI_RUN = 3,
		ANI_SKILL0_START = 4,
		ANI_SKILL0_LOOP = 5,
		ANI_SKILL0_END = 6,
		ANI_SKILL1_START1 = 7,
		ANI_SKILL1_LOOP1 = 8,
		ANI_SKILL1_START2 = 9,
		ANI_SKILL1_LOOP2 = 10,
		ANI_SKILL1_END2 = 11,
		ANI_SKILL2_START1 = 12,
		ANI_SKILL2_LOOP1 = 13,
		ANI_SKILL2_START2 = 14,
		ANI_SKILL2_LOOP2 = 15,
		ANI_SKILL2_END2 = 16,
		ANI_SKILL3_START1 = 17,
		ANI_SKILL3_LOOP1 = 18,
		ANI_SKILL3_START2 = 19,
		ANI_SKILL3_LOOP2 = 20,
		ANI_SKILL3_END2 = 21,
		ANI_SKILL4_START1 = 22,
		ANI_SKILL4_LOOP1 = 23,
		ANI_SKILL4_START2 = 24,
		ANI_SKILL4_END2 = 25,
		ANI_SKILL4_END3 = 26,
		ANI_HURT = 27,
		ANI_DEAD = 28,
		MAX_ANIMATION_ID = 29
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

	private int[] DefaultSkillCard = new int[3] { 1, 2, 3 };

	private static int[] DefaultRangedSkillCard = new int[3] { 3, 4, 5 };

	private List<int> RangedSKC = new List<int>();

	private List<int> SkillCard = new List<int>();

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

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[SerializeField]
	private CatchPlayerTool CatchTool;

	[Header("待機等待Frame")]
	[SerializeField]
	private float IdleWaitTime = 1f;

	private int IdleWaitFrame;

	[Header("閃電柱")]
	[SerializeField]
	private string Skill0UseFx = "fxuse_LIFE-VIRUS_000";

	[SerializeField]
	private string Skill0PreFx = "fxuse_LIFE-VIRUS_001";

	[SerializeField]
	private string PreTargetFx = "fxuseTarget";

	[SerializeField]
	private int CloudDis = 2;

	[SerializeField]
	private float Skill0ShootInterval = 3f;

	[SerializeField]
	private int Skill0ShootTimes = 5;

	[SerializeField]
	private float Skill0ShootCycle = 10f;

	private int Skill0PreActionFrame;

	private int Skill0ActionTimes;

	private Vector3 Skill0ShootPosition;

	private List<ValueTuple<int, Vector3>> Skill0Magazine = new List<ValueTuple<int, Vector3>>();

	[SerializeField]
	private bool CanUseSkill0;

	private int SpawnCloud;

	private int[] CloudOrder = new int[0];

	[Header("水彈")]
	[SerializeField]
	private int Skill1ShootTimes = 3;

	[SerializeField]
	private float Skill1ShootFrame = 0.6f;

	[SerializeField]
	private ParticleSystem Skill1UseFx1;

	[SerializeField]
	private ParticleSystem Skill1UseFx2;

	[SerializeField]
	private ParticleSystem Skill1UseFx3;

	private int Skill1ShootAngle;

	[Header("火焰蔓燒")]
	[SerializeField]
	private float Skill2ShootTime = 5f;

	[SerializeField]
	private float Skill2StartAngle = 160f;

	[SerializeField]
	private float Skill2EndAngle = 80f;

	[SerializeField]
	private ParticleSystem Skill2UseFx;

	[SerializeField]
	private int Skill2FireInterval = 2;

	private float Skill2Angle = 70f;

	private float Skill2AngleSpace = 2f;

	[Header("衝刺")]
	[SerializeField]
	private int Skill3MoveSpeed = 15000;

	[SerializeField]
	private ParticleSystem Skill3UseFx;

	[Header("抓取")]
	[SerializeField]
	private int Skill4ThrowSpeed = 15000;

	[SerializeField]
	private float Skill4ThrowTime = 3f;

	[SerializeField]
	private string Skill4HandObjName = "Skill4Collide";

	private CollideBullet Skill4Collide;

	private SkillPattern skillpattern;

	private CollideBulletHitSelf BuffCollide1;

	private CollideBulletHitSelf BuffCollide2;

	private CollideBulletHitSelf BuffCollide3;

	[SerializeField]
	private string BuffCollideName1 = "BuffCollide1";

	[SerializeField]
	private string BuffCollideName2 = "BuffCollide2";

	[SerializeField]
	private string BuffCollideName3 = "BuffCollide3";

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private bool bPlayLP;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

	private void PlayLP(bool sw)
	{
		if (sw != bPlayLP)
		{
			bPlayLP = sw;
			if (sw)
			{
				PlayBossSE("BossSE05", "bs043_drmvirus03_lp");
			}
			else
			{
				PlayBossSE("BossSE05", "bs043_drmvirus03_stop");
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
		switch (AiState)
		{
		case AI_STATE.mob_002:
			BuffCollide3.Active(friendMask);
			break;
		default:
			BuffCollide2.Active(friendMask);
			BuffCollide3.Active(friendMask);
			break;
		case AI_STATE.mob_003:
		case AI_STATE.mob_004:
			break;
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[29];
		_animationHash[0] = Animator.StringToHash("BS096@IDLE_LOOP");
		_animationHash[1] = Animator.StringToHash("BS096@DEBUT");
		_animationHash[2] = Animator.StringToHash("BS096@WALK_LOOP");
		_animationHash[3] = Animator.StringToHash("BS096@RUN_LOOP");
		_animationHash[4] = Animator.StringToHash("BS096@SKILL1_CASTING1");
		_animationHash[5] = Animator.StringToHash("BS096@SKILL1_CASTLOOP1");
		_animationHash[6] = Animator.StringToHash("BS096@SKILL1_CASTOUT1");
		_animationHash[7] = Animator.StringToHash("BS096@SKILL2_CASTING1");
		_animationHash[8] = Animator.StringToHash("BS096@SKILL2_CASTLOOP1");
		_animationHash[9] = Animator.StringToHash("BS096@SKILL2_CASTING2");
		_animationHash[10] = Animator.StringToHash("BS096@SKILL2_CASTLOOP2");
		_animationHash[11] = Animator.StringToHash("BS096@SKILL2_CASTOUT1");
		_animationHash[12] = Animator.StringToHash("BS096@SKILL3_CASTING1");
		_animationHash[13] = Animator.StringToHash("BS096@SKILL3_CASTLOOP1");
		_animationHash[14] = Animator.StringToHash("BS096@SKILL3_CASTING2");
		_animationHash[15] = Animator.StringToHash("BS096@SKILL3_CASTLOOP2");
		_animationHash[16] = Animator.StringToHash("BS096@SKILL3_CASTOUT1");
		_animationHash[17] = Animator.StringToHash("BS096@SKILL4_CASTING1");
		_animationHash[18] = Animator.StringToHash("BS096@SKILL4_CASTLOOP1");
		_animationHash[19] = Animator.StringToHash("BS096@SKILL4_CASTING2");
		_animationHash[20] = Animator.StringToHash("BS096@SKILL4_CASTLOOP2");
		_animationHash[21] = Animator.StringToHash("BS096@SKILL4_CASTOUT1");
		_animationHash[22] = Animator.StringToHash("BS096@SKILL5_CASTING1");
		_animationHash[23] = Animator.StringToHash("BS096@SKILL5_CASTLOOP1");
		_animationHash[24] = Animator.StringToHash("BS096@SKILL5_CASTING2");
		_animationHash[25] = Animator.StringToHash("BS096@SKILL5_CASTOUT1");
		_animationHash[26] = Animator.StringToHash("BS096@SKILL5_CASTOUT2");
		_animationHash[27] = Animator.StringToHash("BS096@HURT_LOOP");
		_animationHash[28] = Animator.StringToHash("BS096@DEATH");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		Skill4Collide = OrangeBattleUtility.FindChildRecursive(ref childs, Skill4HandObjName, true).gameObject.AddOrGetComponent<CollideBullet>();
		BuffCollide1 = OrangeBattleUtility.FindChildRecursive(ref childs, BuffCollideName1, true).gameObject.AddOrGetComponent<CollideBulletHitSelf>();
		BuffCollide2 = OrangeBattleUtility.FindChildRecursive(ref childs, BuffCollideName2, true).gameObject.AddOrGetComponent<CollideBulletHitSelf>();
		BuffCollide3 = OrangeBattleUtility.FindChildRecursive(ref childs, BuffCollideName3, true).gameObject.AddOrGetComponent<CollideBulletHitSelf>();
		if (Skill1UseFx1 == null)
		{
			Skill1UseFx1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1UseFx1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1UseFx2 == null)
		{
			Skill1UseFx2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1UseFx2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1UseFx3 == null)
		{
			Skill1UseFx3 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1UseFx3", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill2UseFx == null)
		{
			Skill2UseFx = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill2UseFx", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill3UseFx == null)
		{
			Skill3UseFx = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill3UseFx", true).gameObject.AddOrGetComponent<ParticleSystem>();
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
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(PreTargetFx, 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Skill0UseFx, 8);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Skill0PreFx, 2);
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
			if (nSet == 3)
			{
				Skill1ShootAngle = netSyncData.nParam0;
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
		case MainStatus.IdleStand:
			_velocity.x = 0;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				_velocity = VInt3.zero;
				ActionTimes = (int)((MaxPos.x - MinPos.x) / (float)CloudDis) + 1;
				int[] TempArray = new int[ActionTimes];
				CloudOrder = new int[ActionTimes];
				for (int j = 0; j < ActionTimes; j++)
				{
					TempArray[j] = j;
				}
				int num2 = 0;
				while (TempArray.Length != 0)
				{
					int i = UnityEngine.Random.Range(0, TempArray.Length);
					CloudOrder[num2] = TempArray[i];
					TempArray = Array.FindAll(TempArray, (int val) => val != TempArray[i]);
					num2++;
				}
				break;
			}
			case SubStatus.Phase1:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill0UseFx, new Vector3(MinPos.x + (float)(CloudOrder[0] * CloudDis), MaxPos.y - 2f, -1f), Quaternion.identity, Array.Empty<object>());
				CloudOrder = Array.FindAll(CloudOrder, (int val) => val != CloudOrder[0]);
				break;
			case SubStatus.Phase2:
				CanUseSkill0 = true;
				Skill0ActionTimes = Skill0ShootTimes;
				Skill0PreActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0ShootInterval * 20f);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE05", "bs043_drmvirus02_1");
				SwitchFx(Skill1UseFx1);
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				SwitchFx(Skill1UseFx1, false);
				SwitchFx(Skill1UseFx2);
				break;
			case SubStatus.Phase2:
				ActionTimes = Skill1ShootTimes;
				SwitchFx(Skill1UseFx3);
				break;
			case SubStatus.Phase3:
				SwitchFx(Skill1UseFx2, false);
				SwitchFx(Skill1UseFx3, false);
				HasActed = false;
				ActionAnimatorFrame = Skill1ShootFrame;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE05", "bs043_drmvirus09");
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				SwitchFx(Skill2UseFx);
				break;
			case SubStatus.Phase2:
				Skill2Angle = Skill2StartAngle;
				_animator.SetFloat(_HashAngle, Skill2Angle);
				Skill2AngleSpace = (Skill2EndAngle - Skill2StartAngle) / (Skill2ShootTime * 20f);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill2ShootTime * 20f);
				PlayLP(true);
				break;
			case SubStatus.Phase4:
				PlayLP(false);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase2:
				_collideBullet.UpdateBulletData(EnemyWeapons[4].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				SwitchFx(Skill3UseFx);
				_velocity.x = Skill3MoveSpeed * base.direction;
				EndPos = GetTargetPos();
				break;
			case SubStatus.Phase4:
				PlayBossSE("BossSE05", "bs043_drmvirus04_2");
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				PlayBossSE("BossSE05", "bs043_drmvirus07");
				break;
			case SubStatus.Phase2:
				Skill4Collide.Active(targetMask);
				EndPos = GetTargetPos();
				ShotAngle = Vector2.Angle(Vector2.up, EndPos - CatchTool.CatchTransform.position);
				_animator.SetFloat(_HashAngle, ShotAngle);
				break;
			case SubStatus.Phase3:
			{
				float num = (MinPos.x + MaxPos.x) / 2f;
				if ((NowPos.x < num && base.direction == -1) || (NowPos.x > num && base.direction == 1))
				{
					UpdateDirection(-base.direction);
				}
				Skill4Collide.BackToPool();
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill4ThrowTime * 20f);
				break;
			}
			case SubStatus.Phase4:
				Skill4Collide.BackToPool();
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayLP(false);
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				nDeadCount = 0;
				break;
			case SubStatus.Phase1:
				StartCoroutine(BossDieFlow(_transform.position, "FX_BOSS_EXPLODE2", false, false));
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
				_currentAnimationId = AnimationID.ANI_IDLE;
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
		case MainStatus.IdleStand:
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
			default:
				return;
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
			default:
				return;
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
				_currentAnimationId = AnimationID.ANI_SKILL3_END2;
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
				_currentAnimationId = AnimationID.ANI_SKILL4_END2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL4_END3;
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_DEAD;
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
			if (mainStatus == MainStatus.Skill1)
			{
				Skill1ShootAngle = OrangeBattleUtility.Random(0, 90);
				UploadEnemyStatus((int)mainStatus, false, new object[1] { Skill1ShootAngle });
			}
			else
			{
				UploadEnemyStatus((int)mainStatus);
			}
		}
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
				if (!CanUseSkill0)
				{
					skillpattern = SkillPattern.State1;
				}
				switch (skillpattern)
				{
				case SkillPattern.State1:
					mainStatus = MainStatus.Skill0;
					break;
				case SkillPattern.State2:
					mainStatus = MainStatus.Skill1;
					break;
				case SkillPattern.State3:
					mainStatus = MainStatus.Skill3;
					break;
				case SkillPattern.State4:
					mainStatus = MainStatus.Skill2;
					break;
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
			if (mainStatus == MainStatus.Skill1)
			{
				Skill1ShootAngle = OrangeBattleUtility.Random(0, 90);
				UploadEnemyStatus((int)mainStatus, false, new object[1] { Skill1ShootAngle });
			}
			else
			{
				UploadEnemyStatus((int)mainStatus);
			}
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
						if (_enemyAutoAimSystem.CheckContainPlayer(StageUpdate.runPlayers[i].GetTargetPoint()))
						{
							Target = StageUpdate.runPlayers[i];
						}
						break;
					}
				}
			}
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
				UpdateDirection();
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_004)
				{
					UpdateRandomState();
				}
				else
				{
					UpdateNextState();
				}
			}
			else
			{
				UpdateRandomState(MainStatus.IdleStand);
			}
			break;
		case MainStatus.IdleStand:
			if (_currentFrame > 1f)
			{
				SetStatus(MainStatus.Idle);
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
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill0, _subStatus);
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
					skillpattern = SkillPattern.State2;
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
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					EndPos = GetTargetPos();
					UpdateDirection();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPos.position);
					_animator.SetFloat(_HashAngle, ShotAngle);
					Vector3 pDirection2 = EndPos - ShootPos.position;
					RandomReflectBullet randomReflectBullet = BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos.position, pDirection2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as RandomReflectBullet;
					randomReflectBullet._HitReflectSE = new string[2] { "BossSE05", "bs043_drmvirus02_2" };
					randomReflectBullet.SetSeed(Skill1ShootAngle);
					Skill1ShootAngle++;
				}
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill1, _subStatus);
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
					skillpattern = SkillPattern.State3;
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
				if (_currentFrame > 3f)
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
				EndPos = GetTargetPos();
				UpdateDirection();
				ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPos.position);
				_animator.SetFloat(_HashAngle, ShotAngle);
				ActionTimes++;
				if (ActionTimes >= Skill2FireInterval)
				{
					ActionTimes = 0;
					Vector3 pDirection = EndPos - ShootPos.position;
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, ShootPos.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					skillpattern = SkillPattern.State2;
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
					SetStatus(MainStatus.Skill3, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
				{
					_velocity.x = 0;
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					UpdateDirection(-base.direction);
					AI_STATE aiState = AiState;
					if (aiState == AI_STATE.mob_004)
					{
						SetStatus(MainStatus.Idle);
					}
					else
					{
						SetStatus(MainStatus.Skill4);
					}
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					if (CatchTool.IsCatching)
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase3);
						PlayBossSE("BossSE05", "bs043_drmvirus05");
						base.SoundSource.PlaySE("BossSE05", "bs043_drmvirus06", 0.4f);
					}
					else
					{
						SetStatus(MainStatus.Skill4, SubStatus.Phase4);
					}
				}
				break;
			case SubStatus.Phase3:
				if (CatchTool.IsCatching && _currentFrame > 0.44f)
				{
					if (CatchTool.TargetOC.Controller.Collisions.right || CatchTool.TargetOC.Controller.Collisions.left)
					{
						(BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, CatchTool.TargetOC._transform.position, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
						CatchTool.ReleaseTarget();
					}
					else
					{
						CatchTool.MoveTargetWithForce(new VInt3(Skill4ThrowSpeed * base.direction, 0, 0));
					}
				}
				if ((!CatchTool.IsCatching || GameLogicUpdateManager.GameFrame > ActionFrame) && _currentFrame > 1f)
				{
					if (CatchTool.IsCatching)
					{
						CatchTool.ReleaseTarget();
					}
					skillpattern = SkillPattern.State4;
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					skillpattern = SkillPattern.State4;
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
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			}
			break;
		}
		if (CanUseSkill0 && Skill0ActionTimes > 0 && GameLogicUpdateManager.GameFrame >= Skill0PreActionFrame)
		{
			Skill0ShootPosition = GetTargetPos();
			Skill0ShootPosition.y = MaxPos.y - 2f;
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill0PreFx, Skill0ShootPosition, Quaternion.identity, Array.Empty<object>());
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>(PreTargetFx, Skill0ShootPosition, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector3.right, Vector3.down)), Array.Empty<object>()).SetEffect(Skill0ShootPosition.y - MinPos.y, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 1f, 0.5f);
			int item = GameLogicUpdateManager.GameFrame + 20;
			Skill0ShootPosition.y = MinPos.y;
			Skill0Magazine.Add(new ValueTuple<int, Vector3>(item, Skill0ShootPosition));
			for (int j = 1; j <= Skill0ShootTimes / 2; j++)
			{
				Skill0ShootPosition.y = MaxPos.y - 2f;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill0PreFx, Skill0ShootPosition + Vector3.right * Skill0ShootInterval * j, Quaternion.identity, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill0PreFx, Skill0ShootPosition + Vector3.left * Skill0ShootInterval * j, Quaternion.identity, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>(PreTargetFx, Skill0ShootPosition + Vector3.right * Skill0ShootInterval * j, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector3.right, Vector3.down)), Array.Empty<object>()).SetEffect(Skill0ShootPosition.y - MinPos.y, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 1f, 0.5f);
				MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>(PreTargetFx, Skill0ShootPosition + Vector3.left * Skill0ShootInterval * j, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector3.right, Vector3.down)), Array.Empty<object>()).SetEffect(Skill0ShootPosition.y - MinPos.y, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 1f, 0.5f);
				Skill0ShootPosition.y = MinPos.y;
				Skill0Magazine.Add(new ValueTuple<int, Vector3>(item, Skill0ShootPosition + Vector3.right * Skill0ShootInterval * j));
				Skill0Magazine.Add(new ValueTuple<int, Vector3>(item, Skill0ShootPosition + Vector3.left * Skill0ShootInterval * j));
			}
			Skill0PreActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0ShootCycle * 20f);
		}
		int num = 0;
		while (CanUseSkill0 && Skill0Magazine.Count > 0 && GameLogicUpdateManager.GameFrame >= Skill0Magazine[0].Item1 && num < Skill0ShootTimes)
		{
			(BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, Skill0Magazine[0].Item2, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
			Skill0Magazine.RemoveAt(0);
			num++;
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
		if (mainStatus != MainStatus.Skill4)
		{
			return;
		}
		switch (_subStatus)
		{
		case SubStatus.Phase2:
		{
			Collider2D collider2D = Physics2D.OverlapBox(CatchTool.CatchTransform.position, Vector2.one * 1.5f, 0f, LayerMask.GetMask("Player"));
			if ((bool)collider2D && (int)Hp > 0)
			{
				OrangeCharacter component = collider2D.GetComponent<OrangeCharacter>();
				CatchTool.CatchTarget(component);
			}
			if (CatchTool.IsCatching)
			{
				CatchTool.MoveTarget();
			}
			break;
		}
		case SubStatus.Phase3:
			if (CatchTool.IsCatching && _currentFrame < 0.44f)
			{
				CatchTool.MoveTarget();
				Controller.LogicPosition = new VInt3(_transform.position);
			}
			break;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			CanUseSkill0 = false;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			Skill4Collide.UpdateBulletData(EnemyWeapons[5].BulletData);
			Skill4Collide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			BuffCollide1.UpdateBulletData(EnemyWeapons[8].BulletData);
			BuffCollide1.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			BuffCollide2.UpdateBulletData(EnemyWeapons[9].BulletData);
			BuffCollide2.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			BuffCollide3.UpdateBulletData(EnemyWeapons[10].BulletData);
			BuffCollide3.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			switch (AiState)
			{
			case AI_STATE.mob_002:
				SetStatus(MainStatus.Debut);
				break;
			case AI_STATE.mob_003:
				SetStatus(MainStatus.Debut);
				break;
			case AI_STATE.mob_004:
				base.AllowAutoAim = true;
				SetStatus(MainStatus.Idle);
				break;
			default:
				SetStatus(MainStatus.Debut);
				break;
			}
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
			StopAllFX();
			CanUseSkill0 = false;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CLOSE_FX);
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)Skill4Collide)
			{
				Skill4Collide.BackToPool();
			}
			if (CatchTool.IsCatching)
			{
				CatchTool.ReleaseTarget();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_004)
		{
			BattleInfoUI.Instance.SwitchOptionBtn(true);
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

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y, 0f);
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

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_002:
			Skill0ShootTimes = 3;
			Skill0ShootCycle = 15f;
			Skill1ShootTimes = 2;
			Skill2ShootTime = 4f;
			base.DeadPlayCompleted = true;
			break;
		case AI_STATE.mob_003:
			Skill0ShootTimes = 3;
			Skill0ShootCycle = 20f;
			Skill1ShootTimes = 1;
			Skill2ShootTime = 3f;
			base.DeadPlayCompleted = true;
			break;
		case AI_STATE.mob_004:
			Skill0ShootTimes = 3;
			Skill0ShootCycle = 20f;
			Skill1ShootTimes = 1;
			Skill2ShootTime = 3f;
			base.DeadPlayCompleted = false;
			break;
		default:
			Skill0ShootTimes = 5;
			Skill0ShootCycle = 10f;
			Skill1ShootTimes = 3;
			Skill2ShootTime = 5f;
			base.DeadPlayCompleted = true;
			break;
		}
	}

	private void SwitchFx(ParticleSystem Fx, bool onoff = true)
	{
		if ((bool)Fx)
		{
			if (onoff)
			{
				Fx.Play();
			}
			else
			{
				Fx.Stop();
			}
		}
		else
		{
			Debug.LogError(string.Concat("特效載入有誤，目前狀態是 ", _mainStatus, "的階段 ", _subStatus));
		}
	}

	private void StopAllFX()
	{
		SwitchFx(Skill1UseFx1, false);
		SwitchFx(Skill1UseFx2, false);
		SwitchFx(Skill1UseFx3, false);
		SwitchFx(Skill2UseFx, false);
		SwitchFx(Skill3UseFx, false);
	}
}
