using System;
using UnityEngine;

public class CH046_Controller : CharacterControlBase
{
	private readonly int SKILL0_END = (int)(0.7f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_END = (int)(0.9f / GameLogicUpdateManager.m_fFrameLen);

	private int nowFrame;

	private int skillProcessFrame;

	private Transform shootPointTransform;

	private SKILL_TABLE linkSkill;

	public override void Start()
	{
		base.Start();
		InitializeSkill();
		InitializeExtraMesh();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInCharacterDependEvt = TeleportInCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		shootPointTransform = OrangeBattleUtility.FindChildRecursive(ref target, "ShootPoint1", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_magicfield_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_magicfield_001", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_magicfield_002", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_magicfield_003", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_magicfield_004", 2);
	}

	private void InitializeExtraMesh()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, "WeaponMesh_c");
		Renderer[] extraMeshOpen;
		if (array != null)
		{
			OrangeCharacter refEntity = _refEntity;
			extraMeshOpen = new SkinnedMeshRenderer[array.Length];
			refEntity.ExtraMeshOpen = extraMeshOpen;
			for (int i = 0; i < array.Length; i++)
			{
				_refEntity.ExtraMeshOpen[i] = array[i].GetComponent<SkinnedMeshRenderer>();
			}
		}
		else
		{
			OrangeCharacter refEntity2 = _refEntity;
			extraMeshOpen = new SkinnedMeshRenderer[0];
			refEntity2.ExtraMeshOpen = extraMeshOpen;
		}
		OrangeCharacter refEntity3 = _refEntity;
		extraMeshOpen = new SkinnedMeshRenderer[0];
		refEntity3.ExtraMeshClose = extraMeshOpen;
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.ResetVelocity();
			_refEntity.PlayerStopDashing();
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_refEntity.ToggleExtraMesh(true);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.ResetVelocity();
			_refEntity.PlayerStopDashing();
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_refEntity.ToggleExtraMesh(true);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
		if (mainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (subStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.PlayerSkills[_refEntity.CurrentActiveSkill].weaponStatus, shootPointTransform);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill]);
			CreateSkillBullet(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill]);
			PlayVoiceSE("v_ch033_skill03");
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_magicfield_000", shootPointTransform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			}
			else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			}
			skillProcessFrame = nowFrame + SKILL0_END;
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.PlayerSkills[_refEntity.CurrentActiveSkill].weaponStatus, _refEntity.PlayerSkills[_refEntity.CurrentActiveSkill].ShootTransform[0]);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill]);
			_refEntity.selfBuffManager.AddBuff(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill].BulletData.n_CONDITION_ID, 0, 0, _refEntity.PlayerSkills[_refEntity.CurrentActiveSkill].BulletData.n_ID);
			PlayVoiceSE("v_ch033_skill04");
			PlaySkillSE("ch033_ois");
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(_refEntity.PlayerSkills[_refEntity.CurrentActiveSkill].BulletData.n_LINK_SKILL, out linkSkill))
			{
				_refEntity.tRefPassiveskill.ReCalcuSkill(ref linkSkill);
				_refEntity.selfBuffManager.AddBuff(linkSkill.n_CONDITION_ID, 0, 0, linkSkill.n_ID);
			}
			if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(1108))
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_magicfield_001", _refEntity._transform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			else if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(1109))
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_magicfield_003", _refEntity._transform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			else if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(1110))
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_magicfield_002", _refEntity._transform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			else
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxduring_magicfield_004", _refEntity._transform.position, OrangeCharacter.NormalQuaternion, Array.Empty<object>());
			}
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
			else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
			}
			skillProcessFrame = nowFrame + SKILL1_END;
			break;
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
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
			if (nowFrame < skillProcessFrame)
			{
				break;
			}
			ResetSkill();
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_SKILL_START)
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
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame < skillProcessFrame)
			{
				break;
			}
			ResetSkill();
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				if (_refEntity.AnimateID == (HumanBase.AnimateId)68u)
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
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			_refEntity.EnableCurrentWeapon();
			if (_refEntity.GetCurrentSkillObj().MagazineRemain > 0f)
			{
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
			}
			switch (_refEntity.CurrentActiveSkill)
			{
			case 0:
				_refEntity.BulletCollider.BackToPool();
				break;
			}
		}
		ResetSkill();
	}

	public void TeleportInExtraEffect()
	{
		PlaySkillSE("ch033_start01");
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override void ControlCharacterDead()
	{
	}

	private void ResetSkill()
	{
		_refEntity.ToggleExtraMesh(false);
		_refEntity.Dashing = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public void TeleportInCharacterDepend()
	{
		if (_refEntity.CurrentFrame >= 0.8f)
		{
			_refEntity.ToggleExtraMesh(false);
		}
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		_refEntity.FreshBullet = true;
		_refEntity.IsShoot = 1;
		_refEntity.StartShootTimer();
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, shootPointTransform, weaponStruct.SkillLV);
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch046_skill_01_crouch", "ch046_skill_01_stand", "ch046_skill_01_jump", "ch046_skill_02_crouch", "ch046_skill_02_stand", "ch046_skill_02_jump" };
	}

	public override string GetTeleportInExtraEffect()
	{
		return "fxuse_ailehalloween_in";
	}
}
