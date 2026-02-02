using System;
using Better;
using UnityEngine;

public class CH117_Controller : CharacterControlBase
{
	protected bool bInSkill;

	protected CharacterMaterial _cmWeapon;

	protected ParticleSystem _fxWeapon;

	protected SKILL_TABLE _tS1LS_TriSlash;

	protected SKILL_TABLE _tS1LS_SlashWaveA;

	protected SKILL_TABLE _tS1LS_SlashWaveB;

	protected IAimTarget _pRushTarget;

	protected Transform _tfHitTransform;

	protected Vector3 _vSkillStartPosition;

	protected Vector2 _vSkillVelocity;

	protected RushCollideBullet _pRushCollideBullet;

	protected bool _bSyncRushSkillCompleted;

	protected Vector3 _vSyncDirection;

	protected ParticleSystem _psRushFx;

	protected bool bFxFlag;

	private ParticleSystem m_fxuse_skill;

	private OrangeConsoleCharacter _refPlayer;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[7] { "ch117_skill_02_start", "ch117_skill_02_jump", "ch117_skill_02_crouch", "ch117_skill_01_start", "ch117_skill_01_loop", "ch117_skill_01_stand_end", "ch117_skill_01_jump_end" };
	}

	public override void Start()
	{
		base.Start();
		InitLinkSkill();
		InitExtraMeshData();
		_refEntity.AnimatorModelShiftYOverride = new Dictionary<OrangeCharacter.MainStatus, float>
		{
			{
				OrangeCharacter.MainStatus.TELEPORT_IN,
				0.1f
			},
			{
				OrangeCharacter.MainStatus.TELEPORT_OUT,
				0.1f
			},
			{
				OrangeCharacter.MainStatus.SKILL,
				0.1f
			},
			{
				OrangeCharacter.MainStatus.GIGA_ATTACK,
				0.1f
			}
		};
	}

	private void InitExtraMeshData()
	{
		CharacterMaterial[] componentsInChildren = _refEntity.GetComponentsInChildren<CharacterMaterial>();
		if (componentsInChildren != null && componentsInChildren.Length > 1)
		{
			componentsInChildren[0].SetSubCharacterMaterial(componentsInChildren[1]);
		}
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms = new Transform[2];
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "Saber_117", true);
		_cmWeapon = transform.GetComponent<CharacterMaterial>();
		_cmWeapon.Disappear();
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "FX", true);
		_fxWeapon = transform2.GetComponent<ParticleSystem>();
		_fxWeapon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_skill", true);
		if (transform3 != null)
		{
			m_fxuse_skill = transform3.GetComponent<ParticleSystem>();
		}
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_DSlash_003", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_DSlash_000", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_DSlash_001", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_DSlash_002", 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_ch117_skill1_000", 2);
	}

	private void InitLinkSkill()
	{
		_refPlayer = _refEntity as OrangeConsoleCharacter;
		if (_tS1LS_TriSlash == null && _refEntity.PlayerSkills[1].BulletData.n_LINK_SKILL != 0)
		{
			SetupLinkSkill(_refEntity.PlayerSkills[1].BulletData.n_LINK_SKILL, ref _tS1LS_TriSlash);
		}
		if (_tS1LS_SlashWaveA == null && _tS1LS_TriSlash.n_LINK_SKILL != 0)
		{
			SetupLinkSkill(_tS1LS_TriSlash.n_LINK_SKILL, ref _tS1LS_SlashWaveA);
		}
		if (_tS1LS_SlashWaveB == null && _tS1LS_SlashWaveA.n_LINK_SKILL != 0)
		{
			SetupLinkSkill(_tS1LS_SlashWaveA.n_LINK_SKILL, ref _tS1LS_SlashWaveB);
		}
	}

	public override void ExtraVariableInit()
	{
		if (m_fxuse_skill != null && m_fxuse_skill.isPlaying)
		{
			m_fxuse_skill.Stop();
		}
	}

	protected void SetupLinkSkill(int linkSkillId, ref SKILL_TABLE skillTable)
	{
		if (linkSkillId != 0 && ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(linkSkillId, out skillTable))
		{
			_refEntity.tRefPassiveskill.ReCalcuSkill(ref skillTable);
			if (skillTable.s_MODEL == "p_DUMMY")
			{
				GameObject obj = new GameObject();
				CollideBullet go = obj.AddComponent<CollideBullet>();
				obj.name = skillTable.s_MODEL;
				obj.layer = ManagedSingleton<OrangeLayerManager>.Instance.BulletLayer;
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(go, skillTable.s_MODEL, 5);
			}
			else if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(skillTable.s_MODEL) && !MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(skillTable.s_MODEL))
			{
				BulletBase.PreloadBullet<BasicBullet>(skillTable);
			}
		}
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.CheckSkillLockDirectionEvt = CheckSkillLockDirection;
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
		}
	}

	private void UpdateRushFxRotatation(Vector2 dir)
	{
		float num = Vector3.Angle(dir, Vector3.right);
		if (dir.y < 0f)
		{
			num = 0f - num;
		}
		Quaternion p_quaternion = Quaternion.Euler(0f, 0f, num);
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_DSlash_000", _refEntity.AimPosition + Vector3.up * 0.4f, p_quaternion, Array.Empty<object>());
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
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				if (m_fxuse_skill != null && !m_fxuse_skill.isPlaying)
				{
					m_fxuse_skill.Play();
				}
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				_refEntity.SetSpeed((int)_vSkillVelocity.x, (int)_vSkillVelocity.y);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
			{
				bFxFlag = true;
				_refEntity.SetSpeed(0, 0);
				bool flag = false;
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					flag = true;
				}
				else if (_vSkillVelocity.y <= 0f && (bool)OrangeBattleUtility.RaycastIgnoreSelf(_refEntity.ModelTransform.position, Vector2.down, 0.25f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer, _refEntity.ModelTransform))
				{
					flag = true;
				}
				if (flag)
				{
					_refEntity.IgnoreGravity = false;
					_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				}
				break;
			}
			case OrangeCharacter.SubStatus.SKILL1_3:
				bFxFlag = true;
				break;
			case OrangeCharacter.SubStatus.SKILL1_4:
				bFxFlag = true;
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
			case OrangeCharacter.SubStatus.SKILL0_2:
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_ch117_skill1_000", OrangeBattleUtility.FindChildRecursive(_refEntity._transform, "HandMesh_R_c"), (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				Vector3 position = _refEntity.AimTransform.position;
				position.x += (float)_refEntity.direction * -0.25f;
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, position, wsSkill.SkillLV, Vector3.up);
				_refEntity.CheckUsePassiveSkill(0, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			}
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.PushBulletDetail(wsSkill.BulletData, wsSkill.weaponStatus, _refEntity.AimTransform, wsSkill.SkillLV, _vSkillVelocity, true, null, Skill1MoveHitCB);
				_refEntity.CheckUsePassiveSkill(1, wsSkill.BulletData, wsSkill.weaponStatus, wsSkill.ShootTransform[0]);
				OrangeBattleUtility.UpdateSkillCD(wsSkill);
				break;
			}
		}
	}

	protected void CreateTriSlashBullet(WeaponStruct wsSkill, SKILL_TABLE linkSkillTable)
	{
		_refEntity.PushBulletDetail(linkSkillTable, wsSkill.weaponStatus, _refEntity.AimTransform.position, wsSkill.SkillLV);
	}

	public override void CallPet(int petID, bool isHurt, int nSetNumID = -1, Vector3? vSetPos = null)
	{
	}

	public void CheckSkillLockDirection()
	{
		OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
		_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
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
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (_refEntity.CurrentFrame > 1f)
			{
				SkillEndChnageToIdle();
			}
			else if (bInSkill && _refEntity.CurrentFrame > 0.38f)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.PlayerSkills[0]);
			}
			else if (CheckCancelAnimate(0) && _refEntity.CurrentFrame > 0.65f)
			{
				SkipSkill0Animation();
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
			bool flag = false;
			bool flag2 = false;
			if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
			{
				flag = true;
				flag2 = true;
				Vector3 p_worldPos = _refEntity.AimPosition + new Vector3(1.5f, 0f, 0f) * _refEntity.direction;
				if (_refEntity.IsLocalPlayer && _tfHitTransform != null)
				{
					p_worldPos = _tfHitTransform.position;
				}
				else if (_pRushTarget != null)
				{
					p_worldPos = _pRushTarget.AimPosition;
				}
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_DSlash_003", p_worldPos, Quaternion.identity, Array.Empty<object>());
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
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
				}
				else
				{
					CancelSkill1();
				}
			}
			break;
		}
		case OrangeCharacter.SubStatus.SKILL1_2:
			if (bFxFlag && _refEntity.CurrentFrame > 0.18f)
			{
				bFxFlag = false;
				CreateTriSlashBullet(_refEntity.PlayerSkills[1], _tS1LS_TriSlash);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_DSlash_001", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			if (_refEntity.CurrentFrame > 0.22f)
			{
				CreateTriSlashBullet(_refEntity.PlayerSkills[1], _tS1LS_SlashWaveA);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (bFxFlag && _refEntity.CurrentFrame > 0.37f)
			{
				bFxFlag = false;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_DSlash_001", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			else if (_refEntity.CurrentFrame > 0.4f)
			{
				CreateTriSlashBullet(_refEntity.PlayerSkills[1], _tS1LS_SlashWaveA);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_4);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_4:
			if (bFxFlag && _refEntity.CurrentFrame > 0.55f)
			{
				bFxFlag = false;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_DSlash_002", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			if (bInSkill && _refEntity.CurrentFrame > 0.6f)
			{
				bInSkill = false;
				CreateTriSlashBullet(_refEntity.PlayerSkills[1], _tS1LS_SlashWaveB);
			}
			else if (_refEntity.CurrentFrame > 0.75f && CheckCancelAnimate(1))
			{
				CancelSkill1();
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
			if (_refEntity.direction != num && Mathf.Abs(vector.Value.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
				_refEntity.ShootDirection = vector.Value;
			}
		}
	}

	private void UseSkill0(int skillId)
	{
		PlayVoiceSE("v_z_skill03");
		bInSkill = true;
		_refEntity.CurrentActiveSkill = skillId;
		_refEntity.SkillEnd = false;
		_refEntity.PlayerStopDashing();
		_refEntity.SetSpeed(0, 0);
		_refEntity.IsShoot = 1;
		ToggleWeapon(1);
		_refEntity.SoundSource.PlaySE(_refEntity.SkillSEID, "z_rakuou", 0.5f);
		if (_refEntity.UseAutoAim)
		{
			TurnToAimTarget();
		}
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
		if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
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
		PlayVoiceSE("v_z_skill02");
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
		}
		_vSkillStartPosition = _refEntity.AimPosition;
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
		_refEntity.SkillEnd = true;
		SkipSkill1Animation();
	}

	private void SkipSkill1Animation()
	{
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
			if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.SKILL && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.SKILL0_2)
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
			_cmWeapon.Disappear();
			_fxWeapon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			break;
		case -2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_cmWeapon.Disappear();
			_fxWeapon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			break;
		case -1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_cmWeapon.Disappear();
			_fxWeapon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			break;
		case 1:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_cmWeapon.Disappear();
			_fxWeapon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			break;
		case 2:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentWeaponObj(), 0.01f);
			}
			_cmWeapon.Appear();
			_fxWeapon.Play(true);
			break;
		default:
			if (_refEntity.CheckCurrentWeaponIndex())
			{
				_refEntity.EnableCurrentWeapon();
			}
			_cmWeapon.Disappear();
			_fxWeapon.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
