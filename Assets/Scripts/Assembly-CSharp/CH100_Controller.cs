using System;
using UnityEngine;

public class CH100_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected bool bInSkillFx;

	protected SKILL_TABLE _tSkill1_LinkSkill;

	protected Vector3 _vSkillStartPosition;

	protected Vector2 _vSkillVelocity;

	protected OrangeTimer _otSkill1Timer;

	protected CH100_ShungokusatsuBullet _pShungokusatsuBullet;

	protected CH100_ShungokusatsuHitFx _pShungokusatsuFx;

	protected Transform _tfShungokusatsuTarget;

	protected FxBase _fxSkill1Shinzin;

	private ParticleSystem m_fxuse_skill;

	protected bool _bSyncSkillTargetCompleted;

	protected IAimTarget _pSyncTarget;

	protected Vector3 _vSyncDirection;

	private OrangeConsoleCharacter _refPlayer;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[5] { "ch100_skill_01_stand", "ch100_skill_02_step1_start", "ch100_skill_02_step1_loop", "ch100_skill_02_step2_loop", "ch100_skill_02_step2_end" };
	}

	public override void Start()
	{
		base.Start();
		InitLinkSkill();
		InitPet();
		InitExtraMeshData();
	}

	private void InitExtraMeshData()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[5];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill0FxPoint", true);
		_refEntity.ExtraTransforms[3] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill1Point", true);
		_refEntity.ExtraTransforms[4] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill1Point02", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_skill", true);
		if (transform != null)
		{
			m_fxuse_skill = transform.GetComponent<ParticleSystem>();
		}
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "p_shungokusatsu_000", true);
		_pShungokusatsuBullet = transform2.GetComponent<CH100_ShungokusatsuBullet>();
		_otSkill1Timer = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_sekia_000", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_shungokusatsu_000", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_shungokusatsu_001", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_shungokusatsu_003", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_shungokusatsu_004", 3);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_shungokusatsu_000", 5);
	}

	public override void ExtraVariableInit()
	{
		if (m_fxuse_skill != null && m_fxuse_skill.isPlaying)
		{
			m_fxuse_skill.Stop();
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

	private void InitPet()
	{
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
	}

	public override void SyncSkillDirection(Vector3 dir, IAimTarget target)
	{
		_bSyncSkillTargetCompleted = true;
		_vSyncDirection = dir;
		_pSyncTarget = target;
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
			PlayVoiceSE("v_go_skill01");
			PlaySkillSE("go_spining");
			UseSkill0(id);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			PlayVoiceSE("v_go_skill02");
			PlaySkillSE("go_shungokusatsu01");
			UseSkill1(id);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if ((mainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.CurSubStatus != OrangeCharacter.SubStatus.SKILL0) && m_fxuse_skill != null && m_fxuse_skill.isPlaying)
		{
			m_fxuse_skill.Stop();
		}
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
			{
				Vector3 p_worldPos = _refEntity.ModelTransform.position + new Vector3(1.09f, 0f, 0f) * _refEntity.direction;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxdemo_gouki_000B", p_worldPos, (_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
				ToggleWeapon(-2);
				break;
			}
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				ToggleWeapon(-3);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.Animator._animator.speed = 2f;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.Animator._animator.speed = 2f;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				if (m_fxuse_skill != null && !m_fxuse_skill.isPlaying)
				{
					m_fxuse_skill.Play();
				}
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				_refEntity.SetSpeed((int)_vSkillVelocity.x, (int)_vSkillVelocity.y);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetSpeed(0, 0);
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
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
			case OrangeCharacter.SubStatus.SKILL1:
				if (_refEntity.IsLocalPlayer)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
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
			{
				Vector3 shootPosition = _refEntity.ModelTransform.position + Vector3.right * _refEntity.direction;
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, shootPosition, wsSkill.SkillLV);
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			}
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.AimTransform, wsSkill.SkillLV, _vSkillVelocity, true, null, Skill1MoveHitCB);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			}
		}
	}

	public void CreateShungokusatsuBullet(WeaponStruct wsSkill)
	{
		if ((bool)_pShungokusatsuBullet)
		{
			_pShungokusatsuBullet.SetHitTarget(_tfShungokusatsuTarget);
			_pShungokusatsuBullet.UpdateBulletData(_tSkill1_LinkSkill, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
			_pShungokusatsuBullet.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			_pShungokusatsuBullet.BulletLevel = wsSkill.SkillLV;
			if ((bool)_tfShungokusatsuTarget)
			{
				_pShungokusatsuBullet.Active(_tfShungokusatsuTarget.transform.position, _refEntity.ShootDirection, _refEntity.TargetMask);
			}
			else
			{
				Vector3 pPos = _refEntity.AimPosition + Vector3.right * _refEntity.direction;
				_pShungokusatsuBullet.Active(pPos, _refEntity.ShootDirection, _refEntity.TargetMask);
			}
		}
		_tfShungokusatsuTarget = null;
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 49) > 4u)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	public void TeleportInExtraEffect()
	{
		Vector3 p_worldPos = _refEntity.ModelTransform.position + new Vector3(1.09f, 0f, 0f) * _refEntity.direction;
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), p_worldPos, (_refEntity.direction == 1) ? OrangeBattleUtility.QuaternionNormal : OrangeBattleUtility.QuaternionReverse, Array.Empty<object>());
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxdemo_gouki_000";
	}

	private void UpdateSkill()
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkillFx && _refEntity.CurrentFrame > 0.08f)
			{
				bInSkillFx = false;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_sekia_000", _refEntity.ExtraTransforms[2], Quaternion.identity, Array.Empty<object>());
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.55f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.7f)
			{
				SkipSkill0Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (!(_refEntity.CurrentFrame > 1f))
			{
				break;
			}
			if (_refEntity.IsLocalPlayer)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			else if (_bSyncSkillTargetCompleted)
			{
				if (_pSyncTarget != null)
				{
					_refEntity.PlayerAutoAimSystem.AutoAimTarget = _pSyncTarget;
					TurnToAimTarget();
					_vSkillVelocity = (_pSyncTarget.AimPosition - _refEntity.AimPosition).normalized * ((float)OrangeCharacter.DashSpeed * 4f);
					UpdateSkill1Direction(_vSkillVelocity.x);
				}
				else
				{
					_vSkillVelocity = _vSyncDirection.normalized * ((float)OrangeCharacter.DashSpeed * 4f);
					UpdateSkill1Direction(_vSkillVelocity.x);
				}
				_bSyncSkillTargetCompleted = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
			{
				_otSkill1Timer.TimerStart();
				_refEntity.BulletCollider.BackToPool();
				_refEntity.BulletCollider.HitCallback = null;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				Vector3 p_worldPos = _refEntity.AimPosition + new Vector3(1.5f, 0f, 0f) * _refEntity.direction;
				_pShungokusatsuFx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<CH100_ShungokusatsuHitFx>("fxhit_shungokusatsu_000", p_worldPos, Quaternion.identity, Array.Empty<object>());
				bool visible = IsHitPlayer(-1);
				_pShungokusatsuFx.ActivePlayBlackBG(visible);
				if (m_fxuse_skill != null && m_fxuse_skill.isPlaying)
				{
					m_fxuse_skill.Stop();
				}
			}
			else if (Vector2.Distance(_vSkillStartPosition, _refEntity.AimPosition) > _refEntity.PlayerSkills[1].BulletData.f_DISTANCE || _refEntity.PlayerSkills[1].LastUseTimer.GetMillisecond() > 350)
			{
				_refEntity.SetSpeed(0, 0);
				_refEntity.BulletCollider.BackToPool();
				SkipSkill1Animation();
				if (m_fxuse_skill != null && m_fxuse_skill.isPlaying)
				{
					m_fxuse_skill.Stop();
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (bInSkill)
			{
				bInSkill = false;
				CreateShungokusatsuBullet(_refEntity.PlayerSkills[1]);
			}
			else if (_refEntity.AnimateID != (HumanBase.AnimateId)68u && _otSkill1Timer.GetMillisecond() > 100)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
			else if (_otSkill1Timer.GetMillisecond() > 800)
			{
				PlaySkillSE("go_shungokusatsu03");
				_fxSkill1Shinzin = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_shungokusatsu_003", _refEntity.ExtraTransforms[4], Quaternion.identity, Array.Empty<object>());
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_shungokusatsu_004", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (_otSkill1Timer.GetMillisecond() > 1800)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
			}
			else if (CheckCancelAnimate(1) && _otSkill1Timer.GetMillisecond() > 1400)
			{
				if ((bool)_fxSkill1Shinzin)
				{
					_fxSkill1Shinzin.StopEmittingBackToPool(0f);
				}
				SkipSkill1Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_4:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.5f)
			{
				SkipSkill1Animation();
			}
			break;
		}
	}

	private bool IsHitPlayer(int buffId)
	{
		foreach (PerBuff listBuff in _refEntity.selfBuffManager.listBuffs)
		{
			if (listBuff.nBuffID == buffId && !string.IsNullOrEmpty(listBuff.sPlayerID))
			{
				return true;
			}
		}
		return false;
	}

	private void TurnToAimTarget()
	{
		Vector3? vector = _refEntity.CalibrateAimDirection(_refEntity.AimPosition);
		if (vector.HasValue)
		{
			int num = Math.Sign(vector.Value.x);
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(vector.Value.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = vector.Value;
			}
		}
	}

	private void UseSkill0(int skillId)
	{
		bInSkill = true;
		bInSkillFx = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		TurnToAimTarget();
		ToggleWeapon(1);
		if (_refEntity.Controller.Collisions.below)
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
		SkillEndChnageToIdle();
	}

	private void UseSkill1(int skillId)
	{
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		_refEntity.IgnoreGravity = true;
		ToggleWeapon(2);
		if (_refEntity.IsLocalPlayer)
		{
			if (_refEntity.UseAutoAim)
			{
				TurnToAimTarget();
				IAimTarget autoAimTarget = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
				if (autoAimTarget != null)
				{
					_vSkillVelocity = (autoAimTarget.AimPosition - _refEntity.AimPosition).normalized * ((float)OrangeCharacter.DashSpeed * 4f);
					UpdateSkill1Direction(_vSkillVelocity.x);
				}
				else
				{
					_vSkillVelocity = new Vector2((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 4f), 0f);
				}
			}
			else
			{
				_vSkillVelocity = _refEntity.ShootDirection * ((float)OrangeCharacter.DashSpeed * 4f);
				UpdateSkill1Direction(_vSkillVelocity.x);
			}
		}
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_shungokusatsu_000", _refEntity.AimPosition, Quaternion.identity, Array.Empty<object>());
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_shungokusatsu_001", _refEntity.AimPosition, Quaternion.identity, Array.Empty<object>());
		_vSkillStartPosition = _refEntity.AimPosition;
		_refEntity.PlayerSkills[1].LastUseTimer.TimerStart();
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

	private void CancelSkill1()
	{
		if (!_refEntity.BulletCollider.bIsEnd)
		{
			_refEntity.BulletCollider.BackToPool();
			_refEntity.BulletCollider.HitCallback = null;
		}
		if (!_pShungokusatsuBullet.bIsEnd)
		{
			_pShungokusatsuBullet.BackToPool();
			if ((bool)_pShungokusatsuFx)
			{
				_pShungokusatsuFx.BackToPool();
				_pShungokusatsuFx = null;
			}
		}
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		SkillEndChnageToIdle();
	}

	private void Skill1MoveHitCB(object obj)
	{
		if ((!_refEntity.IsLocalPlayer && !_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1)) || _refEntity.UsingVehicle)
		{
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
		_otSkill1Timer.TimerStart();
		_tfShungokusatsuTarget = transform;
		_refEntity.BulletCollider.BackToPool();
		_refEntity.BulletCollider.HitCallback = null;
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
		if (_refEntity.IsLocalPlayer)
		{
			if ((bool)orangeCharacter)
			{
				_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0, false, orangeCharacter.sPlayerID);
			}
			else
			{
				_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0);
			}
		}
		Vector3 p_worldPos = _refEntity.AimPosition + new Vector3(1.5f, 0f, 0f) * _refEntity.direction;
		PlaySkillSE("go_shungokusatsu02");
		_pShungokusatsuFx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<CH100_ShungokusatsuHitFx>("fxhit_shungokusatsu_000", p_worldPos, Quaternion.identity, Array.Empty<object>());
		if (_refEntity.IsLocalPlayer || (bool)orangeCharacter)
		{
			_pShungokusatsuFx.ActivePlayBlackBG(true);
		}
		else
		{
			_pShungokusatsuFx.ActivePlayBlackBG(false);
		}
	}

	private bool CheckCancelAnimate(int skilliD)
	{
		switch (skilliD)
		{
		case 0:
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
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
		bInSkillFx = false;
		_tfShungokusatsuTarget = null;
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
}
