using System;
using System.Collections;
using UnityEngine;

public class CH130_Controller : CharacterControlBase, ILogicUpdate
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private int endBreakFrame;

	private bool isSkillEventEnd;

	private SKILL_TABLE linkSkl1;

	protected IAimTarget _pRushTarget;

	protected Transform _tfHitTransform;

	protected Vector3 _vSkillStartPosition;

	protected Vector2 _vSkillVelocity;

	protected RushCollideBullet _pRushCollideBullet;

	protected bool _bSyncRushSkillCompleted;

	protected Vector3 _vSyncDirection;

	private int sklFrenzyId = 22661;

	private int conditionId = -1;

	private SKILL_TABLE linkSklPassive;

	protected ShieldBullet _pShieldBullet;

	private ParticleSystem _regularEffect;

	private ParticleSystem[] _FrenzyEffect;

	private bool isInit;

	private OrangeConsoleCharacter _refPlayer;

	private readonly string FX_0_00 = "fxuse_blackbombs_000";

	private readonly string FX_1_00 = "fxuse_GoreICO_000";

	private readonly string FX_2_00 = "fxhit_barrage_003";

	protected readonly int SKL0_TRIGGER = (int)(0.292f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END = (int)(0.833f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL0_END_BREAK = (int)(0.62f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_START = (int)(0.067f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_LOOP = (int)(0.35 / (double)GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_TRIGGER = (int)(0.26f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_BREAK = (int)(0.9f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_MISS = (int)(0.6f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_MISS_JUMP = (int)(0.45f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int SKL1_END_MISS_BREAK = (int)(0.33f / GameLogicUpdateManager.m_fFrameLen);

	protected readonly int FRENZY_TIMER = (int)(2f / GameLogicUpdateManager.m_fFrameLen);

	private void OnEnable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		_refEntity.teleportInVoicePlayed = true;
	}

	private void InitializeSkill()
	{
		Transform transform = new GameObject("CustomShootPoint0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 0.8f, 0f);
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = transform;
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.PlayerSkills[0].ShootTransform[0] = _refEntity.ExtraTransforms[0];
		_refEntity.PlayerSkills[1].ShootTransform[0] = _refEntity.ExtraTransforms[1];
		SKILL_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(sklFrenzyId, out value))
		{
			conditionId = value.n_CONDITION_ID;
			if (value.n_LINK_SKILL != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(value.n_LINK_SKILL, out linkSklPassive))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref linkSklPassive);
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<ShieldBullet>("prefab/bullet/" + linkSklPassive.s_MODEL, linkSklPassive.s_MODEL, 3, PreloadLinkComplete);
			}
		}
		ManagedSingleton<CharacterControlHelper>.Instance.PreloadLinkSkl<CollideBullet>(_refEntity, 1, out linkSkl1);
		_regularEffect = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_GoreICO_000").GetComponent<ParticleSystem>();
		_regularEffect.Play(true);
		_FrenzyEffect = new ParticleSystem[2];
		_FrenzyEffect[0] = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_barrage_000_L").GetComponent<ParticleSystem>();
		_FrenzyEffect[1] = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_barrage_000_R").GetComponent<ParticleSystem>();
		_FrenzyEffect[0].Stop(true);
		_FrenzyEffect[1].Stop(true);
		_refPlayer = _refEntity as OrangeConsoleCharacter;
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_0_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_1_00, 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(FX_2_00, 2);
	}

	private void PreloadLinkComplete()
	{
		isInit = true;
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.PlayTeleportOutEffectEvt = PlayTeleportOutEffect;
	}

	private void PlayTeleportOutEffect()
	{
		Vector3 p_worldPos = base.transform.position;
		if (_refEntity != null)
		{
			p_worldPos = _refEntity.AimPosition;
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("FX_TELEPORT_OUT", p_worldPos, Quaternion.identity, Array.Empty<object>());
	}

	public void LogicUpdate()
	{
		if (isInit)
		{
			CheckFrenzyBuff();
			CheckRushBuff();
		}
	}

	private void CheckFrenzyBuff()
	{
		if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(conditionId))
		{
			if (_pShieldBullet == null)
			{
				CreateShieldBullet();
				PlayFrenzyFx();
			}
		}
		else if (_pShieldBullet != null)
		{
			_pShieldBullet.BackToPool();
			_pShieldBullet = null;
			StopFrenzyFx();
		}
	}

	private void CheckRushBuff()
	{
		if (_pRushCollideBullet != null && !_refEntity.IsLocalPlayer && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			RecoverRushCollideBullet();
		}
	}

	private void CreateShieldBullet()
	{
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		_pShieldBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ShieldBullet>(linkSklPassive.s_MODEL);
		_pShieldBullet.UpdateBulletData(linkSklPassive, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
		_pShieldBullet.SetBulletAtk(weaponStruct.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
		_pShieldBullet.BulletLevel = 0;
		_pShieldBullet.BindBuffId(conditionId, _refEntity.IsLocalPlayer, false);
		_pShieldBullet.Active(_refEntity.ExtraTransforms[0], Quaternion.identity, _refEntity.TargetMask, true);
	}

	private void PlayFrenzyFx()
	{
		_FrenzyEffect[0].Play(true);
		_FrenzyEffect[1].Play(true);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_2_00, _refEntity.AimTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
		PlaySkillSE("ic2_black01_lp");
	}

	private void StopFrenzyFx()
	{
		_FrenzyEffect[0].Stop(true);
		_FrenzyEffect[1].Stop(true);
		PlaySkillSE("ic2_black01_stop");
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				isSkillEventEnd = false;
				_refEntity.IsShoot = 1;
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, OrangeCharacter.SubStatus.SKILL0, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
			}
			break;
		case 1:
			if (!_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			_refEntity.PlayerStopDashing();
			_refEntity.SetSpeed(0, 0);
			_refEntity.IsShoot = 1;
			_bSyncRushSkillCompleted = false;
			_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0f);
			if (_refEntity.IsLocalPlayer)
			{
				if (_refEntity.UseAutoAim && _refEntity.PlayerAutoAimSystem.AutoAimTarget != null)
				{
					ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
					_pRushTarget = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
					_vSkillVelocity = (_pRushTarget.AimPosition - _refEntity.AimPosition).normalized * GetRushSpd();
					UpdateSkill1Direction(_vSkillVelocity.x);
				}
				else
				{
					_pRushTarget = null;
					_vSkillVelocity = _refEntity.ShootDirection.normalized * GetRushSpd();
					UpdateSkill1Direction(_vSkillVelocity.x);
				}
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				OrangeBattleUtility.UpdateSkillCD(weaponStruct);
				_refEntity.CheckUsePassiveSkill(1, weaponStruct.BulletData, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
			}
			isSkillEventEnd = false;
			_vSkillStartPosition = _refEntity.AimPosition;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			_refEntity.IgnoreGravity = true;
			break;
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged())
		{
			return;
		}
		UpdateVirtualButtonAnalog();
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		nowFrame = GameLogicUpdateManager.GameFrame;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[0];
				ManagedSingleton<CharacterControlHelper>.Instance.UpdateShootDirByAimDir(_refEntity);
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, weaponStruct2.ShootTransform[0], MagazineType.ENERGY, -1, 1);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame && (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT)))
			{
				endFrame = nowFrame + 1;
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (_refEntity.CurrentFrame > 1f)
			{
				bool flag3 = false;
				if (_refEntity.IsLocalPlayer)
				{
					flag3 = true;
				}
				else if (_bSyncRushSkillCompleted)
				{
					_bSyncRushSkillCompleted = false;
					flag3 = true;
				}
				if (flag3)
				{
					WeaponStruct weaponStruct3 = _refEntity.PlayerSkills[1];
					_refEntity.PushBulletDetail(weaponStruct3.BulletData, weaponStruct3.weaponStatus, _refEntity.AimTransform, weaponStruct3.SkillLV, _vSkillVelocity, true, null, RushSkillHitCB);
					_vSkillVelocity = _vSkillVelocity.normalized * GetRushSpd();
					UpdateSkill1Direction(_vSkillVelocity.x);
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
					_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
					_refEntity.IgnoreGravity = true;
					_refEntity.SetSpeed((int)_vSkillVelocity.x, (int)_vSkillVelocity.y);
					endFrame = GameLogicUpdateManager.GameFrame + SKL1_START;
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_1_00, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
		{
			bool flag = false;
			bool flag2 = false;
			if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
			{
				flag = true;
				flag2 = true;
			}
			else if (Vector2.Distance(_vSkillStartPosition, _refEntity.AimPosition) > _refEntity.PlayerSkills[1].BulletData.f_DISTANCE || _refEntity.PlayerSkills[1].LastUseTimer.GetMillisecond() > 350)
			{
				flag = true;
				flag2 = false;
				_pRushTarget = null;
			}
			if (flag)
			{
				if (!_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetSpeed(_refEntity.direction * OrangeCharacter.WalkSpeed, 0);
				}
				else
				{
					_refEntity.SetSpeed(0, 0);
				}
				RecoverRushCollideBullet();
				if (flag2)
				{
					isSkillEventEnd = false;
					endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, 1, SKL1_END_TRIGGER, SKL1_END, OrangeCharacter.SubStatus.SKILL1_3, out skillEventFrame, out endFrame);
					ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)70u, (HumanBase.AnimateId)70u, (HumanBase.AnimateId)72u);
				}
				else
				{
					endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_MISS_BREAK;
					int num = (_refEntity.IsInGround ? SKL1_END_MISS : SKL1_END_MISS_JUMP);
					ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, 1, num, num, OrangeCharacter.SubStatus.SKILL1_2, out skillEventFrame, out endFrame);
					_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				}
			}
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
				_refEntity.PushBulletDetail(linkSkl1, weaponStruct.weaponStatus, _refEntity.ModelTransform, weaponStruct.SkillLV);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				ManagedSingleton<CharacterControlHelper>.Instance.CheckBreakFrame(_refEntity.UserID, ref endFrame);
			}
			break;
		}
	}

	private void OnSkillEnd()
	{
		if (_refEntity.IgnoreGravity)
		{
			_refEntity.IgnoreGravity = false;
		}
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		RecoverRushCollideBullet(true);
		HumanBase.AnimateId animateID = _refEntity.AnimateID;
		if (animateID != HumanBase.AnimateId.ANI_SKILL_START)
		{
			if (_refEntity.IsInGround)
			{
				_refEntity.Dashing = false;
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
		}
		else
		{
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
		}
	}

	public override void ClearSkill()
	{
		RecoverRushCollideBullet(true);
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public override void SetStun(bool enable)
	{
		base.SetStun(enable);
		_refEntity.EnableCurrentWeapon();
	}

	public override void ControlCharacterDead()
	{
		ToggleRegularEffect(false);
	}

	public override void ControlCharacterContinue()
	{
		StartCoroutine(OnToggleRegularEffect(true, 0.6f));
	}

	private void TeleportOutCharacterDepend()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			float currentFrame = _refEntity.CurrentFrame;
			if (currentFrame > 1.5f && currentFrame <= 2f)
			{
				ToggleRegularEffect(false);
			}
		}
	}

	protected void StageTeleportInCharacterDepend()
	{
		ToggleRegularEffect(false);
		StartCoroutine(OnToggleRegularEffect(true, 0.6f));
	}

	protected void StageTeleportOutCharacterDepend()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.TELEPORT_OUT)
		{
			StartCoroutine(OnToggleRegularEffect(false, 0.2f));
		}
		else if (!_refEntity.Animator.IsDefaultAnimator)
		{
			StartCoroutine(OnToggleRegularEffect(false, 0.2f));
		}
	}

	private IEnumerator OnToggleRegularEffect(bool isActive, float delay)
	{
		yield return new WaitForSeconds(delay);
		ToggleRegularEffect(isActive);
	}

	private void ToggleRegularEffect(bool isActive)
	{
		if (isActive)
		{
			_regularEffect.Play(true);
		}
		else
		{
			_regularEffect.Stop(true);
		}
	}

	private float GetRushSpd()
	{
		return (float)OrangeCharacter.DashSpeed * 4f;
	}

	private void UpdateVirtualButtonAnalog()
	{
		if (_refEntity.IsLocalPlayer && _refPlayer != null)
		{
			_refPlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, true);
		}
	}

	private void RushSkillHitCB(object obj)
	{
		if (!_refEntity.IsLocalPlayer && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			return;
		}
		if (_refEntity.UsingVehicle)
		{
			RecoverRushCollideBullet(true);
			return;
		}
		Collider2D collider2D = obj as Collider2D;
		if (collider2D == null)
		{
			return;
		}
		Transform transform = collider2D.transform;
		StageObjParam stageObjParam = transform.GetComponent<StageObjParam>();
		if (stageObjParam == null)
		{
			PlayerCollider component = transform.GetComponent<PlayerCollider>();
			if (component != null && component.IsDmgReduceShield())
			{
				stageObjParam = component.GetDmgReduceOwner();
			}
		}
		if (stageObjParam == null || stageObjParam.tLinkSOB == null)
		{
			return;
		}
		OrangeCharacter orangeCharacter = stageObjParam.tLinkSOB as OrangeCharacter;
		EnemyControllerBase enemyControllerBase = stageObjParam.tLinkSOB as EnemyControllerBase;
		if (!orangeCharacter && !enemyControllerBase)
		{
			return;
		}
		RecoverRushCollideBullet();
		if (_refEntity.IsLocalPlayer)
		{
			if ((bool)orangeCharacter)
			{
				_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0, false, orangeCharacter.sPlayerID);
				_pRushTarget = orangeCharacter;
			}
			else
			{
				_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0);
				_pRushTarget = enemyControllerBase;
			}
			_tfHitTransform = transform;
		}
	}

	private void UpdateSkill1Direction(float dirX)
	{
		if (Mathf.Abs(dirX) > 0.05f)
		{
			int num = Math.Sign(dirX);
			if (num != _refEntity.direction)
			{
				_refEntity.direction = num;
			}
		}
	}

	public override void SetRushBullet(RushCollideBullet rushCollideBullet)
	{
		_pRushCollideBullet = rushCollideBullet;
		if (_refEntity.UsingVehicle)
		{
			_pRushTarget = null;
			RecoverRushCollideBullet(true);
		}
	}

	public override void SyncSkillDirection(Vector3 dir, IAimTarget target)
	{
		if (_refEntity.UsingVehicle)
		{
			_pRushTarget = null;
			RecoverRushCollideBullet(true);
			return;
		}
		_bSyncRushSkillCompleted = true;
		_vSyncDirection = dir;
		_pRushTarget = target;
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1)
		{
			_vSkillVelocity = _vSyncDirection.normalized * GetRushSpd();
			UpdateSkill1Direction(_vSkillVelocity.x);
		}
	}

	protected void RecoverRushCollideBullet(bool claerBuff = false)
	{
		if (claerBuff && _refEntity.IsLocalPlayer && _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(-1);
		}
		if ((bool)_pRushCollideBullet)
		{
			_pRushCollideBullet.BackToPool();
			_pRushCollideBullet.HitCallback = null;
			_pRushCollideBullet = null;
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[8] { "ch130_skill_01_crouch", "ch130_skill_01_stand", "ch130_skill_01_jump", "ch130_skill_02_start", "ch130_skill_02_loop", "ch130_skill_02_end", "ch130_skill_02_end_miss", "ch130_skill_02_jump_end" };
	}
}
