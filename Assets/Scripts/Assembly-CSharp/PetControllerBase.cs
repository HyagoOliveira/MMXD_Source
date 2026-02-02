using System.Collections.Generic;
using StageLib;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class PetControllerBase : StageNetSyncObj, IAimTarget, ILogicUpdate, IManagedUpdateBehavior
{
	protected bool bLockByStageCtrlEvent;

	[SerializeField]
	public string[] activeSE;

	[SerializeField]
	public string[] unactiveSE;

	[SerializeField]
	public string[] boomSE;

	[SerializeField]
	public string[] hitSE;

	public OrangeCharacter _follow_Player;

	public int follow_skill_id;

	public PetHumanBase.FollowType _FollowType;

	protected VInt3 mFollow_Target;

	protected bool IgnoreGlobalVelocity;

	public bool IgnoreGravity = true;

	protected Vector3 FollowOffset;

	protected bool FollowEnabled = true;

	protected bool b_follow_pos_end = true;

	protected OrangeTimer _animationTimer;

	protected PetHumanBase.PetAnimateId _animateID;

	protected PetHumanBase.PetAnimateId _animateIDPrev;

	protected PetHumanBase.MainStatus _mainStatus;

	protected PetHumanBase.SubStatus _subStatus;

	private LayerMask s_collisionMask;

	private LayerMask s_wallkickMask;

	private LayerMask s_collisionMaskThrough;

	protected new int direction = -1;

	protected VInt3 _velocity;

	protected VInt3 _velocityExtra;

	protected VInt3 _velocityShift;

	protected VInt _maxGravity;

	protected PetAutoAimSystem _autoAim;

	protected float distanceDelta;

	protected float _currentFrame;

	protected CharacterMaterial _characterMaterial;

	public int PetID;

	[HideInInspector]
	public PET_TABLE PetTable;

	protected int EquippedWeaponNum;

	protected PetWeaponStruct[] PetWeapons;

	protected CollideBullet _collideBullet;

	protected VInt3 OldLogicPosition;

	protected int _nDeactiveType;

	[HideInInspector]
	public AnimationParameters AnimationParams;

	[HideInInspector]
	public PetAnimatorBase AnimatorBase;

	[HideInInspector]
	public Controller2D Controller;

	private bool visible;

	protected OrangeCharacter MainOC;

	private OrangeCriSource _SSource;

	private bool isStarted;

	public override bool Activate
	{
		get
		{
			if (base.Activate)
			{
				return !bLockByStageCtrlEvent;
			}
			return false;
		}
		set
		{
			base.Activate = value;
		}
	}

	public LayerMask TargetMask { get; set; }

	public int DeactiveType
	{
		get
		{
			return _nDeactiveType;
		}
		private set
		{
			_nDeactiveType = value;
		}
	}

	private bool bNextClosePauseFirstUpdate { get; set; }

	protected bool bOnPauseFirstUpdate { get; set; }

	public OrangeCriSource SoundSource
	{
		get
		{
			if (_SSource == null)
			{
				_SSource = base.gameObject.GetComponent<OrangeCriSource>();
				if (_SSource == null)
				{
					_SSource = base.gameObject.AddComponent<OrangeCriSource>();
					_SSource.Initial(OrangeSSType.PET);
				}
			}
			return _SSource;
		}
		set
		{
			OrangeCriSource component = base.gameObject.GetComponent<OrangeCriSource>();
			if (component != null)
			{
				Object.Destroy(component);
			}
			_SSource = value;
		}
	}

	protected virtual void Awake()
	{
		Controller = GetComponent<Controller2D>();
		AnimatorBase = GetComponent<PetAnimatorBase>();
		_animationTimer = OrangeTimerManager.GetTimer();
		AnimatorBase.InitAnimator();
		Transform transform = OrangeBattleUtility.FindChildRecursive(base.transform, "AutoAimSystem");
		_autoAim = transform.gameObject.AddOrGetComponent<PetAutoAimSystem>();
		ModelTransform = OrangeBattleUtility.FindChildRecursive(base.transform, "model");
		_velocity = VInt3.zero;
		_maxGravity = OrangeBattleUtility.FP_MaxGravity;
		_velocityExtra = VInt3.zero;
		_velocityShift = VInt3.zero;
		_transform = base.transform;
		s_collisionMask = Controller.collisionMask;
		s_wallkickMask = Controller.wallkickMask;
		s_collisionMaskThrough = Controller.collisionMaskThrough;
		SetCollider2D(false);
		SetController2D(false);
		SetFollowOffset(new Vector3(1f, 1f, 0f));
		SetFollowEnabled(true);
		_characterMaterial = GetComponent<CharacterMaterial>();
		if ((bool)_characterMaterial)
		{
			_characterMaterial.DefaultDissolveValue = 1;
		}
		SetStatus(PetHumanBase.MainStatus.IDLE, PetHumanBase.SubStatus.IDLE);
		Singleton<GenericEventManager>.Instance.AttachEvent<GameObject, StageCtrlInsTruction>(EventManager.ID.STAGE_OBJ_CTRL, ObjCtrl);
		Singleton<GenericEventManager>.Instance.AttachEvent<string, int, string>(EventManager.ID.STAGE_OBJ_CTRL_PET_ACTION, ObjCtrl_PetAction);
		StageResManager.GetStageUpdate().RegisterStageObjBase(this);
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			bOnPauseFirstUpdate = true;
		}
	}

	public void ObjCtrl(GameObject tObj, StageCtrlInsTruction tSCE)
	{
		if (PetID == 0)
		{
			return;
		}
		switch (tSCE.tStageCtrl)
		{
		case 2:
			if (!(_follow_Player == null) && _follow_Player.gameObject.GetInstanceID() == tObj.GetInstanceID())
			{
				bLockByStageCtrlEvent = false;
			}
			break;
		case 1:
			if (!(_follow_Player == null) && _follow_Player.gameObject.GetInstanceID() == tObj.GetInstanceID())
			{
				bLockByStageCtrlEvent = true;
			}
			break;
		}
	}

	public void ObjCtrl_PetAction(string sTmpNetID, int nTmpID, string nTmpMsg)
	{
		if (PetID != 0 && sTmpNetID == sNetSerialID)
		{
			UpdateStatus(nTmpID, nTmpMsg);
		}
	}

	protected virtual void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<GameObject, StageCtrlInsTruction>(EventManager.ID.STAGE_OBJ_CTRL, ObjCtrl);
		Singleton<GenericEventManager>.Instance.DetachEvent<string, int, string>(EventManager.ID.STAGE_OBJ_CTRL_PET_ACTION, ObjCtrl_PetAction);
		StageUpdate stageUpdate = StageResManager.GetStageUpdate();
		if ((object)stageUpdate != null)
		{
			stageUpdate.UnRegisterStageObjBase(this);
		}
	}

	public virtual void Set_follow_Player(OrangeCharacter mOC, bool linkBuffManager = true)
	{
		_follow_Player = mOC;
		if (tRefPassiveskill == null)
		{
			tRefPassiveskill = _follow_Player.tRefPassiveskill;
		}
		if (linkBuffManager)
		{
			selfBuffManager = _follow_Player.selfBuffManager;
		}
		else
		{
			selfBuffManager.Init(this);
		}
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsMultiply)
		{
			MainOC = StageUpdate.GetMainPlayerOC();
		}
		else
		{
			MainOC = _follow_Player;
		}
	}

	protected virtual void Start()
	{
		if (!isStarted)
		{
			isStarted = true;
			Initialize();
			_collideBullet = GetComponentInChildren<CollideBullet>();
			base.gameObject.SetActive(false);
		}
	}

	public override void ResetStatus()
	{
		Start();
	}

	protected virtual void Initialize()
	{
		PetTable = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[PetID];
		SKILL_TABLE[] petAllSkillData = ManagedSingleton<OrangeTableHelper>.Instance.GetPetAllSkillData(PetTable);
		if (_follow_Player != null)
		{
			sNetSerialID = _follow_Player.sNetSerialID + PetTable.n_ID;
		}
		EquippedWeaponNum = petAllSkillData.Length;
		if (EquippedWeaponNum <= 0)
		{
			return;
		}
		PetWeapons = new PetWeaponStruct[EquippedWeaponNum];
		for (int i = 0; i < EquippedWeaponNum; i++)
		{
			PetWeapons[i] = new PetWeaponStruct();
			PetWeapons[i].BulletData = petAllSkillData[i];
			PetWeapons[i].MagazineRemain = petAllSkillData[i].n_MAGAZINE;
			PetWeapons[i].LastUseTimer = new UpdateTimer();
			if (_follow_Player != null)
			{
				_follow_Player.tRefPassiveskill.ReCalcuSkill(ref PetWeapons[i].BulletData);
			}
		}
		if (_follow_Player != null && _follow_Player.IsLocalPlayer)
		{
			TargetMask = ManagedSingleton<OrangeLayerManager>.Instance.PlayerUseMask;
		}
		_autoAim.targetMask = TargetMask;
		_autoAim.Init(false, true);
		_autoAim.UpdateAimRange(PetWeapons[0].BulletData.f_DISTANCE);
	}

	protected void UpdateMagazine(int id = -1, bool forceUpdate = false)
	{
		if (PetWeapons == null)
		{
			return;
		}
		for (int i = 0; i < PetWeapons.Length; i++)
		{
			if (forceUpdate || PetWeapons[i].LastUseTimer.GetMillisecond() >= PetWeapons[i].BulletData.n_RELOAD)
			{
				PetWeapons[i].MagazineRemain = PetWeapons[i].BulletData.n_MAGAZINE;
			}
		}
	}

	protected void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	protected void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public void LogicLateUpdate()
	{
		AnimationParams.AnimateUpperID = (short)_animateID;
		AnimationParams.AnimateUpperKeepFlag = false;
		_animateIDPrev = _animateID;
		AnimatorBase.SetAnimatorParameters(AnimationParams);
	}

	protected virtual void UpdateGravity()
	{
		if (!IgnoreGravity)
		{
			if ((_velocity.y < 0 && Controller.Collisions.below) || (_velocity.y > 0 && Controller.Collisions.above))
			{
				_velocity.y = 0;
			}
			_velocity.y += OrangeBattleUtility.FP_Gravity * GameLogicUpdateManager.g_fixFrameLenFP / 1000;
			_velocity.y = IntMath.Sign(_velocity.y) * IntMath.Min(IntMath.Abs(_velocity.y), IntMath.Abs(_maxGravity.i));
		}
	}

	public void BaseLogicUpdate(bool ignoreFrameLength = false)
	{
		if (Activate)
		{
			for (int i = 0; i < EquippedWeaponNum; i++)
			{
				PetWeapons[i].LastUseTimer += GameLogicUpdateManager.m_fFrameLenMS;
			}
			float num = (ignoreFrameLength ? 1f : GameLogicUpdateManager.m_fFrameLen);
			if (!IgnoreGlobalVelocity)
			{
				VInt3 globalVelocityExtra = OrangeBattleUtility.GlobalVelocityExtra;
			}
			else
			{
				VInt3 zero = VInt3.zero;
			}
			UpdateGravity();
			Controller.Move((_velocity + _velocityExtra) * num + _velocityShift);
			distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			_velocityExtra = VInt3.zero;
			_velocityShift = VInt3.zero;
		}
	}

	public void UpdateValue()
	{
		LogicLateUpdate();
		AnimationEnd();
		UpdateMagazine();
		if (bNextClosePauseFirstUpdate)
		{
			bOnPauseFirstUpdate = false;
			bNextClosePauseFirstUpdate = false;
		}
		else if (bOnPauseFirstUpdate && !StageUpdate.bWaitReconnect)
		{
			bNextClosePauseFirstUpdate = true;
		}
	}

	protected void SetCollider2D(bool enable)
	{
		if (!enable)
		{
			Controller.Collider2D.enabled = false;
		}
		else
		{
			Controller.Collider2D.enabled = true;
		}
	}

	private void SetController2D(bool enable)
	{
		if (!enable)
		{
			Controller.collisionMask = 0;
			Controller.wallkickMask = 0;
			Controller.collisionMaskThrough = 0;
		}
		else
		{
			Controller.collisionMask = s_collisionMask;
			Controller.wallkickMask = s_wallkickMask;
			Controller.collisionMaskThrough = s_collisionMaskThrough;
		}
	}

	public void SetFollowEnabled(bool value)
	{
		FollowEnabled = value;
	}

	public void SetFollowOffset(Vector3 pos)
	{
		FollowOffset = pos;
	}

	protected virtual void FollowEnd()
	{
	}

	protected virtual void UpdateFollowPos()
	{
		if (!FollowEnabled)
		{
			return;
		}
		if (_follow_Player._characterDirection == CharacterDirection.RIGHT)
		{
			mFollow_Target = new VInt3(new Vector3(_follow_Player.transform.position.x - FollowOffset.x, _follow_Player.transform.position.y + FollowOffset.y, _follow_Player.transform.position.z));
		}
		else
		{
			mFollow_Target = new VInt3(new Vector3(_follow_Player.transform.position.x + FollowOffset.x, _follow_Player.transform.position.y + FollowOffset.y, _follow_Player.transform.position.z));
		}
		if (Controller.LogicPosition.x != mFollow_Target.x)
		{
			int num = mFollow_Target.x - Controller.LogicPosition.x;
			if (num > 5 || num < -5)
			{
				_velocity.x = (mFollow_Target.x - Controller.LogicPosition.x) * 5;
			}
			else
			{
				_velocity.x = mFollow_Target.x - Controller.LogicPosition.x;
			}
			b_follow_pos_end = false;
		}
		else
		{
			_velocity.x = 0;
		}
		if (Controller.LogicPosition.y != mFollow_Target.y)
		{
			int num2 = mFollow_Target.y - Controller.LogicPosition.y;
			if (num2 > 5 || num2 < -5)
			{
				_velocity.y = (mFollow_Target.y - Controller.LogicPosition.y) * 5;
			}
			else
			{
				_velocity.y = mFollow_Target.y - Controller.LogicPosition.y;
			}
			b_follow_pos_end = false;
		}
		else
		{
			_velocity.y = 0;
		}
		if (OldLogicPosition == Controller.LogicPosition)
		{
			if (!b_follow_pos_end)
			{
				FollowEnd();
				b_follow_pos_end = true;
			}
		}
		else
		{
			OldLogicPosition = Controller.LogicPosition;
		}
	}

	public virtual void LogicUpdate()
	{
		if (Activate)
		{
			UpdateValue();
			BaseLogicUpdate();
			UpdateDirecion();
			UpdateFollowPos();
		}
	}

	protected virtual void SetStatusDepend(PetHumanBase.MainStatus mainStatus, PetHumanBase.SubStatus subStatus)
	{
	}

	protected virtual void AnimationEndDepend(PetHumanBase.MainStatus mainStatus, PetHumanBase.SubStatus subStatus)
	{
	}

	protected void SetStatus(PetHumanBase.MainStatus mainStatus, PetHumanBase.SubStatus subStatus)
	{
		_mainStatus = mainStatus;
		_subStatus = subStatus;
		if (_mainStatus == PetHumanBase.MainStatus.IDLE && _subStatus == PetHumanBase.SubStatus.IDLE)
		{
			SetAnimateId(PetHumanBase.PetAnimateId.ANI_STAND);
		}
		SetStatusDepend(_mainStatus, _subStatus);
	}

	public void AnimationEnd()
	{
		if (!(_currentFrame < 1f))
		{
			PetHumanBase.MainStatus mainStatus = _mainStatus;
			PetHumanBase.SubStatus subStatus = _subStatus;
			AnimationEndDepend(mainStatus, subStatus);
		}
	}

	protected void SetAnimateId(PetHumanBase.PetAnimateId id)
	{
		if (_animateID != id)
		{
			_animationTimer.TimerStart();
		}
		_animateID = id;
	}

	public virtual void UpdateFunc()
	{
		if (Activate)
		{
			UpdatePosition();
			_currentFrame = AnimatorBase._animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}
	}

	protected virtual void UpdatePosition()
	{
		_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
	}

	protected bool isSkillAvailable(int skillid)
	{
		if (PetWeapons[skillid].LastUseTimer.GetMillisecond() < PetWeapons[skillid].BulletData.n_FIRE_SPEED || PetWeapons[skillid].MagazineRemain <= 0f)
		{
			return false;
		}
		return true;
	}

	protected bool IsWeaponAvailable(int weaponID)
	{
		if (PetWeapons[weaponID].LastUseTimer.IsStarted() && !(PetWeapons[weaponID].MagazineRemain > 0f))
		{
			return PetWeapons[weaponID].LastUseTimer.GetMillisecond() > PetWeapons[weaponID].BulletData.n_RELOAD;
		}
		return true;
	}

	public virtual void UpdateDirecion()
	{
		if (FollowEnabled)
		{
			int num = new VInt3(_follow_Player.transform.position).x - Controller.LogicPosition.x;
			if (num > 0 && direction != 1)
			{
				direction = 1;
			}
			else if (num < 0 && direction != -1)
			{
				direction = -1;
			}
			if (ModelTransform != null)
			{
				ModelTransform.localScale = new Vector3(1f, 1f, direction);
			}
		}
	}

	public virtual void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		if (bBack)
		{
			direction = -1;
			base.transform.SetPositionAndRotation(pos, new Quaternion(0f, 180f, 0f, 0f));
			Controller.LogicPosition = new VInt3(_transform.position);
		}
		else
		{
			direction = 1;
			base.transform.SetPositionAndRotation(pos, Quaternion.identity);
			Controller.LogicPosition = new VInt3(_transform.position);
		}
	}

	public void SetMaxGravity(VInt value)
	{
		_maxGravity = value;
	}

	public void SetVelocity(VInt3 value)
	{
		_velocity = value;
	}

	public virtual void OnToggleCharacterMaterial(bool appear)
	{
	}

	public virtual void SetActive(bool isActive)
	{
		Controller.enabled = isActive;
		Activate = isActive;
		if (isActive)
		{
			if (_follow_Player != null)
			{
				if (_follow_Player._characterDirection == CharacterDirection.RIGHT)
				{
					SetPositionAndRotation(new Vector3(_follow_Player.transform.position.x - FollowOffset.x, _follow_Player.transform.position.y + FollowOffset.y, _follow_Player.transform.position.z), false);
				}
				else
				{
					SetPositionAndRotation(new Vector3(_follow_Player.transform.position.x + FollowOffset.x, _follow_Player.transform.position.y + FollowOffset.y, _follow_Player.transform.position.z), false);
				}
			}
			_velocityExtra = VInt3.zero;
			for (int i = 0; i < EquippedWeaponNum; i++)
			{
				PetWeapons[i].LastUseTimer.TimerStart();
				PetWeapons[i].MagazineRemain = PetWeapons[i].BulletData.n_MAGAZINE;
			}
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
			}
			AfterActive();
		}
		else
		{
			AfterDeactive();
			SoundSource.Clear();
		}
		base.transform.gameObject.SetActive(isActive);
	}

	protected virtual void AfterActive()
	{
	}

	protected virtual void AfterDeactive()
	{
		if ((bool)_follow_Player)
		{
			_follow_Player.CheckPetDeactiveTrigger(PetID, _nDeactiveType);
		}
	}

	public virtual string[] GetPetDependAnimations()
	{
		return null;
	}

	public virtual string[][] GetPetDependAnimationsBlendTree()
	{
		return null;
	}

	public virtual void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[1] { "null" };
		target = new string[1] { "null" };
	}

	public override int GetSOBType()
	{
		return 4;
	}

	public virtual bool ignoreColliderBullet()
	{
		return false;
	}

	public virtual void UpdateSkillCD(int skillIndex)
	{
		PetWeapons[skillIndex].MagazineRemain -= PetWeapons[skillIndex].BulletData.n_USE_COST;
		PetWeapons[skillIndex].LastUseTimer.TimerStart();
	}

	public virtual void SetParams(string modelName, long lifeTime, int bulletSkillId, WeaponStatus weaponStatus, long debutTime = 0L)
	{
	}

	public virtual void ReplaceListBulletSkillTable(List<SKILL_TABLE> list, bool useRandomSkl = true)
	{
	}

	public void PlaySE(string s_acb, string s_cue, float delay = 0f, bool ForceTrigger = false)
	{
		if (ForceTrigger)
		{
			_SSource.PlaySE(s_acb, s_cue, delay);
		}
		else
		{
			_SSource.ForcePlaySE(s_acb, s_cue, delay);
		}
	}

	private void OnBecameVisible()
	{
		visible = true;
	}

	private void OnBecameInvisible()
	{
		visible = false;
	}
}
