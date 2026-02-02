using System;
using UnityEngine;

public class LayerController : CharacterControlBase
{
	private bool bInSkill;

	private bool bInShootBullet;

	private Transform mskillEft;

	private int SkilleStatusId;

	private int nLastSkillIndex0;

	private bool PlaySkill01LoopSE;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[12]
		{
			"ch017_skill_01_jump_first_start", "ch017_skill_01_jump_first_end", "ch017_skill_01_jump_second_start", "ch017_skill_01_jump_second_end", "ch017_skill_01_stand_first_start", "ch017_skill_01_stand_first_end", "ch017_skill_01_stand_second_start", "ch017_skill_01_stand_second_end", "ch017_skill_02_jump_start", "ch017_skill_02_jump_loop",
			"ch017_skill_02_stand_start", "ch017_skill_02_end"
		};
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_leafslash_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_leafslash_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_skylord_000", 2);
		mskillEft = OrangeBattleUtility.FindChildRecursive(_refEntity.PlayerSkills[0].WeaponMesh[0].transform, "skill1eft");
		SkilleStatusId = 0;
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
	}

	private bool checkCancelAnimate(int skilliD)
	{
		if (skilliD == 0)
		{
			if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL0))
			{
				return true;
			}
		}
		else if (ManagedSingleton<InputStorage>.Instance.IsAnyHeld(_refEntity.UserID) && !ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.SKILL1))
		{
			return true;
		}
		return false;
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || !bInSkill)
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
		case OrangeCharacter.SubStatus.SKILL0_4:
			if ((double)_refEntity.CurrentFrame > 0.35 && !bInShootBullet)
			{
				bInShootBullet = true;
				_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkillIndex0], _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ModelTransform, _refEntity.GetCurrentSkillObj().SkillLV);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
		case OrangeCharacter.SubStatus.SKILL0_6:
			if ((double)_refEntity.CurrentFrame > 0.18 && !bInShootBullet)
			{
				bInShootBullet = true;
				_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkillIndex0], _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ModelTransform, _refEntity.GetCurrentSkillObj().SkillLV);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
		case OrangeCharacter.SubStatus.SKILL0_7:
			if (checkCancelAnimate(0))
			{
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
					_refEntity.SkillEnd = true;
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
					_refEntity.SkillEnd = true;
					_refEntity.IgnoreGravity = false;
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (mskillEft != null && _refEntity.CurrentFrame > 0.3f && !mskillEft.GetComponent<ParticleSystem>().isPlaying)
			{
				mskillEft.GetComponent<ParticleSystem>().Play();
				if (!PlaySkill01LoopSE)
				{
					PlaySkillSE(4);
					PlaySkill01LoopSE = true;
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (!_refEntity.Controller.Collisions.below)
			{
				break;
			}
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
			if (mskillEft != null)
			{
				mskillEft.GetComponent<ParticleSystem>().Stop();
				if (PlaySkill01LoopSE)
				{
					PlaySkillSE(5);
					PlaySkill01LoopSE = false;
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (!bInShootBullet)
			{
				bInShootBullet = true;
				if (PlaySkill01LoopSE)
				{
					PlaySkillSE(5);
					PlaySkill01LoopSE = false;
				}
				_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().BulletData, _refEntity.GetCurrentSkillObj().weaponStatus, new Vector3(_refEntity.ExtraTransforms[0].position.x, _refEntity.ModelTransform.position.y, _refEntity.ModelTransform.position.z), _refEntity.GetCurrentSkillObj().SkillLV, Vector3.zero, false, 1);
			}
			if (checkCancelAnimate(1))
			{
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.Dashing = false;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
					_refEntity.SkillEnd = true;
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
					_refEntity.SkillEnd = true;
					_refEntity.IgnoreGravity = false;
				}
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.PlayerSkills[0]);
			break;
		case 1:
			_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.PlayerSkills[0]);
			break;
		}
		_refEntity.SkillEnd = true;
		bInSkill = false;
		_refEntity.CurrentActiveSkill = -1;
		SkilleStatusId = 0;
		if (mskillEft != null)
		{
			mskillEft.GetComponent<ParticleSystem>().Stop();
			if (PlaySkill01LoopSE)
			{
				PlaySkillSE(5);
				PlaySkill01LoopSE = false;
			}
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_4:
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_5:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_6:
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
				break;
			case OrangeCharacter.SubStatus.SKILL0_7:
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)74u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)75u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)76u);
				break;
			}
		}
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			_refEntity.SkillEnd = true;
			_refEntity.IgnoreGravity = false;
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_3);
			break;
		case OrangeCharacter.SubStatus.SKILL0_3:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			_refEntity.SkillEnd = true;
			_refEntity.IgnoreGravity = false;
			break;
		case OrangeCharacter.SubStatus.SKILL0_4:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_5);
			break;
		case OrangeCharacter.SubStatus.SKILL0_5:
			_refEntity.Dashing = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			_refEntity.SkillEnd = true;
			break;
		case OrangeCharacter.SubStatus.SKILL0_6:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_7);
			break;
		case OrangeCharacter.SubStatus.SKILL0_7:
			_refEntity.Dashing = false;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			_refEntity.SkillEnd = true;
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
			break;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				_refEntity.SkillEnd = true;
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
			}
			break;
		}
	}

	private bool Check_Skill_Status(int eff_id)
	{
		if (_refEntity.IsLocalPlayer)
		{
			if (_refEntity.selfBuffManager.CheckHasEffect(eff_id))
			{
				return true;
			}
			return false;
		}
		if (SkilleStatusId != eff_id)
		{
			SkilleStatusId = eff_id;
			return false;
		}
		SkilleStatusId = 0;
		return true;
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
		{
			if (_refEntity.CurrentActiveSkill != -1 || !_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.SetSpeed(0, 0);
			_refEntity.StopShootTimer();
			bInShootBullet = false;
			nLastSkillIndex0 = _refEntity.GetCurrentSkillObj().Reload_index;
			int num = Math.Sign(_refEntity.ShootDirection.x);
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(_refEntity.ShootDirection.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			}
			if (nLastSkillIndex0 == 0)
			{
				if (_refEntity.ShootDirection.x > 0f)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_leafslash_000", _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				}
				else
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_leafslash_000", _refEntity.ModelTransform.position, OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				}
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_4);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				}
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
			}
			else
			{
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_6);
					if (_refEntity.ShootDirection.x > 0f)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_leafslash_001", _refEntity.ModelTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
					}
					else
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_leafslash_001", _refEntity.ModelTransform.position, OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
					}
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
					if (_refEntity.ShootDirection.x > 0f)
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_leafslash_001", new Vector3(_refEntity.ModelTransform.position.x, _refEntity.ModelTransform.position.y + 0.25f, _refEntity.ModelTransform.position.z), OrangeCharacter.NormalQuaternion, Array.Empty<object>());
					}
					else
					{
						MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_leafslash_001", new Vector3(_refEntity.ModelTransform.position.x, _refEntity.ModelTransform.position.y + 0.25f, _refEntity.ModelTransform.position.z), OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
					}
				}
				_refEntity.RemoveComboSkillBuff(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkillIndex0].n_ID);
				if (_refEntity.GetCurrentSkillObj().BulletData.n_RELOAD > 0)
				{
					OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				}
			}
			_refEntity.UpdateWeaponMesh(_refEntity.PlayerSkills[id], _refEntity.GetCurrentWeaponObj());
			_refEntity.PlaySE(_refEntity.VoiceID, 26);
			break;
		}
		case 1:
			if (_refEntity.CurrentActiveSkill != -1 || !_refEntity.CheckUseSkillKeyTrigger(id))
			{
				break;
			}
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			bInShootBullet = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.StopShootTimer();
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			_refEntity.UpdateWeaponMesh(_refEntity.PlayerSkills[0], _refEntity.GetCurrentWeaponObj());
			if (_refEntity.Controller.Collisions.below)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_skylord_000", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				if (!PlaySkill01LoopSE)
				{
					PlaySkillSE(4);
					PlaySkill01LoopSE = true;
				}
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
			}
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
			_refEntity.PlaySE(_refEntity.VoiceID, 8);
			break;
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		SKILL_TABLE bulletDatum = weaponStruct.BulletData;
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.PushBulletDetail(weaponStruct.FastBulletDatas[0], weaponStruct.weaponStatus, _refEntity.ExtraTransforms[0], weaponStruct.SkillLV);
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp > 0 && id != 0)
		{
			int num = 1;
		}
	}
}
