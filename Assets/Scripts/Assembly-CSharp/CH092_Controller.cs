using System;
using UnityEngine;

public class CH092_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected bool bPlaySkillFx;

	protected bool bPlaySkillFx2;

	protected SkinnedMeshRenderer _tfWeaponMesh;

	protected SkinnedMeshRenderer _tfRopeMesh;

	protected SKILL_TABLE _tSkill0_LinkSkill;

	protected SKILL_TABLE _tSkill1_LinkSkill;

	protected Vector3 _vSkillStartPosition;

	protected Vector2 _vSkillVelocity;

	protected IAimTarget _pSkillTarget;

	private ParticleSystem m_fxuse_skill;

	protected int _nSkill0_GuardCount;

	protected OrangeTimer _otSkill0Timer;

	[SerializeField]
	protected Vector3 _vSkill0FxPosition;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch092_skill_01_start", "ch092_skill_01_end", "ch092_skill_02_step1_loop", "ch092_skill_02_step1_end", "ch092_skill_02_step2_start", "ch092_skill_02_step2_end" };
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
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "SaberMesh_m", true);
		_tfWeaponMesh = transform.GetComponent<SkinnedMeshRenderer>();
		_tfWeaponMesh.enabled = true;
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "RopeMesh_e", true);
		_tfRopeMesh = transform2.GetComponent<SkinnedMeshRenderer>();
		_tfRopeMesh.enabled = false;
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_skill", true);
		if (transform3 != null)
		{
			m_fxuse_skill = transform3.GetComponent<ParticleSystem>();
		}
		_refEntity.CDSkill(1);
		_otSkill0Timer = OrangeTimerManager.GetTimer();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_soaringkick_000", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_soaringkick_001", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_soaringkick_002", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_spiritslash_000", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_spiritslash_000", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_spiritslash_001", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_spiritslash_002", 5);
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
		if (_tSkill0_LinkSkill == null && _refEntity.PlayerSkills[0].BulletData.n_LINK_SKILL != 0)
		{
			WeaponStruct weaponStruct = _refEntity.PlayerSkills[0];
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct.BulletData.n_LINK_SKILL, out _tSkill0_LinkSkill))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref _tSkill0_LinkSkill);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/bullet/" + _tSkill0_LinkSkill.s_MODEL, _tSkill0_LinkSkill.s_MODEL, delegate(GameObject obj)
				{
					BulletBase component = obj.GetComponent<BulletBase>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BulletBase>(UnityEngine.Object.Instantiate(component), _tSkill0_LinkSkill.s_MODEL, 5);
				});
			}
		}
		if (_tSkill1_LinkSkill == null && _refEntity.PlayerSkills[1].BulletData.n_LINK_SKILL != 0)
		{
			WeaponStruct weaponStruct2 = _refEntity.PlayerSkills[1];
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(weaponStruct2.BulletData.n_LINK_SKILL, out _tSkill1_LinkSkill))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref _tSkill1_LinkSkill);
				GameObject obj2 = new GameObject();
				CollideBullet go = obj2.AddComponent<CollideBullet>();
				obj2.name = _tSkill1_LinkSkill.s_MODEL;
				obj2.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
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
		_refEntity.GuardCalculateEvt = GuardCalculate;
		_refEntity.GuardHurtEvt = GuardHurt;
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
			UpdateSkill();
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				UseSkill0(id);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				UseSkill1(id);
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
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
				_refEntity.Animator._animator.speed = 4f;
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
				if (m_fxuse_skill != null && !m_fxuse_skill.isPlaying)
				{
					m_fxuse_skill.Play();
				}
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.Animator._animator.speed = 2f;
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.Animator._animator.speed = 2f;
				_refEntity.IgnoreGravity = false;
				_refEntity.SetSpeed(0, OrangeCharacter.JumpSpeed);
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				PlaySkillSE("mh1_hisyo02");
				_refEntity.Animator._animator.speed = 1f;
				_refEntity.GravityMultiplier = new VInt(4f);
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
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
			case OrangeCharacter.SubStatus.SKILL0_3:
				SkillEndChnageToIdle();
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_tfRopeMesh.enabled = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
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
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.BulletCollider.UpdateBulletData(wsSkill.FastBulletDatas[0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
				_refEntity.BulletCollider.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = wsSkill.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.BulletCollider.HitCallback = null;
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.BulletCollider.UpdateBulletData(wsSkill.FastBulletDatas[0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++, _refEntity.direction);
				_refEntity.BulletCollider.SetBulletAtk(wsSkill.weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.BulletLevel = wsSkill.SkillLV;
				_refEntity.BulletCollider.Active(_refEntity.TargetMask);
				_refEntity.BulletCollider.HitCallback = Skill1FlyKickHitCB;
				_refEntity.CheckUsePassiveSkill(1, wsSkill.weaponStatus, _refEntity.ExtraTransforms[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.PushBulletDetail(_tSkill1_LinkSkill, wsSkill.weaponStatus, _refEntity.ModelTransform, wsSkill.SkillLV);
				break;
			}
		}
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		if ((uint)(curSubStatus - 19) > 3u && (uint)(curSubStatus - 49) > 3u)
		{
			_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
		}
	}

	public void StageTeleportOutCharacterDepend()
	{
		_tfRopeMesh.enabled = false;
	}

	public override bool GuardCalculate(HurtPassParam tHurtPassParam)
	{
		if ((int)_refEntity.Hp <= 0)
		{
			return false;
		}
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_1 || _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2))
		{
			return true;
		}
		return false;
	}

	public void GuardHurt(HurtPassParam tHurtPassParam)
	{
		if (_refEntity.IsLocalPlayer && _refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_1 && _nSkill0_GuardCount == 0)
		{
			_nSkill0_GuardCount++;
			_refEntity.PushBulletDetail(_tSkill0_LinkSkill, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.AimTransform, _refEntity.PlayerSkills[0].SkillLV);
		}
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
			if (bPlaySkillFx && _refEntity.CurrentFrame > 0.28f)
			{
				bPlaySkillFx = false;
				Vector3 p_worldPos2 = _refEntity.ModelTransform.position + new Vector3(0f, 0.67f, 0f);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_spiritslash_000", p_worldPos2, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			if (bPlaySkillFx2 && _refEntity.CurrentFrame > 0.5f)
			{
				bPlaySkillFx2 = false;
				Vector3 p_worldPos3 = _refEntity.ModelTransform.position + new Vector3(0f, 0.67f, 0f);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_spiritslash_001", p_worldPos3, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			else if (_refEntity.CurrentFrame > 0.55f)
			{
				_vSkillVelocity = Vector3.right * OrangeCharacter.DashSpeed * 4f * _refEntity.direction;
				_vSkillStartPosition = _refEntity.AimPosition;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_spiritslash_002", _refEntity.ModelTransform, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				_refEntity.Animator._animator.speed = 1f;
				_refEntity.SetSpeed((int)_vSkillVelocity.x, (int)_vSkillVelocity.y);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (Vector2.Distance(_vSkillStartPosition, _refEntity.AimPosition) > _refEntity.PlayerSkills[0].BulletData.f_DISTANCE || _refEntity.PlayerSkills[0].LastUseTimer.GetMillisecond() > 500)
			{
				_otSkill0Timer.TimerStart();
				_refEntity.BulletCollider.BackToPool();
				_refEntity.SetSpeed(OrangeCharacter.WalkSpeed * _refEntity.direction, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (CheckCancelAnimate(0))
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.IsShoot = 0;
				SkipSkill0Animation();
			}
			else if (_otSkill0Timer.GetMillisecond() < 500)
			{
				int x = Mathf.RoundToInt(OrangeCharacter.WalkSpeed - OrangeCharacter.WalkSpeed / 500 * _otSkill0Timer.GetMillisecond()) * _refEntity.direction;
				_refEntity.SetSpeed(x, 0);
			}
			else
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.SetSpeed(0, 0);
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.IsShoot = 0;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (CheckCancelAnimate(0))
			{
				SkipSkill0Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (Vector2.Distance(_vSkillStartPosition, _refEntity.AimPosition) > _refEntity.PlayerSkills[1].BulletData.f_DISTANCE || _refEntity.PlayerSkills[1].LastUseTimer.GetMillisecond() > 500)
			{
				_tfRopeMesh.enabled = false;
				if (!_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetSpeed(_refEntity.direction * OrangeCharacter.WalkSpeed, 0);
				}
				else
				{
					_refEntity.SetSpeed(0, 0);
				}
				_refEntity.BulletCollider.BackToPool();
				SkipSkill1Animation();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (_refEntity.CurrentFrame > 1f)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
			}
			else if (_refEntity.CurrentFrame > 0.3f)
			{
				if ((float)_refEntity.Velocity.y > (float)OrangeCharacter.JumpSpeed * 0.2f)
				{
					_refEntity.Animator._animator.speed = 0.8f;
					_refEntity.SetSpeed(0, (int)((float)OrangeCharacter.JumpSpeed * 0.2f));
				}
				else if ((float)_refEntity.Velocity.y < 0f)
				{
					_refEntity.SetSpeed(0, 0);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.2f)
			{
				bInSkill = false;
				Vector3 p_worldPos = _refEntity.ModelTransform.position + new Vector3(_refEntity.direction, -1f, 0f);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_soaringkick_000", p_worldPos, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				p_worldPos = _refEntity.ModelTransform.position + new Vector3(_refEntity.direction, 0f, 0f);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_soaringkick_001", p_worldPos, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				CreateSkillBullet(_refEntity.PlayerSkills[1]);
			}
			else if (CheckCancelAnimate(1) && _refEntity.CurrentFrame > 0.3f)
			{
				SkipSkill1Animation();
			}
			break;
		}
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
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		_refEntity.IgnoreGravity = true;
		ToggleWeapon(1);
		_nSkill0_GuardCount = 0;
		bPlaySkillFx = true;
		bPlaySkillFx2 = true;
		PlayVoiceSE("v_mh1_skill01");
		PlaySkillSE("mh1_iai01");
		_refEntity.PlayerStopDashing();
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
	}

	private void CancelSkill0()
	{
		_refEntity.SkillEnd = true;
		if (!_refEntity.BulletCollider.bIsEnd)
		{
			_refEntity.BulletCollider.BackToPool();
		}
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
		ToggleWeapon(2);
		TurnToAimTarget();
		_refEntity.IgnoreGravity = true;
		PlayVoiceSE("v_mh1_skill02");
		_pSkillTarget = _refEntity.PlayerAutoAimSystem.AutoAimTarget;
		if (_pSkillTarget != null)
		{
			_vSkillVelocity = (_pSkillTarget.AimPosition - _refEntity.AimPosition).normalized * ((float)OrangeCharacter.DashSpeed * 4f);
			int num = Math.Sign(_vSkillVelocity.x);
			if (num != _refEntity.direction && num != 0)
			{
				_refEntity.direction = Math.Sign(_vSkillVelocity.x);
			}
		}
		else
		{
			_vSkillVelocity = new Vector2((int)_refEntity._characterDirection * (int)((float)OrangeCharacter.DashSpeed * 4f), 0f);
		}
		_vSkillStartPosition = _refEntity.AimPosition;
		_refEntity.SetSpeed((int)_vSkillVelocity.x, (int)_vSkillVelocity.y);
		_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
	}

	private void CancelSkill1()
	{
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
		SkillEndChnageToIdle();
	}

	private void Skill1FlyKickHitCB(object obj)
	{
		if (_refEntity.BulletCollider.HitTarget == null || _refEntity.UsingVehicle)
		{
			return;
		}
		StageObjParam component = _refEntity.BulletCollider.HitTarget.transform.GetComponent<StageObjParam>();
		if (!(component == null) && !(component.tLinkSOB == null))
		{
			OrangeCharacter obj2 = component.tLinkSOB as OrangeCharacter;
			EnemyControllerBase enemyControllerBase = component.tLinkSOB as EnemyControllerBase;
			if ((bool)obj2 || (bool)enemyControllerBase)
			{
				_refEntity.BulletCollider.BackToPool();
				_refEntity.BulletCollider.HitCallback = null;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				Vector3 p_worldPos = _refEntity.ModelTransform.position + new Vector3((float)_refEntity.direction * 1.5f, 1f, 0f);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_soaringkick_002", p_worldPos, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
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

	private void DebutOrClearStageToggleWeapon(bool bDebut)
	{
		ToggleWeapon(-1);
	}

	private void ToggleWeapon(int style)
	{
		switch (style)
		{
		case -3:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				_refEntity.EnableHandMesh(true);
			}
			_tfWeaponMesh.enabled = false;
			_tfRopeMesh.enabled = true;
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				_refEntity.EnableHandMesh(true);
			}
			_tfWeaponMesh.enabled = false;
			_tfRopeMesh.enabled = false;
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
				_refEntity.EnableHandMesh(true);
			}
			_tfWeaponMesh.enabled = true;
			_tfRopeMesh.enabled = false;
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			_tfRopeMesh.enabled = false;
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_tfWeaponMesh.enabled = true;
			_tfRopeMesh.enabled = false;
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_tfWeaponMesh.enabled = false;
			_tfRopeMesh.enabled = false;
			break;
		}
	}
}
