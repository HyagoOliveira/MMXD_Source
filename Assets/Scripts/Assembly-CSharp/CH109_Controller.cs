using System;
using UnityEngine;

public class CH109_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected SKILL_TABLE _tSkill1_LinkSkill;

	protected PlayerAutoAimSystem _pSkill0AimSystem;

	protected IAimTarget _pSkill0Target;

	protected bool _bOldSkill0Flag;

	protected IAimTarget _pRushTarget;

	protected Transform _tfHitTransform;

	protected Vector3 _vSkillStartPosition;

	protected Vector2 _vSkillVelocity;

	protected RushCollideBullet _pRushCollideBullet;

	protected bool _bSyncRushSkillCompleted;

	protected Vector3 _vSyncDirection;

	protected CH100_ShungokusatsuBullet _pShungokusatsuBullet;

	protected FxBase _pShungokusatsuFx;

	protected ParticleSystem _psRushFx;

	private OrangeConsoleCharacter _refPlayer;

	private Transform _tfWind;

	private ParticleSystem _psWindBitL;

	private ParticleSystem _psWindBitR;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[7] { "ch109_skill_01_stand", "ch109_skill_01_jump", "ch109_skill_01_crouch", "ch109_skill_02_satat", "ch109_skill_02_loop", "ch109_skill_02_stand_end", "ch109_skill_02_jump_end" };
	}

	public override void Start()
	{
		base.Start();
		InitLinkSkill();
		InitExtraMeshData();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_tfWind = OrangeBattleUtility.FindChildRecursive(ref target, "WingMesh_g", true);
		_psWindBitL = OrangeBattleUtility.FindChildRecursive(ref target, "bitsL", true).GetComponent<ParticleSystem>();
		_psWindBitR = OrangeBattleUtility.FindChildRecursive(ref target, "bitsR", true).GetComponent<ParticleSystem>();
		GameObject gameObject = new GameObject("Skill0AutoAimSystem");
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localPosition = Vector3.zero;
		_pSkill0AimSystem = gameObject.AddOrGetComponent<PlayerAutoAimSystem>();
		_pSkill0AimSystem.targetMask = _refEntity.PlayerAutoAimSystem.targetMask;
		_pSkill0AimSystem.Init(false, _refEntity.IsLocalPlayer);
		_pSkill0AimSystem.UpdateAimRange(_refEntity.PlayerSkills[0].BulletData.f_DISTANCE);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "p_shungokusatsu_000", true);
		_pShungokusatsuBullet = transform.GetComponent<CH100_ShungokusatsuBullet>();
		_psRushFx = OrangeBattleUtility.FindChildRecursive(ref target, "fxduring_loveico_000_", true).GetComponent<ParticleSystem>();
		if ((bool)_psRushFx)
		{
			_psRushFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_loveico_000", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_loveico_001", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_loveico_002", 5);
		if (_refEntity.PlayerWeapons[0] != null && _refEntity.PlayerWeapons[0].ChipEfx != null)
		{
			_refEntity.PlayerWeapons[0].ChipEfx.MeshActiveColor /= 2f;
		}
		if (_refEntity.PlayerWeapons[1] != null && _refEntity.PlayerWeapons[1].ChipEfx != null)
		{
			_refEntity.PlayerWeapons[1].ChipEfx.MeshActiveColor /= 2f;
		}
	}

	private void InitLinkSkill()
	{
		_refPlayer = _refEntity as OrangeConsoleCharacter;
		if (_tSkill1_LinkSkill == null && _refEntity.PlayerSkills[1].BulletData.n_LINK_SKILL != 0)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[1];
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out _tSkill1_LinkSkill))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref _tSkill1_LinkSkill);
				GameObject obj = new GameObject();
				CollideBullet go = obj.AddComponent<CollideBullet>();
				obj.name = _tSkill1_LinkSkill.s_MODEL;
				obj.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(go, _tSkill1_LinkSkill.s_MODEL, 5);
			}
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
	}

	public override void SetRushBullet(RushCollideBullet rushCollideBullet)
	{
		_pRushCollideBullet = rushCollideBullet;
		if (_refEntity.UsingVehicle)
		{
			_pRushCollideBullet.BackToPool();
			_pRushCollideBullet = null;
		}
	}

	public override void SyncSkillDirection(Vector3 dir, IAimTarget target)
	{
		if (_refEntity.UsingVehicle)
		{
			if ((bool)_pRushCollideBullet)
			{
				_pRushCollideBullet.BackToPool();
				_pRushCollideBullet = null;
			}
			_pRushTarget = null;
			return;
		}
		_bSyncRushSkillCompleted = true;
		_vSyncDirection = dir;
		_pRushTarget = target;
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL1)
		{
			_vSkillVelocity = _vSyncDirection.normalized * (OrangeCharacter.DashSpeed * 4);
			UpdateSkill1Direction(_vSkillVelocity.x);
			if ((bool)_psRushFx)
			{
				UpdateRushFxRotatation(_vSkillVelocity);
				_psRushFx.Play(true);
			}
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill == 0)
		{
			CancelSkill0();
		}
		else if (_refEntity.CurrentActiveSkill == 1)
		{
			CancelSkill1();
		}
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void CheckSkill()
	{
		if (!_refEntity.IsAnimateIDChanged())
		{
			UpdateVirtualButtonAnalog();
			UpdateSkill();
		}
	}

	private void UpdateVirtualButtonAnalog()
	{
		if (_refEntity.IsLocalPlayer && _refPlayer != null)
		{
			_refPlayer.SetVirtualButtonAnalog(VirtualButtonId.SKILL1, true);
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UseSkill0(id);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			UseSkill1(id);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				ToggleWeapon(-2);
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				ToggleWeapon(-3);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				_refEntity.Animator._animator.speed = 2f;
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				_refEntity.SetSpeed((int)_vSkillVelocity.x, (int)_vSkillVelocity.y);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.IgnoreGravity = false;
				if ((bool)_psRushFx)
				{
					_psRushFx.transform.localEulerAngles = Vector3.zero;
					_psRushFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				}
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetSpeed(0, 0);
					_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				}
				else
				{
					_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				}
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SLASH:
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				ToggleWeapon(0);
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				SkillEndChnageToIdle(true);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetSpeed(0, 0);
				SkillEndChnageToIdle();
				break;
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.AimTransform, _pSkill0Target, wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.AimTransform, wsSkill.SkillLV, _vSkillVelocity, true, null, Skill1MoveHitCB);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.BulletData, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			}
		}
	}

	public void CreateShungokusatsuBullet(WeaponStruct wsSkill)
	{
		if (_pShungokusatsuBullet != null && _pRushTarget != null)
		{
			if (_refEntity.IsLocalPlayer && _tfHitTransform != null)
			{
				_pShungokusatsuBullet.SetHitTarget(_tfHitTransform);
			}
			else
			{
				StageObjBase stageObjBase = _pRushTarget as StageObjBase;
				if (stageObjBase != null)
				{
					_pShungokusatsuBullet.SetHitTarget(stageObjBase._transform);
				}
				else
				{
					_pShungokusatsuBullet.SetHitTarget(_pRushTarget.AimTransform);
				}
			}
			_pShungokusatsuBullet.UpdateBulletData(_tSkill1_LinkSkill, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
			_pShungokusatsuBullet.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_pShungokusatsuBullet.BulletLevel = wsSkill.SkillLV;
			if ((bool)_pRushTarget.AimTransform)
			{
				_pShungokusatsuBullet.Active(_pRushTarget.AimPosition, _refEntity.ShootDirection, _refEntity.TargetMask);
			}
			else
			{
				Vector3 pPos = _refEntity.AimPosition + Vector3.right * _refEntity.direction;
				_pShungokusatsuBullet.Active(pPos, _refEntity.ShootDirection, _refEntity.TargetMask);
			}
		}
		_pRushTarget = null;
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
	}

	public void StageTeleportOutCharacterDepend()
	{
		_tfWind.gameObject.SetActive(false);
		_psWindBitL.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		_psWindBitR.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
	}

	public void StageTeleportInCharacterDepend()
	{
		_tfWind.gameObject.SetActive(true);
		_psWindBitL.Play(true);
		_psWindBitR.Play(true);
	}

	public void TeleportInCharacterDepend()
	{
	}

	public void TeleportInExtraEffect()
	{
	}

	private void UpdateSkill()
	{
		if (!_refEntity.IsLocalPlayer && !bInSkill)
		{
			bool flag = _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(LineLinkBullet.nFlagBuffLineLink);
			if (flag && !_bOldSkill0Flag)
			{
				DoSkill0(0);
				_bOldSkill0Flag = flag;
				return;
			}
			_bOldSkill0Flag = flag;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
			case OrangeCharacter.SubStatus.SKILL0_1:
			case OrangeCharacter.SubStatus.SKILL0_2:
				if (_refEntity.CurrentFrame > 1f)
				{
					SkillEndChnageToIdle();
				}
				else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.6f)
				{
					SkipSkill1Animation();
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				if (_refEntity.CurrentFrame > 1f)
				{
					bool flag2 = false;
					if (_refEntity.IsLocalPlayer)
					{
						flag2 = true;
					}
					else if (_bSyncRushSkillCompleted)
					{
						_bSyncRushSkillCompleted = false;
						flag2 = true;
					}
					if (flag2)
					{
						UpdateRushFxRotatation(_vSkillVelocity);
						CreateSkillBullet(_refEntity.PlayerSkills[1]);
						_vSkillVelocity = _vSkillVelocity.normalized * ((float)OrangeCharacter.DashSpeed * 4f);
						UpdateSkill1Direction(_vSkillVelocity.x);
						_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
					}
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
			{
				bool flag3 = false;
				if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
				{
					flag3 = true;
					Vector3 p_worldPos = _refEntity.AimPosition + new Vector3(1.5f, 0f, 0f) * _refEntity.direction;
					if (_refEntity.IsLocalPlayer && _tfHitTransform != null)
					{
						p_worldPos = _tfHitTransform.position;
					}
					else if (_pRushTarget != null)
					{
						p_worldPos = _pRushTarget.AimPosition;
					}
					_pShungokusatsuFx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_loveico_002", p_worldPos, Quaternion.identity, Array.Empty<object>());
				}
				else if (Vector2.Distance(_vSkillStartPosition, _refEntity.AimPosition) > _refEntity.PlayerSkills[1].BulletData.f_DISTANCE || _refEntity.PlayerSkills[1].LastUseTimer.GetMillisecond() > 350)
				{
					flag3 = true;
					_pRushTarget = null;
				}
				if (flag3)
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
					bInSkill = false;
					if (_pRushTarget != null)
					{
						CreateShungokusatsuBullet(_refEntity.PlayerSkills[1]);
					}
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				}
				break;
			}
			case OrangeCharacter.SubStatus.SKILL1_2:
				if (_refEntity.CurrentFrame > 1f)
				{
					SkillEndChnageToIdle();
				}
				else if (_refEntity.CurrentFrame > 0.45f && (CheckCancelAnimate(1) || _refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below))
				{
					SkipSkill1Animation();
				}
				break;
			}
		}
		else
		{
			RecoverRushCollideBullet();
		}
	}

	private void TurnToAimTarget()
	{
		Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.AimPosition);
		if (vector.HasValue)
		{
			int num = Math.Sign(vector.Value.x);
			if (_refEntity.direction != num && Mathf.Abs(vector.Value.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = vector.Value;
			}
		}
	}

	private void FindSkill0Target()
	{
		_pSkill0Target = null;
		if (_refEntity.PlayerAutoAimSystem.AutoAimTarget != null && Vector2.Distance(_refEntity.PlayerAutoAimSystem.AutoAimTarget.AimPosition, _refEntity.AimPosition) < _refEntity.PlayerSkills[0].BulletData.f_DISTANCE)
		{
			_pSkill0Target = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
		}
		if (_pSkill0Target == null)
		{
			_pSkill0Target = _pSkill0AimSystem.GetClosestTarget();
		}
	}

	private void TurnToShootDirection(Vector3 dir)
	{
		int num = Math.Sign(dir.x);
		if (_refEntity.direction != num && Mathf.Abs(dir.x) > 0.05f)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			_refEntity.ShootDirection = dir;
		}
	}

	private void UseSkill0(int skillId)
	{
		if (_refEntity.IsLocalPlayer)
		{
			FindSkill0Target();
			if (_pSkill0Target != null)
			{
				DoSkill0(skillId);
			}
		}
	}

	private void DoSkill0(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(1);
		if (_pSkill0Target != null)
		{
			Vector3 normalized = (_pSkill0Target.AimPosition - _refEntity.AimPosition).normalized;
			int num = Math.Sign(normalized.x);
			if (_refEntity.direction != num && Mathf.Abs(normalized.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = normalized;
			}
		}
		PlayVoiceSE("v_ic_skill03");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_loveico_000", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
		WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, _refEntity.AimTransform, _pSkill0Target, weaponStruct.SkillLV);
		_refEntity.CheckUsePassiveSkill(0, weaponStruct.weaponStatus, weaponStruct.ShootTransform[0]);
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
		}
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		SkipSkill0Animation();
	}

	private void SkipSkill0Animation()
	{
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_5 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_8)
		{
			SkillEndChnageToIdle(true);
		}
		else
		{
			SkillEndChnageToIdle();
		}
	}

	private void UseSkill1(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(2);
		_bSyncRushSkillCompleted = false;
		if (_refEntity.IsLocalPlayer)
		{
			if (_refEntity.UseAutoAim && _refEntity.PlayerAutoAimSystem.AutoAimTarget != null)
			{
				TurnToAimTarget();
				_pRushTarget = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
				_vSkillVelocity = (_pRushTarget.AimPosition - _refEntity.AimPosition).normalized * ((float)OrangeCharacter.DashSpeed * 4f);
				UpdateSkill1Direction(_vSkillVelocity.x);
			}
			else
			{
				_pRushTarget = null;
				_vSkillVelocity = _refEntity.ShootDirection.normalized * ((float)OrangeCharacter.DashSpeed * 4f);
				UpdateSkill1Direction(_vSkillVelocity.x);
			}
			if ((bool)_psRushFx)
			{
				UpdateRushFxRotatation(_vSkillVelocity);
				_psRushFx.Play(true);
			}
		}
		PlayVoiceSE("v_ic_skill02");
		PlaySkillSE("ic_allfor01");
		_vSkillStartPosition = _refEntity.AimPosition;
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_loveico_001", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
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

	private void UpdateRushFxRotatation(Vector2 dir)
	{
		if ((bool)_psRushFx)
		{
			Vector3 to = ((_refEntity._characterDirection == CharacterDirection.LEFT) ? Vector3.left : Vector3.right);
			float num = Vector3.Angle(dir, to);
			if (dir.y > 0f)
			{
				num = 0f - num;
			}
			_psRushFx.transform.localEulerAngles = new Vector3(num, _psRushFx.transform.localEulerAngles.y, _psRushFx.transform.localEulerAngles.z);
		}
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		RecoverRushCollideBullet();
		if (!_pShungokusatsuBullet.bIsEnd)
		{
			_pShungokusatsuBullet.BackToPool();
			if ((bool)_pShungokusatsuFx)
			{
				_pShungokusatsuFx.BackToPool();
				_pShungokusatsuFx = null;
			}
		}
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		_refEntity.SetSpeed(0, 0);
		if ((bool)_psRushFx)
		{
			_psRushFx.transform.localEulerAngles = Vector3.zero;
			_psRushFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		SkillEndChnageToIdle();
	}

	private void Skill1MoveHitCB(object obj)
	{
		if (!_refEntity.IsLocalPlayer && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			return;
		}
		if (_refEntity.UsingVehicle)
		{
			RecoverRushCollideBullet();
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

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 0:
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_5 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_8))
			{
				if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
				{
					return true;
				}
			}
			else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
			break;
		case 1:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void SkillEndChnageToIdle(bool isCrouch = false)
	{
		_refEntity.SkillEnd = true;
		_refEntity.Dashing = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.GravityMultiplier = new VInt(1f);
		_refEntity.Animator._animator.speed = 1f;
		bInSkill = false;
		_pSkill0Target = null;
		_pRushTarget = null;
		_tfHitTransform = null;
		RecoverRushCollideBullet();
		if (_refEntity.IsLocalPlayer && _refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(-1);
		}
		ToggleWeapon(0);
		if (isCrouch)
		{
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
		}
		else if (_refEntity.Controller.Collisions.below)
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		}
		else
		{
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
		}
	}

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -3:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			break;
		}
	}

	protected void RecoverRushCollideBullet()
	{
		if ((bool)_pRushCollideBullet)
		{
			_pRushCollideBullet.BackToPool();
			_pRushCollideBullet.HitCallback = null;
			_pRushCollideBullet = null;
		}
	}
}
