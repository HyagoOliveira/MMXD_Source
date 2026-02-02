using System;
using System.Collections.Generic;
using Better;
using StageLib;
using UnityEngine;

public class CH134_Gospel_Controller : MonoBehaviour, ILogicUpdate, IManagedUpdateBehavior
{
	private struct EntityCacheData
	{
		public Vector3 Point;

		public OrangeCharacter.MainStatus CurMainStatus;

		public OrangeCharacter.SubStatus CurSubStatus;

		public short AnimateId;

		public int VelocityY;

		public sbyte IsShoot;

		public int Dir;
	}

	private enum Status
	{
		Idle = 0,
		JumpOut = 1,
		Waiting = 2,
		JumpIn = 3,
		DoNothing = 4
	}

	private readonly string FX_IN = "fxuse_GospelAttack_001";

	private readonly string FX_OUT = "fxuse_GospelAttack_002";

	private Status _status;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private CharacterMaterial cm;

	[SerializeField]
	private Transform transformShootPoint;

	[SerializeField]
	private Transform transformShootFxPoint;

	[SerializeField]
	private string JumpInSE = "fo2_return";

	private readonly int ANI_APPEAR_FRAME = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int WAITING_SKL_FRAME = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int WAITING_LOCK_FRAME = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int hashVelocityY = Animator.StringToHash("fVelocityY");

	private System.Collections.Generic.Dictionary<HumanBase.AnimateId, int> dictAnimateHash = new Better.Dictionary<HumanBase.AnimateId, int>();

	private bool isIdle = true;

	public Vector3 localDefaultPos = new Vector3(0f, 0f, -0.8f);

	public Vector3 localDefaultEuler = new Vector3(0f, 95f, 0f);

	public Vector3 localWallPos = new Vector3(-0.028f, 0f, -0.725f);

	public Vector3 localWallEulerL = new Vector3(0f, -111.384f, 1f);

	public Vector3 localWallEulerR = new Vector3(0f, -66.384f, 1f);

	private OrangeCharacter _refEntity;

	private SKILL_TABLE sklBullet;

	private WeaponStatus bulletStatus = new WeaponStatus();

	private int queueCount = 1;

	private readonly int queueCountMax = 1;

	private Queue<EntityCacheData> queueCaches = new Queue<EntityCacheData>();

	private Vector3 nextPos = Vector3.zero;

	private float distanceDelta;

	private int nextFrame;

	private bool waitExFrame;

	private bool isEntitySlash;

	private EntityCacheData preCacheData = new EntityCacheData
	{
		CurMainStatus = OrangeCharacter.MainStatus.NONE
	};

	private AniSpeedData tAniSpeedData = new AniSpeedData();

	public bool CanUseSkill
	{
		get
		{
			if (!isIdle && _status != Status.JumpIn)
			{
				return _status == Status.DoNothing;
			}
			return true;
		}
	}

	private bool IsIdleStatus(OrangeCharacter.MainStatus mainStatus)
	{
		if (mainStatus != 0 && mainStatus != OrangeCharacter.MainStatus.CROUCH)
		{
			return mainStatus == OrangeCharacter.MainStatus.SLASH;
		}
		return true;
	}

	private void Awake()
	{
		InitAnimateHash();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_IN, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_OUT, 2);
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	private void Start()
	{
		if ((bool)cm)
		{
			cm.isAllowHurtEffect = false;
			cm.IsAllowInvincibleEffect = false;
		}
	}

	public void LogicUpdate()
	{
		int gameFrame = GameLogicUpdateManager.GameFrame;
		switch (_status)
		{
		case Status.Idle:
			isIdle = true;
			break;
		case Status.JumpOut:
			if (gameFrame >= nextFrame)
			{
				nextFrame = gameFrame + WAITING_SKL_FRAME;
				_status = Status.Waiting;
				cm.Disappear(null, 0.01f);
			}
			break;
		case Status.Waiting:
			if ((bool)_refEntity)
			{
				distanceDelta = 0f;
				nextPos = _refEntity.transform.position + localDefaultPos;
				base.transform.position = nextPos;
			}
			if (gameFrame < nextFrame)
			{
				break;
			}
			if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive)
			{
				waitExFrame = true;
				break;
			}
			if (waitExFrame)
			{
				waitExFrame = false;
				nextFrame = gameFrame + WAITING_LOCK_FRAME;
				break;
			}
			queueCaches.Clear();
			nextFrame = gameFrame + ANI_APPEAR_FRAME;
			_status = Status.JumpIn;
			if ((bool)_refEntity && (_refEntity.UsingVehicle || _refEntity.IsDead()))
			{
				_status = Status.Idle;
				break;
			}
			PlayAnimation((HumanBase.AnimateId)68u);
			cm.Appear(null, 0.01f);
			if ((bool)_refEntity)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_IN, _refEntity.transform.position + localDefaultPos + new Vector3(0f, 0.5f, 0f), Quaternion.identity, Array.Empty<object>());
				_refEntity.PlaySE(_refEntity.SkillSEID, JumpInSE);
			}
			break;
		case Status.JumpIn:
			if (gameFrame >= nextFrame)
			{
				queueCaches.Clear();
				SaveCache();
				_status = Status.Idle;
			}
			break;
		case Status.DoNothing:
			break;
		}
	}

	private void InitAnimateHash()
	{
		dictAnimateHash.Clear();
		int value = Animator.StringToHash("ch134_Voss_idle");
		foreach (HumanBase.AnimateId value2 in Enum.GetValues(typeof(HumanBase.AnimateId)))
		{
			dictAnimateHash.Add(value2, value);
		}
		dictAnimateHash[HumanBase.AnimateId.ANI_JUMP] = Animator.StringToHash("ch134_Voss_jump_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_FALL] = Animator.StringToHash("ch134_Voss_fall");
		dictAnimateHash[HumanBase.AnimateId.ANI_LAND] = Animator.StringToHash("ch134_Voss_landing");
		dictAnimateHash[HumanBase.AnimateId.ANI_STEP] = Animator.StringToHash("ch134_Voss_run");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALK] = Animator.StringToHash("ch134_Voss_run");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALKBACK] = Animator.StringToHash("ch134_Voss_run_back");
		dictAnimateHash[HumanBase.AnimateId.ANI_DASH] = Animator.StringToHash("ch134_Voss_dash_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_DASH_END] = Animator.StringToHash("ch134_Voss_dash_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_SLIDE] = Animator.StringToHash("ch134_Voss_dash_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_SLIDE_END] = Animator.StringToHash("ch134_Voss_dash_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_AIRDASH_END] = Animator.StringToHash("ch134_Voss_dash_end");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALLKICK] = Animator.StringToHash("ch134_Voss_jump_start");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALLKICK_END] = Animator.StringToHash("ch134_Voss_jump_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALLGRAB_BEGIN] = Animator.StringToHash("ch134_Voss_jump_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALLGRAB] = Animator.StringToHash("ch134_Voss_jump_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALLGRAB_END] = Animator.StringToHash("ch134_Voss_jump_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_JUMPSLASH] = Animator.StringToHash("ch134_Voss_rotate_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_DASHSLASH1] = Animator.StringToHash("ch134_Voss_rotate_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_DASHSLASH1_END] = Animator.StringToHash("ch134_Voss_rotate_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_DASHSLASH2] = Animator.StringToHash("ch134_Voss_rotate_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_DASHSLASH2_END] = Animator.StringToHash("ch134_Voss_rotate_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALKSLASH1] = Animator.StringToHash("ch134_Voss_run");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALKSLASH1_END] = Animator.StringToHash("ch134_Voss_run");
		dictAnimateHash[HumanBase.AnimateId.ANI_WALKSLASH2] = Animator.StringToHash("ch134_Voss_run");
		dictAnimateHash[HumanBase.AnimateId.ANI_HURT_BEGIN] = Animator.StringToHash("ch134_Voss_jump_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_HURT_LOOP] = Animator.StringToHash("ch134_Voss_jump_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_HURT_END] = Animator.StringToHash("ch134_Voss_jump_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_TELEPORT_IN_POSE] = Animator.StringToHash("ch134_Voss_login");
		dictAnimateHash[HumanBase.AnimateId.ANI_WIN_POSE] = Animator.StringToHash("ch134_Voss_win");
		dictAnimateHash[HumanBase.AnimateId.ANI_TELEPORT_OUT_POSE] = Animator.StringToHash("ch134_Voss_logout");
		dictAnimateHash[HumanBase.AnimateId.ANI_LOGOUT2] = Animator.StringToHash("ch134_Voss_logout");
		dictAnimateHash[HumanBase.AnimateId.ANI_GIGA_JUMP_START] = Animator.StringToHash("ch134_Voss_rotate_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_GIGA_JUMP_END] = Animator.StringToHash("ch134_Voss_rotate_loop");
		dictAnimateHash[HumanBase.AnimateId.ANI_SKILL_START] = Animator.StringToHash("ch134_Voss_skill_02_start");
		dictAnimateHash[(HumanBase.AnimateId)66u] = Animator.StringToHash("ch134_Voss_skill_02_loop");
		dictAnimateHash[(HumanBase.AnimateId)67u] = Animator.StringToHash("ch134_Voss_skill_02_end");
		dictAnimateHash[(HumanBase.AnimateId)68u] = Animator.StringToHash("ch134_Voss_visible");
		dictAnimateHash[(HumanBase.AnimateId)69u] = Animator.StringToHash("ch134_Voss_Invisible");
		dictAnimateHash[(HumanBase.AnimateId)70u] = Animator.StringToHash("ch134_Voss_skill_01_loop");
	}

	public void SetEntity(OrangeCharacter p_refEntity)
	{
		_refEntity = p_refEntity;
		base.transform.SetParentNull();
		nextPos = _refEntity.transform.position + localDefaultPos;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(23017, out sklBullet))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref sklBullet);
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<BasicBullet>("prefab/bullet/" + sklBullet.s_MODEL, sklBullet.s_MODEL, 3, null);
			bulletStatus.CopyWeaponStatus(_refEntity.PlayerWeapons[0].weaponStatus, 0);
		}
	}

	public void TryPlayAnimation()
	{
		if (!isIdle || !_refEntity)
		{
			return;
		}
		if (_refEntity.IsTeleporting)
		{
			nextPos = _refEntity.transform.position;
			PlayAnimation((HumanBase.AnimateId)_refEntity.AnimationParams.AnimateUpperID);
			return;
		}
		if (queueCaches.Count < queueCount)
		{
			PlayAnimation(HumanBase.AnimateId.ANI_STAND);
			SaveCache();
			return;
		}
		SaveCache();
		EntityCacheData entityCacheData = queueCaches.Dequeue();
		OrangeCharacter.MainStatus curMainStatus = entityCacheData.CurMainStatus;
		short animateId = entityCacheData.AnimateId;
		OrangeCharacter.SubStatus curSubStatus = entityCacheData.CurSubStatus;
		sbyte isShoot = entityCacheData.IsShoot;
		int velocityY = entityCacheData.VelocityY;
		int dir = entityCacheData.Dir;
		if (IsIdleStatus(entityCacheData.CurMainStatus) && IsIdleStatus(_refEntity.CurMainStatus))
		{
			queueCount = 0;
			nextPos = _refEntity.transform.position;
		}
		else
		{
			queueCount = queueCountMax;
			nextPos = entityCacheData.Point;
		}
		switch (curMainStatus)
		{
		case OrangeCharacter.MainStatus.SLASH:
			nextPos += localDefaultPos;
			PlayAnimation((HumanBase.AnimateId)70u);
			PlayShootAnim(dir, true);
			return;
		case OrangeCharacter.MainStatus.SKILL:
			nextPos += localDefaultPos;
			return;
		case OrangeCharacter.MainStatus.CROUCH:
			nextPos += localDefaultPos;
			if ((uint)(curSubStatus - 2) <= 1u)
			{
				PlayShootAnim(dir, true);
				return;
			}
			if (isShoot == 1)
			{
				PlayShootAnim(dir, true);
				return;
			}
			break;
		case OrangeCharacter.MainStatus.IDLE:
			nextPos += localDefaultPos;
			if (isShoot == 1)
			{
				PlayShootAnim(dir, false);
				return;
			}
			break;
		case OrangeCharacter.MainStatus.WALLGRAB:
			if (dir == 1)
			{
				base.transform.localRotation = Quaternion.Euler(localWallEulerL);
			}
			else
			{
				base.transform.localRotation = Quaternion.Euler(localWallEulerR);
			}
			nextPos += localWallPos;
			break;
		default:
			base.transform.localRotation = Quaternion.Euler(localDefaultEuler);
			nextPos += localDefaultPos;
			break;
		}
		if (dir == 1)
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		else
		{
			base.transform.localScale = new Vector3(1f, 1f, -1f);
		}
		distanceDelta = Vector3.Distance(base.transform.localPosition, nextPos) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		animator.SetFloat(hashVelocityY, velocityY);
		PlayAnimation((HumanBase.AnimateId)animateId);
	}

	private void PlayShootAnim(int _entityDir, bool _isEntitySlash)
	{
		isEntitySlash = _isEntitySlash;
		PlayAnimation((HumanBase.AnimateId)70u);
		if (_entityDir == 1)
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		else
		{
			base.transform.localScale = new Vector3(1f, 1f, -1f);
		}
		distanceDelta = Vector3.Distance(base.transform.localPosition, nextPos) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
	}

	private void SaveCache()
	{
		EntityCacheData entityCacheData = default(EntityCacheData);
		entityCacheData.AnimateId = _refEntity.AnimationParams.AnimateUpperID;
		entityCacheData.CurMainStatus = _refEntity.CurMainStatus;
		entityCacheData.CurSubStatus = _refEntity.CurSubStatus;
		entityCacheData.Point = _refEntity.transform.position;
		entityCacheData.VelocityY = _refEntity.Velocity.y;
		entityCacheData.IsShoot = _refEntity.IsShoot;
		entityCacheData.Dir = _refEntity.direction;
		EntityCacheData item = entityCacheData;
		queueCaches.Enqueue(item);
		preCacheData = item;
	}

	public void UpdateFunc()
	{
		if (isIdle)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, nextPos, distanceDelta);
		}
	}

	public void UseSkill01()
	{
		SetJumpOut();
	}

	public void UseSkill02()
	{
		SetJumpOut();
	}

	public void Logout()
	{
		cm.ChangeDissolveTime(0.8f);
	}

	public void PlayAnimation(HumanBase.AnimateId animateId)
	{
		animator.Play(dictAnimateHash[animateId], 0);
	}

	private void SetJumpOut()
	{
		PlayAnimation((HumanBase.AnimateId)69u);
		nextFrame = GameLogicUpdateManager.GameFrame + ANI_APPEAR_FRAME;
		isIdle = false;
		_status = Status.JumpOut;
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_OUT, animator.transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity, Array.Empty<object>());
	}

	public void AnimationEvent_ShootBullet()
	{
		if (!transformShootPoint || !_refEntity || sklBullet == null)
		{
			return;
		}
		Vector3 vector = _refEntity.ShootDirection;
		if (isEntitySlash)
		{
			Vector3? vector2 = _refEntity.CalibrateAimDirection(_refEntity.AimPosition);
			if (vector2.HasValue)
			{
				int num = Math.Sign(vector2.Value.x);
				if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(vector2.Value.x) > 0.05f)
				{
					vector = ((_refEntity._characterDirection != CharacterDirection.RIGHT) ? Vector3.left : Vector3.right);
				}
			}
		}
		_refEntity.PushBulletDetail(sklBullet, bulletStatus, transformShootPoint.position, 0, vector);
		if ((bool)transformShootFxPoint && !ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(sklBullet.s_USE_FX))
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sklBullet.s_USE_FX, transformShootFxPoint.position, Quaternion.Euler(vector), Array.Empty<object>());
		}
	}

	public void Appear()
	{
		_status = Status.DoNothing;
		isIdle = false;
		PlayAnimation((HumanBase.AnimateId)68u);
		cm.Appear(SetStatusToIdle, 0.01f);
		nextPos = _refEntity.transform.position + localDefaultPos;
		base.transform.position = nextPos;
		queueCaches.Clear();
		SaveCache();
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_IN, animator.transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity, Array.Empty<object>());
		_refEntity.PlaySE(_refEntity.SkillSEID, JumpInSE);
	}

	private void SetStatusToIdle()
	{
		_status = Status.Idle;
	}

	public void Disappear()
	{
		_status = Status.Idle;
		cm.Disappear(null, 0.01f);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_OUT, animator.transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity, Array.Empty<object>());
	}

	public void LockAnimator(bool bLock)
	{
		if (bLock)
		{
			tAniSpeedData = new AniSpeedData();
			tAniSpeedData.tAniPlayer = animator;
			tAniSpeedData.fSpeed = tAniSpeedData.tAniPlayer.speed;
			tAniSpeedData.fSetSeed = 0f;
			tAniSpeedData.tAniPlayer.speed = tAniSpeedData.fSetSeed;
		}
		else if (tAniSpeedData.tAniPlayer.speed == tAniSpeedData.fSetSeed)
		{
			tAniSpeedData.tAniPlayer.speed = tAniSpeedData.fSpeed;
		}
	}
}
