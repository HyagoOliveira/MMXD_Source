#define RELEASE
using System;
using System.Collections.Generic;
using UnityEngine;

public class FerhamController : CharacterControlBase
{
	private bool bInSkill;

	private Transform Skill_effect;

	public Transform Skill_mesh_effect;

	public List<BulletBase> listBullets = new List<BulletBase>();

	private BulletBase mSkillBullet;

	private OrangeCharacter.MainStatus LastMainStatus;

	private OrangeCharacter.SubStatus LastSubStatus;

	private FxBase tSkill0Fx;

	protected bool bStartWinPose;

	private const int FerhamBuffID = -1;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[8] { "ch007_skill_01_stand_start", "ch007_skill_01_stand_loop", "ch007_skill_01_stand_end", "ch007_skill_01_jump_start", "ch007_skill_01_jump_loop", "ch007_skill_01_jump_end", "ch007_skill_02_stand_start", "ch007_skill_02_jump_start" };
	}

	public override void Start()
	{
		base.Start();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
		_refEntity.TeleportOutCharacterDependEvt = TeleportOutCharacterDepend;
		_refEntity.EnterRideArmorEvt = EnterRideArmor;
	}

	protected override void Setup()
	{
		Skill_effect = OrangeBattleUtility.FindChildRecursive(base.transform, "skill_effect");
		Skill_mesh_effect = OrangeBattleUtility.FindChildRecursive(base.transform, "mesheffect");
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_bloody_000", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_bloody_001", 2);
		Transform[] target = base.transform.GetComponentsInChildren<Transform>(true);
		OrangeCharacter refEntity = _refEntity;
		Renderer[] extraMeshClose = new SkinnedMeshRenderer[0];
		refEntity.ExtraMeshClose = extraMeshClose;
		Transform[] array = OrangeBattleUtility.FindAllChildRecursive(ref target, "WhipMesh");
		if (array != null)
		{
			OrangeCharacter refEntity2 = _refEntity;
			extraMeshClose = new SkinnedMeshRenderer[array.Length];
			refEntity2.ExtraMeshOpen = extraMeshClose;
			for (int i = 0; i < array.Length; i++)
			{
				_refEntity.ExtraMeshOpen[i] = array[i].GetComponent<SkinnedMeshRenderer>();
			}
		}
	}

	public override void ExtraVariableInit()
	{
		_refEntity.ToggleExtraMesh(false);
		bInSkill = false;
	}

	public override void CheckSkill()
	{
		if (mSkillBullet != null)
		{
			if (_refEntity.IsDead())
			{
				mSkillBullet.BackToPool();
				mSkillBullet = null;
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(-1);
			}
			else if (!_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
			{
				mSkillBullet.BackToPool();
				mSkillBullet = null;
			}
			else if (mSkillBullet.bIsEnd && _refEntity.IsLocalPlayer)
			{
				_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(-1);
				mSkillBullet = null;
			}
		}
		else if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1) && mSkillBullet == null)
		{
			mSkillBullet = _refEntity.CreateFSBulletEx(_refEntity.PlayerSkills[1], 0);
		}
		if (!bInSkill)
		{
			return;
		}
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.SKILL:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus != OrangeCharacter.SubStatus.SKILL0_1)
			{
				int num = 50;
			}
			else if (_refEntity.CurrentFrame > 0.15f && bInSkill)
			{
				bInSkill = false;
				Debug.Log("Trigger Skill!");
				int nowRecordNO = _refEntity.GetNowRecordNO();
				_refEntity.BulletCollider.UpdateBulletData(_refEntity.PlayerSkills[0].BulletData, _refEntity.sPlayerName, nowRecordNO, _refEntity.nBulletRecordID++, (int)_refEntity._characterDirection);
				_refEntity.BulletCollider.SetBulletAtk(_refEntity.PlayerSkills[0].weaponStatus, _refEntity.selfBuffManager.sBuffStatus);
				_refEntity.BulletCollider.Active(base.transform.position, _refEntity.ShootDirection, _refEntity.TargetMask);
				_refEntity.BulletCollider.BulletLevel = _refEntity.PlayerSkills[0].SkillLV;
				_refEntity.CheckUsePassiveSkill(0, _refEntity.PlayerSkills[0].weaponStatus, _refEntity.PlayerSkills[0].ShootTransform[0]);
			}
			break;
		}
		case OrangeCharacter.MainStatus.FALL:
		{
			OrangeCharacter.SubStatus curSubStatus = _refEntity.CurSubStatus;
			if (curSubStatus == OrangeCharacter.SubStatus.IDLE && _refEntity.Velocity.x != 0)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		}
		}
	}

	public override void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			int currentActiveSkill = _refEntity.CurrentActiveSkill;
			if ((uint)currentActiveSkill <= 1u)
			{
				_refEntity.ToggleExtraMesh(false);
				Skill_effect.gameObject.SetActive(false);
				_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.GetCurrentSkillObj());
			}
			_refEntity.Dashing = false;
			_refEntity.SkillEnd = true;
			_refEntity.CurrentActiveSkill = -1;
		}
	}

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (LastMainStatus)
		{
		case OrangeCharacter.MainStatus.IDLE:
		{
			OrangeCharacter.SubStatus lastSubStatus = LastSubStatus;
			if (lastSubStatus == OrangeCharacter.SubStatus.SKILL_IDLE)
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.ToggleExtraMesh(false);
				Skill_effect.gameObject.SetActive(false);
				bInSkill = false;
				_refEntity.EnableCurrentWeapon();
				if (tSkill0Fx != null && (bool)tSkill0Fx.gameObject)
				{
					tSkill0Fx.BackToPool();
				}
				tSkill0Fx = null;
			}
			break;
		}
		case OrangeCharacter.MainStatus.FALL:
		{
			OrangeCharacter.SubStatus lastSubStatus = LastSubStatus;
			if (lastSubStatus == OrangeCharacter.SubStatus.IDLE)
			{
				_refEntity.IgnoreGravity = false;
				_refEntity.ToggleExtraMesh(false);
				Skill_effect.gameObject.SetActive(false);
				bInSkill = false;
				_refEntity.EnableCurrentWeapon();
				if (tSkill0Fx != null && (bool)tSkill0Fx.gameObject)
				{
					tSkill0Fx.BackToPool();
				}
				tSkill0Fx = null;
			}
			break;
		}
		}
		switch (_refEntity.CurMainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (_refEntity.CurSubStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				_refEntity.ToggleExtraMesh(true);
				Skill_effect.gameObject.SetActive(true);
				_refEntity.SetAnimateId(HumanBase.AnimateId.ANI_TELEPORT_IN_POSE);
			}
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
			if (subStatus == OrangeCharacter.SubStatus.WIN_POSE)
			{
				bStartWinPose = false;
			}
			break;
		case OrangeCharacter.MainStatus.SKILL:
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetAnimateId((HumanBase.AnimateId)(65 + ((!_refEntity.PreBelow) ? 3 : 0)));
				_refEntity.DisableCurrentWeapon();
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)(66 + ((!_refEntity.PreBelow) ? 3 : 0)));
				break;
			case OrangeCharacter.SubStatus.SKILL0_2:
				_refEntity.SetAnimateId((HumanBase.AnimateId)(67 + ((!_refEntity.PreBelow) ? 3 : 0)));
				_refEntity.BulletCollider.BackToPool();
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.SetAnimateId((HumanBase.AnimateId)(71u + ((!_refEntity.PreBelow) ? 1u : 0u)));
				Skill_effect.gameObject.SetActive(true);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.IDLE:
			if (subStatus == OrangeCharacter.SubStatus.SKILL_IDLE)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)67u);
				_refEntity.BulletCollider.BackToPool();
				_refEntity.Dashing = false;
				_refEntity.SkillEnd = true;
				_refEntity.IgnoreGravity = false;
			}
			break;
		case OrangeCharacter.MainStatus.FALL:
			if (subStatus == OrangeCharacter.SubStatus.IDLE)
			{
				_refEntity.SetAnimateId((HumanBase.AnimateId)70u);
				_refEntity.BulletCollider.BackToPool();
				_refEntity.Dashing = false;
				_refEntity.SkillEnd = true;
			}
			break;
		}
		LastMainStatus = _refEntity.CurMainStatus;
		LastSubStatus = _refEntity.CurSubStatus;
	}

	public void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		switch (mainStatus)
		{
		case OrangeCharacter.MainStatus.TELEPORT_IN:
			if (subStatus == OrangeCharacter.SubStatus.TELEPORT_POSE)
			{
				_refEntity.ToggleExtraMesh(false);
				Skill_effect.gameObject.SetActive(false);
			}
			break;
		case OrangeCharacter.MainStatus.TELEPORT_OUT:
		{
			int num = 1;
			break;
		}
		case OrangeCharacter.MainStatus.SKILL:
			switch (subStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0_1);
				_refEntity.ToggleExtraMesh(true);
				Skill_effect.gameObject.SetActive(true);
				break;
			case OrangeCharacter.SubStatus.SKILL0_1:
				if (_refEntity.PreBelow)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.SKILL_IDLE);
					break;
				}
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.IDLE);
				bInSkill = true;
				break;
			case OrangeCharacter.SubStatus.SKILL1_1:
				_refEntity.IgnoreGravity = false;
				_refEntity.SkillEnd = true;
				_refEntity.IsJacking = (bInSkill = false);
				_refEntity.Dashing = false;
				_refEntity.ToggleExtraMesh(false);
				_refEntity.EnableCurrentWeapon();
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				Skill_effect.gameObject.SetActive(false);
				break;
			}
			break;
		case OrangeCharacter.MainStatus.IDLE:
			if (subStatus == OrangeCharacter.SubStatus.SKILL_IDLE)
			{
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				_refEntity.IgnoreGravity = false;
				_refEntity.ToggleExtraMesh(false);
				Skill_effect.gameObject.SetActive(false);
				bInSkill = false;
				_refEntity.EnableCurrentWeapon();
				tSkill0Fx = null;
			}
			break;
		case OrangeCharacter.MainStatus.FALL:
			if (subStatus == OrangeCharacter.SubStatus.IDLE)
			{
				if (_refEntity.Controller.Collisions.below)
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
				}
				else
				{
					_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				}
				_refEntity.IgnoreGravity = false;
				_refEntity.ToggleExtraMesh(false);
				Skill_effect.gameObject.SetActive(false);
				bInSkill = false;
				_refEntity.EnableCurrentWeapon();
				tSkill0Fx = null;
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
		if ((int)_refEntity.Hp <= 0)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.PlayerSkills[id].MagazineRemain -= _refEntity.PlayerSkills[id].BulletData.n_USE_COST;
				_refEntity.PlayerSkills[id].LastUseTimer.TimerStart();
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				_refEntity.StopShootTimer();
				_refEntity.IsShoot = 0;
				tSkill0Fx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(_refEntity.Controller.Collisions.below ? "fxuse_bloody_000" : "fxuse_bloody_001", _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				_refEntity.IgnoreGravity = !_refEntity.Controller.Collisions.below;
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.PlayerSkills[id].MagazineRemain -= _refEntity.PlayerSkills[id].BulletData.n_USE_COST;
				_refEntity.PlayerSkills[id].LastUseTimer.TimerStart();
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				_refEntity.StopShootTimer();
				_refEntity.IsShoot = 0;
				_refEntity.ToggleExtraMesh(true);
				_refEntity.DisableCurrentWeapon();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1_1);
				_refEntity.IgnoreGravity = !_refEntity.Controller.Collisions.below;
				if (_refEntity.IsLocalPlayer)
				{
					_refEntity.selfBuffManager.AddBuff(-1, 0, 0, 0);
				}
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				_refEntity.PlaySE(_refEntity.SkillSEID, 4);
			}
			break;
		}
	}

	public void TeleportOutCharacterDepend()
	{
		if (tSkill0Fx != null && (bool)tSkill0Fx.gameObject)
		{
			tSkill0Fx.BackToPool();
		}
		tSkill0Fx = null;
		if (_refEntity.CurMainStatus == OrangeCharacter.MainStatus.TELEPORT_OUT && _refEntity.CurSubStatus == OrangeCharacter.SubStatus.WIN_POSE)
		{
			if (!bStartWinPose)
			{
				bStartWinPose = true;
			}
			else if (!Skill_effect.gameObject.activeSelf)
			{
				_refEntity.ToggleExtraMesh(true);
				Skill_effect.gameObject.SetActive(true);
			}
		}
	}

	public bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		if (_refEntity.selfBuffManager.CheckHasEffectByCONDITIONID(-1))
		{
			_refEntity.selfBuffManager.RemoveBuffByCONDITIONID(-1);
		}
		if (mSkillBullet != null)
		{
			mSkillBullet.BackToPool();
			mSkillBullet = null;
		}
		return _refEntity.EnterRideArmor(targetRideArmor);
	}
}
