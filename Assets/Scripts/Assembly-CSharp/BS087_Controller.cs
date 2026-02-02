#define RELEASE
using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS087_Controller : EnemyControllerBase, IManagedUpdateBehavior, IF_ForceExecute
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
		Judgement = 8,
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
		MAX_SUBSTATUS = 7
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_RUN = 2,
		ANI_CHARGE = 3,
		ANI_Skill0 = 4,
		ANI_Skill1 = 5,
		ANI_Skill2 = 6,
		ANI_Skill3 = 7,
		ANI_Skill4 = 8,
		ANI_Skill5 = 9,
		ANI_ROUNDABOUT = 10,
		ANI_HURT = 11,
		MAX_ANIMATION_ID = 12
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

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private Transform _ObjInfoBar;

	[SerializeField]
	private float MoveSpeed = 4500f;

	[SerializeField]
	private float AssaultSpeed = 7500f;

	[SerializeField]
	private Transform _shieldTransform;

	[SerializeField]
	private Transform HiltTransform;

	[SerializeField]
	private Transform RootTransform;

	private float RotateAngle;

	private Vector3 StartPos;

	private Vector3 EndPos;

	private float lastdistance;

	private bool WaitCD;

	private int _otherTexIndex;

	[SerializeField]
	private float NextStatusTime = 0.5f;

	private int NextStatusFrame;

	private float AssaultTime;

	private bool GetWall;

	[SerializeField]
	private float JudgeDis = 2.5f;

	private float TurnToward;

	private bool GotHurt;

	private bool HasArrive;

	private bool CanStop;

	[SerializeField]
	private float StopTime = 0.5f;

	private int StopFrame;

	[SerializeField]
	private float SK3AssaultTime = 0.5f;

	private int AssaultFrame;

	[SerializeField]
	private float SK5AssaultTime = 1.5f;

	private int AtkTimes = 3;

	private Vector3[] AssaultPos = new Vector3[4];

	private readonly int _HashAngle = Animator.StringToHash("angle");

	[SerializeField]
	private int ActionTimes = 3;

	private string ShowSequence = "222";

	private float FlashTime = 0.5f;

	private int FlashFrame;

	[SerializeField]
	private int AIStep;

	private MainStatus[] AICircuit;

	private MainStatus[] AICircuit_1 = new MainStatus[7]
	{
		MainStatus.Idle,
		MainStatus.Skill0,
		MainStatus.Idle,
		MainStatus.Skill1,
		MainStatus.Idle,
		MainStatus.Skill2,
		MainStatus.Skill3
	};

	private MainStatus[] AICircuit_2 = new MainStatus[8]
	{
		MainStatus.Idle,
		MainStatus.Skill0,
		MainStatus.Idle,
		MainStatus.Skill4,
		MainStatus.Skill5,
		MainStatus.Idle,
		MainStatus.Skill2,
		MainStatus.Skill3
	};

	[SerializeField]
	private GameObject SwordMesh;

	[SerializeField]
	private Transform _projectorTransform;

	[SerializeField]
	private ParticleSystem _efx_SpotLight;

	private float _projectorHeight = 6f;

	private MainStatus NextSkill;

	protected internal BossCorpsTool CorpsTool = new BossCorpsTool();

	private int MissionRunTimes;

	private bool needStop;

	private bool isObedient
	{
		get
		{
			if (CorpsTool.Member != null)
			{
				return CorpsTool.isObedient;
			}
			return false;
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

	protected virtual void HashAnimation()
	{
		_animationHash = new int[12];
		_animationHash[1] = Animator.StringToHash("BS073@debut");
		_animationHash[0] = Animator.StringToHash("BS073@idle_loop");
		_animationHash[2] = Animator.StringToHash("BS073@run_loop");
		_animationHash[3] = Animator.StringToHash("BS073@charge_loop");
		_animationHash[4] = Animator.StringToHash("BS073@skill_1_loop");
		_animationHash[5] = Animator.StringToHash("BS073@skill_3_loop");
		_animationHash[6] = Animator.StringToHash("BS073@skill_3_loop");
		_animationHash[7] = Animator.StringToHash("BS073@skill_5_loop");
		_animationHash[8] = Animator.StringToHash("BS073@skill_5_loop");
		_animationHash[9] = Animator.StringToHash("BS073@skill_5_loop");
		_animationHash[10] = Animator.StringToHash("BS073@skill_2_loop");
		_animationHash[11] = Animator.StringToHash("BS073@hurt_loop");
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] componentsInChildren = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(componentsInChildren);
		_animator = ModelTransform.GetComponent<Animator>();
		HashAnimation();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		base.AimTransform = HiltTransform;
		base.AimPoint = Vector3.zero;
		_enemyAutoAimSystem.UpdateAimRange(80f);
		FallDownSE = new string[2] { "BattleSE", "bt_ridearmor02" };
		AiTimer.TimerStart();
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
			Controller.LogicPosition.z = 0;
			TargetPos.x = netSyncData.TargetPosX;
			TargetPos.y = netSyncData.TargetPosY;
			TargetPos.z = netSyncData.TargetPosZ;
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
			if (netSyncData.nParam0 != 0)
			{
				AICircuit = ((netSyncData.nParam0 == 1) ? AICircuit_1 : AICircuit_2);
			}
			if (netSyncData.sParam0 != string.Empty && netSyncData.sParam0 != null)
			{
				switch ((MainStatus)nSet)
				{
				case MainStatus.Skill3:
					try
					{
						TurnToward = float.Parse(netSyncData.sParam0.Split(',')[1]);
					}
					catch
					{
						TurnToward = 0f;
						Debug.LogError("旋轉角度非數字。");
					}
					break;
				case MainStatus.Skill4:
					ShowSequence = netSyncData.sParam0.Split(',')[1] ?? "";
					break;
				}
				string text = netSyncData.sParam0.Split(',')[0];
				for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
				{
					if (!(StageUpdate.runPlayers[i].sNetSerialID != text))
					{
						Target = StageUpdate.runPlayers[i];
						break;
					}
				}
			}
		}
		if (!GetWall)
		{
			GetWall = CheckRoomSize();
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
			if (CorpsTool.hasDebut)
			{
				base.AllowAutoAim = true;
				return;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SwordMesh.SetActive(false);
				break;
			case SubStatus.Phase1:
				_projectorTransform.gameObject.SetActive(true);
				_efx_SpotLight.Stop();
				LeanTween.value(base.gameObject, ModelTransform.position.y + _projectorHeight, ModelTransform.position.y + 2f, 2f).setOnUpdate(delegate(float f)
				{
					Vector3 position = _projectorTransform.position;
					position.y = f;
					_projectorTransform.position = position;
				}).setOnComplete((Action)delegate
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				});
				break;
			case SubStatus.Phase2:
				PlaySE("BossSE03", "bs109_chop01");
				_efx_SpotLight.Play();
				break;
			case SubStatus.Phase3:
				SwordMesh.SetActive(true);
				break;
			case SubStatus.Phase4:
				base.AllowAutoAim = true;
				if ((bool)_ObjInfoBar)
				{
					_ObjInfoBar.gameObject.SetActive(true);
				}
				LeanTween.value(base.gameObject, _projectorTransform.position.y, _projectorTransform.position.y + _projectorHeight - 2f, 2f).setOnUpdate(delegate(float f)
				{
					Vector3 position2 = _projectorTransform.position;
					position2.y = f;
					_projectorTransform.position = position2;
				}).setOnComplete((Action)delegate
				{
					_projectorTransform.gameObject.SetActive(false);
					SetStatus(MainStatus.Debut, SubStatus.Phase5);
				});
				break;
			case SubStatus.Phase5:
				if (AiState == AI_STATE.mob_003)
				{
					Unlock();
				}
				break;
			}
			break;
		case MainStatus.Idle:
			NextStatusFrame = (int)(NextStatusTime * 20f) + GameLogicUpdateManager.GameFrame;
			_velocity = VInt3.zero;
			_animator.speed = 1f;
			_animator.SetLayerWeight(1, 0f);
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			break;
		case MainStatus.Skill0:
			if (_subStatus == SubStatus.Phase0)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				Vector3 vector2;
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
					UpdateDirection();
					EndPos = Target._transform.position;
					vector2 = (EndPos - _transform.position).normalized;
				}
				else
				{
					EndPos = Vector3.up * 8f;
					vector2 = Vector3.up;
				}
				Vector3.Angle(Vector3.right * base.direction, vector2);
				lastdistance = Vector3.Distance(_transform.position, EndPos);
				_velocity = new VInt3(vector2 * 0.001f * MoveSpeed);
			}
			break;
		case MainStatus.Skill1:
			if (_subStatus == SubStatus.Phase0)
			{
				_velocity = VInt3.zero;
			}
			break;
		case MainStatus.Skill2:
			if (_subStatus == SubStatus.Phase0)
			{
				HasArrive = false;
				CanStop = false;
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					TargetPos = new VInt3(Target._transform.position);
				}
				Vector3 vector4;
				if ((bool)Target)
				{
					UpdateDirection();
					EndPos = TargetPos.vec3;
					vector4 = (EndPos - _transform.position).normalized;
				}
				else
				{
					EndPos = Vector3.up * 8f;
					vector4 = Vector3.up;
				}
				lastdistance = Vector3.Distance(_transform.position, EndPos);
				_velocity = new VInt3(vector4 * 0.001f * MoveSpeed);
			}
			break;
		case MainStatus.Skill3:
			if (_subStatus == SubStatus.Phase0)
			{
				_animator.speed = 1f;
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				Vector3 vector = Quaternion.Euler(0f, 0f, (float)(-base.direction) * TurnToward) * Vector3.up;
				vector.z = 0f;
				if (TurnToward > 180f)
				{
					TurnToward = 360f - TurnToward;
					UpdateDirection(-base.direction);
				}
				_animator.SetFloat(_HashAngle, TurnToward);
				AssaultTime = SK3AssaultTime;
				AssaultFrame = (int)(AssaultTime * 20f) + GameLogicUpdateManager.GameFrame;
				_velocity = new VInt3(vector * 0.001f * AssaultSpeed);
				base.SoundSource.AddLoopSE("BossSE03", "bs109_chop02", 0.4f);
			}
			break;
		case MainStatus.Skill4:
			if (_subStatus == SubStatus.Phase0)
			{
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				if (!Target)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					TargetPos = new VInt3(Target._transform.position);
				}
				Vector3 vector5;
				if ((bool)Target)
				{
					UpdateDirection();
					EndPos = TargetPos.vec3;
					vector5 = (EndPos - _transform.position).normalized;
				}
				else
				{
					EndPos = Vector3.up * 8f;
					vector5 = Vector3.up;
				}
				float value2 = Vector3.Angle(Vector3.up, vector5);
				_animator.SetFloat(_HashAngle, value2);
				lastdistance = Vector3.Distance(_transform.position, EndPos);
				AssaultTime = SK5AssaultTime;
				AssaultFrame = (int)(AssaultTime * 20f) + GameLogicUpdateManager.GameFrame;
				_velocity = new VInt3(vector5 * 0.001f * AssaultSpeed);
				base.SoundSource.AddLoopSE("BossSE03", "bs109_chop02", 0.3f);
			}
			break;
		case MainStatus.Skill5:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 != 0)
			{
				if (subStatus2 != SubStatus.Phase1)
				{
					break;
				}
			}
			else
			{
				AtkTimes = ActionTimes;
				_subStatus = SubStatus.Phase1;
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			}
			AtkTimes--;
			int num = 0;
			try
			{
				num = int.Parse(ShowSequence[0].ToString());
				ShowSequence = ShowSequence.Remove(0, 1);
			}
			catch
			{
				num = 0;
				Debug.LogError("位置序列中，含有非數字的符號。");
			}
			_transform.position = AssaultPos[num];
			Controller.LogicPosition = new VInt3(_transform.position);
			if (!Target)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
			}
			Vector3 vector3;
			if ((bool)Target)
			{
				TargetPos = new VInt3(Target._transform.position);
				UpdateDirection();
				EndPos = Target._transform.position;
				vector3 = (EndPos - _transform.position).normalized;
			}
			else
			{
				EndPos = Vector3.up * 8f;
				vector3 = Vector3.up;
			}
			float value = Vector3.Angle(Vector3.up, vector3);
			_animator.SetFloat(_HashAngle, value);
			lastdistance = Vector3.Distance(_transform.position, EndPos);
			AssaultTime = SK5AssaultTime;
			AssaultFrame = (int)(AssaultTime * 20f) + GameLogicUpdateManager.GameFrame;
			_velocity = new VInt3(vector3 * 0.001f * AssaultSpeed);
			break;
		}
		case MainStatus.Judgement:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				if (Vector3.Distance(_transform.position, Target._transform.position) < JudgeDis)
				{
					SetStatus(MainStatus.Skill1);
				}
				else
				{
					SetStatus(MainStatus.Skill0);
				}
			}
			else
			{
				SetStatus(MainStatus.Skill1);
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_animator.speed = 1f;
				_animator.SetLayerWeight(1, 0f);
				_velocity = VInt3.zero;
				if (_otherTexIndex != 0)
				{
					_otherTexIndex = 0;
					_characterMaterial.UpdateTex(_otherTexIndex);
				}
				break;
			case SubStatus.Phase1:
			{
				AI_STATE aiState = AiState;
				CorpsTool.GoBack();
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
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase3)
			{
				_currentAnimationId = AnimationID.ANI_DEBUT;
			}
			else
			{
				_currentAnimationId = AnimationID.ANI_IDLE;
			}
			break;
		}
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill0:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_Skill0;
				break;
			}
			return;
		case MainStatus.Skill1:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_Skill1;
				break;
			}
			return;
		case MainStatus.Skill2:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_Skill2;
				break;
			}
			return;
		case MainStatus.Skill3:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_Skill3;
				PlaySE("BossSE03", "bs109_chop02");
				_animator.SetLayerWeight(1, 1f);
				_animator.Play(_animationHash[10], 1);
				break;
			}
			return;
		case MainStatus.Skill4:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_Skill4;
				_animator.SetLayerWeight(1, 1f);
				_animator.Play(_animationHash[10], 1);
				break;
			}
			return;
		case MainStatus.Skill5:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase1)
			{
				_currentAnimationId = AnimationID.ANI_Skill5;
				_animator.SetLayerWeight(1, 1f);
				_animator.Play(_animationHash[10], 1);
				break;
			}
			return;
		}
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0)
			{
				_currentAnimationId = AnimationID.ANI_HURT;
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
				if (_introReady && IntroCallBack != null)
				{
					IntroCallBack();
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				else if (AiState == AI_STATE.mob_003)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase2:
				if (AiTimer.GetMillisecond() > 1000)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					_efx_SpotLight.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
					SetStatus(MainStatus.Debut, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase5:
				if (_unlockReady)
				{
					CorpsTool.SetDebutOver();
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (!bWaitNetStatus && GameLogicUpdateManager.GameFrame > NextStatusFrame)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
				}
				UpdateDirection();
				UpdateNextState();
			}
			break;
		case MainStatus.Skill0:
		{
			if (_subStatus != 0)
			{
				break;
			}
			float num3 = Vector3.Distance(_transform.position, EndPos);
			if (num3 > lastdistance)
			{
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					num3 = Vector3.Distance(_transform.position, Target._transform.position);
					if (num3 < JudgeDis)
					{
						AICircuit = AICircuit_1;
					}
					else
					{
						AICircuit = AICircuit_2;
					}
				}
				else
				{
					AICircuit = AICircuit_2;
				}
				if (isObedient)
				{
					SetStatus(MainStatus.Idle);
				}
				else
				{
					UpdateNextState();
				}
			}
			else
			{
				lastdistance = num3;
			}
			break;
		}
		case MainStatus.Skill1:
			if (_subStatus == SubStatus.Phase0 && _currentFrame > 0.95f)
			{
				if (isObedient)
				{
					SetStatus(MainStatus.Idle);
				}
				else
				{
					UpdateNextState();
				}
			}
			break;
		case MainStatus.Skill2:
		{
			if (_subStatus != 0)
			{
				break;
			}
			float num2 = Vector3.Distance(_transform.position, EndPos);
			if (num2 > lastdistance)
			{
				HasArrive = true;
			}
			if (_currentFrame > 0.5f && !CanStop)
			{
				CanStop = true;
			}
			if (_currentFrame > 2f && HasArrive)
			{
				if (isObedient)
				{
					SetStatus(MainStatus.Idle);
				}
				else
				{
					UpdateNextState(0);
				}
			}
			else if (GotHurt && !bWaitNetStatus && GameLogicUpdateManager.GameFrame > StopFrame)
			{
				GotHurt = false;
				TurnToward = (_currentFrame % 1f * 360f + 90f) % 360f;
				if (isObedient)
				{
					SetStatus(MainStatus.Skill3);
				}
				else
				{
					UpdateNextState();
				}
			}
			else
			{
				lastdistance = num2;
			}
			break;
		}
		case MainStatus.Skill3:
			if (_subStatus == SubStatus.Phase0 && GameLogicUpdateManager.GameFrame > AssaultFrame)
			{
				if (isObedient)
				{
					SetStatus(MainStatus.Idle);
				}
				else
				{
					UpdateNextState();
				}
			}
			break;
		case MainStatus.Skill4:
			if (_subStatus == SubStatus.Phase0 && GameLogicUpdateManager.GameFrame > AssaultFrame)
			{
				if (isObedient)
				{
					SetStatus(MainStatus.Skill5);
				}
				else
				{
					UpdateNextState();
				}
			}
			break;
		case MainStatus.Skill5:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus != SubStatus.Phase1)
			{
				break;
			}
			if (AtkTimes > 0)
			{
				if (GameLogicUpdateManager.GameFrame > AssaultFrame)
				{
					_velocity = VInt3.zero;
					distanceDelta = 0f;
					SetStatus(MainStatus.Skill5, SubStatus.Phase1);
				}
				break;
			}
			float num = Vector3.Distance(_transform.position, EndPos);
			if (num > lastdistance)
			{
				_velocity = VInt3.zero;
				if (isObedient)
				{
					base.SoundSource.RemoveLoopSE("BossSE03", "bs109_chop02");
					SetStatus(MainStatus.Idle);
				}
				else
				{
					UpdateNextState();
				}
			}
			else
			{
				lastdistance = num;
			}
			break;
		}
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (GameLogicUpdateManager.GameFrame > FlashFrame)
				{
					if (CorpsTool.isBossDead)
					{
						SetStatus(MainStatus.Die, SubStatus.Phase1);
						break;
					}
					HurtPassParam hurtPassParam = new HurtPassParam();
					base.DeadBehavior(ref hurtPassParam);
				}
				break;
			}
			break;
		}
		if (GameLogicUpdateManager.GameFrame < FlashFrame)
		{
			_otherTexIndex = ((_otherTexIndex == 0) ? 1 : 0);
			_characterMaterial.UpdateTex(_otherTexIndex);
		}
		else if (_otherTexIndex != 0)
		{
			_otherTexIndex = 0;
			_characterMaterial.UpdateTex(_otherTexIndex);
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
			GetWall = false;
			_projectorTransform.gameObject.SetActive(false);
			SwordMesh.SetActive(true);
			if (CorpsTool.Member == null)
			{
				foreach (StageUpdate.EnemyCtrlID runEnemy in StageUpdate.runEnemys)
				{
					if ((bool)runEnemy.mEnemy && runEnemy.mEnemy is BS053_Controller && !(runEnemy.mEnemy as BS053_Controller).RegistMemberBornBySync(this, Hp))
					{
						Debug.LogWarning("註冊成員失敗，請參考主體BOSS。");
					}
				}
				if (CorpsTool.Member != null)
				{
					CorpsTool.SetDebutOver();
				}
			}
			GetWall = CheckRoomSize();
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			IgnoreGravity = true;
			AICircuit = AICircuit_1;
			ObjInfoBar componentInChildren = _transform.GetComponentInChildren<ObjInfoBar>();
			if ((bool)componentInChildren)
			{
				_ObjInfoBar = componentInChildren.transform;
				_ObjInfoBar.gameObject.SetActive(false);
			}
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
		ModelTransform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * -1f);
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
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			SetColliderEnable(false);
			CorpsTool.fightState = BossCorpsTool.FightState.Dead;
			SetStatus(MainStatus.Die);
		}
	}

	private void LoadParts(Transform[] childs)
	{
		if (!ModelTransform)
		{
			ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		}
		if (!_efx_SpotLight)
		{
			_efx_SpotLight = OrangeBattleUtility.FindChildRecursive(ref childs, "efx_SpotLight", true).GetComponent<ParticleSystem>();
		}
		if (!_projectorTransform)
		{
			_projectorTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "projector", true);
		}
		if (!SwordMesh)
		{
			SwordMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS073_Mesh", true).gameObject;
		}
		if (!_collideBullet)
		{
			_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "SwordCollider").gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (!_shieldTransform)
		{
			_shieldTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "ShieldCollider", true);
		}
		if (!HiltTransform)
		{
			HiltTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "AimTransform", true);
		}
		if (!RootTransform)
		{
			RootTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "Rig", true);
		}
		if ((bool)_shieldTransform)
		{
			_shieldTransform.gameObject.AddOrGetComponent<StageObjParam>().nSubPartID = 1;
			GuardTransform.Add(1);
		}
	}

	private void ResetSkillParam()
	{
		_velocity = VInt3.zero;
	}

	private void UploadStatus(MainStatus status)
	{
		ResetSkillParam();
		if (status != 0)
		{
			if (CheckHost())
			{
				int num = 1;
				num = ((AICircuit.Length == AICircuit_1.Length) ? 1 : 2);
				switch (status)
				{
				case MainStatus.Skill3:
					UploadEnemyStatus((int)status, false, new object[1] { num }, new object[1] { Target.sNetSerialID + "," + TurnToward });
					break;
				case MainStatus.Skill4:
				{
					ShowSequence = "";
					int num2 = -1;
					for (int i = 0; i < ActionTimes; i++)
					{
						int num3 = OrangeBattleUtility.Random(0, 40) / 10;
						if (num2 == num3)
						{
							num3 = (num3 + 1) % 4;
						}
						ShowSequence += num3;
						num2 = num3;
					}
					UploadEnemyStatus((int)status, false, new object[1] { num }, new object[1] { Target.sNetSerialID + "," + ShowSequence });
					break;
				}
				default:
					UploadEnemyStatus((int)status, false, new object[1] { num }, new object[1] { Target.sNetSerialID + "," });
					break;
				}
			}
			else
			{
				bWaitNetStatus = true;
			}
		}
		else
		{
			SetStatus(MainStatus.Idle);
		}
	}

	private void UpdateNextState(bool needSync)
	{
		UpdateNextState(-1, needSync);
	}

	private void UpdateNextState(int Step = -1, bool needSync = true)
	{
		if (needSync && !bWaitNetStatus)
		{
			if (isObedient)
			{
				MissionRunTimes--;
				if (MissionRunTimes > 0)
				{
					UploadStatus(NextSkill);
					return;
				}
				int mission = -1;
				if (!CorpsTool.MissionComplete())
				{
					mission = CorpsTool.ReceiveMission();
				}
				SetMission(mission);
				UploadStatus(NextSkill);
			}
			else
			{
				if (Step == -1)
				{
					AIStep = (AIStep + 1) % AICircuit.Length;
				}
				else
				{
					AIStep = Step % AICircuit.Length;
				}
				UploadStatus(AICircuit[AIStep]);
			}
		}
		else
		{
			if (Step == -1)
			{
				AIStep = (AIStep + 1) % AICircuit.Length;
			}
			else
			{
				AIStep = Step % AICircuit.Length;
			}
			SetStatus(AICircuit[AIStep]);
		}
	}

	private bool CheckRoomSize()
	{
		Vector3 vector = Vector3.zero;
		object[] values = CorpsTool.Master.GetValues();
		if (values[0] != null)
		{
			vector = (Vector3)values[0];
		}
		float x = vector.x + 10f;
		float x2 = vector.x - 10f;
		float y = vector.y + 10f;
		float y2 = vector.y - 10f;
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.left, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.up, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		RaycastHit2D raycastHit2D4 = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.down, 20f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _transform);
		if (!raycastHit2D2 || !raycastHit2D || !raycastHit2D3 || !raycastHit2D4)
		{
			Debug.LogWarning("BS087 (素模劍、斬擊者) 需要四個方向都有牆壁，下次動作開始將會重新尋找。");
			return false;
		}
		if ((bool)raycastHit2D2)
		{
			x = raycastHit2D2.point.x + 2f;
		}
		if ((bool)raycastHit2D)
		{
			x2 = raycastHit2D.point.x - 2f;
		}
		if ((bool)raycastHit2D3)
		{
			y = raycastHit2D3.point.y + 6f;
		}
		if ((bool)raycastHit2D4)
		{
			y2 = raycastHit2D4.point.y + 0.5f;
		}
		AssaultPos[0] = new Vector3(x2, y, 0f);
		AssaultPos[1] = new Vector3(x2, y2, 0f);
		AssaultPos[2] = new Vector3(x, y, 0f);
		AssaultPos[3] = new Vector3(x, y2, 0f);
		Debug.Log("成功找到牆壁，之後程序將停止重新尋找。");
		return true;
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (_mainStatus == MainStatus.Skill2 && CanStop && !GotHurt)
		{
			_velocity = VInt3.zero;
			_animator.speed = 0f;
			GotHurt = true;
			StopFrame = (int)(StopTime * 20f) + GameLogicUpdateManager.GameFrame;
		}
		int num = Hp;
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if ((int)obscuredInt < num)
		{
			FlashFrame = (int)(FlashTime * 20f) + GameLogicUpdateManager.GameFrame;
		}
		if (CorpsTool.Member != null)
		{
			CorpsTool.ReturnHp(obscuredInt);
		}
		return obscuredInt;
	}

	protected internal void SetParam(BossCorpsTool bosscorps, int SetHp)
	{
		CorpsTool = bosscorps;
		CorpsTool.SetCanForceExecute(this);
		Hp = SetHp;
		SetStatus(MainStatus.Idle);
	}

	protected internal void SetDebut()
	{
		UpdateDirection(-1);
		SetStatus(MainStatus.Debut);
		base.AllowAutoAim = false;
	}

	public bool SetMission(int mission = -1)
	{
		switch (mission)
		{
		case -1:
			NextSkill = MainStatus.Idle;
			MissionRunTimes = 0;
			return true;
		case 0:
			NextSkill = MainStatus.Judgement;
			MissionRunTimes = 3;
			return true;
		case 1:
			NextSkill = MainStatus.Skill4;
			MissionRunTimes = 1;
			return true;
		case 2:
			NextSkill = MainStatus.Skill2;
			MissionRunTimes = 1;
			return true;
		default:
			NextSkill = MainStatus.Idle;
			return true;
		}
	}

	public bool ForceExecuteMission()
	{
		SetStatus(NextSkill);
		return true;
	}

	public bool SetStopMission()
	{
		needStop = true;
		return true;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
	}
}
