using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS047_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		FindPlayer = 2,
		Skill_01 = 3,
		Skill_02 = 4,
		Skill_03 = 5,
		Skill_04 = 6,
		NextAction = 7,
		ShieldOn = 8,
		ShieldOff = 9,
		Die = 10,
		IdleWaitNet = 11,
		IdleChip = 12
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
		ANI_SHIELD_ON_BACK_IDLE_LOOP = 0,
		ANI_SHIELD_ON_BACK_DEBUT_LOOP = 1,
		ANI_SHIELD_ON_BACK_DEBUT_END = 2,
		ANI_SHIELD_ON_BACK_DEAD_LOOP = 3,
		ANI_SHIELD_ON_BACK_HURT_LOOP = 4,
		ANI_SHIELD_ON_BACK_RUN_LOOP = 5,
		ANI_SHIELD_ON_BACK_SKILL_01_JUMP_START = 6,
		ANI_SHIELD_ON_BACK_SKILL_01_JUMP_LOOP = 7,
		ANI_SHIELD_ON_BACK_SKILL_01_ATK_START = 8,
		ANI_SHIELD_ON_BACK_SKILL_01_ATK_LOOP = 9,
		ANI_SHIELD_ON_BACK_SKILL_01_ATK_END = 10,
		ANI_SHIELD_ON_BACK_SKILL_01_JUMP_TO_FALL = 11,
		ANI_SHIELD_ON_BACK_SKILL_01_FALL_LOOP = 12,
		ANI_SHIELD_ON_BACK_SKILL_01_LANDING = 13,
		ANI_SHIELD_ON_BACK_SKILL_04_START = 14,
		ANI_SHIELD_ON_BACK_SKILL_04_LOOP = 15,
		ANI_SHIELD_ON_BACK_SKILL_04_END = 16,
		ANI_SHIELD_ON_BACK_SKILL_06_START = 17,
		ANI_SHIELD_ON_BACK_SKILL_06_LOOP = 18,
		ANI_SHIELD_ON_BACK_SKILL_06_END = 19,
		ANI_SHIELD_ON_BACK_TAKEN_ON = 20,
		ANI_SHIELD_ON_HAND_TAKEN_OFF = 21,
		ANI_SHIELD_ON_HAND_IDLE_LOOP = 22,
		ANI_SHIELD_ON_HAND_DEAD_LOOP = 23,
		ANI_SHIELD_ON_HAND_HURT_LOOP = 24,
		ANI_SHIELD_ON_HAND_RUN_LOOP = 25,
		ANI_SHIELD_ON_HAND_SKILL_02_JUMP_START = 26,
		ANI_SHIELD_ON_HAND_SKILL_02_JUMP_LOOP = 27,
		ANI_SHIELD_ON_HAND_SKILL_02_ATK_START = 28,
		ANI_SHIELD_ON_HAND_SKILL_02_ATK_LOOP = 29,
		ANI_SHIELD_ON_HAND_SKILL_02_ATK_END = 30,
		ANI_SHIELD_ON_HAND_SKILL_02_JUMP_TO_FALL = 31,
		ANI_SHIELD_ON_HAND_SKILL_02_FALL_LOOP = 32,
		ANI_SHIELD_ON_HAND_SKILL_02_LANDING = 33,
		ANI_SHIELD_ON_HAND_SKILL_03_START = 34,
		ANI_SHIELD_ON_HAND_SKILL_03_LOOP = 35,
		ANI_SHIELD_ON_HAND_SKILL_03_END = 36,
		ANI_SHIELD_ON_HAND_SKILL_05_START = 37,
		ANI_SHIELD_ON_HAND_SKILL_05_LOOP = 38,
		ANI_SHIELD_ON_HAND_SKILL_05_END = 39,
		ANI_SHIELD_ON_HAND_SKILL_07_START = 40,
		ANI_SHIELD_ON_HAND_SKILL_07_LOOP = 41,
		ANI_SHIELD_ON_HAND_SKILL_07_END = 42,
		MAX_ANIMATION_ID = 43
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

	private int[] _animationHash;

	private Transform _tfSwordPoint;

	private Transform _tfShieldMesh;

	[SerializeField]
	private int _runSpeed = 4;

	[SerializeField]
	private int _dashSpeed = 10;

	private VInt2 _jupmpSpeed = new VInt2(0, 13);

	private bool _checkUseSkill01;

	private ParticleSystem _efx_Skill01_Start;

	private ParticleSystem _efx_Skill01_Dash;

	private ParticleSystem _eft_Skill03_Ring;

	private ParticleSystem _efx_Skill04_Start;

	private ParticleSystem _efx_Skill04_Dash;

	private bool _shootBullet;

	private VajurilaFFRingBullet _ringBullet;

	private MainStatus _lateAction;

	private bool _takeShield;

	private int _comboIndex = -1;

	private MainStatus[] _combos = new MainStatus[4]
	{
		MainStatus.Skill_03,
		MainStatus.Skill_03,
		MainStatus.Skill_01,
		MainStatus.Skill_01
	};

	[SerializeField]
	private MainStatus _deubgNextAI = MainStatus.Debut;

	private OrangeTimer _summonTimer;

	private bool _bDeadCallResult = true;

	private List<MainStatus> _combolist = new List<MainStatus>();

	private bool HoldShield;

	private int nDeadCount;

	[SerializeField]
	private Transform _shieldTransform;

	private bool IsChipInfoAnim;

	private bool bPlay04;

	public override void SetChipInfoAnim()
	{
		if (!_takeShield)
		{
			_tfShieldMesh.gameObject.SetActive(false);
			_shieldTransform.gameObject.SetActive(false);
		}
		else
		{
			_tfShieldMesh.gameObject.SetActive(true);
			_shieldTransform.gameObject.SetActive(true);
		}
		SetStatus(MainStatus.IdleChip);
		IsChipInfoAnim = true;
		UpdateAnimation();
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

	private void HashAnimation()
	{
		_animationHash = new int[43];
		_animationHash[1] = Animator.StringToHash("BS047@shield_on_back_debut_loop");
		_animationHash[2] = Animator.StringToHash("BS047@shield_on_back_debut_end");
		_animationHash[0] = Animator.StringToHash("BS047@shield_on_back_idle_loop");
		_animationHash[3] = Animator.StringToHash("BS047@shield_on_back_dead_loop");
		_animationHash[4] = Animator.StringToHash("BS047@shield_on_back_hurt_loop");
		_animationHash[5] = Animator.StringToHash("BS047@shield_on_back_run_loop");
		_animationHash[6] = Animator.StringToHash("BS047@shield_on_back_skill_01_jump_start");
		_animationHash[7] = Animator.StringToHash("BS047@shield_on_back_skill_01_jump_loop");
		_animationHash[8] = Animator.StringToHash("BS047@shield_on_back_skill_01_atk_start");
		_animationHash[9] = Animator.StringToHash("BS047@shield_on_back_skill_01_atk_loop");
		_animationHash[10] = Animator.StringToHash("BS047@shield_on_back_skill_01_atk_end");
		_animationHash[11] = Animator.StringToHash("BS047@shield_on_back_skill_01_jump_to_fall");
		_animationHash[12] = Animator.StringToHash("BS047@shield_on_back_skill_01_fall_loop");
		_animationHash[13] = Animator.StringToHash("BS047@shield_on_back_skill_01_landing");
		_animationHash[14] = Animator.StringToHash("BS047@shield_on_back_skill_04_start");
		_animationHash[15] = Animator.StringToHash("BS047@shield_on_back_skill_04_loop");
		_animationHash[16] = Animator.StringToHash("BS047@shield_on_back_skill_04_end");
		_animationHash[17] = Animator.StringToHash("BS047@shield_on_back_skill_06_start");
		_animationHash[18] = Animator.StringToHash("BS047@shield_on_back_skill_06_loop");
		_animationHash[19] = Animator.StringToHash("BS047@shield_on_back_skill_06_end");
		_animationHash[22] = Animator.StringToHash("BS047@shield_on_hand_idle_loop");
		_animationHash[20] = Animator.StringToHash("BS047@shield_on_back_taken_on");
		_animationHash[21] = Animator.StringToHash("BS047@shield_on_hand_taken_off");
		_animationHash[23] = Animator.StringToHash("BS047@shield_on_hand_dead_loop");
		_animationHash[24] = Animator.StringToHash("BS047@shield_on_hand_hurt_loop");
		_animationHash[25] = Animator.StringToHash("BS047@shield_on_hand_run_loop");
		_animationHash[26] = Animator.StringToHash("BS047@shield_on_hand_skill_02_jump_start");
		_animationHash[27] = Animator.StringToHash("BS047@shield_on_hand_skill_02_jump_loop");
		_animationHash[28] = Animator.StringToHash("BS047@shield_on_hand_skill_02_atk_start");
		_animationHash[29] = Animator.StringToHash("BS047@shield_on_hand_skill_02_atk_loop");
		_animationHash[30] = Animator.StringToHash("BS047@shield_on_hand_skill_02_atk_end");
		_animationHash[31] = Animator.StringToHash("BS047@shield_on_hand_skill_02_jump_to_fall");
		_animationHash[32] = Animator.StringToHash("BS047@shield_on_hand_skill_02_fall_loop");
		_animationHash[33] = Animator.StringToHash("BS047@shield_on_hand_skill_02_landing");
		_animationHash[34] = Animator.StringToHash("BS047@shield_on_hand_skill_08_start");
		_animationHash[35] = Animator.StringToHash("BS047@shield_on_hand_skill_08_loop");
		_animationHash[36] = Animator.StringToHash("BS047@shield_on_hand_skill_08_end");
		_animationHash[37] = Animator.StringToHash("BS047@shield_on_hand_skill_05_start");
		_animationHash[38] = Animator.StringToHash("BS047@shield_on_hand_skill_05_loop");
		_animationHash[39] = Animator.StringToHash("BS047@shield_on_hand_skill_05_end");
		_animationHash[40] = Animator.StringToHash("BS047@shield_on_hand_skill_07_start");
		_animationHash[41] = Animator.StringToHash("BS047@shield_on_hand_skill_07_loop");
		_animationHash[42] = Animator.StringToHash("BS047@shield_on_hand_skill_07_end");
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Bip Spine").gameObject.AddOrGetComponent<CollideBullet>();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		base.AimPoint = new Vector3(0f, 1f, 0f);
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
		_tfSwordPoint = OrangeBattleUtility.FindChildRecursive(ref target, "SwordPoint", true);
		_tfShieldMesh = OrangeBattleUtility.FindChildRecursive(ref target, "BS047_ShieldMesh_G", true);
		if (!_shieldTransform)
		{
			_shieldTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShieldCollider", true);
		}
		if ((bool)_shieldTransform)
		{
			_shieldTransform.gameObject.AddOrGetComponent<StageObjParam>().nSubPartID = 1;
			GuardTransform.Add(1);
		}
		_efx_Skill01_Start = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_vajurila_ff_000", true).GetComponentInChildren<ParticleSystem>();
		_efx_Skill01_Dash = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_vajurila_ff_000", true).GetComponentInChildren<ParticleSystem>();
		_eft_Skill03_Ring = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_vajurila_ff_002", true).GetComponentInChildren<ParticleSystem>();
		_efx_Skill04_Start = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_vajurila_ff_001", true).GetComponentInChildren<ParticleSystem>();
		_efx_Skill04_Dash = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_vajurila_ff_001", true).GetComponentInChildren<ParticleSystem>();
		_eft_Skill03_Ring.Stop();
		HashAnimation();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		_summonTimer = OrangeTimerManager.GetTimer();
		_mainStatus = MainStatus.Idle;
		_subStatus = SubStatus.Phase0;
		AiTimer.TimerStart();
		base.DeadPlayCompleted = false;
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

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		bool flag = false;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			mainStatus = MainStatus.Idle;
			break;
		case MainStatus.Idle:
		case MainStatus.NextAction:
			if ((float)(int)Hp > (float)(int)MaxHp * 0.5f && !HoldShield)
			{
				flag = true;
			}
			else
			{
				bool flag2 = true;
				if (_comboIndex >= 0 && _comboIndex + 1 < _combos.Length)
				{
					_comboIndex++;
					mainStatus = _combos[_comboIndex];
					flag2 = mainStatus == MainStatus.Idle;
				}
				if (flag2)
				{
					int num = OrangeBattleUtility.Random(0, 100);
					switch (AiState)
					{
					case AI_STATE.mob_002:
						if (num < 20)
						{
							_comboIndex = 0;
							_combos[2] = MainStatus.Skill_01;
							_combos[3] = MainStatus.Idle;
							mainStatus = _combos[_comboIndex];
						}
						else if (num < 40)
						{
							_comboIndex = 0;
							_combos[2] = MainStatus.Skill_02;
							_combos[3] = MainStatus.Skill_02;
							mainStatus = _combos[_comboIndex];
						}
						else
						{
							flag = true;
						}
						break;
					case AI_STATE.mob_003:
					case AI_STATE.mob_004:
					case AI_STATE.mob_005:
						if (num < 20)
						{
							MainStatus item = (_takeShield ? MainStatus.ShieldOff : MainStatus.ShieldOn);
							int num2 = OrangeBattleUtility.Random(0, 20);
							_comboIndex = 0;
							_combolist = new List<MainStatus>
							{
								MainStatus.Skill_03,
								MainStatus.Skill_03,
								MainStatus.Skill_01,
								item,
								MainStatus.Idle
							};
							_combos = _combolist.ToArray();
							if (_takeShield)
							{
								_combos[2] = ((num2 / 10 == 0) ? MainStatus.Skill_01 : MainStatus.Skill_04);
							}
							else
							{
								_combos[2] = MainStatus.Skill_01;
							}
							mainStatus = _combos[_comboIndex];
						}
						else if (num < 40)
						{
							MainStatus item2 = (_takeShield ? MainStatus.ShieldOff : MainStatus.ShieldOn);
							_comboIndex = 0;
							_combolist = new List<MainStatus>
							{
								MainStatus.Skill_03,
								MainStatus.Skill_03,
								MainStatus.Skill_02,
								MainStatus.Skill_02,
								item2
							};
							_combos = _combolist.ToArray();
							mainStatus = _combos[_comboIndex];
						}
						else if (num < 60 && (float)(int)Hp < (float)(int)MaxHp * 0.5f)
						{
							MainStatus item3 = (_takeShield ? MainStatus.ShieldOff : MainStatus.ShieldOn);
							_comboIndex = 0;
							_combolist = new List<MainStatus>
							{
								MainStatus.Skill_03,
								MainStatus.Skill_03,
								MainStatus.Skill_02,
								MainStatus.Skill_02,
								MainStatus.Skill_04,
								MainStatus.Skill_04,
								item3
							};
							_combos = _combolist.ToArray();
							if (!_takeShield)
							{
								_combos[4] = MainStatus.Skill_01;
								_combos[5] = MainStatus.Skill_01;
							}
							mainStatus = _combos[_comboIndex];
						}
						else
						{
							flag = true;
						}
						break;
					default:
						if (num < 25)
						{
							_comboIndex = 0;
							_combos[2] = MainStatus.Skill_01;
							_combos[3] = MainStatus.Skill_01;
							mainStatus = _combos[_comboIndex];
						}
						else if (num < 50)
						{
							_comboIndex = 0;
							_combos[2] = MainStatus.Skill_02;
							_combos[3] = MainStatus.Skill_02;
							mainStatus = _combos[_comboIndex];
						}
						else
						{
							flag = true;
						}
						break;
					}
				}
			}
			if (flag)
			{
				AI_STATE aiState = AiState;
				mainStatus = (MainStatus)(((uint)(aiState - 2) > 2u) ? ((_lateAction != MainStatus.Skill_01) ? OrangeBattleUtility.Random(3, 6) : OrangeBattleUtility.Random(4, 6)) : ((_lateAction != MainStatus.Skill_01 && _lateAction != MainStatus.Skill_04) ? ((!_takeShield) ? OrangeBattleUtility.Random(3, 6) : OrangeBattleUtility.Random(3, 7)) : OrangeBattleUtility.Random(4, 6)));
			}
			break;
		case MainStatus.FindPlayer:
			mainStatus = MainStatus.Skill_01;
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
		if (IsChipInfoAnim)
		{
			return;
		}
		if (_mainStatus == MainStatus.Debut)
		{
			BaseUpdate();
			UpdateGravity();
			Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
			base.FalldownUpdate();
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
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					if (HoldShield)
					{
						base.SoundSource.PlaySE("BossSE02", "bs012_vajur12", 0.4f);
						SetStatus(MainStatus.Debut, SubStatus.Phase3);
					}
					else if (_introReady)
					{
						SetStatus(MainStatus.Debut, SubStatus.Phase2);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f && _introReady)
				{
					UpdateRandomState();
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f && _introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			}
			break;
		case MainStatus.FindPlayer:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				_enemyAutoAimSystem.UpdateAimRange(20f);
				TargetPos = Target.Controller.LogicPosition;
				UpdateDirection();
				UpdateRandomState();
			}
			else if (AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
			{
				UpdateRandomState();
			}
			break;
		case MainStatus.Idle:
			if (AiTimer.GetMillisecond() > EnemyData.n_AI_TIMER)
			{
				SetStatus(MainStatus.NextAction);
			}
			break;
		case MainStatus.NextAction:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				TargetPos = Target.Controller.LogicPosition;
				UpdateDirection();
				UpdateRandomState();
			}
			else
			{
				SetStatus(MainStatus.FindPlayer);
			}
			break;
		case MainStatus.Skill_01:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_01, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					_checkUseSkill01 = true;
					if (_velocity.y <= 7000)
					{
						SetStatus(MainStatus.Skill_01, SubStatus.Phase4);
					}
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_01, SubStatus.Phase3);
					if (!bPlay04)
					{
						PlaySE("BossSE02", "bs012_vajur04_lp");
						bPlay04 = true;
					}
				}
				break;
			case SubStatus.Phase3:
				if (Controller.Collisions.left || Controller.Collisions.right)
				{
					SetStatus(MainStatus.Skill_01, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_01, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (Controller.Collisions.below)
				{
					if (_checkUseSkill01)
					{
						SetStatus(MainStatus.Skill_01, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Skill_01, SubStatus.Phase6);
					}
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.NextAction);
				}
				break;
			}
			break;
		case MainStatus.Skill_02:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_02, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				float num = 0.35f;
				if (_takeShield)
				{
					num = 0.3f;
				}
				if (!_shootBullet && _currentFrame > num)
				{
					_shootBullet = true;
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, _tfSwordPoint.position, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.NextAction);
				}
				break;
			}
			}
			break;
		case MainStatus.Skill_03:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_03, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((bool)_ringBullet && _currentFrame > 0.35f)
				{
					_eft_Skill03_Ring.Stop();
					_ringBullet.Shoot();
					_ringBullet = null;
				}
				else if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.NextAction);
				}
				break;
			}
			break;
		case MainStatus.Skill_04:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill_04, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && (Controller.Collisions.left || Controller.Collisions.right))
				{
					SetStatus(MainStatus.Skill_04, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.NextAction);
				}
				break;
			}
			break;
		case MainStatus.ShieldOn:
		case MainStatus.ShieldOff:
			if (_currentFrame > 1f)
			{
				SetStatus(MainStatus.NextAction);
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.5f)
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
			case SubStatus.Phase2:
				if (_currentFrame > 5f)
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
			}
			break;
		case MainStatus.IdleWaitNet:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (!IsChipInfoAnim && (Activate || _mainStatus == MainStatus.Debut))
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			if (_mainStatus == MainStatus.Skill_01 && _checkUseSkill01 && (bool)Target && (double)Mathf.Abs(Controller.LogicPosition.y - Target.Controller.LogicPosition.y) < 400.0)
			{
				SetStatus(MainStatus.Skill_01, SubStatus.Phase2);
			}
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
			_summonTimer.TimerStop();
			SetStatus(MainStatus.Debut);
		}
		else
		{
			_comboIndex = -1;
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
		UpdateDirection(base.direction);
		base.transform.position = pos;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		AI_STATE aiState = AI_STATE.mob_001;
		if (EnemyData.s_AI != "null")
		{
			aiState = (AI_STATE)Enum.Parse(typeof(AI_STATE), EnemyData.s_AI);
		}
		AiState = aiState;
		switch (AiState)
		{
		case AI_STATE.mob_002:
			_bDeadCallResult = false;
			base.DeadPlayCompleted = false;
			HoldShield = false;
			break;
		case AI_STATE.mob_003:
			base.DeadPlayCompleted = true;
			_bDeadCallResult = true;
			HoldShield = true;
			break;
		case AI_STATE.mob_004:
			_bDeadCallResult = false;
			base.DeadPlayCompleted = false;
			HoldShield = true;
			break;
		case AI_STATE.mob_005:
			_bDeadCallResult = false;
			base.DeadPlayCompleted = true;
			HoldShield = true;
			break;
		default:
			base.DeadPlayCompleted = true;
			_bDeadCallResult = true;
			HoldShield = false;
			break;
		}
		AI_STATE aiState2 = AiState;
		if ((uint)(aiState2 - 2) <= 2u)
		{
			_takeShield = true;
		}
		else
		{
			_takeShield = false;
		}
		if (!_takeShield)
		{
			_tfShieldMesh.gameObject.SetActive(false);
			_shieldTransform.gameObject.SetActive(false);
		}
		else
		{
			_tfShieldMesh.gameObject.SetActive(true);
			_shieldTransform.gameObject.SetActive(true);
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
			if ((bool)_efx_Skill04_Start)
			{
				_efx_Skill04_Start.Stop();
			}
			if ((bool)_efx_Skill04_Dash)
			{
				_efx_Skill04_Dash.Stop();
			}
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			SetStatus(MainStatus.Die);
		}
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		if (IsChipInfoAnim)
		{
			return;
		}
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
		{
			_lateAction = _mainStatus;
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase2 && IntroCallBack != null)
			{
				IntroCallBack();
			}
			break;
		}
		case MainStatus.FindPlayer:
			_lateAction = _mainStatus;
			_enemyAutoAimSystem.UpdateAimRange(200f);
			break;
		case MainStatus.Idle:
			_lateAction = _mainStatus;
			_velocity = VInt3.zero;
			break;
		case MainStatus.Skill_01:
			_lateAction = _mainStatus;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_checkUseSkill01 = false;
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase1:
				_velocity.x = 0;
				if ((bool)Target)
				{
					int num = Mathf.RoundToInt((float)(Target.Controller.LogicPosition.y - Controller.LogicPosition.y + 1000) * 100f / (float)Mathf.Abs(_maxGravity.i));
					if (num > _jupmpSpeed.y)
					{
						_velocity.y = num * 1000;
					}
					else
					{
						_velocity.y = _jupmpSpeed.y * 1000;
					}
				}
				else
				{
					_velocity.y = _jupmpSpeed.y * 1000;
				}
				break;
			case SubStatus.Phase2:
				IgnoreGravity = true;
				_checkUseSkill01 = false;
				_velocity = VInt3.zero;
				IsInvincible = true;
				_efx_Skill01_Start.Play();
				PlaySE("BossSE02", "bs012_vajur02");
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				if (AiState == AI_STATE.mob_002 || AiState == AI_STATE.mob_004)
				{
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform, 998);
				}
				break;
			case SubStatus.Phase3:
				_velocity.x = _dashSpeed * base.direction * 1000;
				_efx_Skill01_Dash.Play();
				break;
			case SubStatus.Phase4:
				if (bPlay04)
				{
					PlaySE("BossSE02", "bs012_vajur04_stop");
					bPlay04 = false;
				}
				_efx_Skill01_Start.Stop();
				_efx_Skill01_Dash.Stop();
				_efx_Skill01_Start.Clear();
				_efx_Skill01_Dash.Clear();
				IsInvincible = false;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				break;
			case SubStatus.Phase5:
				IgnoreGravity = false;
				break;
			case SubStatus.Phase6:
				_checkUseSkill01 = false;
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Skill_02:
			_lateAction = _mainStatus;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE02", "bs012_vajur05");
				_velocity = VInt3.zero;
				_shootBullet = false;
				break;
			}
			break;
		case MainStatus.Skill_03:
			_lateAction = _mainStatus;
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlayBossSE("BossSE02", "bs012_vajur08");
				_velocity = VInt3.zero;
				_eft_Skill03_Ring.Play();
				base.SoundSource.PlaySE("BossSE02", "bs012_vajur09", 0.6f);
				if ((AiState == AI_STATE.mob_002 || AiState == AI_STATE.mob_004) && (!_summonTimer.IsStarted() || _summonTimer.GetMillisecond() > 10000))
				{
					_summonTimer.TimerStart();
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
				}
				break;
			case SubStatus.Phase1:
				_ringBullet = (VajurilaFFRingBullet)BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, _transform.position + Vector3.up * 2.5f, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				_ringBullet.SetOwner(this);
				break;
			}
			break;
		case MainStatus.Skill_04:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
					UpdateDirection();
				}
				else
				{
					UpdateDirection(-base.direction);
				}
				_efx_Skill04_Start.Play();
				PlaySE("BossSE02", "bs012_vajur13");
				break;
			case SubStatus.Phase1:
				_collideBullet.UpdateBulletData(EnemyWeapons[4].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_velocity.x = _dashSpeed * base.direction * 1000;
				_efx_Skill04_Dash.Play();
				PlaySE("BossSE02", "bs012_vajur00");
				break;
			case SubStatus.Phase2:
				PlayBossSE("BossSE02", "bs012_vajur14");
				_efx_Skill04_Start.Stop();
				_efx_Skill04_Dash.Stop();
				_efx_Skill04_Start.Clear();
				_efx_Skill04_Dash.Clear();
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.ShieldOn:
			PlaySE("BossSE02", "bs012_vajur12");
			_takeShield = true;
			_shieldTransform.gameObject.SetActive(true);
			break;
		case MainStatus.ShieldOff:
			PlaySE("BossSE02", "bs012_vajur12");
			_takeShield = false;
			_shieldTransform.gameObject.SetActive(false);
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (bPlay04)
				{
					PlaySE("BossSE02", "bs012_vajur04_stop");
					bPlay04 = false;
				}
				_checkUseSkill01 = false;
				base.AllowAutoAim = false;
				_collideBullet.BackToPool();
				_velocity.x = 0;
				OrangeBattleUtility.LockPlayer();
				if (!Controller.Collisions.below)
				{
					IgnoreGravity = true;
					SetStatus(MainStatus.Die, SubStatus.Phase2);
					return;
				}
				break;
			case SubStatus.Phase1:
				if (_bDeadCallResult)
				{
					StartCoroutine(BossDieFlow(GetTargetPoint()));
				}
				else
				{
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
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
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_DEBUT_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_DEBUT_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_TAKEN_ON;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Idle:
		case MainStatus.IdleChip:
			if (!_takeShield)
			{
				_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_IDLE_LOOP;
			}
			else
			{
				_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_IDLE_LOOP;
			}
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Skill_01:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.SoundSource.PlaySE("BossSE02", "bs012_vajur01", 0.3f);
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_01_JUMP_START;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_02_JUMP_START;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_01_JUMP_LOOP;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_02_JUMP_LOOP;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_01_ATK_START;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_02_ATK_START;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase3:
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_01_ATK_LOOP;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_02_ATK_LOOP;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				if (!_takeShield)
				{
					if (_checkUseSkill01)
					{
						_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_01_JUMP_TO_FALL;
					}
					else
					{
						_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_01_ATK_END;
					}
				}
				else if (_checkUseSkill01)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_02_JUMP_TO_FALL;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_02_ATK_END;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase5:
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_01_FALL_LOOP;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_02_FALL_LOOP;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase6:
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_01_LANDING;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_02_LANDING;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_02:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_04_START;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_05_START;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				base.SoundSource.PlaySE("BossSE02", "bs012_vajur06", 0.4f);
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_04_END;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_05_END;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_03:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_06_START;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_07_START;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				base.SoundSource.PlaySE("BossSE02", "bs012_vajur10", 0.4f);
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_SKILL_06_END;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_07_END;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.Skill_04:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_03_START;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_03_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_SKILL_03_END;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.ShieldOn:
			_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_TAKEN_ON;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.ShieldOff:
			_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_TAKEN_OFF;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_DEAD_LOOP;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_DEAD_LOOP;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase2:
				if (!_takeShield)
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_BACK_HURT_LOOP;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_SHIELD_ON_HAND_HURT_LOOP;
				}
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
			break;
		case MainStatus.FindPlayer:
		case MainStatus.NextAction:
		case MainStatus.IdleWaitNet:
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
}
