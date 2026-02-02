using System;
using UnityEngine;

public class CinnamonLoverController : CharacterControlBase
{
	private bool bInSkill;

	private CollideBullet tCB;

	private Vector3 CtrlShotDir;

	private Transform mp_choco_001;

	private int nLastSkillIndex0;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[7] { "ch028_skill_01_send_chocolate_to_target_jump", "ch028_skill_01_send_friend_chocolate_to_target_stand", "ch028_skill_01_send_lover_chocolate_to_target_stand", "ch028_skill_02_change_to_friend_chocolate_jump", "ch028_skill_02_change_to_friend_chocolate_stand", "ch028_skill_02_change_to_lover_chocolate_jump", "ch028_skill_02_change_to_lover_chocolate_stand" };
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "Bip", true);
		mp_choco_001 = OrangeBattleUtility.FindChildRecursive(ref target, "p_choco_001", true);
		GameObject gameObject = new GameObject();
		tCB = gameObject.AddComponent<CollideBullet>();
		tCB.gameObject.layer = base.gameObject.layer;
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_love_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_choco_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_choco_001", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
	}

	private Vector3 GetShotDirByWeaponDir()
	{
		Vector3 result = _refEntity.ShootDirection;
		if (_refEntity.UseAutoAim && _refEntity.CurrentActiveSkill < 2 && _refEntity.IAimTargetLogicUpdate != null && _refEntity.IAimTargetLogicUpdate.AimTransform != null)
		{
			Transform transform = _refEntity.ExtraTransforms[0];
			result = ((_refEntity.IAimTargetLogicUpdate.AimTransform.position + _refEntity.IAimTargetLogicUpdate.AimPoint).xy() - transform.position.xy()).normalized;
		}
		return result;
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
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (_refEntity.CurrentFrame > 0f && bInSkill)
			{
				bInSkill = false;
				_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkillIndex0], _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0].position, _refEntity.GetCurrentSkillObj().SkillLV, GetShotDirByWeaponDir());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0]);
				_refEntity.RemoveComboSkillBuff(_refEntity.GetCurrentSkillObj().FastBulletDatas[nLastSkillIndex0].n_ID);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_2:
		case OrangeCharacter.SubStatus.SKILL1_3:
			if (_refEntity.CurrentFrame > 0.1f && _refEntity.CurrentFrame < 0.52f && bInSkill)
			{
				if ((bool)mp_choco_001)
				{
					mp_choco_001.gameObject.SetActive(true);
				}
			}
			else if ((bool)mp_choco_001)
			{
				mp_choco_001.gameObject.SetActive(false);
			}
			if (_refEntity.CurrentFrame > 0.5f && bInSkill)
			{
				bInSkill = false;
				int nowRecordNO = _refEntity.GetNowRecordNO();
				tCB.transform.position = base.transform.position;
				tCB.UpdateBulletData(_refEntity.GetCurrentSkillObj().BulletData, _refEntity.sPlayerName, nowRecordNO, _refEntity.nBulletRecordID++, (int)_refEntity._characterDirection);
				tCB.SetBulletAtk(_refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				tCB.BulletLevel = _refEntity.GetCurrentSkillObj().SkillLV;
				tCB.Active(BulletScriptableObject.Instance.BulletLayerMaskPlayer);
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0]);
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			int currentActiveSkill = _refEntity.CurrentActiveSkill;
			if ((uint)currentActiveSkill <= 1u)
			{
				_refEntity.DisableWeaponMesh(_refEntity.GetCurrentSkillObj());
				_refEntity.EnableCurrentWeapon();
			}
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = true;
			_refEntity.CurrentActiveSkill = -1;
			if ((bool)mp_choco_001)
			{
				mp_choco_001.gameObject.SetActive(false);
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
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				break;
			case OrangeCharacter.SubStatus.SKILL1_3:
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
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
		default:
			return;
		case OrangeCharacter.SubStatus.SKILL1_2:
			if ((bool)mp_choco_001)
			{
				mp_choco_001.gameObject.SetActive(false);
			}
			goto case OrangeCharacter.SubStatus.SKILL0;
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			_refEntity.SkillEnd = true;
			_refEntity.IgnoreGravity = false;
			return;
		case OrangeCharacter.SubStatus.SKILL1_3:
			if ((bool)mp_choco_001)
			{
				mp_choco_001.gameObject.SetActive(false);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
		case OrangeCharacter.SubStatus.SKILL0_2:
		case OrangeCharacter.SubStatus.SKILL1_1:
			break;
		}
		_refEntity.Dashing = false;
		_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
		_refEntity.SkillEnd = true;
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id != 0 && id == 1 && _refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.SetSpeed(0, 0);
			_refEntity.StopShootTimer();
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			_refEntity.DisableCurrentWeapon();
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_love_000", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			if (_refEntity.Controller.Collisions.below)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_3);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_2);
			}
			_refEntity.PlaySE(_refEntity.SkillSEID, 5);
			_refEntity.PlaySE(_refEntity.VoiceID, 8);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp <= 0)
		{
			return;
		}
		if (id != 0)
		{
			int num2 = 1;
		}
		else
		{
			if (_refEntity.CurrentActiveSkill != -1 || !_refEntity.CheckUseSkillKeyTrigger(id))
			{
				return;
			}
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SkillEnd = false;
			bInSkill = true;
			_refEntity.SetSpeed(0, 0);
			CtrlShotDir = _refEntity.ShootDirection;
			nLastSkillIndex0 = _refEntity.GetCurrentSkillObj().Reload_index;
			if (nLastSkillIndex0 == 0)
			{
				ParticleSystem[] componentsInChildren = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_choco_000", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>()).transform.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					ParticleSystem.MainModule main = componentsInChildren[i].main;
					main.simulationSpeed = 5f;
				}
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				}
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			}
			else
			{
				ParticleSystem[] componentsInChildren = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_choco_001", _refEntity.ModelTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>()).transform.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					ParticleSystem.MainModule main2 = componentsInChildren[i].main;
					main2.simulationSpeed = 5f;
				}
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
				}
				else
				{
					_refEntity.IgnoreGravity = true;
					_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				}
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			}
			int num = Math.Sign(CtrlShotDir.x);
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(CtrlShotDir.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			}
			_refEntity.StopShootTimer();
			_refEntity.DisableCurrentWeapon();
			_refEntity.PlaySE(_refEntity.SkillSEID, 3);
			_refEntity.PlaySE(_refEntity.VoiceID, 9);
		}
	}
}
