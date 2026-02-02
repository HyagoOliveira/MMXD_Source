#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS045_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Run = 2,
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
		Phase6 = 6,
		Phase7 = 7,
		MAX_SUBSTATUS = 8
	}

	public enum AnimationID
	{
		ANI_DEBUT_LOOP = 0,
		ANI_IDLE = 1,
		ANI_IDLE_NO = 2,
		ANI_RUN = 3,
		ANI_RUN_NO = 4,
		ANI_SKILL0_START = 5,
		ANI_SKILL0_LOOP = 6,
		ANI_SKILL0_END = 7,
		ANI_SKILL1_START1 = 8,
		ANI_SKILL1_LOOP1 = 9,
		ANI_SKILL1_START2 = 10,
		ANI_SKILL1_LOOP2 = 11,
		ANI_SKILL1_END2 = 12,
		ANI_SKILL2_START = 13,
		ANI_SKILL2_LOOP = 14,
		ANI_SKILL2_END = 15,
		ANI_SKILL3_START = 16,
		ANI_SKILL3_LOOP = 17,
		ANI_SKILL3_END = 18,
		ANI_SKILL4_START = 19,
		ANI_SKILL4_LOOP = 20,
		ANI_SKILL4_END = 21,
		ANI_SKILL5_START = 22,
		ANI_SKILL5_LOOP = 23,
		ANI_SKILL5_END = 24,
		ANI_DEAD_START = 25,
		ANI_DEAD_LOOP = 26,
		ANI_DEAD_END = 27,
		ANI_DEAD_START_NO = 28,
		ANI_DEAD_LOOP_NO = 29,
		ANI_DEAD_END_NO = 30,
		ANI_HURT = 31,
		MAX_ANIMATION_ID = 32
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

	private int[] DefaultSkillCard = new int[4] { 0, 0, 1, 2 };

	private List<int> SkillCard = new List<int>();

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private Vector2 RoomCenter = new Vector2(9f, 8f);

	private Vector2 RoomDistance = new Vector2(9f, 5f);

	private bool HoldSickle = true;

	private CollideBullet SickleCollide;

	[SerializeField]
	private CharacterMaterial SickleMaterial;

	[SerializeField]
	private ParticleSystem SickleFX;

	[SerializeField]
	private ParticleSystem SickleFXEND;

	[SerializeField]
	private ParticleSystem MistFX;

	[SerializeField]
	private ParticleSystem MistFXEND;

	[SerializeField]
	private GameObject SickleMesh;

	private int ShowFrame;

	private Vector3[] ShowPoint = new Vector3[9];

	private bool StealthFinish = true;

	[SerializeField]
	private GameObject BodyMesh;

	[SerializeField]
	private GameObject HandMeshL;

	[SerializeField]
	private GameObject HandMeshR;

	private int ShootTimes;

	private float NextFrame;

	[SerializeField]
	private ParticleSystem BallFX;

	[SerializeField]
	private Transform LightBallPoint;

	private int MoveSpeed = 15000;

	[SerializeField]
	private ParticleSystem SickleATKFX;

	private int ShadowFrame;

	private OrangeCharacter targetOC;

	private EM110_Controller ThrownSickle;

	private Vector3 CenterGround;

	[SerializeField]
	private Transform HandSickle;

	private bool HasRetrieve;

	[SerializeField]
	private Transform HandPoint;

	private string ShowSequence = "444";

	private int ActionTimes = 3;

	private int AngleMode = 1;

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

	protected virtual void HashAnimation()
	{
		_animationHash[0] = Animator.StringToHash("BS045@sickle_debut_loop");
		_animationHash[1] = Animator.StringToHash("BS045@sickle_idle_loop");
		_animationHash[2] = Animator.StringToHash("BS045@nothing_idle_loop");
		_animationHash[3] = Animator.StringToHash("BS045@sickle_run_loop");
		_animationHash[4] = Animator.StringToHash("BS045@nothing_run_loop");
		_animationHash[5] = Animator.StringToHash("BS045@sickle_skill_01_start");
		_animationHash[6] = Animator.StringToHash("BS045@sickle_skill_01_loop");
		_animationHash[7] = Animator.StringToHash("BS045@sickle_skill_01_end");
		_animationHash[8] = Animator.StringToHash("BS045@sickle_skill_02_start");
		_animationHash[9] = Animator.StringToHash("BS045@sickle_skill_02_loop");
		_animationHash[10] = Animator.StringToHash("BS045@sickle_skill_02_atk_start");
		_animationHash[11] = Animator.StringToHash("BS045@sickle_skill_02_atk_loop");
		_animationHash[12] = Animator.StringToHash("BS045@sickle_skill_02_atk_end");
		_animationHash[13] = Animator.StringToHash("BS045@sickle_skill_03_start");
		_animationHash[14] = Animator.StringToHash("BS045@sickle_skill_03_loop");
		_animationHash[15] = Animator.StringToHash("BS045@sickle_skill_03_end");
		_animationHash[16] = Animator.StringToHash("BS045@sickle_skill_04_start");
		_animationHash[17] = Animator.StringToHash("BS045@sickle_skill_04_loop");
		_animationHash[18] = Animator.StringToHash("BS045@sickle_skill_04_end");
		_animationHash[19] = Animator.StringToHash("BS045@nothing_skill_05_start");
		_animationHash[20] = Animator.StringToHash("BS045@nothing_skill_05_loop");
		_animationHash[21] = Animator.StringToHash("BS045@nothing_skill_05_end");
		_animationHash[22] = Animator.StringToHash("BS045@nothing_skill_06_start");
		_animationHash[23] = Animator.StringToHash("BS045@nothing_skill_06_loop");
		_animationHash[24] = Animator.StringToHash("BS045@nothing_skill_06_end");
		_animationHash[25] = Animator.StringToHash("BS045@sickle_dead_start");
		_animationHash[26] = Animator.StringToHash("BS045@sickle_dead_loop");
		_animationHash[27] = Animator.StringToHash("BS045@sickle_dead_end");
		_animationHash[28] = Animator.StringToHash("BS045@nothing_dead_start");
		_animationHash[29] = Animator.StringToHash("BS045@nothing_dead_loop");
		_animationHash[30] = Animator.StringToHash("BS045@nothing_dead_end");
		_animationHash[31] = Animator.StringToHash("BS045@hurt_loop");
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Bip001").gameObject.AddOrGetComponent<CollideBullet>();
		SickleCollide = OrangeBattleUtility.FindChildRecursive(ref target, "SickleCollider").gameObject.AddOrGetComponent<CollideBullet>();
		GetObjects(ref target);
		_animationHash = new int[32];
		HashAnimation();
		base.AimPoint = new Vector3(0f, 1.6f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
	}

	protected override void Start()
	{
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
			if (netSyncData.sParam0 != string.Empty)
			{
				ShowSequence = netSyncData.sParam0;
			}
		}
		SetStatus((MainStatus)nSet);
	}

	private void UpdateDirection(int forceDirection = 0)
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)(-base.direction));
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
			if (subStatus2 == SubStatus.Phase2 && IntroCallBack != null)
			{
				IntroCallBack();
			}
			break;
		}
		case MainStatus.Idle:
			UpdateDirection();
			_velocity.x = 0;
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Stealth(true);
				break;
			case SubStatus.Phase1:
				ShowFrame = GameLogicUpdateManager.GameFrame + 20;
				break;
			case SubStatus.Phase2:
				SetShowPos(4);
				UpdateDirection();
				Stealth(false);
				break;
			case SubStatus.Phase4:
				BallFX.transform.position = new Vector3(0.8f * (float)base.direction, 0.5f, 0f - LightBallPoint.position.z) + LightBallPoint.position;
				SwitchFx(BallFX, true);
				ShootTimes = 9;
				NextFrame = 0.5f;
				break;
			case SubStatus.Phase5:
				SwitchFx(BallFX, false);
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Stealth(true);
				break;
			case SubStatus.Phase1:
				ShowFrame = GameLogicUpdateManager.GameFrame + 20;
				break;
			case SubStatus.Phase2:
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				int num3 = 0;
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target.transform.position);
					if (Target.transform.position.x < RoomCenter.x)
					{
						num3 += 2;
					}
					if (Target.transform.position.y < RoomCenter.y)
					{
						num3++;
					}
				}
				SetShowPos(num3);
				UpdateDirection();
				Stealth(false);
				break;
			}
			case SubStatus.Phase3:
				base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig02", 0.5f);
				break;
			case SubStatus.Phase5:
				SickleATKFX.gameObject.SetActive(true);
				SwitchFx(SickleATKFX, true);
				UpdateSickleCollide(2);
				SickleCollide.Active(targetMask);
				break;
			case SubStatus.Phase6:
				ShadowFrame = GameLogicUpdateManager.GameFrame;
				_velocity.x = MoveSpeed * base.direction;
				break;
			case SubStatus.Phase7:
				SickleCollide.BackToPool();
				_velocity.x = 0;
				SwitchFx(SickleATKFX, false);
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SickleMesh.layer = ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer;
				Stealth(true);
				break;
			case SubStatus.Phase1:
				ShowFrame = GameLogicUpdateManager.GameFrame + 20;
				break;
			case SubStatus.Phase2:
			{
				PlaySE("BossSE03", "bs021_ptmsig04");
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					_transform.position = new Vector3(0f, -0.2f, 0f) + Target.transform.position;
					Controller.LogicPosition = new VInt3(_transform.position);
				}
				Stealth(false, false);
				string s_MODEL = EnemyData.s_MODEL;
				if (s_MODEL == "enemy_bs123")
				{
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
				}
				break;
			}
			case SubStatus.Phase4:
				UpdateSickleCollide(3);
				if ((bool)targetOC)
				{
					_collideBullet.BackToPool();
				}
				break;
			case SubStatus.Phase5:
				PlaySE("BossSE03", "bs021_ptmsig05");
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Stealth(true);
				break;
			case SubStatus.Phase1:
				ShowFrame = GameLogicUpdateManager.GameFrame + 20;
				break;
			case SubStatus.Phase2:
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				int num2 = 7;
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target.transform.position);
					if (Target.transform.position.x < RoomCenter.x)
					{
						num2++;
						UpdateDirection(-1);
					}
					else
					{
						UpdateDirection(1);
					}
				}
				SetShowPos(num2);
				Stealth(false);
				break;
			}
			case SubStatus.Phase3:
				PlaySE("BossSE03", "bs021_ptmsig06");
				break;
			case SubStatus.Phase4:
				base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig07", 0.5f);
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ShowSickle(false);
				HasRetrieve = false;
				if (ThrownSickle.transform.position.x - _transform.position.x < 0f)
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				break;
			}
			break;
		case MainStatus.Skill5:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Stealth(true);
				break;
			case SubStatus.Phase1:
				ShowFrame = GameLogicUpdateManager.GameFrame + 20;
				break;
			case SubStatus.Phase2:
			{
				int num = 4;
				try
				{
					num = int.Parse(ShowSequence[0].ToString());
					ShowSequence = ShowSequence.Remove(0, 1);
				}
				catch
				{
					num = 4;
					Debug.LogError("散彈攻擊的位置序列中，含有非數字的符號。");
				}
				SetShowPos(num);
				UpdateDirection();
				Stealth(false);
				break;
			}
			case SubStatus.Phase4:
			{
				BallFX.transform.position = new Vector3(0f, -0.6f, 0f) + LightBallPoint.position;
				SwitchFx(BallFX, true);
				string s_MODEL = EnemyData.s_MODEL;
				if (s_MODEL == "enemy_bs123")
				{
					base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig13");
				}
				else
				{
					base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig23");
				}
				break;
			}
			case SubStatus.Phase5:
				HasShot = false;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				nDeadCount = 0;
				string s_MODEL = EnemyData.s_MODEL;
				if (s_MODEL == "enemy_bs123")
				{
					base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig20", 0.5f);
				}
				if (_transform.position.y > ShowPoint[4].y)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase4);
					break;
				}
				IgnoreGravity = false;
				Controller.collisionMask = 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer;
				base.AllowAutoAim = false;
				_velocity.x = 0;
				break;
			}
			case SubStatus.Phase2:
			{
				string s_MODEL = EnemyData.s_MODEL;
				if (s_MODEL == "enemy_bs123")
				{
					base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig22");
				}
				break;
			}
			case SubStatus.Phase4:
			{
				string s_MODEL = EnemyData.s_MODEL;
				if (s_MODEL == "enemy_bs123")
				{
					StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
				}
				else
				{
					StartCoroutine(BossDieFlow(GetTargetPoint()));
				}
				break;
			}
			case SubStatus.Phase5:
				base.DeadPlayCompleted = true;
				nDeadCount = 0;
				base.AllowAutoAim = false;
				_velocity = VInt3.zero;
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
				_currentAnimationId = AnimationID.ANI_IDLE;
				return;
			}
			break;
		case MainStatus.Idle:
			if (HoldSickle)
			{
				_currentAnimationId = AnimationID.ANI_IDLE;
			}
			else
			{
				_currentAnimationId = AnimationID.ANI_IDLE_NO;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL1_START1;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP1;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL1_START2;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP2;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_SKILL1_END2;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL2_START;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL2_END;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL3_START;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL3_LOOP;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL3_END;
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			default:
				return;
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
			default:
				return;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_IDLE_NO;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_SKILL5_START;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_SKILL5_LOOP;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_SKILL5_END;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				if (HoldSickle)
				{
					_currentAnimationId = AnimationID.ANI_DEAD_START;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_DEAD_START_NO;
				}
				break;
			case SubStatus.Phase1:
				if (HoldSickle)
				{
					_currentAnimationId = AnimationID.ANI_DEAD_LOOP;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_DEAD_LOOP_NO;
				}
				break;
			case SubStatus.Phase2:
				if (HoldSickle)
				{
					_currentAnimationId = AnimationID.ANI_DEAD_END;
				}
				else
				{
					_currentAnimationId = AnimationID.ANI_DEAD_END_NO;
				}
				break;
			case SubStatus.Phase3:
				return;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
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
			if (selfBuffManager.listBuffs.Count > 0)
			{
				selfBuffManager.ClearBuff();
			}
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
				if (_introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_introReady && !bWaitNetStatus)
				{
					_characterMaterial.ChangeDissolveColor(new Color(0.1f, 0f, 0.3f));
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (bWaitNetStatus)
			{
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				if (Math.Abs(Target.transform.position.x - _transform.position.x) > RoomDistance.x)
				{
					UploadStatus(MainStatus.Skill2);
				}
				else
				{
					UploadStatus(MainStatus.Skill1);
				}
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (StealthFinish)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && GameLogicUpdateManager.GameFrame > ShowFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (StealthFinish)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f && ShootTimes == 0)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				else if (ShootTimes > 0 && _currentFrame > NextFrame)
				{
					ShootTimes--;
					NextFrame += 1f;
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					Vector3 pDirection = Vector3.down;
					if ((bool)Target)
					{
						pDirection = (Target.transform.position - _transform.position).normalized;
					}
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, BallFX.transform.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill1:
			if (bWaitNetStatus)
			{
				break;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (StealthFinish)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && GameLogicUpdateManager.GameFrame > ShowFrame)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (StealthFinish)
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
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f)
				{
					string s_MODEL = EnemyData.s_MODEL;
					if (s_MODEL == "enemy_bs123")
					{
						PlaySE("BossSE03", "bs021_ptmsig15");
					}
					else
					{
						PlaySE("BossSE03", "bs021_ptmsig03");
					}
					SetStatus(MainStatus.Skill1, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if ((float)(-base.direction) * (_transform.position.x - (RoomCenter.x + 6f * (float)base.direction)) < 0f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase7);
				}
				else if (GameLogicUpdateManager.GameFrame > ShadowFrame)
				{
					ShadowFrame = GameLogicUpdateManager.GameFrame + 3;
					string s_MODEL = EnemyData.s_MODEL;
					if (!(s_MODEL == "enemy_bs123"))
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_sigma-death_003", _transform, Quaternion.Euler(0f, 0f, 0f), new Vector3(base.direction, 1f, 1f), Array.Empty<object>());
					}
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					SickleATKFX.gameObject.SetActive(false);
					UploadStatus(MainStatus.Skill2);
				}
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (StealthFinish)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && GameLogicUpdateManager.GameFrame > ShowFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (StealthFinish)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (bWaitNetStatus)
				{
					break;
				}
				if (_currentFrame > 1f)
				{
					_collideBullet.HitCallback = null;
					if ((bool)targetOC)
					{
						SetStatus(MainStatus.Skill2, SubStatus.Phase4);
						break;
					}
					SickleMesh.layer = ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer;
					UploadStatus(MainStatus.Skill3);
				}
				else if (!_collideBullet.IsActivate && _currentFrame > 0.5f)
				{
					_collideBullet.Active(targetMask);
					_collideBullet.HitCallback = StunHitCallBack;
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 0.4f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (bWaitNetStatus)
				{
					break;
				}
				if (_currentFrame > 0.075f && !SickleCollide.IsActivate)
				{
					SickleCollide.Active(targetMask);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_sigma-death_slash_000", new Vector3(0f, -0.48f, 0f) + HandSickle.position, Quaternion.Euler(0f, 90 + 90 * base.direction, 0f), new object[1]
					{
						new Vector3(1f, 1f, 1f)
					});
				}
				else if (_currentFrame > 1f)
				{
					_collideBullet.Active(targetMask);
					SickleCollide.BackToPool();
					SickleMesh.layer = ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer;
					if ((bool)targetOC)
					{
						targetOC.SetStun(false);
						targetOC = null;
						UploadStatus(MainStatus.Skill0);
					}
					else
					{
						UploadStatus(MainStatus.Skill3);
					}
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (StealthFinish)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && GameLogicUpdateManager.GameFrame > ShowFrame)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (StealthFinish)
				{
					SetStatus(MainStatus.Skill3, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
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
				if (_currentFrame > 0.1f && ThrownSickle == null && HoldSickle)
				{
					ThrownSickle = SpawnSickle();
					if ((bool)ThrownSickle)
					{
						Vector3 vector3 = CenterGround - HandPoint.position;
						vector3.z = 0f;
						ThrownSickle.SetPositionAndRotation(HandSickle.position, base.direction);
						ThrownSickle.SetParameter(new VInt3(vector3 * 2.5f), 1);
						ThrownSickle.SetParent(this);
						ShowSickle(false);
						ThrownSickle.SetActive(true);
					}
				}
				else if (_currentFrame > 1f)
				{
					UploadStatus(MainStatus.Skill5);
				}
				break;
			}
			break;
		case MainStatus.Skill4:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f && !HasRetrieve)
				{
					if ((bool)ThrownSickle)
					{
						HasRetrieve = true;
						float num2 = Vector2.Angle(HandPoint.position - ThrownSickle.transform.position, Vector2.right * base.direction);
						PlaySE("BossSE03", "bs021_ptmsig10");
						string s_MODEL = EnemyData.s_MODEL;
						if (s_MODEL == "enemy_bs123")
						{
							base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig19");
						}
						else
						{
							base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig11", 0.3f);
						}
						s_MODEL = EnemyData.s_MODEL;
						if (s_MODEL == "enemy_bs123")
						{
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_Halloween-Sigma_000", HandPoint.position, Quaternion.Euler(num2, 0f, 0f), new object[1]
							{
								new Vector3(1f, 1f, 1f)
							});
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_Halloween-Sigma_000", ThrownSickle.transform.position, Quaternion.Euler(0f - num2, 0f, 0f), new object[1]
							{
								new Vector3(1f, 1f, 1f)
							});
						}
						else
						{
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_sigma-death_000", HandPoint.position, Quaternion.Euler(num2, 0f, 0f), new object[1]
							{
								new Vector3(1f, 1f, 1f)
							});
							MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_sigma-death_000", ThrownSickle.transform.position, Quaternion.Euler(0f - num2, 0f, 0f), new object[1]
							{
								new Vector3(1f, 1f, 1f)
							});
						}
					}
					else
					{
						Debug.LogError("鐮刀異常消失，發動收回特效");
					}
				}
				if (_currentFrame > 2.4f)
				{
					if ((bool)ThrownSickle)
					{
						Vector3 vector2 = HandPoint.position - ThrownSickle.transform.position;
						vector2.z = 0f;
						ThrownSickle.SetParameter(new VInt3(vector2 * 2f), -1);
						SetStatus(MainStatus.Skill4, SubStatus.Phase1);
					}
					else
					{
						Debug.LogError("鐮刀異常消失，收回鐮刀");
					}
				}
				break;
			case SubStatus.Phase1:
				if (Vector2.Distance(HandPoint.position, ThrownSickle.transform.position) < 0.5f)
				{
					ThrownSickle.BackToPool();
					ThrownSickle = null;
					ShowSickle(true);
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
				if (StealthFinish)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f && GameLogicUpdateManager.GameFrame > ShowFrame)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (StealthFinish)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill5, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 0.25f && !HasShot)
				{
					HasShot = true;
					float num = 0f;
					if (AngleMode == 1)
					{
						AngleMode = 2;
					}
					else
					{
						AngleMode = 1;
						num = 22.5f;
					}
					for (int i = 0; i < 8; i++)
					{
						Vector3 vector = Quaternion.Euler(0f, 0f, (float)(i * 45) + num) * Vector3.up;
						Vector3 worldPos = vector * 0.5f + BallFX.transform.position;
						BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, worldPos, vector, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					}
					SwitchFx(BallFX, false);
					string s_MODEL = EnemyData.s_MODEL;
					if (s_MODEL == "enemy_bs123")
					{
						base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig14");
					}
					else
					{
						base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig24");
					}
				}
				if (_currentFrame > 1f)
				{
					if (ShowSequence.Length > 0)
					{
						SetStatus(MainStatus.Skill5);
					}
					else
					{
						UploadStatus(MainStatus.Skill4);
					}
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
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 0.8f)
				{
					string s_MODEL = EnemyData.s_MODEL;
					if (s_MODEL == "enemy_bs123")
					{
						StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
					}
					else
					{
						StartCoroutine(BossDieFlow(GetTargetPoint()));
					}
					SetStatus(MainStatus.Die, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase5:
				if (nDeadCount > 5)
				{
					SetStatus(MainStatus.Die);
				}
				else
				{
					nDeadCount++;
				}
				break;
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				break;
			}
			break;
		case MainStatus.Run:
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
		_characterMaterial.ChangeDissolveColor(new Color(1f, 1f, 1f));
		IgnoreGravity = true;
		base.SetActive(isActive);
		if (isActive)
		{
			IgnoreGravity = true;
			CheckRoomSize();
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			UpdateSickleCollide(2);
			SickleMaterial.Appear(null, 0.5f);
			SetStatus(MainStatus.Debut);
			string s_MODEL = EnemyData.s_MODEL;
			bool flag = s_MODEL == "enemy_bs123";
			base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig12", 0.5f);
			base.SoundSource.PlaySE("BossSE03", "bs021_ptmsig21", 0.5f);
		}
		else
		{
			if ((bool)ThrownSickle)
			{
				ThrownSickle.SetActive(false);
			}
			SickleMaterial.Disappear(null, 0.5f);
			SickleCollide.BackToPool();
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
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * -1f);
		base.transform.position = pos;
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

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			if ((bool)SickleCollide)
			{
				SickleCollide.BackToPool();
			}
			SwitchFx(SickleATKFX, false);
			SwitchFx(BallFX, false);
			StageUpdate.SlowStage();
			SetColliderEnable(false);
			if (targetOC != null)
			{
				targetOC.SetStun(false);
				targetOC = null;
			}
			string s_MODEL = EnemyData.s_MODEL;
			if (s_MODEL == "enemy_bs123")
			{
				SetStatus(MainStatus.Die, SubStatus.Phase5);
			}
			else
			{
				SetStatus(MainStatus.Die);
			}
			selfBuffManager.ClearBuff();
		}
	}

	private void UploadStatus(MainStatus status)
	{
		if (status == MainStatus.Idle || !CheckHost())
		{
			return;
		}
		if (status == MainStatus.Skill5)
		{
			ShowSequence = "";
			for (int i = 0; i < ActionTimes; i++)
			{
				ShowSequence += OrangeBattleUtility.Random(40, 70) / 10;
			}
			UploadEnemyStatus((int)status, false, null, new object[1] { ShowSequence });
		}
		else
		{
			UploadEnemyStatus((int)status);
		}
	}

	private void CheckRoomSize()
	{
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.left, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		if ((bool)raycastHit2D && (bool)raycastHit2D2)
		{
			RoomDistance.x = (raycastHit2D2.point.x - raycastHit2D.point.x) / 2f;
			RoomCenter.x = raycastHit2D2.point.x - RoomDistance.x;
		}
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(_transform.position, Vector2.down, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		if ((bool)raycastHit2D3)
		{
			RoomDistance.y = 5f;
			RoomCenter.y = raycastHit2D3.transform.position.y + 2.5f;
			CenterGround = new Vector3(RoomCenter.x, raycastHit2D3.transform.position.y, 0f);
		}
		ShowPoint[0] = new Vector3(RoomCenter.x - (RoomDistance.x - 2.5f), RoomCenter.y + (RoomDistance.y / 2f - 1f), 0f);
		ShowPoint[1] = new Vector3(RoomCenter.x - (RoomDistance.x - 2.5f), RoomCenter.y - (RoomDistance.y / 2f - 1f), 0f);
		ShowPoint[2] = new Vector3(RoomCenter.x + (RoomDistance.x - 2.5f), RoomCenter.y + (RoomDistance.y / 2f - 1f), 0f);
		ShowPoint[3] = new Vector3(RoomCenter.x + (RoomDistance.x - 2.5f), RoomCenter.y - (RoomDistance.y / 2f - 1f), 0f);
		ShowPoint[4] = new Vector3(RoomCenter.x, RoomCenter.y, 0f);
		ShowPoint[5] = new Vector3(RoomCenter.x - (RoomDistance.x - 2f) / 2f, RoomCenter.y + RoomDistance.y / 4f, 0f);
		ShowPoint[6] = new Vector3(RoomCenter.x + (RoomDistance.x - 2f) / 2f, RoomCenter.y + RoomDistance.y / 4f, 0f);
		ShowPoint[7] = new Vector3(RoomCenter.x - (RoomDistance.x - 2f) / 2f, RoomCenter.y + (RoomDistance.y / 2f - 1f), 0f);
		ShowPoint[8] = new Vector3(RoomCenter.x + (RoomDistance.x - 2f) / 2f, RoomCenter.y + (RoomDistance.y / 2f - 1f), 0f);
	}

	private void Stealth(bool SwitchOn, bool opencollide = true)
	{
		StealthFinish = false;
		if (SwitchOn)
		{
			_enemyCollider[0].SetColliderEnable(false);
			_characterMaterial.Disappear(StealthStart, 0.8f);
			SickleMaterial.Disappear(null, 0.5f);
			_collideBullet.BackToPool();
			SwitchFx(SickleFX, false);
			SwitchFx(MistFX, false);
			SwitchFx(MistFXEND, true);
			SwitchFx(SickleFXEND, true);
			return;
		}
		SwitchFx(MistFX, true);
		SickleMesh.SetActive(true);
		BodyMesh.SetActive(true);
		HandMeshL.SetActive(true);
		HandMeshR.SetActive(true);
		_enemyCollider[0].SetColliderEnable();
		_characterMaterial.Appear(StealthOver, 0.8f);
		SickleMaterial.Appear(null, 0.5f);
		if (opencollide)
		{
			_collideBullet.Active(targetMask);
		}
	}

	private void SetShowPos(int position)
	{
		Controller.LogicPosition = new VInt3(ShowPoint[position]);
		_transform.position = ShowPoint[position];
	}

	private void StunHitCallBack(object obj)
	{
		if (obj == null || targetOC != null)
		{
			return;
		}
		Collider2D collider2D = obj as Collider2D;
		if (collider2D != null)
		{
			targetOC = OrangeBattleUtility.GetHitTargetOrangeCharacter(collider2D);
			if (targetOC != null)
			{
				targetOC.direction = base.direction * -1;
			}
		}
		if (targetOC != null && (int)targetOC.Hp > 0 && (int)Hp > 0)
		{
			if ((bool)targetOC.IsUnBreakX())
			{
				targetOC = null;
			}
			else
			{
				targetOC.SetStun(true);
			}
		}
		else if ((targetOC != null && (int)targetOC.Hp < 0) || (int)Hp < 0)
		{
			targetOC.SetStun(false);
			targetOC = null;
		}
	}

	private void StealthStart()
	{
		StealthFinish = true;
		SickleMesh.SetActive(false);
		BodyMesh.SetActive(false);
		HandMeshL.SetActive(false);
		HandMeshR.SetActive(false);
	}

	private void StealthOver()
	{
		SwitchFx(SickleFX, true);
		StealthFinish = true;
	}

	private void ShowSickle(bool active)
	{
		HoldSickle = active;
		HandSickle.gameObject.SetActive(active);
		if (active)
		{
			SwitchFx(SickleFX, true);
		}
		else
		{
			SwitchFx(SickleFX, false);
		}
	}

	private EM110_Controller SpawnSickle(int NetSerialID = 0)
	{
		EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[5].BulletData.f_EFFECT_X], sNetSerialID + NetSerialID);
		if ((bool)enemyControllerBase)
		{
			return enemyControllerBase.gameObject.GetComponent<EM110_Controller>();
		}
		return null;
	}

	private void UpdateSickleCollide(int skill)
	{
		SickleCollide.UpdateBulletData(EnemyWeapons[skill].BulletData);
		SickleCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
	}

	public float GetRoomEdge(int forward = 1)
	{
		if (forward != 1 && forward != -1)
		{
			Debug.LogError("參數請輸入 1 或 -1");
		}
		return RoomCenter.x + (RoomDistance.x - 0.5f) * (float)forward;
	}

	private void GetObjects(ref Transform[] childs)
	{
		if (!HandPoint)
		{
			HandPoint = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip001 R Hand");
		}
		if (!SickleFX)
		{
			Transform transform = OrangeBattleUtility.FindChildRecursive(ref childs, "fx", true);
			if ((bool)transform)
			{
				SickleFX = transform.GetComponent<ParticleSystem>();
			}
		}
		if (!SickleFXEND)
		{
			Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref childs, "fx_end", true);
			if ((bool)transform2)
			{
				SickleFXEND = transform2.GetComponent<ParticleSystem>();
			}
		}
		if (!MistFX)
		{
			Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref childs, "fxduring_mist", true);
			if ((bool)transform3)
			{
				MistFX = transform3.GetComponent<ParticleSystem>();
			}
		}
		if (!MistFXEND)
		{
			Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref childs, "fxduring_mist_end", true);
			if ((bool)transform4)
			{
				MistFXEND = transform4.GetComponent<ParticleSystem>();
			}
		}
		if (!LightBallPoint)
		{
			LightBallPoint = OrangeBattleUtility.FindChildRecursive(ref childs, "LightballPoint", true);
		}
		if (!BallFX)
		{
			Transform transform5 = OrangeBattleUtility.FindChildRecursive(ref childs, "fxduring_001", true);
			if ((bool)transform5)
			{
				BallFX = transform5.GetComponent<ParticleSystem>();
			}
		}
		if (!SickleATKFX)
		{
			Transform transform6 = OrangeBattleUtility.FindChildRecursive(ref childs, "fxduring_skill2", true);
			if ((bool)transform6)
			{
				SickleATKFX = transform6.GetComponent<ParticleSystem>();
			}
		}
		if (!BodyMesh)
		{
			BodyMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS045_BodyMesh_G_c", true).gameObject;
		}
		if (!HandMeshL)
		{
			HandMeshL = OrangeBattleUtility.FindChildRecursive(ref childs, "BS045_HandMesh_G_m", true).gameObject;
		}
		if (!HandMeshR)
		{
			HandMeshR = OrangeBattleUtility.FindChildRecursive(ref childs, "BS045_HandMesh_G_c", true).gameObject;
		}
		if (!SickleMesh)
		{
			SickleMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS045_SickleMesh", true).gameObject;
		}
		if (!HandSickle)
		{
			HandSickle = OrangeBattleUtility.FindChildRecursive(ref childs, "BS045_Sickle", true);
			SickleMaterial = HandSickle.GetComponent<CharacterMaterial>();
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
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(50f);
		UpdateAIState();
		AI_STATE aiState = AiState;
		string s_MODEL = EnemyData.s_MODEL;
		if (s_MODEL == "enemy_bs123")
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_Halloween-Sigma_000", 2);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_sigma-death_slash_000", 2);
		}
		else
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_sigma-death_003", 2);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_sigma-death_000", 2);
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_sigma-death_slash_000", 2);
		}
	}
}
