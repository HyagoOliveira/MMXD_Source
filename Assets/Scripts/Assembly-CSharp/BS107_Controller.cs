using System;
using System.Collections.Generic;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS107_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum EyeStatus
	{
		Open = 0,
		Opening = 1,
		Close = 2,
		Closing = 3,
		MAX_EYE_STATUS = 4
	}

	private enum MoveFoward
	{
		Up = 0,
		Right = 1,
		Down = 2,
		Left = 3
	}

	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Appear = 2,
		Moving = 3,
		Skill1 = 4,
		Skill2 = 5,
		Skill3 = 6,
		Skill0 = 7,
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
		Phase7 = 7,
		MAX_SUBSTATUS = 8
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_MOVE_UP = 2,
		ANI_MOVE_LEFT = 3,
		ANI_MOVE_DOWN = 4,
		ANI_MOVE_RIGHT = 5,
		ANI_Skill0_PREPARE_START = 6,
		ANI_Skill0_PREPARE_LOOP = 7,
		ANI_Skill0_ATK = 8,
		ANI_Skill0_ATK_LINE = 9,
		ANI_Skill0_TO_GROUND = 10,
		ANI_Skill0_ON_GROUND = 11,
		ANI_Skill0_BACK = 12,
		ANI_Skill0_END = 13,
		ANI_Skill1_START = 14,
		ANI_Skill1_LOOP = 15,
		ANI_Skill1_END = 16,
		ANI_HURT = 17,
		ANI_DEAD_START = 18,
		ANI_DEAD_LOOP = 19,
		ANI_DEAD_END = 20,
		MAX_ANIMATION_ID = 21
	}

	[SerializeField]
	private MainStatus _mainStatus;

	[SerializeField]
	private SubStatus _subStatus;

	[SerializeField]
	private AnimationID _currentAnimationId;

	[SerializeField]
	private float _currentFrame;

	private EyeStatus _eyestatus = EyeStatus.Close;

	private bool _isEyeOpen;

	private float _currentEyeFrame;

	private int[] _animationHash;

	private int[] _eyeAnimationHash;

	private int[] DefaultSkillCard = new int[6] { 0, 0, 1, 2, 3, 3 };

	private List<int> SkillCard = new List<int>();

	private int[] CobwebWeightArray = new int[6] { 1, 1, 2, 2, 1, 1 };

	private Transform EyeTransform;

	[SerializeField]
	private MoveFoward moveto;

	private int CobwebPattern;

	private int updownspeed = 8000;

	private int leftrightspeed = 10000;

	private int PillarNum = 5;

	private int CourseNum = 6;

	private bool SpeedUp;

	private BoxCollider2D BulletBox;

	private Vector3 LeftTop;

	private float X_Distance = 2.4f;

	private float Y_Distance = 1f;

	private Transform LeftTopTransform;

	private VInt2 NextNode = new VInt2(0, 0);

	private Vector3 NextNodePos;

	private bool[,] CobwebPosition = new bool[6, 5];

	private bool isAppear;

	private bool bAppearVoice;

	private float fMoveUpLastPosY;

	public GameObject PillarBlock;

	[SerializeField]
	private int Skill3ShootTimes = 3;

	private int ActionTimes;

	private bool HasActed;

	private bool CanSummon;

	[SerializeField]
	private float SummonTime = 20f;

	private int SummonFrame;

	private int ShootNum = 4;

	private int UseWeapon = 1;

	[SerializeField]
	private bool DebugMode;

	[SerializeField]
	private MainStatus SetSkill;

	private int SummonPattern;

	[SerializeField]
	public GameObject[] RenderModes;

	private Vector3 _bornPos = Vector3.zero;

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
				SetSkill = MainStatus.Idle;
				break;
			case "Skill0":
				SetSkill = MainStatus.Skill0;
				break;
			case "Skill1":
				SetSkill = MainStatus.Skill1;
				break;
			case "Skill2":
				SetSkill = MainStatus.Skill2;
				break;
			}
		}
	}

	protected virtual void HashAnimation()
	{
		_animationHash[0] = Animator.StringToHash("BS013@idle_loop");
		_animationHash[1] = Animator.StringToHash("BS013@debut");
		_animationHash[2] = Animator.StringToHash("BS013@move_up_loop");
		_animationHash[3] = Animator.StringToHash("BS013@move_left_loop");
		_animationHash[4] = Animator.StringToHash("BS013@move_down_loop");
		_animationHash[5] = Animator.StringToHash("BS013@move_right_loop");
		_animationHash[6] = Animator.StringToHash("BS013@skill_01_prepare_start");
		_animationHash[7] = Animator.StringToHash("BS013@skill_01_prepare_loop");
		_animationHash[8] = Animator.StringToHash("BS013@skill_01_atk_start");
		_animationHash[9] = Animator.StringToHash("BS013@skill_01_atk_on_line_loop");
		_animationHash[10] = Animator.StringToHash("BS013@skill_01_atk_to_ground");
		_animationHash[11] = Animator.StringToHash("BS013@skill_01_atk_on_ground_loop");
		_animationHash[12] = Animator.StringToHash("BS013@skill_01_atk_back_from_ground");
		_animationHash[13] = Animator.StringToHash("BS013@skill_01_end");
		_animationHash[14] = Animator.StringToHash("BS013@skill_02_start");
		_animationHash[15] = Animator.StringToHash("BS013@skill_02_loop");
		_animationHash[16] = Animator.StringToHash("BS013@skill_02_end");
		_animationHash[17] = Animator.StringToHash("BS013@hurt_loop");
		_animationHash[18] = Animator.StringToHash("BS013@dead_start");
		_animationHash[19] = Animator.StringToHash("BS013@dead_loop");
		_animationHash[20] = Animator.StringToHash("BS013@dead_end");
		_eyeAnimationHash[0] = Animator.StringToHash("BS013@eye_open_loop");
		_eyeAnimationHash[1] = Animator.StringToHash("BS013@eye_opening");
		_eyeAnimationHash[2] = Animator.StringToHash("BS013@eye_close_loop");
		_eyeAnimationHash[3] = Animator.StringToHash("BS013@eye_closing");
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		EyeTransform = OrangeBattleUtility.FindChildRecursive(ref target, "BackGateA_jnt", true);
		LeftTopTransform = OrangeBattleUtility.FindChildRecursive(ref target, "LeftTop", true);
		_animationHash = new int[21];
		_eyeAnimationHash = new int[4];
		HashAnimation();
		base.AimPoint = new Vector3(0.1f, 0.8f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		AiTimer.TimerStart();
		FallDownSE = new string[2] { "BossSE02", "bs016_spider04" };
	}

	protected override void Start()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_Bospider_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_Bospider_000", 2);
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
			if (netSyncData.nParam0 != -1)
			{
				NextNode = new VInt2(netSyncData.nParam0, -1);
				NextNodePos.x = LeftTop.x + (float)NextNode.x * X_Distance;
				NextNodePos.x = LeftTop.y - (float)NextNode.y * Y_Distance;
			}
			if (nSet == 3)
			{
				if (netSyncData.sParam0 != string.Empty)
				{
					CobwebPattern = int.Parse(netSyncData.sParam0);
				}
				base.SoundSource.PlaySE("BossSE02", "bs016_spider01");
				SetCobweb(CobwebPattern);
			}
			if (netSyncData.bSetHP)
			{
				Hp = netSyncData.nHP;
			}
		}
		_velocity = VInt3.zero;
		SetStatus((MainStatus)nSet);
	}

	private void ChangeToStandVar()
	{
		_velocity.y = 0;
		_velocity.x = 0;
		IsInvincible = true;
		isAppear = false;
		bAppearVoice = false;
		_isEyeOpen = false;
		base.AllowAutoAim = false;
		BulletBox.offset = new Vector2(0f, 2f);
		BulletBox.size = new Vector2(2.5f, 2.6f);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			ChangeToStandVar();
			AiTimer.TimerStart();
			if ((int)Hp < (int)MaxHp / 2 && !SpeedUp)
			{
				SpeedUp = true;
				updownspeed = (int)((float)updownspeed * 1.5f);
				leftrightspeed = (int)((float)leftrightspeed * 1.5f);
			}
			break;
		case MainStatus.Appear:
			ChangeToStandVar();
			base.AllowAutoAim = false;
			moveto = MoveFoward.Down;
			isAppear = true;
			bAppearVoice = true;
			_velocity = VInt3.zero;
			break;
		case MainStatus.Moving:
			switch (moveto)
			{
			case MoveFoward.Up:
				_velocity.y = updownspeed;
				_velocity.x = 0;
				break;
			case MoveFoward.Right:
				_velocity.x = leftrightspeed;
				_velocity.y = 0;
				break;
			case MoveFoward.Left:
				_velocity.x = -leftrightspeed;
				_velocity.y = 0;
				break;
			case MoveFoward.Down:
			{
				_velocity.y = -updownspeed;
				_velocity.x = 0;
				if (isAppear)
				{
					break;
				}
				bool flag = false;
				for (int i = NextNode.y + 1; i < CobwebPosition.GetLength(0); i++)
				{
					if (CobwebPosition[i, NextNode.x])
					{
						flag = true;
						NextNodePos.x = LeftTop.x + X_Distance * (float)NextNode.x;
						NextNodePos.y = LeftTop.y - Y_Distance * (float)i - 0.9f;
						NextNode = new VInt2(NextNode.x, i);
						break;
					}
				}
				if (!flag && NextNode.y < CobwebPosition.GetLength(0) - 1)
				{
					NextNodePos.x = LeftTop.x + X_Distance * (float)NextNode.x;
					NextNodePos.y = LeftTop.y - (Y_Distance * (float)CobwebPosition.GetLength(0) - 1f) - 0.9f;
					NextNode = new VInt2(NextNode.x, CobwebPosition.GetLength(0) - 1);
				}
				break;
			}
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ChangeToStandVar();
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase2:
				IgnoreGravity = false;
				break;
			case SubStatus.Phase4:
				Controller.LogicPosition.y += 100;
				break;
			case SubStatus.Phase6:
				_velocity.y += 5000;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ChangeToStandVar();
				break;
			case SubStatus.Phase1:
				ShootNum = 4;
				UseWeapon = 1;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ChangeToStandVar();
				break;
			case SubStatus.Phase1:
				ShootNum = 4;
				UseWeapon = 2;
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ActionTimes = Skill3ShootTimes;
				SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				return;
			case SubStatus.Phase1:
				HasActed = false;
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				StageUpdate.SlowStage();
				SetColliderEnable(false);
				_velocity.y = 0;
				base.AllowAutoAim = false;
				_velocity.x = 0;
				OrangeBattleUtility.LockPlayer();
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
		if (_transform.localScale.z != -1f)
		{
			base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, -1f);
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
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Moving:
			switch (moveto)
			{
			case MoveFoward.Up:
				PlaySE("BossSE02", "bs016_spider02");
				_currentAnimationId = AnimationID.ANI_MOVE_UP;
				fMoveUpLastPosY = _transform.position.y;
				break;
			case MoveFoward.Right:
				PlaySE("BossSE02", "bs016_spider03");
				_currentAnimationId = AnimationID.ANI_MOVE_RIGHT;
				break;
			case MoveFoward.Left:
				PlaySE("BossSE02", "bs016_spider03");
				_currentAnimationId = AnimationID.ANI_MOVE_LEFT;
				break;
			case MoveFoward.Down:
				if (!isAppear)
				{
					PlaySE("BossSE02", "bs016_spider02");
				}
				_currentAnimationId = AnimationID.ANI_MOVE_DOWN;
				break;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill0_PREPARE_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill0_PREPARE_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill0_ATK;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill0_ATK_LINE;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill0_TO_GROUND;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_Skill0_ON_GROUND;
				break;
			case SubStatus.Phase6:
				_currentAnimationId = AnimationID.ANI_Skill0_BACK;
				break;
			case SubStatus.Phase7:
				_currentAnimationId = AnimationID.ANI_Skill0_END;
				break;
			}
			break;
		case MainStatus.Skill1:
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_Skill1_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill1_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill1_END;
				break;
			}
			break;
		case MainStatus.Skill3:
		{
			SubStatus subStatus = _subStatus;
			if (subStatus == SubStatus.Phase1)
			{
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			return;
		}
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEAD_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEAD_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEAD_END;
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		if (_mainStatus == MainStatus.Idle)
		{
			mainStatus = (MainStatus)RandomCard(3);
		}
		if (mainStatus == MainStatus.Moving)
		{
			moveto = MoveFoward.Down;
			CobwebPattern = WeightRandom(CobwebWeightArray, 0);
		}
		if (mainStatus != 0)
		{
			UploadEnemyStatus((int)mainStatus, false, new object[1] { -1 }, new object[1] { CobwebPattern.ToString() });
			_velocity = VInt3.zero;
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
					Controller.LogicPosition = (VInt3)new Vector3(LeftTop.x + 2f * X_Distance, LeftTop.y - 2f * Y_Distance, 0f);
					_transform.position = new Vector3(_transform.position.x, _transform.position.y - 8f, 0f);
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (IntroCallBack != null)
				{
					IntroCallBack();
					SetStatus(MainStatus.Debut, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_unlockReady)
				{
					moveto = MoveFoward.Up;
					SetStatus(MainStatus.Moving);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CLOSE_FX);
			if (AiTimer.GetMillisecond() > 500 && CheckHost())
			{
				AiTimer.TimerStop();
				UpdateRandomState();
			}
			break;
		case MainStatus.Appear:
			SetStatus(MainStatus.Moving);
			break;
		case MainStatus.Moving:
			switch (moveto)
			{
			case MoveFoward.Up:
				if (_transform.position.y < LeftTop.y + 2f && _transform.position.y - fMoveUpLastPosY >= 3f)
				{
					PlaySE("BossSE02", "bs016_spider02");
					fMoveUpLastPosY += 3f;
				}
				if (_transform.position.y > LeftTop.y + 5f)
				{
					SetNextLine();
				}
				break;
			case MoveFoward.Right:
				if (_transform.position.x > NextNodePos.x)
				{
					Controller.LogicPosition.x = (int)(NextNodePos.x * 1000f);
					if (NextNode.y >= CobwebPosition.GetLength(0) - 1)
					{
						SetStatus(MainStatus.Skill0);
						break;
					}
					moveto = MoveFoward.Down;
					SetStatus(MainStatus.Moving);
				}
				break;
			case MoveFoward.Left:
				if (_transform.position.x < NextNodePos.x)
				{
					Controller.LogicPosition.x = (int)(NextNodePos.x * 1000f);
					if (NextNode.y >= CobwebPosition.GetLength(0) - 1)
					{
						SetStatus(MainStatus.Skill0);
						break;
					}
					moveto = MoveFoward.Down;
					SetStatus(MainStatus.Moving);
				}
				break;
			case MoveFoward.Down:
				if (isAppear)
				{
					if (bAppearVoice && (double)_transform.position.y <= (double)LeftTop.y + 2.0)
					{
						PlaySE("BossSE02", "bs016_spider02");
						bAppearVoice = false;
					}
					if (_transform.position.y <= LeftTop.y)
					{
						Controller.LogicPosition.y = (int)(LeftTop.y * 1000f);
						SetStatus(MainStatus.Idle);
					}
				}
				else
				{
					if (!(_transform.position.y <= NextNodePos.y))
					{
						break;
					}
					if (!CobwebPosition[NextNode.y, NextNode.x])
					{
						SetStatus(MainStatus.Skill0);
						break;
					}
					int num3 = 0;
					VInt2 nextNode = NextNode;
					Controller.LogicPosition.y = (int)(NextNodePos.y * 1000f);
					for (int i = 0; i <= NextNode.x; i++)
					{
						if (CobwebPosition[NextNode.y, i])
						{
							num3++;
						}
					}
					if (num3 % 2 == 0)
					{
						NextNode.x--;
						NextNodePos.x -= X_Distance;
						moveto = MoveFoward.Left;
						SetStatus(MainStatus.Moving);
					}
					else
					{
						NextNode.x++;
						NextNodePos.x += X_Distance;
						moveto = MoveFoward.Right;
						SetStatus(MainStatus.Moving);
					}
				}
				break;
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
				if (Controller.Collisions.below)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CLOSE_FX);
					_isEyeOpen = true;
					base.AllowAutoAim = true;
					IsInvincible = false;
					BulletBox.offset = new Vector2(0f, 1f);
					BulletBox.size = new Vector2(4.3f, 1.5f);
					SetStatus(MainStatus.Skill0, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 25f)
				{
					_isEyeOpen = false;
					base.AllowAutoAim = false;
					IsInvincible = true;
					BulletBox.offset = new Vector2(0f, 2f);
					BulletBox.size = new Vector2(2.5f, 2.6f);
					SetStatus(MainStatus.Skill0, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase7);
				}
				break;
			case SubStatus.Phase7:
				if (_currentFrame > 1f)
				{
					IgnoreGravity = true;
					moveto = MoveFoward.Up;
					SetStatus(MainStatus.Moving);
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
				else
				{
					if (!(_currentFrame > 0.5f) || ShootNum <= 0)
					{
						break;
					}
					EM190_Controller[] array2 = new EM190_Controller[4];
					for (int num2 = ShootNum - 1; num2 >= 0; num2--)
					{
						ShootNum--;
						EnemyControllerBase enemyControllerBase2 = StageUpdate.StageSpawnEnemyByMob(ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[UseWeapon].BulletData.f_EFFECT_X], sNetSerialID + num2, 16);
						if ((bool)enemyControllerBase2)
						{
							array2[num2] = enemyControllerBase2.gameObject.GetComponent<EM190_Controller>();
							if ((bool)array2[num2])
							{
								Vector3 pos2 = new Vector3(_transform.position.x, _transform.position.y + 3f, 0f);
								array2[num2].SetPositionAndRotation(pos2, base.direction == -1);
								array2[num2].SetParent(this);
								array2[num2].SetActive(true);
								base.SoundSource.PlaySE("BossSE02", "bs016_spider04");
							}
						}
					}
					array2[0].SetParameter(-1, -1, new VInt3(-800, 6000, 0));
					array2[1].SetParameter(-1, 1, new VInt3(-400, 8000, 0));
					array2[2].SetParameter(1, -1, new VInt3(800, 6000, 0));
					array2[3].SetParameter(1, 1, new VInt3(600, 7000, 0));
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					moveto = MoveFoward.Up;
					SetStatus(MainStatus.Moving);
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
				else
				{
					if (!(_currentFrame > 0.5f) || ShootNum <= 0)
					{
						break;
					}
					EM191_Controller[] array = new EM191_Controller[4];
					for (int num = ShootNum - 1; num >= 0; num--)
					{
						ShootNum--;
						EnemyControllerBase enemyControllerBase = StageUpdate.StageSpawnEnemyByMob(ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)EnemyWeapons[UseWeapon].BulletData.f_EFFECT_X], sNetSerialID + num, 16);
						if ((bool)enemyControllerBase)
						{
							array[num] = enemyControllerBase.gameObject.GetComponent<EM191_Controller>();
							if ((bool)array[num])
							{
								Vector3 pos = new Vector3(_transform.position.x, _transform.position.y + 3f, 0f);
								array[num].SetPositionAndRotation(pos, base.direction == -1);
								array[num].SetParent(this);
								array[num].SetActive(true);
								base.SoundSource.PlaySE("BossSE02", "bs016_spider04");
							}
						}
					}
					array[0].SetParameter(-1, -1, new VInt3(-800, 6000, 0));
					array[1].SetParameter(-1, 1, new VInt3(-400, 8000, 0));
					array[2].SetParameter(1, -1, new VInt3(800, 6000, 0));
					array[3].SetParameter(1, 1, new VInt3(600, 7000, 0));
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					moveto = MoveFoward.Up;
					SetStatus(MainStatus.Moving);
				}
				break;
			}
			break;
		case MainStatus.Skill3:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Skill3, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if (!HasActed && _currentFrame > 0.2f)
				{
					HasActed = true;
					Vector3 pDirection = GetTargetPos() - _transform.position;
					BulletBase.TryShotBullet(EnemyWeapons[3].BulletData, _transform.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask)._transform.localScale *= 2f;
				}
				if (_currentFrame > 0.4f)
				{
					if (--ActionTimes > 0)
					{
						SetStatus(MainStatus.Skill3, SubStatus.Phase1);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0 && _currentFrame > 0.5f)
			{
				SetStatus(MainStatus.Die, SubStatus.Phase1);
			}
			break;
		}
		UpdateEyeCtrl();
		if (CanSummon && GameLogicUpdateManager.GameFrame > SummonFrame)
		{
			if (SummonPattern == 0)
			{
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
			}
			else
			{
				MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform, 998);
			}
			SummonPattern++;
			SummonFrame = GameLogicUpdateManager.GameFrame + (int)(SummonTime * 20f);
		}
	}

	private void SetCobweb(int cobweb)
	{
		switch (cobweb)
		{
		case 0:
			CobwebPosition = new bool[6, 5]
			{
				{ true, true, false, true, true },
				{ false, true, true, false, false },
				{ false, false, true, true, false },
				{ true, true, true, true, false },
				{ false, true, true, false, false },
				{ false, false, false, true, true }
			};
			break;
		case 1:
			CobwebPosition = new bool[6, 5]
			{
				{ false, true, true, false, false },
				{ false, false, false, true, true },
				{ true, true, true, true, false },
				{ false, true, true, true, true },
				{ false, false, true, true, false },
				{ false, true, true, false, false }
			};
			break;
		case 2:
			CobwebPosition = new bool[6, 5]
			{
				{ false, false, true, true, false },
				{ true, true, true, true, false },
				{ false, false, false, true, true },
				{ true, true, false, true, true },
				{ false, true, true, true, true },
				{ true, true, true, true, false }
			};
			break;
		case 3:
			CobwebPosition = new bool[6, 5]
			{
				{ false, true, true, true, true },
				{ true, true, false, false, false },
				{ false, false, true, true, false },
				{ true, true, false, true, true },
				{ false, true, true, true, true },
				{ true, true, true, true, false }
			};
			break;
		case 4:
			CobwebPosition = new bool[6, 5]
			{
				{ true, true, true, true, false },
				{ false, true, true, true, true },
				{ true, true, true, true, false },
				{ false, true, true, true, true },
				{ true, true, true, true, false },
				{ false, true, true, true, true }
			};
			break;
		case 5:
			CobwebPosition = new bool[6, 5]
			{
				{ true, true, false, true, true },
				{ true, true, false, true, true },
				{ false, true, true, true, true },
				{ true, true, true, true, false },
				{ true, true, false, true, true },
				{ true, true, false, true, true }
			};
			break;
		}
		for (int i = 0; i < CobwebPosition.GetLength(0); i++)
		{
			for (int j = 0; j < CobwebPosition.GetLength(1) - 1; j++)
			{
				if (CobwebPosition[i, j] && CobwebPosition[i, j + 1])
				{
					j++;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_Bospider_000", new Vector3(LeftTop.x + (float)j * X_Distance, LeftTop.y - (float)i * Y_Distance + 1.5f, 0f), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				}
			}
		}
	}

	private void UpdateEyeCtrl()
	{
		_currentEyeFrame = _animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
		switch (_eyestatus)
		{
		case EyeStatus.Opening:
			if (_currentEyeFrame >= 1f)
			{
				_eyestatus = EyeStatus.Open;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_Bospider_000", EyeTransform, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				_animator.Play(_eyeAnimationHash[(int)_eyestatus], 1);
			}
			break;
		case EyeStatus.Open:
			if (!_isEyeOpen)
			{
				_eyestatus = EyeStatus.Closing;
				_animator.Play(_eyeAnimationHash[(int)_eyestatus], 1);
			}
			break;
		case EyeStatus.Closing:
			if (_currentEyeFrame >= 1f)
			{
				_eyestatus = EyeStatus.Close;
				_animator.Play(_eyeAnimationHash[(int)_eyestatus], 1);
			}
			break;
		case EyeStatus.Close:
			if (_isEyeOpen)
			{
				_eyestatus = EyeStatus.Opening;
				_animator.Play(_eyeAnimationHash[(int)_eyestatus], 1);
			}
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
		base.AllowAutoAim = false;
		if (isActive)
		{
			IgnoreGravity = true;
			IsInvincible = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			string gStageName = StageUpdate.gStageName;
			if (!(gStageName == "stage05_0802_e1"))
			{
				_bornPos = _transform.position;
			}
			else
			{
				_bornPos = new Vector3(168.4f, -176.3f, 0f);
			}
			LeftTop = new Vector3(_bornPos.x - X_Distance * (float)(PillarNum / 2), _bornPos.y - 6.2f, 0f);
			LeftTopTransform.position = LeftTop;
			for (int i = 0; i < PillarNum; i++)
			{
				for (int j = -1; j < CourseNum + 2; j++)
				{
					GameObject obj = UnityEngine.Object.Instantiate(PillarBlock, LeftTopTransform);
					obj.transform.SetParentNull();
					obj.transform.position = new Vector3(LeftTop.x + (float)i * X_Distance, LeftTop.y + (float)(-j) * Y_Distance + 1.5f, 0.5f);
				}
			}
			BulletBox = _collideBullet.gameObject.GetComponent<BoxCollider2D>();
			SetStatus(MainStatus.Debut);
		}
		else
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CLOSE_FX);
			_collideBullet.BackToPool();
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		AI_STATE aiState = AiState;
		if (aiState == AI_STATE.mob_002)
		{
			_bDeadCallResult = false;
		}
		else
		{
			_bDeadCallResult = true;
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
		base.transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, -1f);
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
		CanSummon = true;
		if (CanSummon)
		{
			SummonFrame = GameLogicUpdateManager.GameFrame + (int)(SummonTime * 20f);
		}
		if ((int)Hp > 0)
		{
			SetColliderEnable(true);
		}
		if (InGame)
		{
			Activate = true;
		}
	}

	private int WeightRandom(int[] WeightArray, int SkillStart)
	{
		int num = 0;
		int num2 = 0;
		int num3 = WeightArray.Length;
		for (int i = 0; i < num3; i++)
		{
			num2 += WeightArray[i];
		}
		int num4 = OrangeBattleUtility.Random(0, num2);
		for (int j = 0; j < num3; j++)
		{
			num += WeightArray[j];
			if (num4 < num)
			{
				return j + SkillStart;
			}
		}
		return 0;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			CanSummon = false;
			SetStatus(MainStatus.Die);
		}
	}

	private void SetNextLine()
	{
		_velocity = VInt3.zero;
		if (CheckHost())
		{
			int num = OrangeBattleUtility.Random(0, PillarNum);
			Controller.LogicPosition.x = (int)((LeftTop.x + X_Distance * (float)num) * 1000f);
			UploadEnemyStatus(2, false, new object[1] { num });
		}
	}

	private void SetCobwebParameter()
	{
		if (AiState == AI_STATE.mob_001)
		{
			PillarNum = 5;
			CourseNum = 6;
			Y_Distance = 1f;
			X_Distance = 2.4f;
			CobwebWeightArray = new int[6] { 1, 1, 2, 2, 1, 1 };
		}
	}

	protected override void UpdateGravity()
	{
		if (!IgnoreGravity)
		{
			if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
			{
				_velocity.y = 0;
			}
			_velocity.y += OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(_maxGravity.i));
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
		return _transform.position + Vector3.down * 3f;
	}
}
