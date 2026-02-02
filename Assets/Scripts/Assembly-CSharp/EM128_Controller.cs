using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using UnityEngine;

public class EM128_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Atk = 2,
		Hurt = 3,
		Die = 4,
		Back = 5
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4
	}

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT = 1,
		ANI_RUN = 2,
		ANI_RUNAWAY = 3,
		ANI_Skill0_START = 4,
		ANI_Skill0_LOOP = 5,
		ANI_Skill0_END = 6,
		ANI_HURT = 7,
		ANI_DEAD = 8,
		MAX_ANIMATION_ID = 9
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

	private int BackTime = 60;

	private int BackFrame;

	[SerializeField]
	private GameObject[] Emotions;

	[SerializeField]
	private ParticleSystem[] Smokes;

	public GameObject[] RenderModes;

	private bool DebutOver;

	private int RunSpeed = 4800;

	private Vector3 RoomLeft;

	private Vector3 RoomRight;

	private bool hasBack;

	[SerializeField]
	private GameObject Weapon;

	private bool WeaponOpen;

	private int ShootTime;

	private float ShootCD = 1f;

	private int CDFrame;

	[SerializeField]
	private Transform ShootPos;

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
		_animationHash[1] = Animator.StringToHash("BS067@debut_3");
		_animationHash[2] = Animator.StringToHash("BS067@run2_loop");
		_animationHash[3] = Animator.StringToHash("BS067@run_loop");
		_animationHash[4] = Animator.StringToHash("BS067@skill_3_start");
		_animationHash[5] = Animator.StringToHash("BS067@skill_3_loop");
		_animationHash[6] = Animator.StringToHash("BS067@skill_3_end");
		_animationHash[7] = Animator.StringToHash("BS067@hurt_loop");
		_animationHash[8] = Animator.StringToHash("BS067@death");
	}

	protected override void Awake()
	{
		base.Awake();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		base.AimPoint = new Vector3(0f, 0.45f, 0f);
		_animator = ModelTransform.GetComponent<Animator>();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.SetColliderType(EnemyAutoAimSystem.ColliderType.Box);
		_enemyAutoAimSystem.UpdateAimRange(20f, 15f);
		_animationHash = new int[9];
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
			UpdateDirection();
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
		_currentFrame = 0f;
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
			ShowWeapon(false);
			_velocity = VInt3.zero;
			IdleFrame = IdleTime * 20 + GameLogicUpdateManager.GameFrame;
			break;
		case MainStatus.Atk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
			{
				ChangeEmotion(3);
				Vector3 vector = (RoomLeft + RoomRight) / 2f;
				if (_transform.position.x > vector.x)
				{
					UpdateDirection(-1);
				}
				else
				{
					UpdateDirection(1);
				}
				PlaySE("BossSE03", "bs022_kobun14");
				_velocity.x = RunSpeed * base.direction;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fx_dash_smoke", _transform, Quaternion.Euler(0f, 90 - 90 * base.direction, 0f), Array.Empty<object>());
				break;
			}
			case SubStatus.Phase1:
				ChangeEmotion(2);
				_velocity = VInt3.zero;
				ShowWeapon(true);
				ShootTime = 4;
				break;
			case SubStatus.Phase2:
			{
				ShootTime--;
				Vector3 pDirection = Vector3.right * base.direction;
				if ((bool)Target)
				{
					TargetPos = new VInt3(Target.transform.position);
					UpdateDirection();
					pDirection = Target.Controller.GetCenterPos() - Controller.GetCenterPos();
				}
				PlaySE("BossSE03", "bs022_kobun16");
				BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, ShootPos.position, pDirection, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				break;
			}
			case SubStatus.Phase4:
				ChangeEmotion(0);
				_velocity = VInt3.zero;
				break;
			}
			break;
		case MainStatus.Back:
			ShowWeapon(false);
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
		case MainStatus.Atk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_RUNAWAY;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_Skill0_START;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_Skill0_LOOP;
				break;
			case SubStatus.Phase3:
				return;
			case SubStatus.Phase4:
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
			if ((bool)Target)
			{
				TargetPos = new VInt3(Target._transform.position);
				UpdateDirection();
				mainStatus = MainStatus.Atk;
			}
			break;
		case MainStatus.Atk:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 3f)
				{
					SetStatus(MainStatus.Atk, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (_currentFrame > 1f)
				{
					SetStatus(MainStatus.Atk, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if (_currentFrame > 1f)
				{
					CDFrame = GameLogicUpdateManager.GameFrame + (int)(ShootCD * 20f);
					SetStatus(MainStatus.Atk, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if (GameLogicUpdateManager.GameFrame > CDFrame)
				{
					if (ShootTime > 0)
					{
						SetStatus(MainStatus.Atk, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Atk, SubStatus.Phase4);
					}
				}
				break;
			case SubStatus.Phase4:
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
				KobunBack();
				break;
			}
			break;
		case MainStatus.Die:
			if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _animationHash[8])
			{
				break;
			}
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (_currentFrame > 0.15f)
				{
					PlaySE("BossSE03", "bs022_kobun11");
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
		ShowWeapon(false);
		ChangeEmotion(0);
		SetStatus(MainStatus.Idle);
	}

	public void SetDebut()
	{
		ShowWeapon(false);
		ChangeEmotion(3);
		UpdateDirection(-1);
		SetStatus(MainStatus.Debut);
		base.AllowAutoAim = false;
	}

	public void SetRoomPos(Vector3 LeftBorn, Vector3 RightBorn)
	{
		RoomLeft = LeftBorn;
		RoomRight = RightBorn;
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
		if (!Weapon)
		{
			Weapon = OrangeBattleUtility.FindChildRecursive(ref childs, "BS067_GunMesh", true).gameObject;
		}
		if (!ModelTransform)
		{
			ModelTransform = OrangeBattleUtility.FindChildRecursive(ref childs, "model", true);
		}
		if (!ShootPos)
		{
			ShootPos = OrangeBattleUtility.FindChildRecursive(ref childs, "ShootPoint_Gun", true);
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
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
		WeaponOpen = false;
		Weapon.SetActive(false);
	}

	private void ShowWeapon(bool active)
	{
		WeaponOpen = active;
		Weapon.SetActive(active);
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
