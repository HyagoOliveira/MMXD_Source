using System;
using UnityEngine;

public class CH040_Controller : CharacterControlBase
{
	private bool bInSkill;

	private Transform tfLifebuoyMesh;

	private Renderer pLifebuoyMesh;

	private CollideBullet pWaterShieldBullet;

	private ParticleSystem fxWatershield;

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_rollswimsuit_in";
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch040_skill_01_stand", "ch040_skill_01_jump", "ch040_skill_01_crouch", "ch040_skill_02_stand", "ch040_skill_02_jump", "ch040_skill_02_crouch" };
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[3];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		_refEntity.ExtraTransforms[2] = OrangeBattleUtility.FindChildRecursive(ref target, "Skill0ShotPosition", true);
		tfLifebuoyMesh = OrangeBattleUtility.FindChildRecursive(ref target, "LifebuoyMesh_m", true);
		fxWatershield = OrangeBattleUtility.FindChildRecursive(ref target, "fxuse_watershield_000_", true).GetComponent<ParticleSystem>();
		fxWatershield.Stop();
		pLifebuoyMesh = tfLifebuoyMesh.GetComponent<Renderer>();
		pWaterShieldBullet = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<CollideBullet>(_refEntity.PlayerSkills[1].BulletData.s_MODEL);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_swimring_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_watershield_001", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
		_refEntity.StageTeleportInCharacterDependEvt = StageTeleportInCharacterDepend;
		_refEntity.StageTeleportOutCharacterDependEvt = StageTeleportOutCharacterDepend;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
	}

	public override void ClearSkill()
	{
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		if ((uint)currentActiveSkill <= 1u)
		{
			_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.GetCurrentSkillObj());
		}
	}

	public void TeleportInExtraEffect()
	{
		_refEntity.PlaySE("BattleSE", "bt_water01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override void CheckSkill()
	{
		int n_CONDITION_ID = _refEntity.PlayerSkills[1].FastBulletDatas[0].n_CONDITION_ID;
		if (pWaterShieldBullet.IsActivate)
		{
			if (_refEntity.IsDead())
			{
				DisableWaterShield();
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(n_CONDITION_ID);
			}
			else if (!_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID))
			{
				DisableWaterShield();
			}
		}
		else if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID) && !pWaterShieldBullet.IsActivate)
		{
			fxWatershield.Play();
			pWaterShieldBullet.UpdateBulletData(_refEntity.PlayerSkills[1].FastBulletDatas[0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
			pWaterShieldBullet.UseExtraCollider = true;
			pWaterShieldBullet.SetBulletAtk(_refEntity.PlayerSkills[1].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			pWaterShieldBullet.BulletLevel = _refEntity.PlayerSkills[1].SkillLV;
			pWaterShieldBullet.Active(_refEntity.transform, Quaternion.identity, _refEntity.TargetMask, true, (_refEntity.PlayerSkills[1].BulletData.n_TRACKING > 0) ? _refEntity.PlayerAutoAimSystem.AutoAimTarget : null);
		}
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (_refEntity.CurrentFrame > 0.22f && bInSkill)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity._transform);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (_refEntity.CurrentFrame > 0.25f && bInSkill)
			{
				bInSkill = false;
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity._transform);
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.StopShootTimer();
				_refEntity.DisableCurrentWeapon();
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_swimring_000", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				}
				_refEntity.PlaySE(_refEntity.VoiceID, "v_rl_skill03");
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.StopShootTimer();
				_refEntity.DisableCurrentWeapon();
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_watershield_001", _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.CROUCH)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				}
				_refEntity.PlaySE(_refEntity.VoiceID, "v_rl_skill04");
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id != 0)
		{
			int num = 1;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.WIN_POSE:
				pLifebuoyMesh.enabled = true;
				break;
			case OrangeCharacter.SubStatus.TELEPORT_POSE:
				pLifebuoyMesh.enabled = true;
				break;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetSpeed(0, 0);
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetSpeed(0, 0);
				_refEntity.Dashing = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetSpeed(0, 0);
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				}
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetSpeed(0, 0);
				_refEntity.Dashing = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			}
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.TELEPORT_IN && subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
		{
			pLifebuoyMesh.enabled = false;
			_refEntity.EnableCurrentWeapon();
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.SkillEnd = true;
			bInSkill = false;
			_refEntity.EnableCurrentWeapon();
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.SkillEnd = true;
			bInSkill = false;
			_refEntity.EnableCurrentWeapon();
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SkillEnd = true;
			bInSkill = false;
			_refEntity.EnableCurrentWeapon();
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.SkillEnd = true;
			bInSkill = false;
			_refEntity.EnableCurrentWeapon();
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct wsSkill)
	{
		_refEntity.FreshBullet = true;
		_refEntity.StartShootTimer();
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.IsShoot = 0;
			_refEntity.PushBulletDetail(wsSkill.FastBulletDatas[0], wsSkill.weaponStatus, _refEntity.ExtraTransforms[2], wsSkill.SkillLV, Vector3.right * _refEntity.direction);
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
			_refEntity.IsShoot = 1;
			if (!pWaterShieldBullet.IsActivate)
			{
				fxWatershield.Play();
				pWaterShieldBullet.UpdateBulletData(_refEntity.PlayerSkills[1].FastBulletDatas[0], _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.nBulletRecordID++);
				pWaterShieldBullet.UseExtraCollider = true;
				pWaterShieldBullet.SetBulletAtk(_refEntity.PlayerSkills[1].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				pWaterShieldBullet.BulletLevel = _refEntity.PlayerSkills[1].SkillLV;
				pWaterShieldBullet.Active(_refEntity.transform, Quaternion.identity, _refEntity.TargetMask, true, (_refEntity.PlayerSkills[1].BulletData.n_TRACKING > 0) ? _refEntity.PlayerAutoAimSystem.AutoAimTarget : null);
			}
			break;
		}
	}

	private bool checkCancelAnimate(int skilliD)
	{
		return false;
	}

	public override void ControlCharacterDead()
	{
		DisableWaterShield();
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		int n_CONDITION_ID = _refEntity.PlayerSkills[1].FastBulletDatas[0].n_CONDITION_ID;
		if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(n_CONDITION_ID))
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(n_CONDITION_ID);
		}
		if (pWaterShieldBullet.IsActivate)
		{
			DisableWaterShield();
		}
		if (fxWatershield.isPlaying)
		{
			fxWatershield.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		return _refEntity.EnterRideArmor(targetRideArmor);
	}

	public void StageTeleportInCharacterDepend()
	{
		if (pWaterShieldBullet.IsActivate)
		{
			pWaterShieldBullet.UpdateFx();
		}
	}

	public void StageTeleportOutCharacterDepend()
	{
		pLifebuoyMesh.enabled = false;
	}

	private void DisableWaterShield()
	{
		if (pWaterShieldBullet.IsActivate)
		{
			pWaterShieldBullet.Reset_Duration_Time();
			pWaterShieldBullet.BackToPool();
		}
	}
}
