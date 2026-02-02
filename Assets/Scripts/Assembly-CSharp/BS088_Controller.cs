#define RELEASE
using System;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using NaughtyAttributes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class BS088_Controller : EnemyControllerBase, IManagedUpdateBehavior, IF_ForceExecute
{
	private enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Run = 2,
		Jump = 3,
		Climbing = 4,
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
		MAX_SUBSTATUS = 6
	}

	public enum AnimationID
	{
		ANI_CHARGE_LOOP = 0,
		ANI_DEBUT = 1,
		ANI_HURT_LOOP = 2,
		ANI_IDLE_01_LOOP = 3,
		ANI_IDLE_02_LOOP = 4,
		ANI_IDLE_03_LOOP = 5,
		ANI_TRANSFORM_LOOP = 6,
		MAX_ANIMATION_ID = 7
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

	private int _otherTexIndex;

	private Transform _ObjInfoBar;

	private bool _bRotate;

	[SerializeField]
	private float _rotateSpeed = 30f;

	[SerializeField]
	private float _rotateZ;

	[SerializeField]
	private int _runSpeed = 12;

	[SerializeField]
	private int _dashSpeed = 25;

	[SerializeField]
	private int _jumpSpeed = 15;

	[SerializeField]
	private float rotateTang;

	private OrangeTimer _climbingTimer;

	private int _climbingCount;

	private float _rotateSpeedOld = 12f;

	private int _nowSpeed;

	private Transform _projectorTransform;

	private ParticleSystem _efx_SpotLight;

	private float _projectorHeight = 6f;

	private float _hurtTimer;

	private float _hurtFlashTimer;

	private MainStatus NextSkill;

	protected internal BossCorpsTool CorpsTool = new BossCorpsTool();

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

	protected override void Awake()
	{
		base.Awake();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		ModelTransform = OrangeBattleUtility.FindChildRecursive(ref target, "model", true);
		_projectorTransform = OrangeBattleUtility.FindChildRecursive(ref target, "projector", true);
		_efx_SpotLight = OrangeBattleUtility.FindChildRecursive(ref target, "efx_SpotLight", true).GetComponent<ParticleSystem>();
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		_enemyAutoAimSystem.UpdateAimRange(20f);
		_climbingTimer = OrangeTimerManager.GetTimer();
		_animationHash = new int[7];
		_animationHash[0] = Animator.StringToHash("BS056@charge_loop");
		_animationHash[1] = Animator.StringToHash("BS056@debut");
		_animationHash[2] = Animator.StringToHash("BS056@hurt_loop");
		_animationHash[3] = Animator.StringToHash("BS056@idle_01_loop");
		_animationHash[4] = Animator.StringToHash("BS056@idle_02_loop");
		_animationHash[5] = Animator.StringToHash("BS056@idle_03_loop");
		_animationHash[6] = Animator.StringToHash("BS056@transform_loop");
		_mainStatus = MainStatus.Debut;
		_subStatus = SubStatus.Phase0;
		AiTimer.TimerStart();
	}

	public override void UpdateStatus(int nSet, string sMsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if ((int)Hp <= 0)
		{
			return;
		}
		if (!string.IsNullOrEmpty(sMsg))
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
			if (netSyncData.sParam0 != string.Empty)
			{
				base.direction = int.Parse(netSyncData.sParam0);
			}
		}
		SetStatus((MainStatus)nSet);
	}

	private void UpdateRandomState(bool hurt = false)
	{
		MainStatus mainStatus = MainStatus.Idle;
		switch (_mainStatus)
		{
		case MainStatus.Debut:
			mainStatus = MainStatus.Idle;
			break;
		case MainStatus.Idle:
			mainStatus = MainStatus.Run;
			break;
		case MainStatus.Run:
			mainStatus = ((!hurt) ? MainStatus.Climbing : MainStatus.Jump);
			break;
		case MainStatus.Jump:
			mainStatus = MainStatus.Run;
			break;
		case MainStatus.Climbing:
			mainStatus = MainStatus.Run;
			break;
		}
		if (!hurt && isObedient)
		{
			int mission = -1;
			if (!CorpsTool.MissionComplete())
			{
				mission = CorpsTool.ReceiveMission();
				needStop = false;
			}
			SetMission(mission);
			mainStatus = NextSkill;
		}
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				UploadEnemyStatus((int)mainStatus, false, null, new object[1] { base.direction.ToString() });
				_mainStatus = MainStatus.IdleWaitNet;
			}
		}
		else
		{
			SetStatus(mainStatus);
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
		if (_velocity.x < 0)
		{
			base.direction = -1;
		}
		else if (_velocity.x > 0)
		{
			base.direction = 1;
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
					if (IntroCallBack != null)
					{
						IntroCallBack();
						SetStatus(MainStatus.Debut, SubStatus.Phase1);
					}
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
				if ((double)_currentFrame > 0.5)
				{
					_efx_SpotLight.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
					SetStatus(MainStatus.Debut, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase5:
				if (_unlockReady)
				{
					CorpsTool.SetDebutOver();
					UpdateRandomState();
				}
				break;
			case SubStatus.Phase1:
			case SubStatus.Phase4:
				break;
			}
			break;
		case MainStatus.Idle:
			base.AllowAutoAim = true;
			if (AiTimer.GetMillisecond() <= 2000)
			{
				break;
			}
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if ((bool)Target)
			{
				if (Target.Controller.LogicPosition.x < Controller.LogicPosition.x)
				{
					base.direction = -1;
				}
				else
				{
					base.direction = 1;
				}
			}
			_climbingTimer.TimerStart();
			UpdateRandomState();
			break;
		case MainStatus.Run:
			if ((_velocity.x < 0 && Controller.Collisions.left) || (_velocity.x > 0 && Controller.Collisions.right))
			{
				_velocity.x *= -1;
			}
			if (Controller.Collisions.below && _climbingTimer.GetMillisecond() > 5000)
			{
				if (isObedient && needStop)
				{
					needStop = false;
					UpdateRandomState();
				}
				else if (!isObedient)
				{
					UpdateRandomState();
				}
			}
			break;
		case MainStatus.Jump:
			if ((_velocity.x < 0 && Controller.Collisions.left) || (_velocity.x > 0 && Controller.Collisions.right))
			{
				_velocity.x *= -1;
			}
			if (Controller.Collisions.below)
			{
				UpdateRandomState(true);
			}
			break;
		case MainStatus.Climbing:
			switch (_subStatus)
			{
			case SubStatus.Phase1:
				if (_climbingCount < 8 || isObedient)
				{
					int num = _climbingCount % 4;
					if (num == 0 && (Controller.Collisions.left || Controller.Collisions.right))
					{
						_velocity.x = 0;
						_velocity.y = _nowSpeed * 1000;
						_climbingCount++;
						if (needStop && _climbingCount > 2)
						{
							needStop = false;
							_rotateSpeed = _rotateSpeedOld;
							_nowSpeed = _runSpeed;
							IgnoreGravity = false;
							SetStatus(MainStatus.Idle);
							UpdateRandomState();
						}
					}
					else if (num == 1 && Controller.Collisions.above)
					{
						base.direction *= -1;
						_velocity.x = _nowSpeed * base.direction * 1000;
						_velocity.y = 0;
						_climbingCount++;
					}
					else if (num == 2 && (Controller.Collisions.left || Controller.Collisions.right))
					{
						_velocity.x = 0;
						_velocity.y = -_nowSpeed * 1000;
						_climbingCount++;
					}
					else if (num == 3 && Controller.Collisions.below)
					{
						base.direction *= -1;
						_velocity.x = _nowSpeed * base.direction * 1000;
						_velocity.y = 0;
						_climbingCount++;
						if (needStop && _climbingCount > 2)
						{
							needStop = false;
							_rotateSpeed = _rotateSpeedOld;
							_nowSpeed = _runSpeed;
							IgnoreGravity = false;
							SetStatus(MainStatus.Idle);
							UpdateRandomState();
						}
					}
				}
				else
				{
					SetStatus(MainStatus.Climbing, SubStatus.Phase2);
				}
				break;
			case SubStatus.Phase2:
				if ((_velocity.x < 0 && Controller.Collisions.left) || (_velocity.x > 0 && Controller.Collisions.right))
				{
					_velocity.x *= -1;
					base.direction *= -1;
				}
				break;
			case SubStatus.Phase0:
				break;
			}
			break;
		case MainStatus.Die:
			if (_subStatus == SubStatus.Phase0 && AiTimer.GetMillisecond() > 500)
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
		case MainStatus.IdleWaitNet:
			if (CheckHost())
			{
				SetStatus(MainStatus.Idle);
			}
			break;
		}
	}

	public void UpdateFunc()
	{
		if (!Activate && _mainStatus != MainStatus.Debut)
		{
			return;
		}
		if (_bRotate && _mainStatus != MainStatus.Debut)
		{
			float num = _rotateSpeed * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			rotateTang += num;
			_rotateZ = (_rotateZ + num * (float)(-base.direction)) % 360f;
			ModelTransform.localRotation = Quaternion.Euler(0f, 0f, _rotateZ);
			if (rotateTang / 90f > 1f)
			{
				rotateTang -= 90f;
				if (_rotateSpeed <= 30f)
				{
					PlaySE("BossSE02", "bs108_shuriken02");
				}
				else
				{
					PlaySE("BossSE02", "bs108_shuriken03");
				}
			}
		}
		if (_hurtTimer > 0f)
		{
			_hurtTimer -= Time.deltaTime;
			_hurtFlashTimer += Time.deltaTime;
			if (_hurtFlashTimer > 0.1f)
			{
				_hurtFlashTimer -= 0.1f;
				_otherTexIndex = ((_otherTexIndex == 0) ? 1 : 0);
				_characterMaterial.UpdateTex(_otherTexIndex);
			}
		}
		else if (_otherTexIndex != 0)
		{
			_otherTexIndex = 0;
			_characterMaterial.UpdateTex(_otherTexIndex);
		}
		base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
	}

	public override void SetActive(bool isActive)
	{
		if (!isActive)
		{
			LeanTween.cancel(base.gameObject);
		}
		base.SetActive(isActive);
		if (isActive)
		{
			_projectorTransform.gameObject.SetActive(false);
			ModelTransform.localScale = Vector3.one;
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
			_collideBullet.UpdateBulletData(EnemyWeapons[0].BulletData);
			_collideBullet.SetBulletAtk(null, selfBuffManager.sBuffStatus, EnemyData);
			_collideBullet.Active(targetMask);
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

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (GuardTransform.Contains(tHurtPassParam.nSubPartID))
		{
			return Hp;
		}
		tHurtPassParam.dmg = selfBuffManager.ReduceDmgByEnergyShild(tHurtPassParam.dmg);
		OrangeBattleUtility.UpdateEnemyHp(ref Hp, ref tHurtPassParam.dmg);
		if (!InGame)
		{
			Debug.LogWarning("[Enemy] InGame Flag is false.");
			return Hp;
		}
		UpdateHurtAction();
		if ((int)Hp > 0)
		{
			_characterMaterial.Hurt();
			_hurtTimer = 1f;
			_hurtFlashTimer = 0f;
			if (_mainStatus == MainStatus.Run)
			{
				UpdateRandomState(true);
			}
		}
		else if (_mainStatus != MainStatus.Die)
		{
			if ((bool)_collideBullet)
			{
				_collideBullet.BackToPool();
			}
			SetColliderEnable(false);
			if (CorpsTool.Member != null)
			{
				CorpsTool.fightState = BossCorpsTool.FightState.Dead;
			}
			SetStatus(MainStatus.Die);
		}
		if (CorpsTool.Member != null)
		{
			CorpsTool.ReturnHp(Hp);
		}
		return Hp;
	}

	private void SetStatus(MainStatus mainStatus, SubStatus subStatus = SubStatus.Phase0)
	{
		if (_mainStatus == MainStatus.Die && mainStatus != MainStatus.Die)
		{
			return;
		}
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
				ModelTransform.localScale = Vector3.zero;
				break;
			case SubStatus.Phase1:
				_bRotate = false;
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
				PlaySE("BossSE02", "bs108_shuriken01");
				_efx_SpotLight.Play();
				break;
			case SubStatus.Phase3:
				ModelTransform.localScale = Vector3.one;
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
			_velocity = VInt3.zero;
			_bRotate = true;
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			LeanTween.value(base.gameObject, 0f, 30f, 2f).setOnUpdate(delegate(float f)
			{
				_rotateSpeed = f;
			});
			break;
		case MainStatus.Run:
			IgnoreGravity = false;
			_nowSpeed = _runSpeed;
			_velocity.x = base.direction * _nowSpeed * 1000;
			break;
		case MainStatus.Jump:
			_velocity.y = _jumpSpeed * 1000;
			break;
		case MainStatus.Climbing:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				_velocity.x = 0;
				LeanTween.value(base.gameObject, _rotateSpeed, 45f, 1f).setOnUpdate(delegate(float f)
				{
					_rotateSpeed = f;
				}).setOnComplete((Action)delegate
				{
					if (_mainStatus != MainStatus.Die)
					{
						SetStatus(MainStatus.Climbing, SubStatus.Phase1);
					}
				});
				break;
			case SubStatus.Phase1:
				IgnoreGravity = true;
				_climbingCount = 0;
				Target = _enemyAutoAimSystem.GetClosetPlayer();
				if ((bool)Target)
				{
					if (Target.Controller.LogicPosition.x < Controller.LogicPosition.x)
					{
						base.direction = -1;
					}
					else
					{
						base.direction = 1;
					}
				}
				_nowSpeed = _dashSpeed;
				_velocity.x = base.direction * _nowSpeed * 1000;
				_velocity.y = 0;
				break;
			case SubStatus.Phase2:
				IgnoreGravity = false;
				LeanTween.value(base.gameObject, _rotateSpeed, _rotateSpeedOld, 1f).setOnUpdate(delegate(float f)
				{
					_rotateSpeed = f;
				}).setOnComplete((Action)delegate
				{
					_climbingTimer.TimerStart();
					if (mainStatus != MainStatus.Die)
					{
						UpdateRandomState();
					}
				});
				LeanTween.value(base.gameObject, _nowSpeed, _runSpeed, 1f).setOnUpdate(delegate(float f)
				{
					_nowSpeed = (int)f;
					_velocity.x = (int)((float)base.direction * f * 1000f);
				});
				break;
			}
			break;
		case MainStatus.Die:
			switch (_subStatus)
			{
			case SubStatus.Phase0:
				base.AllowAutoAim = false;
				_velocity.x = 0;
				_hurtTimer = 0f;
				_hurtFlashTimer = 0f;
				if (_otherTexIndex != 0)
				{
					_otherTexIndex = 0;
					_characterMaterial.UpdateTex(_otherTexIndex);
				}
				LeanTween.cancel(base.gameObject);
				OrangeBattleUtility.LockPlayer();
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
		MainStatus mainStatus = _mainStatus;
		if (mainStatus == MainStatus.Debut)
		{
			switch (_subStatus)
			{
			case SubStatus.Phase3:
				_currentAnimationId = AnimationID.ANI_DEBUT;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			case SubStatus.Phase4:
				break;
			default:
				_currentAnimationId = AnimationID.ANI_IDLE_01_LOOP;
				_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
				break;
			}
		}
		else
		{
			_currentAnimationId = AnimationID.ANI_IDLE_01_LOOP;
			_animator.Play(_animationHash[(int)_currentAnimationId], 0, 0f);
		}
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
		SetStatus(MainStatus.Debut);
		base.AllowAutoAim = false;
	}

	public bool SetMission(int mission = -1)
	{
		switch (mission)
		{
		case -1:
			NextSkill = MainStatus.Idle;
			return true;
		case 0:
			NextSkill = MainStatus.Run;
			return true;
		case 1:
			NextSkill = MainStatus.Climbing;
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
		if (!CorpsTool.WaitMission)
		{
			needStop = true;
		}
		return true;
	}

	public override void UpdateEnemyID(int id)
	{
		base.UpdateEnemyID(id);
		UpdateAIState();
	}
}
