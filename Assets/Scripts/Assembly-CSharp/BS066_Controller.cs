#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS066_Controller : EnemyControllerBase, IManagedUpdateBehavior, IF_Master
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Skill2 = 4,
		Skill3 = 5,
		TestSkill0 = 6,
		TestSkill1 = 7,
		TestSkill2 = 8,
		TestSkill3 = 9,
		Die = 10
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
		ANI_Skill0_START = 2,
		ANI_Skill0_LOOP = 3,
		ANI_Skill0_END = 4,
		ANI_DEAD = 5,
		MAX_ANIMATION_ID = 6
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

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

	private Vector3 EndPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private static readonly int[] SkillCard = new int[4] { 0, 1, 2, 3 };

	private List<int> UseSKC = new List<int>(SkillCard);

	private bool CanKeepOn = true;

	private bool haveSpawn;

	private EM157_Controller EMLHand;

	private EM157_Controller EMRHand;

	private BossCorpsTool LHandCorp;

	private BossCorpsTool RHandCorp;

	[SerializeField]
	private float IdleWaitTime = 1f;

	private int IdleWaitFrame;

	[SerializeField]
	private float tempangle = 24f;

	[SerializeField]
	private float jdis = 5f;

	private int lasthand;

	[SerializeField]
	private Transform[] SK1Objs = new Transform[5];

	private FxBase[] SK1USEFX = new FxBase[5];

	private FxBase[] SK1FX = new FxBase[5];

	private CollideBullet[] SK1Collide = new CollideBullet[5];

	[SerializeField]
	private int DefaultAtkTime = 5;

	private int AtkTime;

	[SerializeField]
	private int DefaultTrackTime = 4;

	private int TrackTime;

	[SerializeField]
	private float AtkIntervalTime = 1f;

	private int AtkIntervalFrame;

	[SerializeField]
	private float SK1MoveDisDelta = 0.1f;

	private float SK1MoveDis;

	private float Moved;

	[SerializeField]
	private float WaitTime = 2f;

	private int WaitFrame;

	[SerializeField]
	private float MoveWaitTime = 1f;

	private int MoveWaitFrame;

	private Vector3 MovePosTmp;

	[SerializeField]
	private int DefaultSkill2AtkTimes = 4;

	[SerializeField]
	private Transform ShootPos;

	[SerializeField]
	private ParticleSystem HeadShootFx;

	private int Sk2AtkTimes;

	private float ShootFrame;

	private bool HasShot;

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
				NextSkill = MainStatus.TestSkill0;
				break;
			case "Skill5":
				NextSkill = MainStatus.TestSkill1;
				break;
			case "Skill6":
				NextSkill = MainStatus.TestSkill2;
				break;
			case "Skill7":
				NextSkill = MainStatus.TestSkill3;
				break;
			}
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash = new int[6];
		_animationHash[0] = Animator.StringToHash("BS066@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS066@debut");
		_animationHash[2] = Animator.StringToHash("BS066@skill_start");
		_animationHash[3] = Animator.StringToHash("BS066@skill_loop");
		_animationHash[4] = Animator.StringToHash("BS066@skill_end");
		_animationHash[5] = Animator.StringToHash("BS066@dead02");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "HeadShootPoint", true);
		SK1Objs = OrangeBattleUtility.FindAllChildRecursive(ref childs, "Skill1Collider");
		for (int i = 0; i < SK1Objs.Length; i++)
		{
			SK1Collide[i] = SK1Objs[i].gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPos");
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		HashAnimation();
		LoadParts(ref childs);
		base.AimPoint = new Vector3(0f, 0f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(30f);
		if (HeadShootFx != null)
		{
			HeadShootFx.Stop();
		}
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_final-sigma-w_000", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("p_final-sigma-w_000", 3);
		if ((bool)_characterMaterial)
		{
			_characterMaterial.HurtColor = new Color(0.43f, 0.43f, 0.43f, 0.65f);
		}
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

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			if (_subStatus == SubStatus.Phase0)
			{
				SpawnHand();
			}
			break;
		case MainStatus.Idle:
			lasthand = 0;
			_velocity = VInt3.zero;
			IdleWaitFrame = GameLogicUpdateManager.GameFrame + (int)(IdleWaitTime * 20f);
			break;
		case MainStatus.TestSkill0:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				LHandCorp.SendMission(0);
				RHandCorp.SendMission(0);
				break;
			}
			break;
		case MainStatus.TestSkill1:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				LHandCorp.SendMission(1);
				RHandCorp.SendMission(1);
				break;
			}
			break;
		case MainStatus.TestSkill2:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				LHandCorp.SendMission(2);
				RHandCorp.SendMission(2);
				break;
			}
			break;
		case MainStatus.TestSkill3:
			SetStatus(MainStatus.Skill2);
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				CanKeepOn = false;
				break;
			case SubStatus.Phase1:
				SendMission();
				break;
			case SubStatus.Phase2:
				SendMission();
				break;
			}
			break;
		case MainStatus.Skill1:
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				AtkTime = 0;
				TrackTime = 0;
				SendMission();
				CanKeepOn = false;
				break;
			case SubStatus.Phase1:
				AtkIntervalFrame = GameLogicUpdateManager.GameFrame + (int)(AtkIntervalTime * 20f);
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					EndPos = Target.AimPosition;
					if (Target._transform.position.x < MinPos.x)
					{
						EndPos.x = MinPos.x;
					}
					if (Target._transform.position.x > MaxPos.x)
					{
						EndPos.x = MaxPos.x;
					}
					if (Target._transform.position.y < MinPos.y)
					{
						EndPos.y = MinPos.y;
					}
					if (Target._transform.position.y > MaxPos.y)
					{
						EndPos.y = MaxPos.y;
					}
				}
				else
				{
					EndPos = _transform.position;
				}
				if (SK1Objs[AtkTime] != null)
				{
					SK1Objs[AtkTime].position = EndPos;
					SK1USEFX[AtkTime] = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_final-sigma-w_000", SK1Objs[AtkTime], Quaternion.identity, new object[1] { Vector3.one });
				}
				break;
			case SubStatus.Phase2:
				WaitFrame = GameLogicUpdateManager.GameFrame + (int)(WaitTime * 20f);
				break;
			case SubStatus.Phase3:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					Vector3 vector = Target._transform.position - SK1Objs[AtkTime].position;
					if (Mathf.Abs(vector.x) < Mathf.Abs(vector.y) || Mathf.Abs(vector.y) > 3f)
					{
						if ((vector.y > 0f && Mathf.Abs(SK1Objs[AtkTime].position.y - MaxPos.y) > 0.4f) || (vector.y <= 0f && Mathf.Abs(SK1Objs[AtkTime].position.y - MinPos.y) <= 0.4f))
						{
							EndPos = new Vector3(SK1Objs[AtkTime].position.x, MaxPos.y, 0f);
						}
						else
						{
							EndPos = new Vector3(SK1Objs[AtkTime].position.x, MinPos.y, 0f);
						}
					}
					else if ((vector.x > 0f && Mathf.Abs(SK1Objs[AtkTime].position.x - MaxPos.x) > 0.4f) || (vector.x <= 0f && Mathf.Abs(SK1Objs[AtkTime].position.x - MinPos.x) <= 0.4f))
					{
						EndPos = new Vector3(MaxPos.x, SK1Objs[AtkTime].position.y, 0f);
					}
					else
					{
						EndPos = new Vector3(MinPos.x, SK1Objs[AtkTime].position.y, 0f);
					}
				}
				else
				{
					EndPos = new Vector3(SK1Objs[AtkTime].position.x, MaxPos.y, 0f);
				}
				Moved = 0f;
				SK1MoveDis = Vector3.Distance(EndPos, SK1Objs[AtkTime].position);
				MovePosTmp = Vector3.MoveTowards(SK1Objs[AtkTime].position, EndPos, SK1MoveDisDelta * 3f);
				break;
			case SubStatus.Phase4:
				MoveWaitFrame = GameLogicUpdateManager.GameFrame + (int)(MoveWaitTime * 20f);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Sk2AtkTimes = DefaultSkill2AtkTimes;
				SendMission();
				break;
			case SubStatus.Phase1:
				ShootFrame = 1.5f;
				HasShot = false;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (StageUpdate.gStageName != "stage04_4001_e1")
				{
					PlaySE("BossSE04", "bs039_hellsig08");
				}
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
				}
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
				}
				if ((bool)EMLHand)
				{
					EMLHand.CloseCollider();
				}
				if ((bool)EMRHand)
				{
					EMRHand.CloseCollider();
				}
				break;
			case SubStatus.Phase1:
			{
				AI_STATE aiState = AiState;
				if (aiState == AI_STATE.mob_002)
				{
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
				}
				else
				{
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
				}
				break;
			}
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void SendMission()
	{
		switch (_mainStatus)
		{
		case MainStatus.Skill0:
		case MainStatus.Skill2:
			if (lasthand == 0)
			{
				if (OrangeBattleUtility.Random(0, 100) < 50)
				{
					lasthand = -1;
					LHandCorp.SendMission(0);
				}
				else
				{
					lasthand = 1;
					RHandCorp.SendMission(0);
				}
			}
			else if (lasthand == 1)
			{
				LHandCorp.SendMission(0);
			}
			else if (lasthand == -1)
			{
				RHandCorp.SendMission(0);
			}
			break;
		case MainStatus.Skill1:
			LHandCorp.SendMission(2);
			break;
		case MainStatus.Skill3:
			RHandCorp.SendMission(1);
			break;
		}
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		default:
			return;
		case MainStatus.Debut:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase1)
			{
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			}
			return;
		}
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill1:
		case MainStatus.Skill3:
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
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_Skill0_END;
				break;
			}
			break;
		case MainStatus.Skill2:
		case MainStatus.TestSkill0:
		case MainStatus.TestSkill1:
		case MainStatus.TestSkill2:
		case MainStatus.TestSkill3:
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
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill0_END;
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			return;
		case MainStatus.Skill0:
			return;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			SetStatus(MainStatus.Idle);
			break;
		case MainStatus.Idle:
			mainStatus = (MainStatus)RandomCard(2);
			break;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
	}

	private int RandomCard(int StartPos)
	{
		if (UseSKC.ToArray().Length < 1)
		{
			UseSKC = new List<int>(SkillCard);
		}
		int num = UseSKC[OrangeBattleUtility.Random(0, UseSKC.ToArray().Length)];
		UseSKC.Remove(num);
		return num + StartPos;
	}

	public override void LogicUpdate()
	{
		if (_mainStatus == MainStatus.Debut)
		{
			BaseUpdate();
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
				if (IntroCallBack != null)
				{
					IntroCallBack();
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
				if (_introReady && !bWaitNetStatus)
				{
					base.AllowAutoAim = true;
					EMLHand.SetIdle();
					EMRHand.SetIdle();
					UpdateRandomState();
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
			if ((bool)Target)
			{
				if (DebugMode)
				{
					SetStatus(NextSkill);
				}
				else
				{
					UpdateRandomState();
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
				if (CanKeepOn)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (CheckMissionOverAll())
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
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
		case MainStatus.TestSkill0:
		case MainStatus.TestSkill1:
		case MainStatus.TestSkill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(_mainStatus, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (CheckMissionOverAll())
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				SetStatus(MainStatus.Idle);
				break;
			}
			break;
		case MainStatus.Skill1:
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f && CanKeepOn)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (AtkIntervalFrame < GameLogicUpdateManager.GameFrame)
				{
					if (SK1USEFX[AtkTime] != null)
					{
						SK1USEFX[AtkTime] = null;
					}
					SK1FX[AtkTime] = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("p_final-sigma-w_000", SK1Objs[AtkTime], Quaternion.identity, new object[1] { Vector3.one });
					SK1Collide[AtkTime].Active(targetMask);
					if (AtkTime > 0)
					{
						SK1FX[AtkTime - 1].BackToPool();
						SK1Collide[AtkTime - 1].BackToPool();
					}
					AtkTime++;
					if (AtkTime >= DefaultAtkTime)
					{
						AtkTime--;
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
					else
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
					}
				}
				break;
			case SubStatus.Phase2:
				if (WaitFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (Moved > SK1MoveDis)
				{
					PlaySE("BossSE03", "bs029_finsig06");
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
					SK1Objs[AtkTime].position = EndPos;
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				Moved += SK1MoveDisDelta * 3f;
				MovePosTmp = Vector3.MoveTowards(SK1Objs[AtkTime].position, EndPos, SK1MoveDisDelta * 3f);
				break;
			case SubStatus.Phase4:
				if (MoveWaitFrame < GameLogicUpdateManager.GameFrame)
				{
					TrackTime++;
					if (TrackTime < DefaultTrackTime)
					{
						SetStatus(_mainStatus, SubStatus.Phase3);
						break;
					}
					SK1FX[AtkTime].BackToPool();
					SK1Collide[AtkTime].BackToPool();
					SetStatus(_mainStatus, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f && CheckMissionOverAll())
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
					SetStatus(_mainStatus, SubStatus.Phase1);
					if (HeadShootFx != null && !HeadShootFx.isPlaying)
					{
						HeadShootFx.Play();
					}
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 3f)
				{
					if (Sk2AtkTimes > 0)
					{
						SetStatus(_mainStatus, SubStatus.Phase1);
					}
					else
					{
						SetStatus(_mainStatus, SubStatus.Phase2);
					}
				}
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					float x = MaxPos.x - (MaxPos.x - MinPos.x) / (float)(DefaultSkill2AtkTimes + 2) * (float)Sk2AtkTimes;
					EndPos = new Vector3(x, MinPos.y - 1.5f, 0f);
					Vector3 vector = EndPos - ShootPos.position;
					BulletBase.TryShotBullet(EnemyWeapons[0].BulletData, ShootPos, EndPos, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					Sk2AtkTimes--;
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f && CheckMissionOverAll())
				{
					SetStatus(MainStatus.Idle);
					if (HeadShootFx != null)
					{
						HeadShootFx.Stop();
					}
				}
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.1f)
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
		if ((bool)Target)
		{
			float num = Target._transform.position.x - _transform.position.x;
			num = ((num > jdis) ? jdis : ((num < 0f - jdis) ? (0f - jdis) : num));
			ModelTransform.eulerAngles = Vector3.up * (90f + (0f - tempangle) * (num / jdis) / 2f);
		}
	}

	public void UpdateFunc()
	{
		if (Activate || _mainStatus == MainStatus.Die)
		{
			MainStatus mainStatus = _mainStatus;
			if ((mainStatus == MainStatus.Skill1 || mainStatus == MainStatus.Skill3) && _subStatus == SubStatus.Phase3)
			{
				SK1Objs[AtkTime].position = Vector3.MoveTowards(SK1Objs[AtkTime].position, MovePosTmp, SK1MoveDisDelta);
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		IgnoreGravity = true;
		base.SetActive(isActive);
		if (isActive)
		{
			base.AllowAutoAim = false;
			for (int i = 0; i < SK1Collide.Length; i++)
			{
				SK1Collide[i].UpdateBulletData(EnemyWeapons[1].BulletData);
				SK1Collide[i].SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			}
			SetStatus(MainStatus.Debut);
			return;
		}
		if ((bool)EMLHand)
		{
			EMLHand.BackToPool();
		}
		if ((bool)EMRHand)
		{
			EMRHand.BackToPool();
		}
		for (int j = 0; j < SK1Collide.Length; j++)
		{
			if ((bool)SK1Collide[j])
			{
				SK1Collide[j].BackToPool();
			}
		}
	}

	private void SpawnHand()
	{
		if (haveSpawn)
		{
			return;
		}
		MOB_TABLE tMOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[3].BulletData.f_EFFECT_X];
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(tMOB_TABLE, sNetSerialID + "0");
		if ((bool)enemyControllerBase)
		{
			EMLHand = enemyControllerBase.gameObject.GetComponent<EM157_Controller>();
			if ((bool)EMLHand)
			{
				Vector2 vector = _transform.position + Vector3.right * CheckRoomSize(-1) + Vector3.up * 3f;
				EMLHand.SetPositionAndRotation(vector, false);
				EMLHand.SetCenterPos(_transform.position.x);
				EMLHand.gameObject.name = "Left_Hand";
				EMLHand.SetActive(true);
				LHandCorp = new BossCorpsTool();
				LHandCorp.Master = this;
				EMLHand.JoinCorps(LHandCorp);
			}
		}
		EnemyControllerBase enemyControllerBase2 = StageUpdate.StageSpawnEnemyByMob(tMOB_TABLE, sNetSerialID + "1");
		if ((bool)enemyControllerBase2)
		{
			EMRHand = enemyControllerBase2.gameObject.GetComponent<EM157_Controller>();
			if ((bool)EMRHand)
			{
				Vector2 vector2 = _transform.position + Vector3.right * CheckRoomSize(1) + Vector3.up * 3f;
				EMRHand.SetPositionAndRotation(vector2, true);
				EMRHand.SetCenterPos(_transform.position.x);
				EMRHand.gameObject.name = "Right_Hand";
				EMRHand.SetActive(true);
				RHandCorp = new BossCorpsTool();
				RHandCorp.Master = this;
				EMRHand.JoinCorps(RHandCorp);
			}
		}
		haveSpawn = true;
	}

	private float CheckRoomSize(int direct)
	{
		int layerMask = 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer;
		int blockEnemyLayer = ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer;
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y + 5f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right * direct, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.up * direct, 30f, layerMask, _transform);
		if (!raycastHit2D)
		{
			return 8 * direct;
		}
		if (!raycastHit2D2)
		{
			Debug.LogError("沒有偵測到天花板或地板，之後SK1 無法準確判斷位置");
			return raycastHit2D.point.x - _transform.position.x + 0.01f * (float)(-direct);
		}
		switch (direct)
		{
		case 1:
			MaxPos = new Vector3(raycastHit2D.point.x - 1.6f, raycastHit2D2.point.y - 1.6f, 0f);
			break;
		case -1:
			MinPos = new Vector3(raycastHit2D.point.x + 1.6f, raycastHit2D2.point.y + 1.6f, 0f);
			break;
		}
		return raycastHit2D.point.x - _transform.position.x + 1f * (float)(-direct);
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
		pos.z = 10f;
		_transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z);
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

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Die)
		{
			return;
		}
		for (int i = 0; i < SK1Collide.Length; i++)
		{
			if ((bool)SK1FX[i])
			{
				SK1FX[i].BackToPool();
			}
			if ((bool)SK1USEFX[i])
			{
				SK1USEFX[i].BackToPool();
			}
			if ((bool)SK1Collide[i])
			{
				SK1Collide[i].BackToPool();
			}
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CLOSE_FX);
		StageUpdate.SlowStage();
		SetColliderEnable(false);
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			SetStatus(MainStatus.Die);
			return;
		}
		Debug.LogError("此Boss 在此模式不該被擊殺");
		SetStatus(MainStatus.Die);
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
	}

	private bool CheckMissionOverAll()
	{
		if (!LHandCorp.CheckMissionProgress())
		{
			return false;
		}
		if (!RHandCorp.CheckMissionProgress())
		{
			return false;
		}
		return true;
	}

	private bool CheckMissionOver(BossCorpsTool corp)
	{
		return corp.WaitMission;
	}

	public void ReportObjects(object[] cankeepon)
	{
		CanKeepOn = (bool)cankeepon[0];
	}

	public object[] GetValues(object[] param = null)
	{
		return null;
	}
}
