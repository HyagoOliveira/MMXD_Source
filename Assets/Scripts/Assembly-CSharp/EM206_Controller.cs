#define RELEASE
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using UnityEngine;

public class EM206_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Skill0 = 1,
		Skill1 = 2,
		Skill2 = 3,
		Skill3 = 4,
		Skill4 = 5,
		Skill5 = 6,
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

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private Vector3 LastTargetPos = new Vector3(0f, 0f, 0f);

	private Transform _ObjInfoBar;

	[SerializeField]
	private float MoveSpeed = 4500f;

	[SerializeField]
	private float AssaultSpeed = 9000f;

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
	private float NextStatusTime = 1f;

	private int NextStatusFrame;

	private float AssaultTime;

	[SerializeField]
	private float UpWallDis = 15f;

	[SerializeField]
	private float JudgeDis = 3f;

	private float TurnToward;

	private bool GotHurt;

	private bool HasArrive;

	private bool CanStop;

	[SerializeField]
	private float StopTime = 0.5f;

	private int StopFrame;

	[SerializeField]
	private float SK3AssaultTime = 0.8f;

	private int AssaultFrame;

	[SerializeField]
	private float SK5AssaultTime = 2.5f;

	private int AtkTimes = 3;

	private Vector3[] AssaultPos = new Vector3[4];

	private readonly int _HashAngle = Animator.StringToHash("angle");

	[SerializeField]
	private int ActionTimes = 3;

	private string ShowSequence = "222";

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

	private MainStatus[] AICircuit_2 = new MainStatus[9]
	{
		MainStatus.Idle,
		MainStatus.Skill0,
		MainStatus.Idle,
		MainStatus.Skill4,
		MainStatus.Idle,
		MainStatus.Skill5,
		MainStatus.Idle,
		MainStatus.Skill2,
		MainStatus.Skill3
	};

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
		AiTimer.TimerStart();
		FallDownSE = new string[2] { "BattleSE", "bt_ridearmor02" };
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
		base.SoundSource.RemoveLoopSE("BossSE03", "bs109_chop02");
		switch (_mainStatus)
		{
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
				Vector3 vector4;
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target._transform.position);
					UpdateDirection();
					EndPos = Target._transform.position;
					vector4 = (EndPos - _transform.position).normalized;
				}
				else
				{
					EndPos = Vector3.up * 8f;
					vector4 = Vector3.up;
				}
				Vector3.Angle(Vector3.right * base.direction, vector4);
				lastdistance = Vector3.Distance(_transform.position, EndPos);
				_velocity = new VInt3(vector4 * 0.001f * MoveSpeed);
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
				Target = _enemyAutoAimSystem.GetClosetPlayer();
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
				lastdistance = Vector3.Distance(_transform.position, EndPos);
				_velocity = new VInt3(vector3 * 0.001f * MoveSpeed);
			}
			break;
		case MainStatus.Skill3:
			if (_subStatus == SubStatus.Phase0)
			{
				_animator.speed = 1f;
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				Vector3 vector5 = Quaternion.Euler(0f, 0f, (float)(-base.direction) * TurnToward) * Vector3.up;
				if (TurnToward > 180f)
				{
					TurnToward = 360f - TurnToward;
					UpdateDirection(-base.direction);
				}
				_animator.SetFloat(_HashAngle, TurnToward);
				AssaultTime = SK3AssaultTime;
				AssaultFrame = (int)(AssaultTime * 20f) + GameLogicUpdateManager.GameFrame;
				_velocity = new VInt3(vector5 * 0.001f * AssaultSpeed);
				base.SoundSource.AddLoopSE("BossSE03", "bs109_chop02", 0.4f);
			}
			break;
		case MainStatus.Skill4:
			if (_subStatus == SubStatus.Phase0)
			{
				_collideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
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
				float value2 = Vector3.Angle(Vector3.up, vector2);
				_animator.SetFloat(_HashAngle, value2);
				lastdistance = Vector3.Distance(_transform.position, EndPos);
				AssaultTime = SK5AssaultTime;
				AssaultFrame = (int)(AssaultTime * 20f) + GameLogicUpdateManager.GameFrame;
				_velocity = new VInt3(vector2 * 0.001f * AssaultSpeed);
				base.SoundSource.AddLoopSE("BossSE03", "bs109_chop02", 0.4f);
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
			base.SoundSource.AddLoopSE("BossSE03", "bs109_chop02", 0.4f);
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
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			Vector3 vector;
			if ((bool)Target)
			{
				TargetPos = new VInt3(Target._transform.position);
				UpdateDirection();
				EndPos = Target._transform.position;
				vector = (EndPos - _transform.position).normalized;
			}
			else
			{
				EndPos = Vector3.up * 8f;
				vector = Vector3.up;
			}
			float value = Vector3.Angle(Vector3.up, vector);
			_animator.SetFloat(_HashAngle, value);
			lastdistance = Vector3.Distance(_transform.position, EndPos);
			AssaultTime = SK5AssaultTime;
			AssaultFrame = (int)(AssaultTime * 20f) + GameLogicUpdateManager.GameFrame;
			_velocity = new VInt3(vector * 0.001f * AssaultSpeed);
			break;
		}
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
				AI_STATE aI_STATE = aiState - 1;
				int num2 = 1;
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
				PlaySE("BossSE03", "bs109_chop02");
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
				PlaySE("BossSE03", "bs109_chop02");
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
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		_currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (_mainStatus)
		{
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
				UpdateNextState();
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
				UpdateNextState();
			}
			break;
		case MainStatus.Skill2:
			if (_subStatus == SubStatus.Phase0)
			{
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
					UpdateNextState(0);
				}
				else if (GotHurt && !bWaitNetStatus && GameLogicUpdateManager.GameFrame > StopFrame)
				{
					GotHurt = false;
					TurnToward = (_currentFrame % 1f * 360f + 90f) % 360f;
					UpdateNextState(false);
				}
				else
				{
					lastdistance = num2;
				}
			}
			break;
		case MainStatus.Skill3:
			if (_subStatus == SubStatus.Phase0 && GameLogicUpdateManager.GameFrame > AssaultFrame)
			{
				UpdateNextState();
			}
			break;
		case MainStatus.Skill4:
			if (_subStatus == SubStatus.Phase0 && GameLogicUpdateManager.GameFrame > AssaultFrame)
			{
				UpdateNextState();
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
			}
			else
			{
				float num = Vector3.Distance(_transform.position, EndPos);
				if (num > lastdistance)
				{
					_velocity = VInt3.zero;
					UpdateNextState();
				}
				else
				{
					lastdistance = num;
				}
			}
			break;
		}
		case MainStatus.Die:
			if (_subStatus != 0)
			{
				int num4 = 1;
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			CheckRoomSize();
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			IgnoreGravity = true;
			AICircuit = AICircuit_1;
			SetStatus(MainStatus.Idle);
		}
		else
		{
			AI_STATE aiState = AiState;
			if (aiState == AI_STATE.mob_003)
			{
				BattleInfoUI.Instance.SwitchOptionBtn(true);
			}
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
		_transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y, _transform.localScale.z * -1f);
		_transform.position = pos;
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
		base.DeadBehavior(ref tHurtPassParam);
	}

	private void LoadParts(Transform[] childs)
	{
		if (!ModelTransform)
		{
			ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		}
		if (!_collideBullet)
		{
			_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "SwordCollider").gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (!HiltTransform)
		{
			HiltTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "AimTransform", true);
		}
		if (!RootTransform)
		{
			RootTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "Rig", true);
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
			if (!CheckHost())
			{
				return;
			}
			if (status == MainStatus.Skill5)
			{
				ShowSequence = "";
				int num = -1;
				for (int i = 0; i < ActionTimes; i++)
				{
					int num2 = OrangeBattleUtility.Random(0, 40) / 10;
					if (num == num2)
					{
						num2 = (num2 + 1) % 4;
					}
					ShowSequence += num2;
					num = num2;
				}
				UploadEnemyStatus((int)status, false, null, new object[1] { ShowSequence });
			}
			else
			{
				UploadEnemyStatus((int)status);
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
			if (DebugMode)
			{
				UploadStatus(NextSkill);
				return;
			}
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

	private void CheckRoomSize()
	{
		int layerMask = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockPlayerLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.WallKickMask);
		float x = _transform.position.x + 10f;
		float x2 = _transform.position.x - 10f;
		float y = _transform.position.y + 10f;
		float y2 = _transform.position.y - 10f;
		float distance = 40f;
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.left, distance, layerMask, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right, distance, layerMask, _transform);
		RaycastHit2D raycastHit2D3 = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.up, distance, layerMask, _transform);
		RaycastHit2D raycastHit2D4 = OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.down, distance, layerMask, _transform);
		if (!raycastHit2D2 || !raycastHit2D || !raycastHit2D4)
		{
			Debug.LogError("BS073 (素模劍、斬擊者) 需要四個方向都有牆壁");
			return;
		}
		if (!raycastHit2D3)
		{
			y = raycastHit2D4.point.y + UpWallDis;
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
			y = raycastHit2D3.point.y + 0.5f;
		}
		if ((bool)raycastHit2D4)
		{
			y2 = raycastHit2D4.point.y + 0.5f;
		}
		AssaultPos[0] = new Vector3(x2, y, 0f);
		AssaultPos[1] = new Vector3(x2, y2, 0f);
		AssaultPos[2] = new Vector3(x, y, 0f);
		AssaultPos[3] = new Vector3(x, y2, 0f);
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
		int num = (int)Hp;
		return base.Hurt(tHurtPassParam);
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_002:
			base.DeadPlayCompleted = false;
			break;
		case AI_STATE.mob_003:
			BattleInfoUI.Instance.IsBossAppear = true;
			base.DeadPlayCompleted = false;
			break;
		default:
			base.DeadPlayCompleted = true;
			break;
		}
	}
}
