#define RELEASE
using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS098_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Walk = 2,
		Hide = 3,
		Skill_1 = 4,
		Skill_2 = 5,
		Skill_3 = 6,
		Skill_4 = 7,
		Die = 8,
		IdleWaitNet = 9
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
		ANI_IDLE_LOOP = 0,
		ANI_DEBUT = 1,
		ANI_DEFENSE_LOOP = 2,
		ANI_RUN_LOOP = 3,
		ANI_HURT_LOOP = 4,
		ANI_DEAD = 5,
		ANI_JUMP_START = 6,
		ANI_JUMP_TO_FALL = 7,
		ANI_JUMP_LOOP = 8,
		ANI_LANDING = 9,
		ANI_SKILL01_START = 10,
		ANI_SKILL01_LOOP = 11,
		ANI_SKILL01_ATK_JUMP_START = 12,
		ANI_SKILL01_ATK_JUMP_LOOP = 13,
		ANI_SKILL01_ATK_JUMP_TO_FALL = 14,
		ANI_SKILL01_ATK_FALL_LOOP = 15,
		ANI_SKILL01_ATK_LANDING = 16,
		ANI_SKILL02_START = 17,
		ANI_SKILL02_LOOP = 18,
		ANI_SKILL02_ATK_START = 19,
		ANI_SKILL02_ATK_LOOP = 20,
		ANI_SKILL02_END = 21,
		ANI_SKILL03_START = 22,
		ANI_SKILL03_LOOP = 23,
		ANI_SKILL03_END = 24,
		ANI_SKILL01_START_BACK = 25,
		MAX_ANIMATION_ID = 26
	}

	private Transform _shootPoint;

	private Vector3 _shootDirection;

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

	private int[] _animationHash;

	private Transform _tfEnemyCollider;

	private Transform _enemyColliderMax;

	private Collider2D _enemyCollider2D;

	private Collider2D _enemyColliderMax2D;

	private int _gunFireCount;

	private OrangeTimer _summonTimer;

	private int _SummonTime = 25000;

	private Transform _swingTarget;

	private OrangeTimer PredictTimer;

	private SpriteRenderer PredictSpriteRenderer;

	[SerializeField]
	private Sprite _circlePredictSprite;

	[SerializeField]
	private int _WalkSpeed = 2;

	[SerializeField]
	private int _RunSpeed = 10;

	[SerializeField]
	private VInt2 _JupmpSpeed = new VInt2(2, 1000);

	private Vector3 MaxPos;

	private Vector3 MinPos;

	private bool CanUseSkill4;

	[SerializeField]
	[Tooltip("放屁技能CD")]
	private float Skill4Time = 25f;

	private int SKill4Frame;

	[SerializeField]
	[Tooltip("擴散時間間隔")]
	private float Skill4ShootTime = 0.1f;

	[SerializeField]
	[Tooltip("擴散速度間隔")]
	private float Skill4ShootDis = 1.5f;

	private int SKill4ShootFrame;

	[SerializeField]
	[Tooltip("發動次數")]
	private int Skill4UseTime = 2;

	[SerializeField]
	[Tooltip("波數間隔時間")]
	private float Skill4UseInterval = 1f;

	private int UseTime;

	private Vector3 LSkill4Pos;

	private Vector3 RSkill4Pos;

	private bool _bDeadCallResult = true;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		_shootPoint = OrangeBattleUtility.FindChildRecursive(_transform, "ShootPoint", true);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Hat", true);
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
		_tfEnemyCollider = OrangeBattleUtility.FindChildRecursive(ref target, "EnemyCollider", true);
		_enemyColliderMax = OrangeBattleUtility.FindChildRecursive(ref target, "EnemyCollider_Max", true);
		_summonTimer = OrangeTimerManager.GetTimer();
		_animationHash = new int[26];
		_animationHash[0] = Animator.StringToHash("BS038@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS038@debut");
		_animationHash[2] = Animator.StringToHash("BS038@defense_loop");
		_animationHash[3] = Animator.StringToHash("BS038@run_loop");
		_animationHash[4] = Animator.StringToHash("BS038@hurt_loop");
		_animationHash[5] = Animator.StringToHash("BS038@dead");
		_animationHash[6] = Animator.StringToHash("BS038@jump_start");
		_animationHash[7] = Animator.StringToHash("BS038@jump_to_fall");
		_animationHash[8] = Animator.StringToHash("BS038@jump_loop");
		_animationHash[9] = Animator.StringToHash("BS038@landing");
		_animationHash[25] = Animator.StringToHash("BS038@skill_01_start_back");
		_animationHash[10] = Animator.StringToHash("BS038@skill_01_start");
		_animationHash[11] = Animator.StringToHash("BS038@skill_01_loop");
		_animationHash[12] = Animator.StringToHash("BS038@skill_01_atk_jump_start");
		_animationHash[13] = Animator.StringToHash("BS038@skill_01_atk_jump_loop");
		_animationHash[14] = Animator.StringToHash("BS038@skill_01_atk_jump_to_fall");
		_animationHash[15] = Animator.StringToHash("BS038@skill_01_atk_fall_loop");
		_animationHash[16] = Animator.StringToHash("BS038@skill_01_atk_landing");
		_animationHash[17] = Animator.StringToHash("BS038@skill_02_start");
		_animationHash[18] = Animator.StringToHash("BS038@skill_02_loop");
		_animationHash[19] = Animator.StringToHash("BS038@skill_02_atk_start");
		_animationHash[20] = Animator.StringToHash("BS038@skill_02_atk_loop");
		_animationHash[21] = Animator.StringToHash("BS038@skill_02_end");
		_animationHash[22] = Animator.StringToHash("BS038@skill_03_start");
		_animationHash[23] = Animator.StringToHash("BS038@skill_03_loop");
		_animationHash[24] = Animator.StringToHash("BS038@skill_03_end");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget", 4);
	}

	public override void UpdateStatus(int nSet, string sMsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (!string.IsNullOrEmpty(sMsg))
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(sMsg);
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

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (_swingTarget == null)
		{
			PredictTimer = OrangeTimerManager.GetTimer();
			GameObject gameObject = new GameObject("SwingTarget");
			_swingTarget = gameObject.transform;
			PredictSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			PredictSpriteRenderer.sprite = _circlePredictSprite;
			PredictSpriteRenderer.enabled = false;
			CanUseSkill4 = false;
			_enemyCollider2D = _tfEnemyCollider.GetComponent<BoxCollider2D>();
			_enemyColliderMax2D = _enemyColliderMax.GetComponent<BoxCollider2D>();
			SwitchColliderSize(false);
		}
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			_summonTimer.TimerStart();
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_summonTimer.TimerStop();
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
		base.transform.position = pos;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		UpdateAIState();
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			_bDeadCallResult = false;
			_SummonTime = 25000;
		}
		else
		{
			_bDeadCallResult = false;
			_SummonTime = 25000;
		}
	}

	public override void BossIntro(Action cb)
	{
		IntroCallBack = cb;
		_introReady = true;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
			{
				StageUpdate.runPlayers[i].ReStartInvicibleTimer();
			}
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	public override void BackToPool()
	{
		base.BackToPool();
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
		{
			if ((int)StageUpdate.runPlayers[i].Hp <= 0)
			{
				tHurtPassParam.dmg = 0;
				if (AiState != AI_STATE.mob_002)
				{
					IsInvincible = true;
				}
			}
			else
			{
				IsInvincible = false;
			}
		}
		return base.Hurt(tHurtPassParam);
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			mainStatus = MainStatus.Idle;
			break;
		case MainStatus.Idle:
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
				if (Target.UsingVehicle)
				{
					TargetPos = new VInt3(Target._transform.position);
				}
				mainStatus = (MainStatus)OrangeBattleUtility.Random(4, 7);
				if (CanUseSkill4)
				{
					mainStatus = MainStatus.Skill_4;
				}
			}
			else
			{
				mainStatus = MainStatus.Walk;
			}
			break;
		case MainStatus.Walk:
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
				if (Target.UsingVehicle)
				{
					TargetPos = new VInt3(Target._transform.position);
				}
				mainStatus = (MainStatus)OrangeBattleUtility.Random(4, 7);
				if (CanUseSkill4)
				{
					mainStatus = MainStatus.Skill_4;
				}
			}
			break;
		case MainStatus.Skill_1:
			mainStatus = MainStatus.Idle;
			break;
		case MainStatus.Skill_2:
			mainStatus = MainStatus.Idle;
			break;
		case MainStatus.Skill_3:
			mainStatus = MainStatus.Idle;
			break;
		case MainStatus.Skill_4:
			mainStatus = MainStatus.Idle;
			break;
		}
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus);
				_mainStatus = MainStatus.IdleWaitNet;
			}
		}
		else
		{
			SetStatus(mainStatus);
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
		if (_summonTimer.GetMillisecond() > _SummonTime)
		{
			_summonTimer.TimerStart();
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
		}
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				UpdateRandomState();
			}
			break;
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f && _introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_unlockReady)
				{
					SwitchColliderSize(false);
					UpdateRandomState();
				}
				break;
			}
			break;
		case MainStatus.Walk:
			if (_currentFrame > 1f)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					UpdateRandomState();
				}
			}
			break;
		case MainStatus.Skill_1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_velocity.y < 0)
				{
					SetStatus(MainStatus.Skill_1, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_1, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill_1, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					if ((bool)Target)
					{
						TargetPos = Target.Controller.LogicPosition;
						if (Target.UsingVehicle)
						{
							TargetPos = new VInt3(Target._transform.position);
						}
						UpdateDirection();
					}
					else
					{
						UpdateDirection(-base.direction);
					}
					UpdateRandomState();
				}
				else if (_currentFrame > 0.1f && _enemyColliderMax.gameObject.activeSelf)
				{
					SwitchColliderSize(false);
				}
				break;
			}
			break;
		case MainStatus.Skill_2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Controller.Collisions.left || Controller.Collisions.right)
				{
					SetStatus(MainStatus.Skill_2, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_2, SubStatus.Phase3);
					base.SoundSource.ForcePlaySE("BossSE02", "bs106_metgi08");
					base.SoundSource.ForcePlaySE("BossSE02", "bs106_metgi08", 1f);
					base.SoundSource.ForcePlaySE("BossSE02", "bs106_metgi08", 2f);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 3f)
				{
					SetStatus(MainStatus.Skill_2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (!(_currentFrame > 1f))
				{
					break;
				}
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					if (Target.UsingVehicle)
					{
						TargetPos = new VInt3(Target._transform.position);
					}
					UpdateDirection();
				}
				else
				{
					UpdateDirection(-base.direction);
				}
				UpdateRandomState();
				break;
			}
			break;
		case MainStatus.Skill_3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				int num = 3;
				if (EnemyWeapons[num].MagazineRemain > 0f)
				{
					if (_currentFrame - (float)EnemyWeapons[num].BulletData.n_MAGAZINE + EnemyWeapons[num].MagazineRemain > 0.5f)
					{
						_shootDirection = ((base.direction > 0) ? (_shootDirection = Vector3.right) : (_shootDirection = Vector3.left));
						BulletBase.TryShotBullet(EnemyWeapons[num].BulletData, _shootPoint, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						EnemyWeapons[num].MagazineRemain -= 1f;
					}
				}
				else if (_currentFrame - (float)EnemyWeapons[num].BulletData.n_MAGAZINE > 1f)
				{
					SetStatus(MainStatus.Skill_3, SubStatus.Phase2);
				}
				break;
			}
			case SubStatus.Phase2:
				if (!(_currentFrame > 1f))
				{
					break;
				}
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					if (Target.UsingVehicle)
					{
						TargetPos = new VInt3(Target._transform.position);
					}
					UpdateDirection();
				}
				UpdateRandomState();
				break;
			}
			break;
		case MainStatus.Skill_4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 2f)
				{
					base.SoundSource.PlaySE("BossSE02", "bs106_metgi12");
					SetStatus(MainStatus.Skill_4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (GameLogicUpdateManager.GameFrame < SKill4ShootFrame)
				{
					break;
				}
				if (LSkill4Pos.y < MaxPos.y)
				{
					(BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, LSkill4Pos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
				}
				if (RSkill4Pos.y < MaxPos.y)
				{
					(BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, RSkill4Pos, Vector3.zero, null, selfBuffManager.sBuffStatus, EnemyData, targetMask) as CollideBullet).bNeedBackPoolModelName = true;
				}
				if (LSkill4Pos.x - Skill4ShootDis < MinPos.x)
				{
					LSkill4Pos.x = MinPos.x + 0.8f;
					LSkill4Pos += Vector3.up * Skill4ShootDis;
				}
				else
				{
					LSkill4Pos += Vector3.left * Skill4ShootDis;
				}
				if (RSkill4Pos.x + Skill4ShootDis > MaxPos.x)
				{
					RSkill4Pos.x = MaxPos.x - 0.8f;
					RSkill4Pos += Vector3.up * Skill4ShootDis;
				}
				else
				{
					RSkill4Pos += Vector3.right * Skill4ShootDis;
				}
				if (LSkill4Pos.y > MaxPos.y && RSkill4Pos.y > MaxPos.y)
				{
					if (++UseTime < Skill4UseTime)
					{
						SetStatus(MainStatus.Skill_4, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.Skill_4, SubStatus.Phase4);
					}
				}
				else
				{
					SetStatus(MainStatus.Skill_4, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame >= SKill4ShootFrame)
				{
					SetStatus(MainStatus.Skill_4, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase4:
				if (!(_currentFrame > 1f))
				{
					break;
				}
				SKill4Frame = GameLogicUpdateManager.GameFrame + (int)(Skill4Time * 20f);
				CanUseSkill4 = false;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					if (Target.UsingVehicle)
					{
						TargetPos = new VInt3(Target._transform.position);
					}
					UpdateDirection();
				}
				UpdateRandomState();
				break;
			}
			break;
		case MainStatus.Die:
		{
			for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
			{
				StageUpdate.runPlayers[i].ReStartInvicibleTimer();
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.5f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 5f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			}
			break;
		}
		}
		if (!CanUseSkill4 && GameLogicUpdateManager.GameFrame > SKill4Frame)
		{
			CanUseSkill4 = true;
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (mainStatus)
		{
		case MainStatus.Idle:
			_velocity.x = 0;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			break;
		case MainStatus.Debut:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase1)
			{
				CheckRoomSize();
				SKill4Frame = GameLogicUpdateManager.GameFrame + (int)(Skill4Time * 20f);
				CanUseSkill4 = false;
				if (IntroCallBack != null)
				{
					IntroCallBack();
				}
			}
			break;
		}
		case MainStatus.Walk:
			_velocity.x = base.direction * _WalkSpeed * 1000;
			break;
		case MainStatus.Skill_1:
			_gunFireCount = 0;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = 0;
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				break;
			case SubStatus.Phase1:
				PlayBossSE("BossSE02", "bs106_metgi01");
				break;
			case SubStatus.Phase2:
				PlayBossSE("BossSE02", "bs106_metgi02");
				break;
			case SubStatus.Phase3:
				_velocity.x = base.direction * _JupmpSpeed.x * 1000;
				_velocity.y = _JupmpSpeed.y * 1000;
				break;
			case SubStatus.Phase4:
				if ((bool)Target)
				{
					TargetPos = Target.Controller.LogicPosition;
					if (Target.UsingVehicle)
					{
						TargetPos = new VInt3(Target._transform.position);
					}
					UpdateDirection();
				}
				_velocity = VInt3.zero;
				IgnoreGravity = true;
				SwitchColliderSize(true);
				break;
			case SubStatus.Phase5:
			{
				RaycastHit2D raycastHit2D = Physics2D.Raycast(TargetPos.vec3, Vector2.down, 20f, BulletScriptableObject.Instance.BulletLayerMaskObstacle);
				if ((bool)raycastHit2D)
				{
					_swingTarget.position = new Vector3(TargetPos.vec3.x, raycastHit2D.point.y, 0f);
					_swingTarget.localRotation = Quaternion.Euler(90f, 90f, 0f);
				}
				else
				{
					_swingTarget.position = TargetPos.vec3;
					_swingTarget.rotation = Quaternion.Euler(0f, 0f, 0f);
				}
				_swingTarget.localScale = Vector3.one * 2f;
				PredictSpriteRenderer.enabled = true;
				PredictTimer.TimerStart();
				int num = Mathf.RoundToInt((float)Mathf.Abs(TargetPos.x - Controller.LogicPosition.x) * 0.8f);
				_velocity.x = num * Mathf.Abs(_maxGravity.i) / (_JupmpSpeed.y * 2 * 1000);
				_velocity.x = _velocity.x * base.direction * 100;
				IgnoreGravity = false;
				break;
			}
			case SubStatus.Phase6:
				PlayBossSE("BossSE02", "bs106_metgi03");
				_velocity.x = 0;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 2f, false);
				PredictSpriteRenderer.enabled = false;
				break;
			}
			break;
		case MainStatus.Skill_2:
			_gunFireCount = 0;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = 0;
				UpdateDirection();
				_collideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * _RunSpeed * 1000;
				break;
			case SubStatus.Phase2:
				PlayBossSE("BossSE02", "bs106_metgi07");
				_velocity.x = 0;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.SetLockWallJump(2000);
				break;
			}
			break;
		case MainStatus.Skill_3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE02", "bs106_metgi09");
				_velocity.x = 0;
				_gunFireCount++;
				EnemyWeapons[3].MagazineRemain = EnemyWeapons[3].BulletData.n_MAGAZINE;
				break;
			case SubStatus.Phase2:
				_shootDirection = ((base.direction > 0) ? (_shootDirection = Vector3.right) : (_shootDirection = Vector3.left));
				BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, _shootPoint, _shootDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			break;
		case MainStatus.Skill_4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				UseTime = 0;
				_velocity = VInt3.zero;
				LSkill4Pos = _transform.position + Vector3.left + Vector3.up;
				RSkill4Pos = _transform.position + Vector3.right + Vector3.up;
				if (LSkill4Pos.x <= MinPos.x + 2f)
				{
					LSkill4Pos.x = MinPos.x + 2f;
				}
				if (RSkill4Pos.x >= MaxPos.x - 2f)
				{
					RSkill4Pos.x = MaxPos.x - 2f;
				}
				psSwingTarget psSwingTarget2 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", LSkill4Pos, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, Vector2.left)), Array.Empty<object>());
				psSwingTarget psSwingTarget3 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", RSkill4Pos, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, Vector2.right)), Array.Empty<object>());
				psSwingTarget psSwingTarget4 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", MinPos + Vector3.right * 1.5f + Vector3.up * 1f, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, Vector2.up)), Array.Empty<object>());
				psSwingTarget obj = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", new Vector3(MaxPos.x - 1.5f, MinPos.y + 1f, 0f), Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, Vector2.up)), Array.Empty<object>());
				psSwingTarget2.SetEffect(LSkill4Pos.x - MinPos.x, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 1f);
				psSwingTarget3.SetEffect(MaxPos.x - RSkill4Pos.x, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 1f);
				psSwingTarget4.SetEffect(10f, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 1f);
				obj.SetEffect(10f, new Color(0.6f, 0f, 0.5f, 0.7f), new Color(0.6f, 0f, 0.5f), 1f);
				break;
			}
			case SubStatus.Phase2:
				SKill4ShootFrame = GameLogicUpdateManager.GameFrame + (int)(Skill4ShootTime * 20f);
				break;
			case SubStatus.Phase3:
				_velocity = VInt3.zero;
				LSkill4Pos = _transform.position + Vector3.left + Vector3.up;
				RSkill4Pos = _transform.position + Vector3.right + Vector3.up;
				if (LSkill4Pos.x <= MinPos.x + 2f)
				{
					LSkill4Pos.x = MinPos.x + 2f;
				}
				if (RSkill4Pos.x >= MaxPos.x - 2f)
				{
					RSkill4Pos.x = MaxPos.x - 2f;
				}
				SKill4ShootFrame = GameLogicUpdateManager.GameFrame + (int)(Skill4UseInterval * 20f);
				break;
			}
			break;
		case MainStatus.Die:
			PredictSpriteRenderer.enabled = false;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_collideBullet.BackToPool();
				_summonTimer.TimerStop();
				base.AllowAutoAim = false;
				_velocity.x = 0;
				OrangeBattleUtility.LockPlayer();
				if (!Controller.Collisions.below)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				if (_bDeadCallResult)
				{
					StartCoroutine(BossDieFlow(base.AimTransform));
				}
				else
				{
					StartCoroutine(BossDieFlow(base.AimTransform, "FX_BOSS_EXPLODE2", false, false));
				}
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
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE_LOOP;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Debut:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_DEBUT;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0.02f);
			}
			break;
		case MainStatus.Walk:
			_currentAnimationId = AnimationID.ANI_RUN_LOOP;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Skill_1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL01_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL01_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL01_ATK_JUMP_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL01_ATK_JUMP_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL01_ATK_JUMP_TO_FALL;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL01_ATK_FALL_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase6:
				base.SoundSource.PlaySE("BossSE02", "bs106_metgi04", 2.1f);
				_currentAnimationId = AnimationID.ANI_SKILL01_ATK_LANDING;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL02_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL02_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL02_ATK_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL02_ATK_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				base.SoundSource.PlaySE("BossSE02", "bs106_metgi04", 0.87f);
				_currentAnimationId = AnimationID.ANI_SKILL02_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL03_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL03_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL03_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL01_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase2:
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEFENSE_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL01_START_BACK;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEAD;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_HURT_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Hide:
			break;
		}
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else
		{
			int num = Controller.LogicPosition.x - TargetPos.x;
			base.direction = ((num <= 0) ? 1 : (-1));
		}
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	private void SwitchColliderSize(bool max)
	{
		if (max)
		{
			if ((bool)_enemyCollider2D)
			{
				_enemyCollider2D.enabled = false;
			}
			else
			{
				_enemyCollider2D.gameObject.SetActive(false);
			}
			if ((bool)_enemyColliderMax2D)
			{
				_enemyColliderMax2D.enabled = true;
			}
			else
			{
				_enemyColliderMax.gameObject.SetActive(true);
			}
		}
		else
		{
			if ((bool)_enemyCollider2D)
			{
				_enemyCollider2D.enabled = true;
			}
			else
			{
				_enemyCollider2D.gameObject.SetActive(true);
			}
			if ((bool)_enemyColliderMax2D)
			{
				_enemyColliderMax2D.enabled = false;
			}
			else
			{
				_enemyColliderMax.gameObject.SetActive(false);
			}
		}
	}

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		Vector3 vector = new Vector3(_transform.position.x, _transform.position.y + 5f, 0f);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.left, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.right, 30f, layerMask, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(vector, Vector3.down, 30f, layerMask, _transform);
		if (!raycastHit2D3)
		{
			Debug.LogError("沒有偵測到天花板或地板，之後一些技能無法準確判斷位置");
			return;
		}
		MaxPos = new Vector3(raycastHit2D2.point.x, raycastHit2D3.point.y + 13f, 0f);
		MinPos = new Vector3(raycastHit2D.point.x, raycastHit2D3.point.y, 0f);
	}
}
