using System;
using System.Collections.Generic;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class BS003_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Hurt = 2,
		Skill0 = 3,
		Skill1 = 4,
		Skill2 = 5,
		IdleWaitNet = 6,
		Dead = 7
	}

	protected enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		MAX_SUBSTATUS = 6
	}

	private enum CapStatus
	{
		CLOSE_LOOP = 0,
		CLOSE_START = 1,
		OPEN_LOOP = 2,
		OPEN_START = 3,
		MAX_CAP_STATUS = 4
	}

	protected enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_DEBUT_CLOSE = 1,
		ANI_DEBUT = 2,
		ANI_DEAD_START = 3,
		ANI_DEAD_LOOP = 4,
		ANI_DEAD_END = 5,
		ANI_HURT = 6,
		ANI_HURT_LOOP = 7,
		ANI_SKILL0_START = 8,
		ANI_SKILL0_LOOP = 9,
		ANI_SKILL0_END = 10,
		ANI_SKILL1_START = 11,
		ANI_SKILL1_LOOP = 12,
		ANI_SKILL1_END = 13,
		ANI_SKILL2_START = 14,
		ANI_SKILL2_LOOP = 15,
		ANI_SKILL2_END = 16,
		MAX_ANIMATION_ID = 17
	}

	protected int[] _capAnimationHash;

	private float _currentCapFrame;

	private CapStatus _capStatus;

	private bool _isCapOpen;

	private MainStatus _mainStatus;

	private SubStatus _subStatus;

	protected AnimationID _currentAnimationId;

	protected float _currentFrame;

	protected int[] _animationHash;

	private Transform _missileTransform;

	private Transform[] _tentacleTransforms;

	private bool IsChipInfoAnim;

	protected Transform[] EventSpots;

	private int _beamGroup;

	private CollideBullet[] tentacleCollideBullets;

	private bool _bDeadCallResult = true;

	private bool _canCloseCap = true;

	private bool isDebutSEPlayed;

	private int useWeapon;

	private int shootCount;

	private List<MainStatus> statusStack = new List<MainStatus>();

	private readonly Color _beginCol = new Color(1f, 1f, 0f, 0.6f);

	private readonly Color _endCol = new Color(1f, 0.54f, 0f);

	private void OnEnable()
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
		_animator = GetComponentInChildren<Animator>();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model");
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Body_00");
		_missileTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_Body_05");
		_tentacleTransforms = new Transform[4];
		_tentacleTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_0");
		_tentacleTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_1");
		_tentacleTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_2");
		_tentacleTransforms[3] = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint_3");
		tentacleCollideBullets = new CollideBullet[_tentacleTransforms.Length];
		for (int i = 0; i < _tentacleTransforms.Length; i++)
		{
			tentacleCollideBullets[i] = _tentacleTransforms[i].gameObject.AddComponent<CollideBullet>();
		}
		_animationHash = new int[17];
		_animationHash[2] = Animator.StringToHash("BS003@debut");
		_animationHash[1] = Animator.StringToHash("BS003@idle_hide_loop");
		_animationHash[0] = Animator.StringToHash("BS003@idle_loop");
		_animationHash[3] = Animator.StringToHash("BS003@dead_start");
		_animationHash[4] = Animator.StringToHash("BS003@dead_loop");
		_animationHash[5] = Animator.StringToHash("BS003@dead_end");
		_animationHash[6] = Animator.StringToHash("BS003@hurt");
		_animationHash[7] = Animator.StringToHash("BS003@hurt_loop");
		_animationHash[8] = Animator.StringToHash("BS003@skill_01_start");
		_animationHash[9] = Animator.StringToHash("BS003@skill_01_loop");
		_animationHash[10] = Animator.StringToHash("BS003@skill_01_end");
		_animationHash[11] = Animator.StringToHash("BS003@skill_02_start");
		_animationHash[12] = Animator.StringToHash("BS003@skill_02_loop");
		_animationHash[13] = Animator.StringToHash("BS003@skill_02_end");
		_animationHash[14] = Animator.StringToHash("BS003@skill_03_start");
		_animationHash[15] = Animator.StringToHash("BS003@skill_03_loop");
		_animationHash[16] = Animator.StringToHash("BS003@skill_03_end");
		_capAnimationHash = new int[4];
		_capAnimationHash[3] = Animator.StringToHash("BS003@cover_open_start");
		_capAnimationHash[2] = Animator.StringToHash("BS003@cover_open_loop");
		_capAnimationHash[1] = Animator.StringToHash("BS003@cover_close_start");
		_capAnimationHash[0] = Animator.StringToHash("BS003@cover_close_loop");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuseTarget", 4);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxstory_explode_000", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_BOSS_EXPLODE2");
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		_collideBullet.isForceSE = (_collideBullet.isBossBullet = true);
		IsInvincible = true;
		SetStatus(MainStatus.Debut);
	}

	public override void SetChipInfoAnim()
	{
		IsChipInfoAnim = true;
		SetStatus(MainStatus.Idle);
		UpdateAnimation();
	}

	public override void Unlock()
	{
		_unlockReady = true;
		base.AllowAutoAim = true;
		SetColliderEnable(true);
		if (InGame)
		{
			Activate = true;
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
			if (smsg[0] == '{')
			{
				SetStatus((MainStatus)nSet);
				return;
			}
			int subStatus = int.Parse(smsg.Split(',')[0]);
			SetStatus((MainStatus)nSet, (SubStatus)subStatus);
		}
		else
		{
			SetStatus((MainStatus)nSet);
		}
	}

	public override void LogicUpdate()
	{
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
			case SubStatus.Phase1:
				if (!isDebutSEPlayed && (double)base.transform.localPosition.y < 0.5)
				{
					isDebutSEPlayed = true;
					base.SoundSource.PlaySE("BossSE", 112);
				}
				break;
			case SubStatus.Phase2:
				if (_introReady)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (_introReady)
				{
					SetStatus(_mainStatus, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				if (_unlockReady)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase0:
				break;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			case SubStatus.Phase1:
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				useWeapon = 1;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if (EnemyWeapons[useWeapon].MagazineRemain > 0f)
				{
					if (EnemyWeapons[useWeapon].LastUseTimer.IsStarted() && EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() <= EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED)
					{
						break;
					}
					for (int i = 0; i < 4; i++)
					{
						Transform transform = _tentacleTransforms[i];
						if ((bool)Target)
						{
							BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, transform.position.xy(), (Target.GetTargetPoint() - transform.position).normalized.xy(), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						}
					}
					EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
					EnemyWeapons[useWeapon].MagazineRemain -= 1f;
				}
				else if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					AiTimer.TimerStart();
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
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
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				useWeapon = 6;
				if (EnemyWeapons[useWeapon].MagazineRemain > 0f)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					if (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED)
					{
						Transform missileTransform = _missileTransform;
						BulletBase.TryShotBullet(EnemyWeapons[useWeapon].BulletData, missileTransform, OrangeBattleUtility.DegreeToVector2(180 + OrangeBattleUtility.Random(-20, 20)), null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
						EnemyWeapons[useWeapon].MagazineRemain -= 1f;
					}
				}
				if (EnemyWeapons[useWeapon].MagazineRemain == 0f && (double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					AiTimer.TimerStart();
				}
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(MainStatus.Idle);
				}
				break;
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 1.0 && (bool)Controller.BelowInBypassRange)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
				}
				UpdateGravity();
				break;
			case SubStatus.Phase2:
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase3);
					_animator.speed = 0.1f;
				}
				break;
			case SubStatus.Phase3:
				_animator.speed = 0.1f;
				break;
			}
			break;
		case MainStatus.Idle:
		case MainStatus.Hurt:
		case MainStatus.IdleWaitNet:
			break;
		}
	}

	public override void BossIntro(Action cb)
	{
		_introReady = true;
		IntroCallBack = cb;
	}

	public void UpdateFunc()
	{
		UpdateCap();
		switch (_mainStatus)
		{
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				useWeapon = 2;
				if (AiTimer.GetMillisecond() <= 1000)
				{
					break;
				}
				if (EnemyWeapons[useWeapon].MagazineRemain > 0f)
				{
					if (!EnemyWeapons[useWeapon].LastUseTimer.IsStarted() || EnemyWeapons[useWeapon].LastUseTimer.GetMillisecond() > EnemyWeapons[useWeapon].BulletData.n_FIRE_SPEED)
					{
						for (int i = 0; i < 4; i++)
						{
							BulletBase.TryShotBullet(EnemyWeapons[useWeapon + i].BulletData, _tentacleTransforms[i].position.xy(), (EventSpots[4 * _beamGroup + i].position - _tentacleTransforms[i].position).xy().normalized, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
						}
						EnemyWeapons[useWeapon].LastUseTimer.TimerStart();
						EnemyWeapons[useWeapon].MagazineRemain -= 1f;
						shootCount++;
					}
				}
				else if ((double)_currentFrame > 16.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase2);
					AiTimer.TimerStart();
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase0:
			case SubStatus.Phase2:
			case SubStatus.Phase3:
			case SubStatus.Phase4:
			case SubStatus.MAX_SUBSTATUS:
				break;
			}
			break;
		case MainStatus.Idle:
		case MainStatus.Debut:
		case MainStatus.Hurt:
		case MainStatus.Skill1:
		case MainStatus.Skill2:
		case MainStatus.IdleWaitNet:
		case MainStatus.Dead:
			break;
		}
	}

	protected void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		if (IsChipInfoAnim)
		{
			return;
		}
		switch (_mainStatus)
		{
		case MainStatus.Debut:
		{
			SubStatus subStatus2 = _subStatus;
			if (subStatus2 == SubStatus.Phase5)
			{
				int num = 16;
				EventSpots = new Transform[num];
				for (int i = 0; i < num; i++)
				{
					EventSpots[i] = FreeAutoAimSystem.GetTarget("EventSpot (" + i + ")");
				}
				if (IntroCallBack != null)
				{
					IntroCallBack();
				}
			}
			break;
		}
		case MainStatus.Idle:
			UpdateRandomState();
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateMagazine(6, true);
				shootCount = 0;
				if (AiState == AI_STATE.mob_002)
				{
					MonoBehaviourSingleton<OrangeBattleUtility>.Instance.CallSummonEnemyEvent(_transform);
				}
				break;
			case SubStatus.Phase1:
			{
				AiTimer.TimerStart();
				_beamGroup = OrangeBattleUtility.Random(0, 4);
				for (int j = 0; j < 4; j++)
				{
					PredictAttack(_tentacleTransforms[j].position, EventSpots[4 * _beamGroup + j].position);
				}
				break;
			}
			case SubStatus.Phase2:
				ToggleCap();
				break;
			}
			break;
		case MainStatus.Skill1:
			if (_subStatus == SubStatus.Phase0)
			{
				UpdateMagazine(6, true);
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				UpdateMagazine(6, true);
				break;
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				base.AllowAutoAim = false;
				_velocity.x = 0;
				OrangeBattleUtility.LockPlayer();
				break;
			case SubStatus.Phase3:
				if (_bDeadCallResult)
				{
					StartCoroutine(BossDieFlow(_transform));
				}
				else
				{
					StartCoroutine(BossDieFlow(_transform, "FX_BOSS_EXPLODE2", false, false));
				}
				break;
			}
			break;
		}
		UpdateAnimation();
	}

	private void MoveBossToDebut()
	{
		if ((bool)_characterMaterial)
		{
			_characterMaterial.Disappear();
		}
		LeanTween.value(_transform.position.y, _transform.position.y - 15f, 4f).setOnUpdate(delegate(float f)
		{
			Vector3 position = _transform.position;
			position.y = f;
			_transform.position = position;
		}).setOnComplete((Action<object>)delegate
		{
			Controller.SetLogicPosition(new VInt3(_transform.position));
			if ((bool)_characterMaterial)
			{
				_characterMaterial.Appear(delegate
				{
				});
			}
		});
	}

	private void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Debut:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_DEBUT_CLOSE;
				break;
			case SubStatus.Phase1:
				if ((bool)_characterMaterial)
				{
					_characterMaterial.Disappear();
				}
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEBUT_CLOSE;
				_animator.enabled = false;
				break;
			case SubStatus.Phase3:
				_animator.enabled = true;
				_currentAnimationId = AnimationID.ANI_DEBUT;
				if ((bool)_characterMaterial)
				{
					_characterMaterial.AppearCanStopNoInterrupt(delegate
					{
					});
				}
				break;
			default:
				_currentAnimationId = AnimationID.ANI_IDLE;
				break;
			}
			break;
		case MainStatus.Hurt:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_HURT;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_HURT_LOOP;
				break;
			}
			break;
		case MainStatus.Skill0:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL0_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL0_END;
				break;
			}
			break;
		case MainStatus.Skill1:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL1_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL1_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL1_END;
				break;
			}
			break;
		case MainStatus.Skill2:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_SKILL2_START;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL2_LOOP;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_SKILL2_END;
				break;
			}
			break;
		case MainStatus.Dead:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_DEAD_START;
				break;
			case SubStatus.Phase2:
				_currentAnimationId = AnimationID.ANI_DEAD_LOOP;
				break;
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEAD_END;
				break;
			}
			break;
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
	}

	private void UpdateCap()
	{
		_currentCapFrame = _animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
		switch (_capStatus)
		{
		case CapStatus.CLOSE_LOOP:
			if (_isCapOpen)
			{
				SetCapStatus(CapStatus.OPEN_START);
			}
			break;
		case CapStatus.CLOSE_START:
			if (_currentCapFrame >= 1f)
			{
				SetCapStatus(CapStatus.CLOSE_LOOP);
			}
			break;
		case CapStatus.OPEN_LOOP:
			if (!_isCapOpen)
			{
				SetCapStatus(CapStatus.CLOSE_START);
			}
			break;
		case CapStatus.OPEN_START:
			if (_currentCapFrame >= 1f)
			{
				SetCapStatus(CapStatus.OPEN_LOOP);
			}
			break;
		}
	}

	private void SetCapStatus(CapStatus capStatus)
	{
		_capStatus = capStatus;
		_animator.Play(_capAnimationHash[(int)capStatus], 1);
	}

	public void ToggleCap()
	{
		if (_canCloseCap)
		{
			_isCapOpen = !_isCapOpen;
			IsInvincible = !_isCapOpen;
		}
	}

	protected void UpdateRandomState()
	{
		if (statusStack.Count == 0)
		{
			for (int i = 3; i < 6; i++)
			{
				statusStack.Add((MainStatus)i);
			}
		}
		int index = OrangeBattleUtility.Random(0, statusStack.Count);
		MainStatus mainStatus = statusStack[index];
		statusStack.RemoveAt(index);
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)mainStatus);
				SetStatus(MainStatus.IdleWaitNet);
			}
		}
		else
		{
			SetStatus(mainStatus);
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
		ModelTransform.localScale = new Vector3(ModelTransform.localScale.x, ModelTransform.localScale.y, Mathf.Abs(ModelTransform.localScale.z) * (float)base.direction);
		base.transform.position = pos;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(50f);
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
			_canCloseCap = true;
			break;
		case AI_STATE.mob_003:
			_bDeadCallResult = false;
			IsInvincible = false;
			_isCapOpen = true;
			_canCloseCap = false;
			break;
		default:
			_bDeadCallResult = true;
			_canCloseCap = true;
			break;
		}
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		for (int i = 0; i < _tentacleTransforms.Length; i++)
		{
			if ((bool)tentacleCollideBullets[i])
			{
				tentacleCollideBullets[i].BackToPool();
			}
		}
		SetStatus(MainStatus.Dead);
	}

	private void PredictAttack(Vector3 sourcePos, Vector3 targetPos)
	{
		float distance = 50f;
		Vector3 vector = (sourcePos.xy() - targetPos.xy()).normalized;
		psSwingTarget obj = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<psSwingTarget>("fxuseTarget", sourcePos, Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, -vector)), Array.Empty<object>());
		obj.transform.localScale = Vector3.one * 0.5f;
		obj.SetEffect(distance, _beginCol, _endCol, 1f, 3f);
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		_animator.enabled = isActive;
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
			for (int i = 0; i < _tentacleTransforms.Length; i++)
			{
				tentacleCollideBullets[i].UpdateBulletData(EnemyWeapons[0].BulletData);
				tentacleCollideBullets[i].SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				tentacleCollideBullets[i].Active(targetMask);
			}
		}
		else
		{
			_collideBullet.BackToPool();
			for (int j = 0; j < _tentacleTransforms.Length; j++)
			{
				tentacleCollideBullets[j].BackToPool();
			}
		}
	}
}
