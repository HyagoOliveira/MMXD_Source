#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS114_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum SkillPattern
	{
		State1 = 1,
		State2 = 2,
		State3 = 3
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
		Phase6 = 6,
		Phase7 = 7,
		MAX_SUBSTATUS = 8
	}

	protected enum ScepterStatus
	{
		Move = 0,
		Circle = 1,
		Out = 2,
		Down = 3,
		Ground = 4,
		Disappear = 5
	}

	protected class Scepter
	{
		public int ActionFrame;

		public int Rotate;

		public ScepterStatus Status;

		public Transform Obj;

		public Vector3 Velocity;
	}

	protected enum HearbandStatus
	{
		Idle = 0,
		Debut = 1,
		Circle = 2,
		Move = 3,
		Skill = 4,
		MoveOut = 5,
		Disappear = 6
	}

	protected enum HearbandElement
	{
		Ice = 0,
		Thunder = 1
	}

	protected class Hearband
	{
		public int ActionFrame;

		public int ActionTimes;

		public int LoopTimes;

		public int Position;

		public int Rotate;

		public HearbandStatus Status;

		public Transform Obj;
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_DEBUT_LOOP = 2,
		ANI_MOVE = 3,
		ANI_SKILL0_START1 = 4,
		ANI_SKILL0_START2 = 5,
		ANI_SKILL0_LOOP2 = 6,
		ANI_SKILL0_END2 = 7,
		ANI_SKILL0_END3 = 8,
		ANI_SKILL1_START1 = 9,
		ANI_SKILL1_LOOP1 = 10,
		ANI_SKILL1_END1 = 11,
		ANI_SKILL1_START2 = 12,
		ANI_SKILL1_LOOP2 = 13,
		ANI_SKILL1_END2 = 14,
		ANI_SKILL2_START1 = 15,
		ANI_SKILL2_START2 = 16,
		ANI_SKILL2_LOOP2 = 17,
		ANI_SKILL2_START3 = 18,
		ANI_SKILL2_LOOP3 = 19,
		ANI_SKILL2_END3 = 20,
		ANI_HURT = 21,
		ANI_DEAD = 22,
		MAX_ANIMATION_ID = 23
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

	[SerializeField]
	private CharacterMaterial stickMesh;

	private bool bHasShow;

	[Header("邪惡迴旋曲")]
	[SerializeField]
	private float fStickRotateTime = 1f;

	[SerializeField]
	private int nSkill0ActTimes = 3;

	[SerializeField]
	private int nSkill0Spd = 12000;

	[SerializeField]
	private float fSKill0WaitTime = 0.5f;

	[SerializeField]
	private ParticleSystem psSKill0UseFX;

	private float fMoveDis;

	private Vector2 WallDis = new Vector2(2f, 3f);

	[Header("連續循環")]
	[SerializeField]
	private Transform WeaponScepter;

	[SerializeField]
	private float fSkill1ActTime = 0.8f;

	[SerializeField]
	private Transform[] ScepterObjs = new Transform[4];

	[SerializeField]
	private ParticleSystem psSKill1UseFX1;

	[SerializeField]
	private ParticleSystem psSKill1UseFX2;

	[SerializeField]
	private int ScepterRotateOverTime = 8;

	[SerializeField]
	private float fSkill1SceptersSpd = 8f;

	private Scepter[] Scepters = new Scepter[4];

	private CollideBullet[] SceptersCollide = new CollideBullet[4];

	[Header("冰火挽歌")]
	[SerializeField]
	private GameObject HearbandMesh;

	[SerializeField]
	private Transform[] HearbandsPos = new Transform[2];

	[SerializeField]
	private Transform[] HearbandsObjs = new Transform[4];

	[SerializeField]
	private float fHearbandWallDis = 0.75f;

	[SerializeField]
	private int nSkill2IceActTime = 1;

	[SerializeField]
	private int nSkill2ThunderActTime = 3;

	private CollideBullet[] HearbandsCollide = new CollideBullet[4];

	private Hearband[] Hearbands;

	private HearbandElement Element;

	[Header("邪惡迴旋曲-飄落版")]
	[SerializeField]
	private int nSkill4Spd = 8000;

	[SerializeField]
	private int nSkill4FallSpd = -1000;

	[SerializeField]
	private float fSkill4StartTweenTime = 1f;

	[SerializeField]
	private float fSkill4DuringTweenTime = 1.5f;

	[SerializeField]
	private int nSkill4ActTimes = 3;

	[Header("待機等待Frame")]
	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[Header("AI控制")]
	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SkillPattern skillPattern = SkillPattern.State1;

	private SkillPattern nextState = SkillPattern.State1;

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

	private void PlaySE_pandora12(bool bPlay)
	{
		if (bPlay)
		{
			if (!bPlayLP)
			{
				PlaySE("BossSE06", "bs051_pandora12_lp");
				bPlayLP = true;
			}
		}
		else if (bPlayLP)
		{
			PlaySE("BossSE06", "bs051_pandora12_stop");
			bPlayLP = false;
		}
	}

	private void CreateScepters()
	{
		for (int i = 0; i < ScepterObjs.Length; i++)
		{
			Scepters[i] = newScepter(i);
			SceptersCollide[i].UpdateBulletData(EnemyWeapons[2].BulletData);
			SceptersCollide[i].SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			ScepterObjs[i].gameObject.SetActive(false);
		}
	}

	private Scepter newScepter(int id)
	{
		Scepter obj = new Scepter
		{
			Status = ScepterStatus.Disappear,
			Obj = ScepterObjs[id]
		};
		ScepterObjs[id].SetParentNull();
		obj.ActionFrame = 0;
		obj.Rotate = 0;
		return obj;
	}

	private bool IsSceptersGoOut()
	{
		if (Scepters == null)
		{
			return true;
		}
		for (int i = 0; i < Scepters.Length; i++)
		{
			if (Scepters[i].Status != ScepterStatus.Out)
			{
				return false;
			}
			if (GameLogicUpdateManager.GameFrame < Scepters[i].ActionFrame)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsSceptersDisappear()
	{
		if (Scepters == null)
		{
			return true;
		}
		for (int i = 0; i < Scepters.Length; i++)
		{
			if (Scepters[i].Status != ScepterStatus.Disappear)
			{
				return false;
			}
		}
		return true;
	}

	private Hearband newHearband(Transform obj, int pos)
	{
		Hearband obj2 = new Hearband
		{
			Obj = obj
		};
		obj.SetParentNull();
		obj2.ActionFrame = 0;
		obj2.LoopTimes = 4;
		obj2.Rotate = 180;
		obj2.Position = pos;
		obj2.Status = HearbandStatus.Idle;
		return obj2;
	}

	private bool IsHearbandsCircleOver()
	{
		if (Hearbands == null)
		{
			return true;
		}
		for (int i = 0; i < Hearbands.Length; i++)
		{
			if (Hearbands[i].Rotate > 0)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsHearbandsDisappear()
	{
		if (Hearbands == null)
		{
			return true;
		}
		for (int i = 0; i < Hearbands.Length; i++)
		{
			if (Hearbands[i].Status != HearbandStatus.Disappear)
			{
				return false;
			}
		}
		return true;
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
				NextSkill = MainStatus.Skill3;
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
		_animationHash = new int[23];
		_animationHash[0] = Animator.StringToHash("BS114@IDL_LOOP");
		_animationHash[1] = Animator.StringToHash("BS114@DEBUT");
		_animationHash[2] = Animator.StringToHash("BS114@DEBUT_LOOP");
		_animationHash[3] = Animator.StringToHash("BS114@MOVE_LOOP");
		_animationHash[4] = Animator.StringToHash("BS114@PANDORA_SKILL1_CASTING1");
		_animationHash[5] = Animator.StringToHash("BS114@PANDORA_SKILL1_CASTING2");
		_animationHash[6] = Animator.StringToHash("BS114@PANDORA_SKILL1_CASTLOOP1");
		_animationHash[7] = Animator.StringToHash("BS114@PANDORA_SKILL1_CASTING3");
		_animationHash[8] = Animator.StringToHash("BS114@PANDORA_SKILL1_CASTOUT1");
		_animationHash[9] = Animator.StringToHash("BS114@PANDORA_SKILL2_CASTING1");
		_animationHash[10] = Animator.StringToHash("BS114@PANDORA_SKILL2_CASTLOOP1");
		_animationHash[11] = Animator.StringToHash("BS114@PANDORA_SKILL2_CASTOUT1");
		_animationHash[12] = Animator.StringToHash("BS114@PANDORA_SKILL2_CASTING2");
		_animationHash[13] = Animator.StringToHash("BS114@PANDORA_SKILL2_CASTLOOP2");
		_animationHash[14] = Animator.StringToHash("BS114@PANDORA_SKILL2_CASTOUT2");
		_animationHash[15] = Animator.StringToHash("BS114@PANDORA_SKILL3_CASTING1");
		_animationHash[16] = Animator.StringToHash("BS114@PANDORA_SKILL3_CASTING2");
		_animationHash[17] = Animator.StringToHash("BS114@PANDORA_SKILL3_CASTLOOP2");
		_animationHash[18] = Animator.StringToHash("BS114@PANDORA_SKILL3_CASTING3");
		_animationHash[19] = Animator.StringToHash("BS114@PANDORA_SKILL3_CASTLOOP3");
		_animationHash[20] = Animator.StringToHash("BS114@PANDORA_SKILL3_CASTOUT1");
		_animationHash[21] = Animator.StringToHash("BS114@HURT_LOOP");
		_animationHash[22] = Animator.StringToHash("BS114@DEAD");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		for (int i = 1; i <= SceptersCollide.Length; i++)
		{
			SceptersCollide[i - 1] = OrangeBattleUtility.FindChildRecursive(ref childs, "SceptersCollide" + i, true).gameObject.AddOrGetComponent<CollideBullet>();
		}
		for (int j = 1; j <= HearbandsCollide.Length; j++)
		{
			HearbandsCollide[j - 1] = OrangeBattleUtility.FindChildRecursive(ref childs, "HearbandsCollide" + j, true).gameObject.AddOrGetComponent<CollideBullet>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		base.AimPoint = new Vector3(0f, 1.5f, 0f);
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
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				PlaySE("BossSE06", "bs051_pandora01");
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fStickRotateTime * 20f);
				break;
			case SubStatus.Phase2:
				DisAppear();
				break;
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
				Appear();
				ActionTimes = nSkill0ActTimes;
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE06", "bs051_pandora09");
				break;
			case SubStatus.Phase2:
				PlaySE("BossSE06", "bs051_pandora10");
				EndPos = GetTargetPos();
				StartPos = NowPos;
				fMoveDis = Vector2.Distance(StartPos, EndPos);
				_velocity = new VInt3((EndPos - StartPos).normalized) * nSkill0Spd * 0.001f;
				UpdateDirection();
				SwitchFx(psSKill0UseFX, true);
				break;
			case SubStatus.Phase3:
				SwitchFx(psSKill0UseFX, false);
				ActionFrame = GameLogicUpdateManager.GameFrame + 15;
				break;
			case SubStatus.Phase4:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSKill0WaitTime * 20f);
				break;
			case SubStatus.Phase6:
				DisAppear();
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Appear();
				ActionTimes = 4;
				UpdateDirection(-1);
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE06", "bs051_pandora11");
				SwitchFx(psSKill1UseFX1, true);
				break;
			case SubStatus.Phase2:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill1ActTime * 20f);
				SwitchFx(psSKill1UseFX2, true);
				break;
			case SubStatus.Phase3:
				SwitchFx(psSKill1UseFX1, false);
				SwitchFx(psSKill1UseFX2, false);
				break;
			case SubStatus.Phase4:
				PlaySE("BossSE06", "bs051_pandora16");
				SwitchFx(psSKill1UseFX1, true);
				break;
			case SubStatus.Phase6:
				SwitchFx(psSKill1UseFX1, false);
				ActionTimes = 4;
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(fSkill1ActTime * 20f);
				break;
			case SubStatus.Phase7:
				DisAppear();
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Appear();
				UpdateDirection(-1);
				if (OrangeBattleUtility.Random(0, 100) < 50)
				{
					Element = HearbandElement.Thunder;
				}
				else
				{
					Element = HearbandElement.Ice;
				}
				UpdateTex();
				Hearbands = new Hearband[2];
				HearbandsObjs[(int)Element * 2].position = HearbandsPos[0].position;
				HearbandsObjs[(int)Element * 2 + 1].position = HearbandsPos[1].position;
				switch (Element)
				{
				case HearbandElement.Ice:
					Hearbands[0] = newHearband(HearbandsObjs[(int)Element * 2], 0);
					Hearbands[1] = newHearband(HearbandsObjs[(int)Element * 2 + 1], 2);
					break;
				case HearbandElement.Thunder:
					Hearbands[0] = newHearband(HearbandsObjs[(int)Element * 2], 0);
					Hearbands[1] = newHearband(HearbandsObjs[(int)Element * 2 + 1], 1);
					break;
				}
				break;
			case SubStatus.Phase3:
			{
				if (Element == HearbandElement.Thunder)
				{
					PlaySE("BossSE06", "bs051_pandora07");
				}
				else
				{
					PlaySE("BossSE06", "bs051_pandora04");
				}
				for (int i = 0; i < Hearbands.Length; i++)
				{
					Hearbands[i].Obj.localRotation = Quaternion.Euler(0f, 0f, Hearbands[i].Rotate);
					Hearbands[i].Status = HearbandStatus.Debut;
					Hearbands[i].Obj.gameObject.SetActive(true);
					HearbandsCollide[(int)Element * 2 + i].Active(targetMask);
					HearbandMesh.SetActive(false);
				}
				break;
			}
			case SubStatus.Phase5:
				DisAppear();
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Appear();
				ActionTimes = 0;
				break;
			case SubStatus.Phase1:
				LeanTween.value(base.gameObject, nSkill4Spd / 4 * -base.direction, nSkill4Spd * base.direction, fSkill4StartTweenTime).setOnUpdate(delegate(float n)
				{
					_velocity.x = (int)n;
					_velocity.y = nSkill4FallSpd / 2;
				}).setOnComplete((Action)delegate
				{
					LeanTween.cancel(base.gameObject);
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				});
				break;
			case SubStatus.Phase2:
				LeanTween.value(base.gameObject, nSkill4Spd * base.direction, nSkill4Spd * base.direction, fSkill4DuringTweenTime).setOnComplete((Action)delegate
				{
					_velocity.x = nSkill0Spd / 4 * base.direction;
					ActionTimes++;
					if (ActionTimes < nSkill0ActTimes)
					{
						LeanTween.value(base.gameObject, nSkill0Spd / 4 * base.direction, nSkill0Spd * -base.direction, fSkill4StartTweenTime).setOnUpdate(delegate(float n)
						{
							if (ActionTimes > 0)
							{
								UpdateDirection((_velocity.x >= 0) ? 1 : (-1));
							}
							_velocity.x = (int)n;
							_velocity.y = nSkill4FallSpd / 2;
						}).setOnComplete((Action)delegate
						{
							SetStatus(MainStatus.Skill3, SubStatus.Phase2);
						});
					}
					else
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase3);
					}
				});
				LeanTween.value(base.gameObject, nSkill4FallSpd, 0f, fSkill4DuringTweenTime);
				break;
			case SubStatus.Phase3:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase4:
				DisAppear();
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
				_currentAnimationId = AnimationID.ANI_DEBUT_LOOP;
				break;
			case SubStatus.Phase2:
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
				_currentAnimationId = AnimationID.ANI_SKILL0_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_START2;
				break;
			case SubStatus.Phase2:
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL0_END2;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL0_END3;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_END1;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL1_START2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP2;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL1_END2;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL0_END3;
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
				_currentAnimationId = AnimationID.ANI_SKILL2_START2;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_START3;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP3;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_END3;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START1;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_START2;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP2;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_END2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL0_END3;
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			}
			return;
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
				skillPattern = nextState;
				switch (skillPattern)
				{
				case SkillPattern.State1:
					mainStatus = MainStatus.Skill2;
					break;
				case SkillPattern.State2:
					mainStatus = MainStatus.Skill1;
					nextState = SkillPattern.State1;
					break;
				case SkillPattern.State3:
					mainStatus = MainStatus.Skill0;
					nextState = SkillPattern.State1;
					break;
				default:
					mainStatus = MainStatus.Skill2;
					nextState = SkillPattern.State2;
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
				if (_introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (IntroCallBack != null && !bHasShow)
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
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f && bHasShow)
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
				if (Vector2.Distance(NowPos, StartPos) >= fMoveDis)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				_velocity /= 2f;
				if (Vector2.Distance(Vector2.zero, new Vector2(_velocity.x, _velocity.y)) < 500f || GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					_velocity = VInt3.zero;
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill0, SubStatus.Phase5);
					}
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f && !bHasShow)
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
				if (_currentFrame > 1f && bHasShow)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					PlaySE_pandora12(true);
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (ActionTimes > 0 && GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					int num = 4 - ActionTimes;
					Scepters[num].Obj.gameObject.SetActive(true);
					Scepters[num].Obj.position = WeaponScepter.position;
					Scepters[num].Obj.localRotation = Quaternion.identity;
					Scepters[num].Status = ScepterStatus.Move;
					SceptersCollide[num].Active(targetMask);
					ActionFrame += (int)(fSkill1ActTime * 20f);
					ActionTimes--;
				}
				ScepterLogicUpdate();
				if (ActionTimes <= 0 && IsSceptersGoOut())
				{
					PlaySE_pandora12(false);
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
					SetStatus(MainStatus.Skill1, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (ActionTimes > 0 && GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					int ScepterID = 4 - ActionTimes;
					EndPos = GetTargetPos();
					Scepters[ScepterID].Obj.gameObject.SetActive(true);
					Scepters[ScepterID].Obj.position = new Vector3(EndPos.x, MaxPos.y + 5f, 0f);
					Scepters[ScepterID].Obj.localRotation = Quaternion.identity;
					Scepters[ScepterID].Status = ScepterStatus.Down;
					PlaySE("BossSE06", "bs051_pandora14");
					Scepters[ScepterID].ActionFrame = GameLogicUpdateManager.GameFrame + 7;
					Scepters[ScepterID].Velocity = (GetTargetPos() - Scepters[ScepterID].Obj.position).normalized * fSkill1SceptersSpd;
					SceptersCollide[ScepterID].Active(targetMask);
					LeanTween.move(Scepters[ScepterID].Obj.gameObject, new Vector3(EndPos.x, MinPos.y + 0.5f, 0f), 0.3f).setOnUpdateVector3(delegate(Vector3 pos)
					{
						Scepters[ScepterID].Obj.position = pos;
					}).setOnComplete((Action)delegate
					{
						LeanTween.cancel(Scepters[ScepterID].Obj.gameObject);
					});
					ActionFrame += 10;
					ActionTimes--;
				}
				ScepterLogicUpdate();
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (ActionTimes > 0 && GameLogicUpdateManager.GameFrame >= ActionFrame)
				{
					int ScepterID2 = 4 - ActionTimes;
					EndPos = GetTargetPos();
					Scepters[ScepterID2].Obj.gameObject.SetActive(true);
					Scepters[ScepterID2].Obj.position = new Vector3(EndPos.x, MaxPos.y + 5f, 0f);
					Scepters[ScepterID2].Obj.localRotation = Quaternion.identity;
					Scepters[ScepterID2].Status = ScepterStatus.Down;
					PlaySE("BossSE06", "bs051_pandora14");
					Scepters[ScepterID2].ActionFrame = 0;
					Scepters[ScepterID2].Velocity = (GetTargetPos() - Scepters[ScepterID2].Obj.position).normalized * fSkill1SceptersSpd;
					SceptersCollide[ScepterID2].Active(targetMask);
					LeanTween.move(Scepters[ScepterID2].Obj.gameObject, new Vector3(EndPos.x, MinPos.y + 0.25f, 0f), 0.3f).setOnUpdateVector3(delegate(Vector3 pos)
					{
						Scepters[ScepterID2].Obj.position = pos;
					}).setOnComplete((Action)delegate
					{
						LeanTween.cancel(Scepters[ScepterID2].Obj.gameObject);
						Scepters[ScepterID2].Status = ScepterStatus.Ground;
					});
					ActionFrame += (int)(fSkill1ActTime * 20f);
					ActionTimes--;
				}
				ScepterLogicUpdate();
				if (ActionTimes <= 0 && IsSceptersDisappear() && _currentFrame > 1f && !bHasShow)
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
				if (_currentFrame > 1f && bHasShow)
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
				SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f && !bHasShow)
				{
					switch (Element)
					{
					case HearbandElement.Ice:
						nextState = SkillPattern.State2;
						break;
					case HearbandElement.Thunder:
						nextState = SkillPattern.State3;
						break;
					default:
						nextState = SkillPattern.State2;
						break;
					}
					HearbandMesh.SetActive(true);
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f && bHasShow)
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
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f && !bHasShow)
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
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			}
			break;
		}
		if (Hearbands != null && !IsHearbandsDisappear())
		{
			HeadbandUpdate();
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Debut)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	private void ScepterLogicUpdate()
	{
		if (Scepters == null)
		{
			return;
		}
		int i;
		for (i = 0; i < Scepters.Length; i++)
		{
			if (Scepters[i].Status == ScepterStatus.Disappear)
			{
				continue;
			}
			switch (Scepters[i].Status)
			{
			case ScepterStatus.Move:
				Scepters[i].Obj.position = Vector3.MoveTowards(Scepters[i].Obj.position, GetTargetPoint() + Vector3.back, 0.2f);
				if (Vector2.Distance(Scepters[i].Obj.position, GetTargetPoint()) < 0.2f)
				{
					Scepters[i].Rotate = 0;
					Scepters[i].Status = ScepterStatus.Circle;
				}
				break;
			case ScepterStatus.Circle:
				Scepters[i].Rotate += ScepterRotateOverTime;
				Scepters[i].Obj.position = GetTargetPoint() + Quaternion.Euler(0f, -Scepters[i].Rotate, 0f) * Vector3.back;
				if (Scepters[i].Rotate >= 360)
				{
					PlaySE("BossSE06", "bs051_pandora13");
					Scepters[i].Status = ScepterStatus.Out;
					Scepters[i].ActionFrame = GameLogicUpdateManager.GameFrame + 40;
					Scepters[i].Velocity = (GetTargetPos() - Scepters[i].Obj.position).normalized * fSkill1SceptersSpd;
					Scepters[i].Velocity.z = 0f;
					LeanTween.move(Scepters[i].Obj.gameObject, Scepters[i].Obj.position + Scepters[i].Velocity * 3f, 3f).setOnUpdateVector3(delegate(Vector3 pos)
					{
						Scepters[i].Obj.position = pos;
					}).setOnComplete((Action)delegate
					{
						LeanTween.cancel(Scepters[i].Obj.gameObject);
					});
				}
				break;
			case ScepterStatus.Out:
				Scepters[i].Rotate += ScepterRotateOverTime * 5;
				Scepters[i].Obj.localRotation = Quaternion.Euler(0f, 0f, Scepters[i].Rotate);
				break;
			case ScepterStatus.Down:
				if (GameLogicUpdateManager.GameFrame >= Scepters[i].ActionFrame)
				{
					Scepters[i].ActionFrame = 0;
					Scepters[i].Status = ScepterStatus.Ground;
				}
				break;
			case ScepterStatus.Ground:
				if (Scepters[i].ActionFrame >= 20)
				{
					Scepters[i].Status = ScepterStatus.Disappear;
					Scepters[i].Obj.gameObject.SetActive(false);
					SceptersCollide[i].BackToPool();
				}
				Scepters[i].ActionFrame++;
				break;
			default:
				Debug.LogError("權杖 LogicUpdate 不該進來這裡");
				Scepters[i].Status = ScepterStatus.Disappear;
				Scepters[i].Obj.gameObject.SetActive(false);
				SceptersCollide[i].BackToPool();
				break;
			}
		}
	}

	private void HeadbandUpdate()
	{
		if (Hearbands == null)
		{
			return;
		}
		for (int i = 0; i < Hearbands.Length; i++)
		{
			if (Hearbands[i].Status == HearbandStatus.Disappear)
			{
				continue;
			}
			switch (Hearbands[i].Status)
			{
			case HearbandStatus.Debut:
			{
				float num3 = 0f;
				float y = 0f;
				num3 = ((i != 0) ? (MaxPos.x - fHearbandWallDis) : (MinPos.x + fHearbandWallDis));
				switch (Hearbands[i].Position)
				{
				case 0:
					y = MaxPos.y - fHearbandWallDis;
					break;
				case 1:
				case 3:
					y = CenterPos.y;
					break;
				case 2:
					y = MinPos.y + fHearbandWallDis;
					break;
				}
				Hearbands[i].Obj.position = Vector3.MoveTowards(Hearbands[i].Obj.position, new Vector3(num3, y, 0f), 0.2f);
				if (Vector2.Distance(Hearbands[i].Obj.position, new Vector3(num3, y, 0f)) < 0.2f)
				{
					Hearbands[i].Status = HearbandStatus.Circle;
				}
				break;
			}
			case HearbandStatus.Circle:
				if (Hearbands[i].Rotate > 0)
				{
					Hearbands[i].Rotate -= 18;
					Hearbands[i].Obj.localRotation = Quaternion.Euler(0f, 0f, Hearbands[i].Rotate);
				}
				if (IsHearbandsCircleOver())
				{
					Hearbands[i].Status = HearbandStatus.Move;
					if (Element == HearbandElement.Ice)
					{
						Hearbands[i].ActionFrame = GameLogicUpdateManager.GameFrame + 4;
						Hearbands[i].ActionTimes = nSkill2IceActTime;
					}
				}
				break;
			case HearbandStatus.Move:
				switch (Element)
				{
				case HearbandElement.Ice:
					if (GameLogicUpdateManager.GameFrame >= Hearbands[i].ActionFrame && Hearbands[i].ActionTimes > 0)
					{
						Vector3 pDirection = (i * 2 - 1) * Vector3.left;
						Vector3 position = Hearbands[i].Obj.position;
						BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						Hearbands[i].ActionTimes--;
						Hearbands[i].ActionFrame += 3;
					}
					break;
				}
				MoveHearband(Hearbands[i]);
				break;
			case HearbandStatus.Skill:
				switch (Element)
				{
				case HearbandElement.Ice:
					if (GameLogicUpdateManager.GameFrame >= Hearbands[i].ActionFrame)
					{
						if (--Hearbands[i].LoopTimes > 0)
						{
							Hearbands[i].Status = HearbandStatus.Move;
							Hearbands[i].ActionFrame = GameLogicUpdateManager.GameFrame + 4;
							Hearbands[i].ActionTimes = nSkill2IceActTime;
							MoveHearband(Hearbands[i]);
						}
						else
						{
							Hearbands[i].Status = HearbandStatus.MoveOut;
						}
					}
					break;
				case HearbandElement.Thunder:
					if (GameLogicUpdateManager.GameFrame < Hearbands[i].ActionFrame)
					{
						break;
					}
					if (Hearbands[i].ActionTimes > 0)
					{
						Vector3 pDirection2 = (i * 2 - 1) * Vector3.left;
						Vector3 position2 = Hearbands[i].Obj.position;
						BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, position2, pDirection2, null, selfBuffManager.sBuffStatus, EnemyData, targetMask)._transform.localScale = (0.6f + 0.4f * (float)Hearbands[i].ActionTimes / (float)nSkill2ThunderActTime) * Vector3.one;
						Hearbands[i].ActionTimes--;
						if (Hearbands[i].ActionTimes > 0)
						{
							Hearbands[i].ActionFrame += 3;
						}
						else
						{
							Hearbands[i].ActionFrame += 2;
						}
					}
					else if (--Hearbands[i].LoopTimes > 0)
					{
						Hearbands[i].Status = HearbandStatus.Move;
						MoveHearband(Hearbands[i]);
					}
					else
					{
						Hearbands[i].Status = HearbandStatus.MoveOut;
					}
					break;
				}
				break;
			case HearbandStatus.MoveOut:
			{
				float num = 0f;
				float num2 = 0f;
				num = ((i != 0) ? (MaxPos.x - fHearbandWallDis) : (MinPos.x + fHearbandWallDis));
				num2 = Hearbands[i].Obj.position.y;
				Vector3 vector = new Vector3(num, num2, 0f) + (i * 2 - 1) * Vector3.right * 5f;
				Hearbands[i].Obj.position = Vector3.MoveTowards(Hearbands[i].Obj.position, vector, 0.2f);
				if (Vector2.Distance(Hearbands[i].Obj.position, vector) < 0.2f)
				{
					Hearbands[i].Obj.gameObject.SetActive(false);
					Hearbands[i].Status = HearbandStatus.Disappear;
					HearbandsCollide[(int)Element * 2 + i].BackToPool();
				}
				break;
			}
			case HearbandStatus.Disappear:
				Debug.LogError("髮帶 Updata 不該來這裡");
				break;
			}
		}
		if (IsHearbandsDisappear())
		{
			Hearbands = null;
		}
	}

	private void MoveHearband(Hearband obj)
	{
		float y = 0f;
		switch (obj.Position)
		{
		case 0:
			y = MaxPos.y - fHearbandWallDis;
			break;
		case 1:
		case 3:
			y = CenterPos.y;
			break;
		case 2:
			y = MinPos.y + fHearbandWallDis;
			break;
		}
		Vector3 vector = new Vector3(obj.Obj.position.x, y, 0f);
		obj.Obj.position = Vector3.MoveTowards(obj.Obj.position, vector, 0.2f);
		if (Vector2.Distance(obj.Obj.position, vector) < 0.2f)
		{
			obj.Position++;
			obj.Position %= 4;
			obj.Status = HearbandStatus.Skill;
			switch (Element)
			{
			case HearbandElement.Ice:
				obj.ActionFrame = GameLogicUpdateManager.GameFrame + 10;
				break;
			case HearbandElement.Thunder:
				PlaySE("BossSE06", "bs051_pandora08");
				obj.ActionFrame = GameLogicUpdateManager.GameFrame + 2;
				obj.ActionTimes = nSkill2ThunderActTime;
				break;
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			IgnoreGravity = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			CheckRoomSize();
			CreateScepters();
			for (int i = 0; i < HearbandsCollide.Length; i++)
			{
				HearbandsCollide[i].UpdateBulletData(EnemyWeapons[3 + i / 2 * 2].BulletData);
				HearbandsCollide[i].SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				HearbandsObjs[i].gameObject.SetActive(false);
			}
			SetStatus(MainStatus.Debut);
		}
		else
		{
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
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		base.SoundSource.PlaySE("BossSE06", "bs051_pandora15", 0.5f);
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		PlaySE_pandora12(false);
		CollideBullet[] sceptersCollide = SceptersCollide;
		foreach (CollideBullet collideBullet in sceptersCollide)
		{
			if (collideBullet != null && collideBullet.IsActivate)
			{
				collideBullet.BackToPool();
			}
		}
		sceptersCollide = HearbandsCollide;
		foreach (CollideBullet collideBullet2 in sceptersCollide)
		{
			if (collideBullet2 != null && collideBullet2.IsActivate)
			{
				collideBullet2.BackToPool();
			}
		}
		Transform[] scepterObjs = ScepterObjs;
		for (int i = 0; i < scepterObjs.Length; i++)
		{
			scepterObjs[i].gameObject.SetActive(false);
		}
		scepterObjs = HearbandsObjs;
		for (int i = 0; i < scepterObjs.Length; i++)
		{
			scepterObjs[i].gameObject.SetActive(false);
		}
		SwitchFx(psSKill0UseFX, false);
		SwitchFx(psSKill1UseFX1, false);
		SwitchFx(psSKill1UseFX2, false);
		StageUpdate.SlowStage();
		SetColliderEnable(false);
		HearbandMesh.SetActive(true);
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
				TargetPos = new VInt3(Target.GetTargetPoint() + Vector3.up * 0.15f);
			}
			return TargetPos.vec3;
		}
		return CenterPos;
	}

	private void UpdateTex()
	{
		_characterMaterial.UpdateTex((int)Element);
		stickMesh.UpdateTex((int)Element);
	}

	private void Appear()
	{
		PlaySE("BossSE06", "bs051_pandora03");
		switch (_mainStatus)
		{
		case MainStatus.Skill0:
			if (GetTargetPos().x > CenterPos.x)
			{
				EndPos = new Vector3(MinPos.x + WallDis.x, MaxPos.y - WallDis.y, 0f);
			}
			else
			{
				EndPos = new Vector3(MaxPos.x - WallDis.x, MaxPos.y - WallDis.y, 0f);
			}
			break;
		case MainStatus.Skill1:
			EndPos = new Vector3(CenterPos.x, MinPos.y + 0.5f, 0f);
			break;
		case MainStatus.Skill2:
			EndPos = CenterPos + Vector3.down * Controller.Collider2D.size.y;
			break;
		}
		_transform.position = EndPos;
		Controller.LogicPosition = new VInt3(EndPos);
		stickMesh.Appear(delegate
		{
			_characterMaterial.Appear(delegate
			{
				bHasShow = true;
				base.AllowAutoAim = true;
				SetColliderEnable(true);
				_collideBullet.Active(targetMask);
			});
		});
	}

	private void DisAppear()
	{
		SetColliderEnable(false);
		PlaySE("BossSE06", "bs051_pandora03");
		base.AllowAutoAim = false;
		_collideBullet.BackToPool();
		_characterMaterial.Disappear();
		stickMesh.Disappear(delegate
		{
			bHasShow = false;
		});
	}

	private void Skill0Start()
	{
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
}
