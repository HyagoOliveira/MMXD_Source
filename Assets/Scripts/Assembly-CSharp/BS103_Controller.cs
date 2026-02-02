#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS103_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		Skill7 = 9,
		Skill8 = 10,
		Skill9 = 11,
		Skill10 = 12,
		Die = 13
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
		Phase11 = 11,
		Phase12 = 12,
		Phase13 = 13,
		Phase14 = 14,
		Phase15 = 15,
		MAX_SUBSTATUS = 16
	}

	private enum AttackPattern
	{
		UpperPunch = 0,
		CrouchPunch = 1,
		DragonPunch = 2,
		GroundPunch = 3,
		KneeKick = 4,
		LowKick = 5,
		DownKick = 6,
		CycleKick = 7,
		FrontKick = 8,
		Body = 9
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
		ANI_DEBUT_LOOP = 2,
		ANI_SKILL0_START1 = 3,
		ANI_SKILL0_LOOP1 = 4,
		ANI_SKILL0_START2 = 5,
		ANI_SKILL0_LOOP2 = 6,
		ANI_SKILL0_END2 = 7,
		ANI_SKILL1_START1 = 8,
		ANI_SKILL1_LOOP1 = 9,
		ANI_SKILL1_START2 = 10,
		ANI_SKILL1_LOOP2 = 11,
		ANI_SKILL1_END2 = 12,
		ANI_SKILL2_START1 = 13,
		ANI_SKILL2_LOOP1 = 14,
		ANI_SKILL2_START2 = 15,
		ANI_SKILL2_LOOP2 = 16,
		ANI_SKILL2_START3 = 17,
		ANI_SKILL2_LOOP3 = 18,
		ANI_SKILL2_START4 = 19,
		ANI_SKILL2_START5 = 20,
		ANI_SKILL2_LOOP5 = 21,
		ANI_SKILL2_START6 = 22,
		ANI_SKILL2_LOOP6 = 23,
		ANI_SKILL2_END6 = 24,
		ANI_SKILL2_START7 = 25,
		ANI_SKILL2_LOOP7 = 26,
		ANI_SKILL2_END7 = 27,
		ANI_SKILL3_START1 = 28,
		ANI_SKILL3_LOOP1 = 29,
		ANI_SKILL3_START2 = 30,
		ANI_SKILL3_LOOP2 = 31,
		ANI_SKILL3_END2 = 32,
		ANI_SKILL4_START1 = 33,
		ANI_SKILL4_LOOP1 = 34,
		ANI_SKILL4_START2 = 35,
		ANI_SKILL4_LOOP2 = 36,
		ANI_SKILL4_START3 = 37,
		ANI_SKILL4_LOOP3 = 38,
		ANI_SKILL4_START4 = 39,
		ANI_SKILL4_LOOP4 = 40,
		ANI_SKILL4_START5 = 41,
		ANI_SKILL4_START6 = 42,
		ANI_SKILL4_START7 = 43,
		ANI_SKILL4_LOOP7 = 44,
		ANI_SKILL4_END7 = 45,
		ANI_SKILL5_START = 46,
		ANI_SKILL5_LOOP = 47,
		ANI_SKILL5_END = 48,
		ANI_SKILL6_START1 = 49,
		ANI_SKILL6_LOOP1 = 50,
		ANI_SKILL6_LOOP2 = 51,
		ANI_SKILL6_END2 = 52,
		ANI_SKILL7_START = 53,
		ANI_SKILL8_LOOP = 54,
		ANI_HURT = 55,
		ANI_DEAD = 56,
		MAX_ANIMATION_ID = 57
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

	private int[] DefaultSkillCard = new int[7] { 0, 1, 2, 3, 4, 5, 6 };

	private static int[] DefaultRangedSkillCard = new int[3] { 3, 4, 5 };

	private List<int> RangedSKC = new List<int>();

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	[Header("通用")]
	private Vector3 StartPos;

	private Vector3 EndPos;

	[SerializeField]
	private Transform ShootPos;

	private float MaxXPos;

	private float MinXPos;

	private float GroundYPos;

	private int ActionTimes;

	private float ActionAnimatorFrame;

	private int ActionFrame;

	private bool HasActed;

	private float ShotAngle;

	private readonly int _HashAngle = Animator.StringToHash("Angle");

	[SerializeField]
	private CatchPlayerTool CatchTool;

	private CollideBullet UpperPunchCollider;

	private CollideBullet CrouchPunchCollider;

	private CollideBullet DragonPunchCollider;

	private CollideBullet GroundPunchCollider;

	private CollideBullet KneeKickCollider;

	private CollideBullet LowKickCollider;

	private CollideBullet DownKickCollider;

	private CollideBullet CycleKickCollider;

	private CollideBullet FrontKickCollider;

	[SerializeField]
	private string UpperPunch;

	[SerializeField]
	private string CrouchPunch;

	[SerializeField]
	private string DragonPunch;

	[SerializeField]
	private string GroundPunch;

	[SerializeField]
	private string KneeKick;

	[SerializeField]
	private string LowKick;

	[SerializeField]
	private string DownKick;

	[SerializeField]
	private string CycleKick;

	[SerializeField]
	private string FrontKick;

	[SerializeField]
	private ParticleSystem Skill0Fx1;

	[SerializeField]
	private ParticleSystem Skill0Fx2;

	[SerializeField]
	private ParticleSystem Skill1Fx1L;

	[SerializeField]
	private ParticleSystem Skill1Fx1R;

	[SerializeField]
	private ParticleSystem Skill1Fx2L;

	[SerializeField]
	private ParticleSystem Skill1Fx2R;

	[SerializeField]
	private ParticleSystem Skill1Fx3;

	[SerializeField]
	private ParticleSystem CycleKickFx;

	[SerializeField]
	private ParticleSystem ExDragonFx;

	[SerializeField]
	private ParticleSystem ExGroundFx1;

	[SerializeField]
	private ParticleSystem ExGroundFx2;

	[SerializeField]
	private ParticleSystem Skill5Fx1;

	[SerializeField]
	private ParticleSystem Skill5Fx1_1;

	[SerializeField]
	private ParticleSystem Skill5Fx2;

	[SerializeField]
	private ParticleSystem Skill5Fx3;

	[SerializeField]
	private ParticleSystem Skill5Fx4;

	[SerializeField]
	private TrailRenderer Skill5Fx2_1;

	[SerializeField]
	private string Skill5Fx5 = "fxduring_AKUMA_004";

	[SerializeField]
	private ParticleSystem Skill6Fx1;

	[SerializeField]
	private ParticleSystem Skill6Fx2;

	[SerializeField]
	private ParticleSystem Skill6Fx3;

	[SerializeField]
	private ParticleSystem Skill6Fx5;

	[SerializeField]
	private string Skill6Fx4 = "fxuse_AKUMA_017";

	[SerializeField]
	private ParticleSystem ExModeFx1;

	[SerializeField]
	private ParticleSystem ExModeFx1_1;

	[SerializeField]
	private ParticleSystem ExModeFx2L;

	[SerializeField]
	private ParticleSystem ExModeFx2R;

	[SerializeField]
	private string Skill6UIFx1 = "fxuse_AKUMA_012";

	[SerializeField]
	private string Skill6UIFx2 = "fxuse_AKUMA_013";

	[SerializeField]
	private string Skill6UIFx3 = "fxuse_AKUMA_014";

	[SerializeField]
	private string Skill6UIFx4 = "fxuse_AKUMA_015";

	[Header("待機")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("豪波動拳")]
	[SerializeField]
	private int Skill0ShootTimes = 1;

	[SerializeField]
	private float Skill0ShootFrame = 0.6f;

	[Header("灼熱波動拳")]
	[SerializeField]
	private int Skill1ShootTimes = 1;

	[SerializeField]
	private float Skill1ShootFrame = 0.6f;

	[Header("豪昇龍拳")]
	[SerializeField]
	private int Skill2MoveSpeed1 = 4000;

	[SerializeField]
	private int Skill2JumpSpeed1 = 10000;

	[SerializeField]
	private int Skill2MoveSpeed2 = 3000;

	[SerializeField]
	private int Skill2JumpSpeed2 = 12000;

	[Header("龍捲斬空腳")]
	[SerializeField]
	private int Skill3MoveSpeed1 = 4000;

	[SerializeField]
	private int Skill3JumpSpeed1 = 10000;

	[Header("空中龍捲斬空腳")]
	[SerializeField]
	private int Skill4MoveSpeed1 = 4000;

	[SerializeField]
	private int Skill4JumpSpeed1 = 10000;

	[SerializeField]
	private int Skill4MoveSpeed2 = 3000;

	[SerializeField]
	private int Skill4JumpSpeed2 = 12000;

	[SerializeField]
	private int Skill4MoveSpeed3 = 3000;

	[SerializeField]
	private int Skill4JumpSpeed3 = 14000;

	[SerializeField]
	private float Skill4UseFrame = 0.45f;

	[Header("阿修羅閃空")]
	[SerializeField]
	private int Skill5MoveSpeed1 = 9000;

	[Header("瞬獄殺")]
	private bool FirstUseSkill6;

	private bool SecondUseSkill6;

	private int UseSkill6Times;

	[SerializeField]
	private float Skill6Time1 = 8f;

	[SerializeField]
	private float Skill6Time2 = 1f;

	[SerializeField]
	private int Skill6EventID1 = 999;

	[SerializeField]
	private int Skill6EventID2 = 998;

	[SerializeField]
	private int Skill6MoveSpeed = 22500;

	[SerializeField]
	private int Skill6AtkTimes = 2;

	private CameraControl StageMainCamera;

	[SerializeField]
	private float Skill6ZoomInFOV = 18f;

	private float OriginFOV = 22f;

	[Header("移動")]
	[SerializeField]
	private int Skill8MoveSpeed = 6000;

	[SerializeField]
	private float Skill8UseTime = 1f;

	[SerializeField]
	private float Skill8UseDis = 1f;

	[Header("昇龍拳")]
	private bool UseSkill9;

	[Header("3圈龍捲腳")]
	private bool UseSkill10;

	[SerializeField]
	private int Skill10UseTimes = 3;

	[SerializeField]
	private int Skill10MoveSpeed = 3000;

	[Header("進入EX狀態")]
	private bool UseSkill7;

	private CollideBulletHitSelf BuffCollide;

	[SerializeField]
	private string BuffCollideName = "BuffCollide";

	[Header("死亡")]
	[SerializeField]
	private string TeleportOutFxName = "fx_bs086_teleport_out";

	[Header("AI控制")]
	[SerializeField]
	private bool EXMode;

	[SerializeField]
	private float XDisJudge1 = 5f;

	[SerializeField]
	private float Skill3JudgeDis = 2f;

	[SerializeField]
	private float YDisJudge1 = 4f;

	private SkillPattern skillpattern;

	private int State1Count;

	[SerializeField]
	private int ExModeHp = 50;

	[SerializeField]
	private int FirstUseHp = 40;

	[SerializeField]
	private int SecondUseHp = 20;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private bool bPlay09;

	private Coroutine tZoomInOutCoroutine;

	private Vector3 NowPos
	{
		get
		{
			return _transform.position;
		}
	}

	private void Play09Loop(bool sw)
	{
		if (sw)
		{
			PlayBossSE("bs042_gouki09_lg");
			bPlay09 = true;
		}
		else
		{
			PlayBossSE("bs042_gouki09_stop");
			bPlay09 = false;
		}
	}

	private void PlayBossSE(string cue)
	{
		PlaySE("BossSE05", cue);
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
			case "Skill7":
				NextSkill = MainStatus.Skill7;
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
		_animationHash = new int[57];
		_animationHash[0] = Animator.StringToHash("BS103@IDLE_LOOP");
		_animationHash[1] = Animator.StringToHash("BS103@DEBUT_START");
		_animationHash[2] = Animator.StringToHash("BS103@DEBUT_STANDBY_LOOP");
		_animationHash[3] = Animator.StringToHash("BS103@AKUMA_SKILL1_CASTING1");
		_animationHash[4] = Animator.StringToHash("BS103@AKUMA_SKILL1_CASTLOOP1");
		_animationHash[5] = Animator.StringToHash("BS103@AKUMA_SKILL1_CASTING2");
		_animationHash[6] = Animator.StringToHash("BS103@AKUMA_SKILL1_CASTLOOP2");
		_animationHash[7] = Animator.StringToHash("BS103@AKUMA_SKILL1_CASTOUT1");
		_animationHash[8] = Animator.StringToHash("BS103@AKUMA_SKILL2_CASTING1");
		_animationHash[9] = Animator.StringToHash("BS103@AKUMA_SKILL2_CASTLOOP1");
		_animationHash[10] = Animator.StringToHash("BS103@AKUMA_SKILL2_CASTING2");
		_animationHash[11] = Animator.StringToHash("BS103@AKUMA_SKILL2_CASTLOOP2");
		_animationHash[12] = Animator.StringToHash("BS103@AKUMA_SKILL2_CASTOUT1");
		_animationHash[13] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTING1");
		_animationHash[14] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTLOOP1");
		_animationHash[15] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTING2");
		_animationHash[16] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTLOOP2");
		_animationHash[17] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTING3");
		_animationHash[18] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTLOOP3");
		_animationHash[19] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTING4");
		_animationHash[20] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTING5");
		_animationHash[21] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTLOOP4");
		_animationHash[22] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTING6");
		_animationHash[23] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTLOOP5");
		_animationHash[24] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTOUT1");
		_animationHash[25] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTING7");
		_animationHash[26] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTLOOP6");
		_animationHash[27] = Animator.StringToHash("BS103@AKUMA_SKILL3_CASTOUT2");
		_animationHash[28] = Animator.StringToHash("BS103@AKUMA_SKILL4_CASTING1");
		_animationHash[29] = Animator.StringToHash("BS103@AKUMA_SKILL4_CASTLOOP1");
		_animationHash[30] = Animator.StringToHash("BS103@AKUMA_SKILL4_CASTING2");
		_animationHash[31] = Animator.StringToHash("BS103@AKUMA_SKILL4_CASTLOOP2");
		_animationHash[32] = Animator.StringToHash("BS103@AKUMA_SKILL4_CASTOUT1");
		_animationHash[33] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTING1");
		_animationHash[34] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTLOOP1");
		_animationHash[35] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTING2");
		_animationHash[36] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTLOOP2");
		_animationHash[37] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTING3");
		_animationHash[38] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTLOOP3");
		_animationHash[39] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTING4");
		_animationHash[40] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTLOOP4");
		_animationHash[41] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTING5");
		_animationHash[42] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTING6");
		_animationHash[43] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTING7");
		_animationHash[44] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTLOOP5");
		_animationHash[45] = Animator.StringToHash("BS103@AKUMA_SKILL5_CASTOUT1");
		_animationHash[46] = Animator.StringToHash("BS103@AKUMA_SKILL6_CASTING1");
		_animationHash[47] = Animator.StringToHash("BS103@AKUMA_SKILL6_CASTLOOP1");
		_animationHash[48] = Animator.StringToHash("BS103@AKUMA_SKILL6_CASTOUT1");
		_animationHash[49] = Animator.StringToHash("BS103@AKUMA_SKILL7_CASTING1");
		_animationHash[50] = Animator.StringToHash("BS103@AKUMA_SKILL7_CASTLOOP1");
		_animationHash[51] = Animator.StringToHash("BS103@AKUMA_SKILL7_CASTLOOP2");
		_animationHash[52] = Animator.StringToHash("BS103@AKUMA_SKILL7_CASTOUT1");
		_animationHash[53] = Animator.StringToHash("BS103@VTRIGGER");
		_animationHash[54] = Animator.StringToHash("BS103@MOVE_LOOP");
		_animationHash[55] = Animator.StringToHash("BS103@HURT_LOOP");
		_animationHash[56] = Animator.StringToHash("BS103@DEAD");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		BuffCollide = OrangeBattleUtility.FindChildRecursive(ref childs, BuffCollideName, true).gameObject.AddOrGetComponent<CollideBulletHitSelf>();
		CatchTool = _transform.GetComponent<CatchPlayerTool>();
		StageMainCamera = OrangeSceneManager.FindObjectOfTypeCustom<CameraControl>();
		UpperPunchCollider = OrangeBattleUtility.FindChildRecursive(ref childs, UpperPunch, true).gameObject.AddOrGetComponent<CollideBullet>();
		CrouchPunchCollider = OrangeBattleUtility.FindChildRecursive(ref childs, CrouchPunch, true).gameObject.AddOrGetComponent<CollideBullet>();
		DragonPunchCollider = OrangeBattleUtility.FindChildRecursive(ref childs, DragonPunch, true).gameObject.AddOrGetComponent<CollideBullet>();
		GroundPunchCollider = OrangeBattleUtility.FindChildRecursive(ref childs, GroundPunch, true).gameObject.AddOrGetComponent<CollideBullet>();
		KneeKickCollider = OrangeBattleUtility.FindChildRecursive(ref childs, KneeKick, true).gameObject.AddOrGetComponent<CollideBullet>();
		LowKickCollider = OrangeBattleUtility.FindChildRecursive(ref childs, LowKick, true).gameObject.AddOrGetComponent<CollideBullet>();
		DownKickCollider = OrangeBattleUtility.FindChildRecursive(ref childs, DownKick, true).gameObject.AddOrGetComponent<CollideBullet>();
		CycleKickCollider = OrangeBattleUtility.FindChildRecursive(ref childs, CycleKick, true).gameObject.AddOrGetComponent<CollideBullet>();
		FrontKickCollider = OrangeBattleUtility.FindChildRecursive(ref childs, FrontKick, true).gameObject.AddOrGetComponent<CollideBullet>();
		if (Skill0Fx1 == null)
		{
			Skill0Fx1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0Fx1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill0Fx2 == null)
		{
			Skill0Fx2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0Fx2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1Fx1L == null)
		{
			Skill1Fx1L = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1Fx1L", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1Fx1R == null)
		{
			Skill1Fx1R = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1Fx1R", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1Fx2L == null)
		{
			Skill1Fx2L = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1Fx2L", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1Fx2R == null)
		{
			Skill1Fx2R = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1Fx2R", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1Fx3 == null)
		{
			Skill1Fx3 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1Fx3", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (CycleKickFx == null)
		{
			CycleKickFx = OrangeBattleUtility.FindChildRecursive(ref childs, "CycleKickFx", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ExDragonFx == null)
		{
			ExDragonFx = OrangeBattleUtility.FindChildRecursive(ref childs, "ExDragonFx", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ExGroundFx1 == null)
		{
			ExGroundFx1 = OrangeBattleUtility.FindChildRecursive(ref childs, "ExGroundFx1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ExGroundFx2 == null)
		{
			ExGroundFx2 = OrangeBattleUtility.FindChildRecursive(ref childs, "ExGroundFx2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill5Fx1 == null)
		{
			Skill5Fx1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5Fx1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill5Fx1_1 == null)
		{
			Skill5Fx1_1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5Fx1_1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill5Fx2 == null)
		{
			Skill5Fx2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5Fx2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill5Fx2_1 == null)
		{
			Skill5Fx2_1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5Fx2_1", true).gameObject.AddOrGetComponent<TrailRenderer>();
		}
		if (Skill5Fx3 == null)
		{
			Skill5Fx3 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5Fx3", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill5Fx4 == null)
		{
			Skill5Fx4 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill5Fx4", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill6Fx1 == null)
		{
			Skill6Fx1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill6Fx1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill6Fx2 == null)
		{
			Skill6Fx2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill6Fx2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill6Fx3 == null)
		{
			Skill6Fx3 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill6Fx3", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill6Fx5 == null)
		{
			Skill6Fx5 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill6Fx5", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ExModeFx1 == null)
		{
			ExModeFx1 = OrangeBattleUtility.FindChildRecursive(ref childs, "ExModeFx1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ExModeFx1_1 == null)
		{
			ExModeFx1_1 = OrangeBattleUtility.FindChildRecursive(ref childs, "ExModeFx1_1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ExModeFx2L == null)
		{
			ExModeFx2L = OrangeBattleUtility.FindChildRecursive(ref childs, "ExModeFx2L", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (ExModeFx2R == null)
		{
			ExModeFx2R = OrangeBattleUtility.FindChildRecursive(ref childs, "ExModeFx2R", true).gameObject.AddOrGetComponent<ParticleSystem>();
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
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Skill5Fx5, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Skill6Fx4, 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Skill6UIFx1);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Skill6UIFx2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Skill6UIFx3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Skill6UIFx4);
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
				PlayBossSE("bs042_gouki15");
				_velocity.x = 0;
				ActionTimes = Skill0ShootTimes;
				break;
			case SubStatus.Phase1:
				SwitchFx(Skill0Fx1);
				break;
			case SubStatus.Phase2:
				HasActed = false;
				ActionAnimatorFrame = Skill0ShootFrame;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("bs042_gouki17");
				_velocity.x = 0;
				ActionTimes = Skill1ShootTimes;
				SwitchFx(Skill1Fx1L);
				SwitchFx(Skill1Fx1R);
				break;
			case SubStatus.Phase1:
				SwitchFx(Skill1Fx2L);
				SwitchFx(Skill1Fx2R);
				break;
			case SubStatus.Phase2:
				HasActed = false;
				ActionAnimatorFrame = Skill1ShootFrame;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				EndPos = GetTargetPos();
				UpdateDirection();
				CloseAllCollider();
				break;
			case SubStatus.Phase1:
				OpenCollider(UpperPunchCollider, AttackPattern.UpperPunch);
				break;
			case SubStatus.Phase2:
				CloseAllCollider();
				break;
			case SubStatus.Phase3:
				PlayBossSE("bs042_gouki25");
				OpenCollider(KneeKickCollider, AttackPattern.KneeKick);
				break;
			case SubStatus.Phase4:
				PlayBossSE("bs042_gouki01");
				CloseAllCollider();
				_velocity.x = Skill2MoveSpeed1 * base.direction;
				_velocity.y = Skill2JumpSpeed1;
				break;
			case SubStatus.Phase5:
				PlayBossSE("bs042_gouki24");
				SwitchFx(CycleKickFx);
				OpenCollider(CycleKickCollider, AttackPattern.CycleKick);
				_velocity.x = 0;
				break;
			case SubStatus.Phase7:
				PlayBossSE("bs042_gouki01");
				if (EXMode)
				{
					PlayBossSE("bs042_gouki23");
				}
				else
				{
					PlayBossSE("bs042_gouki19");
				}
				OpenCollider(DragonPunchCollider, AttackPattern.DragonPunch);
				_velocity.x = Skill2MoveSpeed2 * base.direction;
				_velocity.y = Skill2JumpSpeed2;
				break;
			case SubStatus.Phase8:
				if (EXMode)
				{
					SwitchFx(ExDragonFx);
				}
				break;
			case SubStatus.Phase9:
				if (EXMode)
				{
					SwitchFx(ExDragonFx, false);
				}
				CloseAllCollider();
				OpenCollider(_collideBullet, AttackPattern.Body);
				_velocity.x = 0;
				IgnoreGravity = false;
				break;
			case SubStatus.Phase11:
				OpenCollider(_collideBullet, AttackPattern.Body);
				break;
			case SubStatus.Phase12:
				PlayBossSE("bs042_gouki20");
				SwitchFx(ExDragonFx, false);
				_velocity.x = 0;
				IgnoreGravity = true;
				break;
			case SubStatus.Phase13:
				CloseAllCollider();
				OpenCollider(GroundPunchCollider, AttackPattern.GroundPunch);
				SwitchFx(ExGroundFx1);
				break;
			case SubStatus.Phase14:
				PlayBossSE("bs042_gouki22");
				CloseAllCollider();
				OpenCollider(_collideBullet, AttackPattern.Body);
				IgnoreGravity = false;
				_velocity.y = -2000;
				SwitchFx(ExGroundFx2);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				CloseAllCollider();
				EndPos = GetTargetPos();
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				PlayBossSE("bs042_gouki25");
				IgnoreGravity = false;
				OpenCollider(LowKickCollider, AttackPattern.LowKick);
				break;
			case SubStatus.Phase2:
				PlayBossSE("bs042_gouki01");
				if (UseSkill10)
				{
					ActionTimes = Skill10UseTimes;
				}
				else
				{
					_velocity.y = Skill3JumpSpeed1;
				}
				_velocity.x = Skill3MoveSpeed1 * base.direction;
				CloseAllCollider();
				break;
			case SubStatus.Phase3:
				if (UseSkill10)
				{
					_velocity.x = Skill10MoveSpeed * base.direction;
				}
				SwitchFx(CycleKickFx);
				OpenCollider(CycleKickCollider, AttackPattern.CycleKick);
				break;
			case SubStatus.Phase4:
				PlayBossSE("bs042_gouki02");
				_velocity = VInt3.zero;
				IgnoreGravity = false;
				CloseAllCollider();
				OpenCollider(_collideBullet, AttackPattern.Body);
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("bs042_gouki01");
				CloseAllCollider();
				EndPos = GetTargetPos();
				UpdateDirection();
				_velocity.x = Skill4MoveSpeed1 * base.direction;
				_velocity.y = Skill4JumpSpeed1;
				break;
			case SubStatus.Phase2:
				OpenCollider(FrontKickCollider, AttackPattern.FrontKick);
				_velocity.y = 0;
				_velocity.x = _velocity.x * 3 / 2;
				break;
			case SubStatus.Phase3:
				IgnoreGravity = true;
				_velocity.x = 0;
				break;
			case SubStatus.Phase4:
				CloseAllCollider();
				IgnoreGravity = false;
				break;
			case SubStatus.Phase5:
				OpenCollider(CrouchPunchCollider, AttackPattern.CrouchPunch);
				break;
			case SubStatus.Phase6:
				CloseAllCollider();
				break;
			case SubStatus.Phase7:
				OpenCollider(KneeKickCollider, AttackPattern.KneeKick);
				break;
			case SubStatus.Phase8:
				PlayBossSE("bs042_gouki01");
				CloseAllCollider();
				_velocity.x = Skill4MoveSpeed2 * base.direction;
				_velocity.y = Skill4JumpSpeed2;
				break;
			case SubStatus.Phase9:
				SwitchFx(CycleKickFx);
				OpenCollider(CycleKickCollider, AttackPattern.CycleKick);
				_velocity.x = 0;
				HasActed = false;
				break;
			case SubStatus.Phase10:
				CloseAllCollider();
				OpenCollider(DownKickCollider, AttackPattern.DownKick);
				break;
			case SubStatus.Phase11:
				IgnoreGravity = false;
				break;
			case SubStatus.Phase12:
				CloseAllCollider();
				OpenCollider(_collideBullet, AttackPattern.Body);
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				State1Count = 0;
				PlayBossSE("bs042_gouki13");
				SwitchFx(Skill5Fx1_1);
				EndPos = GetTargetPos();
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill5Fx5, ModelTransform.position, Quaternion.Euler(0f, 90 * base.direction, 0f), Array.Empty<object>());
				break;
			case SubStatus.Phase2:
				PlayBossSE("bs042_gouki14");
				break;
			case SubStatus.Phase3:
				EndPos = GetTargetPos();
				UpdateDirection();
				switch (skillpattern)
				{
				case SkillPattern.State1:
					if (CheckBlock(EndPos, base.direction))
					{
						EndPos += Vector3.left * 1.2f * base.direction;
					}
					else
					{
						EndPos += Vector3.right * 1.2f * base.direction;
					}
					break;
				case SkillPattern.State2:
				case SkillPattern.State3:
				case SkillPattern.State4:
					if (UseSkill10)
					{
						EndPos += Vector3.right * 1.2f * base.direction;
					}
					else
					{
						UpdateDirection(-base.direction);
					}
					break;
				case SkillPattern.State5:
					UpdateDirection(-base.direction);
					break;
				}
				_velocity.x = Skill5MoveSpeed1 * base.direction;
				SwitchFx(Skill5Fx2);
				Skill5Fx2_1.enabled = true;
				SwitchFx(Skill5Fx3);
				SwitchFx(Skill5Fx4);
				break;
			case SubStatus.Phase4:
				_velocity.x = 0;
				SwitchFx(Skill5Fx2, false);
				Skill5Fx2_1.enabled = false;
				SwitchFx(Skill5Fx3, false);
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				CloseAllCollider();
				IsInvincible = true;
				SwitchFx(Skill6Fx1);
				EndPos = GetTargetPos();
				UpdateDirection();
				CameraZoom(true);
				PlayBossSE("bs042_gouki10");
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill6UIFx1, Vector3.zero, Quaternion.identity, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill6UIFx2, Vector3.zero, Quaternion.identity, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill6UIFx3, Vector3.zero, Quaternion.identity, Array.Empty<object>());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill6UIFx4, Vector3.zero, Quaternion.identity, Array.Empty<object>());
				break;
			case SubStatus.Phase1:
				CameraZoom(false);
				SwitchFx(Skill5Fx2);
				Skill5Fx2_1.enabled = true;
				SwitchFx(Skill5Fx3);
				SwitchFx(Skill5Fx4);
				_velocity.x = Skill6MoveSpeed * base.direction;
				break;
			case SubStatus.Phase2:
				PlayBossSE("bs042_gouki11");
				EndPos = GetTargetPos();
				SwitchFx(Skill6Fx2);
				SwitchFx(Skill5Fx2, false);
				Skill5Fx2_1.enabled = false;
				SwitchFx(Skill5Fx3, false);
				SwitchFx(Skill6Fx5);
				SwitchFx(ExModeFx2L, false);
				SwitchFx(ExModeFx2R, false);
				HasActed = false;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill6Fx4, EndPos, Quaternion.identity, Array.Empty<object>());
				(BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, EndPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill6Time1 * 20f);
				break;
			case SubStatus.Phase3:
				PlayBossSE("bs042_gouki12");
				CloseAllCollider();
				SwitchFx(Skill6Fx3);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill6Time2 * 20f);
				SwitchFx(ExModeFx2L);
				SwitchFx(ExModeFx2R);
				break;
			case SubStatus.Phase4:
				_velocity.x = 0;
				OpenCollider(_collideBullet, AttackPattern.Body);
				break;
			case SubStatus.Phase5:
				_velocity.x = 0;
				SwitchFx(Skill5Fx2, false);
				Skill5Fx2_1.enabled = false;
				SwitchFx(Skill5Fx3, false);
				OpenCollider(_collideBullet, AttackPattern.Body);
				break;
			}
			break;
		case MainStatus.Skill7:
			if (_subStatus == SubStatus.Phase0)
			{
				EXMode = true;
				PlayBossSE("bs042_gouki21");
				SwitchFx(ExModeFx1_1);
				EndPos = GetTargetPos();
				UpdateDirection();
				BuffCollide.Active(friendMask);
			}
			break;
		case MainStatus.Skill8:
			if (_subStatus == SubStatus.Phase0)
			{
				EndPos = GetTargetPos();
				UpdateDirection();
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill8UseTime * 20f);
				_velocity.x = Skill8MoveSpeed * base.direction;
			}
			break;
		case MainStatus.Skill9:
			if (_subStatus == SubStatus.Phase0)
			{
				EndPos = GetTargetPos();
				UpdateDirection();
				UseSkill9 = true;
				SetStatus(MainStatus.Skill2, SubStatus.Phase7);
				return;
			}
			break;
		case MainStatus.Skill10:
			if (_subStatus == SubStatus.Phase0)
			{
				EndPos = GetTargetPos();
				UpdateDirection();
				UseSkill10 = true;
				SetStatus(MainStatus.Skill5);
				return;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (bPlay09)
				{
					Play09Loop(false);
				}
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				EndPos = GetTargetPos();
				UpdateDirection();
				IgnoreGravity = false;
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase2:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(TeleportOutFxName, Controller.GetRealCenterPos(), Quaternion.identity, new object[1] { Vector3.one });
				BackToPool();
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
				_currentAnimationId = AnimationID.ANI_SKILL2_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL2_START4;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL2_START5;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP5;
				break;
			case SubStatus.Phase9:
				_currentAnimationId = AnimationID.ANI_SKILL2_START6;
				break;
			case SubStatus.Phase10:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP6;
				break;
			case SubStatus.Phase11:
				_currentAnimationId = AnimationID.ANI_SKILL2_END6;
				break;
			case SubStatus.Phase12:
				_currentAnimationId = AnimationID.ANI_SKILL2_START7;
				break;
			case SubStatus.Phase13:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP7;
				break;
			case SubStatus.Phase14:
				_currentAnimationId = AnimationID.ANI_SKILL2_END7;
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
				_currentAnimationId = AnimationID.ANI_SKILL3_END2;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
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
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL4_START3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP3;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL4_START4;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP4;
				break;
			case SubStatus.Phase8:
				_currentAnimationId = AnimationID.ANI_SKILL4_START5;
				break;
			case SubStatus.Phase9:
				_currentAnimationId = AnimationID.ANI_SKILL4_START6;
				break;
			case SubStatus.Phase10:
				_currentAnimationId = AnimationID.ANI_SKILL4_START7;
				break;
			case SubStatus.Phase11:
				_currentAnimationId = AnimationID.ANI_SKILL4_LOOP7;
				break;
			case SubStatus.Phase12:
				_currentAnimationId = AnimationID.ANI_SKILL4_END7;
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL5_START;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL5_END;
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL6_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL6_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL6_LOOP2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL6_END2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL5_END;
				break;
			case SubStatus.Phase3:
				return;
			}
			break;
		case MainStatus.Skill7:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_SKILL7_START;
			}
			break;
		case MainStatus.Skill8:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_SKILL8_LOOP;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase3:
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
				if (UseSkill7 && !EXMode)
				{
					UseSkill7 = false;
					mainStatus = MainStatus.Skill7;
					break;
				}
				if (FirstUseSkill6 && UseSkill6Times < 1)
				{
					UseSkill6Times++;
					FirstUseSkill6 = false;
					mainStatus = MainStatus.Skill6;
					break;
				}
				if (SecondUseSkill6 && UseSkill6Times < 2)
				{
					UseSkill6Times++;
					SecondUseSkill6 = false;
					mainStatus = MainStatus.Skill6;
					break;
				}
				switch (skillpattern)
				{
				case SkillPattern.State1:
					if (State1Count >= 3)
					{
						State1Count = 0;
						mainStatus = MainStatus.Skill5;
						break;
					}
					EndPos = GetTargetPos();
					if (Math.Abs(EndPos.x - NowPos.x) > XDisJudge1)
					{
						if (OrangeBattleUtility.Random(0, 100) > 30)
						{
							mainStatus = ((!EXMode) ? MainStatus.Skill0 : MainStatus.Skill1);
						}
						else
						{
							State1Count = 0;
							mainStatus = MainStatus.Skill5;
						}
					}
					else
					{
						mainStatus = MainStatus.Skill8;
					}
					State1Count++;
					break;
				case SkillPattern.State2:
					EndPos = GetTargetPos();
					mainStatus = ((!(EndPos.y - NowPos.y > YDisJudge1)) ? ((!(Math.Abs(EndPos.x - NowPos.x) > Skill3JudgeDis)) ? MainStatus.Skill3 : MainStatus.Skill10) : MainStatus.Skill9);
					break;
				case SkillPattern.State3:
					mainStatus = ((!EXMode) ? MainStatus.Skill0 : MainStatus.Skill1);
					break;
				case SkillPattern.State4:
					EndPos = GetTargetPos();
					mainStatus = ((!(Math.Abs(EndPos.x - NowPos.x) > XDisJudge1)) ? MainStatus.Skill8 : MainStatus.Skill10);
					break;
				case SkillPattern.State5:
					EndPos = GetTargetPos();
					if (Math.Abs(EndPos.x - NowPos.x) > XDisJudge1)
					{
						mainStatus = MainStatus.Idle;
						skillpattern = SkillPattern.State1;
					}
					else
					{
						mainStatus = MainStatus.Skill8;
					}
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
					UpdateDirection(-base.direction);
					UpdateNextState(MainStatus.Skill5);
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
					Vector3 pDirection = EndPos - ShootPos.position;
					BulletBase.TryShotBullet(EnemyWeapons[7].BulletData, ShootPos.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					SwitchFx(Skill0Fx2);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase2);
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
					SkillPattern skillPattern = skillpattern;
					if (skillPattern == SkillPattern.State3)
					{
						skillpattern = SkillPattern.State5;
					}
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
					EndPos = GetTargetPos();
					UpdateDirection();
					ShotAngle = Vector2.Angle(Vector2.up, EndPos - ShootPos.position);
					_animator.SetFloat(_HashAngle, ShotAngle);
					Vector3 pDirection2 = EndPos - ShootPos.position;
					BulletBase.TryShotBullet(EnemyWeapons[8].BulletData, ShootPos.position, pDirection2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					SwitchFx(Skill1Fx3);
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill1, SubStatus.Phase2);
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
					SkillPattern skillPattern = skillpattern;
					if (skillPattern == SkillPattern.State3)
					{
						skillpattern = SkillPattern.State5;
					}
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
				if (_currentFrame > 1f)
				{
					Play09Loop(true);
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					Play09Loop(false);
					SetStatus(MainStatus.Skill2, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (_velocity.y < 0)
				{
					_velocity.x = 0;
					IgnoreGravity = true;
				}
				if (_currentFrame > 1f)
				{
					if (EXMode && !UseSkill9)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase12);
					}
					else
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase9);
					}
				}
				break;
			case SubStatus.Phase9:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase10);
				}
				break;
			case SubStatus.Phase10:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase11);
				}
				break;
			case SubStatus.Phase11:
				if (!(_currentFrame > 1f))
				{
					break;
				}
				SetStatus(MainStatus.Idle);
				if (!UseSkill9)
				{
					skillpattern = SkillPattern.State1;
					break;
				}
				UseSkill9 = false;
				switch (skillpattern)
				{
				case SkillPattern.State1:
					skillpattern = SkillPattern.State1;
					break;
				case SkillPattern.State2:
				case SkillPattern.State4:
					skillpattern = SkillPattern.State4;
					break;
				case SkillPattern.State5:
					skillpattern = SkillPattern.State5;
					break;
				case SkillPattern.State3:
					break;
				}
				break;
			case SubStatus.Phase12:
				if (_currentFrame > 0.7f)
				{
					IgnoreGravity = false;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase13);
				}
				break;
			case SubStatus.Phase13:
				if (Controller.Collisions.below)
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1.5f, false);
					SetStatus(MainStatus.Skill2, SubStatus.Phase14);
				}
				break;
			case SubStatus.Phase14:
				if (_currentFrame > 1f)
				{
					SkillPattern skillPattern = skillpattern;
					if (skillPattern == SkillPattern.State5)
					{
						skillpattern = SkillPattern.State1;
					}
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
					PlayBossSE("bs042_gouki24");
					Play09Loop(true);
				}
				break;
			case SubStatus.Phase3:
				if (!(_currentFrame > 1f))
				{
					break;
				}
				if (UseSkill10)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase3);
						break;
					}
					Play09Loop(false);
					_velocity.x = 0;
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				else
				{
					Play09Loop(false);
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (!(_currentFrame > 1f))
				{
					break;
				}
				if (UseSkill10)
				{
					UseSkill10 = false;
					if (skillpattern == SkillPattern.State2)
					{
						skillpattern = SkillPattern.State3;
					}
					else
					{
						skillpattern = SkillPattern.State1;
					}
				}
				else
				{
					skillpattern = SkillPattern.State3;
				}
				SetStatus(MainStatus.Idle);
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
				if (_velocity.y < 9000)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_velocity.y < 0)
				{
					_velocity.y = -600;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_velocity.y < 0)
				{
					_velocity.x = 0;
					IgnoreGravity = true;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase5);
				}
				else if (!Controller.Collisions.below)
				{
					_velocity.y += -1000;
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase8);
				}
				break;
			case SubStatus.Phase8:
				if (_currentFrame > 1f)
				{
					PlayBossSE("bs042_gouki24");
					Play09Loop(true);
					SetStatus(MainStatus.Skill4, SubStatus.Phase9);
				}
				break;
			case SubStatus.Phase9:
				if (_currentFrame > Skill4UseFrame)
				{
					_velocity.x = Skill4MoveSpeed3 * base.direction;
					_velocity.y = Skill4JumpSpeed3;
				}
				if (_currentFrame > 1f)
				{
					Play09Loop(false);
					SetStatus(MainStatus.Skill4, SubStatus.Phase10);
				}
				break;
			case SubStatus.Phase10:
				if (_velocity.y < 0 && _currentFrame < 0.75f)
				{
					_velocity.x = 0;
					IgnoreGravity = true;
				}
				if (IgnoreGravity && _currentFrame > 0.75f)
				{
					IgnoreGravity = false;
					_velocity.y = -1000;
				}
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase11);
				}
				break;
			case SubStatus.Phase11:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill4, SubStatus.Phase12);
				}
				break;
			case SubStatus.Phase12:
				if (_currentFrame > 1f)
				{
					skillpattern = SkillPattern.State1;
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
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				switch (skillpattern)
				{
				case SkillPattern.State1:
					if ((NowPos.x - EndPos.x) * (float)base.direction > 0f || (base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
					{
						_velocity.x = 0;
						SetStatus(MainStatus.Skill5, SubStatus.Phase4);
					}
					break;
				case SkillPattern.State2:
				case SkillPattern.State3:
				case SkillPattern.State4:
					if (UseSkill10)
					{
						if ((NowPos.x - EndPos.x) * (float)base.direction > 0f || (base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
						{
							_velocity.x = 0;
							SetStatus(MainStatus.Skill5, SubStatus.Phase4);
						}
					}
					else if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
					{
						_velocity.x = 0;
						SetStatus(MainStatus.Skill5, SubStatus.Phase4);
					}
					break;
				case SkillPattern.State5:
					if ((base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
					{
						_velocity.x = 0;
						SetStatus(MainStatus.Skill5, SubStatus.Phase4);
					}
					break;
				}
				break;
			case SubStatus.Phase4:
				if (!(_currentFrame > 1f))
				{
					break;
				}
				switch (skillpattern)
				{
				case SkillPattern.State1:
					skillpattern = SkillPattern.State2;
					SetStatus(MainStatus.Idle);
					break;
				case SkillPattern.State2:
				case SkillPattern.State4:
					if (UseSkill10)
					{
						SetStatus(MainStatus.Skill3);
						break;
					}
					skillpattern = SkillPattern.State1;
					SetStatus(MainStatus.Idle);
					break;
				case SkillPattern.State5:
					skillpattern = SkillPattern.State1;
					SetStatus(MainStatus.Idle);
					break;
				case SkillPattern.State3:
					SetStatus(MainStatus.Idle);
					break;
				}
				break;
			}
			break;
		case MainStatus.Skill6:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase2:
				if (!HasActed && GameLogicUpdateManager.GameFrame > ActionFrame - 40)
				{
					(BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, EndPos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill6Fx4, EndPos, Quaternion.identity, Array.Empty<object>());
					SwitchFx(Skill6Fx5, false);
					HasActed = true;
				}
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Skill6, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					if (CatchTool.IsCatching)
					{
						CatchTool.ReleaseTarget();
					}
					SetStatus(MainStatus.Skill6, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					State1Count = 0;
					skillpattern = SkillPattern.State1;
					IsInvincible = false;
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					State1Count = 0;
					skillpattern = SkillPattern.State1;
					IsInvincible = false;
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase1:
				break;
			}
			break;
		case MainStatus.Skill7:
			if (_subStatus == SubStatus.Phase0)
			{
				IsInvincible = true;
				if (_currentFrame > 1f)
				{
					IsInvincible = false;
					SwitchFx(ExModeFx2L);
					SwitchFx(ExModeFx2R);
					_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
					_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
					_collideBullet.Active(targetMask);
					SetStatus(MainStatus.Idle);
				}
			}
			break;
		case MainStatus.Skill8:
			if (_subStatus != 0)
			{
				break;
			}
			if (GameLogicUpdateManager.GameFrame > ActionFrame)
			{
				_velocity = VInt3.zero;
				switch (skillpattern)
				{
				case SkillPattern.State1:
					if (EXMode)
					{
						SetStatus(MainStatus.Skill1);
					}
					else
					{
						SetStatus(MainStatus.Skill0);
					}
					break;
				case SkillPattern.State4:
				case SkillPattern.State5:
					SetStatus(MainStatus.Skill5);
					break;
				}
				break;
			}
			EndPos = GetTargetPos();
			if ((!Controller.Collisions.right || base.direction != 1) && (!Controller.Collisions.left || base.direction != -1) && !(Math.Abs(EndPos.x - NowPos.x) < Skill8UseDis))
			{
				break;
			}
			_velocity = VInt3.zero;
			switch (skillpattern)
			{
			case SkillPattern.State1:
				SetStatus(MainStatus.Skill9);
				break;
			case SkillPattern.State4:
				EndPos = GetTargetPos();
				if (EndPos.y - NowPos.y > YDisJudge1)
				{
					SetStatus(MainStatus.Skill9);
				}
				else
				{
					SetStatus(MainStatus.Skill4);
				}
				break;
			case SkillPattern.State5:
				EndPos = GetTargetPos();
				if (EndPos.y - NowPos.y > YDisJudge1)
				{
					SetStatus(MainStatus.Skill9);
				}
				else
				{
					SetStatus(MainStatus.Skill2);
				}
				break;
			case SkillPattern.State2:
			case SkillPattern.State3:
				break;
			}
			break;
		case MainStatus.Skill9:
			if (_subStatus == SubStatus.Phase0)
			{
				SetStatus(MainStatus.Skill2, SubStatus.Phase7);
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
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
				if (_currentFrame > 0.4f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die);
				}
				break;
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Skill10:
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
		if (_mainStatus == MainStatus.Skill6 && _subStatus == SubStatus.Phase1)
		{
			Collider2D collider2D = Physics2D.OverlapBox(ShootPos.position, Vector2.one * 2f, 0f, LayerMask.GetMask("Player"));
			if ((bool)collider2D && (int)Hp > 0)
			{
				OrangeCharacter component = collider2D.GetComponent<OrangeCharacter>();
				CatchTool.CatchTarget(component);
			}
			if (CatchTool.IsCatching)
			{
				_transform.position = CatchTool.TargetOC._transform.position - Vector3.right * base.direction;
				Controller.LogicPosition = new VInt3(_transform.position);
				_velocity.x = 0;
				SetStatus(MainStatus.Skill6, SubStatus.Phase2);
			}
			else if ((NowPos.x - EndPos.x) * (float)base.direction > 0.5f || (base.direction == 1 && Controller.Collisions.right) || (base.direction == -1 && Controller.Collisions.left))
			{
				SetStatus(MainStatus.Skill6, SubStatus.Phase5);
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
			ModelTransform.localScale = new Vector3(1.2f, 1.2f, 1.2f * (float)base.direction);
			UseSkill6Times = 0;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			BuffCollide.UpdateBulletData(EnemyWeapons[9].BulletData, "", base.gameObject.GetInstanceID());
			BuffCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			SetStatus(MainStatus.Debut);
		}
		else
		{
			if (bPlay09)
			{
				Play09Loop(false);
			}
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
		IgnoreGravity = false;
		if (_mainStatus != MainStatus.Die)
		{
			CloseAllCollider();
			StopAllFX();
			if (CatchTool.IsCatching)
			{
				CatchTool.ReleaseTarget();
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

	private void CloseAllCollider()
	{
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		if ((bool)UpperPunchCollider)
		{
			UpperPunchCollider.BackToPool();
		}
		if ((bool)CrouchPunchCollider)
		{
			CrouchPunchCollider.BackToPool();
		}
		if ((bool)DragonPunchCollider)
		{
			DragonPunchCollider.BackToPool();
		}
		if ((bool)GroundPunchCollider)
		{
			GroundPunchCollider.BackToPool();
		}
		if ((bool)KneeKickCollider)
		{
			KneeKickCollider.BackToPool();
		}
		if ((bool)LowKickCollider)
		{
			LowKickCollider.BackToPool();
		}
		if ((bool)DownKickCollider)
		{
			DownKickCollider.BackToPool();
		}
		if ((bool)CycleKickCollider)
		{
			CycleKickCollider.BackToPool();
		}
		if ((bool)FrontKickCollider)
		{
			FrontKickCollider.BackToPool();
		}
		if ((bool)BuffCollide)
		{
			BuffCollide.BackToPool();
		}
	}

	private void OpenCollider(CollideBullet bullet, AttackPattern skill)
	{
		if ((bool)bullet)
		{
			bullet.UpdateBulletData(EnemyWeapons[GetSkillID(skill)].BulletData);
			bullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			bullet.Active(targetMask);
		}
	}

	private int GetSkillID(AttackPattern skill)
	{
		switch (skill)
		{
		case AttackPattern.UpperPunch:
		case AttackPattern.CrouchPunch:
			return 1;
		case AttackPattern.DragonPunch:
			return 4;
		case AttackPattern.GroundPunch:
			return 5;
		case AttackPattern.KneeKick:
		case AttackPattern.LowKick:
		case AttackPattern.DownKick:
		case AttackPattern.FrontKick:
			return 2;
		case AttackPattern.CycleKick:
			return 3;
		case AttackPattern.Body:
			return 0;
		default:
			return 0;
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		int num = (int)Hp - (int)tHurtPassParam.dmg;
		if (!EXMode && num <= (int)MaxHp * ExModeHp / 100)
		{
			tHurtPassParam.dmg = (int)Hp - (int)MaxHp * ExModeHp / 100;
			UseSkill7 = true;
		}
		if (UseSkill6Times < 1 && num <= (int)MaxHp * FirstUseHp / 100)
		{
			tHurtPassParam.dmg = (int)Hp - (int)MaxHp * FirstUseHp / 100;
			FirstUseSkill6 = true;
		}
		if (UseSkill6Times < 2 && num <= (int)MaxHp * SecondUseHp / 100)
		{
			tHurtPassParam.dmg = (int)Hp - (int)MaxHp * SecondUseHp / 100;
			SecondUseSkill6 = true;
		}
		return base.Hurt(tHurtPassParam);
	}

	private bool CheckBlock(Vector3 RayOrigin, int direct = 1)
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(RayOrigin, Vector3.right * direct, 2f, layerMask, _transform))
		{
			return true;
		}
		return false;
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
		SwitchFx(Skill0Fx1, false);
		SwitchFx(Skill0Fx2, false);
		SwitchFx(Skill1Fx1L, false);
		SwitchFx(Skill1Fx1R, false);
		SwitchFx(Skill1Fx2L, false);
		SwitchFx(Skill1Fx2R, false);
		SwitchFx(Skill1Fx3, false);
		SwitchFx(CycleKickFx, false);
		SwitchFx(ExDragonFx, false);
		SwitchFx(ExGroundFx1, false);
		SwitchFx(ExGroundFx2, false);
		SwitchFx(Skill5Fx1, false);
		SwitchFx(Skill5Fx1_1, false);
		SwitchFx(Skill5Fx2, false);
		Skill5Fx2_1.enabled = false;
		SwitchFx(Skill5Fx3, false);
		SwitchFx(Skill5Fx4, false);
		SwitchFx(Skill6Fx1, false);
		SwitchFx(Skill6Fx2, false);
		SwitchFx(Skill6Fx3, false);
		SwitchFx(Skill6Fx5, false);
		SwitchFx(ExModeFx1, false);
		SwitchFx(ExModeFx1_1, false);
		SwitchFx(ExModeFx2L, false);
		SwitchFx(ExModeFx2R, false);
	}

	private void CallEvent(int EventID)
	{
		EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
		stageEventCall.nID = EventID;
		stageEventCall.tTransform = _transform;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
	}

	private void CameraZoom(bool zoomin)
	{
		if (StageMainCamera == null)
		{
			StageMainCamera = OrangeSceneManager.FindObjectOfTypeCustom<CameraControl>();
		}
		if (StageMainCamera == null)
		{
			Debug.LogError("BS103 找不到 Main Camera");
			return;
		}
		if (tZoomInOutCoroutine != null)
		{
			StopCoroutine(tZoomInOutCoroutine);
			tZoomInOutCoroutine = null;
		}
		if (zoomin)
		{
			OriginFOV = 22f;
			StageMainCamera.Target = Controller;
			StageMainCamera.transform.position = Controller.LogicPosition.vec3;
			tZoomInOutCoroutine = StartCoroutine(StageResManager.TweenFloatCoroutine(22f, Skill6ZoomInFOV, 0.2f, delegate(float f)
			{
				StageMainCamera.UpdateCameraFov(f);
			}, delegate
			{
				StageMainCamera.UpdateCameraFov(Skill6ZoomInFOV);
				tZoomInOutCoroutine = null;
			}));
		}
		else
		{
			StageMainCamera.Target = StageUpdate.runPlayers[0].Controller;
			StageMainCamera.transform.position = StageUpdate.runPlayers[0].Controller.LogicPosition.vec3;
			tZoomInOutCoroutine = StartCoroutine(StageResManager.TweenFloatCoroutine(StageMainCamera.DesignFov, OriginFOV, 0.2f, delegate(float f)
			{
				StageMainCamera.UpdateCameraFov(f);
			}, delegate
			{
				StageMainCamera.UpdateCameraFov(OriginFOV);
				tZoomInOutCoroutine = null;
			}));
		}
	}
}
