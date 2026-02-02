#define RELEASE
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class EM157_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill0 = 2,
		Skill1 = 3,
		Skill2 = 4,
		Die = 5,
		IdleWaitNet = 6
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
		ANI_Skill1_START = 2,
		ANI_Skill1_LOOP = 3,
		ANI_Skill1_END = 4,
		ANI_Skill2_START = 5,
		ANI_Skill2_LOOP = 6,
		ANI_Skill2_END = 7,
		ANI_DEAD = 8,
		MAX_ANIMATION_ID = 9
	}

	private enum SpikeAnimationID
	{
		ANI_ON = 0,
		ANI_OFF = 1,
		MAX_ID = 2
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

	private int[] _spikeanimationHash;

	private Vector3 lastpos;

	private BoxCollider2D BoxCol2D;

	[SerializeField]
	private int MoveSpeed = 1500;

	[SerializeField]
	private int BackSpeed;

	private int UpDown = 1;

	private float Distance = 5f;

	private float NextYPos;

	private float UpYPos;

	private float DownYPos;

	private float CenterXPos;

	private float OriginXPos;

	private BossCorpsTool CorpsTool;

	private MainStatus NextSkill;

	[SerializeField]
	private float WaitTime;

	private int WaitFrame;

	private Vector3 AtkVelocity = Vector3.right;

	private RaycastHit2D hitup;

	private RaycastHit2D hitdown;

	private RaycastHit2D hitright;

	private RaycastHit2D hitleft;

	private int blocklayer;

	[SerializeField]
	private GameObject HandMesh;

	[SerializeField]
	private int SwingSpeed = 6000;

	[SerializeField]
	private float OverTime = 0.5f;

	private int OverFrame;

	[SerializeField]
	private Transform ShootPos;

	[SerializeField]
	private int AtkTimes = 2;

	private bool isAtking;

	private float ShootFrame;

	private bool HasShot;

	private CollideBullet CrushCollider;

	private ParticleSystem FX_Booster_R1;

	private ParticleSystem FX_Booster_R2;

	private ParticleSystem FX_Booster_R3;

	[SerializeField]
	private ParticleSystem Thunder;

	[SerializeField]
	private ParticleSystem ShootPosFx;

	private float EndXPos;

	private CollideBullet ThunderCollide;

	private bool bChargeSE;

	private bool bThunderSE;

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
		_animationHash = new int[9];
		_animationHash[0] = Animator.StringToHash("EM157@idle_loop");
		_animationHash[2] = Animator.StringToHash("EM157@skill_1_start");
		_animationHash[3] = Animator.StringToHash("EM157@skill_1_loop");
		_animationHash[4] = Animator.StringToHash("EM157@skill_1_end");
		_animationHash[5] = Animator.StringToHash("EM157@skill_3_start");
		_animationHash[6] = Animator.StringToHash("EM157@skill_3_loop");
		_animationHash[7] = Animator.StringToHash("EM157@skill_3_end");
		_spikeanimationHash = new int[2];
		_spikeanimationHash[0] = Animator.StringToHash("EM157@spike_front_loop");
		_spikeanimationHash[1] = Animator.StringToHash("EM157@spike_up_loop");
	}

	private void LoadParts(ref Transform[] childs)
	{
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		_animator = ModelTransform.GetComponent<Animator>();
		_collideBullet = OrangeBattleUtility.FindChildRecursive(ref childs, "Collider", true).gameObject.AddOrGetComponent<CollideBullet>();
		BoxCol2D = OrangeBattleUtility.FindChildRecursive(ref childs, "BlockCollider").GetComponent<BoxCollider2D>();
		if (HandMesh == null)
		{
			HandMesh = OrangeBattleUtility.FindChildRecursive(ref childs, "BS066_HandMesh").gameObject;
		}
		if (ShootPos == null)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPos");
		}
		if (Thunder == null)
		{
			Thunder = OrangeBattleUtility.FindChildRecursive(ref childs, "Thunder").GetComponent<ParticleSystem>();
		}
		if (ThunderCollide == null)
		{
			ThunderCollide = OrangeBattleUtility.FindChildRecursive(ref childs, "ThunderCollider").gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (CrushCollider == null)
		{
			CrushCollider = OrangeBattleUtility.FindChildRecursive(ref childs, "CrushCollider").gameObject.AddOrGetComponent<CollideBullet>();
		}
		if (ShootPosFx == null)
		{
			ShootPosFx = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPosFX").GetComponent<ParticleSystem>();
		}
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref childs, "FX_Booster_R1", true);
		if (transform != null)
		{
			FX_Booster_R1 = transform.GetComponent<ParticleSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref childs, "FX_Booster_R2", true);
		if (transform != null)
		{
			FX_Booster_R2 = transform.GetComponent<ParticleSystem>();
		}
		transform = OrangeBattleUtility.FindChildRecursive(ref childs, "FX_Booster_R3", true);
		if (transform != null)
		{
			FX_Booster_R3 = transform.GetComponent<ParticleSystem>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Transform[] childs = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(ref childs);
		HashAnimation();
		_mainStatus = MainStatus.Idle;
		base.AimPoint = new Vector3(0f, 0f, 0f);
		base.AllowAutoAim = false;
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f, 20f);
		StageObjParam[] componentsInChildren = GetComponentsInChildren<StageObjParam>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].tLinkSOB = null;
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
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			_velocity = VInt3.zero;
			break;
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			_velocity.y = MoveSpeed * UpDown;
			OpenAndCheckPlayer();
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				PlaySE("BossSE03", "bs029_finsig02");
				if ((bool)Target)
				{
					AtkVelocity = (Target.Controller.GetCenterPos().xy() - (_transform.position.xy() + BoxCol2D.offset)).normalized;
				}
				else
				{
					AtkVelocity = Vector3.right * base.direction;
				}
				_collideBullet.Active(targetMask);
				_velocity = new VInt3(AtkVelocity) * SwingSpeed * 0.001f;
				if (_velocity.x * base.direction < 0)
				{
					_velocity.x = 0;
				}
				break;
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				OverFrame = GameLogicUpdateManager.GameFrame + (int)(OverTime * 20f);
				break;
			case SubStatus.Phase2:
				CorpsTool.Master.ReportObjects(new object[1] { true });
				_velocity = VInt3.right * -base.direction * BackSpeed * 0.001f;
				_collideBullet.BackToPool();
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				BoxCol2D.enabled = false;
				HandMesh.layer = ManagedSingleton<OrangeLayerManager>.Instance.RenderEnemy;
				_velocity = VInt3.zero;
				if (AtkTimes % 2 == 1)
				{
					NextYPos = UpYPos - (UpYPos - DownYPos) * 0.25f;
				}
				else
				{
					NextYPos = UpYPos - (UpYPos - DownYPos) * 0.75f;
				}
				if (_transform.position.y > NextYPos)
				{
					UpDown = -1;
				}
				else
				{
					UpDown = 1;
				}
				_velocity.y = MoveSpeed * UpDown * 2;
				break;
			case SubStatus.Phase1:
				_velocity = VInt3.zero;
				break;
			case SubStatus.Phase2:
				WaitFrame = GameLogicUpdateManager.GameFrame + (int)(WaitTime * 20f);
				break;
			case SubStatus.Phase3:
				ShootFrame = 1f;
				HasShot = false;
				CorpsTool.Master.ReportObjects(new object[1] { true });
				break;
			case SubStatus.Phase4:
				WaitFrame = GameLogicUpdateManager.GameFrame + (int)(WaitTime * 10f);
				break;
			case SubStatus.Phase5:
				HandMesh.layer = ManagedSingleton<OrangeLayerManager>.Instance.RenderSPEnemy;
				isAtking = false;
				AtkTimes = 2;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				BoxCol2D.enabled = false;
				_velocity = VInt3.zero;
				NextYPos = UpYPos;
				if (_transform.position.y > NextYPos)
				{
					UpDown = -1;
				}
				else
				{
					UpDown = 1;
				}
				_velocity.y = MoveSpeed * UpDown * 2;
				break;
			case SubStatus.Phase2:
				_velocity = VInt3.zero;
				WaitFrame = GameLogicUpdateManager.GameFrame + (int)(WaitTime * 20f);
				break;
			case SubStatus.Phase3:
				_velocity.x = MoveSpeed * base.direction * 2;
				EndXPos = _transform.position.x + Mathf.Abs(CenterXPos - OriginXPos) * (float)base.direction;
				ThunderCollide.Active(targetMask);
				PlaySE("BossSE03", "bs029_finsig03_lp");
				bThunderSE = true;
				Thunder.Play();
				CorpsTool.Master.ReportObjects(new object[1] { true });
				break;
			case SubStatus.Phase4:
				_velocity = VInt3.zero;
				ThunderCollide.BackToPool();
				PlaySE("BossSE03", "bs029_finsig03_stop");
				bThunderSE = false;
				Thunder.Stop();
				WaitFrame = GameLogicUpdateManager.GameFrame + (int)(WaitTime * 10f);
				break;
			case SubStatus.Phase6:
				_velocity.x = -MoveSpeed * base.direction * 2;
				break;
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
	}

	private void UpdateNextState(int Step = -1, bool needSync = true)
	{
		if (needSync && !bWaitNetStatus)
		{
			int mission = -1;
			if (!CorpsTool.MissionComplete())
			{
				mission = CorpsTool.ReceiveMission();
			}
			SetMission(mission);
			UploadStatus(NextSkill);
		}
	}

	public bool SetMission(int mission = -1)
	{
		switch (mission)
		{
		case -1:
			NextSkill = MainStatus.Idle;
			return true;
		case 0:
			NextSkill = MainStatus.Skill0;
			return true;
		case 1:
			NextSkill = MainStatus.Skill1;
			return true;
		case 2:
			NextSkill = MainStatus.Skill2;
			return false;
		default:
			NextSkill = MainStatus.Idle;
			return true;
		}
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Debut:
			_currentAnimationId = AnimationID.ANI_IDLE;
			_animator.Play(_spikeanimationHash[1], 1);
			break;
		case MainStatus.Skill0:
			_currentAnimationId = AnimationID.ANI_IDLE;
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_animator.Play(_spikeanimationHash[0], 1);
				break;
			case SubStatus.Phase2:
				_animator.Play(_spikeanimationHash[1], 1);
				break;
			}
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				if (isAtking)
				{
					return;
				}
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
				if (isAtking)
				{
					return;
				}
				_currentAnimationId = AnimationID.ANI_Skill1_START;
				break;
			case SubStatus.Phase2:
				if (isAtking)
				{
					return;
				}
				_currentAnimationId = AnimationID.ANI_Skill1_LOOP;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_Skill1_END;
				break;
			case SubStatus.Phase3:
			case SubStatus.Phase4:
				return;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill2_START;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill2_LOOP;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill2_END;
				break;
			case SubStatus.Phase3:
				return;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			case SubStatus.Phase1:
				return;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UploadStatus(MainStatus status)
	{
		if (status != 0)
		{
			if (CheckHost())
			{
				UploadEnemyStatus((int)status);
			}
		}
		else
		{
			SetStatus(MainStatus.Idle);
		}
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
			if ((_transform.position.y < NextYPos && UpDown == -1) || (_transform.position.y > NextYPos && UpDown == 1))
			{
				UpDown *= -1;
				NextYPos = ((UpDown == 1) ? UpYPos : DownYPos);
				SetStatus(MainStatus.Idle);
			}
			if (!bWaitNetStatus && !CorpsTool.MissionComplete())
			{
				UpdateNextState();
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((_transform.position.x - (CenterXPos - (float)base.direction * 2f)) * (float)base.direction > 0f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				else if (_transform.position.y > UpYPos + 1f || _transform.position.y < DownYPos - 1f)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (OverFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Skill0, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((_transform.position.x - OriginXPos) * (float)base.direction < 0f)
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
				if ((_transform.position.y < NextYPos && UpDown == -1) || (_transform.position.y > NextYPos && UpDown == 1))
				{
					UpDown = base.direction;
					NextYPos = ((UpDown == 1) ? UpYPos : DownYPos);
					SetStatus(MainStatus.Skill1, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase2);
					if (ShootPosFx != null)
					{
						PlaySE("BossSE03", "bs029_finsig07_lp");
						bChargeSE = true;
						ShootPosFx.Play();
					}
				}
				break;
			case SubStatus.Phase2:
				if (WaitFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill1, SubStatus.Phase4);
				}
				if (!HasShot && _currentFrame > ShootFrame)
				{
					HasShot = true;
					BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos.position + Vector3.right * base.direction, Vector3.right * base.direction, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					PlaySE("BossSE03", "bs029_finsig07_stop");
					bChargeSE = false;
				}
				break;
			case SubStatus.Phase4:
				if (WaitFrame >= GameLogicUpdateManager.GameFrame)
				{
					break;
				}
				AtkTimes--;
				if (AtkTimes > 0)
				{
					isAtking = true;
					SetStatus(MainStatus.Skill1);
					break;
				}
				if (bChargeSE)
				{
					PlaySE("BossSE03", "bs029_finsig07_stop");
				}
				bChargeSE = false;
				SetStatus(MainStatus.Skill1, SubStatus.Phase5);
				break;
			case SubStatus.Phase5:
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
				if ((_transform.position.y < NextYPos && UpDown == -1) || (_transform.position.y > NextYPos && UpDown == 1))
				{
					UpDown *= -1;
					NextYPos = ((UpDown == 1) ? UpYPos : DownYPos);
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
				if (WaitFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((_transform.position.x - EndXPos) * (float)base.direction > 0f)
				{
					_transform.position = new Vector3(EndXPos, _transform.position.y, _transform.position.z);
					Controller.LogicPosition = new VInt3(_transform.position);
					SetStatus(MainStatus.Skill2, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (WaitFrame < GameLogicUpdateManager.GameFrame)
				{
					SetStatus(MainStatus.Skill2, SubStatus.Phase6);
				}
				break;
			case SubStatus.Phase6:
				if ((_transform.position.x - OriginXPos) * (float)base.direction < 0f)
				{
					_transform.position = new Vector3(OriginXPos, _transform.position.y, _transform.position.z);
					Controller.LogicPosition = new VInt3(_transform.position);
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Debut:
		case MainStatus.Die:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			_transform.localPosition = Vector3.MoveTowards(base.transform.position, Controller.LogicPosition.vec3, distanceDelta);
			MainStatus mainStatus = _mainStatus;
			if (mainStatus == MainStatus.Idle || mainStatus == MainStatus.Skill0)
			{
				MovePlayer();
			}
			lastpos = _transform.position;
		}
	}

	public override void SetActive(bool isActive)
	{
		base.AllowAutoAim = false;
		base.SetActive(isActive);
		if (isActive)
		{
			if (FX_Booster_R1 != null && !FX_Booster_R1.isPlaying)
			{
				FX_Booster_R1.Play();
				FX_Booster_R2.Play();
				FX_Booster_R3.Play();
			}
			IsInvincible = true;
			IgnoreGravity = true;
			BoxCol2D.enabled = true;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			ThunderCollide.UpdateBulletData(EnemyWeapons[2].BulletData);
			ThunderCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			CrushCollider.UpdateBulletData(EnemyWeapons[0].BulletData);
			CrushCollider.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			CrushCollider.HitCallback = CrushCallBack;
			SetMoveDistance();
			SetStatus(MainStatus.Debut);
		}
		else
		{
			if (bChargeSE)
			{
				PlaySE("BossSE03", "bs029_finsig07_stop");
			}
			if (bThunderSE)
			{
				PlaySE("BossSE03", "bs029_finsig03_stop");
			}
		}
	}

	private void SetMoveDistance()
	{
		blocklayer = (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer);
		RaycastHit2D raycastHit2D = OrangeBattleUtility.RaycastIgnoreSelf(_transform.position, Vector2.down, float.PositiveInfinity, blocklayer, _transform);
		RaycastHit2D raycastHit2D2 = OrangeBattleUtility.RaycastIgnoreSelf(_transform.position, Vector2.up, float.PositiveInfinity, blocklayer, _transform);
		if (!raycastHit2D || !raycastHit2D2)
		{
			Debug.LogError("上面沒找到? " + raycastHit2D2.transform.name + " ----- " + base.name);
			Debug.LogError("下面沒找到? " + raycastHit2D.transform.name + " ----- " + base.name);
			Debug.LogError("場景需要有上下擋牆來瘸定上下移動距離與目前的點。目前位置" + _transform.position.ToString());
			UpYPos = _transform.position.y + 2.4f;
			DownYPos = _transform.position.y - 3.6f;
			Distance = 6f;
			UpDown = base.direction;
			NextYPos = ((UpDown == 1) ? UpYPos : DownYPos);
			OriginXPos = ModelTransform.position.x;
		}
		else
		{
			float distance = raycastHit2D2.point.y - raycastHit2D.point.y - 2f;
			UpYPos = raycastHit2D2.point.y - BoxCol2D.size.y / 2f - BoxCol2D.offset.y;
			DownYPos = raycastHit2D.point.y + BoxCol2D.size.y / 2f - BoxCol2D.offset.y;
			Distance = distance;
			UpDown = base.direction;
			NextYPos = ((UpDown == 1) ? UpYPos : DownYPos);
			OriginXPos = ModelTransform.position.x;
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, ModelTransform.localScale.z * (float)(-base.direction));
		_transform.position = pos;
		_transform.position += Vector3.forward * 1.95f;
		lastpos = _transform.position;
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
			_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(_maxGravity.i * 3));
		}
	}

	protected void MovePlayer()
	{
		for (int i = 0; i < StageUpdate.runPlayers.Count; i++)
		{
			OrangeCharacter orangeCharacter = StageUpdate.runPlayers[i];
			Vector2 zero = Vector2.zero;
			Controller2D controller = orangeCharacter.Controller;
			zero = controller.Collider2D.size;
			zero /= 2f;
			Vector3 realCenterPos = controller.GetRealCenterPos();
			Vector2 vector = new Vector2(_transform.position.x + BoxCol2D.offset.x - (BoxCol2D.size.x / 2f - 0.05f), _transform.position.y + BoxCol2D.offset.y - BoxCol2D.size.y / 2f - 0.05f);
			Vector2 vector2 = new Vector2(_transform.position.x + BoxCol2D.offset.x + BoxCol2D.size.x / 2f + 0.05f, _transform.position.y + BoxCol2D.offset.y + BoxCol2D.size.y / 2f + 0.05f);
			if (!(realCenterPos.x >= vector.x - zero.x) || !(realCenterPos.x <= vector2.x + zero.x) || !(realCenterPos.y >= vector.y - zero.y) || !(realCenterPos.y <= vector2.y + zero.y))
			{
				continue;
			}
			bool flag = false;
			float num = zero.y * 2f + BoxCol2D.size.y / 2f + 0.05f;
			float num2 = zero.x * 2f + BoxCol2D.size.x / 2f + 0.05f;
			float distance = zero.y - 0.015f;
			float distance2 = zero.x - 0.015f;
			Vector3 vector3 = _transform.position - lastpos;
			Vector3 position = orangeCharacter._transform.position;
			if (Mathf.Abs(vector3.y) > 0f)
			{
				realCenterPos += vector3;
				hitup = OrangeBattleUtility.RaycastIgnoreSelf(realCenterPos, Vector2.up, distance, blocklayer, _transform);
				hitdown = OrangeBattleUtility.RaycastIgnoreSelf(realCenterPos, Vector2.down, distance, blocklayer, _transform);
				if ((bool)hitup && orangeCharacter._transform.position.y + vector3.y + 0.05f >= _transform.position.y + BoxCol2D.size.y / 2f + BoxCol2D.offset.y && realCenterPos.x - zero.x < vector2.x && realCenterPos.x + zero.x > vector.x)
				{
					flag = true;
					vector3 += Vector3.down * (num + BoxCol2D.size.y / 2f);
				}
				if ((bool)hitdown && orangeCharacter._transform.position.y + vector3.y < _transform.position.y - BoxCol2D.size.y / 2f + BoxCol2D.offset.y && realCenterPos.x - zero.x < vector2.x && realCenterPos.x + zero.x > vector.x)
				{
					flag = true;
					vector3 += Vector3.up * (num + BoxCol2D.size.y / 2f);
				}
				if ((bool)hitdown && vector3.y < 0f)
				{
					vector3.y = 0f;
				}
				else if ((bool)hitup && vector3.y > 0f)
				{
					vector3.y = 0f;
				}
			}
			realCenterPos = controller.GetRealCenterPos();
			if (Mathf.Abs(vector3.x) > 0f)
			{
				hitright = OrangeBattleUtility.RaycastIgnoreSelf(realCenterPos + vector3, Vector2.right, distance2, blocklayer, _transform);
				hitleft = OrangeBattleUtility.RaycastIgnoreSelf(realCenterPos + vector3, Vector2.left, distance2, blocklayer, _transform);
				if ((bool)hitleft && vector3.x < 0f)
				{
					vector3.x = 0f;
				}
				else if ((bool)hitright && vector3.x > 0f)
				{
					vector3.x = 0f;
				}
				realCenterPos += vector3;
				if ((bool)hitright && realCenterPos.y + zero.y - 0.065f > vector.y && realCenterPos.y - zero.y + 0.065f < vector2.y)
				{
					Vector3 vector4 = Vector3.up * (_transform.position.y + BoxCol2D.offset.y + BoxCol2D.size.y / 2f - position.y + 0.05f);
					Vector3 vector5 = Vector3.down * (position.y + zero.y * 2f - (_transform.position.y + BoxCol2D.offset.y - BoxCol2D.size.y / 2f) + 0.05f);
					hitup = OrangeBattleUtility.RaycastIgnoreSelf(realCenterPos + vector4, Vector2.up, distance, blocklayer, _transform);
					hitdown = OrangeBattleUtility.RaycastIgnoreSelf(realCenterPos + vector5, Vector2.down, distance, blocklayer, _transform);
					if (!hitup)
					{
						vector3 = vector4;
					}
					else if (!hitdown)
					{
						vector3 = vector5;
					}
					else
					{
						vector3 += Vector3.left * (num2 + BoxCol2D.size.x / 2f + 0.1f);
					}
					flag = true;
				}
				if ((bool)hitleft && realCenterPos.y + zero.y - 0.065f > vector.y && realCenterPos.y - zero.y + 0.065f < vector2.y)
				{
					Vector3 vector6 = Vector3.up * (_transform.position.y + BoxCol2D.offset.y + BoxCol2D.size.y / 2f - position.y + 0.05f);
					Vector3 vector7 = Vector3.down * (position.y - (_transform.position.y - BoxCol2D.size.y / 2f) + zero.y * 2f + BoxCol2D.offset.y + 0.05f);
					hitup = OrangeBattleUtility.RaycastIgnoreSelf(realCenterPos + vector6, Vector2.up, distance, blocklayer, _transform);
					hitdown = OrangeBattleUtility.RaycastIgnoreSelf(realCenterPos + vector7, Vector2.down, distance, blocklayer, _transform);
					if (!hitup)
					{
						vector3 = vector6;
					}
					else if (!hitdown)
					{
						vector3 = vector7;
					}
					else
					{
						vector3 += Vector3.right * (num2 + (BoxCol2D.size.x / 2f + 0.1f));
					}
					flag = true;
				}
			}
			orangeCharacter._transform.position += vector3;
			controller.LogicPosition = new VInt3(controller.LogicPosition.vec3 + vector3);
			if (flag)
			{
				OpenCrush(controller.GetRealCenterPos());
			}
		}
	}

	public void SetCenterPos(float CenterX)
	{
		CenterXPos = CenterX;
	}

	public void JoinCorps(BossCorpsTool corps)
	{
		CorpsTool = corps;
	}

	public void SetDead()
	{
		SetStatus(MainStatus.Die);
	}

	private void OpenAndCheckPlayer()
	{
		Vector2 vector = _transform.position.xy() + BoxCol2D.offset;
		BoxCol2D.enabled = true;
		Collider2D collider2D = Physics2D.OverlapBox(vector, BoxCol2D.size * 0.95f, 0f, LayerMask.GetMask("Player"));
		if ((bool)collider2D)
		{
			OrangeCharacter component = collider2D.GetComponent<OrangeCharacter>();
			Vector2 zero = Vector2.zero;
			Controller2D controller = component.Controller;
			zero = controller.Collider2D.size;
			Vector3 position = component._transform.position;
			hitup = OrangeBattleUtility.RaycastIgnoreSelf(vector.xy() + Vector2.up * BoxCol2D.size.y / 2f, Vector2.up, zero.y, blocklayer, _transform);
			hitdown = OrangeBattleUtility.RaycastIgnoreSelf(vector.xy() + Vector2.down * BoxCol2D.size.y / 2f, Vector2.down, zero.y, blocklayer, _transform);
			if (!hitup)
			{
				component._transform.position += new Vector3(0.015f * (float)base.direction, vector.y - component._transform.position.y + BoxCol2D.size.y / 2f + 0.015f, 0f);
			}
			else if (!hitdown)
			{
				component._transform.position -= new Vector3(0.015f * (float)(-base.direction), vector.y - component._transform.position.y + BoxCol2D.size.y / 2f + 0.015f, 0f);
			}
			controller.LogicPosition = new VInt3(component._transform.position);
			OpenCrush(controller.GetRealCenterPos());
		}
	}

	private void OpenCrush(Vector3 point)
	{
		CrushCollider.transform.position = point;
		CrushCollider.Active(targetMask);
	}

	private void CrushCallBack(object obj)
	{
		CrushCollider.BackToPool();
	}

	public void SetIdle()
	{
		SetStatus(MainStatus.Idle);
	}

	public void CloseCollider()
	{
		if ((bool)CrushCollider)
		{
			CrushCollider.BackToPool();
		}
		if ((bool)ThunderCollide)
		{
			ThunderCollide.BackToPool();
		}
		_velocity = VInt3.zero;
		_animator.speed = 0f;
		Activate = false;
		if ((bool)FX_Booster_R1)
		{
			FX_Booster_R1.Stop();
		}
		if ((bool)FX_Booster_R2)
		{
			FX_Booster_R2.Stop();
		}
		if ((bool)FX_Booster_R3)
		{
			FX_Booster_R3.Stop();
		}
		if ((bool)Thunder)
		{
			Thunder.Stop();
		}
		if ((bool)ShootPosFx)
		{
			ShootPosFx.Stop();
		}
	}
}
