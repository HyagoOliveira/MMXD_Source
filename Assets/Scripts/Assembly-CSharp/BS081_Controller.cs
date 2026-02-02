#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS081_Controller : EnemyControllerBase, IManagedUpdateBehavior
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
		Die = 7
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
		ANI_SKILL0_START = 2,
		ANI_SKILL0_LOOP = 3,
		ANI_SKILL0_END = 4,
		ANI_SKILL1_START = 5,
		ANI_SKILL1_LOOP = 6,
		ANI_SKILL1_END = 7,
		ANI_SKILL2_START = 8,
		ANI_SKILL2_LOOP = 9,
		ANI_SKILL2_END = 10,
		ANI_SKILL3_START = 11,
		ANI_SKILL3_LOOP = 12,
		ANI_SKILL3_END = 13,
		ANI_HURT = 14,
		ANI_DEAD = 15,
		MAX_ANIMATION_ID = 16
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

	private int[] DefaultSkillCard = new int[4] { 0, 1, 2, 3 };

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private int ShootTimes;

	private float ShootFrame;

	private int ActionFrame;

	private bool HasShot;

	private Vector3 ShotPos;

	private Vector3 EndPos;

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private Vector3 CenterPos;

	private float GroundYPos;

	[SerializeField]
	private Transform _AimTransform;

	[Header("生怪")]
	private bool HaveSpawn;

	[SerializeField]
	private Transform Skill0ShootPos;

	[SerializeField]
	private int SummonEnemyLimit = 12;

	[SerializeField]
	private float Skill0WaitTime = 6f;

	private int SpawnCount;

	private List<Vector3> SpawnPoints = new List<Vector3>();

	private List<int[]> ChoosePointsSet = new List<int[]>();

	private int ChoosePoint;

	[SerializeField]
	private ParticleSystem Skill0FlashFX1;

	[SerializeField]
	private ParticleSystem Skill0FlashFX2;

	[SerializeField]
	private ParticleSystem Skill0FlashFX3;

	private List<EnemyControllerBase> EnemyList = new List<EnemyControllerBase>();

	[Header("水平雷射")]
	[SerializeField]
	private Transform Skill1ShootPos;

	[SerializeField]
	private float S1ill1ShootFrame = 0.1f;

	[SerializeField]
	private ParticleSystem Skill1UseFX;

	[SerializeField]
	private ParticleSystem Skill1DuringFX;

	[SerializeField]
	private float UseFxTime = 1f;

	[SerializeField]
	private float DuringFxStartTime = 0.3f;

	[SerializeField]
	private float DuringFxTime = 3f;

	private CollideBullet Skill1Collide;

	[Header("垂直雷射")]
	[SerializeField]
	private float Skill2ShootInterval = 6f;

	[SerializeField]
	private int Skill2ShootTime = 4;

	[SerializeField]
	private float Skill2ShootFrame = 0.5f;

	[SerializeField]
	private string Skill2PreFxName = "fxduring_HELL_SIGMA_001";

	[SerializeField]
	private ParticleSystem Skill2FlashFX;

	[Header("黑洞彈")]
	[SerializeField]
	private float Skill3ShootInterval = 6f;

	[SerializeField]
	private int SKill3Condition = 3;

	[SerializeField]
	private int Skill3ShootTime = 3;

	[SerializeField]
	private float Skill3ShootFrame = 0.5f;

	[SerializeField]
	private float IdleWaitTime = 0.2f;

	private int IdleWaitFrame;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus NextSkill;

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

	protected virtual void HashAnimation()
	{
		_animationHash = new int[16];
		_animationHash[0] = Animator.StringToHash("BS081@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS081@idle_loop");
		_animationHash[2] = Animator.StringToHash("BS081@skill_1_start");
		_animationHash[3] = Animator.StringToHash("BS081@skill_1_loop");
		_animationHash[4] = Animator.StringToHash("BS081@skill_1_end");
		_animationHash[5] = Animator.StringToHash("BS081@skill_2_start");
		_animationHash[6] = Animator.StringToHash("BS081@skill_2_loop");
		_animationHash[7] = Animator.StringToHash("BS081@skill_2_end");
		_animationHash[8] = Animator.StringToHash("BS081@skill_3_start");
		_animationHash[9] = Animator.StringToHash("BS081@skill_3_loop");
		_animationHash[10] = Animator.StringToHash("BS081@skill_3_end");
		_animationHash[11] = Animator.StringToHash("BS081@skill_4_start");
		_animationHash[12] = Animator.StringToHash("BS081@skill_4_loop");
		_animationHash[13] = Animator.StringToHash("BS081@skill_4_end");
		_animationHash[14] = Animator.StringToHash("BS081@hurt_loop");
		_animationHash[15] = Animator.StringToHash("BS081@dead");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		Skill1Collide = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1Collide", true).gameObject.AddOrGetComponent<CollideBullet>();
		if (Skill0ShootPos == null)
		{
			Skill0ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "FX_Mouth_Pos", true);
		}
		if (Skill1ShootPos == null)
		{
			Skill1ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "Bone013_Weapon", true);
		}
		if (Skill0FlashFX1 == null)
		{
			Skill0FlashFX1 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0FlashFX1", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill0FlashFX2 == null)
		{
			Skill0FlashFX2 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0FlashFX2", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill0FlashFX3 == null)
		{
			Skill0FlashFX3 = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill0FlashFX3", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1UseFX == null)
		{
			Skill1UseFX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1UseFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill1DuringFX == null)
		{
			Skill1DuringFX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill1DuringFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (Skill2FlashFX == null)
		{
			Skill2FlashFX = OrangeBattleUtility.FindChildRecursive(ref childs, "Skill2FlashFX", true).gameObject.AddOrGetComponent<ParticleSystem>();
		}
		if (_AimTransform == null)
		{
			_AimTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "AimTransform", true);
		}
		base.AimTransform = _AimTransform.transform;
		base.AimPoint = new Vector3(0f, 0f, 0f);
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
		FallDownSE = new string[2] { "BossSE03", "bs030_via02" };
	}

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(Skill2PreFxName, 3);
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
			if (nSet == 2)
			{
				ChoosePoint = netSyncData.nParam0;
				if (ChoosePoint < 0 || ChoosePoint > 3)
				{
					ChoosePoint = 0;
					Debug.LogError("生怪群組數字有誤，收到的數字是 " + netSyncData.nParam0);
				}
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
				EndPos = GetTargetPos();
				_velocity = VInt3.zero;
				SwitchFx(Skill0FlashFX1, true);
				SwitchFx(Skill0FlashFX2, true);
				SwitchFx(Skill0FlashFX3, true);
				ShootTimes = ChoosePointsSet[ChoosePoint].Length;
				break;
			case SubStatus.Phase1:
			{
				int num = ChoosePointsSet[ChoosePoint].Length - ShootTimes;
				int num2 = ChoosePointsSet[ChoosePoint][num];
				EndPos = SpawnPoints[num2];
				BS082_SummonBullet bS082_SummonBullet = BulletBase.TryShotBullet(EnemyWeapons[8].BulletData, Skill0ShootPos, EndPos - Skill0ShootPos.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as BS082_SummonBullet;
				bS082_SummonBullet.FreeDISTANCE = Vector3.Distance(Skill0ShootPos.position, EndPos) - 0.3f;
				switch (num2)
				{
				case 0:
				case 1:
				case 2:
				case 3:
					bS082_SummonBullet.SummonID = 3;
					break;
				case 4:
				case 5:
					bS082_SummonBullet.SummonID = 1;
					break;
				case 6:
				case 7:
				case 8:
					bS082_SummonBullet.SummonID = 2;
					break;
				default:
					Debug.Log("不該進來這裡，發現此Log請通知Hank");
					break;
				}
				bS082_SummonBullet.BackCallback = SpawnEnemy;
				break;
			}
			case SubStatus.Phase2:
				HaveSpawn = true;
				SwitchFx(Skill0FlashFX1, false);
				SwitchFx(Skill0FlashFX2, false);
				SwitchFx(Skill0FlashFX3, false);
				break;
			case SubStatus.Phase3:
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(Skill0WaitTime * 20f);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				HaveSpawn = false;
				base.SoundSource.PlaySE("BossSE04", "bs039_hellsig10", 0.8f);
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE04", "bs039_hellsig07_lp");
				SwitchFx(Skill1UseFX, true);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(UseFxTime * 20f);
				break;
			case SubStatus.Phase2:
				PlaySE("BossSE04", "bs039_hellsig07_stop");
				SwitchFx(Skill1DuringFX, true);
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(DuringFxStartTime * 20f);
				break;
			case SubStatus.Phase3:
			{
				Skill1Collide._transform.eulerAngles = Vector3.zero;
				Skill1Collide._transform.localPosition = Vector3.zero;
				Skill1Collide.Active(targetMask);
				for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
				{
					EnemyControllerBase mEnemy = StageUpdate.runEnemys[i].mEnemy;
					if ((object)mEnemy == null)
					{
						continue;
					}
					EM160_Controller eM160_Controller;
					if ((object)(eM160_Controller = mEnemy as EM160_Controller) == null)
					{
						EM161_Controller eM161_Controller;
						if ((object)(eM161_Controller = mEnemy as EM161_Controller) == null)
						{
							EM162_Controller eM162_Controller;
							if ((object)(eM162_Controller = mEnemy as EM162_Controller) != null)
							{
								EM162_Controller item = eM162_Controller;
								EnemyList.Add(item);
							}
						}
						else
						{
							EM161_Controller item2 = eM161_Controller;
							EnemyList.Add(item2);
						}
					}
					else
					{
						EM160_Controller item3 = eM160_Controller;
						EnemyList.Add(item3);
					}
				}
				EnemySetDie();
				ActionFrame = GameLogicUpdateManager.GameFrame + (int)(DuringFxTime * 20f);
				break;
			}
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				HaveSpawn = false;
				ShootTimes = Skill2ShootTime;
				PlaySE("BossSE04", "bs039_hellsig11");
				break;
			case SubStatus.Phase1:
				HasShot = false;
				ShootFrame = Skill2ShootFrame;
				SwitchFx(Skill2FlashFX, true);
				EndPos = GetTargetPos();
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(Skill2PreFxName, new Vector3(EndPos.x, GroundYPos - 0.5f, 0f), Quaternion.identity, new object[1] { Vector3.one });
				break;
			case SubStatus.Phase2:
				SwitchFx(Skill2FlashFX, false);
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				HaveSpawn = false;
				ShootTimes = Skill3ShootTime;
				break;
			case SubStatus.Phase1:
				HasShot = false;
				ShootFrame = Skill3ShootFrame;
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
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_IDLE;
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
			case SubStatus.Phase2:
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP;
				break;
			case SubStatus.Phase4:
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
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL3_END;
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
			{
				int num = 0;
				for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
				{
					EnemyControllerBase mEnemy = StageUpdate.runEnemys[i].mEnemy;
					EM160_Controller eM160_Controller;
					EM161_Controller eM161_Controller;
					EM162_Controller eM162_Controller;
					if ((object)mEnemy != null && ((object)(eM160_Controller = mEnemy as EM160_Controller) != null || (object)(eM161_Controller = mEnemy as EM161_Controller) != null || (object)(eM162_Controller = mEnemy as EM162_Controller) != null))
					{
						num++;
					}
				}
				if (!HaveSpawn)
				{
					if (num > SummonEnemyLimit / 2 - 1)
					{
						HaveSpawn = true;
					}
					else
					{
						mainStatus = MainStatus.Skill0;
					}
				}
				if (HaveSpawn)
				{
					mainStatus = ((num < SKill3Condition) ? ((!Target || !(Target._transform.position.x > CenterPos.x - 1f)) ? MainStatus.Skill1 : MainStatus.Skill2) : MainStatus.Skill3);
				}
				break;
			}
			}
		}
		if (DebugMode)
		{
			mainStatus = NextSkill;
		}
		if (mainStatus == MainStatus.Skill0)
		{
			new List<Vector3>(SpawnPoints.ToArray());
			ChoosePoint = RandomCard(0);
		}
		switch (mainStatus)
		{
		case MainStatus.Skill0:
			if (CheckHost())
			{
				UploadEnemyStatus((int)mainStatus, false, new object[1] { ChoosePoint });
			}
			break;
		default:
			if (CheckHost())
			{
				UploadEnemyStatus((int)mainStatus);
			}
			break;
		case MainStatus.Idle:
			break;
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
			if (!bWaitNetStatus && IdleWaitFrame < GameLogicUpdateManager.GameFrame)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
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
				if (_currentFrame > 1f)
				{
					if (--ShootTimes > 0)
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
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
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
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame > ActionFrame)
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
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					(BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, new Vector3(EndPos.x, GroundYPos - 0.5f, 0f), Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
					for (int i = 0; i < StageUpdate.runEnemys.Count; i++)
					{
						EnemyControllerBase mEnemy = StageUpdate.runEnemys[i].mEnemy;
						if ((object)mEnemy == null)
						{
							continue;
						}
						EM160_Controller eM160_Controller;
						if ((object)(eM160_Controller = mEnemy as EM160_Controller) == null)
						{
							EM161_Controller eM161_Controller;
							if ((object)(eM161_Controller = mEnemy as EM161_Controller) == null)
							{
								EM162_Controller eM162_Controller;
								if ((object)(eM162_Controller = mEnemy as EM162_Controller) != null)
								{
									EM162_Controller eM162_Controller2 = eM162_Controller;
									if (Mathf.Abs(eM162_Controller2._transform.position.x - EndPos.x) < 2f)
									{
										EnemyList.Add(eM162_Controller2);
									}
								}
							}
							else
							{
								EM161_Controller eM161_Controller2 = eM161_Controller;
								if (Mathf.Abs(eM161_Controller2._transform.position.x - EndPos.x) < 2f)
								{
									EnemyList.Add(eM161_Controller2);
								}
							}
						}
						else
						{
							EM160_Controller eM160_Controller2 = eM160_Controller;
							if (Mathf.Abs(eM160_Controller2._transform.position.x - EndPos.x) < 2f)
							{
								EnemyList.Add(eM160_Controller2);
							}
						}
					}
					EnemySetDie();
				}
				if (_currentFrame > Skill2ShootInterval)
				{
					if (--ShootTimes > 0)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase2);
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
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					EndPos = GetTargetPos();
					BulletBase.TryShotBullet(EnemyWeapons[6].BulletData, Skill1ShootPos, EndPos - Skill1ShootPos.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask).BackCallback = ShootSubBullet;
				}
				if (_currentFrame > Skill3ShootInterval)
				{
					if (--ShootTimes > 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase2);
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
		case MainStatus.Skill4:
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
			HaveSpawn = false;
			CheckRoomSize();
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			Skill1Collide.UpdateBulletData(EnemyWeapons[4].BulletData);
			Skill1Collide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_collideBullet.BackToPool();
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
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			EnemySetDie();
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_002)
			{
				SetStatus(MainStatus.Die);
				return;
			}
			Debug.LogError("此Boss 在此模式不該被擊殺");
			SetStatus(MainStatus.Die);
		}
	}

	private void SpawnEnemy(object obj)
	{
		if ((int)Hp <= 0)
		{
			return;
		}
		BS082_SummonBullet bS082_SummonBullet = null;
		if (obj != null)
		{
			bS082_SummonBullet = obj as BS082_SummonBullet;
		}
		if (bS082_SummonBullet == null)
		{
			Debug.LogError("子彈資料有誤。");
			return;
		}
		int summonID = bS082_SummonBullet.SummonID;
		Vector3 position = bS082_SummonBullet._transform.position;
		MOB_TABLE enemy = GetEnemy((int)EnemyWeapons[summonID].BulletData.f_EFFECT_X);
		if (enemy == null)
		{
			Debug.LogError("要生成的怪物資料有誤，生怪技能ID " + summonID + " 怪物GroupID " + EnemyWeapons[summonID].BulletData.f_EFFECT_X);
			return;
		}
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(enemy, sNetSerialID + SpawnCount);
		SpawnCount++;
		if ((bool)enemyControllerBase)
		{
			enemyControllerBase.SetPositionAndRotation(position, base.direction == 1);
			enemyControllerBase.SetActive(true);
		}
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

	private Vector3 GetTargetPos(bool realcenter = true)
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
				TargetPos = new VInt3(Target.GetTargetPoint());
			}
			return TargetPos.vec3;
		}
		return _transform.position + Vector3.right * 3f * base.direction;
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x - 4f, _transform.position.y + 5f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.down, 30f, layerMask, _transform);
		if (!raycastHit2D3)
		{
			Debug.LogError("沒有偵測到地板，之後一些技能無法準確判斷位置");
			MaxPos = new Vector3(_transform.position.x - 8f, _transform.position.y + 8f - Controller.Collider2D.size.y, 0f);
			MinPos = new Vector3(_transform.position.x + 8f, _transform.position.y, 0f);
			GroundYPos = _transform.position.y;
			CenterPos = (MaxPos + MinPos) / 2f;
		}
		else
		{
			MaxPos = new Vector3(raycastHit2D2.point.x, raycastHit2D3.point.y + 7f);
			MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D3.point.y);
			GroundYPos = raycastHit2D3.point.y;
			CenterPos = (MaxPos + MinPos) / 2f;
			SetSpawnPoints();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
	}

	private void SetSpawnPoints()
	{
		Vector3[] collection = new Vector3[9]
		{
			new Vector3(MinPos.x + 1f, GroundYPos + 1.5f, 0f),
			new Vector3(MinPos.x + 4f, GroundYPos + 1.5f, 0f),
			new Vector3(MinPos.x + 7f, GroundYPos + 1.5f, 0f),
			new Vector3(MinPos.x + 10f, GroundYPos + 1.5f, 0f),
			new Vector3(CenterPos.x + 2f, CenterPos.y, 0f),
			new Vector3(CenterPos.x - 2f, CenterPos.y, 0f),
			new Vector3(MinPos.x + 2f, MaxPos.y - 3f, 0f),
			new Vector3(MinPos.x + 5f, MaxPos.y - 3f, 0f),
			new Vector3(MinPos.x + 8f, MaxPos.y - 3f, 0f)
		};
		SpawnPoints = new List<Vector3>(collection);
		ChoosePointsSet = new List<int[]>
		{
			new int[6] { 0, 2, 4, 5, 6, 8 },
			new int[6] { 0, 1, 2, 6, 7, 8 },
			new int[6] { 0, 1, 2, 3, 4, 5 },
			new int[6] { 0, 1, 2, 3, 6, 8 }
		};
	}

	private void SwitchFx(ParticleSystem Fx, bool onoff)
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
			Debug.Log(string.Concat("特效載入有誤，目前狀態是 ", _mainStatus, "的階段 ", _subStatus));
		}
	}

	private void ShootSubBullet(object obj)
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
		BulletBase.TryShotBullet(EnemyWeapons[7].BulletData, basicBullet._transform, Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
		BulletBase.TryShotBullet(EnemyWeapons[7].BulletData, basicBullet._transform, Vector3.left, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
	}

	private void EnemySetDie()
	{
		for (int i = 0; i < EnemyList.Count; i++)
		{
			EnemyControllerBase enemyControllerBase = EnemyList[i];
			if ((object)enemyControllerBase == null)
			{
				continue;
			}
			EM160_Controller eM160_Controller;
			if ((object)(eM160_Controller = enemyControllerBase as EM160_Controller) == null)
			{
				EM161_Controller eM161_Controller;
				if ((object)(eM161_Controller = enemyControllerBase as EM161_Controller) == null)
				{
					EM162_Controller eM162_Controller;
					if ((object)(eM162_Controller = enemyControllerBase as EM162_Controller) != null)
					{
						eM162_Controller.SetDie();
					}
				}
				else
				{
					eM161_Controller.SetDie();
				}
			}
			else
			{
				eM160_Controller.SetDie();
			}
		}
		EnemyList.Clear();
	}
}
