using System;
using UnityEngine;

public class CinnamonController : CharacterControlBase
{
	private bool bInSkill;

	private CollideBullet tCB;

	private Vector3 CtrlShotDir;

	private bool bIsCtrlShot;

	private Vector3 _shootPositionShift = new Vector3(0.5f, 0f, 0f);

	private FxBase tSkill0Fx;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[4] { "ch011_skill_01_stand", "ch011_skill_02_stand", "ch011_skill_01_jump", "ch011_skill_02_jump" };
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint", true);
		GameObject gameObject = new GameObject();
		tCB = gameObject.AddComponent<CollideBullet>();
		tCB.gameObject.layer = base.gameObject.layer;
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_syringe_000", 2);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
		_refEntity.SetStatusCharacterDependEvt = SetStatusCharacterDepend;
	}

	private Vector3? GetShotDir(Vector3 tShotPos)
	{
		if (bIsCtrlShot)
		{
			return CtrlShotDir;
		}
		if (_refEntity.IAimTargetLogicUpdate == null || _refEntity.IAimTargetLogicUpdate.AimTransform == null)
		{
			if (_refEntity.PlayerAutoAimSystem == null || _refEntity.PlayerAutoAimSystem.AutoAimTarget == null)
			{
				return null;
			}
			return (_refEntity.PlayerAutoAimSystem.GetTargetPoint() - tShotPos).normalized;
		}
		return (_refEntity.IAimTargetLogicUpdate.AimPosition - tShotPos).normalized;
	}

	public override void ClearSkill()
	{
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
			if (_refEntity.CurrentFrame > 0f && bInSkill)
			{
				bInSkill = false;
				_refEntity.PushBulletDetail(_refEntity.GetCurrentSkillObj().FastBulletDatas[0], _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0].position + _shootPositionShift * _refEntity.direction, _refEntity.GetCurrentSkillObj().SkillLV, GetShotDir(_refEntity.ExtraTransforms[0].position + _shootPositionShift * _refEntity.direction));
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.ExtraTransforms[0]);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
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

	public void SetStatusCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus == OrangeCharacter.MainStatus.SKILL)
		{
			switch (_refEntity.CurSubStatus)
			{
			case OrangeCharacter.SubStatus.SKILL0:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)(65 + ((!_refEntity.PreBelow) ? 2 : 0)));
				break;
			case OrangeCharacter.SubStatus.SKILL1:
				_refEntity.IgnoreGravity = true;
				_refEntity.SetAnimateId((HumanBase.AnimateId)(66 + ((!_refEntity.PreBelow) ? 2 : 0)));
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
			_refEntity.SkillEnd = true;
			_refEntity.IgnoreGravity = false;
			if (_refEntity.PreBelow)
			{
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
			_refEntity.SkillEnd = true;
			_refEntity.IgnoreGravity = false;
			if (_refEntity.PreBelow)
			{
				_refEntity.Dashing = false;
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			}
			break;
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (id != 0)
		{
			int num = 1;
		}
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
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				_refEntity.StopShootTimer();
				tSkill0Fx = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>("fxuse_syringe_000", _refEntity.AimTransform.position, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.ReversedQuaternion : OrangeCharacter.NormalQuaternion, Array.Empty<object>());
				ParticleSystem[] componentsInChildren = tSkill0Fx.transform.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					ParticleSystem.MainModule main = componentsInChildren[i].main;
					main.simulationSpeed = 5f;
				}
				_refEntity.PlayerSkills[0].MagazineRemain -= _refEntity.PlayerSkills[0].BulletData.n_USE_COST;
				bIsCtrlShot = false;
				if (!_refEntity.UseAutoAim)
				{
					bIsCtrlShot = true;
					CtrlShotDir = _refEntity.ShootDirection;
				}
				_refEntity.PlayerSkills[0].LastUseTimer.TimerStart();
				_refEntity.DisableCurrentWeapon();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL0);
				_refEntity.PlaySE(_refEntity.VoiceID, 9);
				_refEntity.PlaySE(_refEntity.SkillSEID, 3);
				Vector3? shotDir = GetShotDir(_refEntity.ExtraTransforms[0].position);
				if (shotDir.HasValue)
				{
					_refEntity._characterDirection = ((Math.Sign(shotDir.Value.x) == 1) ? CharacterDirection.RIGHT : CharacterDirection.LEFT);
				}
				_refEntity.UpdateDirection();
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill == -1 && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.CurrentActiveSkill = id;
				_refEntity.SkillEnd = false;
				bInSkill = true;
				_refEntity.SetSpeed(0, 0);
				_refEntity.StopShootTimer();
				_refEntity.PlayerSkills[1].MagazineRemain -= _refEntity.PlayerSkills[1].BulletData.n_USE_COST;
				_refEntity.PlayerSkills[1].LastUseTimer.TimerStart();
				_refEntity.DisableCurrentWeapon();
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SKILL1);
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
			}
			break;
		}
	}

	public void PlayMeasureSE(int add)
	{
		if (add < 0)
		{
			_refEntity.PlaySE(_refEntity.SkillSEID, 2);
		}
		else if (add > 0)
		{
			_refEntity.PlaySE(_refEntity.SkillSEID, 4);
		}
	}
}
