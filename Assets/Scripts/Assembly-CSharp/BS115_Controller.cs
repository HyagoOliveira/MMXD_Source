#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS115_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		MAX_SUBSTATUS = 7
	}

	private enum SkillPattern
	{
		State1 = 1,
		State2 = 2,
		State3 = 3,
		State4 = 4,
		State5 = 5,
		State6 = 6
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_SKILL0_START1 = 2,
		ANI_SKILL0_LOOP1 = 3,
		ANI_SKILL0_END1 = 4,
		ANI_SKILL0_START2 = 5,
		ANI_SKILL0_LOOP2 = 6,
		ANI_SKILL0_END2 = 7,
		ANI_SKILL1_START = 8,
		ANI_SKILL1_LOOP = 9,
		ANI_SKILL1_END = 10,
		ANI_SKILL2_START1 = 11,
		ANI_SKILL2_LOOP1 = 12,
		ANI_SKILL2_END1 = 13,
		ANI_SKILL2_START2 = 14,
		ANI_SKILL2_LOOP2 = 15,
		ANI_SKILL2_END2 = 16,
		ANI_HURT = 17,
		ANI_DEAD_START1 = 18,
		ANI_DEAD_LOOP1 = 19,
		ANI_DEAD_START2 = 20,
		ANI_DEAD_LOOP2 = 21,
		MAX_ANIMATION_ID = 22
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

	[Header("待機等待Frame")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("深紅爆發")]
	[SerializeField]
	private float fSkill0WaitTime = 2f;

	[SerializeField]
	private float fSkill0ActFrame = 0.2f;

	[SerializeField]
	private int nSkill0ActTimes = 2;

	[SerializeField]
	private ParticleSystem psSkill0UseFX;

	private CollideBulletHitSelf Skill0BuffCollide;

	[SerializeField]
	private string BuffCollideName = "BuffCollide";

	[Header("億兆風暴")]
	[SerializeField]
	private float fSkill1ActTime = 5f;

	[SerializeField]
	private float fSkill1WaitTime = 1f;

	[SerializeField]
	private string sSkill1PreTargetFx = "fxuse_FERAMU_000";

	[SerializeField]
	private string sSkill1EndFx = "fxuse_FERAMU_001";

	private Vector3[] Skill1AtkPos = new Vector3[5];

	[Header("全速渦輪衝擊")]
	[SerializeField]
	private float fSkill2WaitTime = 0.5f;

	[SerializeField]
	private int nSkill2DashSpd = 7500;

	[SerializeField]
	private Vector2 Skill2WallDis = new Vector2(2f, 1f);

	[SerializeField]
	private string sSkill2PreTargetFx = "fxuseTarget";

	[SerializeField]
	private ParticleSystem psSkill2UseFX1;

	[SerializeField]
	private ParticleSystem psSkill2UseFX2;

	[SerializeField]
	private ParticleSystem psSkill2UseFX3;

	[SerializeField]
	private ParticleSystem psSkill2UseFX4;

	[SerializeField]
	private float fSkill2PreFXScale = 1f;

	[Header("AI控制")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SkillPattern skillpattern = SkillPattern.State1;

	private SkillPattern nextState = SkillPattern.State1;

	[SerializeField]
	private bool bIsExMode;

	[Header("Debug用")]
	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	[SerializeField]
	private BitArray bskill2 = new BitArray(8);

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
		_animationHash = new int[22];
		_animationHash[0] = Animator.StringToHash("BS115@idl_loop");
		_animationHash[2] = Animator.StringToHash("BS115@FERAMU_SKILL1_CASTING1");
		_animationHash[3] = Animator.StringToHash("BS115@FERAMU_SKILL1_LOOP1");
		_animationHash[4] = Animator.StringToHash("BS115@FERAMU_SKILL1_CASTOUT1");
		_animationHash[5] = Animator.StringToHash("BS115@FERAMU_SKILL1_CASTING2");
		_animationHash[6] = Animator.StringToHash("BS115@FERAMU_SKILL1_LOOP2");
		_animationHash[7] = Animator.StringToHash("BS115@FERAMU_SKILL1_CASTOUT2");
		_animationHash[8] = Animator.StringToHash("BS115@FERAMU_SKILL2_CASTING1");
		_animationHash[9] = Animator.StringToHash("BS115@FERAMU_SKILL2_LOOP1");
		_animationHash[10] = Animator.StringToHash("BS115@FERAMU_SKILL2_CASTOUT1");
		_animationHash[11] = Animator.StringToHash("BS115@FERAMU_SKILL3_CASTING1");
		_animationHash[12] = Animator.StringToHash("BS115@FERAMU_SKILL3_LOOP1");
		_animationHash[13] = Animator.StringToHash("BS115@FERAMU_SKILL3_CASTOUT1");
		_animationHash[14] = Animator.StringToHash("BS115@FERAMU_SKILL3_CASTING2");
		_animationHash[15] = Animator.StringToHash("BS115@FERAMU_SKILL3_LOOP2");
		_animationHash[16] = Animator.StringToHash("BS115@FERAMU_SKILL3_CASTOUT2");
		_animationHash[17] = Animator.StringToHash("BS115@hurt_loop");
		_animationHash[18] = Animator.StringToHash("BS115@fall_start");
		_animationHash[19] = Animator.StringToHash("BS115@fall_loop");
		_animationHash[20] = Animator.StringToHash("BS115@fall_end");
		_animationHash[21] = Animator.StringToHash("BS115@fall_end_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		Skill0BuffCollide = OrangeBattleUtility.FindChildRecursive(ref childs, BuffCollideName, true).gameObject.AddOrGetComponent<CollideBulletHitSelf>();
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "L WeaponPoint", true);
		}
		if (psSkill0UseFX == null)
		{
			psSkill0UseFX = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill0UseFX", true).GetComponent<ParticleSystem>();
		}
		if (psSkill2UseFX1 == null)
		{
			psSkill2UseFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill2UseFX1", true).GetComponent<ParticleSystem>();
		}
		if (psSkill2UseFX2 == null)
		{
			psSkill2UseFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill2UseFX2", true).GetComponent<ParticleSystem>();
		}
		if (psSkill2UseFX3 == null)
		{
			psSkill2UseFX3 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill2UseFX3", true).GetComponent<ParticleSystem>();
		}
		if (psSkill2UseFX4 == null)
		{
			psSkill2UseFX4 = OrangeBattleUtility.FindChildRecursive(ref childs, "psSkill2UseFX4", true).GetComponent<ParticleSystem>();
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
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sSkill2PreTargetFx, 4);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sSkill1PreTargetFx, 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sSkill1EndFx, 3);
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
			case SubStatus.Phase0:
				Debug.Log("1");
				break;
			case SubStatus.Phase2:
				Debug.Log("2");
				break;
			}
			break;
		case MainStatus.Idle:
			_velocity.x = 0;
			SwitchFx(psSkill2UseFX1, true);
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ActionTimes = nSkill0ActTimes;
				break;
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				UpdateDirection((!(NowPos.x > CenterPos.x)) ? 1 : (-1));
				break;
			case SubStatus.Phase2:
				SwitchCollideBullet(Skill0BuffCollide, true, -1, 1);
				SwitchFx(psSkill0UseFX, true);
				break;
			case SubStatus.Phase3:
				SwitchCollideBullet(Skill0BuffCollide, false);
				break;
			case SubStatus.Phase4:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill0WaitTime * 20f);
				break;
			case SubStatus.Phase5:
				EndPos = GetTargetPos();
				HasActed = false;
				ActionAnimatorFrame = fSkill0ActFrame;
				LeanTween.move(psSkill0UseFX.gameObject, ShootPos, fSkill0ActFrame);
				LeanTween.scale(psSkill0UseFX.gameObject, Vector3.one, fSkill0ActFrame);
				break;
			case SubStatus.Phase6:
				psSkill0UseFX.transform.localPosition = Vector3.up;
				psSkill0UseFX.transform.localScale = Vector3.one * 3f;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = VInt3.zero;
				UpdateDirection((!(NowPos.x > CenterPos.x)) ? 1 : (-1));
				ActionTimes = 0;
				PlayBossSE("BossSE06", "bs049_ferham08");
				break;
			case SubStatus.Phase1:
			{
				int[] array2 = new int[0];
				switch (ActionTimes)
				{
				case 0:
					array2 = new int[3] { 0, 2, 4 };
					break;
				case 1:
					array2 = new int[2] { 1, 3 };
					break;
				case 2:
					array2 = new int[3] { 2, 3, 4 };
					break;
				case 3:
					array2 = new int[3] { 0, 1, 2 };
					break;
				}
				PlayBossSE("BossSE06", "bs049_ferham09");
				for (int j = 0; j < array2.Length; j++)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sSkill1PreTargetFx, Skill1AtkPos[array2[j]], Quaternion.identity, Array.Empty<object>());
				}
				break;
			}
			case SubStatus.Phase3:
			{
				int[] array = new int[0];
				switch (ActionTimes)
				{
				case 0:
					array = new int[3] { 0, 2, 4 };
					break;
				case 1:
					array = new int[2] { 1, 3 };
					break;
				case 2:
					array = new int[3] { 2, 3, 4 };
					break;
				case 3:
					array = new int[3] { 0, 1, 2 };
					break;
				}
				PlayBossSE("BossSE06", "bs049_ferham10");
				for (int i = 0; i < array.Length; i++)
				{
					(BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, Skill1AtkPos[array[i]], Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sSkill1EndFx, Skill1AtkPos[array[i]], Quaternion.identity, Array.Empty<object>());
				}
				break;
			}
			case SubStatus.Phase4:
				SwitchFx(psSkill2UseFX1, true);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill1WaitTime * 20f);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill2WaitTime * 20f);
				_velocity = VInt3.zero;
				ActionTimes = 0;
				PlayPreRouteFx();
				break;
			case SubStatus.Phase1:
				SwitchCollideBullet(_collideBullet, true, 4);
				break;
			case SubStatus.Phase2:
				EndPos = GetTargetPos();
				switch (ActionTimes)
				{
				case 0:
					PlayBossSE("BossSE06", "bs049_ferham11");
					EndPos = new Vector3((NowPos.x > CenterPos.x) ? (MinPos.x + Skill2WallDis.x) : (MaxPos.x - Skill2WallDis.x), MaxPos.y - Skill2WallDis.y, 0f);
					break;
				case 1:
					PlayBossSE("BossSE06", "bs049_ferham11");
					EndPos = new Vector3((NowPos.x > CenterPos.x) ? (MinPos.x + Skill2WallDis.x) : (MaxPos.x - Skill2WallDis.x), MinPos.y + 0.1f, 0f);
					break;
				case 2:
					PlayBossSE("BossSE06", "bs049_ferham11");
					EndPos = new Vector3((NowPos.x > CenterPos.x) ? (MinPos.x + Skill2WallDis.x) : (MaxPos.x - Skill2WallDis.x), MinPos.y + 0.1f, 0f);
					break;
				}
				TargetPos = new VInt3(EndPos);
				UpdateDirection();
				_velocity = new VInt3((EndPos - NowPos).normalized) * nSkill2DashSpd * 0.001f;
				StartPos = NowPos;
				MoveDis = Vector2.Distance(EndPos, StartPos);
				SwitchFx(psSkill2UseFX3, true);
				SwitchFx(psSkill2UseFX4, true);
				break;
			case SubStatus.Phase3:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase5:
				PlayBossSE("BossSE06", "bs049_ferham12");
				_velocity = VInt3.zero;
				EndPos.y = MaxPos.y - Skill2WallDis.y;
				StartPos = NowPos;
				MoveDis = Vector2.Distance(EndPos, StartPos);
				_velocity.y = nSkill2DashSpd;
				SwitchFx(psSkill2UseFX2, true);
				SwitchFx(psSkill2UseFX3, false);
				SwitchFx(psSkill2UseFX4, false);
				break;
			case SubStatus.Phase6:
				SwitchCollideBullet(_collideBullet, true, 0);
				_velocity = VInt3.zero;
				UpdateDirection((!(NowPos.x > CenterPos.x)) ? 1 : (-1));
				SwitchFx(psSkill2UseFX2, false);
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE06", "bs049_ferham13", 0.5f);
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				nDeadCount = 0;
				IgnoreGravity = false;
				Controller.collisionMask = 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer;
				break;
			case SubStatus.Phase3:
				base.DeadPlayCompleted = true;
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
				_currentAnimationId = AnimationID.ANI_DEBUT;
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
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_START1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_END1;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL0_START2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP2;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL0_END2;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_START;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_END;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_START1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_END1;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_START2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP2;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL2_END2;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEAD_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEAD_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEAD_START2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEAD_LOOP2;
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
				skillpattern = nextState;
				switch (skillpattern)
				{
				case SkillPattern.State1:
					mainStatus = (bIsExMode ? MainStatus.Skill1 : MainStatus.Skill0);
					nextState = SkillPattern.State2;
					break;
				case SkillPattern.State2:
					mainStatus = (bIsExMode ? MainStatus.Skill2 : MainStatus.Skill2);
					nextState = SkillPattern.State3;
					break;
				case SkillPattern.State3:
					mainStatus = (bIsExMode ? MainStatus.Skill1 : MainStatus.Skill0);
					nextState = SkillPattern.State4;
					break;
				case SkillPattern.State4:
					mainStatus = (bIsExMode ? MainStatus.Skill0 : MainStatus.Skill2);
					nextState = SkillPattern.State5;
					break;
				case SkillPattern.State5:
					mainStatus = (bIsExMode ? MainStatus.Skill2 : MainStatus.Skill1);
					nextState = ((!bIsExMode) ? SkillPattern.State1 : SkillPattern.State6);
					break;
				case SkillPattern.State6:
					mainStatus = MainStatus.Skill2;
					nextState = SkillPattern.State1;
					break;
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
				if (_currentFrame > 1f)
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
					PlayBossSE("BossSE06", "bs049_ferham03");
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
				SwitchFx(psSkill2UseFX1, false);
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
				SetStatus(MainStatus.Skill0, SubStatus.Phase1);
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
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (!HasActed && _currentFrame > ActionAnimatorFrame)
				{
					HasActed = true;
					Vector3 pDirection = EndPos - ShootPos.position;
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, ShootPos, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask, true);
					SwitchFx(psSkill0UseFX, false);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			case SubStatus.Phase3:
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Skill1, SubStatus.Phase1);
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
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					if (++ActionTimes < 4)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
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
				if (Vector2.Distance(StartPos, NowPos) >= MoveDis)
				{
					_transform.position = EndPos;
					Controller.LogicPosition = new VInt3(EndPos);
					if (++ActionTimes < 3)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase5);
					}
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Vector2.Distance(StartPos, NowPos) >= MoveDis)
				{
					_transform.position = EndPos;
					Controller.LogicPosition = new VInt3(EndPos);
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
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 0.4f)
				{
					if (nDeadCount > 10)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase4);
					}
					else
					{
						nDeadCount++;
					}
				}
				break;
			}
			break;
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
			IgnoreGravity = true;
			SwitchCollideBullet(_collideBullet, true, 0);
			SwitchCollideBullet(Skill0BuffCollide, false, 1);
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
			SwitchFx(psSkill0UseFX, false);
			SwitchFx(psSkill2UseFX1, false);
			SwitchFx(psSkill2UseFX2, false);
			SwitchFx(psSkill2UseFX3, false);
			SwitchFx(psSkill2UseFX4, false);
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

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
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
			return;
		}
		MaxPos = new Vector3(raycastHit2D2.point.x, raycastHit2D3.point.y, 0f);
		MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D4.point.y, 0f);
		CenterPos = (MaxPos + MinPos) / 2f;
		Skill2WallDis = new Vector2((NowPos.x > CenterPos.x) ? raycastHit2D2.distance : raycastHit2D.distance, raycastHit2D3.distance);
		float num = (MaxPos.x - MinPos.x) / 5f;
		Skill1AtkPos[0] = new Vector3(CenterPos.x - num * 2f, MinPos.y, 0f);
		Skill1AtkPos[1] = new Vector3(CenterPos.x - num, MinPos.y, 0f);
		Skill1AtkPos[2] = new Vector3(CenterPos.x, MinPos.y, 0f);
		Skill1AtkPos[3] = new Vector3(CenterPos.x + num, MinPos.y, 0f);
		Skill1AtkPos[4] = new Vector3(CenterPos.x + num * 2f, MinPos.y, 0f);
	}

	private void PlayPreRouteFx()
	{
		float x = NowPos.x;
		float x2 = CenterPos.x;
		StartPos = NowPos + Vector3.up * (Controller.Collider2D.size.y / 2f);
		for (int i = 0; i < 3; i++)
		{
			switch (i)
			{
			case 0:
				EndPos = new Vector3((StartPos.x > CenterPos.x) ? (MinPos.x + Skill2WallDis.x) : (MaxPos.x - Skill2WallDis.x), MaxPos.y - Skill2WallDis.y + Controller.Collider2D.size.y / 2f, 0f);
				break;
			case 1:
				EndPos = new Vector3((StartPos.x > CenterPos.x) ? (MinPos.x + Skill2WallDis.x) : (MaxPos.x - Skill2WallDis.x), MinPos.y + 0.1f + Controller.Collider2D.size.y / 2f, 0f);
				break;
			case 2:
				EndPos = new Vector3((StartPos.x > CenterPos.x) ? (MinPos.x + Skill2WallDis.x) : (MaxPos.x - Skill2WallDis.x), MinPos.y + 0.1f + Controller.Collider2D.size.y / 2f, 0f);
				break;
			}
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>(sSkill2PreTargetFx, StartPos, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector3.right, EndPos - StartPos)), Array.Empty<object>()).SetEffect(Vector2.Distance(EndPos, StartPos), new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 1f, fSkill2PreFXScale);
			StartPos = EndPos;
		}
	}

	private void Skill1PlayFxEnd(object obj)
	{
		BulletBase bulletBase = null;
		if (obj != null)
		{
			bulletBase = obj as BulletBase;
		}
		if (bulletBase == null)
		{
			Debug.LogError("子彈資料有誤。");
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sSkill1EndFx, bulletBase.transform.position, Quaternion.identity, Array.Empty<object>());
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if (!bIsExMode && (int)obscuredInt <= (int)MaxHp / 2 && (int)Hp >= 0)
		{
			bIsExMode = true;
		}
		return obscuredInt;
	}
}
