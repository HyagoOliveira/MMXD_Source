using System;
using UnityEngine;

public class AwakenZeroController : CharacterControlBase
{
	private bool bInSkill;

	private bool bInEffect;

	private ParticleSystem fxSkill02;

	private ParticleSystem fxResident;

	private Animation hairAnim;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[8] { "ch030_skill_01_stand_start", "ch030_skill_01_stand_end", "ch030_skill_01_jump_start", "ch030_skill_01_jump_end", "ch030_skill_02_stand_start", "ch030_skill_02_stand_end", "ch030_skill_02_jump_start", "ch030_skill_02_jump_end" };
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[2];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		_refEntity.ExtraTransforms[1] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		fxSkill02 = OrangeBattleUtility.FindChildRecursive(ref target, "ef_R_WeaponPoint", true).GetComponent<ParticleSystem>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_AwakeZero_skill1_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_AwakeZero_skill_start_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_AwakeZero_skill_start_001", 2);
		fxSkill02.Stop();
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "CH030_000_FX", true);
		fxResident = transform.GetComponent<ParticleSystem>();
		if (null != fxResident)
		{
			fxResident.Stop(true);
		}
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "SpringColliderS", true);
		hairAnim = transform2.GetComponent<Animation>();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
	}

	protected void OnEnable()
	{
		if ((bool)fxSkill02 && fxSkill02.isPlaying)
		{
			fxSkill02.Stop();
		}
	}

	public override void ClearSkill()
	{
		int currentActiveSkill = _refEntity.CurrentActiveSkill;
		if (currentActiveSkill == 1 && fxSkill02.IsAlive())
		{
			fxSkill02.Stop();
		}
	}

	public override void CheckSkill()
	{
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
			if (_refEntity.CurrentFrame > 0.7f && bInSkill)
			{
				bInSkill = false;
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0]);
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(_refEntity.DashSmokeFx, _refEntity._transform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
			}
			else if (_refEntity.CurrentFrame > 0.3f && !bInEffect)
			{
				bInEffect = true;
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_AwakeZero_skill_start_001", _refEntity.ExtraTransforms[1].position, Quaternion.identity, Array.Empty<object>());
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (checkCancelAnimate(0))
			{
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
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (_refEntity.CurrentFrame > 0.25f && bInSkill)
			{
				bInSkill = false;
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (checkCancelAnimate(0))
			{
				_refEntity.SkillEnd = true;
				bInSkill = false;
				_refEntity.EnableCurrentWeapon();
				fxSkill02.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id == 1 && _refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.StopShootTimer();
			_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentSkillObj(), _refEntity.GetCurrentWeaponObj());
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_AwakeZero_skill_start_000", _refEntity._transform, Quaternion.identity, Array.Empty<object>());
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (id == 0 && _refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.StopShootTimer();
			_refEntity.DisableCurrentWeapon();
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			_refEntity.PlaySE(_refEntity.VoiceID, "v_za_skill01");
			_refEntity.PlaySE(_refEntity.SkillSEID, "za_shingetsu01");
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_AwakeZero_skill_start_000", _refEntity._transform, Quaternion.identity, Array.Empty<object>());
			bInEffect = false;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
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
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			}
			else
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SetSpeed(0, 0);
			_refEntity.PlaySE(_refEntity.VoiceID, "v_za_skill02");
			_refEntity.PlaySE(_refEntity.SkillSEID, "za_genmu01");
			fxSkill02.Play();
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_AwakeZero_skill1_000", _refEntity._transform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[1]);
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
			}
			else
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
			}
			break;
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
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
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SkillEnd = true;
				bInSkill = false;
				fxSkill02.Stop();
				_refEntity.EnableCurrentWeapon();
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
					_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.GetCurrentSkillObj(), 0.2f);
				}
				else
				{
					_refEntity.IgnoreGravity = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				break;
			}
		}
		else if (fxSkill02.IsAlive())
		{
			fxSkill02.Stop();
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.StartShootTimer();
		SKILL_TABLE bulletDatum = weaponStruct.BulletData;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.IsShoot = 1;
				_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[1], weaponStruct.SkillLV);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.IsShoot = 0;
				_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV, Vector3.right * _refEntity.direction);
				break;
			}
		}
	}

	private bool checkCancelAnimate(int skilliD)
	{
		if ((skilliD == 0 || skilliD == 1) && ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID))
		{
			return true;
		}
		return false;
	}

	public void TeleportInCharacterDepend()
	{
		if (fxResident != null && !fxResident.isPlaying)
		{
			fxResident.Play(true);
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_AwakeZero_skill_start_000", _refEntity._transform.position, Quaternion.identity, Array.Empty<object>());
		}
	}

	public void TeleportOutCharacterDepend()
	{
		if (fxResident != null && fxResident.isPlaying)
		{
			fxResident.Stop(true);
		}
		if (hairAnim != null)
		{
			hairAnim.Stop();
		}
	}
}
