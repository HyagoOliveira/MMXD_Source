using System;
using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS015_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	protected enum MainStatus
	{
		Idle = 0,
		DashAttack = 1,
		SwingAttack = 2,
		Fall = 3,
		Dead = 4,
		SwingAttackStart = 5,
		Hurt = 6,
		Turn = 7
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

	public enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_SKILL0 = 1,
		ANI_SKILL1 = 2,
		ANI_HURT = 3,
		ANI_DEAD = 4,
		ANI_TURN = 5,
		ANI_ACCELERATE = 6,
		ANI_SKILL1START = 7,
		MAX_ANIMATION_ID = 8
	}

	protected MainStatus _mainStatus;

	protected SubStatus _subStatus;

	protected AnimationID _currentAnimationId;

	public int MoveSpeed = 3000;

	public int DashSpeed = 5000;

	protected readonly int _hashHspd = Animator.StringToHash("fHspd");

	protected float _currentFrame;

	protected int[] _animationHash;

	protected Transform _efxpointTransform;

	protected Transform _efxpointTransform2;

	protected CollideBullet _maceCollideBullet;

	protected bool _bDeadCallResult = true;

	private bool _canCallFireBall;

	protected bool _isFireball;

	protected bool swift_fireball = true;

	protected OrangeTimer SwingTimer = OrangeTimerManager.GetTimer();

	protected void OnEnable()
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
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		base.AimTransform = OrangeBattleUtility.FindChildRecursive(ref target, "Body_jnt");
		_animator = GetComponentInChildren<Animator>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_efxpointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "efxpoint", true);
		_efxpointTransform2 = OrangeBattleUtility.FindChildRecursive(ref target, "efxpoint2", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "BodyPoint_Center", true);
		if (transform == null)
		{
			transform = OrangeBattleUtility.FindChildRecursive(ref target, "BodyPoint_Center", true);
		}
		_collideBullet = transform.gameObject.AddOrGetComponent<CollideBullet>();
		_maceCollideBullet = OrangeBattleUtility.FindChildRecursive(ref target, "Mace_ctrl", true).gameObject.AddOrGetComponent<CollideBullet>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_bs15_run", 10);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_maoh_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_bs15_atk", 2);
		_animationHash = new int[8];
		_animationHash[0] = Animator.StringToHash("Idle");
		_animationHash[4] = Animator.StringToHash("Dead");
		_animationHash[3] = Animator.StringToHash("Hurt");
		_animationHash[1] = Animator.StringToHash("Skill0");
		_animationHash[2] = Animator.StringToHash("Skill1");
		_animationHash[5] = Animator.StringToHash("Turn");
		_animationHash[6] = Animator.StringToHash("Accelerate");
		_animationHash[7] = Animator.StringToHash("Skill1_loop");
		base.direction = 1;
		SetStatus(MainStatus.Idle);
		FallDownSE = new string[2] { "BossSE", "bs102_molb02" };
	}

	protected virtual void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Fall:
			_velocity.x = 0;
			PlaySE("BossSE", 213);
			PlaySE("BossSE", 210);
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				PlaySE("BossSE", 214);
				PlaySE("BossSE", 209);
				_velocity.x = 0;
				break;
			case SubStatus.Phase1:
				_velocity.x = Math.Sign(base.direction) * DashSpeed;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.SwingAttack:
		case MainStatus.SwingAttackStart:
			PlaySE("BossSE", 214);
			PlaySE("BossSE", 209);
			_velocity.x = Math.Sign(base.direction) * MoveSpeed;
			break;
		case MainStatus.Dead:
			PlaySE("BossSE", 210);
			PlaySE("BossSE", 214);
			_velocity.x = 0;
			_collideBullet.BackToPool();
			OrangeBattleUtility.LockPlayer();
			PlaySE("HitSE", 103);
			PlaySE("HitSE", 104);
			StartCoroutine(MBossExplosionSE());
			if (base.AimTransform != null)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_BOSS_EXPLODE2", base.AimTransform, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_BOSS_EXPLODE2", new Vector3(base.transform.position.x, base.transform.position.y + 1f, base.transform.position.z), Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
			}
			if (_bDeadCallResult)
			{
				BattleInfoUI.Instance.ShowExplodeBG(base.gameObject, false);
			}
			else
			{
				StartCoroutine(BossDieFlow(GetTargetPoint(), "FX_BOSS_EXPLODE2", false, false));
			}
			break;
		case MainStatus.Hurt:
			PlaySE("BossSE", 210);
			PlaySE("BossSE", 214);
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity = new VInt3(-_velocity.x, IntMath.Abs(_velocity.x) * 2, 0);
				break;
			case SubStatus.Phase1:
				_velocity.x = 0;
				break;
			default:
				throw new ArgumentOutOfRangeException("subStatus", subStatus, null);
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Turn:
			_velocity.x = 0;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		AiTimer.TimerStart();
		UpdateAnimation();
		UpdateCollider();
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
			if (_currentFrame > 1f && Controller.Collisions.below)
			{
				UpdateRandomState();
			}
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bs15_run", _efxpointTransform, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				if ((double)_currentFrame > 1.0)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bs15_run", _efxpointTransform, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
				{
					SetStatus(MainStatus.Hurt);
					_isFireball = false;
					PlaySE("BossSE", 211);
					if (_canCallFireBall)
					{
						if (Controller.Collisions.left)
						{
							CallEventEnemyPoint(996);
							CallEventEnemyPoint(999);
						}
						else if (Controller.Collisions.right)
						{
							CallEventEnemyPoint(994);
							CallEventEnemyPoint(995);
						}
					}
				}
				if (_canCallFireBall && !_isFireball)
				{
					if (swift_fireball)
					{
						swift_fireball = !swift_fireball;
						CallEventEnemyPoint(998);
					}
					else
					{
						swift_fireball = !swift_fireball;
						CallEventEnemyPoint(997);
					}
					_isFireball = true;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Fall:
			if (Controller.Collisions.below)
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		case MainStatus.SwingAttack:
		case MainStatus.SwingAttackStart:
			if (_canCallFireBall)
			{
				if (!SwingTimer.IsStarted())
				{
					SwingTimer.TimerStart();
				}
				else if (SwingTimer.GetMillisecond() > 500 && !_isFireball)
				{
					if (swift_fireball)
					{
						swift_fireball = !swift_fireball;
						CallEventEnemyPoint(998);
					}
					else
					{
						swift_fireball = !swift_fireball;
						CallEventEnemyPoint(997);
					}
					SwingTimer.TimerStop();
					_isFireball = true;
				}
			}
			if ((_velocity.x > 0 && Controller.Collisions.right) || (_velocity.x < 0 && Controller.Collisions.left))
			{
				SetStatus(MainStatus.Hurt);
				_isFireball = false;
				PlaySE("BossSE", 211);
				if (_canCallFireBall)
				{
					if (Controller.Collisions.left)
					{
						CallEventEnemyPoint(996);
						CallEventEnemyPoint(999);
					}
					else if (Controller.Collisions.right)
					{
						CallEventEnemyPoint(994);
						CallEventEnemyPoint(995);
					}
				}
			}
			if ((double)_currentFrame > 1.0)
			{
				SwingTimer.TimerStop();
				if (_mainStatus == MainStatus.SwingAttack)
				{
					SetStatus(MainStatus.SwingAttackStart);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_bs15_atk", _efxpointTransform2, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_maoh_000", _efxpointTransform2, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				}
				else
				{
					SetStatus(MainStatus.SwingAttack);
				}
			}
			break;
		case MainStatus.Hurt:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				if (Controller.Collisions.below)
				{
					SetStatus(_mainStatus, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if ((double)_currentFrame > 2.0)
				{
					SetStatus(MainStatus.Turn);
					PlaySE("BossSE", 215);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.Turn:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				LeanTween.value(base.direction * 90, -base.direction * 90, 1f).setOnUpdate(delegate(float f)
				{
					ModelTransform.eulerAngles = Vector3.up * f;
				}).setOnComplete((Action)delegate
				{
					base.direction *= -1;
					SetStatus(_mainStatus, SubStatus.Phase2);
				});
				SetStatus(_mainStatus, SubStatus.Phase1);
				break;
			case SubStatus.Phase2:
				SetStatus(MainStatus.Idle);
				break;
			case SubStatus.Phase1:
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MainStatus.Dead:
			break;
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_animator.SetFloat(_hashHspd, (float)_velocity.x / (float)MoveSpeed);
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp > 0)
		{
			if (smsg != null && smsg != "")
			{
				NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
				Controller.LogicPosition.x = netSyncData.SelfPosX;
				Controller.LogicPosition.y = netSyncData.SelfPosY;
				Controller.LogicPosition.z = netSyncData.SelfPosZ;
			}
			SetStatus((MainStatus)nSet);
		}
	}

	protected void UpdateRandomState()
	{
		MainStatus nSetKey = MainStatus.Idle;
		if (StageUpdate.bIsHost)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			nSetKey = (MainStatus)OrangeBattleUtility.Random(1, 4);
		}
		else if (bWaitNetStatus)
		{
			bWaitNetStatus = false;
		}
		if (StageUpdate.bIsHost)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			bWaitNetStatus = true;
			StageUpdate.RegisterSendAndRun(sNetSerialID, (int)nSetKey, JsonConvert.SerializeObject(netSyncData));
		}
	}

	protected virtual void UpdateCollider()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Fall:
		case MainStatus.Dead:
		case MainStatus.Hurt:
		case MainStatus.Turn:
			if (_maceCollideBullet.IsActivate)
			{
				_maceCollideBullet.IsDestroy = true;
			}
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_maceCollideBullet.UpdateBulletData(EnemyWeapons[1].BulletData);
				_maceCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_maceCollideBullet.BackToPool();
				_maceCollideBullet.Active(targetMask);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				break;
			}
			break;
		case MainStatus.SwingAttack:
		case MainStatus.SwingAttackStart:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_maceCollideBullet.UpdateBulletData(EnemyWeapons[2].BulletData);
				_maceCollideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
				_maceCollideBullet.BackToPool();
				_maceCollideBullet.Active(targetMask);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case SubStatus.Phase1:
			case SubStatus.Phase2:
				break;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	protected void UpdateAnimation()
	{
		switch (_mainStatus)
		{
		case MainStatus.Idle:
		case MainStatus.Fall:
			_currentAnimationId = AnimationID.ANI_IDLE;
			break;
		case MainStatus.Dead:
			_currentAnimationId = AnimationID.ANI_DEAD;
			break;
		case MainStatus.DashAttack:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_currentAnimationId = AnimationID.ANI_ACCELERATE;
				break;
			case SubStatus.Phase1:
				_currentAnimationId = AnimationID.ANI_SKILL0;
				break;
			}
			break;
		case MainStatus.SwingAttack:
			_currentAnimationId = AnimationID.ANI_SKILL1;
			break;
		case MainStatus.SwingAttackStart:
			_currentAnimationId = AnimationID.ANI_SKILL1START;
			break;
		case MainStatus.Hurt:
			_currentAnimationId = AnimationID.ANI_HURT;
			break;
		case MainStatus.Turn:
			_currentAnimationId = AnimationID.ANI_TURN;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
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
		ModelTransform.eulerAngles = Vector3.up * base.direction * 90f;
		base.transform.position = pos;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		if ((bool)_collideBullet)
		{
			_collideBullet.BackToPool();
		}
		_maceCollideBullet.BackToPool();
		SetStatus(MainStatus.Dead);
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (isActive)
		{
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
		}
		else
		{
			_collideBullet.BackToPool();
		}
		SkinnedMeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].updateWhenOffscreen = isActive;
		}
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
		switch (AiState)
		{
		case AI_STATE.mob_002:
			_bDeadCallResult = false;
			break;
		case AI_STATE.mob_003:
			_canCallFireBall = true;
			break;
		case AI_STATE.mob_004:
			_bDeadCallResult = false;
			_canCallFireBall = true;
			break;
		default:
			_bDeadCallResult = true;
			break;
		}
	}

	private void CallEventEnemyPoint(int nID)
	{
		EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
		stageEventCall.nID = nID;
		stageEventCall.tTransform = OrangeBattleUtility.CurrentCharacter.transform;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
	}
}
