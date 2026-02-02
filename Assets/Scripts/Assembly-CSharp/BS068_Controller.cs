#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS068_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Jump = 2,
		Rain = 3,
		Shoot = 4,
		Slide = 5,
		IceSculpture = 6,
		JumpShoot = 7,
		Dead = 8,
		Hurt = 9,
		IdleWaitNet = 10
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
		Phase8 = 8,
		Phase9 = 9,
		MAX_SUBSTATUS = 10
	}

	public enum AnimationID
	{
		ANI_DEBUT = 0,
		ANI_DEBUTA = 1,
		ANI_IDLE = 2,
		ANI_JUMP_START = 3,
		ANI_JUMP_LOOP = 4,
		ANI_JUMP_END = 5,
		ANI_SHOOT_BEGIN = 6,
		ANI_SHOOT_END = 7,
		ANI_SLIDE_BEGIN = 8,
		ANI_SLIDE_LOOP = 9,
		ANI_SLIDE_END = 10,
		ANI_RAIN_BEGIN = 11,
		ANI_RAIN_LOOP = 12,
		ANI_RAIN_END = 13,
		ANI_HURT = 14,
		ANI_DEAD = 15,
		MAX_ANIMATION_ID = 16
	}

	[SerializeField]
	private int moveSpeed = 8;

	[SerializeField]
	private int dashSpeed = 15;

	[SerializeField]
	private int jumpSpeed = 22;

	[Header(" --- Component Setup --- ")]
	[SerializeField]
	private Transform modelTransform;

	[SerializeField]
	private Transform aimTransform;

	[SerializeField]
	private Transform mouthTransform;

	[SerializeField]
	private Transform fixedTransform;

	[SerializeField]
	private Transform handleTransform;

	[SerializeField]
	private Transform grabPointTransform;

	[SerializeField]
	private Transform tsunamiPointTransform;

	[SerializeField]
	private int aiLevel = 2;

	private Action intoBack;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus subStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private AnimationID currentAnimationId;

	private int nDeadCount;

	private int bulletCount;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private float currentFrame;

	private int[] animationHash;

	private readonly int hashVspd = Animator.StringToHash("fVspd");

	private int[] sourceBehaviour = new int[4] { 2, 3, 4, 5 };

	private int[] BehaviourRaund = new int[6] { 0, 100, 200, 450, 700, 1000 };

	private List<int> currentBehaviourList = new List<int>();

	private int oldCommand;

	private bool isChipInfoAnim;

	private bool deadCallResult = true;

	private bool multiBoss;

	private float jumpDistance;

	private int collideBulletId = -1;

	private FxBase fxOfSlide;

	private float nowDistance;

	[SerializeField]
	private float distance = 1f;

	private float lastPosX;

	private bool PlayBossFlow;

	protected void OnEnable()
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

	protected override void Start()
	{
		base.Start();
		ModelTransform = modelTransform;
		base.AimTransform = aimTransform;
		_collideBullet = base.AimTransform.gameObject.AddOrGetComponent<CollideBullet>();
		_collideBullet.isForceSE = true;
		_collideBullet.isBossBullet = true;
		_animator = GetComponentInChildren<Animator>();
		animationHash = new int[16];
		animationHash[0] = Animator.StringToHash("BS068@debut");
		animationHash[1] = Animator.StringToHash("BS068@debuta");
		animationHash[2] = Animator.StringToHash("BS068@idle_loop");
		animationHash[15] = Animator.StringToHash("BS068@dead");
		animationHash[14] = Animator.StringToHash("BS068@hurt_loop");
		animationHash[3] = Animator.StringToHash("BS068@jump_start");
		animationHash[4] = Animator.StringToHash("BS068@jump_loop");
		animationHash[5] = Animator.StringToHash("BS068@jump_end");
		animationHash[11] = Animator.StringToHash("BS068@skill_01_start");
		animationHash[12] = Animator.StringToHash("BS068@skill_01_loop");
		animationHash[13] = Animator.StringToHash("BS068@skill_01_end");
		animationHash[6] = Animator.StringToHash("BS068@skill_02_start");
		animationHash[7] = Animator.StringToHash("BS068@skill_02_end");
		animationHash[8] = Animator.StringToHash("BS068@skill_03_start");
		animationHash[9] = Animator.StringToHash("BS068@skill_03_loop");
		animationHash[10] = Animator.StringToHash("BS068@skill_03_end");
		base.direction = 1;
		if (isChipInfoAnim)
		{
			if ((bool)handleTransform)
			{
				handleTransform.gameObject.SetActive(false);
			}
			SetStatus(MainStatus.Idle);
		}
		else
		{
			SetStatus(MainStatus.Debut);
		}
		SetMaxGravity(OrangeBattleUtility.FP_MaxGravity * 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_summer-penguin_002");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		_bDeadPlayCompleted = false;
		if (_collideBullet == null)
		{
			Debug.Log("xc");
		}
	}

	public override void SetChipInfoAnim()
	{
		isChipInfoAnim = true;
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
		AI_STATE aiState2 = AiState;
		if (aiState2 == AI_STATE.mob_002)
		{
			deadCallResult = false;
			multiBoss = true;
		}
		else
		{
			deadCallResult = true;
		}
	}

	public override void UpdateStatus(int nSet, string sMsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (sMsg != null && sMsg != "")
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

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		this.mainStatus = mainStatus;
		this.subStatus = subStatus;
		if (isChipInfoAnim)
		{
			return;
		}
		switch (this.mainStatus)
		{
		case MainStatus.Debut:
			collideBulletId = -1;
			break;
		case MainStatus.Slide:
			if (collideBulletId != 3)
			{
				collideBulletId = 3;
				_collideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				if (multiBoss)
				{
					_collideBullet.Active(targetMask);
				}
				else
				{
					_collideBullet.Active(neutralMask);
				}
			}
			break;
		default:
			if (collideBulletId != 0)
			{
				collideBulletId = 0;
				_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
				_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_collideBullet.Active(targetMask);
			}
			break;
		}
		switch (this.mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.IdleWaitNet:
			UpdateDirection();
			_velocity.x = 0;
			break;
		case MainStatus.Debut:
			switch (this.subStatus)
			{
			case SubStatus.Phase1:
				PlaySE("BossSE", 41);
				break;
			}
			break;
		case MainStatus.Jump:
			switch (this.subStatus)
			{
			case SubStatus.Phase0:
				jumpDistance = _transform.position.x;
				break;
			case SubStatus.Phase1:
				_velocity.x = base.direction * moveSpeed * 1000;
				_velocity.y = jumpSpeed * 1000;
				PlaySE("BossSE", 40);
				break;
			case SubStatus.Phase2:
				jumpDistance = Mathf.Abs(jumpDistance - _transform.position.x);
				_velocity.x = 0;
				PlaySE("BossSE", 41);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Rain:
			switch (this.subStatus)
			{
			case SubStatus.Phase0:
				jumpDistance = grabPointTransform.position.x - _transform.position.x;
				UpdateDirection(Math.Sign(jumpDistance));
				break;
			case SubStatus.Phase1:
				_velocity.x = Mathf.RoundToInt(jumpDistance * 1.8f * 1000f);
				_velocity.y = jumpSpeed * 1000;
				PlaySE("BossSE", 40);
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				break;
			case SubStatus.Phase3:
			{
				_velocity.x = 0;
				_velocity.y = 0;
				Vector3 position = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position;
				position.z = 0f;
				position.y -= 4f;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_summer-penguin_rain_000", position, (base.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, tsunamiPointTransform.position, tsunamiPointTransform.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask)._transform.rotation = tsunamiPointTransform.rotation;
				PlaySE("BossSE02", "bs018_smpeng03");
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.right * base.direction * 5;
				Controller.LogicPosition.x = Mathf.RoundToInt((grabPointTransform.position.x - 0.05f * (float)base.direction) * 1000f);
				Controller.LogicPosition.y = Mathf.RoundToInt((grabPointTransform.position.y - 1.7f) * 1000f);
				break;
			}
			case SubStatus.Phase4:
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.zero;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Shoot:
			if (this.subStatus == SubStatus.Phase0)
			{
				bulletCount = 0;
			}
			break;
		case MainStatus.Slide:
			switch (this.subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE", "bs006_peng12");
				_velocity.x = 0;
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE02", "bs018_smpeng05_lp");
				fxOfSlide = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxduring_summer-penguin_002", ModelTransform.position, (base.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
				if (fxOfSlide != null)
				{
					fxOfSlide.transform.parent = ModelTransform;
				}
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				ReleaseEffectOfSlide();
				break;
			}
			break;
		case MainStatus.Dead:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				base.DeadPlayCompleted = true;
				base.AllowAutoAim = false;
				_velocity.x = 0;
				OrangeBattleUtility.GlobalVelocityExtra = VInt3.zero;
				if (!Controller.Collisions.below)
				{
					IgnoreGravity = true;
				}
				_velocity.x = 0;
				_collideBullet.BackToPool();
				OrangeBattleUtility.LockPlayer();
				break;
			case SubStatus.Phase1:
				StartCoroutine(BossDieFlow(base.AimTransform));
				break;
			}
			break;
		}
		AiTimer.TimerStart();
		UpdateAnimation();
		UpdateCollider();
	}

	public override void LogicUpdate()
	{
		if (mainStatus == MainStatus.Debut)
		{
			BaseUpdate();
			UpdateGravity();
			Controller.Move((_velocity + _velocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
		if (!Activate && mainStatus != MainStatus.Debut)
		{
			return;
		}
		if (mainStatus != MainStatus.Rain || subStatus != SubStatus.Phase3)
		{
			IgnoreGravity = false;
		}
		else
		{
			IgnoreGravity = true;
		}
		base.LogicUpdate();
		currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (mainStatus)
		{
		case MainStatus.Debut:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)currentFrame > 1.0)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((double)currentFrame > 1.0)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((double)currentFrame > 1.0)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase4:
				SetStatus(MainStatus.Idle);
				if (intoBack != null)
				{
					PlaySE("BossSE", "bs006_peng15");
					intoBack();
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Idle:
			if (Activate)
			{
				if (aiLevel >= 1)
				{
					if (Controller.Collisions.below)
					{
						UpdateRandomState();
					}
				}
				else if (currentFrame > 1f && Controller.Collisions.below)
				{
					UpdateRandomState();
				}
			}
			else if (currentFrame > 1f)
			{
				SetStatus(mainStatus);
			}
			break;
		case MainStatus.Jump:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Rain:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (Vector3.Distance(base.AimTransform.position, grabPointTransform.position) < 0.9f)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
					if (aiLevel >= 2)
					{
						if (AiState == AI_STATE.mob_002)
						{
							MonoBehaviourSingleton<OrangeBattleUtility>.Instance.SetLockWallJump(7500);
						}
						else
						{
							MonoBehaviourSingleton<OrangeBattleUtility>.Instance.SetLockWallJump(15000);
						}
					}
				}
				if (currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (Controller.Collisions.below)
				{
					PlaySE("BossSE", 41);
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase3:
				if (currentFrame > 2.2f)
				{
					SetStatus(mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				break;
			}
			break;
		case MainStatus.Slide:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				lastPosX = base.transform.localPosition.x;
				if (currentFrame > 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				else if (currentFrame > 0.73f && _velocity.x == 0)
				{
					_velocity.x = base.direction * dashSpeed * 1000;
				}
				break;
			case SubStatus.Phase1:
			{
				if ((_velocity.x < 0 && Controller.Collisions.left) || (_velocity.x > 0 && Controller.Collisions.right))
				{
					PlaySE("BossSE", 53);
					_velocity.x *= -1;
					base.direction *= -1;
					UpdateDirection(base.direction);
				}
				float x = base.transform.localPosition.x;
				float num = Mathf.Abs(x - lastPosX);
				if (nowDistance + num > distance)
				{
					nowDistance = 0f;
				}
				else
				{
					nowDistance += num;
				}
				lastPosX = x;
				if (Math.Abs(_velocity.x) <= 200)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				else
				{
					_velocity.x -= base.direction * 200;
				}
				break;
			}
			case SubStatus.Phase2:
				if (currentFrame > 1f && Controller.Collisions.below)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Shoot:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(mainStatus, SubStatus.Phase1);
				UpdateDirection();
				break;
			case SubStatus.Phase1:
				if (currentFrame > 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
					BulletBase.TryShotBullet(EnemyWeapons[2].BulletData, fixedTransform.position, base.direction * Vector3.right, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase2:
				if (currentFrame > 1f)
				{
					if (bulletCount < 3)
					{
						SetStatus(mainStatus, SubStatus.Phase1);
						bulletCount++;
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Hurt:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Dead:
			if (subStatus == SubStatus.Phase0 && (double)currentFrame > 0.4)
			{
				if (nDeadCount > 10)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				else
				{
					nDeadCount++;
				}
			}
			if (currentFrame > 0.3f && !PlayBossFlow)
			{
				PlayBossFlow = true;
				ExploderEffect();
			}
			break;
		case MainStatus.IceSculpture:
		case MainStatus.JumpShoot:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate || mainStatus == MainStatus.Debut)
		{
			_animator.SetFloat(hashVspd, _velocity.vec3.y);
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	private MainStatus CheckNewBehaviour(MainStatus tmp)
	{
		MainStatus mainStatus = tmp;
		if (mainStatus == MainStatus.Shoot && oldCommand == 4)
		{
			for (int i = 0; i < sourceBehaviour.Length; i++)
			{
				if (sourceBehaviour[i] != oldCommand)
				{
					currentBehaviourList.Add(sourceBehaviour[i]);
				}
			}
			int[] array = currentBehaviourList.ToArray();
			mainStatus = (MainStatus)array[OrangeBattleUtility.Random(0, array.Length)];
		}
		return mainStatus;
	}

	private void UpdateRandomState()
	{
		MainStatus mainStatus = MainStatus.Idle;
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			mainStatus = (MainStatus)OrangeBattleUtility.Random(2, 6);
			if (aiLevel >= 1)
			{
				int num = OrangeBattleUtility.Random(0, BehaviourRaund[BehaviourRaund.Length - 1]);
				for (int i = 0; i < sourceBehaviour.Length; i++)
				{
					if (num > BehaviourRaund[i] && num < BehaviourRaund[i + 1])
					{
						mainStatus = (MainStatus)sourceBehaviour[i];
					}
				}
				mainStatus = (MainStatus)(oldCommand = (int)CheckNewBehaviour(mainStatus));
			}
			Target = StageUpdate.GetNearestPlayerByVintPos(Controller.LogicPosition, (int)fAIWorkRange);
			if (Target == null)
			{
				return;
			}
			if (Target.UsingVehicle && Target.refRideBaseObj != null)
			{
				TargetPos = Target.refRideBaseObj.Controller.LogicPosition;
			}
			else
			{
				TargetPos = Target.Controller.LogicPosition;
			}
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
		}
		if (StageUpdate.bIsHost)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.TargetPosX = TargetPos.x;
			netSyncData.TargetPosY = TargetPos.y;
			netSyncData.TargetPosZ = TargetPos.z;
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus, JsonConvert.SerializeObject(netSyncData));
		}
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	private void UpdateCollider()
	{
		MainStatus mainStatus = this.mainStatus;
		if (mainStatus != 0)
		{
			MainStatus mainStatus2 = mainStatus - 8;
			int num = 2;
		}
	}

	private void UpdateAnimation()
	{
		switch (mainStatus)
		{
		case MainStatus.Debut:
			switch (subStatus)
			{
			default:
				return;
			case SubStatus.Phase0:
				currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase1:
				currentAnimationId = AnimationID.ANI_JUMP_END;
				break;
			case SubStatus.Phase2:
				currentAnimationId = AnimationID.ANI_DEBUTA;
				break;
			}
			break;
		case MainStatus.Idle:
		case MainStatus.IdleWaitNet:
			currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Jump:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				currentAnimationId = AnimationID.ANI_JUMP_START;
				break;
			case SubStatus.Phase1:
				currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase2:
				currentAnimationId = AnimationID.ANI_JUMP_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Rain:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				currentAnimationId = AnimationID.ANI_JUMP_START;
				break;
			case SubStatus.Phase1:
				currentAnimationId = AnimationID.ANI_JUMP_LOOP;
				break;
			case SubStatus.Phase2:
				currentAnimationId = AnimationID.ANI_JUMP_END;
				break;
			case SubStatus.Phase3:
				currentAnimationId = AnimationID.ANI_RAIN_BEGIN;
				break;
			case SubStatus.Phase4:
				currentAnimationId = AnimationID.ANI_RAIN_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		case MainStatus.Slide:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				currentAnimationId = AnimationID.ANI_SLIDE_BEGIN;
				break;
			case SubStatus.Phase1:
				currentAnimationId = AnimationID.ANI_SLIDE_LOOP;
				break;
			case SubStatus.Phase2:
				currentAnimationId = AnimationID.ANI_SLIDE_END;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase3:
			case SubStatus.Phase4:
			case SubStatus.Phase5:
			case SubStatus.Phase6:
			case SubStatus.Phase7:
			case SubStatus.Phase8:
			case SubStatus.Phase9:
			case SubStatus.MAX_SUBSTATUS:
				break;
			}
			break;
		case MainStatus.Shoot:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
				currentAnimationId = AnimationID.ANI_SHOOT_BEGIN;
				break;
			case SubStatus.Phase2:
				currentAnimationId = AnimationID.ANI_SHOOT_END;
				break;
			}
			break;
		case MainStatus.Dead:
			currentAnimationId = ((!Controller.Collisions.below) ? AnimationID.ANI_HURT : AnimationID.ANI_DEAD);
			break;
		case MainStatus.Hurt:
			currentAnimationId = AnimationID.ANI_HURT;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		_animator.Play(animationHash[(int)currentAnimationId], 0, 0f);
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			handleTransform.SetParentNull();
			handleTransform.GetComponent<CharacterMaterial>().Appear();
			Vector3 position = _transform.position;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(position, Vector2.left, 100f, Controller.collisionMask);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(position, Vector2.right, 100f, Controller.collisionMask);
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(position, Vector2.up, 100f, Controller.collisionMask);
			if ((bool)raycastHit2D2 && (bool)raycastHit2D && (bool)raycastHit2D3)
			{
				Vector3 position2 = new Vector3(position.x - raycastHit2D.distance + (raycastHit2D.distance + raycastHit2D2.distance) / 2f, position.y + raycastHit2D3.distance, 0f);
				handleTransform.position = position2;
			}
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
			ReleaseEffectOfSlide();
		}
	}

	public override void BossIntro(Action cb)
	{
		if (mainStatus == MainStatus.Debut)
		{
			SetStatus(mainStatus, SubStatus.Phase4);
			intoBack = cb;
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

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if (mainStatus != MainStatus.Dead)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			base.SoundSource.PlaySE("BossSE", "bs006_peng16");
			ReleaseEffectOfSlide();
			StageUpdate.SlowStage();
			SetStatus(MainStatus.Dead);
		}
	}

	public void ExploderEffect()
	{
		if (deadCallResult)
		{
			StartCoroutine(BossDieFlow(base.AimTransform));
		}
		else
		{
			StartCoroutine(BossDieFlow(base.AimTransform, "FX_BOSS_EXPLODE2", false, false));
		}
	}

	private void ReleaseEffectOfSlide()
	{
		PlaySE("BossSE02", "bs018_smpeng05_stop");
		if (fxOfSlide != null)
		{
			fxOfSlide.BackToPool();
			fxOfSlide = null;
		}
	}
}
