using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using UnityEngine;

public class EM129_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Seek = 2,
		Atk = 3,
		Hurt = 4,
		Die = 5,
		Back = 6
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

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_RUN = 2,
		ANI_RUNAWAY = 3,
		ANI_SEEK_START = 4,
		ANI_SEEK_LOOP = 5,
		ANI_SEEK_END = 6,
		ANI_Skill0_START1 = 7,
		ANI_Skill0_LOOP1 = 8,
		ANI_Skill0_START2 = 9,
		ANI_Skill0_LOOP2 = 10,
		ANI_Skill0_END = 11,
		ANI_HURT = 12,
		ANI_DEAD = 13,
		MAX_ANIMATION_ID = 14
	}

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	private AnimationID _currentAnimationId;

	private float _currentFrame;

	private int[] _animationHash;

	private int IdleTime = 1;

	private int IdleFrame;

	private int KobunID;

	private bool needBack = true;

	private bool BossDead;

	private BS067_Controller _parentController;

	private int BackTime = 15;

	private int BackFrame;

	[SerializeField]
	private GameObject[] Emotions;

	[SerializeField]
	private ParticleSystem[] Smokes;

	public GameObject[] RenderModes;

	private bool DebutOver;

	private Vector3 RoomLeft;

	private Vector3 RoomRight;

	private bool hasBack;

	private float MoveDis = 10f;

	private float FindDis = 8f;

	private int RunSpeed = 4800;

	private bool _isCatching;

	private float AtkEndPosX;

	private int HugTime = 7;

	private int HugFrame;

	private CollideBullet HugCollide;

	private OrangeCharacter targetOC;

	private bool hasActed;

	private bool isDebut;

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
		_animationHash[0] = Animator.StringToHash("BS067@stand2_loop");
		_animationHash[1] = Animator.StringToHash("BS067@debut_1");
		_animationHash[2] = Animator.StringToHash("BS067@run2_loop");
		_animationHash[3] = Animator.StringToHash("BS067@run_loop");
		_animationHash[4] = Animator.StringToHash("BS067@skill_4_step2_catch_miss_start");
		_animationHash[5] = Animator.StringToHash("BS067@skill_4_step2_catch_miss_loop");
		_animationHash[6] = Animator.StringToHash("BS067@skill_4_step2_catch_miss_end");
		_animationHash[7] = Animator.StringToHash("BS067@skill_4_step1_start");
		_animationHash[8] = Animator.StringToHash("BS067@skill_4_step1_loop");
		_animationHash[9] = Animator.StringToHash("BS067@skill_4_step2_catched_start");
		_animationHash[10] = Animator.StringToHash("BS067@skill_4_step2_catched_loop");
		_animationHash[11] = Animator.StringToHash("BS067@skill_4_step2_catched_end");
		_animationHash[12] = Animator.StringToHash("BS067@hurt_loop");
		_animationHash[13] = Animator.StringToHash("BS067@death");
	}

	protected override void Awake()
	{
		base.Awake();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		Transform[] componentsInChildren = _transform.GetComponentsInChildren<Transform>(true);
		LoadParts(componentsInChildren);
		_animator = ModelTransform.GetComponent<Animator>();
		base.AimPoint = new Vector3(0f, 0.45f, 0f);
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(20f, 15f);
		_animationHash = new int[14];
		HashAnimation();
		base.AllowAutoAim = true;
		FallDownSE = new string[2] { "BossSE03", "bs022_kobun19" };
	}

	protected override void Start()
	{
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fx_dash_smoke");
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
		ModelTransform.localEulerAngles = new Vector3(0f, 90 + base.direction * 15, 0f);
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			_enemyCollider[0].enabled = false;
			base.AllowAutoAim = false;
			break;
		case MainStatus.Idle:
			ChangeEmotion(0);
			_enemyCollider[0].enabled = true;
			base.AllowAutoAim = true;
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(RenderModes, ManagedSingleton<OrangeLayerManager>.Instance.RenderEnemy);
			IsInvincible = false;
			_velocity = VInt3.zero;
			IdleFrame = IdleTime * 20 + GameLogicUpdateManager.GameFrame;
			break;
		case MainStatus.Seek:
			if (_subStatus == SubStatus.Phase0)
			{
				PlaySE("BossSE03", "bs022_kobun20");
				PlaySE("BossSE03", "bs022_kobun08");
				ChangeEmotion(0);
			}
			else
			{
				ChangeEmotion(2);
			}
			break;
		case MainStatus.Atk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				ChangeEmotion(4);
				_isCatching = false;
				PlaySE("BossSE03", "bs022_kobun06");
				_velocity.x = RunSpeed * base.direction;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fx_dash_smoke", _transform, Quaternion.Euler(0f, 90 - 90 * base.direction, 0f), Array.Empty<object>());
				break;
			case SubStatus.Phase2:
				_velocity.x = 0;
				break;
			case SubStatus.Phase3:
				PlaySE("BossSE03", "bs022_kobun07");
				HugCollide.Active(targetMask);
				_collideBullet.BackToPool();
				break;
			case SubStatus.Phase4:
				PlaySE("BossSE03", "bs022_kobun13_lp");
				break;
			case SubStatus.Phase5:
				PlaySE("BossSE03", "bs022_kobun13_stop");
				_isCatching = false;
				_collideBullet.Active(targetMask);
				HugCollide.BackToPool();
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Back:
			IsInvincible = true;
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetDeadParam();
				hasActed = false;
				break;
			case SubStatus.Phase1:
				hasActed = false;
				break;
			case SubStatus.Phase2:
				CallBossDieComplete();
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
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				break;
			case SubStatus.Phase2:
				return;
			}
			break;
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Seek:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SEEK_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SEEK_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SEEK_END;
				break;
			}
			break;
		case MainStatus.Atk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_RUN;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill0_START1;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill0_LOOP1;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_Skill0_START2;
				break;
			case SubStatus.Phase4:
				_currentAnimationId = AnimationID.ANI_Skill0_LOOP2;
				break;
			case SubStatus.Phase5:
				_currentAnimationId = AnimationID.ANI_Skill0_END;
				break;
			}
			break;
		case MainStatus.Back:
			return;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEAD;
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				return;
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
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_introReady)
				{
					SetStatus(MainStatus.Debut, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.8f)
				{
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.ChangeRenderLayer(RenderModes, ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer);
					hasBack = true;
					PlaySE("BossSE03", "bs022_kobun03");
					Smokes[0].Play();
					SetStatus(MainStatus.Debut, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (Activate)
				{
					DebutOver = true;
					_parentController.SetKobunDebutOver(KobunID, DebutOver);
					SetStatus(MainStatus.Back);
				}
				break;
			}
			break;
		case MainStatus.Idle:
			if (bWaitNetStatus || GameLogicUpdateManager.GameFrame <= IdleFrame)
			{
				break;
			}
			if (AiState == AI_STATE.mob_001 && GameLogicUpdateManager.GameFrame > BackFrame && needBack)
			{
				if (needBack)
				{
					SetStatus(MainStatus.Back);
				}
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (!Target)
			{
				break;
			}
			TargetPos = new VInt3(Target._transform.position);
			UpdateDirection();
			if (Math.Abs(Target._transform.position.x - _transform.position.x) < FindDis)
			{
				AtkEndPosX = Target._transform.position.x - 0.5f * (float)base.direction;
			}
			else
			{
				Vector3 vector2 = (RoomLeft + RoomRight) / 2f;
				if (_transform.position.x > vector2.x)
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				AtkEndPosX = _transform.position.x + MoveDis * (float)base.direction;
			}
			mainStatus = MainStatus.Atk;
			break;
		case MainStatus.Seek:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Seek, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.5f && (bool)Target && Math.Abs(Target._transform.position.x - _transform.position.x) < FindDis)
				{
					TargetPos = new VInt3(Target._transform.position);
					UpdateDirection();
					AtkEndPosX = Target._transform.position.x - 0.5f * (float)base.direction;
					SetStatus(MainStatus.Seek, SubStatus.Phase2);
				}
				if (_currentFrame > 1f)
				{
					Vector3 vector = (RoomLeft + RoomRight) / 2f;
					if (_transform.position.x > vector.x)
					{
						UpdateDirection(-1);
					}
					else
					{
						UpdateDirection(1);
					}
					AtkEndPosX = _transform.position.x + MoveDis * (float)base.direction;
					SetStatus(MainStatus.Seek, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f && !bWaitNetStatus)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Atk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (TryCatchPlayer() || (_transform.position.x - AtkEndPosX) * (float)base.direction > 0f)
				{
					SetStatus(MainStatus.Atk, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				TryCatchPlayer();
				_velocity.x = (int)((float)_velocity.x * 0.6f - (float)(80 * base.direction));
				if (_currentFrame > 1f || _velocity.x * base.direction < 0)
				{
					SetStatus(MainStatus.Atk, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					if (_isCatching)
					{
						SetStatus(MainStatus.Atk, SubStatus.Phase3);
					}
					else
					{
						SetStatus(MainStatus.Seek);
					}
				}
				break;
			case SubStatus.Phase3:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Atk, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (GameLogicUpdateManager.GameFrame > HugFrame)
				{
					targetOC.SetStun(false);
					targetOC = null;
					SetStatus(MainStatus.Atk, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_currentFrame > 1f && AiState == AI_STATE.mob_001)
				{
					if (needBack)
					{
						SetStatus(MainStatus.Back);
					}
					else
					{
						SetStatus(MainStatus.Idle);
					}
				}
				break;
			}
			break;
		case MainStatus.Back:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				SetStatus(MainStatus.Back, SubStatus.Phase1);
				break;
			case SubStatus.Phase1:
				if ((bool)targetOC && targetOC.IsStun)
				{
					targetOC.SetStun(false);
					targetOC = null;
				}
				KobunBack();
				break;
			}
			break;
		case MainStatus.Die:
			if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _animationHash[13])
			{
				break;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.15f)
				{
					PlaySE("BossSE03", "bs022_kobun23");
					SetStatus(MainStatus.Die, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 0.5f && BossDead)
				{
					SetStatus(MainStatus.Die, SubStatus.Phase2);
				}
				if (_currentFrame > 0.7f && !hasActed)
				{
					hasActed = true;
					PlaySE("BossSE03", "bs022_kobun22");
				}
				if (_currentFrame > 1f)
				{
					KobunDead();
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 0.7f && !hasActed)
				{
					hasActed = true;
					PlaySE("BossSE03", "bs022_kobun27");
				}
				break;
			}
			break;
		}
		if (mainStatus != 0 && CheckHost())
		{
			UploadEnemyStatus((int)mainStatus);
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void FalldownUpdate()
	{
		if (!isFall)
		{
			isFall = !Controller.Collisions.below;
		}
		else
		{
			if (!Controller.Collisions.below)
			{
				return;
			}
			isFall = false;
			if (FallDownSE != null && FallDownSE.Length >= 2)
			{
				if (isDebut)
				{
					PlaySE(FallDownSE[0], FallDownSE[1], 0.35f);
				}
				else
				{
					PlaySE(FallDownSE[0], FallDownSE[1]);
				}
			}
		}
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		bDeadShock = false;
		if (isActive)
		{
			hasBack = false;
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			HugCollide.UpdateBulletData(EnemyWeapons[1].BulletData);
			HugCollide.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			BackFrame = GameLogicUpdateManager.GameFrame + BackTime * 20;
			float maxDistance = base.SoundSource.MaxDistance;
			base.SoundSource.MaxDistance = 6f;
			base.SoundSource.UpdateDistanceCall();
			base.SoundSource.MaxDistance = maxDistance;
			Smokes[1].Play();
			PlaySE("BossSE03", "bs022_kobun02");
			isDebut = true;
		}
		else
		{
			_collideBullet.BackToPool();
			if (_mainStatus != MainStatus.Die && !hasBack)
			{
				hasBack = true;
				PlaySE("BossSE03", "bs022_kobun03");
				Smokes[0].Play();
			}
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		ObscuredInt obscuredInt = base.Hurt(tHurtPassParam);
		if (_parentController != null)
		{
			ReturnHp(obscuredInt);
		}
		if (_parentController != null && (int)_parentController.Hp <= 0)
		{
			BossDead = true;
		}
		if ((int)obscuredInt <= 0)
		{
			if (AiState == AI_STATE.mob_001 && _mainStatus != MainStatus.Back && _mainStatus != MainStatus.Die)
			{
				if ((bool)_collideBullet)
				{
					_collideBullet.BackToPool();
				}
				SetColliderEnable(false);
				SetStatus(MainStatus.Die);
			}
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxstory_explode_000", _transform, Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f), Array.Empty<object>());
		}
		return obscuredInt;
	}

	public void SetParam(BS067_Controller parent, int ID, int SetHp, bool need = false)
	{
		KobunID = ID;
		_parentController = parent;
		Hp = SetHp;
		needBack = need;
		ChangeEmotion(0);
		SetStatus(MainStatus.Idle);
	}

	public void SetDebut()
	{
		UpdateDirection(-1);
		ChangeEmotion(2);
		SetStatus(MainStatus.Debut);
		base.AllowAutoAim = false;
	}

	public void SetRoomPos(Vector3 LeftBorn, Vector3 RightBorn)
	{
		RoomLeft = LeftBorn;
		RoomRight = RightBorn;
		MoveDis = (RoomRight.x - RoomLeft.x - 1f) / 2f;
	}

	protected void KobunBack()
	{
		ReturnHp(Hp);
		RemoveFromList(KobunID);
		if ((int)Hp > 0)
		{
			_parentController = null;
			BackToPool();
			StageObjParam component = GetComponent<StageObjParam>();
			if (component != null && component.nEventID != 0)
			{
				EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
				stageEventCall.nID = component.nEventID;
				component.nEventID = 0;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			}
		}
	}

	private void ReturnHp(int hp)
	{
		_parentController.SetKobunHP(KobunID, hp);
	}

	public void RemoveFromList(int ID)
	{
		_parentController.RemoveKobun(ID);
	}

	public void SetNeedBack(bool isNeed)
	{
		needBack = isNeed;
	}

	private void LoadParts(Transform[] childs)
	{
		if (!ModelTransform)
		{
			ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		}
		if (!HugCollide)
		{
			HugCollide = OrangeBattleUtility.FindChildRecursive(ref childs, "Bip R Hand", true).gameObject.AddOrGetComponent<CollideBullet>();
		}
	}

	private bool TryCatchPlayer()
	{
		if (!_isCatching)
		{
			Vector2 vector = Vector3.right * base.direction + Controller.GetCenterPos();
			Collider2D collider2D = Physics2D.OverlapBox(size: Controller.Collider2D.size, point: vector + Vector2.down * 0.5f, angle: 0f, layerMask: LayerMask.GetMask("Player"));
			if ((bool)collider2D)
			{
				targetOC = OrangeBattleUtility.GetHitTargetOrangeCharacter(collider2D);
				if ((bool)targetOC && targetOC.IsStun)
				{
					targetOC.SetStun(false);
				}
				if ((bool)targetOC && !targetOC.IsStun)
				{
					_isCatching = true;
					if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right * base.direction, 0.5f, Controller.collisionMask, _transform))
					{
						_transform.position = Vector3.right * base.direction * 0.2f + _transform.position;
						Controller.LogicPosition = new VInt3(_transform.position);
					}
					if ((bool)OrangeBattleUtility.RaycastIgnoreSelf(Controller.GetCenterPos(), Vector2.right * base.direction, 3f, Controller.collisionMask, _transform))
					{
						UpdateDirection(-base.direction);
					}
					_velocity = VInt3.zero;
					TargetPos = new VInt3(collider2D.transform.position);
					targetOC.SetStun(true);
					targetOC._transform.position = Vector3.right * base.direction + _transform.position;
					targetOC.Controller.LogicPosition = new VInt3(targetOC._transform.position);
					HugFrame = GameLogicUpdateManager.GameFrame + HugTime * 20;
					return true;
				}
			}
		}
		return false;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if ((bool)targetOC && targetOC.IsStun)
		{
			targetOC.SetStun(false);
			targetOC = null;
		}
		if (AiState != 0)
		{
			base.DeadBehavior(ref tHurtPassParam);
		}
	}

	private void KobunDead()
	{
		PlaySE(ExplodeSE[0], ExplodeSE[1]);
		Explosion();
		if (DeadCallback != null)
		{
			DeadCallback();
		}
		if (bDeadShock)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
		}
		BackToPool();
		StageObjParam component = GetComponent<StageObjParam>();
		if (component != null && component.nEventID != 0)
		{
			EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
			stageEventCall.nID = component.nEventID;
			component.nEventID = 0;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
		}
	}

	private void SetDeadParam()
	{
		ChangeEmotion(1);
		_enemyCollider[0].enabled = false;
		base.AllowAutoAim = false;
		_velocity = VInt3.zero;
		IsInvincible = false;
		DebutOver = false;
	}

	private void ChangeEmotion(int emotion)
	{
		for (int i = 0; i < Emotions.Length; i++)
		{
			if (i == emotion)
			{
				Emotions[i].SetActive(true);
			}
			else
			{
				Emotions[i].SetActive(false);
			}
		}
	}

	private void CallBossDieComplete()
	{
		if ((bool)_parentController)
		{
			_parentController.SetDeadCompelete(this);
		}
	}
}
