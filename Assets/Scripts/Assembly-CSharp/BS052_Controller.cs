using System;
using CallbackDefs;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS052_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Skill_WingAttack = 2,
		Skill_JumpAttack = 3,
		Skill_LockShot = 4,
		Skill_LineShot = 5,
		Dead = 6,
		WaitNet = 7
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5
	}

	private enum AnimationID
	{
		ANI_IDLE_STAND_CLOSEWING = 0,
		ANI_IDLE_STAND_OPENWING = 1,
		ANI_IDLE_FLY_CLOSEWING = 2,
		ANI_IDLE_FLY_OPENWING = 3,
		ANI_DEBUT = 4,
		ANI_DEAD = 5,
		ANI_HURT = 6,
		ANI_WING_SPREAD_START = 7,
		ANI_WING_SPREAD_LOOP = 8,
		ANI_WING_SPREAD_END = 9,
		ANI_WING_SPREAD2_START = 10,
		ANI_WING_SPREAD2_LOOP = 11,
		ANI_WING_SPREAD2_END = 12,
		ANI_JUMP_START = 13,
		ANI_JUMP_LOOP = 14,
		ANI_JUMP_END = 15,
		ANI_LOCK_TARGET_START = 16,
		ANI_LOCK_TARGET_LOOP = 17,
		ANI_LOCK_TARGET_SHOT = 18,
		ANI_LOCK_TARGET_END = 19,
		ANI_THROW_START = 20,
		ANI_THROW_LOOP = 21,
		ANI_THROW_END = 22,
		ANIMATION_MAX_COUNT = 23
	}

	private const float DISSOLVE_TIME = 750f;

	private const float FADE_TIME = 1100f;

	[SerializeField]
	private Transform modelTransform;

	[SerializeField]
	private Transform wingTransform;

	[SerializeField]
	private Transform enemyCollider;

	[SerializeField]
	private Transform shootPointTransform;

	[SerializeField]
	private CollideBullet basicCollideBullet;

	[SerializeField]
	private CollideBullet skillCollideBullet;

	[SerializeField]
	private BS052_ShaderController fxBuildModel;

	[SerializeField]
	private ParticleSystem fxBuildEffect;

	[SerializeField]
	private ParticleSystem fxStandFadeIn;

	[SerializeField]
	private ParticleSystem fxStandFadeOut;

	[SerializeField]
	private ParticleSystem fxFlyFadeIn;

	[SerializeField]
	private ParticleSystem fxFlyFadeOut;

	[SerializeField]
	private ParticleSystem fxWingLight;

	[SerializeField]
	private ParticleSystem fxWingSpread;

	[SerializeField]
	private ParticleSystem fxFrontSight;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private MainStatus mainStatus;

	[SerializeField]
	[NaughtyAttributes.ReadOnly]
	private SubStatus subStatus;

	private MainStatus cacheStatus;

	private int[] animationHash;

	private float roomWidth;

	private float roomHeight;

	private Vector2 roomOriginPosition;

	private bool isShowResultOnDead = true;

	private Action IntroCallback;

	private OrangeCharacter targetPlayer;

	private Vector3 jumpStartPoint;

	public int jumpSpeed = 50;

	public float shotInterval = 1f;

	public int lockShotCount = 5;

	private int shotCounter;

	private bool attackFlag;

	private bool isTracking;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Start()
	{
		base.Start();
		IgnoreGravity = true;
		SetMaxGravity(jumpSpeed * 1000);
		base.AimPoint = enemyCollider.position - _transform.position;
		SetupAnimationHash();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_sigma_mode3_001", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_sigma_mode3_006");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_cyber-peacock_000");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_cyber-peacock_001");
	}

	public void UpdateFunc()
	{
		if (Activate || mainStatus == MainStatus.Debut)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			UpdateStatusLogicPerFrame();
			UpdateWaitNetStatus();
		}
	}

	public override void LogicUpdate()
	{
		if (Activate || mainStatus == MainStatus.Debut)
		{
			if (mainStatus != MainStatus.Skill_JumpAttack)
			{
				IgnoreGravity = true;
			}
			base.LogicUpdate();
			UpdateStatusLogic();
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			Vector3 position = _transform.position;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(position, Vector2.left, 100f, (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer));
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(position, Vector2.right, 100f, (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer));
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(position, Vector2.up, 100f, (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer));
			RaycastHit2D raycastHit2D4 = Physics2D.Raycast(position, Vector2.down, 100f, (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer) | (1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockEnemyLayer));
			roomWidth = Vector2.Distance(raycastHit2D.point, raycastHit2D2.point);
			roomHeight = Vector2.Distance(raycastHit2D3.point, raycastHit2D4.point);
			roomOriginPosition = new Vector2(raycastHit2D.point.x, raycastHit2D4.point.y);
			basicCollideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			basicCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			basicCollideBullet.Active(targetMask);
			SetStatus(MainStatus.Debut);
		}
	}

	private void SetVisible(bool isVisible, bool isGrounded, bool isPlayFx = true)
	{
		ParticleSystem particleSystem = null;
		if (isVisible)
		{
			base.AllowAutoAim = true;
			_characterMaterial.Appear();
			enemyCollider.gameObject.SetActive(true);
			particleSystem = ((!isGrounded) ? fxFlyFadeIn : fxStandFadeIn);
		}
		else
		{
			base.AllowAutoAim = false;
			_characterMaterial.Disappear();
			enemyCollider.gameObject.SetActive(false);
			basicCollideBullet.BackToPool();
			skillCollideBullet.BackToPool();
			particleSystem = ((!isGrounded) ? fxFlyFadeOut : fxStandFadeOut);
		}
		if (isPlayFx && particleSystem != null)
		{
			particleSystem.transform.rotation = Quaternion.Euler(0f, -90 + base.direction * 90, 0f);
			particleSystem.Play();
		}
	}

	private void UpdateDirection(int forceDirection = 0)
	{
		if (forceDirection != 0)
		{
			base.direction = forceDirection;
		}
		else if (targetPlayer != null)
		{
			if (targetPlayer.Controller.LogicPosition.x > Controller.LogicPosition.x)
			{
				base.direction = 1;
			}
			else
			{
				base.direction = -1;
			}
		}
		modelTransform.localScale = new Vector3(modelTransform.localScale.x, modelTransform.localScale.y, Mathf.Abs(modelTransform.localScale.z) * (float)base.direction);
	}

	public override void UpdateEnemyID(int _id)
	{
		base.UpdateEnemyID(_id);
		UpdateAIState();
		if (_enemyAutoAimSystem == null)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(30f);
		switch (AiState)
		{
		case AI_STATE.mob_002:
			isShowResultOnDead = false;
			base.DeadPlayCompleted = false;
			break;
		case AI_STATE.mob_003:
			isShowResultOnDead = false;
			base.DeadPlayCompleted = true;
			break;
		default:
			isShowResultOnDead = true;
			base.DeadPlayCompleted = false;
			break;
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
		Controller.SetLogicPosition(new VInt3(pos));
		base.transform.position = pos;
	}

	public override void UpdateStatus(int _status, string _message, Callback _callback = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (_message != null && _message != "")
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(_message);
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
		SetStatus((MainStatus)_status);
	}

	protected void UpdateWaitNetStatus()
	{
		if (bWaitNetStatus && (!StageUpdate.gbIsNetGame || !StageUpdate.bIsHost))
		{
			bWaitNetStatus = false;
			SetStatus(cacheStatus);
		}
	}

	private void UpdateNextStatus()
	{
		if (StageUpdate.gbIsNetGame)
		{
			if (!bWaitNetStatus && StageUpdate.bIsHost)
			{
				cacheStatus = GetNextStatus(mainStatus);
				NetSyncData netSyncData = new NetSyncData();
				netSyncData.TargetPosX = TargetPos.x;
				netSyncData.TargetPosY = TargetPos.y;
				netSyncData.TargetPosZ = TargetPos.z;
				netSyncData.SelfPosX = Controller.LogicPosition.x;
				netSyncData.SelfPosY = Controller.LogicPosition.y;
				netSyncData.SelfPosZ = Controller.LogicPosition.z;
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)cacheStatus, JsonConvert.SerializeObject(netSyncData));
				mainStatus = MainStatus.WaitNet;
				bWaitNetStatus = true;
			}
		}
		else
		{
			if (bWaitNetStatus)
			{
				bWaitNetStatus = false;
			}
			cacheStatus = GetNextStatus(mainStatus);
			SetStatus(cacheStatus);
		}
	}

	private MainStatus GetNextStatus(MainStatus currentStatus)
	{
		if (IsRageMode())
		{
			return (MainStatus)OrangeBattleUtility.Random(2, 6);
		}
		return (MainStatus)OrangeBattleUtility.Random(2, 5);
	}

	private void SetStatus(MainStatus _mainStatus, SubStatus _subStatus = SubStatus.Phase0)
	{
		mainStatus = _mainStatus;
		subStatus = _subStatus;
		switch (mainStatus)
		{
		case MainStatus.Debut:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				_characterMaterial.Disappear(null, 0f);
				fxBuildModel.Activate();
				base.SoundSource.PlaySE("BossSE03", "bs025_peacock01", 0.4f);
				break;
			case SubStatus.Phase1:
				PlaySE("BossSE03", "bs025_peacock08");
				_characterMaterial.Appear(null, 0.5f);
				fxBuildModel.Deactivate();
				fxBuildEffect.gameObject.SetActive(true);
				_animator.Play(animationHash[4], 0, 0f);
				break;
			case SubStatus.Phase3:
				_animator.Play(animationHash[3], 0, 0f);
				break;
			case SubStatus.Phase4:
				PlayBossSE("BossSE03", "bs025_peacock09");
				SetVisible(false, false);
				break;
			}
			break;
		case MainStatus.Skill_WingAttack:
			switch (subStatus)
			{
			case SubStatus.Phase0:
			{
				_animator.Play(animationHash[1], 0, 0f);
				UpdateDirection();
				Vector3 position4 = GetRoomPosition(new Vector2(0f, 0f));
				targetPlayer = OrangeBattleUtility.GetRandomPlayer();
				if (targetPlayer != null)
				{
					position4.x = targetPlayer._transform.position.x;
				}
				Controller.SetLogicPosition(new VInt3(position4));
				_transform.position = position4;
				SetVisible(true, true);
				break;
			}
			case SubStatus.Phase1:
				_animator.Play(animationHash[7], 0, 0f);
				basicCollideBullet.Active(targetMask);
				attackFlag = true;
				break;
			case SubStatus.Phase2:
				_animator.Play(animationHash[8], 0, 0f);
				break;
			case SubStatus.Phase3:
				_animator.Play(animationHash[10], 0, 0f);
				attackFlag = true;
				break;
			case SubStatus.Phase4:
				if (IsRageMode())
				{
					_animator.Play(animationHash[12], 0, 0f);
				}
				else
				{
					_animator.Play(animationHash[9], 0, 0f);
				}
				skillCollideBullet.BackToPool();
				break;
			case SubStatus.Phase5:
				PlayBossSE("BossSE03", "bs025_peacock09");
				_animator.Play(animationHash[1], 0, 0f);
				SetVisible(false, true);
				break;
			}
			break;
		case MainStatus.Skill_JumpAttack:
			switch (subStatus)
			{
			case SubStatus.Phase0:
			{
				_animator.Play(animationHash[0], 0, 0f);
				_velocity = VInt3.zero;
				UpdateDirection();
				Vector3 position3 = GetRoomPosition(new Vector2(0f, 0f));
				targetPlayer = OrangeBattleUtility.GetRandomPlayer();
				if (targetPlayer != null)
				{
					position3.x = targetPlayer._transform.position.x;
				}
				Controller.SetLogicPosition(new VInt3(position3));
				_transform.position = position3;
				SetVisible(true, true);
				break;
			}
			case SubStatus.Phase1:
				PlayBossSE("BossSE03", "bs025_peacock10");
				_animator.Play(animationHash[13], 0, 0f);
				_animator.speed = 2f;
				basicCollideBullet.Active(targetMask);
				break;
			case SubStatus.Phase2:
				fxWingSpread.Play();
				_animator.Play(animationHash[14], 0, 0f);
				_animator.speed = 1f;
				jumpStartPoint = _transform.position;
				IgnoreGravity = false;
				_velocity.y = jumpSpeed * 1000;
				skillCollideBullet.UpdateBulletData(EnemyWeapons[3].BulletData);
				skillCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				skillCollideBullet.SoundSource = base.SoundSource;
				skillCollideBullet.Active(targetMask);
				break;
			case SubStatus.Phase3:
				IgnoreGravity = true;
				_velocity = VInt3.zero;
				SetVisible(false, false, false);
				break;
			}
			break;
		case MainStatus.Skill_LockShot:
			switch (subStatus)
			{
			case SubStatus.Phase0:
			{
				_animator.Play(animationHash[2], 0, 0f);
				_animator.speed = 0f;
				Vector3 position2 = GetRoomPosition(new Vector2(0.5f + 0.4f * (float)base.direction, 0.4f));
				Controller.SetLogicPosition(new VInt3(position2));
				_transform.position = position2;
				UpdateDirection(-base.direction);
				SetVisible(true, false);
				break;
			}
			case SubStatus.Phase1:
				_animator.Play(animationHash[16], 0, 0f);
				_animator.speed = 1.5f;
				basicCollideBullet.Active(targetMask);
				targetPlayer = OrangeBattleUtility.GetRandomPlayer();
				shotCounter = 0;
				break;
			case SubStatus.Phase2:
				_animator.Play(animationHash[17], 0, 0f);
				_animator.speed = 1f;
				break;
			case SubStatus.Phase3:
				_animator.Play(animationHash[18], 0, 0f);
				shotCounter++;
				attackFlag = true;
				break;
			case SubStatus.Phase4:
				_animator.Play(animationHash[19], 0, 0f);
				isTracking = false;
				fxFrontSight.Stop();
				break;
			case SubStatus.Phase5:
				PlayBossSE("BossSE03", "bs025_peacock09");
				_animator.Play(animationHash[3], 0, 0f);
				SetVisible(false, false);
				break;
			}
			break;
		case MainStatus.Skill_LineShot:
			switch (subStatus)
			{
			case SubStatus.Phase0:
			{
				_animator.Play(animationHash[3], 0, 0f);
				_animator.speed = 0f;
				float num = 0.35f;
				targetPlayer = OrangeBattleUtility.GetRandomPlayer();
				if (targetPlayer != null)
				{
					num *= (float)((!(targetPlayer._transform.position.x > GetRoomPosition(new Vector2(0.5f, 0.5f)).x)) ? 1 : (-1));
				}
				Vector3 position = GetRoomPosition(new Vector2(0.5f + num, 0.5f));
				Controller.SetLogicPosition(new VInt3(position));
				_transform.position = position;
				UpdateDirection();
				SetVisible(true, false);
				break;
			}
			case SubStatus.Phase1:
				_animator.Play(animationHash[20], 0, 0f);
				_animator.speed = 1.7f;
				basicCollideBullet.Active(targetMask);
				attackFlag = true;
				break;
			case SubStatus.Phase2:
				_animator.Play(animationHash[22], 0, 0f);
				_animator.speed = 1f;
				break;
			case SubStatus.Phase3:
				PlayBossSE("BossSE03", "bs025_peacock09");
				_animator.Play(animationHash[3], 0, 0f);
				SetVisible(false, false);
				break;
			}
			break;
		}
		AiTimer.TimerStart();
	}

	private void UpdateStatusLogic()
	{
		float normalizedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (mainStatus)
		{
		case MainStatus.Idle:
			UpdateNextStatus();
			_velocity.x = 0;
			break;
		case MainStatus.Debut:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				_characterMaterial.Disappear(null, 0f);
				fxBuildModel.UpdateProgress(GameLogicUpdateManager.m_fFrameLen);
				if (fxBuildModel.IsDone())
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (normalizedTime >= 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				else if (normalizedTime >= 0.45f && !fxWingLight.isPlaying)
				{
					fxWingLight.Play();
				}
				break;
			case SubStatus.Phase2:
				if (_introReady)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
					if (IntroCallback != null)
					{
						IntroCallback();
					}
				}
				break;
			case SubStatus.Phase3:
				if (Activate)
				{
					SetStatus(mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if ((float)AiTimer.GetMillisecond() > 750f)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Skill_WingAttack:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if ((float)AiTimer.GetMillisecond() > 1100f)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (normalizedTime >= 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				else if (normalizedTime >= 0.7f && attackFlag)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_cyber-peacock_000", wingTransform.position, Quaternion.identity, Array.Empty<object>());
					skillCollideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
					skillCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
					skillCollideBullet.SoundSource = base.SoundSource;
					skillCollideBullet.Active(targetMask);
					attackFlag = false;
				}
				break;
			case SubStatus.Phase2:
				if (IsRageMode())
				{
					SetStatus(mainStatus, SubStatus.Phase3);
				}
				else if (normalizedTime >= 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase3:
				if (normalizedTime >= 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase4);
				}
				else if (normalizedTime >= 0.8f && attackFlag)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_cyber-peacock_001", wingTransform.position, Quaternion.identity, Array.Empty<object>());
					skillCollideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
					skillCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
					skillCollideBullet.SoundSource = base.SoundSource;
					skillCollideBullet.Active(targetMask);
					attackFlag = false;
				}
				break;
			case SubStatus.Phase4:
				if (normalizedTime >= 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if ((float)AiTimer.GetMillisecond() > 1100f)
				{
					UpdateNextStatus();
				}
				break;
			}
			break;
		case MainStatus.Skill_JumpAttack:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if ((float)AiTimer.GetMillisecond() > 1100f)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (normalizedTime >= 0.65f)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (Mathf.Abs(GetRoomPosition(new Vector2(0f, 0f)).y - _transform.position.y) > roomHeight * 0.5f || _velocity.y <= 0)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((float)AiTimer.GetMillisecond() > 1100f)
				{
					UpdateNextStatus();
				}
				break;
			}
			break;
		case MainStatus.Skill_LockShot:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if ((float)AiTimer.GetMillisecond() > 1100f)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (targetPlayer != null)
				{
					if (normalizedTime >= 1f)
					{
						isTracking = true;
						SetStatus(mainStatus, SubStatus.Phase2);
					}
					if (normalizedTime >= 0.5f)
					{
						if (!fxFrontSight.isPlaying)
						{
							fxFrontSight.transform.position = shootPointTransform.position;
							fxFrontSight.Play();
						}
					}
					else if (normalizedTime >= 0.11f && !fxWingLight.isPlaying)
					{
						fxWingLight.Play();
					}
				}
				else
				{
					UpdateNextStatus();
				}
				break;
			case SubStatus.Phase2:
				if (targetPlayer != null && shotCounter < lockShotCount)
				{
					if (normalizedTime >= 0.1f)
					{
						SetStatus(mainStatus, SubStatus.Phase3);
					}
				}
				else
				{
					SetStatus(mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase3:
				if (normalizedTime >= 1f)
				{
					if (shotCounter < lockShotCount)
					{
						SetStatus(mainStatus, SubStatus.Phase2);
					}
					else
					{
						SetStatus(mainStatus, SubStatus.Phase4);
					}
				}
				else if (normalizedTime >= 0.5f && attackFlag)
				{
					attackFlag = false;
					BulletBase bulletBase = BulletBase.TryShotBullet(EnemyWeapons[4].BulletData, shootPointTransform.position, targetPlayer.Controller.GetCenterPos() - shootPointTransform.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
					if (bulletBase != null)
					{
						bulletBase.SetTartget(targetPlayer);
					}
				}
				break;
			case SubStatus.Phase4:
				if (normalizedTime >= 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if ((float)AiTimer.GetMillisecond() > 1100f)
				{
					UpdateNextStatus();
				}
				break;
			}
			break;
		case MainStatus.Skill_LineShot:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if ((float)AiTimer.GetMillisecond() > 1100f)
				{
					SetStatus(mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (normalizedTime >= 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase2);
				}
				else if (normalizedTime >= 0.8f && attackFlag)
				{
					attackFlag = false;
					BulletBase.TryShotBullet(EnemyWeapons[5].BulletData, shootPointTransform.position, targetPlayer.Controller.GetCenterPos() - shootPointTransform.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				break;
			case SubStatus.Phase2:
				if (normalizedTime >= 1f)
				{
					SetStatus(mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((float)AiTimer.GetMillisecond() > 1100f)
				{
					UpdateNextStatus();
				}
				break;
			}
			break;
		}
	}

	private void UpdateStatusLogicPerFrame()
	{
		MainStatus mainStatus = this.mainStatus;
		if (mainStatus != MainStatus.Skill_LockShot || !isTracking || !fxFrontSight.isPlaying)
		{
			return;
		}
		if (targetPlayer != null)
		{
			float num = 17f;
			Vector3 position = fxFrontSight.transform.position;
			Vector3 realCenterPos = targetPlayer.Controller.GetRealCenterPos();
			Vector3 vector = (realCenterPos - position).normalized * num * Time.deltaTime;
			if (Vector3.Distance(position, realCenterPos) > vector.magnitude)
			{
				fxFrontSight.transform.position += vector;
			}
			else
			{
				fxFrontSight.transform.position = realCenterPos;
			}
		}
		else
		{
			fxFrontSight.Stop();
		}
	}

	public override void BossIntro(Action _callback)
	{
		_introReady = true;
		IntroCallback = _callback;
	}

	private bool IsRageMode()
	{
		return (float)(int)Hp < (float)(int)MaxHp * 0.5f;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		_animator.speed = 1f;
		if (mainStatus != MainStatus.Dead)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			IgnoreGravity = true;
			if (_transform.position.y - GetRoomPosition(new Vector2(0f, 0f)).y > 0.5f)
			{
				_animator.Play(animationHash[6], 0, 0f);
			}
			else
			{
				_animator.Play(animationHash[5], 0, 0f);
			}
			basicCollideBullet.BackToPool();
			skillCollideBullet.BackToPool();
			fxFrontSight.Stop();
			PlayBossSE("BossSE03", "bs025_peacock11");
			_velocity.x = 0;
			if (isShowResultOnDead)
			{
				StartCoroutine(BossDieFlow(wingTransform));
			}
			else
			{
				StartCoroutine(BossDieFlow(wingTransform, "FX_BOSS_EXPLODE2", false, false));
			}
			StageUpdate.SlowStage();
			SetStatus(MainStatus.Dead);
		}
	}

	private Vector2 GetRoomPosition(Vector2 position)
	{
		Vector2 result = roomOriginPosition;
		result.x += roomWidth * position.x;
		result.y += roomHeight * position.y;
		return result;
	}

	private void SetupAnimationHash()
	{
		animationHash = new int[23];
		animationHash[0] = Animator.StringToHash("BS052@idle_stand_closewing_loop");
		animationHash[1] = Animator.StringToHash("BS052@idle_stand_loop");
		animationHash[2] = Animator.StringToHash("BS052@idle_jump_closewing_loop");
		animationHash[3] = Animator.StringToHash("BS052@idle_jump_loop");
		animationHash[4] = Animator.StringToHash("BS052@debut");
		animationHash[5] = Animator.StringToHash("BS052@dead");
		animationHash[6] = Animator.StringToHash("BS052@hurt_loop");
		animationHash[7] = Animator.StringToHash("BS052@skill_01_start");
		animationHash[8] = Animator.StringToHash("BS052@skill_01_loop");
		animationHash[9] = Animator.StringToHash("BS052@skill_01_end");
		animationHash[10] = Animator.StringToHash("BS052@skill_02_start");
		animationHash[11] = Animator.StringToHash("BS052@skill_02_loop");
		animationHash[12] = Animator.StringToHash("BS052@skill_02_end");
		animationHash[13] = Animator.StringToHash("BS052@skill_03_start");
		animationHash[14] = Animator.StringToHash("BS052@skill_03_loop");
		animationHash[15] = Animator.StringToHash("BS052@skill_03_end");
		animationHash[16] = Animator.StringToHash("BS052@skill_04_step1_start");
		animationHash[17] = Animator.StringToHash("BS052@skill_04_step1_loop");
		animationHash[18] = Animator.StringToHash("BS052@skill_04_step2_start");
		animationHash[19] = Animator.StringToHash("BS052@skill_04_step2_end");
		animationHash[20] = Animator.StringToHash("BS052@skill_05_start");
		animationHash[21] = Animator.StringToHash("BS052@skill_05_loop");
		animationHash[22] = Animator.StringToHash("BS052@skill_05_end");
	}
}
