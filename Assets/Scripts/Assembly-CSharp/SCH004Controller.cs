#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class SCH004Controller : PetControllerBase
{
	public enum MainStatus
	{
		Idle = 0,
		Debut = 1,
		Attack = 2,
		Boom = 3,
		Splash = 4,
		Result = 5,
		BackToPool = 6
	}

	protected new MainStatus _mainStatus = MainStatus.Debut;

	private ParticleSystem _mineEffect;

	private Rigidbody2D _rigidbody2D;

	protected CapsuleCollider2D _capsuleCollider;

	protected HashSet<Transform> _hitList;

	protected int _splashWaitFrame;

	protected long _nDebutTime;

	private string _sModelName;

	private OrangeTimer _lifeTimer;

	[SerializeField]
	private long _nLifeTime;

	public int nSpeed = 10;

	[SerializeField]
	private int _nMineCount;

	protected float _easeSpeed = 0.25f;

	protected float[] _globalWaypoints;

	protected int _fromWaypointIndex;

	protected float _percentBetweenWaypoints;

	private VInt3 TargetPos;

	private bool bVisiable = true;

	private bool bNetSync;

	public Action SelfExplodeCB;

	public Action TargeExplodeCB;

	private float fTimeLeft;

	public bool UseSignedAngle { get; set; }

	protected override void Awake()
	{
		base.Awake();
		_mineEffect = _transform.GetComponentInChildren<ParticleSystem>();
		_collideBullet = GetComponentInChildren<CollideBullet>();
		_hitList = new HashSet<Transform>();
		base.gameObject.AddOrGetComponent<StageObjParam>().tLinkSOB = this;
		_rigidbody2D = base.gameObject.AddOrGetComponent<Rigidbody2D>();
		_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		_rigidbody2D.useFullKinematicContacts = true;
		_capsuleCollider = base.gameObject.AddOrGetComponent<CapsuleCollider2D>();
		_capsuleCollider.isTrigger = true;
		_capsuleCollider.direction = CapsuleDirection2D.Horizontal;
		_capsuleCollider.enabled = false;
		_lifeTimer = OrangeTimerManager.GetTimer();
		SetFollowEnabled(false);
		_globalWaypoints = new float[2];
		SelfExplodeCB = delegate
		{
			if (boomSE.Length == 0)
			{
				base.SoundSource.PlaySE("HitSE", 54);
			}
			else
			{
				base.SoundSource.PlaySE(boomSE[0], boomSE[1]);
			}
		};
		TargeExplodeCB = delegate
		{
			if (boomSE.Length == 0)
			{
				base.SoundSource.PlaySE("HitSE", (UnityEngine.Random.value > 0.5f) ? 7 : 101);
			}
			else
			{
				base.SoundSource.PlaySE(boomSE[0], boomSE[1]);
			}
		};
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_explode_003");
	}

	protected override void Start()
	{
		if (PetWeapons != null && PetWeapons[0].BulletData.s_HIT_FX != null)
		{
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(PetWeapons[0].BulletData.s_HIT_FX);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		if (stageUpdate != null)
		{
			stageUpdate.UnRegisterStageObjBase(this);
		}
	}

	protected void OnTriggerEnter2D(Collider2D col)
	{
		OnTriggerHit(col);
	}

	protected void OnTriggerStay2D(Collider2D col)
	{
		OnTriggerHit(col);
	}

	protected void OnTriggerHit(Collider2D col)
	{
		if (ManagedSingleton<StageHelper>.Instance.bEnemyActive && !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && !col.isTrigger && ((1 << col.gameObject.layer) & (int)base.TargetMask) != 0 && !CheckHitList(ref _hitList, col.transform))
		{
			switch (_mainStatus)
			{
			case MainStatus.Idle:
			case MainStatus.Attack:
				ChangeStatus(MainStatus.Boom);
				_hitList.Add(col.transform);
				break;
			case MainStatus.Splash:
				_hitList.Add(col.transform);
				break;
			case MainStatus.Debut:
			case MainStatus.Boom:
				break;
			}
		}
	}

	public override void UpdateFunc()
	{
		if (Activate)
		{
			_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
			_currentFrame = 0f;
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
		if (_follow_Player != null)
		{
			StageResManager.GetStageUpdate().UnRegisterStageObjBase(this);
			sNetSerialID = _follow_Player.sNetSerialID + PetTable.n_ID + _nMineCount;
			StageResManager.GetStageUpdate().RegisterStageObjBase(this);
			base.gameObject.layer = _follow_Player.gameObject.layer;
			base.TargetMask = _follow_Player.TargetMask;
		}
		_autoAim.SetEnable(false);
		_autoAim.targetMask = base.TargetMask;
		_autoAim.SetIgnoreInsideScreen(true);
		_autoAim.SetUseManualTarget(false);
		string[] array = PetWeapons[0].BulletData.s_FIELD.Split(',');
		if (array[0] == "0")
		{
			Controller.Collider2D.offset = new Vector2(float.Parse(array[1]), float.Parse(array[2]));
			Controller.Collider2D.size = new Vector2(float.Parse(array[3]), float.Parse(array[4]));
		}
		nSpeed = PetWeapons[0].BulletData.n_SPEED * 100;
		SetFollowEnabled(false);
	}

	public override void SetActive(bool isActive)
	{
		Controller.enabled = isActive;
		Activate = isActive;
		if (isActive)
		{
			SetStatus(MainStatus.Idle);
			base.SoundSource.UpdateDistanceCall();
			if (_follow_Player.IsLocalPlayer || bNetSync)
			{
				_collideBullet.UpdateBulletData(PetWeapons[0].BulletData, _follow_Player.sPlayerName, _follow_Player.GetNowRecordNO(), _follow_Player.nBulletRecordID++);
				if (follow_skill_id == 0 || follow_skill_id == 1)
				{
					_collideBullet.SetBulletAtk(_follow_Player.PlayerSkills[follow_skill_id].weaponStatus, _follow_Player.selfBuffManager.sBuffStatus);
					_collideBullet.BulletLevel = _follow_Player.PlayerSkills[follow_skill_id].SkillLV;
				}
				else if (follow_skill_id == -1)
				{
					_collideBullet.SetBulletAtk(_follow_Player.PlayerSkills[0].weaponStatus, _follow_Player.selfBuffManager.sBuffStatus);
					_collideBullet.BulletLevel = 1;
				}
				_collideBullet.SetPetBullet();
				_rigidbody2D.WakeUp();
				_capsuleCollider.size = Vector2.one * 0.5f;
				_capsuleCollider.enabled = true;
			}
			_velocityExtra = VInt3.zero;
			for (int i = 0; i < EquippedWeaponNum; i++)
			{
				PetWeapons[i].LastUseTimer.TimerStart();
				PetWeapons[i].MagazineRemain = PetWeapons[i].BulletData.n_MAGAZINE;
			}
			IgnoreGravity = true;
			_autoAim.UpdateAimRange(PetWeapons[0].BulletData.f_DISTANCE);
			_autoAim.SetEnable(true);
			if (PetTable.n_TYPE == 2)
			{
				MaxHp = (int)PetTable.f_PARAM_HP;
				Hp = MaxHp;
				SetCollider2D(true);
			}
			if ((bool)_mineEffect)
			{
				_mineEffect.Play(true);
			}
			_lifeTimer.TimerStart();
			switch (activeSE.Length)
			{
			case 4:
			{
				float result;
				float.TryParse(activeSE[3], out result);
				base.SoundSource.AddLoopSE(activeSE[0], activeSE[1], result);
				break;
			}
			case 2:
			case 3:
				base.SoundSource.PlaySE(activeSE[0], activeSE[1]);
				break;
			}
		}
		else
		{
			if ((bool)_mineEffect)
			{
				_mineEffect.Stop();
			}
			SetCollider2D(false);
			_hitList.Clear();
			SetStatus(MainStatus.Debut);
			_rigidbody2D.Sleep();
			_capsuleCollider.enabled = false;
			_autoAim.SetEnable(false);
			base.SoundSource.StopAll();
			bNetSync = false;
			int num = unactiveSE.Length;
			if ((uint)(num - 2) <= 2u)
			{
				base.SoundSource.PlaySE(unactiveSE[0], unactiveSE[1]);
			}
			_collideBullet.BackToPool();
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, _sModelName);
			StageResManager.GetStageUpdate().UnRegisterStageObjBase(this);
		}
		base.transform.gameObject.SetActive(isActive);
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		direction = 1;
		base.transform.position = pos;
		Controller.LogicPosition = new VInt3(base.transform.localPosition);
	}

	private void ChangeStatus(MainStatus state)
	{
		string sParam = "";
		if (_follow_Player.IsLocalPlayer || bNetSync)
		{
			if (bWaitNetStatus)
			{
				return;
			}
			bWaitNetStatus = true;
			switch (state)
			{
			case MainStatus.Attack:
				if (_autoAim.AutoAimTarget != null)
				{
					IAimTarget autoAimTarget = _autoAim.AutoAimTarget;
					if (autoAimTarget as StageObjBase != null)
					{
						sParam = (autoAimTarget as StageObjBase).sNetSerialID;
					}
				}
				break;
			}
		}
		if (bWaitNetStatus)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			netSyncData.TargetPosX = TargetPos.x;
			netSyncData.TargetPosY = TargetPos.y;
			netSyncData.TargetPosZ = TargetPos.z;
			netSyncData.sParam0 = sParam;
			StageUpdate.RegisterPetSendAndRun(sNetSerialID, (int)state, JsonConvert.SerializeObject(netSyncData), true);
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		bWaitNetStatus = false;
		if (_mainStatus > MainStatus.Attack)
		{
			return;
		}
		string text = "";
		NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
		Controller.LogicPosition.x = netSyncData.SelfPosX;
		Controller.LogicPosition.y = netSyncData.SelfPosY;
		Controller.LogicPosition.z = netSyncData.SelfPosZ;
		TargetPos.x = netSyncData.TargetPosX;
		TargetPos.y = netSyncData.TargetPosY;
		TargetPos.z = netSyncData.TargetPosZ;
		text = netSyncData.sParam0;
		switch (nSet)
		{
		case 2:
			if (text != "")
			{
				_autoAim.SetTargetByNetSerialID(text);
				_autoAim.SetUpdate(false);
			}
			break;
		case 3:
			Debug.Log("UpdateStatus: BOOM!!");
			break;
		}
		SetStatus((MainStatus)nSet);
	}

	private void SetStatus(MainStatus mainStatus)
	{
		_mainStatus = mainStatus;
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			Idle();
			break;
		case MainStatus.Boom:
			Boom();
			break;
		}
	}

	public override void LogicUpdate()
	{
		if (!Activate)
		{
			return;
		}
		base.LogicUpdate();
		switch (_mainStatus)
		{
		case MainStatus.Idle:
			if (_lifeTimer.GetMillisecond() <= _nDebutTime)
			{
				break;
			}
			_autoAim.SetUpdate(true);
			if (_lifeTimer.GetMillisecond() > _nLifeTime)
			{
				if (base.bOnPauseFirstUpdate)
				{
					SetActive(false);
				}
				else
				{
					ChangeStatus(MainStatus.Boom);
				}
			}
			else if (_autoAim.AutoAimTarget != null)
			{
				TargetPos = new VInt3(_autoAim.GetTargetPoint() - MonoBehaviourSingleton<PoolManager>.Instance.transform.position);
				_autoAim.SetUpdate(false);
				ChangeStatus(MainStatus.Attack);
			}
			else
			{
				_velocity.y = Mathf.RoundToInt(CalculateVerticalMovement() * 1000f);
			}
			break;
		case MainStatus.Attack:
			if (_lifeTimer.GetMillisecond() > _nLifeTime)
			{
				ChangeStatus(MainStatus.Boom);
				break;
			}
			if (_autoAim.AutoAimTarget == null)
			{
				ChangeStatus(MainStatus.Idle);
				break;
			}
			if (_follow_Player.IsLocalPlayer || bNetSync)
			{
				if (_autoAim.AutoAimTarget != null)
				{
					TargetPos = new VInt3(_autoAim.GetTargetPoint() - MonoBehaviourSingleton<PoolManager>.Instance.transform.position);
				}
				else
				{
					ChangeStatus(MainStatus.Idle);
				}
			}
			else if (_autoAim.AutoAimTarget != null)
			{
				TargetPos = new VInt3(_autoAim.GetTargetPoint() - MonoBehaviourSingleton<PoolManager>.Instance.transform.position);
			}
			MoveToTarget();
			break;
		case MainStatus.Splash:
			_velocity = VInt3.zero;
			if (_splashWaitFrame > 0)
			{
				_splashWaitFrame--;
			}
			else
			{
				SetStatus(MainStatus.Result);
			}
			break;
		case MainStatus.Result:
			foreach (Transform hit in _hitList)
			{
				_collideBullet.CaluDmg(_collideBullet.GetBulletData, hit);
			}
			if (PetWeapons[0].BulletData.f_RANGE > 0f)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(PetWeapons[0].BulletData.s_HIT_FX, _transform.position, Quaternion.identity, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_explode_003", _transform.position, Quaternion.identity, Array.Empty<object>());
			}
			SetStatus(MainStatus.BackToPool);
			break;
		case MainStatus.BackToPool:
			SetActive(false);
			break;
		case MainStatus.Debut:
		case MainStatus.Boom:
			break;
		}
	}

	private void MoveToTarget()
	{
		float f_DISTANCE = PetWeapons[0].BulletData.f_DISTANCE;
		Vector2 a = new Vector2(TargetPos.vec3.x, TargetPos.vec3.y);
		Vector2 b = new Vector2(Controller.LogicPosition.vec3.x, Controller.LogicPosition.vec3.y);
		if (Vector2.Distance(a, b) > f_DISTANCE && (_follow_Player.IsLocalPlayer || bNetSync))
		{
			_velocity = VInt3.zero;
			ChangeStatus(MainStatus.Idle);
			if (UseSignedAngle)
			{
				_transform.rotation = Quaternion.identity;
			}
			return;
		}
		Vector3 vector = TargetPos.vec3 - Controller.LogicPosition.vec3;
		vector.z = 0f;
		_velocity.x = (int)(vector.normalized.x * (float)nSpeed);
		_velocity.y = (int)(vector.normalized.y * (float)nSpeed);
		if (UseSignedAngle)
		{
			SignedAngle(vector.normalized);
		}
	}

	private void SignedAngle(Vector2 pDirection)
	{
		float num = Vector2.SignedAngle(Vector2.right, pDirection);
		_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, pDirection));
		if (num > 90f && num < 270f)
		{
			_transform.localScale = new Vector3(1f, -1f, 1f);
		}
		else
		{
			_transform.localScale = Vector3.one;
		}
	}

	public override ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		if (_mainStatus > MainStatus.Attack)
		{
			return Hp;
		}
		Hp = (int)Hp - (int)tHurtPassParam.dmg;
		if ((int)Hp <= 0)
		{
			Hp = 0;
			ChangeStatus(MainStatus.Boom);
		}
		return Hp;
	}

	public override bool ignoreColliderBullet()
	{
		return true;
	}

	public void SetOwner(OrangeCharacter player, int mineCount)
	{
		_nMineCount = mineCount;
		bNetSync = player.IsLocalPlayer;
		Set_follow_Player(player, false);
		Initialize();
		SetFollowEnabled(false);
	}

	public override void SetParams(string modelName, long lifeTime, int bulletSkillId, WeaponStatus weaponStatus, long debutTime)
	{
		_sModelName = modelName;
		_nLifeTime = lifeTime;
		_nDebutTime = debutTime;
	}

	private void Idle()
	{
		_autoAim.AutoAimTarget = null;
		TargetPos = Controller.LogicPosition;
		_velocity = VInt3.zero;
		float y = _transform.position.y;
		_globalWaypoints[0] = y + 0.2f;
		_globalWaypoints[1] = y - 0.2f;
	}

	private void Boom()
	{
		if (_lifeTimer.GetMillisecond() > _nLifeTime)
		{
			SelfExplodeCB.CheckTargetToInvoke();
		}
		else
		{
			TargeExplodeCB.CheckTargetToInvoke();
		}
		if (PetWeapons[0].BulletData.f_RANGE > 0f)
		{
			SetStatus(MainStatus.Splash);
			_capsuleCollider.size = Vector2.one * PetWeapons[0].BulletData.f_RANGE * 2f;
			_splashWaitFrame = 2;
		}
		else
		{
			SetStatus(MainStatus.Result);
		}
		_velocity = VInt3.zero;
	}

	protected float Ease(float x)
	{
		return Mathf.Pow(x, 2f) / (Mathf.Pow(x, 2f) + Mathf.Pow(1f - x, 2f));
	}

	protected float CalculateVerticalMovement(bool raw = false)
	{
		_fromWaypointIndex %= _globalWaypoints.Length;
		int num = (_fromWaypointIndex + 1) % _globalWaypoints.Length;
		float num2 = Mathf.Abs(_globalWaypoints[_fromWaypointIndex] - _globalWaypoints[num]);
		_percentBetweenWaypoints += GameLogicUpdateManager.m_fFrameLen * _easeSpeed / num2;
		_percentBetweenWaypoints = Mathf.Clamp01(_percentBetweenWaypoints);
		float t = Ease(_percentBetweenWaypoints);
		float num3 = Mathf.Lerp(_globalWaypoints[_fromWaypointIndex], _globalWaypoints[num], t);
		if (_percentBetweenWaypoints >= 1f)
		{
			_percentBetweenWaypoints = 0f;
			_fromWaypointIndex++;
		}
		if (raw)
		{
			return num3;
		}
		return num3 - _transform.position.y;
	}

	private void OnBecameVisible()
	{
		bVisiable = true;
	}

	private void OnBecameInvisible()
	{
		bVisiable = false;
	}

	protected bool CheckHitList(ref HashSet<Transform> hitList, Transform newHit)
	{
		if (hitList.Contains(newHit))
		{
			return true;
		}
		StageObjParam component = newHit.GetComponent<StageObjParam>();
		if ((bool)component)
		{
			OrangeCharacter orangeCharacter = component.tLinkSOB as OrangeCharacter;
			if ((bool)orangeCharacter)
			{
				CharacterControlBase component2 = orangeCharacter.GetComponent<CharacterControlBase>();
				if ((bool)component2)
				{
					foreach (Transform hit in hitList)
					{
						if (component2.CheckMyShield(hit))
						{
							return true;
						}
					}
				}
			}
		}
		else
		{
			PlayerCollider component3 = newHit.GetComponent<PlayerCollider>();
			if (component3 != null && component3.IsDmgReduceShield() && hitList.Contains(component3.GetDmgReduceOwnerTransform()))
			{
				return true;
			}
		}
		return false;
	}
}
