using System;
using UnityEngine;

public class CH036_Controller : CharacterControlBase
{
	private readonly int SKILL0_START = (int)(0.15f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_LOOP = (int)(1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL0_END = (int)(0.1f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_START = (int)(0.2f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKILL1_END = (int)(0.25f / GameLogicUpdateManager.m_fFrameLen);

	private int nowFrame;

	private int skillProcessFrame;

	private FxBase fxLaser;

	private Transform shootPoint;

	private Transform throwPoint;

	private Vector3 shootDirection;

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportInExtraEffectEvt = TeleportInExtraEffect;
	}

	private void InitializeSkill()
	{
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		shootPoint = OrangeBattleUtility.FindChildRecursive(ref target, "Bip Neck", true);
		throwPoint = OrangeBattleUtility.FindChildRecursive(ref target, "ThrowPoint", true);
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<BasicBullet>("prefab/bullet/p_crusher_000", "p_crusher_000", 3, null);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_shininglaser_000", 2);
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 0 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill == -1 && id == 1 && _refEntity.CheckUseSkillKeyTrigger(id))
		{
			_refEntity.SetHorizontalSpeed(0);
			_refEntity.PlayerStopDashing();
			_refEntity.SkillEnd = false;
			_refEntity.DisableCurrentWeapon();
			_refEntity.CurrentActiveSkill = id;
			_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
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
			_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
			MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_shininglaser_000", _refEntity.ModelTransform, Quaternion.Euler(new Vector3(0f, -90f, 0f)), Array.Empty<object>());
			PlayVoiceSE("v_ch036_skill01");
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_SKILL_START);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)68u);
			}
			skillProcessFrame = nowFrame + SKILL0_START;
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
		{
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[0]);
			float num = ((_refEntity._characterDirection == CharacterDirection.LEFT) ? (-1f) : 1f);
			Vector3 shootPosition = new Vector3(shootPoint.position.x + num, shootPoint.position.y, shootPoint.position.z);
			_refEntity.PushBulletDetail(_refEntity.PlayerSkills[0].BulletData, _refEntity.PlayerSkills[0].weaponStatus, shootPosition, _refEntity.PlayerSkills[0].SkillLV, Vector3.right * _refEntity.direction);
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)66u);
			}
			else
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)69u);
			}
			skillProcessFrame = nowFrame + SKILL0_LOOP;
			break;
		}
		case OrangeCharacter.SubStatus.SKILL0_2:
			_refEntity.BulletCollider.BackToPool();
			_refEntity.IgnoreGravity = false;
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
			}
			else
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
			}
			skillProcessFrame = nowFrame + SKILL0_END;
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			shootDirection = _refEntity.ShootDirection;
			if (_refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH || _refEntity.AnimateID == HumanBase.AnimateId.ANI_CROUCH_END)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)71u);
			}
			else if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)72u);
			}
			else
			{
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)73u);
			}
			PlayVoiceSE("v_ch036_skill02");
			skillProcessFrame = nowFrame + SKILL1_START;
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
		{
			_refEntity.CheckUsePassiveSkill(1, _refEntity.PlayerSkills[1].weaponStatus, _refEntity.PlayerSkills[1].ShootTransform[0]);
			BasicBullet poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<BasicBullet>("p_crusher_000");
			poolObj.UpdateBulletData(_refEntity.PlayerSkills[1].BulletData, _refEntity.sPlayerName, _refEntity.GetNowRecordNO(), _refEntity.direction);
			poolObj.SetBulletAtk(_refEntity.PlayerSkills[1].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
			poolObj.BulletLevel = _refEntity.PlayerSkills[1].SkillLV;
			poolObj.Active(throwPoint.position, _refEntity.ShootDirection, _refEntity.TargetMask);
			OrangeBattleUtility.UpdateSkillCD(_refEntity.PlayerSkills[1]);
			skillProcessFrame = nowFrame + SKILL1_END;
			break;
		}
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
			if (nowFrame >= skillProcessFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= skillProcessFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_2);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL0_2:
			if (nowFrame >= skillProcessFrame)
			{
				ResetSkill();
				if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			if (nowFrame >= skillProcessFrame)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame < skillProcessFrame)
			{
				break;
			}
			ResetSkill();
			if (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below)
			{
				if (_refEntity.AnimateID == (HumanBase.AnimateId)71u)
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
		if (_refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		_refEntity.EnableCurrentWeapon();
		if (_refEntity.GetCurrentSkillObj().MagazineRemain > 0f)
		{
			OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
		}
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			_refEntity.BulletCollider.BackToPool();
			if (fxLaser != null)
			{
				fxLaser.BackToPool();
			}
			break;
		}
		ResetSkill();
	}

	private void ResetSkill()
	{
		_refEntity.Dashing = false;
		_refEntity.SetSpeed(0, 0);
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
	}

	public void TeleportInExtraEffect()
	{
		MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(GetTeleportInExtraEffect(), _refEntity.ModelTransform.position, Quaternion.identity, Array.Empty<object>());
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[9] { "ch036_skill_01_stand_start", "ch036_skill_01_stand_loop", "ch036_skill_01_stand_end", "ch036_skill_01_jump_start", "ch036_skill_01_jump_loop", "ch036_skill_01_jump_end", "ch036_skill_02_crouch", "ch036_skill_02_stand", "ch036_skill_02_jump" };
	}
}
